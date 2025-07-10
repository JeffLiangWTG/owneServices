using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ClearanceHeaderWrapperTest : WrapperHelperTest<ClearanceHeaderWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null CusEntryHeader", () => new ClearanceHeaderWrapper(null));
				AssertExceptionThrown<ArgumentNullException>("Null Declaration", () => new ClearanceHeaderWrapper(Factory.New<CusEntryHeader>()));
			});
		}

		public void TestReceptionCustomsOffice()
		{
			declaration.JE_CustomsOffice = "ES009999";
			AssertEquals("Expected filled ReceptionCustomsOffice", "009999", wrapper.ReceptionCustomsOffice);
		}

		public void TestReceptionT2LReference()
		{
			entryHeader.MovementReferenceNumber = "TestMRN";
			AssertEquals("Expected filled ReceptionT2LReference", "TestMRN", wrapper.ReceptionT2LReference);
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeader.PK;
			var declarant = wrapper.Declarant;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestGoodsLocation()
		{
			CombineAssertions(() =>
			{
				declaration.JE_LocationOfGoods = "ES002801000001";
				AssertEquals("Expected filled GoodsLocation length > 10", "2801000001", wrapper.GoodsLocation);

				declaration.JE_LocationOfGoods = "2801000001";
				AssertEquals("Expected filled GoodsLocation length = 10", "2801000001", wrapper.GoodsLocation);

				declaration.JE_LocationOfGoods = "0311EURO";
				AssertEquals("Expected filled GoodsLocation length < 10", "0311EURO", wrapper.GoodsLocation);
			});
		}

		public void TestTotalLinesNum()
		{
			entryHeader.MergedLines.AddNew();
			entryHeader.MergedLines.AddNew();
			AssertEquals("Expected 3 TotalLinesNum", 3, wrapper.TotalLinesNum);
		}

		public void TestContainersIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false ContainersIndicator", false, wrapper.ContainersIndicator);

				var containerTag = "CONTAINER";
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerTag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
				AssertEquals("Expected true ContainersIndicator", true, wrapper.ContainersIndicator);
			});
		}

		public void TestCommunications()
		{
			CombineAssertions(() =>
			{
				var communications = wrapper.Communications;
				AssertNotNull("Expected not null", communications);
				AssertSame("Cached", wrapper.Communications, communications);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new ClearanceHeaderWrapper(entryHeader);
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		ClearanceHeaderWrapper wrapper;

		protected override ClearanceHeaderWrapper GetProvider() => wrapper;
	}
}
