using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRInvoiceHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateComplementaryDescriptionExportToAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var complementaryDescriptionExport = "Complementary Description Export";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.ComplementaryDescription = complementaryDescriptionExport;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			var writer = new BRInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new BRDataObjectWriterHelper(Factory.BOFactory));
			var testItem = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.First(x => x.HarmonisedCode.ToString() == "1");
			var testItem2 = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.First(x => x.HarmonisedCode.ToString() == "2");
			AssertEquals(complementaryDescriptionExport, testItem.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.ComplementaryDescriptionExport).Value);
			AssertNull(testItem2.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.ComplementaryDescriptionExport));
		}

		public void TestPopulateJobComInvLineRefsCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "111";
			invoiceLine.LPCOJobComInvLineRefsCollection.AddNew().JG_ReferenceNumber = "222";

			var writer = new BRInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new BRDataObjectWriterHelper(Factory.BOFactory));
			var invoiceLineData = writer.GetDataObject(invoice).CommercialInvoiceLineCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals(2, invoiceLineData.CustomsReferenceCollection.Count);
				AssertEquals(Constants.JobComInvLineRefsType.Codes.LPCO, invoiceLineData.CustomsReferenceCollection[0].Type.Code);
				AssertEquals(Constants.JobComInvLineRefsType.Descriptions.LPCO, invoiceLineData.CustomsReferenceCollection[0].Type.Description);
				AssertEquals("111", invoiceLineData.CustomsReferenceCollection[0].Reference);
				AssertEquals(Constants.JobComInvLineRefsType.Codes.LPCO, invoiceLineData.CustomsReferenceCollection[1].Type.Code);
				AssertEquals(Constants.JobComInvLineRefsType.Descriptions.LPCO, invoiceLineData.CustomsReferenceCollection[1].Type.Description);
				AssertEquals("222", invoiceLineData.CustomsReferenceCollection[1].Reference);
			});
		}

		IDisposable setupCreator;

		public void TestCreateAdditionalLineTariffDetailDataObjectWriter()
		{
			var writer = new BRInvoiceHeaderDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new BRDataObjectWriterHelper(Factory.BOFactory));
			AssertType<BRAdditionalLineTariffDetailDataObjectWriter>(writer.GetAdditionalLineTariffDetailDataObjectWriter());
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		class BRInvoiceHeaderDataObjectWriterForTesting : BRInvoiceHeaderDataObjectWriter
		{
			public BRInvoiceHeaderDataObjectWriterForTesting(IDataWritingManager manager, Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null) : base(manager, helper, landedCostDataWriter, relatedEntry)
			{
			}

			public Customs.DataTransfer.Universal.AdditionalLineTariffDetailDataObjectWriter GetAdditionalLineTariffDetailDataObjectWriter()
			{
				return base.CreateAdditionalLineTariffDetailDataObjectWriter();
			}
		}
	}
}
