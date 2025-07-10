using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsWorkOrder : DocWhsPickableDocket
	{
		#region Static

		public static DocWhsWorkOrder New(WhsWorkOrder whsWorkOrder, BusinessObjectFactory factoryToWrap)
		{
			return (whsWorkOrder == null) ? null : new DocWhsWorkOrder(whsWorkOrder, factoryToWrap);
		}

		public new static DocWhsWorkOrder New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsWorkOrder(docketLabel, factoryToWrap));
		}

		#endregion

		#region Contructors

		DocWhsWorkOrder(WhsWorkOrder whsWorkOrder, BusinessObjectFactory factoryToWrap)
			: base(whsWorkOrder, factoryToWrap)
		{
		}

		DocWhsWorkOrder(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		#endregion

		#region Properties

		// Primary collection of all docket lines.
		public DocWhsWorkOrderLineCollection WorkOrderLines
		{
			get
			{
				if (workOrderMatrixLines == null)
				{
					workOrderMatrixLines = new DocWhsWorkOrderLineCollection(Factory);
					if (Lines.Count > 0)
					{
						index = 1;
						AddParentLinesToCollection(Lines, workOrderMatrixLines);
					}
				}
				return workOrderMatrixLines;
			}
		}
		DocWhsWorkOrderLineCollection workOrderMatrixLines;

		#region Work Order Level Heirarchy Tree

		public ZString WorkOrderLevels1st
		{
			get { return WorkOrderLevelDescription(1); }
		}

		public ZString WorkOrderLevels2nd
		{
			get { return WorkOrderLevelDescription(2); }
		}

		public ZString WorkOrderLevels3rd
		{
			get { return WorkOrderLevelDescription(3); }
		}

		public ZString WorkOrderLevels4th
		{
			get { return WorkOrderLevelDescription(4); }
		}

		public ZString WorkOrderLevels5th
		{
			get { return WorkOrderLevelDescription(5); }
		}

		public ZString WorkOrderLevels6th
		{
			get { return WorkOrderLevelDescription(6); }
		}

		public ZString WorkOrderLevels7th
		{
			get { return WorkOrderLevelDescription(7); }
		}

		public ZString WorkOrderLevels8th
		{
			get { return WorkOrderLevelDescription(8); }
		}

		public ZString WorkOrderLevels9th
		{
			get { return WorkOrderLevelDescription(9); }
		}

		public ZString WorkOrderLevels10th
		{
			get { return WorkOrderLevelDescription(10); }
		}

		#endregion

		public override ZString SubTypeDesc
		{
			get { return base.SubTypeDesc.ToUpper(); }
		}

		#endregion

		#region Implementation

#if DEBUG
		internal
#endif
		ZString WorkOrderLevelDescription(int level)
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
				if (workOrderLevels == null && WorkOrderLines != null && WorkOrderLines.Count > 0)
				{
					int maxLevels = Math.Min(WorkOrderLines.Max(l => (int)((DocWhsWorkOrderLine)l).BomLevel) + 1, DisplayLevelLimit);
					workOrderLevels = new List<ZString>(maxLevels);
					workOrderLevels.Add(Res.GetString("cb2a5323-be9e-418f-b3ed-a1181069fb37", "Level {0}", WhsWorkOrder.BOM.Level));
					for (int idx = 1; idx < maxLevels; idx++)
					{
						workOrderLevels.Add(new String(' ', idx * DocWhsWorkOrderLine.indentation) + Res.GetString("d2b6a22d-8ec7-41a3-8913-c2698a9d618d", "Level {0}", idx + WhsWorkOrder.BOM.Level));
					}
				}
				return workOrderLevels;
			}
		}
		List<ZString> workOrderLevels;

		void AddParentLinesToCollection(DocWhsDocketLineCollection workOrderLines, DocWhsWorkOrderLineCollection result)
		{
			DocWhsWorkOrderLineCollection parentLines = new DocWhsWorkOrderLineCollection(Factory);
			parentLines.AddRange(workOrderLines.Where(l => ((DocWhsWorkOrderLine)l).WhsWorkOrderLine.BOM.IsTopLevelProduct));
			if (parentLines.Count > 0)
			{
				AddChildLinesToCollection(parentLines, result);
			}
		}

		void AddChildLinesToCollection(DocWhsWorkOrderLineCollection lines, DocWhsWorkOrderLineCollection result)
		{
			foreach (DocWhsWorkOrderLine line in lines)
			{
				line.Index = index++;
				result.Add(line);

				if (line.WhsWorkOrderLine.WE_Level < DisplayLevelLimit - 1 && line.ChildComponentLines.Count > 0)
				{
					AddChildLinesToCollection(line.ChildComponentLines, result);
				}
			}
		}
		const int DisplayLevelLimit = 10;

		#endregion

		#region Related Business Objects

		WhsWorkOrder WhsWorkOrder
		{
			get { return (WhsWorkOrder)WrappedObject; }
		}

		#endregion

		#region Business Objects Overrides

		protected override DocWhsDocketLineCollection GetDocketLines()
		{
			return new DocWhsWorkOrderLineCollection(WhsWorkOrder.Lines, Factory);
		}

		#endregion

		int index;
	}
}
