using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Registry
{
	class IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItem : BooleanRegistryItem
	{
		public IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItem(IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemImpl inner)
			: base(inner)
		{
		}
	}

	class IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemImpl : RegistryItemImpl
	{
		public IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemImpl(BooleanRegistryItem parentRegistryItem, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new BooleanRegistryDataType(), storage, options)
		{
			this.parentRegistryItem = parentRegistryItem;
		}

		readonly BooleanRegistryItem parentRegistryItem;

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return parentRegistryItem.Value;
		}
	}
}
