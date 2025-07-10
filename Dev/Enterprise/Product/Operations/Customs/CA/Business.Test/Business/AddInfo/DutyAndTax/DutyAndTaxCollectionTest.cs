using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DutyAndTaxCollection))]
	sealed class DutyAndTaxCollectionTest : ActiveBusinessObjectCollectionTestCase<DutyAndTaxCollection>
	{
		public void TestHuntHasChangesSinceLastMarkOnDelete()
		{
			var coll = invoiceLine.DutiesAndTaxes;
			HasChangesHunter hunter = new HasChangesHunter(coll);
			Assert("no changes", !hunter.HasChangesSinceLastMark);

			var child = coll.AddNew();
			child.C1_Override = true;
			Assert("has changes", hunter.HasChangesSinceLastMark);
			Factory.Save();

			Assert("no changes", !hunter.HasChangesSinceLastMark);

			coll.Delete(child);
			Assert("has changes", hunter.HasChangesSinceLastMark);
		}

		public void TestAddNewWithTaxType()
		{
			var coll = GetCollectionToTest();
			var gst = coll.AddNew(DutyAndTaxTypes.Codes.GST);
			AssertEquals(DutyAndTaxTypes.Codes.GST, gst.C1_TaxType);
		}

		public void TestElementValueChanged()
		{
			var countElementOverrideValueChanged = 0;
			var countElementTaxTypeValueChanged = 0;
			invoiceLine.DutiesAndTaxes.ElementOverrideValueChanged += x => countElementOverrideValueChanged++;
			invoiceLine.DutiesAndTaxes.ElementTaxTypeValueChanged += x => countElementTaxTypeValueChanged++;

			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_Override = true;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;

			var tax1 = Factory.New<DutyAndTax>();
			tax1.B7_ParentID = invoiceLine.PK;
			tax1.B7_ParentTableCode = invoiceLine.TablePrefix;
			tax1.C1_Override = true;
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;

			AssertEquals("ElementOverrideValueChanged", 2, countElementOverrideValueChanged);
			AssertEquals("ElementTaxTypeValueChanged", 2, countElementTaxTypeValueChanged);
		}

		public void TestHasChangesChanged()
		{
			var countHasChangesChanged = 0;
			EventHandler dutiesAndTaxesOnHasChangesChanged = (x, y) => countHasChangesChanged++;
			invoiceLine.DutiesAndTaxes.HasChangesChanged += dutiesAndTaxesOnHasChangesChanged;

			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			invoiceLine.Declaration.ApportionmentDirty = false;

			AssertEquals("HasChanges", true, tax.HasChanges);
			AssertEquals("HasChangesChanged", 0, countHasChangesChanged);
			Factory.Save();

			AssertEquals("HasChanges", false, tax.HasChanges);
			AssertEquals("HasChangesChanged is occurred only if HasChanges = true", 0, countHasChangesChanged);

			tax.C1_Override = true;
			AssertEquals("HasChangesChanged", 1, countHasChangesChanged);

			tax.C1_Amount = 10;
			AssertEquals("HasChangesChanged already occurred", 1, countHasChangesChanged);

			invoiceLine.DutiesAndTaxes.HasChangesChanged -= dutiesAndTaxesOnHasChangesChanged;
			invoiceLine.DutiesAndTaxes.HasChangesChanged += dutiesAndTaxesOnHasChangesChanged;
			tax.C1_Amount = 20;
			invoiceLine.Declaration.ApportionmentDirty = false;
			AssertEquals("HasChangesChanged should be occurred if hooked again", 2, countHasChangesChanged);

			tax.HasChanges = false;
			AssertEquals("HasChangesChanged is occurred only if HasChanges = true", 2, countHasChangesChanged);
		}

		protected override DutyAndTaxCollection GetCollectionToTest()
		{
			return new DutyAndTaxCollection(Factory.New<JobComInvoiceLine>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		JobComInvoiceLine invoiceLine;
	}
}
