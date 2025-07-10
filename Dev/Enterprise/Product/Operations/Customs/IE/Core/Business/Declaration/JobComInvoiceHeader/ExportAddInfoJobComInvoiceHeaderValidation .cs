using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class ExportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public ExportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckZG_AgreedPlaceCode()
		{
			var parent = Parent;
			var agreedPlace = Parent.ZG_AgreedPlaceCode;
			if (parent.JZ_IncoTerm.EqualsIgnoringCase(Core.Constants.IncoTerms.Other))
			{
				if (!agreedPlace.IsEmpty)
				{
					var targetInfo = parent.ZG_AgreedPlaceCodeInfo;
					targetInfo.AddMessageError(MandatoryValidation.DoNotEnterMessage(Res.GetString("F06A92E5-4390-4C4D-81FE-222963EAFA96", "{0}. This should be empty when {1} is {2}", targetInfo.HumanReadableName, parent.JZ_IncoTermInfo.HumanReadableName, Core.Constants.IncoTerms.Other)));
				}
			}
			else if (agreedPlace.IsEmpty && !parent.JZ_IncoTerm.IsEmpty)
			{
				parent.ZG_AgreedPlaceCodeInfo.AddMessageError(Res.GetString("244ED8A7-4203-4FF0-949E-157D904EEEF5", "INCO terms place is required when INCO terms is present."));
			}
		}

		protected override void CheckZG_TransportChargesMethodOfPayment()
		{
			base.CheckZG_TransportChargesMethodOfPayment();
			var parent = Parent;

			if (parent.ZG_TransportChargesMethodOfPayment.IsEmpty && parent.CusEntryInstructions.Cast<CusEntryInstruction>().Any(CusEntryInstructionStyleHelper.TransportChargesMoPRequired))
			{
				parent.ZG_TransportChargesMethodOfPaymentInfo.AddMessageError(Res.GetString("5BA202C2-4827-429C-BAF4-9CCD34338109", "Method of Payment is mandatory and should be present."));
			}
		}
	}
}
