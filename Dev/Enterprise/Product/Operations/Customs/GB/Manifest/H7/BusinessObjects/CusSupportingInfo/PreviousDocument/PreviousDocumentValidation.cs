using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.H7.Business;

public class PreviousDocumentValidation : EU.H7.Business.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent)
		: base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		if (Parent.Parent is ICanBeImportOrExport importExportParent)
		{
			if (Parent.CSI_Code == PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument && !importExportParent.IsExport)
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("29272290-9be1-402e-9877-856ce1635e26", "This code is only applicable to exports."));
			}
		}
	}
}
