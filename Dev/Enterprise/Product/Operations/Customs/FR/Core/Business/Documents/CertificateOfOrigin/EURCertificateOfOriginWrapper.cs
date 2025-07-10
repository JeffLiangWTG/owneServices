using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class EURCertificateOfOriginWrapper : EU.Business.Documents.CertificateOfOrigin.EURCertificateOfOriginWrapper
	{
		public EURCertificateOfOriginWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		public EURCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}
		protected override IEURGoodsSummary GetNewGoodsSummary() => new EURGoodsSummaryWrapper(InvoiceLines);

		protected override ZString GetCountryDescription(RefUNLOCO unloco)
		{
			ZString countryCode = unloco?.Country?.Code ?? ZString.Empty;
			if (countryCode == Core.Constants.CountryCodes.Tunisia || countryCode == Core.Constants.CountryCodes.Algeria || countryCode == Core.Constants.CountryCodes.Morocco)
			{
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
				{
					return base.GetCountryDescription(unloco);
				}
			}
			else
			{
				return base.GetCountryDescription(unloco);
			}
		}

		protected override ZString GetDestinationGroup()
		{
			ZString countryCode = Declaration.FinalDestination?.Country?.Code ?? ZString.Empty;
			if (countryCode == Core.Constants.CountryCodes.Tunisia || countryCode == Core.Constants.CountryCodes.Algeria || countryCode == Core.Constants.CountryCodes.Morocco)
			{
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
				{
					return base.GetDestinationGroup();
				}
			}
			else
			{
				return base.GetDestinationGroup();
			}
		}

		protected override ZString GetOriginGroup()
		{
			ZString originGroup = ZString.Empty;
			var countryOfOriginCount = InvoiceLines.Select(x => x.JI_CountryOfOrigin).Distinct().Count();
			if (countryOfOriginCount > 1)
			{
				originGroup = "UE";
			}
			else if (countryOfOriginCount == 1)
			{
				originGroup = InvoiceLines.First().JI_CountryOfOrigin;
			}

			return originGroup;
		}

		protected override ZString GetImporterAddress()
		{
			var address = Declaration.ImporterDocumentaryAddress;
			var importer = address?.Organisation;
			if (importer != null && (address.Country.Code == Core.Constants.CountryCodes.Tunisia || address.Country.Code == Core.Constants.CountryCodes.Algeria || address.Country.Code == Core.Constants.CountryCodes.Morocco))
			{
				var formatter = new AddressFormatter(Declaration.Factory,
				importer.OH_FullName,
				string.Empty,
				address.E2_Address1,
				address.E2_Address2,
				address.E2_City,
				address.E2_State,
				address.E2_Postcode,
				address.Country.RN_DescMultilingual,
				Core.SharedConstants.Languages.French,
				true,
				address.E2_RN_NKCountryCode);

				return formatter.PostalAddress();
			}
			else
			{
				return base.GetImporterAddress();
			}
		}

		protected override IEnumerable<JobComInvoiceLine> GetInvoiceLines()
		{
			var invoiceLines = EntryHeader?.InvoiceLines.Cast<JobComInvoiceLine>() ?? Declaration.InvoiceLines.Cast<JobComInvoiceLine>();
			return invoiceLines.Where(x => x.JI_CountryOfOrigin == Core.Constants.CountryCodes.EuropeanUnion || Declaration.Factory.IsCountryConsideredInEuForSafetyAndSecurity(x.JI_CountryOfOrigin));
		}

		protected override ICustomsEndorsement GetNewCustomsEndorsement() => new FRCustomsEndorsementWrapper((Declaration.JobDeclaration)Declaration);
		protected override IExporterDeclaration GetNewExporterDeclaration() => new FRExporterDeclarationWrapper((Declaration.JobDeclaration)Declaration);
	}
}
