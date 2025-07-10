using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ReferenceInvoiceManualLookups : Customs.Business.CusSupportingInfoLookups
	{
		public ReferenceInvoiceManualLookups(ReferenceInvoiceManual parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList BRStateList => new OrgCodeLists().State_List(Factory, Core.Constants.CountryCodes.Brazil);

		public CodeDescriptionPairList ModelOfNotaFiscalList => Factory.GetCachedValue<ModelOfNotaFiscalList>();
	}
}
