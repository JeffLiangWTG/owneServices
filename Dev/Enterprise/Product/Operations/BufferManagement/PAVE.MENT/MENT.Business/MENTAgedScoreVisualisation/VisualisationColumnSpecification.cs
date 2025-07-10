using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.PAVE.MENT.Business
{
	public class VisualisationColumnSpecification : ColumnSpecification
	{
		public VisualisationColumnSpecification(MENTAgedScoreVisualisation parent)
			: base(parent.Factory)
		{
		}

		[XmlColumnProperty]
		[ResourceStringData("VisualisationColumnSpecification.ColumnDisplay", Caption = "Column Display", FullDescription = "The value to display instead of the column value")]
		public ZString ColumnDisplay
		{
			get { return GetXmlColumnPropertyValue<ZString>(ColumnDisplayInfo); }
			set { SetXmlColumnPropertyValue(ColumnDisplayInfo, value); }
		}

		public ZPropertyInfo ColumnDisplayInfo
		{
			get { return GetZPropertyInfo(nameof(ColumnDisplay)); }
		}
	}
}
