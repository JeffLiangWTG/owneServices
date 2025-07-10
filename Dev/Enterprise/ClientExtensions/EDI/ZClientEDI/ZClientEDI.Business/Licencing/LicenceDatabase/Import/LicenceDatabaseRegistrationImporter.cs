using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public static class LicenceDatabaseRegistrationImporter
	{
		public class ImportResult
		{
			public bool Success { get; set; }
			public string OutputMessage { get; set; }
			public int DatabaseNumber { get; set; }
		}

		#region Register with LicenceDatabaseRegistration

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static ImportResult TryImportLicenceDatabase(LicenceDatabaseRegistration registration)
		{
			var result = new ImportResult();

			if (string.IsNullOrWhiteSpace(registration.Product))
			{
				result.OutputMessage = StatusMessages.InvalidProductType;
			}
			else if (!string.IsNullOrWhiteSpace(registration.LicenceType) && !new DatabaseTypes().ContainsCode(registration.LicenceType))
			{
				result.OutputMessage = StatusMessages.InvalidLicenceType;
			}
			else
			{
				Exception exception = null;
				for (var retry = 0; retry < 3; ++retry)
				{
					exception = null;
					try
					{
						result = TryImport(registration);
						break;
					}
					catch (ZSaveConcurrencyException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(100);
					}
					catch (ZSaveException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(500);
					}
					catch (Exception ex)
					{
						exception = ex;
						break;
					}
				}

				if (exception != null)
				{
					result.Success = false;
					result.DatabaseNumber = 0;
					result.OutputMessage = StatusMessages.InternalException + System.Environment.NewLine + exception.ToString();
					ErrorReporter.ReportOnce("LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase", result.OutputMessage, exception);
				}
			}

			if (!string.IsNullOrWhiteSpace(result.OutputMessage))
			{
				SendNotificationEmail(result, registration);
			}
			return result;
		}

		static ImportResult TryImport(LicenceDatabaseRegistration registration)
		{
			var result = new ImportResult();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(LicenceDatabaseSchema.LD_Product, registration.Product)
				.AddToFilter(LicenceDatabaseSchema.LD_TenantID, registration.SystemId);
			var database = factory.LoadTop1<LicenceDatabase>(query);

			if (database == null)
			{
				var defaultEntID = EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value;

				if (string.IsNullOrWhiteSpace(defaultEntID))
				{
					result.OutputMessage = StatusMessages.InvalidEnterpriseID;
				}
				else
				{
					var defaultEnt = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, defaultEntID));
					if (defaultEnt == null)
					{
						result.OutputMessage = StatusMessages.InvalidEnterpriseID;
					}
					else
					{
						database = defaultEnt.Databases.AddNew();
						database.LD_ServerCode = GetNextServerCode(defaultEnt.PK);
						database.LD_Product = registration.Product;
						database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
						database.LD_TenantID = registration.SystemId;
						database.LD_Status = DatabaseStatusList.Codes.NON;
						database.LD_Billable = DatabaseBillableFlagList.Codes.No;
						database.LD_OH_WebAccessOrg = ZGuid.Empty;
						database.LD_AllowAutoLogin = false;
						if (!string.IsNullOrWhiteSpace(registration.LicenceType))
						{
							database.LD_LicenceType = registration.LicenceType;
						}
						result.OutputMessage = StatusMessages.NewDatabaseImported;
						result.Success = true;
					}
				}
			}
			else
			{
				UpdateExistingDatabaseStatus(result, database);
			}

			if (result.Success)
			{
				SetDatabaseBeforeSaving(database, registration);
				factory.Save();
				result.DatabaseNumber = database.LD_DatabaseNumber;
			}

			return result;
		}

		static void SetDatabaseBeforeSaving(LicenceDatabase database, LicenceDatabaseRegistration registration)
		{
			database.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, registration.ToString());
			var trustedSystem = database.GetOrCreateTrustedSystem();

			if (!string.IsNullOrWhiteSpace(registration.SystemEndpointUrl))
			{
				trustedSystem.ETS_SystemEndpointUrl = registration.SystemEndpointUrl;
			}
		}

		static void SendNotificationEmail(ImportResult result, LicenceDatabaseRegistration registration)
		{
			var subject = result.Success ? $"Licence Database Registration Successful" : $"Licence Database Registration Failed";
			var message = FormattableString.Invariant($@"
{registration.ToString()}

{result.OutputMessage}");
			SendNotificationEmail(subject, message);
		}

		#endregion Register with LicenceDatabaseRegistration

		#region Register with TenantRegistrationInfo

		public static ImportResult TryImportLicenceDatabase(TenantRegistrationInfo tenantInfo)
		{
			var result = new ImportResult();

			if (string.IsNullOrWhiteSpace(tenantInfo.Product))
			{
				result.OutputMessage = StatusMessages.InvalidProductType;
			}
			else if (!string.IsNullOrWhiteSpace(tenantInfo.LicenceType) && !new DatabaseTypes().ContainsCode(tenantInfo.LicenceType))
			{
				result.OutputMessage = StatusMessages.InvalidLicenceType;
			}
			else
			{
				Exception exception = null;
				for (var retry = 0; retry < 3; ++retry)
				{
					exception = null;
					try
					{
						result = TryImport(tenantInfo);
						break;
					}
					catch (ZSaveConcurrencyException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(100);
					}
					catch (ZSaveException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(500);
					}
					catch (Exception ex)
					{
						exception = ex;
						break;
					}
				}

				if (exception != null)
				{
					result.Success = false;
					result.DatabaseNumber = 0;
					result.OutputMessage = StatusMessages.InternalException + System.Environment.NewLine + exception.ToString();
					ErrorReporter.ReportOnce("LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase", result.OutputMessage, exception);
				}
			}

			if (!string.IsNullOrWhiteSpace(result.OutputMessage))
			{
				SendNotificationEmail(result, tenantInfo);
			}
			return result;
		}

		static ImportResult TryImport(TenantRegistrationInfo tenantInfo)
		{
			var result = new ImportResult();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			var systemQuery = new ZQuery(EdiTrustedSystemSchema.ETS_Product, tenantInfo.Product);
			systemQuery.AddToFilter(EdiTrustedSystemSchema.ETS_SystemID, tenantInfo.SystemId);
			var trustedSystem = factory.LoadTop1<EdiTrustedSystem>(systemQuery);

			var tenantQuery = new ZQuery(LicenceDatabaseSchema.LD_Product, tenantInfo.Product);
			tenantQuery.AddToFilter(LicenceDatabaseSchema.LD_TenantID, tenantInfo.TenantId);
			var database = factory.LoadTop1<LicenceDatabase>(tenantQuery);

			if (trustedSystem != null)
			{
				if (database == null)
				{
					var defaultEntID = EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value;

					if (string.IsNullOrWhiteSpace(defaultEntID))
					{
						result.OutputMessage = StatusMessages.InvalidEnterpriseID;
					}
					else
					{
						var defaultEnt = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, defaultEntID));
						if (defaultEnt == null)
						{
							result.OutputMessage = StatusMessages.InvalidEnterpriseID;
						}
						else
						{
							database = defaultEnt.Databases.AddNew();
							database.LD_ServerCode = GetNextServerCode(defaultEnt.PK);
							database.LD_Product = tenantInfo.Product;
							database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
							database.LD_TenantID = tenantInfo.TenantId;
							database.LD_Status = DatabaseStatusList.Codes.NON;
							database.LD_Billable = DatabaseBillableFlagList.Codes.No;
							database.LD_OH_WebAccessOrg = ZGuid.Empty;
							database.LD_AllowAutoLogin = false;
							if (!string.IsNullOrWhiteSpace(tenantInfo.LicenceType))
							{
								database.LD_LicenceType = tenantInfo.LicenceType;
							}
							result.OutputMessage = StatusMessages.NewDatabaseImported;
							result.Success = true;
						}
					}
				}
				else
				{
					UpdateExistingDatabaseStatus(result, database);
				}

				if (result.Success)
				{
					database.LD_ETS_TrustedSystem = trustedSystem.PK;
					database.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, tenantInfo.ToString());
					factory.Save();
					result.DatabaseNumber = database.LD_DatabaseNumber;
				}
			}

			return result;
		}

		static void SendNotificationEmail(ImportResult result, ITenantRegistrationInfo tenantInfo)
		{
			var subject = result.Success ? $"Licence Database Registration Successful" : $"Licence Database Registration Failed";
			var message = FormattableString.Invariant($@"
{tenantInfo.ToString()}

{result.OutputMessage}");
			SendNotificationEmail(subject, message);
		}

		#endregion Register with TenantRegistrationInfo

		#region Register with SystemToSystemTrustTenantRegistrationInfo

		public static ImportResult TryImportLicenceDatabase(SystemToSystemTrustTenantRegistrationInfo tenantInfo, LicenceDatabase duplicateDatabase)
		{
			var result = new ImportResult();

			if (string.IsNullOrWhiteSpace(tenantInfo.Product))
			{
				result.OutputMessage = StatusMessages.InvalidProductType;
			}
			else if (!string.IsNullOrWhiteSpace(tenantInfo.LicenceType) && !new DatabaseTypes().ContainsCode(tenantInfo.LicenceType))
			{
				result.OutputMessage = StatusMessages.InvalidLicenceType;
			}
			else if (string.IsNullOrWhiteSpace(tenantInfo.TenantId))
			{
				result.OutputMessage = StatusMessages.MissingTenantId;
			}
			else
			{
				Exception exception = null;
				for (var retry = 0; retry < 3; ++retry)
				{
					exception = null;
					try
					{
						result = TryImport(tenantInfo, duplicateDatabase);
						break;
					}
					catch (ZSaveConcurrencyException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(100);
					}
					catch (ZSaveException ex)
					{
						exception = ex;
						System.Threading.Thread.Sleep(500);
					}
					catch (Exception ex)
					{
						exception = ex;
						break;
					}
				}

				if (exception != null)
				{
					result.Success = false;
					result.DatabaseNumber = 0;
					result.OutputMessage = StatusMessages.InternalException + System.Environment.NewLine + exception.ToString();
					ErrorReporter.ReportOnce("LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase", result.OutputMessage, exception);
				}
			}

			if (!string.IsNullOrWhiteSpace(result.OutputMessage))
			{
				SendNotificationEmail(result, tenantInfo);
			}
			return result;
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "Temporarily disabling it - it's buggy and thinks the exception being thrown is a return statement")]
		static ImportResult TryImport(SystemToSystemTrustTenantRegistrationInfo tenantInfo, LicenceDatabase duplicateDatabase)
		{
			var result = new ImportResult();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var database = (LicenceDatabase)null;

			if (duplicateDatabase == null)
			{
				var existingDatabaseQuery = new ZQuery(LicenceDatabaseSchema.LD_Product, tenantInfo.Product);
				existingDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_TenantID, tenantInfo.TenantId);

				var existingDatabaseMatchingTenantID = factory.LoadTop1<LicenceDatabase>(existingDatabaseQuery);

				if (existingDatabaseMatchingTenantID == null)
				{
					var defaultEntID = EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.Value;

					if (string.IsNullOrWhiteSpace(defaultEntID))
					{
						result.OutputMessage = StatusMessages.InvalidEnterpriseID;
					}
					else
					{
						var defaultEnt = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, defaultEntID));
						if (defaultEnt == null)
						{
							result.OutputMessage = StatusMessages.InvalidEnterpriseID;
						}
						else
						{
							database = defaultEnt.Databases.AddNew();
							database.LD_ServerCode = GetNextServerCode(defaultEnt.PK);
							database.LD_Product = tenantInfo.Product;
							database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
							database.LD_TenantID = tenantInfo.TenantId;
							database.LD_Status = DatabaseStatusList.Codes.NON;
							database.LD_Billable = DatabaseBillableFlagList.Codes.No;
							database.LD_OH_WebAccessOrg = ZGuid.Empty;
							database.LD_AllowAutoLogin = false;
							if (!string.IsNullOrWhiteSpace(tenantInfo.LicenceType))
							{
								database.LD_LicenceType = tenantInfo.LicenceType;
							}
							result.OutputMessage = StatusMessages.NewDatabaseImported;
							result.Success = true;
						}
					}
				}
				else
				{
					result.Success = true;
					result.DatabaseNumber = existingDatabaseMatchingTenantID.LD_DatabaseNumber;
					return result;
				}
			}
			else
			{
				database = factory.Load<LicenceDatabase>(duplicateDatabase.PK);
				UpdateExistingDatabaseStatus(result, database);
			}

			if (result.Success)
			{
				database.Notes.AddNew(isCustomDescription: true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, tenantInfo.ToString());
				factory.Save();
				result.DatabaseNumber = database.LD_DatabaseNumber;
			}

			return result;
		}

		#endregion Register with SystemToSystemTrustTenantRegistrationInfo

		static void UpdateExistingDatabaseStatus(ImportResult result, LicenceDatabase database)
		{
			switch (database.LD_Status)
			{
				case DatabaseStatusList.Codes.Preregistered:
					{
						if (database.LD_PreRegistrationExpiryDateUTC.IsEmpty || database.LD_PreRegistrationExpiryDateUTC >= ZDateTime.UtcNow)
						{
							database.LD_Status = DatabaseStatusList.Codes.REG;
							result.Success = true;
						}
						else
						{
							result.OutputMessage = StatusMessages.PreRegistrationExpired;
						}

						break;
					}
				case "":
				case DatabaseStatusList.Codes.NON:
					{
						result.Success = true;
						break;
					}
				case DatabaseStatusList.Codes.REG:
					{
						result.OutputMessage = StatusMessages.DatabaseRegisteredAlready;
						break;
					}
				default:
					{
						result.OutputMessage = StatusMessages.InvalidDatabaseStatus;
						break;
					}
			}
		}

		static string GetNextServerCode(ZGuid enterprisePK)
		{
			var sql = "SELECT LD_ServerCode FROM dbo.LicenceDatabase WHERE LD_LE = @LE";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@LE", SqlDbType.UniqueIdentifier, enterprisePK.ToGuid());
				using (var dt = DataUtils.GetDataTableFromCommand(cmd))
				{
					var codes = dt.Rows.OfType<DataRow>().Select(x => x[0].ToString()).ToHashSet();

					foreach (var i in Enumerable.Range(729, 27 * 27 * 27))
					{
						var newCode = Base27Encoding.Encode(i);
						if (!codes.Contains(newCode))
						{
							return newCode;
						}
					}
				}
			}

			throw new InvalidOperationException("Can not get next ServerCode.");
		}

		static void SendNotificationEmail(string subject, string message)
		{
			var regItem = EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup;
			var regFullName = string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;
			var mail = new EmailDef()
			{
				Subject = subject,
				Body =
$@"{message}

You are receiving this email because you are a member of the group in registry {regFullName}",
			};
			try
			{
				Env.OutgoingMailManager.CreateAndSave(mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
			}
			catch (EmailSendFailedException)
			{
			}
		}

		class StatusMessages
		{
			public const string InvalidProductType = "Invalid Product Type";
			public const string InvalidLicenceType = "Invalid Licence Type";
			public const string MissingTenantId = "Missing Tenant ID";
			public const string PreRegistrationExpired = "Pre-Registration Expired";
			public const string DatabaseRegisteredAlready = "Database Registered Already";
			public const string InvalidEnterpriseID = "Invalid Enterprise ID";
			public const string InternalException = "Internal Exception";
			public const string InvalidDatabaseStatus = "Invalid Database Status";
			public const string NewDatabaseImported = "New Database Imported";
		}
	}
}
