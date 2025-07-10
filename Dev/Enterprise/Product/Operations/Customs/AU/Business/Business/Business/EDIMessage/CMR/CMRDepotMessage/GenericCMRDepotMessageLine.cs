
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class GenericCMRDepotMessageLine<MessageT> : ICMRDepotMessageLine where MessageT : CMRCUSRESMessage, ICMRDepotMessage
	{
		public GenericCMRDepotMessageLine(MessageT message)
		{
			this.Message = message;
		}

		public ZInt NumberOfPackages
		{
			get { return GetNumberOfPackagesCore(); }
		}
		protected abstract ZInt GetNumberOfPackagesCore();

		public ZString PackageType
		{
			get { return GetPackageTypeCore(); }
		}
		protected abstract ZString GetPackageTypeCore();

		public ZString ContainerNumber
		{
			get { return GetContainerNumberCore(); }
		}
		protected abstract ZString GetContainerNumberCore();

		public ZString HouseBillNumber
		{
			get { return GetHouseBillNumberCore(); }
		}
		protected abstract ZString GetHouseBillNumberCore();

		public ZString OceanBillNumber
		{
			get { return GetOceanBillNumberCore(); }
		}
		protected abstract ZString GetOceanBillNumberCore();

		public ZString MarksAndNumbers
		{
			get { return GetMarksAndNumbersCore(); }
		}
		protected abstract ZString GetMarksAndNumbersCore();

		public ZString GoodsDescription
		{
			get { return GetGoodsDescriptionCore(); }
		}
		protected abstract ZString GetGoodsDescriptionCore();

		public ZString ContainerMode
		{
			get { return GetContainerModeCore(); }
		}
		protected abstract ZString GetContainerModeCore();

		ICMRDepotMessage ICMRDepotMessageLine.Parent
		{
			get { return Message; }
		}

		public ZDecimal ActualWeight
		{
			get { return GetActualWeightCore(); }
		}
		protected abstract ZDecimal GetActualWeightCore();

		public ZString WeightUnits
		{
			get { return GetWeightUnitsCore(); }
		}
		protected abstract ZString GetWeightUnitsCore();

		public ZDecimal ActualVolume
		{
			get { return GetActualVolumeCore(); }
		}
		protected abstract ZDecimal GetActualVolumeCore();

		public ZString VolumeUnits
		{
			get { return GetVolumeUnitsCore(); }
		}
		protected abstract ZString GetVolumeUnitsCore();

		protected readonly MessageT Message;
	}
}
