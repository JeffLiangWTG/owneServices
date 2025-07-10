using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public PreviousDocumentValidation(PreviousDocument parent) : base(parent) { }

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			var csiCode = Parent.CSI_Code;
			if (Parent.ImportExportParent != null && DisallowedPreviousDocumentCodes.Contains<string>(csiCode))
			{
				var isExport = Parent.ImportExportParent.IsExport;
				var isItemLevel = Parent.ParentIsJobComInvoiceLine;
				if (csiCode == PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument && !isExport)
				{
					Parent.CSI_CodeInfo.AddMessageError("This code is only applicable to exports");
				}
				else if (csiCode == PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr && isExport && isItemLevel)
				{
					Parent.CSI_CodeInfo.AddMessageError("This code is only applicable at header level for exports");
				}
				else if (csiCode == PreviousDocumentCodeListCDS.Codes.MasterUniqueConsignmentReferenceMucr && isItemLevel)
				{
					Parent.CSI_CodeInfo.AddMessageError("This code is only applicable at header level");
				}
			}
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
		IEnumerable<string> DisallowedPreviousDocumentCodes => Parent.Factory.GetCachedValue("GB.DisallowedPreviousDocumentCodes", () => new List<string>()
		{
			PreviousDocumentCodeListCDS.Codes.AdministrativeAccompanyingDocument,
			PreviousDocumentCodeListCDS.Codes.DeclarationUniqueConsignmentReferenceDucr,
			PreviousDocumentCodeListCDS.Codes.MasterUniqueConsignmentReferenceMucr,
		});
	}
}
