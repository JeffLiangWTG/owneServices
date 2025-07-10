using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DVDCommonSendMessageWrapper))]
	public class DVDCommonSendMessageWrapperBaseOnlyTest : DVDCommonSendMessageWrapperAbstractTest<DVDCommonSendMessageWrapper>
	{
		public void TestMRN()
		{
			entryHeader.MovementReferenceNumber = "MRNNumber";
			AssertEquals("Expected filled MRN", "MRNNumber", wrapper.MRN);
		}
		protected override DVDCommonSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DVDCommonSendMessageWrapperForTest(cusEntryHeader, certificateData);

		class DVDCommonSendMessageWrapperForTest : DVDCommonSendMessageWrapper
		{
			public DVDCommonSendMessageWrapperForTest(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
			{
			}
		}
	}
}
