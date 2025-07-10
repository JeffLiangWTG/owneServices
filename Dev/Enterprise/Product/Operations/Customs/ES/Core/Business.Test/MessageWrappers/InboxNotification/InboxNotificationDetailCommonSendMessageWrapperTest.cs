using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationDetailCommonSendMessageWrapperTest : WrapperHelperTest<InboxNotificationDetailCommonSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null bo", () => new InboxNotificationDetailCommonSendMessageWrapper(Factory, null, false, "AA", Certificate));
				AssertExceptionThrown<ArgumentException>("Empty key", () => new InboxNotificationDetailCommonSendMessageWrapper(Factory, esResponseDummyBusinessObject, false, ZString.Empty, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => new InboxNotificationDetailCommonSendMessageWrapper(Factory, esResponseDummyBusinessObject, false, "AA", null));
			});
		}

		public void TestKey()
		{
			AssertEquals("Expected filled Key", "keyCode", wrapper.Key);
		}

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", Certificate.CertificateID, wrapper.CertificateID);
		}

		public void TestIsTest()
		{
			CombineAssertions(() =>
			{
				wrapper = GetWrapper("keyCode", isTest: false);
				AssertEquals("Is declaration a test one?", false, wrapper.IsTest);

				wrapper = GetWrapper("keyCode", isTest: true);
				AssertEquals("It is a test declaration", true, wrapper.IsTest);
			});
		}

		public void TestBusinessObjectReference()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled BusinessObjectReference", "ref", wrapper.BusinessObjectReference);

				var entryHeader = Factory.New<CusEntryHeader>();
				entryHeader.CH_BGMReference = "Reference";
				wrapper = new InboxNotificationDetailCommonSendMessageWrapper(Factory, entryHeader, false, "keyCode", Certificate);
				AssertEquals("Expected filled BusinessObjectReference from entryHeader", "Reference", wrapper.BusinessObjectReference);
			});
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same messagecollection as the one in the bo given as argument", esResponseDummyBusinessObject.MessageCollection, wrapper.Messages);

				var entryHeader = Factory.New<CusEntryHeader>();
				entryHeader.Messages.AddNew();
				entryHeader.Messages.AddNew();
				wrapper = new InboxNotificationDetailCommonSendMessageWrapper(Factory, entryHeader, false, "keyCode", Certificate);
				AssertEquals("Expected two messages from entryHeader", 2, wrapper.Messages.Count);
				AssertSame("Expected same messagecollection as the one in the bo given as argument from entryHeader", entryHeader.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.Factory);
				AssertSame("Expected same factory as the argument given", Factory, wrapper.Factory);
			});
		}

		public void TestBrokerCode()
		{
			AssertEquals("Expected filled BrokerCode", Certificate.BrokerCode, wrapper.BrokerCode);
		}

		public void TestCertificateName()
		{
			AssertEquals("Expected filled CertificateName", Certificate.CertificateName, wrapper.CertificateName);
		}

		public void TestCertificateThumbPrint()
		{
			AssertEquals("Expected filled CertificateThumbPrint", Certificate.CertificateThumbPrint, wrapper.CertificateThumbPrint);
		}

		public void TestCertificateBytes()
		{
			AssertEquals("Expected filled CertificateBytes", Certificate.CertificateBytes, wrapper.CertificateBytes);
		}

		public void TestDecryptedCertificatePassphrase()
		{
			AssertEquals("Expected filled DecryptedCertificatePassphrase", Certificate.DecryptedCertificatePassphrase, wrapper.DecryptedCertificatePassphrase);
		}

		public void TestCertificatePK()
		{
			AssertEquals("Expected filled CertificatePK", Certificate.CertificatePK, wrapper.CertificatePK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var ediMessageCollection = new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory);
			ediMessageCollection.Add(Factory.NewWithValidTestData<ESEDIMessage>());
			esResponseDummyBusinessObject = new ESResponseDummyBusinessObject("ref", ediMessageCollection);

			wrapper = GetWrapper("keyCode");
		}
		ESResponseDummyBusinessObject esResponseDummyBusinessObject;
		InboxNotificationDetailCommonSendMessageWrapper wrapper;

		InboxNotificationDetailCommonSendMessageWrapper GetWrapper(string key, bool isTest = false) => new InboxNotificationDetailCommonSendMessageWrapper(Factory, esResponseDummyBusinessObject, isTest, key, Certificate);

		protected override InboxNotificationDetailCommonSendMessageWrapper GetProvider() => wrapper;

		class ESResponseDummyBusinessObject : IESResponseBusinessObject
		{
			public ESResponseDummyBusinessObject(ZString reference, EDIMessageCollection messages)
			{
				this.reference = reference;
				this.messages = messages;
			}
			readonly ZString reference;
			readonly EDIMessageCollection messages;

			public ZGuid BranchPK => ZGuid.Empty;

			public EDIMessageCollection MessageCollection => messages;

			public ZString EntryReference => reference;
		}
	}
}
