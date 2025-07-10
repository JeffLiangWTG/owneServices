using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();

			var declaration = Parent.JobDeclaration;

			if (HasAnyDiffT2CAndT2LAndEXSEntry && Parent.JZ_IncoTermPlace.IsEmpty && declaration.JE_ShipmentIncoTermPlace.IsEmpty)
			{
				var incoterm = Parent.JZ_IncoTerm.IsEmpty ? declaration.JE_ShipmentIncoTerm : Parent.JZ_IncoTerm;
				var agreedPlaceCode = Parent.ZG_AgreedPlaceCode.IsEmpty ? declaration.ZG_AgreedPlaceCode : Parent.ZG_AgreedPlaceCode;
				if (incoterm == Core.Constants.IncoTerms.Other)
				{
					Parent.JZ_IncoTermPlaceInfo.AddMessageError(Res.GetString("38336C1E-7DB1-4624-BDC8-762FB02EC371", "You have not entered a Description for XXX Incoterm"));
				}
				else if (!incoterm.IsEmpty && !agreedPlaceCode.IsEmpty && agreedPlaceCode.Length <= 2)
				{
					Parent.JZ_IncoTermPlaceInfo.AddMessageError(Res.GetString("234372AF-E016-4026-AD4D-A6454E35823D", "Incoterm Location is required"));
				}
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();

			var declaration = Parent.JobDeclaration;
			if (declaration.IsUCC6 && declaration.JE_ShipmentIncoTerm.IsEmpty && Parent.HasAnyDiffT2CAndT2lAndEXSAndBAndCEntry())
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_IncoTermInfo);
			}
		}

		protected override string BuyerAddressEmptyWarningMessage => Res.GetString("274DC587-AACF-4670-870A-751A576ACD06", "Buyer address should be selected");

		protected override bool IncoTermRequired => HasAnyDiffT2CAndT2LAndEXSEntry && !Parent.JobDeclaration.IsUCC6;

		protected override bool IsJZ_ValuationCodeMandatory => HasAnyDiffT2CAndT2LAndEXSEntry;

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			if (HasAnyDiffT2CAndT2LAndEXSEntry)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceAmountInfo);
			}
		}

		bool HasAnyDiffT2CAndT2LAndEXSEntry => Parent.HasAnyDiffT2CAndT2LAndEXSEntry();
	}
}
