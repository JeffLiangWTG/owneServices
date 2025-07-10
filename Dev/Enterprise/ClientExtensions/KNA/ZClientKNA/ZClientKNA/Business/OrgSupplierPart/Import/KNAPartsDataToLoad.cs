using CargoWise.Types;

namespace Enterprise.Client.KNA.Business
{
	public class KNAPartsDataToLoad : Customs.AU.Declaration.Business.AUOrgSupplierPartDataLoad.PartsDataToLoad
	{
		public KNAPartsDataToLoad() : base() { }

		public ZString DumpingCountryOfExport;		// DCX
		public ZString DMP;
		public ZString DumpingRateOfExchange;		// DRE
		public ZString DumpingExemptionType;		// DXT
		public ZString DumpingSpecificationNumber;// DSN
		public ZString DXP;
		public ZString GSTE;
		public ZString ICN;
		public ZString InvoiceLineAdj;				// ADJ
		public ZString TAN;
		public ZString RNO;
		public ZString TILV;
		public ZString TRN;
		public ZString VAN;
		public ZString WRL;
		public ZString WET;
		public ZString WETQ;
		public ZString WETE;
	}
}
