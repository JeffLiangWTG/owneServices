using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBasePkgPackage : DocBaseWrapper
	{
		protected DocBasePkgPackage(CusPackage pkgPackage, BusinessObjectFactory factoryToWrap)
			: base(pkgPackage, factoryToWrap)
		{
		}

		CusPackage Package => (CusPackage)WrappedObject;

		#region ZDecimal Properties

		public ZDecimal Weight => Package.KP_Weight;

		public ZDecimal Length => Package.KP_Length;

		public ZDecimal Width => Package.KP_Width;

		public ZDecimal Height => Package.KP_Height;

		public ZDecimal Volume => Package.KP_Volume;

		public ZDecimal NetWeight => Package.NetWeight;

		#endregion

		#region ZInt Properties

		public ZInt PackageQty => Package.KP_PackageQty;

		public ZInt PackageSequence => Package.KP_Sequence;

		#endregion

		#region ZString Properties

		public ZString WeightUQ => Package.KP_WeightUQ;

		public ZString DimensionUQ => Package.KP_DimensionUQ;

		public ZString VolumeUQ => Package.KP_VolumeUQ;

		public ZString PackType => Package.KP_F3_NKPackType;

		public ZString MarksAndNumbers => Package.KP_MarksAndNumbers;

		public ZString Summary => Package.KP_GoodsDescription;

		#endregion
	}
}
