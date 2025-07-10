using CargoWise.Customs.IL.MessageDefinitions.GPM.REQ_130.GP_NG_1030_MSG1_GatepassRequestMessage;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM130RequestMessagePrettierTest : ILEDIMessagePrettierTest<GpNg1030Msg1GatepassRequestMessage, ILGPM130RequestMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030_Interpretation.html")).Replace("\r\n", "");

		protected override ZString GetMessageText() => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030_WithData.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILGPM130RequestMessage>();

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("EN", "English");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, "C00042");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILTransportMethod, "31", "Truck", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			factory.Save();
		}
	}
}
