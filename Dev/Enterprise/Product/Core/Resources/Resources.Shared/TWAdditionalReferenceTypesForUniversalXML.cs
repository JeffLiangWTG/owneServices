using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Core
{
	public static class TWAdditionalReferenceTypesForUniversalXML
	{
		#region Codes

		public abstract class Codes
		{
			public const string TransitWarehouseReceive = "TWR";
			public const string TransitWarehouseReceiveASN = "TRA";
			public const string TransitWarehouseDispatch = "TWD";
			public const string TransitWarehouseReceiveReference = "TRE";
			public const string TransitWarehouseDispatchReference = "TDE";
			public const string TransitWarehouseReceiveTransportationUnit = "TRU";
			public const string TransitWarehouseDispatchTransportationUnit = "TDU";
			public const string TransitWarehouseDispatchLoadList = "TDL";
		}

		#endregion

		#region Descriptions

		#region SuppressResourceStringsCheckRegion

		[CodeAlive("Provides a shared list of Additional Reference Type Descriptions used in Transit Warehouse")]
		public abstract class Descriptions
		{
			public const string TransitWarehouseReceive = "Transit Warehouse Receive";
			public const string TransitWarehouseReceiveASN = "Transit Warehouse ReceiveASN";
			public const string TransitWarehouseDispatch = "Transit Warehouse Dispatch";
			public const string TransitWarehouseReceiveReference = "Transit Warehouse Receive Reference";
			public const string TransitWarehouseDispatchReference = "Transit Warehouse Dispatch Reference";
			public const string TransitWarehouseReceiveTransportationUnit = "Transit Warehouse Receive Transportation Unit";
			public const string TransitWarehouseDispatchTransportationUnit = "Transit Warehouse Dispatch Transportation Unit";
			public const string TransitWarehouseDispatchLoadList = "Transit Warehouse Dispatch Load List";
		}

		#endregion

		#endregion
	}
}
