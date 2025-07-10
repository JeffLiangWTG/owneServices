using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobsNumberSequenceCustomisationRegistryItem : StronglyTypedRegistryItem<TransactionNumberSequenceCustomisationCollection>
	{
		public JobsNumberSequenceCustomisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new AccountingJobsNumberSequenceCustomisationRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		public class AccountingJobsNumberSequenceCustomisationRegistryItemImpl : RegistryItemImpl
		{
			public AccountingJobsNumberSequenceCustomisationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new TransactionsNumberSequenceCustomisationRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				TransactionNumberSequenceCustomisationCollection collection = new TransactionNumberSequenceCustomisationCollection();
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.JobHeaderBranchCode;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.JobHeaderDepartmentCode;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement1;
				collection.AddNew().ElementName = TransactionNumberSequenceCustomisation.ElementNames.CustomElement2;
				TransactionNumberSequenceCustomisation sequenceNumber = collection.AddNew();
				sequenceNumber.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
				sequenceNumber.Order = 1;
				sequenceNumber.Include = true;
				sequenceNumber.Length = 8;
				sequenceNumber.Fountain = true;

				return collection;
			}
		}
	}
}
