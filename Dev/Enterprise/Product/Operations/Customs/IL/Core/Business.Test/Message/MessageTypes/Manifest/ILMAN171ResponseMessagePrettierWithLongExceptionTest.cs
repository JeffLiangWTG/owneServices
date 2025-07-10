using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170.MN_MSG1_MANIFEST;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILMAN171ResponseMessagePrettierWithLongExceptionTest : ILEDIMessagePrettierTest<MnMsg4SendManifestFeedBackMessage, ILMAN171ResponseMessageDataObject>
	{
		protected override ILEDIMessage GetNewMessage() => Factory.New<ILMAN171ResponseMessage>();

		public ZString GetExpectedMessageInterpretation_Exposed() => GetExpectedMessageInterpretation();

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_WithLongException_Interpretation.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_WithLongException.xml"));

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;

			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("CMAN", "IL GatePass Movement Status", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC1557_2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "CMAN", "1", "Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeListC1589_1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "CMAN", "9", "Not Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}
	}
}
