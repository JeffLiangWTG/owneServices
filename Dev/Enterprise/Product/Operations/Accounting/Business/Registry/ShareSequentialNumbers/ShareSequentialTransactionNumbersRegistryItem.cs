using System;
using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ShareSequentialTransactionNumbersRegistryItem : StronglyTypedRegistryItem<IShareSequentialTransactionNumbers, ShareSequentialTransactionNumbers>
	{
		public ShareSequentialTransactionNumbersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ShareSequentialTransactionNumbersRegistryDataType(), storage))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			SynchronizeNumberFountains(companyOrOwnerPK, GetValueWithoutFallback(companyOrOwnerPK, branchPK, departmentPK).Value, ((ShareSequentialTransactionNumbers)newValue).Value);
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		protected override void DeleteValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			SynchronizeNumberFountains(companyPK, GetValueWithoutFallback(companyPK, branchPK, departmentPK).Value, DefaultValue.Value);
			base.DeleteValueCore(companyPK, branchPK, departmentPK);
		}

		void SynchronizeNumberFountains(Guid companyPK, ZBool oldValue, ZBool newValue)
		{
			if (oldValue != newValue && GlbCompany.CurrentCompany.PK == companyPK)
			{
				ShareSequentialNumberSynchronizer.SynchronizeNumberGenerators(
					new List<INumberFountainProxy>() {
						Env.NumberFountains.ARInvoiceNo.GetTodaysPeriodFountain(),
						Env.NumberFountains.ARCreditNoteNo.GetTodaysPeriodFountain(),
						Env.NumberFountains.ARAdjustmentNoteNo.GetTodaysPeriodFountain() },
					new BusinessObjectFactory());
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ShareSequentialNumbersRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class ShareSequentialTransactionNumbersRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ShareSequentialTransactionNumbers>
	{
		public ShareSequentialTransactionNumbersRegistryDataType()
		{
		}
	}
}
