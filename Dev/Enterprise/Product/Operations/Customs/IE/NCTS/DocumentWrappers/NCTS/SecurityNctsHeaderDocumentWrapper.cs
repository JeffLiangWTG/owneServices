using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.NCTS;

namespace Enterprise.Customs.IE.NCTS.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class SecurityNctsHeaderDocumentWrapper : Enterprise.DocumentWrappers.Customs.EU.NCTS.SecurityNctsHeaderDocumentWrapper
	{
		protected SecurityNctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public static SecurityNctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factoryToWrap)
		{
			return new SecurityNctsHeaderDocumentWrapper(nctsHeader, factoryToWrap);
		}

		protected new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

		public DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> BoxS29Lines => boxS29Lines ?? (boxS29Lines = new IENctsDepartureCargoDescWrapperCollection(NctsHeader.Bills.FirstOrDefault(), Factory));
		DocBaseWrapperCollection<NctsDepartureCargoDescWrapper> boxS29Lines;

		protected override ZString ConvertDateToString(ZDateTime? date) => date?.ToString("yyyy-MM-dd") ?? ZString.Empty;

		protected override ZString GetBox7ReferenceNumbers()
		{
			var returnString = MovementHeader?.BM_UniqueConsignmentReference ?? ZString.Empty;
			var lrn = MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;
			if (lrn != ZString.Empty)
			{
				if (returnString != ZString.Empty)
				{
					returnString += "; ";
				}
				returnString += lrn;
			}
			return returnString;
		}

		protected override ZString GetBox21BorderTransportId()
		{
			var returnString = base.GetBox21BorderTransportId();
			if (MovementHeader != null && MovementHeader.BM_RN_NKTOLCarrierNationality != ZString.Empty)
			{
				if (returnString != ZString.Empty)
				{
					returnString += "; ";
				}
				returnString += MovementHeader.BM_RN_NKTOLCarrierNationality;
			}
			return returnString;
		}

		protected override ZString GetTransitCustomsOffices(ZInt rank)
		{
			var returnString = base.GetTransitCustomsOffices(rank);
			if (rank == 1)
			{
				var bindingItinerary = NctsHeader.CountriesOfRouting.Any(p => !p.CY_Data.IsEmpty) ? BindingItineraryYes : BindingItineraryNo;
				returnString = returnString == ZString.Empty ? bindingItinerary : $"{bindingItinerary}; {returnString}";
			}
			return returnString;
		}

		protected override ZString GetBoxS28SealsNumberForPhase5()
		{
			var sealCount = NctsHeader.DepartureHeaderContainers.Cast<EU.NCTS.Business.NctsDepartureHeaderContainer>().Sum(p => p.TotalSealCount);
			var sb = new ZStringBuilder(sealCount.ToString());
			var seals = NctsHeader.HeaderContainersSeals;
			foreach (var seal in seals)
			{
				sb.AppendIfNotEmpty(seal);
			}
			return sb.ToStringWithDelimiterBetweenAppends("; ").TrimEnd();
		}

		const string BindingItineraryYes = "1";
		const string BindingItineraryNo = "0";
	}
}
