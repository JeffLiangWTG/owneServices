using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILMAN171ResponseMessage))]
	sealed class ILMAN171ResponseMessageTest : ILEDIResponseMessageTestBase<ILMAN171ResponseMessage>
	{
		protected override string GetExpectedMessageType() => "MAN";

		protected override string GetExpectedMessageSubType() => "171";

		protected override ZString GetExpectedMessageSubTypeDescription() => "Forwarder Manifest Response";

		protected override Type GetExpectedTypeOfMessageDataObject() => typeof(ILMAN171ResponseMessageDataObject);

		protected override ZString GetExpectedMessageInterpretation() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_WithErrors_Interpretation.html"));

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171_WithErrors.xml"));

		protected override void SetupReferenceDataForInterpretation()
		{
			var factory = Factory;

			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("CMAN", "IL Manifest Status", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "CMAN", "1", "Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "CMAN", "9", "Not Accepted", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}
	}
}
