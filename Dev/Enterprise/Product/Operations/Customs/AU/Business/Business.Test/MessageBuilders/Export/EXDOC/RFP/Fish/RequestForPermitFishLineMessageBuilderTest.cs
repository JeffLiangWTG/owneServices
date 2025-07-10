using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RequestForPermitFishLineMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateProductSourceState()
		{
			quarantineLine.QuarantineExDocHeader.QH_ObtainExportCustomsPermit = true;
			invoiceLine.JI_Drawback = false;
			invoiceLine.JI_AUState = "NSW";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Product Source State", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'LOC+ZZZ+NSW'", MessageTextForTesting, '\'');
		}

		public void TestGenerateCatchStartDate()
		{
			quarantineLine.QL_CatchStartDate = new ZDateTime(1987, 12, 11);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertContains("DTM+163:19871211:102'", MessageTextForTesting);
		}

		public void TestGenerateCatchEndDate()
		{
			quarantineLine.QL_CatchEndDate = new ZDateTime(1979, 08, 09);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertContains("DTM+164:19790809:102'", MessageTextForTesting);
		}

		public void TestGenerateLineImperialNetWeight()
		{
			quarantineLine.QL_ImperialNetWeightUnit = EXDOCImperialWeightUnitCodes.Codes.Ounce;
			quarantineLine.QL_ImperialNetWeight = 45.4m;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Imperial Net Weight", "LIN+1'MEA+AAI+AAL+ONZ:45.4'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDrainedWeight()
		{
			quarantineLine.QL_DrainedWeight = 35.7;
			quarantineLine.QL_DrainedWeightUnit = EXDOCMetricWeightUnitCodes.Codes.Kilogram;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Drained Weight", "LIN+1'MEA+AAI+AAF+KGM:35.7'PIA+5+X       :CC'ATT+10++N:FCI:AQ'", MessageTextForTesting, '\'');
		}

		public void TestGenerateHarvestStartDate()
		{
			var quarantineProcess = AddHarvestProcess();
			quarantineProcess.EE_StartDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest start date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'DTM+194:20061212:102'", MessageTextForTesting, '\'');
		}

		public void TestGenerateHarvestEndDate()
		{
			var quarantineProcess = AddHarvestProcess();
			quarantineProcess.EE_EndDate = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest end date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'DTM+206:20061212:102'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDepurationDate()
		{
			var quarantineProcess = AddHarvestProcess();
			quarantineProcess.EE_Depuration = new ZDateTime(2006, 12, 12);
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest depuration date", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'DTM+9:20061212:102'", MessageTextForTesting, '\'');
		}

		public void TestGenerateHarvestAreaLeaseNumber()
		{
			var quarantineProcess = AddHarvestProcess();
			quarantineProcess.EE_HarvestArea = "123";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest Area lease number", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'LOC+48+123'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDepurationPlantNumberInField()
		{
			var quarantineProcess = AddHarvestProcess();
			quarantineProcess.EE_AuthorisationEstablishmentID = "34653";
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest Area depuration plant number", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'PNA+SK+34653'", MessageTextForTesting, '\'');
		}

		public void TestGenerateDepurationPlantNumberInOrg()
		{
			var quarantineProcess = AddHarvestProcess();
			var estOrg = Factory.New<OrgHeader>();
			estOrg.OH_Code = "HATEST";
			var estNumber = estOrg.CustomsCodes.AddNew();
			estNumber.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			estNumber.OK_RN_NKCodeCountry = "AU";
			estNumber.OK_CustomsRegNo = "12345";
			Factory.Save();
			quarantineProcess.EE_AuthorisationEstablishmentID = ZString.Empty;
			var docAddresses = (IDocAddresses)quarantineLine.QuarantineExDocHeader.Declaration;
			var address = docAddresses.DocAddresses.CreateWithRequirement(docAddresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
			address.OrganisationPK = estOrg.PK;
			quarantineProcess.EE_E2_Address = address.PK;
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
			AssertMultilineEquals("Harvest Area depuration plant number", "LIN+1'PIA+5+X       :CC'ATT+10++N:FCI:AQ'PRC+HA:PP:AQ'PNA+SK+12345'", MessageTextForTesting, '\'');
		}

		public void TestGenerateErrata48Segments()
		{
			quarantineLine.QL_FishWaterIndicator = "F";
			invoiceLine.JI_CountryOfOrigin = "AU";
			Factory.Save();
			builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);

			var messageText = MessageTextForTesting;
			AssertNotContains("Should not populate Fish Water Indicator without FUNCS enabled.", "ATT+10++F:FWI:AQ", messageText);
			AssertNotContains("Should not populate Prod Country of Origin without FUNCS enabled.", "LOC+27", messageText);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata48, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				builder.GenerateLine(invoiceLine.JI_LineNo, invoiceLine);
				messageText = MessageTextForTesting;
				AssertContains("Should populate Fish Water Indicator when FUNCS enabled.", "ATT+10++F:FWI:AQ'", messageText);
				AssertContains("Should populate Prod Country of Origin when FUNCS enabled.", "LOC+27+AU'", messageText);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			sancrtMessage = new SANCRTMessage();
			builder = new RequestForPermitFishLineMessageBuilder(sancrtMessage, EXDOCMessageTypeCodes.Descriptions.LDG);
			invoiceLine = helper.Line1;
			quarantineLine = invoiceLine.QuarantineExDocLine;
			quarantineLine.QL_SendHCDesc = true;
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false);
		}

		JobComInvoiceLine invoiceLine;
		SANCRTMessage sancrtMessage;
		RequestForPermitFishLineMessageBuilder builder;
		QuarantineExDocLine quarantineLine;

		ZString MessageTextForTesting => sancrtMessage.ToString(new Edifact.UNOACharacterSet());

		QuarantineExDocEstablishmentAndTime AddHarvestProcess()
		{
			var quarantineProcess = quarantineLine.Processes.AddNew();
			quarantineProcess.EE_ProcessingType = EXDOCProcessTypeCodes.Codes.Harvest;
			return quarantineProcess;
		}
	}
}
