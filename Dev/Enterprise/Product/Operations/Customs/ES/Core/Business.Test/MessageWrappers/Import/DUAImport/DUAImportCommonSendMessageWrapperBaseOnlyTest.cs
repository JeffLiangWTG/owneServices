using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DUAImportCommonSendMessageWrapper))]
	class DUAImportCommonSendMessageWrapperBaseOnlyTest : ImportCommonSendMessageWrapperAbstractTest<DUAImportCommonSendMessageWrapper>
	{
		public void TestIsCeutaOrMelilla()
		{
			CombineAssertions(() =>
			{
				declaration.ZG_PartialWriteoff = false;
				AssertEquals("It is not a Ceuta Or Melilla declaration", false, wrapper.IsCeutaOrMelilla);

				declaration.ZG_PartialWriteoff = true;
				AssertEquals("It is a Ceuta Or Melilla declaration", true, wrapper.IsCeutaOrMelilla);
			});
		}

		public void TestIsCanary()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var canaryIslandCode = "61";
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

			CombineAssertions(() =>
			{
				declaration.ZG_DestinationState = "02";
				AssertEquals("It is not a Canary Islands declaration", false, wrapper.IsCanary);

				declaration.ZG_DestinationState = canaryIslandCode;
				AssertEquals("It is a Canary Islands declaration", true, wrapper.IsCanary);
			});
		}

		protected override DUAImportCommonSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DUAImportCommonSendMessageWrapperForTest(cusEntryHeader, certificateData);
	}

	class DUAImportCommonSendMessageWrapperForTest : DUAImportCommonSendMessageWrapper
	{
		public DUAImportCommonSendMessageWrapperForTest(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}
	}
}
