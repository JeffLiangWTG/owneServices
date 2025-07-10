using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	public abstract class AmountBasedAuthorisationRequirementCollection : RegistryBusinessObjectCollectionTemplate
	{
		public AmountBasedAuthorisationRequirementCollection()
		{
		}

		public AmountBasedAuthorisationRequirementCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new AmountBasedMultiLevelAuthorisationRequirement this[int x]
		{
			get { return (AmountBasedMultiLevelAuthorisationRequirement)base[x]; }
		}

		public void Sort()
		{
			Sort(GetComparer());
		}

		public new AmountBasedMultiLevelAuthorisationRequirement AddNew()
		{
			return (AmountBasedMultiLevelAuthorisationRequirement)base.AddNew();
		}

		public virtual IComparer<BusinessObject> GetComparer()
		{
			return new Comparer();
		}

		int? fCurrentFallbackCompanyCurrencyDecimals;
		public int CurrentFallbackCompanyCurrencyDecimals
		{
			get
			{
				if (!fCurrentFallbackCompanyCurrencyDecimals.HasValue)
				{
					if (CurrentFallbackLevel != null)
					{
						BusinessObjectFactory factory = new BusinessObjectFactory();
						BusinessObject company = (BusinessObject)factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, CurrentFallbackLevel.CompanyPK(false)));
						IRefCurrency currency = factory.LoadTop1<IRefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, company[GlbCompanySchema.GC_RX_NKLocalCurrency]));
						fCurrentFallbackCompanyCurrencyDecimals = currency.Decimals;
					}
					else
					{
						fCurrentFallbackCompanyCurrencyDecimals = Environment.Env.CurrentCompany.LocalCurrency.Decimals;
					}
				}
				return fCurrentFallbackCompanyCurrencyDecimals.Value;
			}
		}

		class Comparer : IComparer<BusinessObject>
		{
			#region IComparer<BusinessObject> Members

			int IComparer<BusinessObject>.Compare(BusinessObject x, BusinessObject y)
			{
				AmountBasedMultiLevelAuthorisationRequirement settingsX = (AmountBasedMultiLevelAuthorisationRequirement)x;
				AmountBasedMultiLevelAuthorisationRequirement settingsY = (AmountBasedMultiLevelAuthorisationRequirement)y;
				int result = string.Compare(settingsY.Range, settingsX.Range);

				if (result == 0)
				{
					result = (settingsX.Amount < settingsY.Amount) ? -1 : (settingsX.Amount > settingsY.Amount) ? 1 : 0;
				}

				return result;
			}

			#endregion
		}
	}
}
