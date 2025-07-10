using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public LocalExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSupportingDocumentCode();
			ValidateSupportingDocumentReferenceNumber();
		}

		protected override void CheckJI_InvoiceQuantity()
		{
			base.CheckJI_InvoiceQuantity();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_InvoiceQuantityInfo);
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InvoiceUQInfo);
		}

		protected override void CheckJI_NetWeight()
		{
			base.CheckJI_NetWeight();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_NetWeightInfo);
		}

		public void ValidateSupportingDocumentCode()
		{
			ValidateCalculatedProperty(Parent.SupportingDocumentCodeInfo);
			Parent.AddAllNotificationsFromSupportingDocumentCodeCSI_Code();
		}

		protected void CheckSupportingDocumentCode()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SupportingDocumentCodeInfo);
		}

		public void ValidateSupportingDocumentReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.SupportingDocumentReferenceNumberInfo);
		}

		protected void CheckSupportingDocumentReferenceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SupportingDocumentReferenceNumberInfo);
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			base.CheckJI_PreviousEntryNumber();
			if (Parent.Declaration.JE_ExportGoodsType == LocalExportGoodsTypeList.Codes.OriginalState)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_PreviousEntryNumberInfo);
				if (Parent.JI_OriginalStateDocType == OriginalStateDocTypeList.Codes._01)
				{
					if (Parent.JI_PreviousEntryNumber.Length != 19 && Parent.JI_PreviousEntryNumber.Length != 20)
					{
						Parent.JI_PreviousEntryNumberInfo.AddMessageError(Res.GetString("05C6CB98-1C2C-4D56-87D4-77B333233A78", "If 'Previous Document Type' is '01', the length of 'Previous Document No.' must be 19 or 20 characters."));
					}
				}
				else if (Parent.JI_OriginalStateDocType == OriginalStateDocTypeList.Codes._02)
				{
					if (Parent.JI_PreviousEntryNumber.Length != 17 && Parent.JI_PreviousEntryNumber.Length != 18)
					{
						Parent.JI_PreviousEntryNumberInfo.AddMessageError(Res.GetString("C2C7C695-69C8-4B15-847E-9118EA45ECD2", "If 'Previous Document Type' is '02', the length of 'Previous Document No.' must be 17 or 18 characters."));
					}
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_PreviousEntryNumberInfo);
			}
		}

		protected override void CheckJI_PackType()
		{
			base.CheckJI_PackType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_PackTypeInfo);
		}

		protected override void CheckJI_NoOfPacks()
		{
			base.CheckJI_NoOfPacks();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_NoOfPacksInfo);
		}

		protected override void CheckJI_OriginalStateDocType()
		{
			base.CheckJI_OriginalStateDocType();
			if (Parent.Declaration.JE_ExportGoodsType == LocalExportGoodsTypeList.Codes.OriginalState)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_OriginalStateDocTypeInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_OriginalStateDocTypeInfo);
			}
		}

		protected override void CheckJI_Ingredient()
		{
			base.CheckJI_Ingredient();
			if (Parent.Declaration.JE_MessageSubType != LocalExportTransactionNatureCodeList.Codes._01)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_IngredientInfo);
			}
		}

		protected override void CheckJI_InboundDate()
		{
			base.CheckJI_InboundDate();
			if (LocalExportTransactionNatureCodeList.Is5DP(Parent.Declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_InboundDateInfo);
				if (Parent.JI_InboundDate.IsValid && !Parent.JI_InboundDate.IsInThePastDatePartOnly)
				{
					Parent.JI_InboundDateInfo.AddMessageError(Res.GetString("270D63C3-F606-421C-8136-DDA8FB7AFF17", "The inbound date should be in the past."));
				}
			}
			else if (LocalExportTransactionNatureCodeList.Is5DQ(Parent.Declaration.JE_MessageSubType))
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JI_InboundDateInfo);
			}
		}
	}
}
