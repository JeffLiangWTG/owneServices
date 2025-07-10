using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class SupportingDocumentsGridColumnsBag
{
	SupportingDocumentsGridColumnsBag()
	{
		LineNoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_LineNo, 50);
		ImageReferenceNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_ReferenceNumber2, 100);
		DocumentTypeCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(SupportingDocument.Schema.CSI_Code, 100);
		OrganisationGuidFindBoxColumn = new GridColumnReference<ZGuidFindBoxColumnStyleInfo>(SupportingDocument.Schema.OrganizationPK, 100);
		CodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(SupportingDocument.Schema.CSI_IssuerType, 100);
	}

	public static SupportingDocumentsGridColumnsBag Instance => instance ??= new SupportingDocumentsGridColumnsBag();

	public IGridColumnReference LineNoTextBoxColumn { get; }
	public IGridColumnReference ImageReferenceNumberTextBoxColumn { get; }
	public IGridColumnReference DocumentTypeCodeFindBoxColumn { get; }
	public IGridColumnReference OrganisationGuidFindBoxColumn { get; }
	public IGridColumnReference CodeDropEditColumn { get; }

	[ThreadStatic]
	static SupportingDocumentsGridColumnsBag instance;
}

