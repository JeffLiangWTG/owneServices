using System.Linq;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusStorageDocPivotValidation : Customs.Business.CusStorageDocPivotValidation
	{
		public CusStorageDocPivotValidation(CusStorageDocPivot parent)
			: base(parent)
		{
			supportDocumentTypes = new[] { string.Empty, Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.JPEG };
		}

		readonly string[] supportDocumentTypes;

		protected override void CheckCSD_DocType()
		{
			base.CheckCSD_DocType();

			if (Parent.Parent is RequestHeader && !supportDocumentTypes.Any(c => c.Equals(Parent.CSD_DocType.ToUpperInvariant())))
			{
				Parent.CSD_DocTypeInfo.AddMessageError(Res.GetString("49FAC4B7-B67F-48CE-A472-BBCD72B0BAFE", "Please choose a document which file type is PDF, JPG or JPEG."));
			}
		}
	}
}
