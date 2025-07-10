using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Module;

namespace Enterprise.Customs.AE.Manifest.Module;

public partial class AEManifestBillFilterStripControl : ASYCUDAManifestBillFilterStripControl
{
	public AEManifestBillFilterStripControl() { }

	public AEManifestBillFilterStripControl(IBusinessObjectCollection gridCollection, AEManifestBillFilterStrip billFilterStripBusinessObject)
		: base(gridCollection, billFilterStripBusinessObject)
	{
		InitializeComponent();
		AddColumns();
	}

	public static class ResourceStrings
	{
		public static ResourceStringData SplitBillNumber => Res.GetData("FEE01D43-C6DC-4A19-B622-C0E4F72AFEB7", "Split Bill Number");
	}

	void AddColumns()
	{
		this.grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = ResourceStrings.SplitBillNumber,
			ColumnName = AsycudaBill.Schema.ABL_SplitBillNumber,
			IsVisible = true,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});
	}
}
