using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAClassificationLookups : AutoCusCAClassificationLookups
	{
		public CusCAClassificationLookups(AutoCusCAClassification parent)
			: base(parent)
		{
		}

		new CusCAClassification Parent => (CusCAClassification)base.Parent;

		CusClassPartPivot Pivot => Parent.Pivot;

		IDutyAndTaxData ParentOfCusCAClassification => Parent.Parent as IDutyAndTaxData;

		public CodeDescriptionPairList StatesOfOrigin => LookupsHelper.StatesOfOriginBase(Factory, Pivot?.IsExport ?? false, Parent.CCA_RN_NKOrigin);

		public CodeDescriptionPairList CFIAStatesOfOrigin => LookupsHelper.CFIAStatesOfOrigin(Factory);

		public CodeDescriptionPairList CanadianProvinces => LookupsHelper.CanadianProvinces(Factory);

		public CACFIAEndUseCodesCollection CFIAEndUseCodes => LookupsHelper.CFIAEndUseCodes(Factory);

		public CACFIAMiscCodesCollection CFIAMiscIDCodes => LookupsHelper.CFIAMiscIDCodes(Factory);

		public ValueForDutyCodes ValueForDutyCodes => LookupsHelper.ValueForDutyCodes(Factory);

		public CodeDescriptionPairList TreatmentCodes
		{
			get
			{
				if (cachedTreatmentCodes == null)
				{
					cachedTreatmentCodes = new CachedProperty<CodeDescriptionPairList>(Factory, () =>
					{
						if (Pivot != null || Parent.Parent is CusClassification)
						{
							return LookupsHelper.TreatmentCodesByOriginAndExport(Factory, Parent.CCA_RN_NKOrigin, ZString.Empty, ZString.Empty);
						}
						else
						{
							return LookupsHelper.TreatmentCodes(Factory);
						}
					});
				}
				return cachedTreatmentCodes.Value;
			}
		}
		CachedProperty<CodeDescriptionPairList> cachedTreatmentCodes;

		public CodeDescriptionPairList ImportReasonCodes => LookupsHelper.ImportReasonCodes(Factory);

		public CodeDescriptionPairList GSTStatusCodes => LookupsHelper.GSTStatusCodes(Factory);

		public CodeDescriptionPairList ETExemptionCodes => LookupsHelper.ETExemptionCodes(Factory);

		public CodeDescriptionPairList ExciseTaxRateCodes
		{
			get
			{
				return UniversalReferenceHelper.GetRefCusRateCodePairList(Factory, DutyAndTaxTypes.Codes.ExciseTax, ZDateTime.Today, ParentOfCusCAClassification?.ClassificationNumber);
			}
		}

		public CodeDescriptionPairList CCA_PGAIndicatorList => LookupsHelper.CA_PGAIndicatorList(Factory);

		public RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public RefCountryCollection DefaultOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}

		public CodeDescriptionPairList StatesOfExport
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}
	}
}
