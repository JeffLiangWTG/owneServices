using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonSendMessageWrapperTest : WrapperHelperTest<T2LPOUSCommonSendMessageWrapper>
	{
		public void TestSendEmailL()
		{
			AssertEquals("SendEmailL is implemented in each child class", "S", wrapper.SendEmailL);
		}

		public void TestSendEmailU()
		{
			AssertEquals("SendEmailU is implemented in each child class", ZString.Empty, wrapper.SendEmailU);
		}

		public void TestSendEmailExp()
		{
			AssertEquals("SendEmailExp is implemented in each child class", ZString.Empty, wrapper.SendEmailExp);
		}

		public void TestNullPersonReqPres()
		{
			CombineAssertions(() =>
			{
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertExceptionThrown<NullReferenceException>(() => wrapper.PersonReqPres.ToString());

				declaration.Declarant.OA_OH = ZGuid.Empty;
				AssertExceptionThrown<NullReferenceException>(() => wrapper.PersonReqPres.ToString());
			});
		}

		public void TestPersonReqPres()
		{
			CombineAssertions(() =>
			{
				AssertEquals("PersonReqPres is implemented in each child class", null, wrapper.PersonReqPres);
			});
		}

		public void TestCustomsOffice()
		{
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected filled CustomsOffice", "ES009999", wrapper.CustomsOffice);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		T2LPOUSCommonSendMessageWrapper wrapper;

		T2LPOUSCommonSendMessageWrapper GetWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) => new T2LPOUSCommonSendMessageWrapperForTest(entryHeader, certificate);

		protected override T2LPOUSCommonSendMessageWrapper GetProvider() => wrapper;

		class T2LPOUSCommonSendMessageWrapperForTest : T2LPOUSCommonSendMessageWrapper
		{
			public T2LPOUSCommonSendMessageWrapperForTest(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
			{
			}

			protected override OrgAddress OrgAddressForPersonReqPresCommon => null;
		}
	}
}
