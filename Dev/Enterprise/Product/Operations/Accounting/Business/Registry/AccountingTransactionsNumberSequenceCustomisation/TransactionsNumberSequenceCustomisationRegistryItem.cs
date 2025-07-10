using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class TransactionsNumberSequenceCustomisationRegistryItem : StronglyTypedRegistryItem<TransactionNumberSequenceCustomisationCollection>
	{
		public TransactionsNumberSequenceCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		public class AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl : RegistryItemImpl
		{
			public AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new NumberSequenceCustomisationRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var collection = CreateDefaultCollection();

				var company = Factory.Load<GlbCompany>(companyPK);
				if (company != null && company.Country.Code == Core.Constants.CountryCodes.China)
				{
					collection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber].Length = 6;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Include = true;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Fountain = true;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code = "2";
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Order = 1;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Include = true;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Fountain = true;
					collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits].Order = 2;
				}

				return collection;
			}

			internal static TransactionNumberSequenceCustomisationCollection CreateDefaultCollection()
			{
				TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement2;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix;
				TransactionNumberSequenceCustomisation sequenceNumber = collection.AddNew();
				sequenceNumber.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
				sequenceNumber.Order = 50;
				sequenceNumber.Include = true;
				sequenceNumber.Length = FormattedNumberFountainFactory.DefaultFormatDigits;
				sequenceNumber.Fountain = true;

				collection[TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits].Code = "4";
				collection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits].Code = "4";

				return collection;
			}

			ReadOnlyBusinessObjectFactory factory;
			ReadOnlyBusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new ReadOnlyBusinessObjectFactory()); }
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.AccountingTransactionsNumberSequenceCustomisationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class TransactionsNumberSequenceCustomisationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TransactionNumberSequenceCustomisationCollection>
	{
		protected override void ValidateCore(IRegistryItem registryItem, TransactionNumberSequenceCustomisationCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.TotalLength > 20)
			{
				throw new RegistryValidationException(Res.GetString("c72170a9-7efd-45ae-bae1-5d63fec2de17", "Please amend your configuration. The total character length of included elements would exceed 20 characters. Transaction Numbers are restricted to a maximum length of 20 characters. Please modify your selections."));
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.AccountingTransactionsNumberSequenceCustomisationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class NumberSequenceCustomisationRegistryDataType : TransactionsNumberSequenceCustomisationRegistryDataType
	{
		protected override TransactionNumberSequenceCustomisationCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			var defaultCollection = TransactionsNumberSequenceCustomisationRegistryItem.AccountingTransactionsNumberSequenceCustomisationRegistryItemImpl.CreateDefaultCollection();
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.CustomElement1);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.CustomElement2);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal);
			AddElementIfMissing(result, defaultCollection, TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix);
			var elementMoveToEndOfList = result[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber];
			result.Remove(elementMoveToEndOfList);
			result.Add(elementMoveToEndOfList);
			return result;
		}

		void AddElementIfMissing(TransactionNumberSequenceCustomisationCollection target, TransactionNumberSequenceCustomisationCollection source, string elementName)
		{
			if (target[elementName] == null && source[elementName] != null)
			{
				var element = source[elementName];
				source.Remove(element);
				target.Add(element);
			}
		}
	}
}
