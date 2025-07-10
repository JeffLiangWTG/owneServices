using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class DAWBRecordTest : MAWBRecordTest
	{
		public override void TestPopulateSendingForwarder()
		{
			SetFieldValuesForTestPopulateSendingForwarder();
			DAWBRecord record = (DAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertNull("Should not try to match SendingForwarder for Direct Consol", record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("SHIPPER"));
		}

		public override void TestPopulateSendingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			Assert("Not relevant to Direct Consol as the SendingForwarder is never populated", true);
		}

		public override void TestPopulateReceivingForwarder()
		{
			SetFieldValuesForTestPopulateReceivingForwarder();
			DAWBRecord record = (DAWBRecord)GetNewRecord(ExpectedLineType, Fields.ConvertToJXCLine());
			JASForwardingConsol consol = record.LoadOrCreateConsol(FactoryProvider);
			record.UpdateConsol(HEADRecord, consol, NotificationBuffer);
			AssertNull("Should not try to match SendingForwarder for Direct Consol", record.FindOrCreateTempOrganisationParamsListForTest.GetByOrganisationName("CONSIGNEE"));
		}

		public override void TestPopulateReceivingForwarder_AddressLine1AssignedFromShipperStreetAddressIfNotAlreadyPopulated()
		{
			Assert("Not relevant to Direct Consol as the ReceivingForwarder is never populated", true);
		}

		protected override ZString ExpectedConsolAgentType
		{
			get
			{
				return Core.Constants.AgentType.Direct;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.DAWB;
			}
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new DAWBRecord(lineType, lineContent);
		}
	}
}
