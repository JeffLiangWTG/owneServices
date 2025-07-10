using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.EU.Business.Declaration;

public class CopyDocumentsSelectionLine(ImportExportAwareSupportingInfo info) : AutoCopyDocumentsSelectionLine(info.Factory)
{
	public ImportExportAwareSupportingInfo Info => info;

	public override ZString CSI_Type => info.CSI_Type;

	public override ZString CSI_SubType => info.CSI_SubType;

	public override ZString CSI_Code => info.CSI_Code;

	public override ZString CSI_ReferenceNumber => info.CSI_ReferenceNumber;

	public override ZString CSI_ReferenceNumber2 => info.CSI_ReferenceNumber2;

	public override ZString CSI_Description => info.CSI_Description;
}
