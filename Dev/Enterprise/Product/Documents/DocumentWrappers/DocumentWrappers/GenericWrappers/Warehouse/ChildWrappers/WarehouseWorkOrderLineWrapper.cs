using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers
{
	public class WarehouseWorkOrderLineWrapper : WarehouseDocketLineWrapper
	{
		public WarehouseWorkOrderLineWrapper(WhsWorkOrderLine whsWorkOrderLine, BusinessObjectFactory factory)
			: base(whsWorkOrderLine, factory)
		{
		}

		#region Properties

		protected override ZString StagingAreaNameCore
		{
			get
			{
				var stagingLocationInfo = ZString.Empty;
				if (WorkOrderLine.StagingLocationBOM != null)
				{
					stagingLocationInfo = WorkOrderLine.StagingLocationBOM.WLV_LocationString;
				}
				return stagingLocationInfo;
			}
		}

		protected override ZString IsTopLevelOrEvenIndexCore
		{
			get { return (BOMLevel == 0 || Index % 2 == 0) ? "Y" : "N"; }
		}

		protected override ZString IsTopLevelOrOddIndexCore
		{
			get { return (BOMLevel == 0 || Index % 2 == 1) ? "Y" : "N"; }
		}

		public override MultilingualString Attributes
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(PartAttribute1WithLabel);
				result.AppendIfNotEmpty(PartAttribute2WithLabel);
				result.AppendIfNotEmpty(PartAttribute3WithLabel);
				result.AppendIfNotEmpty(TrackedSerialWithLabel);
				return (NoResString)result.ToStringWithDelimiterBetweenAppends(",   ");
			}
		}

		#endregion

		internal WarehouseWorkOrderLineWrapperCollection ChildComponentLines
		{
			get
			{
				if (childComponentLines == null)
				{
					childComponentLines = new WarehouseWorkOrderLineWrapperCollection(Factory);
					foreach (WhsWorkOrderLine child in WorkOrderLine.BOM.ChildComponentLines)
					{
						childComponentLines.Add(new WarehouseWorkOrderLineWrapper(child, Factory));
					}
				}
				return childComponentLines;
			}
		}
		WarehouseWorkOrderLineWrapperCollection childComponentLines;

		#region Implementation

		public WhsWorkOrderLine WorkOrderLine
		{
			get { return workOrderLine ?? (workOrderLine = (WhsWorkOrderLine)WrappedBO); }
		}
		WhsWorkOrderLine workOrderLine;

		#endregion
	}
}
