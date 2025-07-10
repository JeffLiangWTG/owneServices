using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class Order : IDataObject
	{
		public Order()
		{
		}

		public Order(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(35), Mandatory]
		public ZString? OrderNumber { get; set; }
		public ZByte? OrderNumberSplit { get; set; }
		public CodeDescriptionPair Type { get; set; }
		public ZDecimal? TotalUnits { get; set; }
		public CodeDescriptionPair Status { get; set; }
		public CodeDescriptionPair SalesChannel { get; set; }
		public Warehouse Warehouse { get; set; }
		public CodeDescriptionPair PickOption { get; set; }
		[MaxLength(35)]
		public ZString? ClientReference { get; set; }
		[MaxLength(35)]
		public ZString? TransportReference { get; set; }
		[MaxLength(3)]
		public ZString? Category { get; set; }
		public DropMode DropMode { get; set; }
		public ZDecimal? LocalCartageInsuranceValue { get; set; }
		[MaxLength(25)]
		public ZString? StagingArea { get; set; }
		public CodeDescriptionPair FulfillmentRule { get; set; }
		public ZDecimal? UnitsSent { get; set; }
		public ZShort? PalletsSent { get; set; }
		public ZDecimal? TotalNetWeightSent { get; set; }
		public ZDecimal? TotalLineWeight { get; set; }
		public ZDecimal? TotalLineVolume { get; set; }
		public ZBool? AddPalletWeightToOrder { get; set; }
		public ZBool? RequiresQualityAudit { get; set; }
		public ZBool? RequiresPacking { get; set; }
		public ZBool? ExcludeFromTotePicking { get; set; }
		public ZBool? AutoFinaliseBOMIntoInventory { get; set; }
		public ZBool? IsInwardsProcessingJob { get; set; }
		public ZBool? UseDirectedPackingConsolidation { get; set; }
		public ZByte? PickPriority { get; set; }
		public ZBool? HoldPalletIDPutaway { get; set; }
		public ZBool? IsReleased { get; set; }
		public DataObjectList<OrderLine> OrderLineCollection { get; private set; }
		public List<Date> DateCollection { get; private set; }
	}
}
