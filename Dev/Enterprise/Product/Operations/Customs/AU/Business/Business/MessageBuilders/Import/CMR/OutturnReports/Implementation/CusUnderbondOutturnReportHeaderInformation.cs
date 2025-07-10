using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusUnderbondOutturnReportHeaderInformation : IOutturnReportHeaderInformation
	{
		public CusUnderbondOutturnReportHeaderInformation(CusUnderbond underbond)
		{
			this.Underbond = underbond;
		}

		public ZString ResponsiblePartyID
		{
			get { return Underbond.AU_OutturnResponsiblePartyID; }
		}

		public ZString EstablishmentID
		{
			get { return Underbond.C4_DestinationPremiseID; }
		}

		protected readonly CusUnderbond Underbond;
	}
}
