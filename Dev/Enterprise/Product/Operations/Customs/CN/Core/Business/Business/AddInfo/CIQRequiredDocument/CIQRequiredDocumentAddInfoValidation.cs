//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCIQRequiredDocumentAddInfoValidation
//
//    This class should be used for overriding validation in AutoCIQRequiredDocumentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class CIQRequiredDocumentAddInfoValidation : AutoCIQRequiredDocumentAddInfoValidation
	{
		public CIQRequiredDocumentAddInfoValidation(AutoCIQRequiredDocumentAddInfo parent) : base(parent)
		{
		}

		new CIQRequiredDocumentAddInfo Parent => (CIQRequiredDocumentAddInfo)base.Parent;

		CIQRequiredDocument CIQRequiredDocument => Parent.Parent as CIQRequiredDocument;

		CusEntryInstruction CusEntryInstruction => CIQRequiredDocument?.CusEntryInstruction;

		internal IValidationModeProvider ValidationModeProvider => CusEntryInstruction?.JobDeclaration;

		protected override void CheckXC_DocumentType()
		{
			base.CheckXC_DocumentType();
			var docType = Parent.XC_DocumentType;
			var info = Parent.XC_DocumentTypeInfo;
			MandatoryValidation.CheckEntered(info);
			info.AddNotificationIfInvalidCode(Parent.Lookups.CIQRequiredDocumentTypes, ValidationModeProvider);
			if (AnotherInstructionInDeclarationHasDocumentType(docType))
			{
				info.AddNotification(Res.GetString("d0120d52-6d77-4f47-91b4-10d812ae8dc8", "Duplicated document type. Please chose another one."), ValidationModeProvider);
			}
			if (docType == CIQRequiredDocumentTypeList.Codes._24 && AnotherInstructionInDeclarationHasDocumentType(CIQRequiredDocumentTypeList.Codes._21))
			{
				info.AddNotification(Res.GetString("94438c90-e33b-406c-b12e-4a21dddfade0", "You cannot require '{0}' and '{1}' at the same time.", CIQRequiredDocumentTypeList.Descriptions._24, CIQRequiredDocumentTypeList.Descriptions._21), ValidationModeProvider);
			}
		}

		bool AnotherInstructionInDeclarationHasDocumentType(ZString documentType)
		{
			return CusEntryInstruction?.CIQRequiredDocuments.Cast<CIQRequiredDocument>().Any(ins => ins.XC_DocumentType == documentType && ins != Parent.Parent) ?? false;
		}

		protected override void CheckXC_NumberOfOriginals()
		{
			base.CheckXC_NumberOfOriginals();
			if (!CIQRequiredDocument.RequestOfNotIssuing)
			{
				Parent.XC_NumberOfOriginalsInfo.AddNotificationIfIsZero(ValidationModeProvider);
				Parent.XC_NumberOfOriginalsInfo.AddNotificationIfIsNegative(ValidationModeProvider);
			}
		}

		protected override void CheckXC_NumberOfCopies()
		{
			base.CheckXC_NumberOfCopies();
			if (!CIQRequiredDocument.RequestOfNotIssuing)
			{
				Parent.XC_NumberOfCopiesInfo.AddNotificationIfIsZero(ValidationModeProvider);
				Parent.XC_NumberOfCopiesInfo.AddNotificationIfIsNegative(ValidationModeProvider);
			}
		}
	}
}
