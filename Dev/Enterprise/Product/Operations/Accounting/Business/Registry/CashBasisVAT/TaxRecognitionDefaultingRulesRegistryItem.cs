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
	public class TaxRecognitionDefaultingRulesRegistryItem : StronglyTypedRegistryItem<TaxRecognitionDefaultingRules>
	{
		public TaxRecognitionDefaultingRulesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new TaxRecognitionDefaultingRulesRegistryItemRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		public class TaxRecognitionDefaultingRulesRegistryItemRegistryItemImpl : RegistryItemImpl
		{
			public TaxRecognitionDefaultingRulesRegistryItemRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new TaxRecognitionDefaultingRulesRegistryDataType(), storage)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.TaxRecognitionDefaultingRulesRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class TaxRecognitionDefaultingRulesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TaxRecognitionDefaultingRules>
	{
		protected override void ValidateCore(IRegistryItem registryItem, TaxRecognitionDefaultingRules proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var currentFallbackLevelCompany = Factory.Load<GlbCompany>(companyPK);
			if (currentFallbackLevelCompany != null && !currentFallbackLevelCompany.GC_IsGSTCashBasis)
			{
				throw new RegistryValidationException(Res.GetString("b566e8ae-cf0e-414a-9db0-84d4026e116e", "Override is Not Allowed. The Selected Company is not Licensed to use Cash Basis GST/VAT features."));
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
