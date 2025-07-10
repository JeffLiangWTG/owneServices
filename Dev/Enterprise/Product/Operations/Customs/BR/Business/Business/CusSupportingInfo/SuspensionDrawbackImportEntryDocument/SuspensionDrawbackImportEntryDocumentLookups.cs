
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackImportEntryDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		public SuspensionDrawbackImportEntryDocumentLookups(SuspensionDrawbackImportEntryDocument parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ImportDocumentCategoryEntryList => Factory.GetCachedValue<ImportDocumentCategoryEntryList>();
	}
}
