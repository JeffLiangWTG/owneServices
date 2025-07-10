using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class JobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public JobDeclarationMessageSendingObjectValidation(JobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDJPDocumentsExist();
			ValidatePreviousDocExistsForC40Declaration();
			ValidateSupportingDocExistsToSendForC44Declaration();
			ValidateImportDescriptionLength();
			ValidateSecurityFlag();
			ValidateRequestDispatch();
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			if (Parent.ShouldSend)
			{
				if (Parent.MessageType.IsEmpty)
				{
					Parent.MessageTypeInfo.AddError(Res.GetString("E75E28F2-B50C-41B4-BABF-3C419361FAEF", "Declaration cannot be sent with empty Message Type"));
				}
				else
				{
					if (Parent.MessageTypesList.Count > 0)
					{
						ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo, Parent.MessageTypesList);
					}
					CheckComplementaryExportInvoiceDocumentDeclared();
				}
			}
		}

		void CheckComplementaryExportInvoiceDocumentDeclared()
		{
			if (Parent.MessageType == DeclarationMessageTypeList.Codes.TypeXExport && Parent.Header.HasNoInvoiceUndeclared())
			{
				Parent.MessageTypeInfo.AddWarning(Res.GetString("E45A0E4E-6DF5-4180-A361-53A60F631AC7", "There are no undeclared invoice documents for this entry"));
			}
		}

		void ValidateDJPDocumentsExist()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.MessageType == DeclarationMessageTypeList.Codes.PendingSupportingDocuments && !Parent.Header.CheckSupportingDocumentsHaveProcedure())
				{
					Parent.AddRowError(Res.GetString("58B3AF0C-23EC-4859-92E2-B656F8AF6C38", "You have not entered a Procedure (DJP) for any document"));
				}
			}
		}

		void ValidatePreviousDocExistsForC40Declaration()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.MessageType == DeclarationMessageTypeList.Codes.ImportAmendmentBox40 && !Parent.Header.CheckPreviousDocumentsExist())
				{
					Parent.AddRowError(Res.GetString("FE929702-8888-4674-8CF7-351AC4DEF9B2", "You have not entered a Previous Document"));
				}
			}
		}

		void ValidateSupportingDocExistsToSendForC44Declaration()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.MessageType == DeclarationMessageTypeList.Codes.Box44Documents && !Parent.Header.CheckAnyEntryLineHasSupportingDocumentsToSend())
				{
					Parent.AddRowError(Res.GetString("1CF6FF01-9802-4C5F-B57F-65EAD71F70F7", "You have not entered Supporting Documents"));
				}
			}
		}

		void ValidateImportDescriptionLength()
		{
			if (Parent.ShouldSend)
			{
				if (MessageTypeNeedsValidation(Parent.MessageType) && !Parent.Header.AreAllDescriptionLengthCorrectForImport())
				{
					Parent.AddRowError(Res.GetString("13AFFC2E-C6AA-44F9-AEC5-A4FC026F93FB", "All Goods Descriptions must be at least 5 characters long"));
				}
			}

			bool MessageTypeNeedsValidation(string messageType) => messageType == DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration
																|| messageType == DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration
																|| messageType == DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
		}

		public void ValidateSecurityFlag()
		{
			((IValidationInternals)this).Validate(Parent.SecurityFlagInfo, SecurityFlagValidationInvoker);
		}

		void SecurityFlagValidationInvoker()
		{
			CheckSecurityFlag();
		}

		void CheckSecurityFlag()
		{
			if (Parent.ShouldSend && Parent.SecurityFlagVisible && Parent.SecurityFlag.IsEmpty && (Parent.Header.Declaration?.JE_MessageSubType ?? CargoWise.Types.ZString.Empty) != EU.Business.EntryStyleListImport.Codes.ImportFromSpecialTerritory)
			{
				Parent.SecurityFlagInfo.AddError(Res.GetString("030E3072-2099-4EF5-8B3C-A1D53A8F1EB3", "You have not entered a Security flag"));
			}
		}

		public void ValidateRequestDispatch()
		{
			((IValidationInternals)this).Validate(Parent.RequestDispatchInfo, RequestDispatchValidationInvoker);
		}

		void RequestDispatchValidationInvoker()
		{
			CheckRequestDispatch();
		}

		void CheckRequestDispatch()
		{
			if (Parent.ShouldSend && Parent.RequestDispatchVisible && Parent.RequestDispatch.IsEmpty)
			{
				Parent.RequestDispatchInfo.AddError(Res.GetString("64F7C573-2E74-459C-966C-9113A4A5F1E5", "You have not entered a Request Dispatch"));
			}
		}
	}
}
