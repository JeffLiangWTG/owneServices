using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class SupportingDocumentsGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_Code, 80);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_ReferenceNumber, 200);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_YearOfIssue, 80);
		builder.AddColumn<ZCodeFindBoxColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_RN_NKCountryCode, 80);
		builder.AddColumn<ZCalcEditColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_ItemNumber, 80);
		builder.AddColumn<ZTextBoxColumnStyleInfo>(NctsSupportingDocument.Schema.CSI_ReferenceNumber2, 200);
		return builder.Build();
	}

	#endregion
}
