using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocPackLocation : DocBaseWrapper
	{
		#region Constructors and Type Override
		protected DocPackLocation(PackLocation packLocation, BusinessObjectFactory factoryToWrap)
			: base(packLocation, factoryToWrap)
		{
		}

		public static DocPackLocation New(PackLocation packLocation, BusinessObjectFactory factoryToWrap)
		{
			DocPackLocation result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(packLocation, factoryToWrap);
			}
			else if (packLocation != null)
			{
				result = new DocPackLocation(packLocation, factoryToWrap);
			}
			return result;
		}

		protected delegate DocPackLocation NewDelegate(PackLocation packLocation, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}
		#endregion

		public ZDateTime DeliveredDate
		{
			get
			{
				return PackLocation.JQ_DeliveredDate;
			}
		}

		public ZInt NoPackages
		{
			get
			{
				return PackLocation.JQ_NoPackages;
			}
		}

		public ZDecimal Volume
		{
			get
			{
				return PackLocation.JQ_Volume;
			}
		}

		public ZString VolumeUV
		{
			get
			{
				return PackLocation.JQ_VolumeUV;
			}
		}

		public ZString WarehouseLocation
		{
			get
			{
				return PackLocation.JQ_WarehouseLocation;
			}
		}

		public ZDecimal Weight
		{
			get
			{
				return PackLocation.JQ_Weight;
			}
		}

		public ZString WeightUW
		{
			get
			{
				return PackLocation.JQ_WeightUW;
			}
		}

		#region Implementation

		PackLocation PackLocation
		{
			get
			{
				return (PackLocation)WrappedObject;
			}
		}

		#endregion
	}
}
