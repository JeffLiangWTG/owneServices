using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	[TestedType(typeof(JobDeclarationToAsycudaManifestShipmentWriter))]
	sealed class JobDeclarationToAsycudaManifestShipmentWriterBaseOnlyTest : JobDeclarationToAsycudaManifestShipmentWriterAbstractTest
	{
		public void TestMappingJI_LineNo()
		{
			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");

			var declaration = Factory.New<BaseJobDeclaration>();
			AddDeclarationData(declaration, supplier, importer, notify, false);
			declaration.SetUserDefinedValue("CustomField1", (ZString)"Hello World");
			var invoiceHeader = CreateInvoiceHeader(declaration, 1);
			var invoiceLine = CreateInvoiceLine(invoiceHeader.JobComInvoiceLines.AddNew(), 1, 1000);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AddEntryDetails(declaration);

			Factory.Save();

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration).SubShipmentCollection[0];
			var packingLine = shipment.PackingLineCollection[0];

			CombineAssertions(() =>
			{
				AssertEquals("packingLine.ItemNo", invoiceLine.JI_LineNo, packingLine.ItemNo);
			});
		}

		public void TestMappingLargeJI_CustomsQuantity()
		{
			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");

			var declaration = Factory.New<BaseJobDeclaration>();
			AddDeclarationData(declaration, supplier, importer, notify, false);
			declaration.SetUserDefinedValue("CustomField1", (ZString)"Hello World");
			var invoiceHeader = CreateInvoiceHeader(declaration, 1);
			var invoiceLine = CreateInvoiceLine(invoiceHeader.JobComInvoiceLines.AddNew(), 1, 1000);
			invoiceLine.JI_CustomsQuantity = 9999999999999.999999m; // Field is Decimal (19,6)
			invoiceLine.JI_CustomsUnitQty = "T2";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AddEntryDetails(declaration);

			Factory.Save();

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration).SubShipmentCollection[0];
			var packingLine = shipment.PackingLineCollection[0];
			var subPackingLine = packingLine.PackingLineCollection[0];
			var addInfoCollection = subPackingLine.AddInfoGroupCollection.First(x => x.Type.GetCodeAsUpperCase() == AddInfoConstants.PackedItem.PackingItemAddInfoType).AddInfoCollection;

			CombineAssertions(() =>
			{
				AssertEquals("addInfoCollection.CustomsUQ", "T2", addInfoCollection.GetZStringValue(AddInfoConstants.PackedItem.CustomsUQ));
				AssertEquals("addInfoCollection.CustomsQty", 9999999999999.999999m, addInfoCollection.GetZDecimalValue(AddInfoConstants.PackedItem.CustomsQty));
			});
		}

		public void TestJI_InvoiceQuantityFieldIsSmallerThanLong()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = CreateInvoiceHeader(declaration, 1);
			var invoiceLine = CreateInvoiceLine(invoiceHeader.JobComInvoiceLines.AddNew(), 1, 1000);
			invoiceLine.JI_InvoiceQuantity = new ZDecimal(long.MaxValue);
			AssertExceptionThrown<CargoWise.EntityFramework.ZSaveException>("Value is to big to save", () => Factory.Save());
		}

		public void TestMappingLargeJI_InvoiceQuantity()
		{
			var importer = CreateOrg("IMPORG");
			var supplier = CreateOrg("SUPORG");
			var notify = CreateOrg("NTFORG");

			var declaration = Factory.New<BaseJobDeclaration>();
			AddDeclarationData(declaration, supplier, importer, notify, false);
			declaration.SetUserDefinedValue("CustomField1", (ZString)"Hello World");

			var invoiceHeader = CreateInvoiceHeader(declaration, 1);
			var invoiceLine = CreateInvoiceLine(invoiceHeader.JobComInvoiceLines.AddNew(), 1, 1000);
			invoiceLine.JI_InvoiceQuantity = 9999999999999.999999m; // Field is Decimal (19,6)
			invoiceLine.JI_InvoiceUQ = "T1";
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AddEntryDetails(declaration);
			Factory.Save();

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
			var writer = CreateWriter(writingManager);
			var shipment = writer.GetDataObject(declaration).SubShipmentCollection[0];
			var packingLine = shipment.PackingLineCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("packingLine.PackQty", 9999999999999L, packingLine.PackQty);
				AssertEquals("packingLine.PackType.Code", "T1", packingLine.PackType.Code);
				AssertEquals("packingLine.packingLine.PackingLineCollection.Count", 1, packingLine.PackingLineCollection.Count);
				var subPackingLine = packingLine.PackingLineCollection[0];
				AssertNull("subPackingLine.PackQty", subPackingLine.PackQty);
				AssertNull("subPackingLine.PackType", subPackingLine.PackType);
			});
		}

		public void TestTaxAmountShouldBeRoundTo2DecimalsOnEachLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var importer = CreateOrg("IMPORG");
				var supplier = CreateOrg("SUPORG");
				var notify = CreateOrg("NTFORG");

				var declaration = Factory.New<BaseJobDeclaration>();
				AddDeclarationData(declaration, supplier, importer, notify, false);

				var invoiceHeader1 = CreateInvoiceHeader(declaration, 1);

				CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 1, 1000);
				CreateInvoiceLine(invoiceHeader1.JobComInvoiceLines.AddNew(), 2, 2000);
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				invoiceHeader1.InvoiceLines[0].CusEntryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 100.0070m);
				invoiceHeader1.InvoiceLines[1].CusEntryLine.Fees.SetAmount(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 100.0070m);

				AddEntryDetails(declaration);

				Factory.Save();

				IDataWritingManager writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, declaration));
				var writer = CreateWriter(writingManager);
				var shipment = writer.GetDataObject(declaration);
				var taxAddInfo = shipment.SubShipmentCollection[0].EntryInstructionCollection[0].AddInfoCollection.Single(x => x.Key == (ZString?)AddInfoConstants.Bill.TaxAmount);
				AssertEquals("The tax amount should be firstly rounded to 2 decimals on each line, then summed up. (100.0070=>100.01 + 100.0070=>100.01) = 200.02", "200.02", taxAddInfo.Value);
			}
		}
	}
}
