using System.Globalization;

namespace Enterprise.Customs.CA.Business
{
	public class IM2AdjustmentsDocHeader : AdjustmentsDocHeader
	{
		public IM2AdjustmentsDocHeader() : base()
		{
		}

		public IM2AdjustmentsDocHeader(JobDeclaration declaration) : base(declaration)
		{
		}

		public override void Initialise(JobDeclaration declaration)
		{
			PopulateImporter(declaration);

			if (declaration.CA_OriginalAccountingDate.IsValid)
			{
				var date = declaration.CA_OriginalAccountingDate;
				Month = date.ToString("MM", CultureInfo.InvariantCulture);
				Day = date.ToString("dd", CultureInfo.InvariantCulture);
				Year = date.ToString("yyyy", CultureInfo.InvariantCulture);
			}

			if (declaration.JE_OH_NotifyParty.IsValid && declaration.JE_OH_NotifyParty != declaration.JE_OH_Importer)
			{
				var mailTo = declaration.NotifyParty;
				if (mailTo != null)
				{
					var address = mailTo.MainAddress;
					MailToFormatted = address == null ? mailTo.OH_FullNameTruncated : AdjustmentDocHelper.AddressForMailToWithFullAddress(address);
				}
			}

			CBSAOffice = declaration.JE_CustomsOffice.TrimStart('0');

			if (declaration.CA_AmendmentTo == AmendmentToList.Codes.OriginalB3 || declaration.PreviousTransactionNumber.IsEmpty)
			{
				OriginalTransactionNo = declaration.CA_OriginalTransactionNo;
			}
			else
			{
				OriginalTransactionNo = declaration.PreviousTransactionNumber;
			}

			SecurityNo = declaration.CA_SecurityNo;
		}
	}
}
