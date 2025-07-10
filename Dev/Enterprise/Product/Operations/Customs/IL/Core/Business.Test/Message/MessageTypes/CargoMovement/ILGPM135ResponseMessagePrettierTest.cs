using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM135ResponseMessagePrettierTest : ILEDIMessagePrettierTest<GpNg1035Msg2GatepassFeedbackMessage, ILGPM135ResponseMessageDataObject>
	{
		protected override ILEDIMessage GetNewMessage() => Factory.New<ILGPM135ResponseMessage>();

		public ZString GetExpectedMessageInterpretation_Exposed() => GetExpectedMessageInterpretation();

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage_WithException_Interpretation.html")).Replace("\r\n", "");

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassFeedbackMessage_WithException.xml"));

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			GlbStaff.CurrentUser.GS_WorkingLanguage = "HE-IL";

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("EN", "English");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("C1557", "IL GatePass Movement Status", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC1557_2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C1557", "2", "תקין - התקבלה בקשה תקינה", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC1557_2, "EN", "Correct");

			helper.CreateNewOrGetExistingCusCodeType("C1589", "IL Gate Pass Returned Code", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC1589_1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C1589", "1", "עדיין אין תשובה מאתרי מקור/יעד", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC1589_1, "EN", "Waiting For Source Or Target Site Approval");

			factory.Save();
		}
	}
}
