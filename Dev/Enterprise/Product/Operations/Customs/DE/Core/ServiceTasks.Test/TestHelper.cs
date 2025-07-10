using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using static Enterprise.Messaging.Business.EDIMessage;
using EDIMessageTypeList = Enterprise.Customs.DE.Messaging.EDIMessageTypeList;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	public abstract class TestHelper : CommonServiceTestHelper
	{
		public static void AssertMessageFilter(BusinessObjectFactory factory
			, OutgoingMessageProcessor outgoingMessageProcessor
			, Func<OutgoingMessageProcessor, ZQuery> messageFilter
			, bool atlasMatchesFilter = false
			, bool aesMatchesFilter = false
			, bool emcsMatchesFilter = false)
		{
			var atlasRequest = factory.New<EDIMessage>();
			atlasRequest.EM_ReceiveTransmit = Direction.Transmit;
			atlasRequest.EM_ApplicationCode = ApplicationCodes.DECustomsAtlasSystem;
			atlasRequest.EM_MessageType = EDIMessageTypeList.Codes.Import;

			var aesRequest = factory.New<EDIMessage>();
			aesRequest.EM_ReceiveTransmit = Direction.Transmit;
			aesRequest.EM_ApplicationCode = ApplicationCodes.DECustomsAesSystem;
			aesRequest.EM_MessageType = EDIMessageTypeList.Codes.AES;

			var emcsRequest = factory.New<EDIMessage>();
			emcsRequest.EM_ReceiveTransmit = Direction.Transmit;
			emcsRequest.EM_ApplicationCode = ApplicationCodes.DECustomsEmcsSystem;
			emcsRequest.EM_MessageType = EDIMessageTypeList.Codes.EMCS;

			NUnit.Framework.AssertionWithHtml.CombineAssertions(() =>
			{
				NUnit.Framework.Assertion.AssertEquals("ATLAS Message Filter", atlasMatchesFilter, atlasRequest.MatchesFilter(messageFilter.Invoke(outgoingMessageProcessor)));
				NUnit.Framework.Assertion.AssertEquals("AES Message Filter", aesMatchesFilter, aesRequest.MatchesFilter(messageFilter.Invoke(outgoingMessageProcessor)));
				NUnit.Framework.Assertion.AssertEquals("EMCS Message Filter", emcsMatchesFilter, emcsRequest.MatchesFilter(messageFilter.Invoke(outgoingMessageProcessor)));
			});
		}

		internal class CompaniesAndBranchesTestData
		{
			internal CompaniesAndBranchesTestData(BusinessObjectFactory factory)
			{
				DECompany1 = factory.NewWithValidTestData<GlbCompany>();
				DECompany1.GC_Code = "DDE";
				DECompany1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Germany;

				BERBranch = DECompany1.Branches.AddNew();
				BERBranch.FillWithValidTestData();
				BERBranch.GB_Code = "BER";
				BERBranch.GB_RL_NKHomePort = "DEBER";

				SITBranch = DECompany1.Branches.AddNew();
				SITBranch.FillWithValidTestData();
				SITBranch.GB_Code = "SIT";
				SITBranch.GB_RL_NKHomePort = "DESIT";

				DECompany2 = factory.NewWithValidTestData<GlbCompany>();
				DECompany2.GC_Code = "DDF";
				DECompany2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Germany;

				BYBBranch = DECompany2.Branches.AddNew();
				BYBBranch.FillWithValidTestData();
				BYBBranch.GB_Code = "BYB";
				BYBBranch.GB_RL_NKHomePort = "DEBYB";
			}

			internal GlbCompany DECompany1 { get; }
			internal GlbCompany DECompany2 { get; }
			internal GlbBranch BERBranch { get; }
			internal GlbBranch SITBranch { get; }
			internal GlbBranch BYBBranch { get; }
		}
	}
}
