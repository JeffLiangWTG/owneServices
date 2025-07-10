using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACusRulingValidation : ZZRefCusRulingCombinedValidation
	{
		public CACusRulingValidation(CACusRuling parent) : base(parent)
		{
		}

		protected new CACusRuling Parent
		{
			get { return (CACusRuling)base.Parent; }
		}

		protected override void CheckZZX_RulingNumber()
		{
			base.CheckZZX_RulingNumber();
			if (!Parent.IsSystem && !Parent.ZZX_RulingNumber.IsEmpty && !Parent.ZZX_RulingNumberInfo.HasErrors())
			{
				var rulingFilter = new ZQuery(ZZRefCusRulingCombinedSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				rulingFilter.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_RulingNumber, Parent.ZZX_RulingNumber);

				if (Parent.Factory.LoadTop1<ZZRefCusRulingCombined>(rulingFilter) != null)
				{
					Parent.ZZX_RulingNumberInfo.AddError(ResString.GetMultilingualString("7BBCD4AD-828D-436E-BFBF-B0037776A453", "Remission Number already exists."));
				}
			}
		}
	}
}
