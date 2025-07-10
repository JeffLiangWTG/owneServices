using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailUnderbondMovementRequestLine : IUnderbondMovementRequestLine
	{
		public CusSeaManOBLDetailUnderbondMovementRequestLine(CusSeaManOBLDetail detail)
		{
			this.detail = detail;
		}

		#region IUnderbondMovementRequestLine Members

		public ZString ContainerNumber
		{
			get
			{
				if (detail.BD_LineCargoType == CMRCargoTypes.Codes.BreakBulk || detail.BD_LineCargoType == CMRCargoTypes.Codes.Bulk)
				{
					return ZString.Empty;
				}
				return detail.BD_ContainerNumber;
			}
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
			get
			{
				if (detail.BD_LineCargoType == CMRCargoTypes.Codes.BreakBulk || detail.BD_LineCargoType == CMRCargoTypes.Codes.Bulk)
				{
					return detail.Header.BO_OceanBill;
				}
				return ZString.Empty;
			}
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
			get { return detail.BD_NoOfPacks; }
		}

		public ZString PackageType
		{
			get { return detail.BD_PackType; }
		}

		public ZString ImportCargoType
		{
			get { return detail.BD_LineCargoType; }
		}

		#endregion

		#region Implementation

		readonly CusSeaManOBLDetail detail;

		#endregion
	}
}
