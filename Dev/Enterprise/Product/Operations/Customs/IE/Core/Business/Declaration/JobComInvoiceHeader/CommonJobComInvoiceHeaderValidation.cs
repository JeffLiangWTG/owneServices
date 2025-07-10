using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		protected CommonJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
		}

		protected override ZBool ShouldCheckMissingPreviousDocuments => false;

		protected override void CheckJZ_IncoTermPlace()
		{
			var incoTerm = Parent.JZ_IncoTerm;
			if (!incoTerm.IsEmpty)
			{
				if (incoTerm.EqualsIgnoringCase(Core.Constants.IncoTerms.Other))
				{
					if (!Parent.JZ_IncoTermPlace.IsEmpty)
					{
						var info = Parent.JZ_IncoTermPlaceInfo;
						info.AddMessageError(MandatoryValidation.DoNotEnterMessage(Res.GetString("0079C45B-8517-4231-8810-CEED5B810BC2", "{0}. This should be empty when {1} is {2}", info.HumanReadableName, Parent.JZ_IncoTermInfo.HumanReadableName, Core.Constants.IncoTerms.Other)));
					}
				}
				else
				{
					CheckJZ_IncoTermPlaceWhenIncoTermIsNotOther();
				}
			}
		}

		protected virtual void CheckJZ_IncoTermPlaceWhenIncoTermIsNotOther()
		{
		}

		protected override void CheckJZ_AdditionalTerms()
		{
			if (Parent.JZ_AdditionalTerms.IsEmpty)
			{
				CheckJZ_AdditionalTermsWhenEmpty();
			}
			else if (!Parent.JZ_IncoTerm.EqualsIgnoringCase(Core.Constants.IncoTerms.Other))
			{
				var info = Parent.JZ_AdditionalTermsInfo;
				info.AddMessageError(MandatoryValidation.DoNotEnterMessage(Res.GetString("31260E69-8C15-47A1-B67F-59FA1538A4C1", "{0}. This should be empty when {1} is not {2}", info.HumanReadableName, Parent.JZ_IncoTermInfo.HumanReadableName, Core.Constants.IncoTerms.Other)));
			}
		}

		protected virtual void CheckJZ_AdditionalTermsWhenEmpty()
		{
		}

		protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocument => false;
	}
}
