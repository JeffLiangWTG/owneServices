using CargoWise.EntityFramework;
using Enterprise.Customs.IE.PBN.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.PBN.Module
{
	public partial class PBNFilterStripControl : ASYCUDA.Module.AsycudaPreBoardingNotificationFilterStripControl
	{
		public PBNFilterStripControl()
		{
			InitializeComponent();
		}

		public PBNFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("4C2C582F-C088-4BA0-9D8F-6C487820A2CD", "PBN ID"),
				ColumnName = AsycudaManifestHeader.Schema.RegistrationNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});
		}
	}
}
