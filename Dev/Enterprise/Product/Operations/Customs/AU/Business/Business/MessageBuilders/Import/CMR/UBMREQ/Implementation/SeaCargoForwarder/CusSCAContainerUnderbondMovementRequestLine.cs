using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerUnderbondMovementRequestLine : IUnderbondMovementRequestLine
	{
		public CusSCAContainerUnderbondMovementRequestLine(CusSCAContainer container)
		{
			this.container = container;
		}

		#region IUnderbondMovementRequestLine Members

		public ZString ContainerNumber
		{
			get { return container.CN_ContainerNumber; }
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
			get { return ZString.Empty; }
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
			get { return CMRCargoTypes.Codes.FullContainerLoad; }
		}

		#endregion

		#region Implementation

		readonly CusSCAContainer container;

		#endregion
	}
}
