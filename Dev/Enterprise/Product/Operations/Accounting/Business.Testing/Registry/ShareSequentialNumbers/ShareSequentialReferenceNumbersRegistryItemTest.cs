using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ShareSequentialReferenceNumbersRegistryItem))]
	class ShareSequentialReferenceNumbersRegistryItemTest : StronglyTypedRegistryItemTestCase<IShareSequentialReferenceNumbers, ShareSequentialReferenceNumbers>
	{
		protected override StronglyTypedRegistryItem<IShareSequentialReferenceNumbers, ShareSequentialReferenceNumbers> GetNewRegistryItem()
		{
			return new ShareSequentialReferenceNumbersRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		public void TestDeleteValueCore()
		{
			var factory = new BusinessObjectFactory();
			var apInvoiceInternalRefFountain = Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain();
			var apCreditNoteInternalRefFountain = Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain();
			var apAdjustmentNoteInternalRefFountain = Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain();

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			var item = GetNewRegistryItem();

			((IRegistryItemInternals)item).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			((IRegistryItemInternals)item).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("Delete a registry value same as default value. Should NOT trigger sychnoization", 1501, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1401, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			var refNumber = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, refNumber);

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			((IRegistryItemInternals)item).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("Delete a registry value different from default value. Should trigger sychnoization", 1601, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apAdjustmentNoteInternalRefFountain.GetNext(factory));
		}

		public void TestSetValueCore()
		{
			var factory = new BusinessObjectFactory();
			var apInvoiceInternalRefFountain = Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain();
			var apCreditNoteInternalRefFountain = Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain();
			var apAdjustmentNoteInternalRefFountain = Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain();

			var oldItem = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);
			var item = GetNewRegistryItem();

			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldItem);

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			var newItem = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(false);
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'YES' to 'NO' should trigger sychnoization", 1601, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			newItem.Value = true;
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'NO' to 'YES' should trigger sychnoization", 1601, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'YES' to 'YES' should NOT trigger sychnoization", 1501, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals(1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals(1401, apAdjustmentNoteInternalRefFountain.GetNext(factory));
		}

		public void TestSynchronizeNumberFountains_UpdatesNumberFountainOnlyForCurrentCompanyWhenSetValue()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var apInvoiceInternalRefFountain = Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain();
			var apCreditNoteInternalRefFountain = Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain();
			var apAdjustmentNoteInternalRefFountain = Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain();

			var registryItem = GetNewRegistryItem();

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);

			var item = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);
			registryItem.SetValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, item);

			AssertEquals("APInvoiceInternalRef for non-current company", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals("APCreditNoteInternalRef for non-current company", 1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals("APAdjustmentNoteInternalRef for non-current company", 1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);

			AssertEquals("APInvoiceInternalRef for current company", 1601, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals("APCreditNoteInternalRef for current company", 1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals("APAdjustmentNoteInternalRef for current company", 1601, apAdjustmentNoteInternalRefFountain.GetNext(factory));
		}

		public void TestSynchronizeNumberFountains_UpdatesNumberFountainOnlyForCurrentCompanyWhenDeleteValue()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var apInvoiceInternalRefFountain = Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain();
			var apCreditNoteInternalRefFountain = Env.NumberFountains.APCreditNoteInternalRef.GetTodaysPeriodFountain();
			var apAdjustmentNoteInternalRefFountain = Env.NumberFountains.APAdjustmentNoteInternalRef.GetTodaysPeriodFountain();

			var item = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(true);

			var registryItem = GetNewRegistryItem();
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			registryItem.SetValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, item);

			apInvoiceInternalRefFountain.SetNext(factory, 1500);
			apCreditNoteInternalRefFountain.SetNext(factory, 1600);
			apAdjustmentNoteInternalRefFountain.SetNext(factory, 1400);

			((IRegistryItemInternals)registryItem).DeleteValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals("APInvoiceInternalRef for non-current company", 1500, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals("APCreditNoteInternalRef for non-current company", 1600, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals("APAdjustmentNoteInternalRef for non-current company", 1400, apAdjustmentNoteInternalRefFountain.GetNext(factory));

			((IRegistryItemInternals)registryItem).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("APInvoiceInternalRef for current company", 1601, apInvoiceInternalRefFountain.GetNext(factory));
			AssertEquals("APCreditNoteInternalRef for current company", 1601, apCreditNoteInternalRefFountain.GetNext(factory));
			AssertEquals("APAdjustmentNoteInternalRef for current company", 1601, apAdjustmentNoteInternalRefFountain.GetNext(factory));
		}
	}
}
