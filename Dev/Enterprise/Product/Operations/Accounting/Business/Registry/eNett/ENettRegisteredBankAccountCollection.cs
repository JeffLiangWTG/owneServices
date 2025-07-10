using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ENettRegisteredBankAccountCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ENettRegisteredBankAccountCollection()
		{
		}

		public ENettRegisteredBankAccountCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#region Collection Implementation

		public new ENettRegisteredBankAccount this[int index]
		{
			get { return (ENettRegisteredBankAccount)Elements[index]; }
		}

		public new ENettRegisteredBankAccount AddNew()
		{
			ENettRegisteredBankAccount newBizObj = (ENettRegisteredBankAccount)base.AddNew();
			if (Count == 1)
			{
				newBizObj.IsDefault = true;
			}
			return newBizObj;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			ENettRegisteredBankAccount elementBeingRemoved = (ENettRegisteredBankAccount)bizO;

			if (elementBeingRemoved.IsDefault)
			{
				ENettRegisteredBankAccount anyNonDefaultAccount = GetBankAccount(elementBeingRemoved.BankAccountCurrencyPK, false);

				if (anyNonDefaultAccount != null)
				{
					anyNonDefaultAccount.IsDefault = true;
				}
			}

			base.OnRemoved(bizO);
		}

		public void Add(ENettRegisteredBankAccount bankAccount)
		{
			base.Add(bankAccount);
			if (Count == 1)
			{
				bankAccount.IsDefault = true;
			}
		}

		public bool IsDuplicateItem(ENettRegisteredBankAccount itemToCheck)
		{
			bool result = false;

			foreach (ENettRegisteredBankAccount item in this)
			{
				if (item != itemToCheck && item.BankAccountPK == itemToCheck.BankAccountPK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public bool ContainsBankAccount(ZGuid bankAccountPK)
		{
			return FindBankAccount(bankAccountPK) != null;
		}

		public ENettRegisteredBankAccount FindBankAccount(ZGuid bankAccountPK)
		{
			foreach (ENettRegisteredBankAccount element in this)
			{
				if (element.BankAccountPK == bankAccountPK)
				{
					return element;
				}
			}

			return null;
		}

		ENettRegisteredBankAccount GetBankAccount(ZGuid currencyPK, bool isDefault)
		{
			foreach (ENettRegisteredBankAccount element in this)
			{
				if (element.IsDefault == isDefault && element.BankAccountCurrencyPK == currencyPK)
				{
					return element;
				}
			}

			return null;
		}

		public ENettRegisteredBankAccount GetDefaultReceiptBankAccount(ZGuid currencyPK)
		{
			return GetBankAccount(currencyPK, true);
		}

		#endregion

		#region Overriden

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ENettRegisteredBankAccountCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ENettRegisteredBankAccount(CurrentFallbackLevel, Factory);
		}

		#endregion
	}
}