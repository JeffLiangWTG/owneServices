using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseInfoLookups : CusSupportingInfoLookups
	{
		public ImportLicenseInfoLookups(ImportLicenseInfo parent) : base(parent)
		{
		}

		JobComInvoiceLine invoiceLine => Parent.Parent as JobComInvoiceLine;

		public override ICollection CodeList => Factory.GetCachedValue<ImportLicenseType>();

		public CodeDescriptionPairList FeeTypeList => BRRefCusTaxOrFee.GetFeeTypeList(Factory, invoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Today);
	}
}
