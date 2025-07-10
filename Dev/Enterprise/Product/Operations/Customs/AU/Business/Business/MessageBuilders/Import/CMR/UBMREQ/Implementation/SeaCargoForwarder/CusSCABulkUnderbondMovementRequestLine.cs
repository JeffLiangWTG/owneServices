using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCABulkUnderbondMovementRequestLine : IUnderbondMovementRequestLine
	{
		public CusSCABulkUnderbondMovementRequestLine(CusSCAContainer container)
		{
			this.container = container;
		}

		#region IUnderbondMovementRequestLine Members

		public ZString ContainerNumber
		{
			get { return ZString.Empty; }
		}

		public ZString HouseBillOfLading
		{
			get { return ZString.Empty; }
		}

		public ZString HouseAirWaybillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString OceanBillOfLading
		{
			get { return container.OceanBill != null ? container.OceanBill.CB_OceanBill : ZString.Empty; }
		}

		public ZString MasterAirWaybillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString UniqueConsignmentReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public ZInt NumberOfPackages
		{
			get { return container.TotalPackages; }
		}

		public ZString PackageType
		{
			get { return container.MostPrevelantPackageType; }
		}

		public ZString ImportCargoType
		{
			get { return container.CN_ContainerMode; }
		}

		#endregion

		#region Implementation

		readonly CusSCAContainer container;

		#endregion
	}
}
