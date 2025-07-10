using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class PreviousDocumentsGridColumnBag
{
	public static PreviousDocumentsGridColumnBag Instance => instance ??= new PreviousDocumentsGridColumnBag();

	[ThreadStatic]
	static PreviousDocumentsGridColumnBag instance;

	PreviousDocumentsGridColumnBag()
	{
		CodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(PreviousDocument.Schema.CSI_Code, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		ReferenceNumberColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_ReferenceNumber, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		CountryCodeColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_RN_NKCountryCode, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
		SubTypeDropEditColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(PreviousDocument.Schema.CSI_SubType, 120, c => c.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper);
	}

	public IGridColumnReference CodeDropEditColumn { get; }

	public IGridColumnReference ReferenceNumberColumn { get; }

	public IGridColumnReference CountryCodeColumn { get; }

	public IGridColumnReference SubTypeDropEditColumn { get; }
}

