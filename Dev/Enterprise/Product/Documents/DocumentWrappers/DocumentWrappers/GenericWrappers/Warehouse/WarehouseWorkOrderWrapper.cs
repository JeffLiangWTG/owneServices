using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers.Warehouse
{
	public class WarehouseWorkOrderWrapper : WarehousePickableDocketWrapper
	{
		public WarehouseWorkOrderWrapper(WhsWorkOrder whsWorkOrder, BusinessObjectFactory factory)
			: base(whsWorkOrder, factory)
		{
		}

		#region Lines

		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			var result = new WarehouseWorkOrderLineWrapperCollection(Factory);

			foreach (WhsWorkOrderLine line in WhsWorkOrder.Lines)
			{
				result.Add(new WarehouseWorkOrderLineWrapper(line, Factory));
			}

			return result;
		}

		protected override WarehouseWorkOrderLineWrapperCollection NewWorkOrderLineWrapperCollection()
		{
			if (workOrderMatrixLines == null)
			{
				workOrderMatrixLines = new WarehouseWorkOrderLineWrapperCollection(Factory);
				if (JobLines.Count > 0)
				{
					index = 1;
					AddParentLinesToCollection((WarehouseWorkOrderLineWrapperCollection)JobLines, workOrderMatrixLines);
				}
			}
			return workOrderMatrixLines;
		}
		WarehouseWorkOrderLineWrapperCollection workOrderMatrixLines;
		ZInt index;

		public ZString WorkOrderLevelDescription(int level)
		{
			ZString result = ZString.Empty;
			if (WorkOrderLevels != null && level <= WorkOrderLevels.Count)
			{
				result = WorkOrderLevels[level - 1];
			}
			return result;
		}

		List<ZString> WorkOrderLevels
		{
			get
			{
				if (workOrderLevels == null && JobLines != null && JobLines.Count > 0)
				{
					int maxLevels = Math.Min(WorkOrderLines.Max(l => (int)((WarehouseWorkOrderLineWrapper)l).BOMLevel) + 1, DisplayLevelLimit);
					workOrderLevels = new List<ZString>(maxLevels);
					workOrderLevels.Add(Res.GetString("6563058c-fa35-4222-9c89-b097f43e7a26", "Level {0}", WhsWorkOrder.BOM.Level));
					for (int idx = 1; idx < maxLevels; idx++)
					{
						workOrderLevels.Add(new String(' ', (int)(0.5 * idx * WarehouseWorkOrderLineWrapper.indentation)) + Res.GetString("92433751-3037-46ef-af6a-718b4d1e1191", "Level {0}", idx + WhsWorkOrder.BOM.Level));
					}
				}
				return workOrderLevels;
			}
		}
		List<ZString> workOrderLevels;

		const int DisplayLevelLimit = 10;

		#endregion

		#region Properties

		protected override ZString WorkOrderLevels1stCore => WorkOrderLevelDescription(1);

		protected override ZString WorkOrderLevels2ndCore => WorkOrderLevelDescription(2);

		protected override ZString WorkOrderLevels3rdCore => WorkOrderLevelDescription(3);

		protected override ZString WorkOrderLevels4thCore => WorkOrderLevelDescription(4);

		protected override ZString WorkOrderLevels5thCore => WorkOrderLevelDescription(5);

		protected override ZString WorkOrderLevels6thCore => WorkOrderLevelDescription(6);

		protected override ZString WorkOrderLevels7thCore => WorkOrderLevelDescription(7);

		protected override ZString WorkOrderLevels8thCore => WorkOrderLevelDescription(8);

		protected override ZString WorkOrderLevels9thCore => WorkOrderLevelDescription(9);

		protected override ZString WorkOrderLevels10thCore => WorkOrderLevelDescription(10);

		protected override LabelValuePairWrapper SecondaryReferenceCore
		{
			get
			{
				LabelValuePairWrapper result = new LabelValuePairWrapper(Factory);
				if (DocketBO != null)
				{
					result = new LabelValuePairWrapper(Res.GetString("cd6d96c0-2990-4f29-8f67-fc64187bb128", "Work Order No."), DocketBO.WD_ExternalReference, Factory);
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		WhsWorkOrder WhsWorkOrder => (WhsWorkOrder)WrappedObject;

		void AddParentLinesToCollection(WarehouseWorkOrderLineWrapperCollection workOrderLines, WarehouseWorkOrderLineWrapperCollection result)
		{
			WarehouseWorkOrderLineWrapperCollection parentLines = new WarehouseWorkOrderLineWrapperCollection(Factory);
			parentLines.AddRange(workOrderLines.Where(l => ((WarehouseWorkOrderLineWrapper)l).WorkOrderLine.BOM.IsTopLevelProduct));
			if (parentLines.Count > 0)
			{
				AddChildLinesToCollection(parentLines, result);
			}
		}

		void AddChildLinesToCollection(WarehouseWorkOrderLineWrapperCollection lines, WarehouseWorkOrderLineWrapperCollection result)
		{
			foreach (WarehouseWorkOrderLineWrapper line in lines)
			{
				line.Index = index++;
				result.Add(line);

				if (line.WorkOrderLine.WE_Level < DisplayLevelLimit - 1 && line.ChildComponentLines.Count > 0)
				{
					AddChildLinesToCollection(line.ChildComponentLines, result);
				}
			}
		}

		#endregion
	}
}
