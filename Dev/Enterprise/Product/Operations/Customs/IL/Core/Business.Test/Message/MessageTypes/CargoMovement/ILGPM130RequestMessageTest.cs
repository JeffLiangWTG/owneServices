using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILGPM130RequestMessage))]
	sealed class ILGPM130RequestMessageTest : ILEDIMessageWithPlaceHolderTestBase<ILGPM130RequestMessage>
	{
		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030_Interpretation.html")).Replace("\r\n", "");

		protected override string GetExpectedMessageSubType() => "130";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Gatepass Movement Request";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030_WithData.xml"));

		protected override string GetExpectedMessageType() => "GPM";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILGPM130RequestMessageDataObject);

		protected override void SetupReferenceDataForInterpretation()
		{
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

			helper.CreateNewOrGetExistingCusCodeType("C0042", "IL Transport Method", Core.Constants.CountryCodes.Israel);
			var refCusCodeListC0042_1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C0042", "31", "Truck", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListLanguage(refCusCodeListC0042_1, "EN", "Truck");

			factory.Save();
		}

		protected override string GetExampleRequestMessage() => "GatepassRequestMessage1030.xml";

		protected override string GetReferenceTag() => "gatepassNumber";

		protected override IILElectronicMessageProvider GetNewEmptyElectronicMessageProvider()
			=> Factory.New<ForwardingShipment>().GatePassMovementProvider;

		protected override string GetRequestMessageSubType() => ILEDIMessageSubTypeList.Codes.GatepassMovementRequest;
	}
}
