using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Registry.Business
{
	public class ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem : StronglyTypedRegistryItem<IShareSequentialReferenceNumbers, ShareSequentialReferenceNumbers>
	{
		public ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItemImpl(name, category, caption, hint, storage, option))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			SynchronizeNumberFountains(companyOrOwnerPK,
				GetValueWithoutFallback(companyOrOwnerPK, branchPK, departmentPK).Value,
				((ShareSequentialReferenceNumbers)newValue).Value);
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		protected override void DeleteValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			SynchronizeNumberFountains(companyPK,
				GetValueWithoutFallback(companyPK, branchPK, departmentPK).Value,
				DefaultValue.Value);
			base.DeleteValueCore(companyPK, branchPK, departmentPK);
		}

		void SynchronizeNumberFountains(Guid companyPK, ZBool oldValue, ZBool newValue)
		{
			if (oldValue != newValue)
			{
				ShareSequentialNumberSynchronizer.SynchronizeNumberGenerators(
					new List<INumberFountainProxy>() {
						Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, companyPK),
						Environment.Env.NumberFountains.ComplianceDocumentInternalReference(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote, companyPK) },
					new BusinessObjectFactory());
			}
		}

		internal class ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItemImpl : RegistryItemImpl
		{
			public ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new ShareSequentialReferenceNumbersRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var baseObj = base.GetDefaultValueCore(companyPK, branchPK, departmentPK);

				bool result = false;
				var targetCountryCode = Enterprise.Core.Constants.CountryCodes.Taiwan;

				if (companyPK == GlbCompany.CurrentCompany.PK)
				{
					result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == targetCountryCode;
				}
				else
				{
					var query = new ZQuery(GlbCompanySchema.PK, companyPK);
					query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, targetCountryCode);
					result = new BusinessObjectFactory().Exists(typeof(GlbCompany), query);
				}

				var refNumber = baseObj as ShareSequentialReferenceNumbers;
				if (refNumber != null)
				{
					refNumber.Value = result;
				}
				else
				{
					baseObj = ShareSequentialNumberSynchronizer.CreateShareSequentialReferenceNumbers(result);
				}

				return baseObj;
			}
		}
	}
}