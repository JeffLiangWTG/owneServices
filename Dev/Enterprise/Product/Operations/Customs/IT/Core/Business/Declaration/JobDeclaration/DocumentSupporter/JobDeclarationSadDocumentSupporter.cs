using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationSadDocumentSupporter : NonPersistentBusinessObject<JobDeclarationSadDocumentSupporterValidation>
{
	public JobDeclarationSadDocumentSupporter(JobDeclaration declaration)
		: base(declaration?.Factory)
	{
		Declaration = Argument.NotNull(declaration, nameof(declaration));
		EntryHeaders = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();

		DefaultValues();
	}

	public static class Schema
	{
		public const string BGMReferenceToPrint = "BGMReferenceToPrint";
		public const string LayoutStyle = "LayoutStyle";
		public const int LayoutMaxLength = 2;
	}

	public JobDeclaration Declaration { get; }
	IEnumerable<CusEntryHeader> EntryHeaders { get; }

	[List(nameof(Lookups) + "." + nameof(JobDeclarationSadDocumentSupporterLookups.EntryHeadersAvailableForPrinting))]
	public ZString BGMReferenceToPrint
	{
		get { return bGMReferenceToPrint; }
		set
		{
			SetNonPersistentPropertyValue(BGMReferenceToPrintInfo, ref bGMReferenceToPrint, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateBGMReferenceToPrint();
			}
		}
	}
	ZString bGMReferenceToPrint;

	public ZPropertyInfo BGMReferenceToPrintInfo => GetZPropertyInfo(Schema.BGMReferenceToPrint);

	[List(nameof(Lookups) + "." + nameof(JobDeclarationSadDocumentSupporterLookups.LayoutStyleList))]
	[MaxLength(Schema.LayoutMaxLength)]
	public ZString LayoutStyle
	{
		get { return copyStyle; }
		set
		{
			SetNonPersistentPropertyValue(LayoutStyleInfo, ref copyStyle, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateLayoutStyle();
			}
		}
	}
	ZString copyStyle;

	public ZPropertyInfo LayoutStyleInfo => GetZPropertyInfo(Schema.LayoutStyle);

	public JobDeclarationSadDocumentSupporterLookups Lookups => lookups ?? (lookups = new JobDeclarationSadDocumentSupporterLookups(this));
	JobDeclarationSadDocumentSupporterLookups lookups;

	public override JobDeclarationSadDocumentSupporterValidation GetNewValidation() => new JobDeclarationSadDocumentSupporterValidation(this);

	public CusEntryHeader GetSelectedEntryHeader() => !BGMReferenceToPrint.IsEmpty ? EntryHeaders.FirstOrDefault(x => x.CH_BGMReference == BGMReferenceToPrint) : null;

	#region Implementation

	void DefaultBGMReferenceToPrint()
	{
		BGMReferenceToPrint = EntryHeaders != null && EntryHeaders.Count() == 1 ? EntryHeaders.First().CH_BGMReference : ZString.Empty;
	}

	void DefaultLayoutStyle()
	{
		if (Declaration.IsImport)
		{
			LayoutStyle = SADImportCopyNumberList.Codes.EsemplareNonValidoAiFiniFiscali;
		}
		else if (Declaration.IsExport)
		{
			LayoutStyle = Declaration.IsUCC6 ? SADExportCopyNumberList.Codes.EsemplareNonValidoAiFiniFiscali : SADExportCopyNumberList.Codes.EsemplarePerLoSpeditoreEsportatore;
		}
	}

	void DefaultValues()
	{
		using (SuspendSettingHasChanges())
		{
			DefaultLayoutStyle();
			DefaultBGMReferenceToPrint();
		}
	}

	#endregion
}
