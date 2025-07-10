using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class EURCertificateOfOriginWrapper : CertificateOfOriginWrapper, IEURCertificateOfOrigin
	{
		public EURCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public EURCertificateOfOriginWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		ZString IEURCertificateOfOrigin.OriginCountry => originCountry ?? (originCountry = GetCountryDescription(Declaration.Origin));
		string originCountry;

		ZString IEURCertificateOfOrigin.OriginGroup => originGroup ?? (originGroup = GetOriginGroup());
		string originGroup;

		ZString IEURCertificateOfOrigin.DestinationGroup => destinationGroup ?? (destinationGroup = GetDestinationGroup());
		string destinationGroup;

		IEURGoodsSummary IEURCertificateOfOrigin.GoodsSummary => goodsSummary ?? (goodsSummary = GetNewGoodsSummary());
		IEURGoodsSummary goodsSummary;

		protected virtual IEURGoodsSummary GetNewGoodsSummary() => new EURGoodsSummaryWrapper(InvoiceLines);

		#region Implementation

		protected virtual ZString GetCountryGroup(RefUNLOCO unloco, ZString country)
		{
			var isPartOfEuropeanUnion = (unloco?.Country?.RN_EconomicGrouping ?? ZString.Empty) == EconomicGroupList.Codes.EuropeanUnion;
			return isPartOfEuropeanUnion ? EuropeZoneDescription : country;
		}

		protected virtual ZString GetOriginGroup()
		{
			return GetCountryGroup(Declaration.Origin, ((IEURCertificateOfOrigin)this).OriginCountry);
		}

		protected virtual ZString GetDestinationGroup()
		{
			return GetCountryGroup(Declaration.FinalDestination, ((ICertificateOfOrigin)this).DestinationCountry);
		}

		ZString EuropeZoneDescription => Declaration.Factory.GetCachedValue("JobDeclarationDocDataObjectDataProvider.EuropeZoneDescription.EUR1", () => Declaration.Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, UniversalReferenceConstants.RefZoneHeaders.EUR1)))?.Description ?? ZString.Empty;

		protected IEnumerable<JobComInvoiceLine> InvoiceLines => GetInvoiceLines();

		protected virtual IEnumerable<JobComInvoiceLine> GetInvoiceLines() => EntryHeader?.InvoiceLines.Cast<JobComInvoiceLine>() ?? Declaration.InvoiceLines.Cast<JobComInvoiceLine>();

		#endregion
	}
}
