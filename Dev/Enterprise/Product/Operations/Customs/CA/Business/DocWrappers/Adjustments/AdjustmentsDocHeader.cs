using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AdjustmentsDocHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdjustmentsDocHeader()
		{
		}

		public AdjustmentsDocHeader(JobDeclaration declaration)
		{
			Initialise(declaration);
		}

		public virtual void Initialise(JobDeclaration declaration)
		{
			PopulateImporter(declaration);

			if (declaration.IsBlanketB2)
			{
				Year = VARIETY;
			}
			else if (declaration.CA_OriginalAccountingDate.IsValid)
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
			OriginalTransactionNo = declaration.CA_OriginalTransactionNo;
			SecurityNo = declaration.CA_SecurityNo;
		}

		protected virtual void PopulateImporter(JobDeclaration declaration)
		{
			var importer = declaration.Importer;
			if (importer != null)
			{
				var address = importer.CustomsAddress ?? importer.MainAddress;
				ImporterFormatted = address == null ? importer.OH_FullNameTruncated : AdjustmentDocHelper.AddressForImporterFormatted(address);

				BusinessNumber = importer.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Canada,
					OrgCusCode.CACodeTypes.BusinessNumberForImportExport, OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial);
				GSTNumber = importer.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax,
					Core.Constants.CountryCodes.Canada);
			}
		}

		public ZString ImporterFormatted { get; set; }
		public ZString Month { get; set; }
		public ZString Day { get; set; }
		public ZString Year { get; set; }
		public ZString MailToFormatted { get; set; }
		public ZString BusinessNumber { get; set; }
		public ZString GSTNumber { get; set; }
		public ZString CBSAOffice { get; set; }
		public ZString OriginalTransactionNo { get; set; }
		public ZString SecurityNo { get; set; }

		const string VARIETY = "VAR";
	}
}
