using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	class PpsMarkerAsClearedTest : TestCaseWithFactory
	{
		public void TestPPSMarkerAsClearedDoEverything()
		{
			using (FRCustomsDataRegistry.Instance.FallbackTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				SetUpOffices(declaration, EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep, true);
				SetUpEntry(declaration, true, DeltaGFallbackStatusList.Codes.PPS);
				var ppsMarkerAsCleared = new PpsMarkerAsCleared(Factory);

				cusEntryNumber.CE_IssueDate = ZDateTime.Now.AddMinutes(-9);
				Factory.Save();
				ppsMarkerAsCleared.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				cusEntryNumber.Reload();
				AssertEquals(DeltaGFallbackStatusList.Codes.PPS, cusEntryNumber.CE_EntryStatus);

				cusEntryNumber.CE_IssueDate = ZDateTime.Now.AddMinutes(-11);
				Factory.Save();
				ppsMarkerAsCleared.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				cusEntryNumber.Reload();
				AssertEquals(DeltaGFallbackStatusList.Codes.PDS, cusEntryNumber.CE_EntryStatus);
			}

			using (FRCustomsDataRegistry.Instance.FallbackTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var ppsMarkerAsCleared = new PpsMarkerAsCleared(Factory);

				cusEntryNumber.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PPS;
				cusEntryNumber.CE_IssueDate = ZDateTime.Now.AddMinutes(-29);
				Factory.Save();
				ppsMarkerAsCleared.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				cusEntryNumber.Reload();
				AssertEquals(DeltaGFallbackStatusList.Codes.PPS, cusEntryNumber.CE_EntryStatus);

				cusEntryNumber.CE_IssueDate = ZDateTime.Now.AddMinutes(-31);
				Factory.Save();
				ppsMarkerAsCleared.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				cusEntryNumber.Reload();
				AssertEquals(DeltaGFallbackStatusList.Codes.PDS, cusEntryNumber.CE_EntryStatus);
			}
		}

		void SetUpOffices(JobDeclaration declaration, ZString officeCode, ZBool createEmailAddress)
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var office = declaration.CustomsOffices.AddNew();
			office.CY_Code = officeCode;
			office.CY_Date = ZDateTime.Today;
			office.CY_Data = "FR000040";

			var office2 = declaration.CustomsOffices.AddNew();
			office2.CY_Code = EuOfficeCodesTypes.Codes.ActualExitOffice;
			office2.CY_Date = ZDateTime.Today;

			Factory.Save();

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = office.CY_Data;
			cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
			if (createEmailAddress)
			{
				var officeAttribute = cusCodeList.Attributes.AddNew();
				officeAttribute.ZZE_Value = "unit.test@cargowise.com";
				officeAttribute.ZZE_ZXE_NKName = "EmailAddress";
			}
			Factory.Save();
		}

		void SetUpEntry(JobDeclaration declaration, ZBool attachEntryNumber, ZString entryNumberStatus)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00000001";

			cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
			cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			cusEntryNumber.CE_EntryNum = "00003";
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_EntryStatus = entryNumberStatus;

			if (attachEntryNumber)
			{
				cusEntryNumber.Parent = entry;
			}

			Factory.Save();
		}

		CusEntryNumber cusEntryNumber;
	}
}
