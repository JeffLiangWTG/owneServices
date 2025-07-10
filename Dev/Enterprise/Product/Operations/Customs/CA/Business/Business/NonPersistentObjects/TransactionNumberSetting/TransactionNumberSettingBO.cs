using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class TransactionNumberSettingBO : NonPersistentBusinessObject
	{
		public TransactionNumberSettingBO(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		/// <summary>
		/// Collection of settings that already exist in database or those that user is going to save to database.
		/// </summary>
		[ChildEditable]
		public TransactionNumberSettingCollection ExistingTransactionNumberSettingCollection
		{
			get
			{
				if (existingTransactionNumberSettingCollection == null)
				{
					existingTransactionNumberSettingCollection = new TransactionNumberSettingCollection(this, true);
					RegisterEditableChildObject(existingTransactionNumberSettingCollection);
				}
				return existingTransactionNumberSettingCollection;
			}
		}

		TransactionNumberSettingCollection existingTransactionNumberSettingCollection;

		/// <summary>
		/// All combination of available parameters for number fountains, except for those that are included into <see cref="ExistingTransactionNumberSettingCollection"/>. 
		/// </summary>
		[ChildEditable]
		public TransactionNumberSettingCollection AvailableTransactionNumberSettingCollection
		{
			get
			{
				if (availableTransactionNumberSettingCollection == null)
				{
					availableTransactionNumberSettingCollection = new TransactionNumberSettingCollection(this, false);
					RegisterEditableChildObject(availableTransactionNumberSettingCollection);
				}
				return availableTransactionNumberSettingCollection;
			}
		}

		TransactionNumberSettingCollection availableTransactionNumberSettingCollection;
	}
}
