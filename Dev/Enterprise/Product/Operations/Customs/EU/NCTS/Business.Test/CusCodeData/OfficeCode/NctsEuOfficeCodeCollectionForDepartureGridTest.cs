using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEuOfficeCodeCollectionForDepartureGrid))]
	public class NctsEuOfficeCodeCollectionForDepartureGridTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NctsEuOfficeCodeCollectionForDepartureGrid(DepartureMovementPhase5ForTest());
		}

		public void TestCusCodeDataShouldHaveCorrectParent_Phase4()
		{
			var parent = Factory.New<NctsHeader>();
			parent.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			parent.SetMovementType(NctsMovementType.Codes.Departure);

			var cusCodes = Factory.Load<EuOfficeCode>(new ZQuery(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode).AddToFilter(CusCodeDataSchema.CY_ParentID, parent.PK));
			AssertEquals("There should be 1 office filled.", 2, cusCodes.Length);
			var cusCode = cusCodes[0];
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent.PK, cusCode.CY_ParentID);
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent, cusCode.Parent);
		}

		public void TestCusCodeDataShouldHaveCorrectParent_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var parent = header.MovementHeader;

			var cusCodes = Factory.Load<EuOfficeCode>(new ZQuery(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode).AddToFilter(CusCodeDataSchema.CY_ParentID, parent.PK));
			AssertEquals("There should be 1 office filled.", 2, cusCodes.Length);
			var cusCode = cusCodes[0];
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent.PK, cusCode.CY_ParentID);
			AssertEquals("The auto filled CusCodeData should have correct parent set.", parent, cusCode.Parent);
		}

		public void TestDSAofficeIsNotLoaded()
		{
			var parent = Factory.NewWithValidTestData<NctsDepartureMovementHeader>();

			var dsa = parent.CustomsOffices.AddNew();
			dsa.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			dsa.CY_Data = "DSAdata";

			var des = parent.CustomsOffices.AddNew();
			des.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
			des.CY_Data = "DESdata";

			Factory.Save();

			AssertEquals("DSA office is not loaded.", 1, parent.CustomsOfficesForDeparture.Count);
		}

		public void TestCheckRuleR0103()
		{
			var messageError = Messages.R0103Message;

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.EnableRule(x => x.IsRuleR0103Active);

				var movementHeader = DepartureMovementPhase5ForTest();

				var customsOfficeTRA = movementHeader.CustomsOffices.AddNew();
				customsOfficeTRA.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

				var customsOfficeTXT = movementHeader.CustomsOffices.AddNew();
				customsOfficeTXT.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;

				CombineAssertions(() =>
				{
					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301050";
					customsOfficeTRA.CY_Data = "PL301050";
					customsOfficeTXT.CY_Data = "PL301050";
					AssertHasMessageError("ExitForTransit == Transit AND ExitForTransit == Destination", customsOfficeTXT.CY_DataInfo, messageError);

					customsOfficeTRA.CY_Data = "PL301060";
					customsOfficeTXT.CY_Data = "PL301060";
					AssertHasMessageError("ExitForTransit == Transit AND ExitForTransit != Destination", customsOfficeTXT.CY_DataInfo, messageError);

					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301070";
					customsOfficeTXT.CY_Data = "PL301070";
					AssertHasMessageError("ExitForTransit != Transit AND ExitForTransit == Destination", customsOfficeTXT.CY_DataInfo, messageError);

					customsOfficeTXT.CY_Data = "PL301080";
					AssertNoMessageError("ExitForTransit != Transit AND ExitForTransit != Destination", customsOfficeTXT.CY_DataInfo, messageError);

					ruleContext.DisableRule(x => x.IsRuleR0103Active);

					movementHeader.DestinationCustomsOfficeCodeForDeparture = "PL301050";
					customsOfficeTRA.CY_Data = "PL301050";
					customsOfficeTXT.CY_Data = "PL301050";
					AssertNoMessageError("ExitForTransit == Transit AND ExitForTransit == Destination, rule is disabled", customsOfficeTXT.CY_DataInfo, messageError);
				});
			}
		}

		public void TestCheckC0030P5CustomOfficesCommonRule()
		{
			var movementHeader = DepartureMovementPhase5ForTest();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "AD", "Andorra", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 12, 31));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CY", "Cyprus", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 12, 31));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "PL", "Polland", new ZDateTime(2023, 01, 01), new ZDateTime(2023, 12, 31));
			Factory.Save();

			using (var ruleContext = new NctsEuOfficeCodeValidationDeciderTestContext<INctsEuOfficeCodeDeparturePhase5ValidationDecider>(Factory))
			{
				ruleContext.DisableRule(c => c.IsRuleB1836Active);
				ruleContext.EnableRule(c => c.IsRuleC0030Active);

				var officeOfDeparture = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "AD000001");
				AssertHasMessageError("Custom office for  transit office is not entered.", officeOfDeparture.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				var officeOfDestination = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "AD000001");
				AssertHasMessageError("Custom office for  transit office is not entered.", officeOfDestination.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

				officeOfDeparture.CY_Data = "PL301080";
				officeOfDestination.CY_Data = "AD000001";
				officeOfDestination.Validation.ValidateCY_Data();
				AssertHasMessageError("Custom office for transit office is not entered.", officeOfDestination.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

				var officeOfTXT = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
				officeOfTXT.CY_Data = "PL301070";
				AssertHasMessageError("TXT - Custom office for transit office is not entered.", officeOfTXT.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

				var transitOffice = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
				transitOffice.CY_Data = "TraOffic";
				officeOfTXT.Validation.ValidateCY_Data();
				officeOfDestination.Validation.ValidateCY_Data();
				AssertNoMessageError("officeOfTXT - Custom office for transit is entered.", officeOfTXT.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				AssertNoMessageError("officeOfDestination - Custom office for transit is entered.", officeOfDestination.CY_DataInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
			}
		}

		public void TestCheckC0030P5CommonRule()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleC0030Active));

				UniversalReferenceTestDataHelper.RunAssertionsInPhase5TransitionPeriod(() =>
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
					AssertNoMessageError("In Phase 5 Transition Period", movementHeader.BM_InBondEntryTypeInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				});

				UniversalReferenceTestDataHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
				{
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
					AssertHasMessageError("Custom office for transit office is not entered.", movementHeader.BM_InBondEntryTypeInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					var bill = nctsHeader.Bills.AddNew();
					var goodItem = bill.GoodsItems.AddNew();
					goodItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
					AssertHasMessageError("Custom office for transit office is not entered.", movementHeader.BM_InBondEntryTypeInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
					AssertNoMessageError("MovementHeader.BM_InBondEntryType is not T or T2", movementHeader.BM_InBondEntryTypeInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);

					movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
					var transitOffice = movementHeader.CustomsOfficesForDeparture.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
					transitOffice.CY_Data = "TraOffic";
					movementHeader.Validation.ValidateBM_InBondEntryType();
					AssertNoMessageError("Custom office for transit is entered.", movementHeader.BM_InBondEntryTypeInfo, NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				});
			}
		}

		NctsDepartureMovementHeader DepartureMovementPhase5ForTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader;
		}

		public ValidationRuleMessages Messages => messages ?? (messages = GetNewMessagesCore());
		ValidationRuleMessages messages;

		protected virtual ValidationRuleMessages GetNewMessagesCore() => new ValidationRuleMessages();
	}
}
