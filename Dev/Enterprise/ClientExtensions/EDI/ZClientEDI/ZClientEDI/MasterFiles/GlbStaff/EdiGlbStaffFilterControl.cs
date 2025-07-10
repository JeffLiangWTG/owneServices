using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Module;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class EdiGlbStaffFilterControl : GlbStaffFilterControl
	{
		public EdiGlbStaffFilterControl()
			: base()
		{
			AddDomesticNameColumn();
		}

		public EdiGlbStaffFilterControl(IBusinessObjectCollection gridCollection, EdiGlbStaffFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			AddDomesticNameColumn();
		}

		void AddDomesticNameColumn()
		{
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIGlbStaff)(null)).DomesticName);

			var domesticNameTextBoxColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			domesticNameTextBoxColumn.CaptionResourceString = ZClientEDI.Res.GetData("DFBE06E2-9868-4FAA-9B25-6C03CD057BCC", "Domestic Name");
			domesticNameTextBoxColumn.ColumnName = "DomesticName";
			domesticNameTextBoxColumn.IsVisible = false;
			domesticNameTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			grid.ColumnStyles.Add(domesticNameTextBoxColumn);
		}
	}
}
