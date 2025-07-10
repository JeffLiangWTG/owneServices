using System.Globalization;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business;

public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
{
	public JobComInvoiceLineLookups(JobComInvoiceLine parent)
		: base(parent)
	{
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public override ICodeDescriptionPairList Procedures
	{
		get
		{
			var shippmentType = Parent.JobDeclaration?.JE_MessageType ?? ZString.Empty;
			var date = Parent.EffectiveAssessmentDate;
			var dateForCache = date.ToString("yyMMdd", CultureInfo.InvariantCulture);
			return Factory.GetCachedValue($"CH.JobComInvoiceLineLookups.Procedures.{shippmentType}.{dateForCache}", () => new RefCusProcedureCollection(Factory, Constants.CountryCodes.Switzerland, date, ZString.Empty, shippmentType));
		}
	}

	public override CodeDescriptionPairList TaxOrFeeCodeList => RefCusTaxOrFee.Loader.GetList(Factory, Constants.CountryCodes.Switzerland, Parent?.EffectiveAssessmentDate ?? ZDateTime.Today, TaxOrFeeType);

	public CodeDescriptionPairList RefundTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.RefundType, Parent.EffectiveAssessmentDate);

	public override CodeDescriptionPairList WeightUQList => base.WeightUQList ?? new CodeDescriptionPairList();

	public CodeDescriptionPairList PermitObligationCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.PermitObligation, Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public CodeDescriptionPairList NonCustomsLawObligationCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.NonCustomsLawObligation, Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public CodeDescriptionPairList StorageTypeCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.StorageType, Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);

	public ZZRefCusCodeListCombinedCollection CusCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeList.EdecTypes.ECICS, Parent?.EffectiveAssessmentDate ?? ZDateTime.Today);
}
