using System;
using CargoWise.Common;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoicePostingExRateOptionRegistryItem : StronglyTypedRegistryItem<InvoicePostingExRateOptionCollection>
	{
		public InvoicePostingExRateOptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new InvoicePostingExRateOptionRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

#if DEBUG
		public void SetValue(Guid companyPK, Guid branchPK, Guid departmentPK, string value)
		{
			var collection = new InvoicePostingExRateOptionCollection();
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, value, 0));
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, value, 0));

			SetValue(companyPK, branchPK, departmentPK, collection);
		}

		public IDisposable SetTemporaryValue(Guid companyPk, Guid branchPk, Guid departmentPk, string temporaryValue)
		{
			var previousValue = GetValueWithoutFallback(companyPk, branchPk, departmentPk);
			SetValue(companyPk, branchPk, departmentPk, temporaryValue);
			return new DisposableAction(() => { SetValue(companyPk, branchPk, departmentPk, previousValue); });
		}
#endif

		class InvoicePostingExRateOptionRegistryItemImpl : RegistryItemImpl
		{
			public InvoicePostingExRateOptionRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new InvoicePostingExRateOptionRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var defaultCollection = new InvoicePostingExRateOptionCollection();
				defaultCollection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code, 0));
				defaultCollection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code, 0));

				return defaultCollection;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.InvoicePostingExRateOptionRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class InvoicePostingExRateOptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoicePostingExRateOptionCollection>
	{
		public InvoicePostingExRateOptionRegistryDataType()
		{
		}
	}
}
