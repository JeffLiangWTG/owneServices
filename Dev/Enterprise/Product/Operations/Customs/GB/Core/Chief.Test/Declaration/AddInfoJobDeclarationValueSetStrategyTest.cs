using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	class AddInfoJobDeclarationValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingPortsEtcUpdatedBox30()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "MAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist1.PK, "Type", "CA3");
			var codelist2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "SOU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist2.PK, "Type", "CA3");
			var codelist3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "LSA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist3.PK, "Type", "CA3");
			var codelist4 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "FXT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist4.PK, "Type", "SEA");
			var codelist5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "STN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist5.PK, "Type", "SEA");
			var codelist6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "OPY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist6.PK, "Type", "SEA");
			Factory.Save();

			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_MessageType = "IMP";
			dec.JE_EntrySubStyle = "A"; //arrived
			dec.JE_TransportMode = "AIR";
			dec.JE_RL_NKPortOfArrival = "GBMNC";
			AssertEquals("Box 30 is the IATA code for Manchester", "MAN", dec.JE_LocationOfGoods);
			dec.JE_RL_NKPortOfArrival = "GBSOU";
			AssertEquals("Box 30 is the IATA code for Southampton", "SOU", dec.JE_LocationOfGoods);
			dec.JE_RL_NKPortOfArrival = "GBSTN";
			AssertEquals("Box 30 is the CHIEF code for Stansted", "LSA", dec.JE_LocationOfGoods);
			dec.JE_TransportMode = "SEA";
			dec.JE_RL_NKPortOfArrival = "GBFXT";
			AssertEquals("FXT", dec.JE_LocationOfGoods);
			dec.JE_RL_NKPortOfArrival = "GBSOU";
			AssertEquals("Box 30 is the CHIEF code for Southampton SEA", "STN", dec.JE_LocationOfGoods);

			dec.JE_EntrySubStyle = "D"; //not arrived
			dec.JE_LocationOfGoods = "OPY";
			dec.JE_RL_NKPortOfArrival = "GBFXT";
			AssertEquals("Box 30 unchanged for substyle D", "OPY", dec.JE_LocationOfGoods);
			dec.JE_EntrySubStyle = "A";
			AssertEquals("Box 30 set on changing substyle", "FXT", dec.JE_LocationOfGoods);

			dec.JE_TransportMode = "XXX";
			dec.JE_LocationOfGoods = "OPY";
			dec.JE_RL_NKPortOfArrival = "GBFXT";
			AssertEquals("Box 30 unchanged for mode XXX", "OPY", dec.JE_LocationOfGoods);
			dec.JE_TransportMode = "SEA";
			AssertEquals("Box 30 set on changing mode", "FXT", dec.JE_LocationOfGoods);

			dec.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Box 30 unchanged when editing import loading port", "FXT", dec.JE_LocationOfGoods);

			dec.JE_MessageType = "EXP";
			dec.JE_EntrySubStyle = "A";
			dec.JE_LocationOfGoods = "OPY";
			dec.JE_RL_NKPortOfLoading = "GBFXT";
			AssertEquals("Box 30 updated to UK port when setting export's loading", "FXT", dec.JE_LocationOfGoods);
			dec.JE_LocationOfGoods = "OPY";
			dec.JE_RL_NKPortOfArrival = "USNYC";
			AssertEquals("Box 30 updated to UK port when setting export's destination", "FXT", dec.JE_LocationOfGoods);

			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dec.JE_MessageType = "IMP";
			dec.JE_EntrySubStyle = "D"; //not arrived, but registry is turned on
			dec.JE_LocationOfGoods = "";
			dec.JE_RL_NKPortOfArrival = "GBFXT";
			AssertEquals("Box 30 calculated for substyle D when registyr is on", "FXT", dec.JE_LocationOfGoods);
		}

		public void TestMasterURCCalculation_TrimLength()
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "DSK";
			badgeCodeSetting.RL_PortCode = "GBLBA";
			badgeCodeSetting.Direction = "EXP";
			badgeCodeSetting.CSPCode = "CCSUK";
			badgeCodeSetting.MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Eori;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			OrgHeader org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Turn, "123456789000");
			var dec = Factory.New<JobDeclaration>();

			dec.JE_MasterBill = (ZString)"Too Long".PadRight(JobDeclaration.Schema.JE_MasterBillMaxLength, '1');
			dec.JE_CustomsProfile = "DSK";
			dec.JE_MessageType = "EXP";
			dec.SubLocation = "BAC";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertEquals("GB/-" + (ZString)"Too Long".PadRight(JobDeclaration.Schema.JE_MasterBillMaxLength - 4, '1'), dec.JE_MasterUCR);
			AssertEquals("No explosion on long MUCR", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		public void TestSettingDeclarantUpdatesDucr()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000");
			orgCusCode.OK_RN_NKCodeCountry = "GB";
			jobDec.JE_UCR = "FOO";
			Factory.Save();
			jobDec.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			AssertContains("123456789000", jobDec.JE_UCR);
		}

		public void TestDefaultingOfAeoCertificateAndCodeTypeForJP()
		{
			GBCustomsDataRegistry.Instance.IncludeY02xSupportingDocumentsForAeoOrganisations.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOF1234567890");
			orgCusCode.OK_RN_NKCodeCountry = "JP";
			sDs = jobDec.SupportingDocuments;
			AssertEquals("No supporting docs have been added to the group invoice yet", 0, sDs.Count);
			jobDec.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Y031", sDs[0].CSI_Code);
			AssertEquals("AEOF1234567890", sDs[0].CSI_ReferenceNumber);
			jobDec.SupplierDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Y031", sDs[0].CSI_Code);
			AssertEquals("AEOF1234567890", sDs[0].CSI_ReferenceNumber);
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOF123456789");
			orgCusCode.OK_RN_NKCodeCountry = "GB";
			sDs = jobDec.SupportingDocuments;
			jobDec.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Y031", sDs[0].CSI_Code);
			AssertEquals("AEOF1234567890", sDs[0].CSI_ReferenceNumber);
		}

		public void TestDefaultingOfAeoCertificate()
		{
			GBCustomsDataRegistry.Instance.IncludeY02xSupportingDocumentsForAeoOrganisations.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			TestDefaultingOfAeoCertificateRunner(false);

			GBCustomsDataRegistry.Instance.IncludeY02xSupportingDocumentsForAeoOrganisations.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false);
			TestDefaultingOfAeoCertificateRunner(true);
		}

		void TestDefaultingOfAeoCertificateRunner(bool expectToSeeNothing)
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEOF123456789");
			orgCusCode.OK_RN_NKCodeCountry = "GB";

			sDs = jobDec.SupportingDocuments;
			AssertEquals("No supporting docs have been added to the group invoice yet", 0, sDs.Count);

			jobDec.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertAeoCertificateResults("Y023", 0, expectToSeeNothing);

			jobDec.SupplierDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertAeoCertificateResults("Y022", 1, expectToSeeNothing);

			jobDec.JE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			AssertAeoCertificateResults("Y024", 2, expectToSeeNothing);

			jobDec.WarehouseDocAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertAeoCertificateResults("Y027", 3, expectToSeeNothing);

			jobDec.JE_OH_ShippingLine = orgHeader.PK;
			AssertAeoCertificateResults("Y028", 4, expectToSeeNothing);

			jobDec.ImporterDocumentaryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals(expectToSeeNothing ? 0 : 4, sDs.Count);

			jobDec.SupplierDocumentaryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals(expectToSeeNothing ? 0 : 3, sDs.Count);

			jobDec.JE_OA_DeclarantAddress = Guid.Empty;
			AssertEquals(expectToSeeNothing ? 0 : 2, sDs.Count);

			jobDec.WarehouseDocAddress.E2_OA_Address = Guid.Empty;
			AssertEquals(expectToSeeNothing ? 0 : 1, sDs.Count);

			jobDec.JE_OH_ShippingLine = Guid.Empty;
			AssertEquals(0, sDs.Count);

			// Check that our routines do not add a duplicate if the user has manually added an SD, rather update it
			var manualSd = sDs.AddNew();
			manualSd.CSI_ReferenceNumber = "123333";
			manualSd.CSI_Code = "Y023";
			jobDec.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			if (expectToSeeNothing)
			{
				// Existing SD was unchanged
				AssertEquals("123333", sDs[0].CSI_ReferenceNumber);
			}
			else
			{
				AssertAeoCertificateResults("Y023", 0, false);
			}
		}

		void AssertAeoCertificateResults(string typeCode, int index, bool expectToSeeNothing)
		{
			if (expectToSeeNothing)
			{
				AssertEquals(0, sDs.Count);
			}
			else
			{
				AssertEquals(typeCode, sDs[index].CSI_Code);
				AssertEquals("J", sDs[index].CSI_Availability);
				AssertEquals("P", sDs[index].CSI_Actions);
				AssertEquals("GBAEOF123456789", sDs[index].CSI_ReferenceNumber);
			}
		}
		SupportingDocumentCollection sDs;
	}
}
