using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocConfirmDivot : DocBaseWrapper
	{
		protected DocConfirmDivot(CommonConfirmDivot divot, BusinessObjectFactory factoryToWrap)
			: base(divot, factoryToWrap)
		{
		}

		public static DocConfirmDivot New(CommonConfirmDivot divot, BusinessObjectFactory factoryToWrap)
		{
			return new DocConfirmDivot(divot, factoryToWrap);
		}

		public CommonConfirmDivot Divot
		{
			get { return (CommonConfirmDivot)WrappedObject; }
		}

		public DocPackLines PackLine
		{
			get { return DocPackLines.New(Divot.PackLine, Factory); }
		}

		public ZInt PackagesDelivered
		{
			get { return Divot.J8_PackagesDelivered; }
		}
	}
}
