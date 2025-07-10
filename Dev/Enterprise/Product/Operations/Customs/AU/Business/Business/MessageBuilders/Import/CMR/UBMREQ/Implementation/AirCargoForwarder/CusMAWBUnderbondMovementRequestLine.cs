using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBUnderbondMovementRequestLine : IUnderbondMovementRequestLine
	{
		public CusMAWBUnderbondMovementRequestLine(CusMAWBBase mAWB)
		{
			Argument.NotNull(mAWB, "MAWB");
			this.mAWB = mAWB;
		}

		public ZString ContainerNumber
		{
			get { return ZString.Empty; }
		}

		public ZString HouseBillOfLading
		{
			get { return ZString.Empty; }
		}

		public virtual ZString HouseAirWaybillNumber
		{
			get { return mAWB.CM_MasterHouseBill; }
		}

		public ZString OceanBillOfLading
		{
			get { return ZString.Empty; }
		}

		public virtual ZString MasterAirWaybillNumber
		{
			get { return mAWB.CM_MAWB; }
		}

		public ZString UniqueConsignmentReferenceNumber
		{
			get { return ZString.Empty; }
		}

		public virtual ZInt NumberOfPackages
		{
			get { return mAWB is CusMAWB ? ((CusMAWB)mAWB).PiecesManifestedForAllHAWBs : ZInt.Zero; }
		}

		public ZString PackageType
		{
			get { return ZString.Empty; }
		}

		public ZString ImportCargoType
		{
			get { return ZString.Empty; }
		}

		readonly CusMAWBBase mAWB;
	}
}
