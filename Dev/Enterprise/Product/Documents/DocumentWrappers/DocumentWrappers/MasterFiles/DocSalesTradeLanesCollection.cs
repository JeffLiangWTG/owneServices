using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocSalesTradeLanesCollection : DocumentWrapperCollection
	{
		public DocSalesTradeLanesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocSalesTradeLanesCollection(BusinessObjectFactory factory, OrgHeader org)
			: base(factory)
		{
			this.org = org;
		}

		readonly OrgHeader org;

		public override void Load()
		{
			LoadTradeLanes();
		}

		void LoadTradeLanes()
		{
			var tradeLanes = new DynamicBusinessObjectCollection(Factory);

			var sqlQuery =
$@"SELECT 
	[ProductName], 
	[OriginCode],
	[DestinationCode], 
	[WarehouseUnloco],
	[IsTraded],
	[TradeMode],
	[TradeType],
	[Service],
	[WeightUQ],
	[VolumeUQ],
	[Currency],
	[ActivityDate],
	[Weight],
	[Volume],
	[TEU],
	[EstimatedRevenue]
FROM 
	dbo.ViewDocTradeLane
WHERE 
	PAS_OH_Client = @OrgPk";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@OrgPk", org.PK, OrgHeaderSchema.PK }
			};

			tradeLanes.Load(sqlQuery, sqlParams);

			foreach (DynamicBusinessObject tradeLane in tradeLanes)
			{
				var view = new ViewDocTradeLane()
				{
					ProductName = (ZString)tradeLane["ProductName"],
					OriginCode = (ZString)tradeLane["OriginCode"],
					DestinationCode = (ZString)tradeLane["DestinationCode"],
					WarehouseUnloco = (ZString)tradeLane["WarehouseUnloco"],
					IsTraded = (ZBool)tradeLane["IsTraded"],
					TradeMode = (ZString)tradeLane["TradeMode"],
					TradeType = (ZString)tradeLane["TradeType"],
					Service = (ZString)tradeLane["Service"],
					WeightUQ = (ZString)tradeLane["WeightUQ"],
					VolumeUQ = (ZString)tradeLane["VolumeUQ"],
					Currency = (ZString)tradeLane["Currency"],
					ActivityDate = (ZDateTime)tradeLane["ActivityDate"],
					Weight = (ZDecimal)tradeLane["Weight"],
					Volume = (ZDecimal)tradeLane["Volume"],
					TEU = (ZDecimal)tradeLane["TEU"],
					EstimatedRevenue = (ZDecimal)tradeLane["EstimatedRevenue"]
				};

				var docTradeLane = DocSalesTradeLane.New(view, Factory);
				Add(docTradeLane);
			}
		}

		public new DocSalesTradeLane this[int index]
		{
			get { return (DocSalesTradeLane)base[index]; }
		}
	}

	public class ViewDocTradeLane
	{
		public ZString ProductName { get; set; }
		public ZString OriginCode { get; set; }
		public ZString DestinationCode { get; set; }
		public ZString WarehouseUnloco { get; set; }
		public ZBool IsTraded { get; set; }
		public ZString TradeMode { get; set; }
		public ZString TradeType { get; set; }
		public ZString Service { get; set; }
		public ZDecimal Weight { get; set; }
		public ZString WeightUQ { get; set; }
		public ZDecimal Volume { get; set; }
		public ZString VolumeUQ { get; set; }
		public ZDecimal TEU { get; set; }
		public ZString Currency { get; set; }
		public ZDecimal EstimatedRevenue { get; set; }
		public ZDateTime ActivityDate { get; set; }
	}
}
