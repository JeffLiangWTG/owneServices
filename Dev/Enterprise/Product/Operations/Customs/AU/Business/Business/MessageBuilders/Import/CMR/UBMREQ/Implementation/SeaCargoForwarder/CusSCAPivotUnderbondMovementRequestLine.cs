using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotUnderbondMovementRequestLine : IUnderbondMovementRequestLine
	{
		public CusSCAPivotUnderbondMovementRequestLine(CusSCAPivot pivot)
		{
			this.pivot = pivot;
		}

		#region IUnderbondMovementRequestLine Members

		public ZString ContainerNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (pivot.Container != null)
				{
					if (!pivot.Container.IsBreakBulk && !pivot.Container.IsBulk)
					{
						result = pivot.Container.CN_ContainerNumber;
					}
				}
				return result;
			}
		}

		public ZString HouseAirWaybillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString MasterAirWaybillNumber
		{
			get { return ZString.Empty; }
		}

		public ZString OceanBillOfLading
		{
			get { return pivot.HouseBill != null && pivot.OceanBill.CB_MultiOBLUnpack ? pivot.HouseBill.CA_HouseBill : pivot.OceanBill.CB_OceanBill; }
		}

		public ZString HouseBillOfLading
		{
			get { return pivot.HouseBill == null || pivot.OceanBill.CB_MultiOBLUnpack ? ZString.Empty : pivot.HouseBill.CA_HouseBill; }
		}

		public ZString UniqueConsignmentReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public ZInt NumberOfPackages
		{
			get { return pivot.CV_PackageCount; }
		}

		public ZString PackageType
		{
			get { return pivot.CV_PackageType; }
		}

		public ZString ImportCargoType
		{
			get { return pivot.Container != null ? pivot.Container.CN_ContainerMode : ZString.Empty; }
		}

		#endregion

		#region Implementation

		readonly CusSCAPivot pivot;

		#endregion
	}
}
