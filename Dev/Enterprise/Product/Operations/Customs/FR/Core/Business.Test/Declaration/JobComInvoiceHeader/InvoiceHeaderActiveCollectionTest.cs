using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class InvoiceHeaderCollectionTest : EU.Business.Declaration.Testing.InvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			return invoice;
		}

		public override void TestRemovingInvoiceLeadsToReapportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var testDec = Factory.New<JobDeclaration>();
				var invoice1 = testDec.Invoices.AddNew();
				invoice1.JZ_InvoiceAmount = 10000m;
				invoice1.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice1.JZ_IncoTerm = "FOB";

				var invoice2 = testDec.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 10000m;
				invoice2.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice2.JZ_IncoTerm = "FOB";

				var cOM = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.Commission, 1000m, testDec.LocalCurrencyCode);
				testDec.ResumeApportionment();
				AssertEquals("Apportioned for Invoice1", 500m, invoice1.GroupCharges.GetCharge(cOM.ChargeKey).Amount);
				AssertEquals("Apportioned for invoice2", 500m, invoice2.GroupCharges.GetCharge(cOM.ChargeKey).Amount);

				testDec.Invoices.Delete(invoice1);
				testDec.ResumeApportionment();
				AssertEquals("Apportioned for invoice2", 1000m, invoice2.GroupCharges.GetCharge(cOM.ChargeKey).Amount);
			}
		}

		public void TestInvoiceHeaderAddedToCollection_ShouldUpdateGroupCharges()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = "SEA";

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_IncoTerm = "CIP";
			invoice.ZG_AgreedPlaceCode = "1";

			AssertEquals("Prerequisite: There should be 0 invoice in the declaration.", 0, testDec.Invoices.Count);
			AssertEquals("Prerequisite: There should be 0 group charges created.", 0, testDec.TopGroupInvoice.Charges.Count);

			testDec.Invoices.Add(invoice);
			AssertEquals("There should be 1 invoice in the declaration", 1, testDec.Invoices.Count);
			AssertEquals("Group charges should be updated when adding a new invoice.", 6, testDec.TopGroupInvoice.Charges.Count);
		}

		public void TestInvoiceHeaderAddedToCollectionButUncommitted_ShouldNotUpdateGroupCharges()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_TransportMode = "SEA";
			testDec.ZG_AgreedPlaceCode = "3";
			testDec.JE_ShipmentIncoTerm = "FOB";

			AssertEquals("Prerequisite: There should be 0 invoice in the declaration.", 0, testDec.Invoices.Count);
			AssertEquals("Prerequisite: There should be 0 group charges created.", 0, testDec.TopGroupInvoice.Charges.Count);

			var invoice = ((IBindingList)testDec.Invoices).AddNew();
			AssertEquals("There should be 1 invoice in the declaration.", 1, testDec.Invoices.Count);
			AssertEquals("Group charges should not be updated when adding a new invoice but not committed.", 0, testDec.TopGroupInvoice.Charges.Count);
		}
	}
}
