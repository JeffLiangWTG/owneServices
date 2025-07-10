using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRManifestLineDetailsBuilderTest : TestCaseWithFactory
	{
		public void TestShouldIncludeAdditionalLineDetails()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			var line = header.Lines.AddNew();
			var lineWrapper = new ExportCustomsManifestLinesManifestLineWrapper(line, 1);
			line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			AssertEquals("Send additional details", false, builder.ShouldIncludeAdditionalLineDetails(lineWrapper));
			line.EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			AssertEquals("Send additional details", true, builder.ShouldIncludeAdditionalLineDetails(lineWrapper));
			builder = new CMRManifestLineDetailsBuilder(true);
			AssertEquals("Send additional details", false, builder.ShouldIncludeAdditionalLineDetails(lineWrapper));
		}

		public void TestAirWayBillNumberMovedToLineLevel()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			var line = header.Lines.AddNew();
			line.EL_CAN = "8324923";
			line.EL_TypeOfCAN = "CAN";
			line.EL_AirWayBill = "08149382748";
			var messageBuilder = new EMMMessageBuilder(header);
			messageBuilder.PopulateMessages();
			Assert(messageBuilder.MessageText.Contains("08149382748"));
		}

		public void TestHouseBillNumberMovedToLineLevel()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			var line = header.Lines.AddNew();
			line.EL_CAN = "8324923";
			line.EL_TypeOfCAN = "CAN";
			line.EL_AirWayBill = "08149382748";
			var messageBuilder = new EMMMessageBuilder(header);
			messageBuilder.PopulateMessages();
			Assert(messageBuilder.MessageText.Contains("08149382748"));
		}

		public void TestDeletedLineOnlyBuildsCNISegment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var lineWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();
			lineWrapper.LineNumber = 5;
			var messageBuilder = new ESMMessageBuilder(consol, false);
			messageBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;

			var cuscar = new CUSCARMessage();
			builder = new CMRManifestLineDetailsBuilder(true);
			builder.PopulateSegment(cuscar.Group7.InstantiateAChildAndAddItToChildrenCollection(), null, lineWrapper);
			AssertEquals("Group 7 CNI segment for Deleted Line", "CNI+5+:::D'", cuscar.Group7[0].CNI[0].ToString(new UNOACharacterSet()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			builder = new CMRManifestLineDetailsBuilder(false);
		}

		CMRManifestLineDetailsBuilder builder;
	}
}
