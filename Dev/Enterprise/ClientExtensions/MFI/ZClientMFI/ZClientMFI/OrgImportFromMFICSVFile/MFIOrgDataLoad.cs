using System;
using System.Collections.Generic;

using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.OrgImportFromMFICSVFile
{
	class MFIOrgDataLoad : OrgDataLoad
	{
		protected override void ImportDataCore(string dataLocation, string dataImporting)
		{
			for (int i = 1; i <= 2; i++)
			{
				base.ImportDataCore(dataLocation, dataImporting);
			}
		}

		protected override bool IsFileHeaderValid(OCsvLine headerLine)
		{
			bool result = ValidMandatoryFileHeaderColumns.Length == headerLine.FieldValues.Length;

			if (result)
			{
				for (int i = 0; i < ValidMandatoryFileHeaderColumns.Length; i++)
				{
					result = result && string.Equals(ValidMandatoryFileHeaderColumns[i], headerLine.FieldValues[i], StringComparison.OrdinalIgnoreCase);
				}
			}
			return result;
		}

		string[] GetNewFieldNames()
		{
			return new string[]
			{
				"CONTROLLINGAGENT",
				"SAL",
				"CUS",
				"INVOICEFREIGHTJOBSTO",
				"APC",
				"UOC",
				"DUN",
				"EIN",
				"TSANUMBER",
				"LASTINSPECT",
				"BRANCH"
			};
		}

		protected override string[] GetNewValidMandatoryFileHeaderColumns()
		{
			var validHeaderColumns = new List<string>(base.GetNewValidMandatoryFileHeaderColumns());
			int indexOfLanguageColumn = validHeaderColumns.IndexOf("LANGUAGE");
			validHeaderColumns.RemoveRange(indexOfLanguageColumn, validHeaderColumns.Count - indexOfLanguageColumn);
			validHeaderColumns.AddRange(GetNewFieldNames());
			return validHeaderColumns.ToArray();
		}

		protected override OrgDataLoad.CsvOrg GetNewCsvOrg()
		{
			return new MFICsvOrg();
		}

		protected override OrgDataLoad.CsvOrg ExtractCSVOrgData(OCsvLine line)
		{
			MFICsvOrg orgData = base.ExtractCSVOrgData(line) as MFICsvOrg;
			orgData.Language = ZString.Empty;
			orgData.MainAddress.Language = ZString.Empty;
			orgData.PostalAddress.Language = ZString.Empty;
			orgData.DeliveryAddress.Language = ZString.Empty;
			orgData.DeliveryAddress.Language = ZString.Empty;
			orgData.BankCurrency = ZString.Empty;

			orgData.ControllingAgent = MapValue("CONTROLLINGAGENT", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.SAL = MapValue("SAL", line, GlbStaffSchema.GS_Code.MaxLength);
			orgData.CUS = MapValue("CUS", line, GlbStaffSchema.GS_Code.MaxLength);
			orgData.InvoiceFreightJobsTo = MapValue("INVOICEFREIGHTJOBSTO", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.APC = MapValue("APC", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.UOC = MapValue("UOC", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.DUN = MapValue("DUN", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.EIN = MapValue("EIN", line, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength);
			orgData.TSANumber = MapValue("TSANUMBER", line, OrgCountryDataSchema.OV_EXApprovalNumber.MaxLength);
			orgData.LastInspect = MapValueAsDateTime("LASTINSPECT", line);
			orgData.Branch = MapValue("BRANCH", line, GlbBranchSchema.GB_Code.MaxLength);

			return orgData;
		}

		protected override Guid ProcessExtractedData(OrgDataLoad.CsvOrg orgData)
		{
			Guid transPK = Guid.Empty;
			var enterpriseOrg = FindOrganisationIfItExists(orgData) ?? Factory.New<OrgHeader>();

			try
			{
				LoadImportedExcelValues(enterpriseOrg, orgData);
				RunCounters.RecsToUpdate++;
				transPK = enterpriseOrg.PK.ToGuid();

				if (enterpriseOrg.IsInDatabase)
				{
					RunCounters.RecsUpdated++;
				}
				else
				{
					RunCounters.RecsCreated++;
				}
				RunCounters.RecsToUpdate++;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RunCounters.RecsExcluded++;
				DisplayFormattedLogMessage(orgData.OrgCode, ex.Message);
			}
			return transPK;
		}

		protected override void LoadImportedExcelValues(OrgHeader enterpriseOrganisation, OrgDataLoad.CsvOrg extractedData)
		{
			MFICsvOrg orgData = extractedData as MFICsvOrg;
			base.LoadImportedExcelValues(enterpriseOrganisation, extractedData);

			if (!orgData.ControllingAgent.IsEmpty)
			{
				MapRelatedParties(enterpriseOrganisation, orgData.ControllingAgent, OrgCusCode.CodeTypes.UniversalOfficeCode,
					RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			}

			if (!orgData.SAL.IsEmpty)
			{
				MapStaffAssignments(enterpriseOrganisation, orgData.SAL, StaffAssignmentRoles.Codes.SalesRep, OrgStaffAssignmentsLookups.AllServices);
			}

			if (!orgData.CUS.IsEmpty)
			{
				MapStaffAssignments(enterpriseOrganisation, orgData.CUS, StaffAssignmentRoles.Codes.CustomerServiceRep, OrgStaffAssignmentsLookups.AllServices);
			}

			if (!orgData.InvoiceFreightJobsTo.IsEmpty)
			{
				MapRelatedParties(enterpriseOrganisation, orgData.InvoiceFreightJobsTo, OrgCusCode.CodeTypes.LegacySystemCode,
					RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.PickupAndDelivery);
			}

			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, orgData.APC);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.UniversalOfficeCode, orgData.UOC);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.CodeTypes.DataUniversalNumberingSystem, orgData.DUN);
			LoadCustomsCode(enterpriseOrganisation, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, orgData.EIN);

			MapTSANumber(enterpriseOrganisation, orgData);

			if (!orgData.Branch.IsEmpty)
			{
				GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, orgData.Branch));
				if (branch != null)
				{
					enterpriseOrganisation.CompanyData.OB_GB_ControllingBranch = branch.PK;
				}
			}
		}

		void MapTSANumber(OrgHeader enterpriseOrganisation, MFICsvOrg orgData)
		{
			if (!orgData.TSANumber.IsEmpty)
			{
				if (enterpriseOrganisation.MainAddress != null)
				{
					OrgCountryData orgCountryDataForUpdate = null;

					OrgCountryData[] matchings = (OrgCountryData[])enterpriseOrganisation.CountryDataCollectionForThisCompany.Find(new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, enterpriseOrganisation.MainAddress.PK));

					if (matchings.Length == 1)
					{
						orgCountryDataForUpdate = matchings[0];
					}
					else
					{
						orgCountryDataForUpdate = enterpriseOrganisation.CountryDataCollectionForThisCompany.AddNew();
					}

					orgCountryDataForUpdate.OV_OA_ApprovedLocation = enterpriseOrganisation.MainAddress.PK;
					orgCountryDataForUpdate.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;
					orgCountryDataForUpdate.OV_EXApprovalNumber = orgData.TSANumber;

					if (orgData.LastInspect.IsValid)
					{
						orgCountryDataForUpdate.OV_EXSiteInspectionDate = orgData.LastInspect;
					}
				}
			}
		}

		void MapRelatedParties(OrgHeader enterpriseOrganisation, ZString orgDataCode, ZString cusCodeType, ZString relatedPartyType, ZString relatedPartyDirection)
		{
			OrgHeader relatedOrg = FindOrganisationFromOrgCusCode(orgDataCode, cusCodeType);

			if (relatedOrg != null)
			{
				enterpriseOrganisation.SetRelatedParty(relatedOrg, relatedPartyType, relatedPartyDirection);
			}
		}

		void MapStaffAssignments(OrgHeader enterpriseOrganisaiton, ZString staffCode, ZString role, ZString department)
		{
			GlbStaff assignedStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, staffCode));

			if (assignedStaff != null)
			{
				enterpriseOrganisaiton.StaffAssignments.SetStaffAssignment(role, assignedStaff.GS_Code, department);
			}
		}

		OrgHeader FindOrganisationFromOrgCusCode(ZString registrationNo, ZString codeType)
		{
			OrgHeader result = null;

			ZQuery cusCodeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, registrationNo);

			OrgCusCode customsCodeNumber = (OrgCusCode)Factory.LoadTop1(typeof(OrgCusCode), cusCodeFilter);

			if (customsCodeNumber != null)
			{
				result = Factory.Load<OrgHeader>(customsCodeNumber.OK_OH);
			}
			return result;
		}

		ZString MapValue(ZString fieldName, OCsvLine line)
		{
			ZString result = ZString.Empty;
			int index = FileHeaderColumns.IndexOf(fieldName.ToUpper());
			if (line.FieldValues.Length > index - 1 && line.FieldValues[index].Length > 0)
			{
				result = line.FieldValues[index].Trim();
			}
			return result;
		}

		List<string> FileHeaderColumns
		{
			get { return fileHeaderColumns ?? (fileHeaderColumns = new List<string>(ValidMandatoryFileHeaderColumns)); }
		}
		List<string> fileHeaderColumns;

		ZString MapValue(ZString fieldName, OCsvLine line, int fieldLength)
		{
			return MapValue(fieldName, line).SubstringSafe(0, fieldLength);
		}

		ZDateTime MapValueAsDateTime(ZString fieldName, OCsvLine line)
		{
			ZDateTime result = ZDateTime.Empty;
			ZString rawString = MapValue(fieldName, line);

			ZDateTime.TryParseExact(rawString, out result, ddMMyyyy);

			if (result.IsEmpty || !result.IsValid)
			{
				ZDateTime.TryParseExact(rawString, out result, yyyyMMdd);
			}
			return result;
		}

		const string ddMMyyyy = "dd/MM/yyyy";
		const string yyyyMMdd = "yyyyMMdd";

		#region MFICsvOrg
		class MFICsvOrg : OrgDataLoad.CsvOrg
		{
			public ZString ControllingAgent;
			public ZString SAL;
			public ZString CUS;
			public ZString InvoiceFreightJobsTo;
			public ZString APC;
			public ZString UOC;
			public ZString DUN;
			public ZString EIN;
			public ZString TSANumber;
			public ZDateTime LastInspect;
			public ZString Branch;
		}
		#endregion
	}
}
