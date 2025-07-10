using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.Visualisation
{
	class RendererGroupByArea : RendererSectionBody
	{
		public RendererGroupByArea(GroupByArea groupByArea)
			: base(groupByArea.SectionBody)
		{
			GroupByArea = groupByArea;
		}
		readonly GroupByArea GroupByArea;

		protected override Area AreaContainingFields
		{
			get { return GroupByArea; }
		}

		public override int GetHeightInXL()
		{
			return 0;
		}

		protected override List<VisualiserComponent> CreateComponentsAndDataContainers(ExcelWorkSheet excelWorkSheet, VisualiserDataSet visualiserDataSet, int horizontalOffset, List<string> addedDataTableNames)
		{
			foreach (string fieldName in GroupByArea.GroupByColumns)
			{
				var unscalesFieldLengthsetHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(fieldName.Length);
				AddColumnToDataTableIfNotAddedAlready(visualiserDataSet, fieldName, new Enterprise.DocumentEngineIntegration.CellFormat(), ControlDpiScalingHelper.NewScaledSize(unscalesFieldLengthsetHeight, 15));
			}
			return base.CreateComponentsAndDataContainers(excelWorkSheet, visualiserDataSet, horizontalOffset, addedDataTableNames);
		}
	}
}
