using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.BatchProcessor
{
	public class LicenceUsageProcessor : IEmailAttachmentProcessor
	{
		public LicenceUsageProcessor(ILogger serviceLogger)
			: base()
		{
			ServiceLogger = serviceLogger;
		}

		public ILogger ServiceLogger { get; private set; }

		public static string DecodeCompressedEncrypted(string inputText)
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return ExtractZipped("EnterpriseReport.xml", encoder.Decrypt(Convert.FromBase64String(inputText)));
		}

		static string ExtractZipped(string fileName, byte[] data)
		{
			string result = string.Empty;
			using (Stream inStream = new MemoryStream(data))
			using (MemoryStream outStream = new MemoryStream())
			{
				ZipExtractor extractor = new ZipExtractor();
				extractor.ExtractZipStream(inStream, outStream, fileName);
				result = Encoding.UTF8.GetString(outStream.ToArray());
			}
			return result;
		}

		protected BusinessObjectFactory Factory;

		public void Process(string xmlData)
		{
			int max = 2;
			for (int retries = 0; retries <= max; ++retries)
			{
				try
				{
					ProcessInAnotherFactory(xmlData);
					break;
				}
				catch (ZSaveConcurrencyException)
				{
					if (retries == max)
					{
						throw;
					}
					System.Threading.Thread.Sleep(100);
				}
				catch (ZSaveException)
				{
					if (retries == max)
					{
						throw;
					}
					System.Threading.Thread.Sleep(500);
				}
				catch (CargoWise.Data.Utils.SqlLockLostException)
				{
					if (retries == max)
					{
						throw;
					}
					System.Threading.Thread.Sleep(30000);
				}

				Factory = null;
				databasePkForStaffList = ZGuid.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessInAnotherFactory(string xmlData)
		{
			var serializer = ZXmlSerializer.New(typeof(LicenceConsumptionLogSchema));
			StringReader reader = new StringReader(xmlData);
			LicenceConsumptionLogSchema usageData = (LicenceConsumptionLogSchema)serializer.Deserialize(reader);
			Factory = new BusinessObjectFactory() { RefreshEnabled = false };

			StringBuilder errorMessageBuilder = new StringBuilder();
			string licenceCode = null;
			usageCount = 0;
			systemUsageCount = 0;

			List<ClientLicenceUsage> usages = new List<ClientLicenceUsage>();
			var contactFactory = CreateContactFactory();
			Dictionary<string, string> staffNameEmailMap = new Dictionary<string, string>(1000, StringComparer.OrdinalIgnoreCase);

			foreach (var usageByCompany in usageData.Items.Cast<LicenceConsumptionLogSchemaLicenceConsumptionLogs>()
				.GroupBy(s => s.CompanyLicenceCode)
				.OrderBy(s => s.Key.Left(3) + s.Key.Right(3)))
			{
				foreach (var usageToSpecifyUtc in usageByCompany)
				{
					usageToSpecifyUtc.UsageTime = DateTime.SpecifyKind(usageToSpecifyUtc.UsageTime.ToDateTime(), DateTimeKind.Utc);
				}

				staffNameEmailMap.Clear();

				LicenceHeader licenceHeader = LicenceHeader.LoadFromLicenceCode(Factory, usageByCompany.Key);
				ZGuid licenceCompanyOrgPk = licenceHeader != null ? licenceHeader.Company.LC_OH : ZGuid.Empty;
				ZString licenceCompanyCode = usageByCompany.Key.SubstringSafe(3, 3);
				LicenceDatabase licenceDatabase = FindDatabase(usageByCompany.Key, licenceHeader);

				if (licenceDatabase != null)
				{
					var clientCompany = FindOrCreateClientCompany(licenceCompanyCode, licenceDatabase.PK, licenceCompanyOrgPk);

					foreach (var companyPeriodUsage in usageByCompany.GroupBy(x => ToBillingPeriod(x.UsageTime.ToDateTime())))
					{
						int period = companyPeriodUsage.Key;

						foreach (var companyPeriodModeUsage in companyPeriodUsage.GroupBy(x => ActualLicenceType(x)))
						{
							var licenceMode = companyPeriodModeUsage.Key;
							var existingUsagePeriodModeCompany = LoadUsageForPeriodModeCompany(Factory, period, licenceMode, clientCompany);
							var moduleToStaffToExistingUsage = existingUsagePeriodModeCompany.GroupBy(x => (string)x.LX2_ModuleCode).ToDictionary(x => x.Key, y => y.ToDictionary(z => z.LX2_LS.ToGuid()));

							foreach (var companyPeriodModeStaffUsage in companyPeriodModeUsage.GroupBy(x => FindOrCreateDatabaseStaff(licenceDatabase.PK, x)))
							{
								var staff = companyPeriodModeStaffUsage.Key;
								if (staff != null && !IsNonBillableStaff(staff))
								{
									if (staff.LS_Code != User.WebUserCode)
									{
										staffNameEmailMap[StaffNameToContactName(staff.LS_FullName)] = staff.LS_Email;
									}

									// Create ClientLicenceUsage records - one per log - CPT only
									foreach (var log in companyPeriodModeStaffUsage)
									{
										++usageCount;

										// we no longer report the error "Corrupt Usage Data" error. This part of the check below can be removed after clients are upgraded.
										// we do it here just to stop them flooding ediProd
										if (!log.ErrorStatus.IsEmpty && !log.ErrorStatus.StartsWith("Corrupt Usage Data", StringComparison.Ordinal))
										{
											licenceCode = log.CompanyLicenceCode;
											errorMessageBuilder.AppendLine(string.Concat(licenceCode, " - ", log.ErrorStatus));
										}

										if (log.LicenceType == LicenceTypes.Codes.CPT)
										{
											var usage = CreateUsagePerLog(Factory, clientCompany, staff, log, licenceMode);
											usages.Add(usage);
										}
									}

									// Create EdiLicenceUsage records - one per month (per staff, module, licence mode)
									foreach (var moduleUsage in companyPeriodModeStaffUsage.GroupBy(x => (string)x.LicenceModuleCode))
									{
										if (!IsNonBillableModule(licenceDatabase, moduleUsage.Key))
										{
											int moduleUsageCount = moduleUsage.Count();
											var firstUsageUtc = moduleUsage.Min(x => x.UsageTime);
											var lastUsageUtc = moduleUsage.Max(x => x.UsageTime);

											EdiLicenceUsage existingUsage = null;
											if (moduleToStaffToExistingUsage.TryGetValue(moduleUsage.Key, out var staffPkToUsage))
											{
												if (!staffPkToUsage.TryGetValue(staff.PK.ToGuid(), out existingUsage))
												{
													existingUsage = null;
												}
											}
											if (existingUsage == null)
											{
												var newUsage = Factory.New<EdiLicenceUsage>();
												newUsage.LX2_Period = period;
												newUsage.LX2_FirstUsageUtc = firstUsageUtc;
												newUsage.LX2_LastUsageUtc = lastUsageUtc;
												newUsage.LX2_LCC = clientCompany.PK;
												newUsage.LX2_LS = staff.PK;
												newUsage.LX2_ModuleCode = moduleUsage.Key;
												newUsage.LX2_LicenceMode = licenceMode;
												newUsage.LX2_UsageCount = moduleUsageCount;
											}
											else
											{
												existingUsage.LX2_UsageCount += moduleUsageCount;
												if (existingUsage.LX2_FirstUsageUtc > firstUsageUtc)
												{
													existingUsage.LX2_FirstUsageUtc = firstUsageUtc;
												}
												if (existingUsage.LX2_LastUsageUtc < lastUsageUtc)
												{
													existingUsage.LX2_LastUsageUtc = lastUsageUtc;
												}
											}
										}
									}
								}
							}
						}
					}

					if (!licenceCompanyOrgPk.IsEmpty)
					{
						CreateContactsByEmailOrName(contactFactory, licenceCompanyOrgPk, staffNameEmailMap);
					}
				}
			}

			if (errorMessageBuilder.Length > 0)
			{
				ReportErrors(licenceCode, errorMessageBuilder.ToString());
			}

			ProcessSave();

			NotifyRequester(usageData, xmlData, Factory);

			if (usageData.Items.Count > 0)
			{
				ZString code = usageData.Items[0].CompanyLicenceCode;
				string enterpriseCode = code.Left(3);
				string serverCode = code.Right(3);

				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Licence:{0}/-/{1} Usage(System):{2}({3})",
						enterpriseCode,
						serverCode,
						usageCount,
						systemUsageCount));
				}

				ReportProcessorHelper.SaveReport(ServiceLogger, "LicenceUsage", enterpriseCode, serverCode, xmlData);
			}

			Factory = null;

			try
			{
				contactFactory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Error, "Saving contact factory", ex);
				}
			}
		}

		public static bool IsNonBillableModule(LicenceDatabase licenceDatabase, string moduleCode)
		{
			if (moduleCode == "RDC" && licenceDatabase.IsHostedOnWiseCloud)
			{
				return true;
			}

			if (moduleCode != BillingConstants.CoreModuleCode && licenceDatabase.LD_LicenceType != DatabaseTypes.Codes.Production)
			{
				return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This contains legacy product names, not the current product name.")]
		public static bool IsNonBillableStaffNameAndEmail(string name, string email)
		{
			if (string.IsNullOrEmpty(email))
			{
				if (name.Equals("CargoWise One Support", StringComparison.OrdinalIgnoreCase) || // Legacy names are allowed
					name.Equals("CargoWise Support", StringComparison.OrdinalIgnoreCase) || // This may have been a legacy name and is also the current name
					name.Equals("EDI Support", StringComparison.OrdinalIgnoreCase)) // Legacy names are allowed
				{
					return true;
				}
			}
			else if (email.EndsWith("@cargowise.com", StringComparison.OrdinalIgnoreCase) ||
				email.EndsWith("@wisetechglobal.com", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		public static bool IsNonBillableStaffCode(string code)
		{
			return code.Equals("E", StringComparison.OrdinalIgnoreCase);
		}

		static bool IsNonBillableStaff(ClientStaff staff)
		{
			return IsNonBillableStaffNameAndEmail(staff.LS_FullName, staff.LS_Email)
				|| IsNonBillableStaffCode(staff.LS_Code);
		}

		static EdiLicenceUsage[] LoadUsageForPeriodModeCompany(
			BusinessObjectFactory factory,
			int period,
			string licenceMode,
			ClientCompany clientCompany)
		{
			var query = new ZDBOnlyQuery(typeof(EdiLicenceUsage));
			query.AddToFilter(EdiLicenceUsageSchema.LX2_Period, period);
			query.AddToFilter(EdiLicenceUsageSchema.LX2_LicenceMode, licenceMode);
			query.AddToFilter(EdiLicenceUsageSchema.LX2_LCC, clientCompany.PK);
			return factory.Load<EdiLicenceUsage>(query);
		}

		static ClientLicenceUsage CreateUsagePerLog(
			BusinessObjectFactory factory,
			ClientCompany clientCompany,
			ClientStaff staff,
			LicenceConsumptionLogSchemaLicenceConsumptionLogs log,
			string finalLicenceType)
		{
			var usage = factory.New<ClientLicenceUsage>();

			usage.LX_ModuleCode = log.LicenceModuleCode;
			usage.LX_UsageTime = log.UsageTime;
			usage.LX_LS = staff.PK;
			usage.LX_LicenceMode = finalLicenceType;
			usage.LX_Branch = log.BranchCode;
			usage.LX_LCC = clientCompany.PK;
			return usage;
		}

		int ToBillingPeriod(DateTime usageTimeUtc)
		{
			return TimeConverter.UtcToPeriod(usageTimeUtc);
		}

		BillingPeriodConverter TimeConverter => billingTimeConverter ?? (billingTimeConverter = new BillingPeriodConverter());
		BillingPeriodConverter billingTimeConverter;

		static string ActualLicenceType(LicenceConsumptionLogSchemaLicenceConsumptionLogs log)
		{
			return log.UsageTime >= new ZDateTime(2013, 7, 1) && (log.LicenceType == LicenceTypes.Codes.PUR || log.LicenceType == LicenceTypes.Codes.REN)
				? LicenceTypes.Codes.ODM
				: (string)log.LicenceType;
		}

		static string StaffNameToContactName(string name)
		{
			return name.Length > OrgContact.Schema.OC_ContactNameMaxLength
				? name.Substring(0, OrgContact.Schema.OC_ContactNameMaxLength)
				: name;
		}

		protected virtual void ProcessSave()
		{
			Factory.Save();
		}

		protected virtual BusinessObjectFactory CreateContactFactory()
		{
			return new BusinessObjectFactory() { RefreshEnabled = false };
		}

		static bool IsSystemStaffCode(string staffCode)
		{
			return staffCode == "C" // "Developer"
				|| staffCode == "X" // "Controller"
				|| staffCode == User.ServiceUserCode
				|| staffCode == "E"; // "EDI Support", "CargoWise Support"
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Not a product name.")]
		static bool IsSystemStaffName(string staffName)
		{
			return staffName == "Developer"
				|| staffName == "Controller"
				|| staffName == "EnterpriseBatchProcessor"
				|| staffName == "EDI Support"
				|| staffName == "CargoWise One Support"
				|| staffName == "CargoWise Support";
		}

		static bool IsLicenceTypeConsideredForUsage(string licenceType)
		{
			return licenceType == "CPT";
		}

		static bool IsWebModule(string moduleCode)
		{
			return moduleCode == LegacyLicence.Codes.WebTracker
				|| moduleCode == LegacyLicence.Codes.WebTrackerBooking
				|| moduleCode == LegacyLicence.Codes.WebTrackerCFS
				|| moduleCode == LegacyLicence.Codes.WebTrackerExportBrokerage
				|| moduleCode == LegacyLicence.Codes.WebTrackerForwarding
				|| moduleCode == LegacyLicence.Codes.WebTrackerImportBrokerage
				|| moduleCode == LegacyLicence.Codes.WebTrackerLocalTransport
				|| moduleCode == LegacyLicence.Codes.WebTrackerOrderManager
				|| moduleCode == LegacyLicence.Codes.WebTrackerShippingManagerBillsOfLading
				|| moduleCode == LegacyLicence.Codes.WebTrackerShippingManagerBookings
				|| moduleCode == LegacyLicence.Codes.WebTrackerWarehouse;
		}

		ClientStaff[] databaseStaffList;
		ZGuid databasePkForStaffList;
		Dictionary<string, ClientStaff> databaseStaffCodeMap;
		Dictionary<string, ClientStaff> databaseStaffNameMap;
		int systemUsageCount;
		int usageCount;

		LicenceDatabase FindDatabase(ZString licenceCode, LicenceHeader licenceHeader)
		{
			LicenceDatabase licenceDatabase = null;
			if (licenceHeader != null)
			{
				licenceDatabase = licenceHeader.Database;
			}
			else
			{
				var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
				query.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, licenceCode.Right(3));
				var enterpriseSubQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceDatabaseSchema.LD_LE);
				enterpriseSubQuery.AddToFilter(LicenceEnterpriseSchema.LE_EnterpriseCode, licenceCode.Left(3));
				query.AddSubQuery(enterpriseSubQuery, JoinCondition.And);

				licenceDatabase = Factory.LoadTop1<LicenceDatabase>(query);
			}
			return licenceDatabase;
		}

		ClientCompany FindOrCreateClientCompany(ZString companyCode, ZGuid databasePk, ZGuid orgPk)
		{
			return ClientCompany.FindOrCreate(Factory, companyCode, databasePk, orgPk, null, null);
		}

		ClientStaff FindOrCreateDatabaseStaff(ZGuid databasePK, LicenceConsumptionLogSchemaLicenceConsumptionLogs log)
		{
			ZString staffCodeUpperCase = log.StaffCode.ToUpper();
			bool isWebUser = false;

			if (!staffCodeUpperCase.IsEmpty)
			{
				// new style report includes staff code
				if (IsSystemStaffCode(staffCodeUpperCase))
				{
					++systemUsageCount;
					if (!IsLicenceTypeConsideredForUsage(log.LicenceType))
					{
						return null;
					}
				}

				isWebUser = staffCodeUpperCase == User.WebUserCode;
			}
			else if (IsWebModule(log.LicenceModuleCode))
			{
				staffCodeUpperCase = User.WebUserCode;
				isWebUser = true;
			}
			else if (!log.StaffName.IsEmpty)
			{
				// old style report has no staff code
				if (IsSystemStaffName(log.StaffName))
				{
					++systemUsageCount;
					if (!IsLicenceTypeConsideredForUsage(log.LicenceType))
					{
						return null;
					}
				}
			}
			else
			{
				// ignore if code and name is blank
				return null;
			}

			EnsureLoadedAllDatabaseStaff(databasePK);

			ClientStaff staff = null;
			if (!staffCodeUpperCase.IsEmpty)
			{
				databaseStaffCodeMap.TryGetValue(staffCodeUpperCase, out staff);
			}

			if (staff == null && !isWebUser && !log.StaffName.IsEmpty)
			{
				ClientStaff namedStaff;
				// Before 2014-3-1 the algorithm had a flaw (see WI00054565).
				// We need to use the old algorithm for usage before that date
				// since mixing the algorithms in the same month can cause double billing.
				if (databaseStaffNameMap.TryGetValue(log.StaffName, out namedStaff)
					&& (log.UsageTime < new ZDateTime(2014, 3, 1)
						|| namedStaff.LS_Code.IsEmpty || staffCodeUpperCase.IsEmpty || namedStaff.LS_Code.EqualsIgnoringCase(staffCodeUpperCase)))
				{
					staff = namedStaff;
				}
			}

			if (staff == null)
			{
				staff = Factory.New<ClientStaff>();
				staff.LS_Code = staffCodeUpperCase;
				staff.LS_LD = databasePkForStaffList;
				databaseStaffCodeMap[staff.LS_Code] = staff;
				if (!isWebUser)
				{
					staff.LS_FullName = log.StaffName;
					staff.LS_Email = log.StaffEmail;
					databaseStaffNameMap[staff.LS_FullName] = staff;
				}
				else
				{
					staff.LS_FullName = User.WebUserName;
				}
			}
			else if (!isWebUser)
			{
				if (staff.LS_Code.IsEmpty && !log.StaffCode.IsEmpty)
				{
					// new report format has discovered the code
					staff.LS_Code = log.StaffCode;
					databaseStaffCodeMap[staff.LS_Code] = staff;
				}

				if (staff.LS_FullName != log.StaffName)
				{
					staff.LS_FullName = log.StaffName;
					databaseStaffNameMap[staff.LS_FullName] = staff;
				}

				if (staff.LS_Email != log.StaffEmail)
				{
					staff.LS_Email = log.StaffEmail;
				}
			}

			return staff;
		}

		void EnsureLoadedAllDatabaseStaff(ZGuid databasePK)
		{
			if (databasePkForStaffList != databasePK)
			{
				databasePkForStaffList = databasePK;
				databaseStaffList = Factory.Load<ClientStaff>(new ZQuery(ClientStaffSchema.LS_LD, databasePK));
				databaseStaffCodeMap = new Dictionary<string, ClientStaff>(StringComparer.OrdinalIgnoreCase);
				databaseStaffNameMap = new Dictionary<string, ClientStaff>(StringComparer.OrdinalIgnoreCase);

				foreach (var staff in databaseStaffList)
				{
					if (!staff.LS_Code.IsEmpty)
					{
						databaseStaffCodeMap[staff.LS_Code] = staff;
					}

					if (!staff.LS_FullName.IsEmpty)
					{
						databaseStaffNameMap[staff.LS_FullName] = staff;
					}
				}
			}
		}

		void NotifyRequester(LicenceConsumptionLogSchema usageData, string xml, BusinessObjectFactory factory)
		{
			if (usageData.IsRequest)
			{
				string subject = string.Format(CultureInfo.CurrentCulture, "Licence Usage received from [{0}/-/{1}] for {2} to {3}",
					usageData.EnterpriseCode,
					usageData.ServerCode,
					usageData.DateFrom.ToShortDateString(),
					usageData.DateTo.ToShortDateString());

				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Information, subject);
				}

				if (!usageData.RequestedBy.IsEmpty)
				{
					GlbStaff staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, usageData.RequestedBy);
					if (staff != null && !staff.GS_EmailAddress.IsEmpty)
					{
						Env.OutgoingMailManager.CreateAndSaveSimple(subject, xml, staff.GS_EmailAddress);
					}
				}
			}
		}

		void ReportErrors(string licenceCode, string message)
		{
			if (ServiceLogger != null)
			{
				ServiceLogger.Log(LogType.Warning, "On Demand Licence Usage Error - " + licenceCode + System.Environment.NewLine + message);
			}
		}

		#region Implementation

		/// <summary>
		/// Create contacts for any using staff where the name and the email don't match any existing contact.
		/// </summary>
		void CreateContactsByEmailOrName(BusinessObjectFactory contactFactory, ZGuid orgPk, Dictionary<string, string> staffNameEmailMap)
		{
			// Select existing names and emails
			string sql = "select " + OrgContactSchema.Constants.OC_ContactName + ", " + OrgContactSchema.Constants.OC_Email +
				" from " + OrgContactSchema.Constants.SqlSchemaName + "." + OrgContactSchema.Constants.TableName +
				" where " + OrgContactSchema.Constants.OC_OH + " = @OrgPk";

			// Remove existing names from staffNameEmailMap.
			// Build a dictionary of existing emails.
			Dictionary<string, string> existingEmails = new Dictionary<string, string>(staffNameEmailMap.Count * 2 + 1000, StringComparer.OrdinalIgnoreCase);
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@OrgPk", System.Data.SqlDbType.UniqueIdentifier, orgPk.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var name = reader[0].ToString();
						var email = reader[1].ToString();

						existingEmails[email] = null;
						staffNameEmailMap.Remove(name);
					}
				}
			}

			foreach (var pair in staffNameEmailMap)
			{
				var email = pair.Value;
				if (string.IsNullOrEmpty(email) || !existingEmails.ContainsKey(email))
				{
					var contact = contactFactory.New<OrgContact>();
					contact.SuspendValidation();
					contact.OC_OH = orgPk;
					contact.OC_ContactName = pair.Key;
					contact.OC_Email = email;
				}
			}
		}

		#endregion
	}
}

