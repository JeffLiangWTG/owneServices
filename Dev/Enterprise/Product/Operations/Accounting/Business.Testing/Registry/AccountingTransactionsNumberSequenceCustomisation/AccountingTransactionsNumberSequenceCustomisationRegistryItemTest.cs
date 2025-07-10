using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TransactionsNumberSequenceCustomisationRegistryItem))]
	class AccountingTransactionsNumberSequenceCustomisationRegistryItemTest : StronglyTypedRegistryItemTestCase<TransactionNumberSequenceCustomisationCollection>
	{
		public void TestExpectedElements()
		{
			var collection = GetNewRegistryItem().DefaultValue;
			AssertEquals(16, collection.Count);
			var elements = collection.Select(x => (x as TransactionNumberSequenceCustomisation)?.ElementName);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.CustomElement1, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.CustomElement2, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber, elements);

			AssertEquals(4, collection[TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits].Length);
			AssertEquals(4, collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Length);
			AssertEquals(8, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Length);

			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal, elements);
			AssertCollectionContains(TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix, elements);
		}

		public void TestDefaultElementValueForCNCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var collection = GetNewRegistryItem().DefaultValue;
				AssertEquals(3, collection.Cast<TransactionNumberSequenceCustomisation>().Count(x => x.Include));

				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include);
				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Fountain);
				AssertEquals(new ZByte(50), collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Order);
				AssertEquals(6, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Length);

				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Include);
				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Fountain);
				AssertEquals(new ZByte(1), collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Order);
				AssertEquals("2", collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code);

				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Include);
				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Fountain);
				AssertEquals(new ZByte(2), collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Order);
			}
		}

		public void TestDefaultElementValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var collection = GetNewRegistryItem().DefaultValue;
				AssertEquals(1, collection.Cast<TransactionNumberSequenceCustomisation>().Count(x => x.Include));

				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Include);
				AssertEquals(true, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Fountain);
				AssertEquals(new ZByte(50), collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Order);
				AssertEquals(8, collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Length);
			}
		}

		protected override StronglyTypedRegistryItem<TransactionNumberSequenceCustomisationCollection, TransactionNumberSequenceCustomisationCollection> GetNewRegistryItem()
		{
			return new TransactionsNumberSequenceCustomisationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
