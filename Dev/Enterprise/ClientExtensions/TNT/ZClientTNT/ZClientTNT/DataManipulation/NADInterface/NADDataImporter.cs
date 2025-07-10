using System;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.NADDataImport
{
	public class NADDataImporter
	{
		public NADDataImporter(NotificationBuffer notify)
		{
			Notify = notify;
		}

		readonly NotificationBuffer Notify;

		#region Implementation

		protected LoggingInformation Log;
		protected string CurrentLine;
		protected Guid ThirdPartyCreditorGroup;
		protected Guid ThirdPartyDebtorGroup;
		protected ZDecimal OrganisationsUpdated;
		protected ZDecimal OrganisationsCreated;

		#region DataFields
		protected ZString OrgID;
		protected ZString OrgMappingCode;
		protected ZString QuantumAccountNumber;
		protected ZString OrgLinkCode;
		protected ZString OrgName;
		protected ZString Address1;
		protected ZString Address2;
		protected ZString Address3;
		protected ZString City;
		protected ZString State;
		protected ZString Postcode;
		protected ZString Phone;
		protected ZString Fax;
		protected ZString ContactTitle;
		protected ZString ContactName;
		protected ZString ContactPhone;
		protected ZString UNLocode;
		protected ZString BusRegNo;
		protected bool IsDebtor;
		protected bool IsConsignor;
		protected bool StopTrade;
		protected bool VATExempt;
		protected ZString UpdateCode;
		#endregion

		BusinessObjectFactory Factory;

		internal void ProcessFiles(FileInfo[] nADFiles)
		{
			ProcessFiles(nADFiles, CancellationToken.None);
		}

		public void ProcessFiles(FileInfo[] nADFiles, CancellationToken token)
		{
			foreach (FileInfo file in nADFiles)
			{
				token.ThrowIfCancellationRequested();
				Notify.Notify(new InfoNotification("Updating NAD details... File: " + file.Name));
				GetDataFromFile(file.FullName, token);
				Notify.Notify(new InfoNotification("NAD updating completed"));
				MoveNADFileToProcessedDirectory(file, Notify);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected void GetDataFromFile(string filename, CancellationToken token)
		{
			Factory = new BusinessObjectFactory();
			ThirdPartyDebtorGroup = GetThirdPartyDebtorGroupPK();
			ThirdPartyCreditorGroup = GetThirdPartyCreditorGroupPK();
			OrganisationsCreated = 0;
			OrganisationsUpdated = 0;

			string[] fileDataRows = File.ReadAllLines(filename);

			ZDecimal totalRecs = 0;
			ZDecimal updatedRecs = 0;
			int displayCount = 0;

			foreach (string currentLine in fileDataRows)
			{
				token.ThrowIfCancellationRequested();
				totalRecs++;
				displayCount++;
				if (IsThisAnOrgRecordToUpdate(currentLine))
				{
					ExtractData(currentLine);
					UpdateOrganisation();
					updatedRecs++;
				}
				if (displayCount > 500)
				{
					Notify.Notify(new InfoNotification("Records processed: " + totalRecs.ToString() + "..."));
					SaveData();
					Factory = new BusinessObjectFactory();
					displayCount = 0;
				}
			}

			Notify.Notify(new InfoNotification("Records processed: " + totalRecs.ToString() + "..."));
			SaveData();

			ZStringBuilder builder = new ZStringBuilder();
			builder.Append("Total records in interface file: " + totalRecs.ToString());
			builder.Append("Records updated to CargoWise One: " + updatedRecs.ToString());
			builder.Append("Organisations created: " + OrganisationsCreated.ToString());
			builder.Append("Organisations updated: " + OrganisationsUpdated.ToString());

			Notify.Notify(new InfoNotification(builder.ToStringWithNewLineBetweenAppends()));
		}

		protected bool IsThisAnOrgRecordToUpdate(string dataRow)
		{
			if (dataRow.Length != 725)
			{
				return false;
			}
			else
			{
				return dataRow.Substring(0, 1) == "1" &&
					dataRow.Substring(4, 3) == "001" &&
					dataRow.Substring(356, 1) == "M";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected void SaveData()
		{
			try
			{
				Notify.Notify(new InfoNotification("Saving records to CargoWise One..."));
				Factory.Save();
				Notify.Notify(new InfoNotification("Save completed..."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notify.Notify(new InfoNotification("There is an error saving data :" + ex.Message));
			}
		}

		void MoveNADFileToProcessedDirectory(FileInfo fileToMove, NotificationBuffer notify)
		{
			ZDateTime currentDateTime = ZDateTime.Now;
			ZString currentDateString = currentDateTime.ToString("yyyyMMdd");
			ZString currentTimeString = currentDateTime.ToString("HHmmss");

			try
			{
				DirectoryInfo directory = new DirectoryInfo(Path.Combine(TNTDataRegistry.Instance.NADFileProcessedDirectory, currentDateString));
				if (!directory.Exists)
				{
					directory.Create();
				}

				String fileName = fileToMove.Name;
				fileName += "." + currentTimeString;
				FileInfo newFile = new FileInfo(Path.Combine(directory.FullName, fileName));

				fileToMove.MoveTo(newFile.FullName);
				notify.Notify(new InfoNotification("NAD File: " + fileToMove.Name + " is moved to processed directory"));
			}
			catch (IOException ex)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, "Error occurred while moving the NAD File: " + fileToMove.Name + " to the Processed Directory -" + ex.Message));
			}
		}

		protected void ExtractData(string dataRow)
		{
			UpdateCode = dataRow.Substring(1, 1);
			OrgMappingCode = dataRow.Substring(10, 12);
			OrgName = dataRow.Substring(22, 50).Trim();
			Address1 = dataRow.Substring(100, 30).Trim();
			Address2 = dataRow.Substring(130, 30) + dataRow.Substring(160, 30).Trim();
			City = dataRow.Substring(220, 30).Trim();
			State = dataRow.Substring(250, 3).Trim();
			Postcode = dataRow.Substring(253, 9).Trim();
			UNLocode = UnlocoMappingUtils.GetPortCodeFromAustralianState(State);
			Fax = dataRow.Substring(304, 7).Trim() + " " + dataRow.Substring(311, 9).Trim();
			Phone = dataRow.Substring(320, 7).Trim() + " " + dataRow.Substring(327, 9).Trim();
			IsConsignor = false;
			BusRegNo = dataRow.Substring(336, 20).Trim();
			if (ABNValidation.CheckValidABN(BusRegNo))
			{
				IsConsignor = true;
			}
			QuantumAccountNumber = dataRow.Substring(357, 9);
			VATExempt = false;
			if (dataRow.Substring(425, 1) == "Y")
			{
				VATExempt = true;
			}

			if (dataRow.Substring(564, 1) == "0" || dataRow.Substring(564, 1) == " ")
			{
				StopTrade = false;
			}
			else
			{
				StopTrade = true;
			}
			ContactTitle = dataRow.Substring(607, 4).Trim();
			if (ContactTitle.IsEmpty)
			{
				ContactName = dataRow.Substring(611, 22).Trim() + " " + dataRow.Substring(633, 22).Trim();
			}
			else
			{
				ContactName = ContactTitle + " " + dataRow.Substring(611, 22).Trim() + " " + dataRow.Substring(633, 22).Trim();
			}
			if (!ContactName.IsEmpty && ContactName.StartsWith(" "))
			{
				ContactName = RemoveLeadingSpaces(ContactName);
			}
			ContactPhone = dataRow.Substring(659, 7).Trim() + " " + dataRow.Substring(327, 9).Trim();
			IsDebtor = true;
		}

		protected void UpdateOrganisation()
		{
			OrgHeader enterpriseOrg = OrgHeader.FindByAccountID(Factory, QuantumAccountNumber);
			if (enterpriseOrg == null)
			{
				OrgHeader newEnterpriseOrg = Factory.New<OrgHeader>();
				newEnterpriseOrg = LoadImportedOrgValues(newEnterpriseOrg);
				OrganisationsCreated++;
			}
			else
			{
				enterpriseOrg = LoadImportedOrgValues(enterpriseOrg);
				OrganisationsUpdated++;
			}
		}

		#endregion

		#region Utilities

		protected OrgHeader LoadImportedOrgValues(OrgHeader enterpriseOrganisation)
		{
			enterpriseOrganisation.OH_FullName = OrgName.ToUpper();
			enterpriseOrganisation.MainAddress.OA_Address1 = Address1;
			enterpriseOrganisation.MainAddress.OA_Address2 = Address2.Left(OrgAddress.Schema.OA_Address2MaxLength);
			enterpriseOrganisation.MainAddress.OA_City = City.Left(OrgAddress.Schema.OA_CityMaxLength);
			enterpriseOrganisation.MainAddress.OA_PostCode = Postcode;
			enterpriseOrganisation.MainAddress.OA_State = State;
			enterpriseOrganisation.MainAddress.OA_Phone_Formatted = Phone;
			enterpriseOrganisation.MainAddress.OA_Fax_Formatted = Fax;
			enterpriseOrganisation.OH_IsDebtor = IsDebtor;
			enterpriseOrganisation.OH_IsConsignor = IsConsignor;
			if (IsDebtor)
			{
				enterpriseOrganisation.MiscServ.OM_OJ_ARDebtorGroup = ThirdPartyDebtorGroup;
				enterpriseOrganisation.MiscServ.OM_AROnCreditHold = StopTrade;
			}
			enterpriseOrganisation.OH_RL_NKClosestPort = UNLocode;
			enterpriseOrganisation.OH_Code = OrgHeaderMappingUtils.GetCompanyCodeFromExternalCode(enterpriseOrganisation, QuantumAccountNumber);
			if (!BusRegNo.IsEmpty)
			{
				LoadBusinessRegNo(enterpriseOrganisation);
			}
			if (!OrgMappingCode.IsEmpty)
			{
				CreatePatternMatch(enterpriseOrganisation, OrgMappingCode);
			}
			if (!QuantumAccountNumber.IsEmpty)
			{
				LoadLegacySystemAccount(enterpriseOrganisation, QuantumAccountNumber);
			}
			if (!ContactName.IsEmpty)
			{
				LoadPrimeContact(enterpriseOrganisation);
			}
			if (UpdateCode == "D")
			{
				enterpriseOrganisation.OH_IsActive = false;
			}
			else
			{
				enterpriseOrganisation.OH_IsActive = true;
			}
			if (VATExempt)
			{
				enterpriseOrganisation.CompanyData.SetARTaxApplicable(false);
			}

			return enterpriseOrganisation;
		}

		Guid GetThirdPartyDebtorGroupPK()
		{
			OrgDebtorGroup debtorGroup = Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, "TPY");
			return (debtorGroup != null) ? debtorGroup.PK.ToGuid() : Guid.Empty;
		}

		Guid GetThirdPartyCreditorGroupPK()
		{
			OrgCreditorGroup creditorGroup = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, "TPY");
			return (creditorGroup != null) ? creditorGroup.PK.ToGuid() : Guid.Empty;
		}

		protected OrgHeader LoadBusinessRegNo(OrgHeader enterpriseOrganisation)
		{
			// Only update ABN if it is not already on Enterprise record
			string codeType = DetermineRegistrationCodeType(BusRegNo);
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, enterpriseOrganisation.PK);
			OrgCusCode businessNumber = Factory.LoadTop1<OrgCusCode>(codeFilter);

			if (businessNumber == null)
			{
				OrgCusCode newEnterpriseOrgBusinessNumber = enterpriseOrganisation.CustomsCodes.AddNew();
				newEnterpriseOrgBusinessNumber.OK_CodeType = codeType;
				newEnterpriseOrgBusinessNumber.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				newEnterpriseOrgBusinessNumber.OK_CustomsRegNo = BusRegNo;
			}

			return enterpriseOrganisation;
		}

		protected string DetermineRegistrationCodeType(ZString registrationNo)
		{
			string registrationCodeType = OrgCusCode.CodeTypes.CorporationCode;
			if (ABNValidation.CheckValidABN(registrationNo))
			{
				registrationCodeType = OrgCusCode.CodeTypes.GSTCode;
			}

			return registrationCodeType;
		}

		protected OrgHeader LoadLegacySystemAccount(OrgHeader enterpriseOrganisation, ZString legacyNumber)
		{
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.LegacySystemCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_OH, enterpriseOrganisation.PK);
			OrgCusCode legacyNumberXref = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), codeFilter);
			if (legacyNumberXref == null)
			{
				OrgCusCode newEnterpriseOrgLegacyXrefNumber = enterpriseOrganisation.CustomsCodes.AddNew();
				newEnterpriseOrgLegacyXrefNumber.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				newEnterpriseOrgLegacyXrefNumber.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				newEnterpriseOrgLegacyXrefNumber.OK_CustomsRegNo = legacyNumber;
			}
			else
			{
				legacyNumberXref.OK_CodeType = OrgCusCode.CodeTypes.LegacySystemCode;
				legacyNumberXref.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				legacyNumberXref.OK_CustomsRegNo = legacyNumber;
			}
			return enterpriseOrganisation;
		}

		protected OrgHeader CreatePatternMatch(OrgHeader enterpriseOrganisation, ZString codePattern)
		{
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, codePattern);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, enterpriseOrganisation.PK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, "ORG");
			OrgPatternMatchOverride orgPattern = Factory.LoadTop1<OrgPatternMatchOverride>(filter);
			if (orgPattern == null)
			{
				OrgPatternMatchOverride newEnterpriseOrgPatternMatchOverride = Factory.New<OrgPatternMatchOverride>();
				newEnterpriseOrgPatternMatchOverride.OO_OH = enterpriseOrganisation.PK;
				newEnterpriseOrgPatternMatchOverride.OO_ForeignCode = codePattern;
				newEnterpriseOrgPatternMatchOverride.OO_Relationship = "ORG";
			}
			else
			{
				orgPattern.OO_ForeignCode = codePattern;
				orgPattern.OO_Relationship = "ORG";
			}
			return enterpriseOrganisation;
		}

		protected OrgHeader LoadPrimeContact(OrgHeader enterpriseOrganisation)
		{
			ZString contactNameOfLength = ContactName.Left(OrgContact.Schema.OC_ContactNameMaxLength);
			ZQuery contactFilter = new ZQuery(OrgContactSchema.OC_ContactName, contactNameOfLength);
			contactFilter.AddToFilter(OrgContactSchema.OC_OH, enterpriseOrganisation.PK);
			OrgContact organisationContact = Factory.LoadTop1<OrgContact>(contactFilter);
			if (organisationContact == null)
			{
				OrgContact newEnterpriseOrganisationContact = enterpriseOrganisation.Contacts.AddNew();
				newEnterpriseOrganisationContact.OC_ContactName = contactNameOfLength;
				newEnterpriseOrganisationContact.OC_Phone_Formatted = ContactPhone;
				newEnterpriseOrganisationContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
			}
			else
			{
				organisationContact.OC_Phone_Formatted = ContactPhone;
			}
			return enterpriseOrganisation;
		}

		protected ZString RemoveLeadingSpaces(ZString value)
		{
			ZString newValue = value;
			while (newValue.StartsWith(" "))
			{
				newValue = newValue.Replace(" ", "");
			}
			return newValue;
		}

		#endregion
	}
}
