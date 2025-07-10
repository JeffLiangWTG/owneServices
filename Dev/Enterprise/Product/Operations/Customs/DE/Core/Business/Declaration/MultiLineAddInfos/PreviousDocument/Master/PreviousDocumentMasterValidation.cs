using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PreviousDocumentMasterValidation : ZValidation
	{
		public PreviousDocumentMasterValidation(PreviousDocumentMaster parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly PreviousDocumentMaster parent;

		public override Type AutoValidationType => typeof(PreviousDocumentMasterValidation);

		public override void ValidateAll()
		{
			if (!(parent.Parent is CusEntryInstruction) || parent.IsImport)
			{
				ValidateCSI_Procedure();
				ValidateAuthorizationNumber();
			}
		}

		public void ValidateCSI_Procedure()
		{
			ValidateCalculatedProperty(parent.CSI_ProcedureInfo);
		}

		protected void CheckCSI_Procedure()
		{
			var parentProvider = parent.Parent;
			if (parentProvider.JobDeclaration is JobDeclaration declaration)
			{
				var info = parent.CSI_ProcedureInfo;
				if (declaration.IsExport && parentProvider is PreviousProcedureParentProvider provider)
				{
					ListValidation.MessageErrorIfInvalidCode(info);

					if (PreviousDocumentValidationHelper.PreviousProcedureIsRequiredForInvoiceLine(provider.InvoiceLine))
					{
						MandatoryValidation.WarnIfNotEntered(info);
					}
				}
				else
				{
					var entryInstruction = parentProvider as CusEntryInstruction;
					if (entryInstruction == null || entryInstruction.CEI_Style != ImportDeclarationTypeList.Codes.AVABR)
					{
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
					}
				}

				var prevDocProcedureCode = parent.CSI_Procedure;
				if (!prevDocProcedureCode.IsEmpty && declaration.JE_PrematureInputFlag && PreviousProcedureList.IsValidForPrematureInputFlag(prevDocProcedureCode))
				{
					info.AddMessageError(Res.GetString("611BCBA4-CD35-465D-8E5E-F6BFF5C0AC67", "Premature Input flag invalid for Previous Document."));
				}
			}
		}

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(parent.AuthorizationNumberInfo);
		}

		protected void CheckAuthorizationNumber()
		{
			var parent = this.parent;
			if (parent.IsAvailable(PreviousDocument.Schema.AuthorizationNumber))
			{
				var info = parent.AuthorizationNumberInfo;
				if (!info.ReadOnly)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
					ListValidation.WarnIfInvalidCode(info, ResString.GetMultilingualString("48E87F6C-ECF8-4A3B-97D3-344F9E40D62A", "The entered Authorization is not linked to an Organization of this Declaration."));
				}
			}
		}

		public void ValidateCSI_CustomsOffice()
		{
			ValidateCalculatedProperty(parent.CSI_CustomsOfficeInfo);
		}

		protected void CheckCSI_CustomsOffice()
		{
			var parent = this.parent;
			if (parent.IsAvailable(PreviousDocument.Schema.CSI_CustomsOffice))
			{
				var info = parent.CSI_CustomsOfficeInfo;
				ListValidation.MessageErrorIfInvalidCode(info, ResString.GetMultilingualString("ae04cd36-7cd9-4bef-b9d5-50375ae58a9d", "The entered Customs Office is not a Main Office in Germany."));
				if (parent.SimplifiedGrantAuthorizationFlag)
				{
					MandatoryValidation.MessageErrorIfNotEntered(info);
				}
			}
		}
	}
}
