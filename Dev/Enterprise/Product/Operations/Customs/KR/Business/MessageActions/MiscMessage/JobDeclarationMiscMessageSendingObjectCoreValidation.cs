using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObjectCoreValidation : JobDeclarationMessageSendingObjectValidationCore
	{
		public JobDeclarationMiscMessageSendingObjectCoreValidation(JobDeclarationMiscMessageSendingObjectCore parent) : base(parent)
		{ }

		protected new JobDeclarationMiscMessageSendingObjectCore Parent => (JobDeclarationMiscMessageSendingObjectCore)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			if (Parent.Header.Declaration.IsExport)
			{
				ValidateExport();
			}
			else if (Parent.Header.Declaration.IsLocalExport)
			{
				ValidateLocalExport();
			}
			else if (Parent.Header.Declaration.IsImport)
			{
				ValidateImport();
			}
		}

		public void ValidateExport()
		{
			ValidateAmendmentReason();
			ValidateReasonCode();
			ValidateFaultParty();
			ValidateDateOfFinalPrice();
		}

		public void ValidateLocalExport()
		{
			ValidateReasonCode();
			ValidateAmendmentReason();
		}

		public void ValidateImport()
		{
			ValidateAmendmentReason();
		}

		public void ValidateAmendmentReason()
		{
			ValidateCalculatedProperty(Parent.AmendmentReasonInfo);
		}

		protected virtual void CheckAmendmentReason()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.Header.IsImport && (Parent.MessageType == ElectronicDocumentTypeList.Codes._DHS || Parent.MessageType == ElectronicDocumentTypeList.Codes._5BB))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AmendmentReasonInfo);
				}
				else if (Parent.Header.IsExport || Parent.ReasonCode == LocalExportAmendmentReasonCodeList.Codes._9)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.AmendmentReasonInfo);
				}
			}
		}

		public void ValidateReasonCode()
		{
			ValidateCalculatedProperty(Parent.ReasonCodeInfo);
		}
		protected virtual void CheckReasonCode()
		{
			if (Parent.ShouldSend)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ReasonCodeInfo);
			}
		}
		public void ValidateFaultParty()
		{
			ValidateCalculatedProperty(Parent.FaultPartyInfo);
		}
		protected virtual void CheckFaultParty()
		{
			if (Parent.ShouldSend)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.FaultPartyInfo);

				if ((Parent.ReasonCode == ExportAmendmentReasonCodeList.Codes._12 || Parent.ReasonCode == ExportAmendmentReasonCodeList.Codes._13) && Parent.FaultParty != ExportImputationReasonCodeList.Codes.A)
				{
					Parent.FaultPartyInfo.AddMessageError(FaultPartyWithReasonCodeMessageErr);
				}

				if (Parent.Header.Declaration.IsSelfDeclaringOwner && !ExportImputationReasonCodeList.IsGoodForSelfDeclaringSupplier(Parent.FaultParty))
				{
					Parent.FaultPartyInfo.AddMessageError(FaultPartyWithSupplierMessageErr);
				}
			}
		}

		public void ValidateDateOfFinalPrice()
		{
			ValidateCalculatedProperty(Parent.DateOfFinalPriceInfo);
		}
		protected virtual void CheckDateOfFinalPrice()
		{
			if (Parent.ShouldSend && !Parent.DateOfFinalPrice_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.DateOfFinalPriceInfo);

				if (Parent.DateOfFinalPrice > ZDateTime.Today)
				{
					Parent.DateOfFinalPriceInfo.AddMessageError(DateOfFinalPriceMessageErr);
				}
			}
		}

		public ZString DateOfFinalPriceMessageErr => Res.GetString("DD486F00-E181-4D43-8B3A-6ECC6CD62CAA", "Please enter a Date Of Final Price earlier than or equal to today");
		public ZString FaultPartyWithReasonCodeMessageErr => Res.GetString("50B6694F-61A5-4EA3-BD5A-4E4B502DF8FA", "If the Reason Code is 12 or 13, you must apply for Fault Party as A");
		public ZString FaultPartyWithSupplierMessageErr => Res.GetString("40CF6DD9-9B3D-4F4F-9261-1C8832C97568", "For self-declaring suppliers, the fault party code should be one of these  C, D, G, Z, or E.");
	}
}
