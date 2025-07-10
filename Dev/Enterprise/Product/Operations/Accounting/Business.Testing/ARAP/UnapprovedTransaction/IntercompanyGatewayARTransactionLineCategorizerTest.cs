using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class IntercompanyGatewayARTransactionLineCategorizerTest : TestCaseWithFactory
	{
		public void TestEmptyListOfLines()
		{
			var categories = IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(new List<InvoiceLineAssociatedWithGatewayJob>());
			AssertEquals(0, categories.Count);
		}

		public void TestTargetJobIsShipment()
		{
			var line1 = CreateInvoiceLineAssociatedWithGatewayJob(new ZGuid("fedfe121-d90d-4aff-9eac-d0b79051f429"), "S79878", JobShipmentSchema.Constants.Prefix);
			var line2 = CreateInvoiceLineAssociatedWithGatewayJob(new ZGuid("08e95e31-98a4-41ad-800d-89ace5ae2341"), "S45454", JobShipmentSchema.Constants.Prefix);
			var invoiceLinesAsscoiatedWithGateway = new List<InvoiceLineAssociatedWithGatewayJob>();
			invoiceLinesAsscoiatedWithGateway.Add(line1);
			invoiceLinesAsscoiatedWithGateway.Add(line2);

			var categories = IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(invoiceLinesAsscoiatedWithGateway);
			Assert(categories.ContainsKey(TargetJobTypes.Shipment));
			categories.TryGetValue(TargetJobTypes.Shipment, out List<InvoiceLineAssociatedWithGatewayJob> shipmentLines);
			Assert(shipmentLines.Contains(line1));
			Assert(shipmentLines.Contains(line2));
			Assert(!categories.ContainsKey(TargetJobTypes.GTWConsol));
			Assert(!categories.ContainsKey(TargetJobTypes.NonGTWConsol));
		}

		public void TestTargetJobIsGatewayConsol()
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol("USLAX", "AUMEL", "C001", sendingGatewayCompany: GlbCompany.CurrentCompany);
			Factory.Save();

			var line1 = CreateInvoiceLineAssociatedWithGatewayJob(gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			var line2 = CreateInvoiceLineAssociatedWithGatewayJob(gatewayConsol.PK, gatewayConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			var invoiceLinesAsscoiatedWithGateway = new List<InvoiceLineAssociatedWithGatewayJob>();
			invoiceLinesAsscoiatedWithGateway.Add(line1);
			invoiceLinesAsscoiatedWithGateway.Add(line2);

			var categories = IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(invoiceLinesAsscoiatedWithGateway);
			Assert(!categories.ContainsKey(TargetJobTypes.Shipment));
			Assert(categories.ContainsKey(TargetJobTypes.GTWConsol));
			categories.TryGetValue(TargetJobTypes.GTWConsol, out List<InvoiceLineAssociatedWithGatewayJob> gatewayLines);
			Assert(gatewayLines.Contains(line1));
			Assert(gatewayLines.Contains(line2));
			Assert(!categories.ContainsKey(TargetJobTypes.NonGTWConsol));
		}

		public void TestTargetJobIsNonGatewayConsol()
		{
			var forwardingConsol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var line1 = CreateInvoiceLineAssociatedWithGatewayJob(forwardingConsol.PK, forwardingConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			var line2 = CreateInvoiceLineAssociatedWithGatewayJob(forwardingConsol.PK, forwardingConsol.JK_UniqueConsignRef, JobConsolSchema.Constants.Prefix);
			var invoiceLinesAsscoiatedWithGateway = new List<InvoiceLineAssociatedWithGatewayJob>();
			invoiceLinesAsscoiatedWithGateway.Add(line1);
			invoiceLinesAsscoiatedWithGateway.Add(line2);

			var categories = IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(invoiceLinesAsscoiatedWithGateway);
			Assert(!categories.ContainsKey(TargetJobTypes.Shipment));
			Assert(!categories.ContainsKey(TargetJobTypes.GTWConsol));
			Assert(categories.ContainsKey(TargetJobTypes.NonGTWConsol));
			categories.TryGetValue(TargetJobTypes.NonGTWConsol, out List<InvoiceLineAssociatedWithGatewayJob> nonGatewayLines);
			Assert(nonGatewayLines.Contains(line1));
			Assert(nonGatewayLines.Contains(line2));
		}

		public void TestTargetJobIsUnknown()
		{
			var line1 = CreateInvoiceLineAssociatedWithGatewayJob(new ZGuid("fedfe121-d90d-4aff-9eac-d0b79051f429"), "S79878", AccBankAccountSchema.Constants.Prefix);
			var line2 = CreateInvoiceLineAssociatedWithGatewayJob(new ZGuid("08e95e31-98a4-41ad-800d-89ace5ae2341"), "S45454", AccBankAccountSchema.Constants.Prefix);
			var invoiceLinesAsscoiatedWithGateway = new List<InvoiceLineAssociatedWithGatewayJob>();
			invoiceLinesAsscoiatedWithGateway.Add(line1);
			invoiceLinesAsscoiatedWithGateway.Add(line2);

			ErrorReporter.Clear();
			IntercompanyGatewayARTransactionLineCategorizer.CategorizeGatewayARInvoiceLinesBasedOnTargetJobType(invoiceLinesAsscoiatedWithGateway);
			AssertEquals("IntercompanyGatewayARTransaction_UnknownTargetJobParentTableCode", ErrorReporter.LastKeyReported);
			AssertEquals("Unknown target parent table code 'AB' found in gateway AR invoice.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		InvoiceLineAssociatedWithGatewayJob CreateInvoiceLineAssociatedWithGatewayJob(ZGuid targetJobPk, ZString targetJobNumber, ZString targetJobParentTableCode)
		{
			var linePK = new ZGuid("14e85b75-82c5-4813-9cfb-b220a8e8559d");
			var lineRelatedJobPK = new ZGuid("c40b85ee-bd8a-4d09-89fd-08005059d21a");
			var lineRelatedJobNumber = "S9999";
			var lineTargetJobPk = targetJobPk;
			var lineTargetJobNumber = targetJobNumber;
			var lineTargetJobParentTableCode = targetJobParentTableCode;
			return new InvoiceLineAssociatedWithGatewayJob(linePK, lineRelatedJobPK, lineRelatedJobNumber, lineTargetJobPk, lineTargetJobNumber, lineTargetJobParentTableCode);
		}

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
