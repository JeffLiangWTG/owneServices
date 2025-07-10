using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsWorkOrderLine : DocWhsDocketLine
	{
		#region Static

		public static DocWhsWorkOrderLine New(WhsWorkOrderLine whsWorkOrderLine, BusinessObjectFactory factoryToWrap)
		{
			return (whsWorkOrderLine == null) ? null : new DocWhsWorkOrderLine(whsWorkOrderLine, factoryToWrap);
		}

		#endregion

		#region Constructors

		DocWhsWorkOrderLine(WhsWorkOrderLine whsWorkOrderLine, BusinessObjectFactory factoryToWrap)
			: base(whsWorkOrderLine, factoryToWrap)
		{
		}

		#endregion

		#region Properties

		public ZString IsBOMTopLevelProduct
		{
			get { return WhsWorkOrderLine.BOM.IsTopLevelProduct ? "Y" : "N"; }
		}

		public ZInt BomLevel
		{
			get { return (ZInt)WhsWorkOrderLine.WE_Level; }
		}

		public ZString IsTopLevelOrOddIndex
		{
			get { return (BomLevel == 0 || Index % 2 == 1) ? "Y" : "N"; }
		}

		public ZString IsTopLevelOrEvenIndex
		{
			get { return (BomLevel == 0 || Index % 2 == 0) ? "Y" : "N"; }
		}

		public ZString BomIndentation
		{
			get { return new ZString(' ', BomLevel * indentation); }
		}
		internal const int indentation = 9;

		public ZString RelatedDocketId { get; set; }

		public ZString Attributes
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(PartAttribute1WithLabel);
				result.AppendIfNotEmpty(PartAttribute2WithLabel);
				result.AppendIfNotEmpty(PartAttribute3WithLabel);
				result.AppendIfNotEmpty(TrackedSerialWithLabel);
				return result.ToStringWithDelimiterBetweenAppends(",   ");
			}
		}

		internal ZInt Index { get; set; }

		#endregion

		#region Related Business Objects

		public override DocWhsDocket Docket
		{
			get { return DocWhsWorkOrder.New((WhsWorkOrder)WhsWorkOrderLine.Docket, Factory); }
		}

		internal WhsWorkOrderLine WhsWorkOrderLine
		{
			get { return (WhsWorkOrderLine)WrappedObject; }
		}

		internal DocWhsWorkOrderLineCollection ChildComponentLines
		{
			get
			{
				if (childComponentLines == null)
				{
					childComponentLines = new DocWhsWorkOrderLineCollection(Factory);
					foreach (WhsWorkOrderLine child in WhsWorkOrderLine.BOM.ChildComponentLines)
					{
						childComponentLines.Add(new DocWhsWorkOrderLine(child, Factory));
					}
				}
				return childComponentLines;
			}
		}
		DocWhsWorkOrderLineCollection childComponentLines;

		#endregion
	}
}
