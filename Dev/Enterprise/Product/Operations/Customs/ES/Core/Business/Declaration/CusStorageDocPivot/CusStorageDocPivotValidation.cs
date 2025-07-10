using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusStorageDocPivotValidation : Customs.Business.CusStorageDocPivotValidation
	{
		public CusStorageDocPivotValidation(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		protected override void CheckCSD_StorageDocReference()
		{
			base.CheckCSD_StorageDocReference();
			ListValidation.ErrorIfInvalidPK(Parent.CSD_StorageDocReferenceInfo);
		}

		protected override void CheckCSD_DocType()
		{
			base.CheckCSD_DocType();
			if (!ESConstants.AcceptedDocumentExtensions.DocumentExtensions.Contains(Parent.CSD_DocType))
			{
				Parent.AddRowError(AnnexExtensionError);
			}
		}

		protected override string DocTypeStorageDuplicatingMessage => Res.GetString("4B617756-2B29-44EC-845E-A2861002C879", "An eDoc can only be declared once per entry");

		protected string AnnexExtensionError => ResString.GetMultilingualString("C9E880CE-90CB-4CAD-B945-E3066AB6E12A", "This document has an invalid extension. Allowed extensions are DOC, DOCX, GIF, JPEG, JPG, PDF, RTF, TIF, TIFF, TXT, XLS, XLSX, ZIP, 7Z ");
	}
}
