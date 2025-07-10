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
	[TestedType(typeof(ShareSequentialTransactionNumbersRegistryItem))]
	class ShareSequentialTransactionNumbersRegistryItemTest : StronglyTypedRegistryItemTestCase<IShareSequentialTransactionNumbers, ShareSequentialTransactionNumbers>
	{
		protected override StronglyTypedRegistryItem<IShareSequentialTransactionNumbers, ShareSequentialTransactionNumbers> GetNewRegistryItem()
		{
			return new ShareSequentialTransactionNumbersRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		public void TestDeleteValueCore()
		{
			var factory = new BusinessObjectFactory();

			var arInvoiceNoFountain = Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain();
			var arCreditNoteNoFountain = Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain();
			var arAdjustmentNoteNoFountain = Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain();

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1400, arAdjustmentNoteNoFountain.GetNext(factory));

			var item = GetNewRegistryItem();
			((IRegistryItemInternals)item).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("Delete a registry value same as default value. Should NOT trigger sychnoization", 1501, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1401, arAdjustmentNoteNoFountain.GetNext(factory));

			var refNumber = CreateShareSequentialTransactionNumbers(true);
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, refNumber);

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1400, arAdjustmentNoteNoFountain.GetNext(factory));

			((IRegistryItemInternals)item).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("Delete a registry value different from default value. Should trigger sychnoization", 1601, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1601, arAdjustmentNoteNoFountain.GetNext(factory));
		}

		public void TestSetValueCore()
		{
			var factory = new BusinessObjectFactory();

			var arInvoiceNoFountain = Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain();
			var arCreditNoteNoFountain = Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain();
			var arAdjustmentNoteNoFountain = Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain();

			var oldItem = CreateShareSequentialTransactionNumbers(true);
			var item = GetNewRegistryItem();
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldItem);

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1400, arAdjustmentNoteNoFountain.GetNext(factory));

			var newItem = CreateShareSequentialTransactionNumbers(false);
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'YES' to 'NO' should trigger sychnoization", 1601, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1601, arAdjustmentNoteNoFountain.GetNext(factory));

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1400, arAdjustmentNoteNoFountain.GetNext(factory));

			newItem.Value = true;
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'NO' to 'YES' should trigger sychnoization", 1601, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1601, arAdjustmentNoteNoFountain.GetNext(factory));

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);
			AssertEquals("Precondition", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1400, arAdjustmentNoteNoFountain.GetNext(factory));

			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newItem);

			AssertEquals("Change registry value from 'YES' to 'YES' should NOT trigger sychnoization", 1501, arInvoiceNoFountain.GetNext(factory));
			AssertEquals(1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals(1401, arAdjustmentNoteNoFountain.GetNext(factory));
		}

		public void TestSynchronizeNumberFountains_UpdatesNumberFountainOnlyForCurrentCompanyWhenSetValue()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var arInvoiceNoFountain = Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain();
			var arCreditNoteNoFountain = Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain();
			var arAdjustmentNoteNoFountain = Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain();

			var registryItem = GetNewRegistryItem();

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);

			var item = CreateShareSequentialTransactionNumbers(true);
			registryItem.SetValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, item);

			AssertEquals("ARInvoiceNo for non-current company", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals("ARCreditNoteNo for non-current company", 1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals("ARAdjustmentNoteNo for non-current company", 1400, arAdjustmentNoteNoFountain.GetNext(factory));

			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);

			AssertEquals("ARInvoiceNo for current company", 1601, arInvoiceNoFountain.GetNext(factory));
			AssertEquals("ARCreditNoteNo for current company", 1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals("ARAdjustmentNoteNo for current company", 1601, arAdjustmentNoteNoFountain.GetNext(factory));
		}

		public void TestSynchronizeNumberFountains_UpdatesNumberFountainOnlyForCurrentCompanyWhenDeleteValue()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var arInvoiceNoFountain = Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain();
			var arCreditNoteNoFountain = Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain();
			var arAdjustmentNoteNoFountain = Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain();

			var item = CreateShareSequentialTransactionNumbers(true);

			var registryItem = GetNewRegistryItem();
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			registryItem.SetValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, item);

			arInvoiceNoFountain.SetNext(factory, 1500);
			arCreditNoteNoFountain.SetNext(factory, 1600);
			arAdjustmentNoteNoFountain.SetNext(factory, 1400);

			((IRegistryItemInternals)registryItem).DeleteValue(testObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			AssertEquals("ARInvoiceNo for non-current company", 1500, arInvoiceNoFountain.GetNext(factory));
			AssertEquals("ARCreditNoteNo for non-current company", 1600, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals("ARAdjustmentNoteNo for non-current company", 1400, arAdjustmentNoteNoFountain.GetNext(factory));

			((IRegistryItemInternals)registryItem).DeleteValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);

			AssertEquals("ARInvoiceNo for current company", 1601, arInvoiceNoFountain.GetNext(factory));
			AssertEquals("ARCreditNoteNo for current company", 1601, arCreditNoteNoFountain.GetNext(factory));
			AssertEquals("ARAdjustmentNoteNo for current company", 1601, arAdjustmentNoteNoFountain.GetNext(factory));
		}

		#region Implementation

		ShareSequentialTransactionNumbers CreateShareSequentialTransactionNumbers(bool value)
		{
			return new ShareSequentialTransactionNumbers() { Value = value };
		}

		#endregion
	}
}
