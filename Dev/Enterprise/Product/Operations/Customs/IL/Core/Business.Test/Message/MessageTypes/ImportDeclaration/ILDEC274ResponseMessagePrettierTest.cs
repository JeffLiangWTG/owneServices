using CargoWise.Customs.IL.MessageDefinitions.DEC.IMP.DF_MSG10000_ImportDeclaration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDEC274ResponseMessagePrettierTest : ILEDIMessagePrettierTest<DfNg2754Msg10004ImportDeclarationResponse, ILDEC274ResponseMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationResponse_2754.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ImportDeclarationResponse_2754.xml"));

		protected override ILEDIMessage GetNewMessage() => Factory.New<ILDEC274ResponseMessage>();

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("EN", "English");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "99", "Draft Accepted, Waiting For Signed Submission", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}
	}
}
