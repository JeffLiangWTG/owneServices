using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailCargoListReportLine : ICargoListReportLine
	{
		public CusSeaManOBLDetailCargoListReportLine(CusSeaManOBLDetailCargoLine detail)
		{
			this.detail = detail;
		}

		public ZGuid PK
		{
			get { return detail.PK; }
		}

		public ZString CargoCode
		{
			get { return detail.Header.BO_HeaderCargoType; }//C, E, X
		}

		public ZString CargoIdentifier
		{
			get { return detail.BD_ContainerNumber; }
		}

		public ZString PortOfDestination
		{
			get { return detail.Header.BO_RL_NKDestinationPort; }
		}

		public ZString PortOfLoading
		{
			get { return detail.Header.BO_RL_NKLoadPort; }
		}

		public ZString ImportCargoType
		{
			get { return detail.BD_LineCargoType; }
		}

		public ZInt NumberOfPackages
		{
			get { return detail.BD_NoOfPacks; }
		}

		public ZString PackageType
		{
			get { return detail.BD_PackType; }
		}

		#region Implementation

		readonly CusSeaManOBLDetailCargoLine detail;

		#endregion
	}
}
