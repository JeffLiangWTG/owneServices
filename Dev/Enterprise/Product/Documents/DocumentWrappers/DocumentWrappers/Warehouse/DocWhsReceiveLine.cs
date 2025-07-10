using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsReceiveLine : DocWhsDocketLine
	{
		#region Static

		public static DocWhsReceiveLine New(WhsReceiveLine whsReceiveLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsReceiveLine == null) ? null : new DocWhsReceiveLine(whsReceiveLine, factoryToWrap);
		}

		#endregion

		#region Constructors

		DocWhsReceiveLine(WhsReceiveLine whsReceiveLine, BusinessObjectFactory factoryToWrap)
			: base(whsReceiveLine, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		WhsReceiveLine WhsReceiveLine
		{
			get { return (WhsReceiveLine)WrappedObject; }
		}

		public override DocWhsDocket Docket
		{
			get { return WhsReceiveLine.DocketType == typeof(WhsReceive) ? DocWhsReceive.New(WhsReceiveLine.Docket, Factory) : null; }
		}

		#endregion
	}
}
