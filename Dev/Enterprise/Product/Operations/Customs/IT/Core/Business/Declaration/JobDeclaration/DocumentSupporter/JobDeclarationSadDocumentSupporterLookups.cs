using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationSadDocumentSupporterLookups : ZLookups
{
	public JobDeclarationSadDocumentSupporterLookups(JobDeclarationSadDocumentSupporter parent) : base(parent)
	{
	}

	new JobDeclarationSadDocumentSupporter Parent => (JobDeclarationSadDocumentSupporter)base.Parent;

	protected override BusinessObjectFactory Factory => Parent.Factory;

	JobDeclaration Declaration => Parent.Declaration;

	public CodeDescriptionPairList EntryHeadersAvailableForPrinting
	{
		get
		{
			if (entryHeadersAvailableForPrintingCachedProperty == null)
			{
				entryHeadersAvailableForPrintingCachedProperty = new CachedProperty<CodeDescriptionPairList>(Factory, () =>
				 {
					 var entryHeadersAvailableForPrinting = new CodeDescriptionPairList();
					 foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
					 {
						 var bgmReference = entryHeader.CH_BGMReference;
						 var description = FormattableString.Invariant($"[{GetEntryStatus(entryHeader)}] {bgmReference}");
						 entryHeadersAvailableForPrinting.AddPair(bgmReference, description);
					 }
					 return entryHeadersAvailableForPrinting;
				 });
			}
			return entryHeadersAvailableForPrintingCachedProperty.Value;
		}
	}
	CachedProperty<CodeDescriptionPairList> entryHeadersAvailableForPrintingCachedProperty;

	public CodeDescriptionPairList LayoutStyleList
	{
		get
		{
			return Factory.GetCachedValue(GetCacheKey(), () => GetCopyNumberList());

			string GetCacheKey()
			{
				var cacheKey = "IT.SadDocumentSupporter_LayoutStyleList_";
				if (Declaration.IsImport)
				{
					cacheKey += nameof(Declaration.IsImport);
				}
				else if (Declaration.IsExport)
				{
					cacheKey += nameof(Declaration.IsExport);
				}
				return cacheKey;
			}
		}
	}

	#region Implementation

	ZString GetEntryStatus(CusEntryHeader entryHeader)
	{
		var entryStatus = entryHeader.CH_EntryStatus;
		return entryStatus.IsEmpty ? (ZString)(NoResString)"no status" : entryStatus;
	}

	CodeDescriptionPairList GetCopyNumberList()
	{
		if (Declaration.IsImport)
		{
			return new SADImportCopyNumberList();
		}

		if (Declaration.IsExport)
		{
			return new SADExportCopyNumberList();
		}

		return new CodeDescriptionPairList();
	}

	#endregion
}
