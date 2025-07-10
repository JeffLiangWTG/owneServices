using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CustomsCusContainersWithTrackingAndAdditionalSealUserControl : Customs.GUI.BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsCusContainersWithTrackingAndAdditionalSealUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			BindingSource.SetBindingMember(AdditionalSealsGrid, nameof(CusContainer.AdditionalSeals));
			base.SetDataBinding(dataSource, dataMember);
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set
			{
				base.JobDeclaration = value;
				HandleVisibilityWhenJE_MessageTypeIsChanged();
			}
		}

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
			base.UnHookControlVisibilityChangeEvents(declaration);
		}

		protected override Freight.GUI.ContainersUserControl GetContainerTrackingUserControl() => new ContainersUserControl();

		void JE_MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			HandleVisibilityWhenJE_MessageTypeIsChanged();
		}

		void HandleVisibilityWhenJE_MessageTypeIsChanged()
		{
			if (AdditionalSealsGroupBox is ZGroupBox additionalSealsGroupBox)
			{
				additionalSealsGroupBox.Visible = JobDeclaration?.AdditionalSealsRequired ?? false;
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			CusContainersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.ContainerControlCheckboxVisible, EUAddInfoSchema.Constants.ZG_IsControl);
			CusContainersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.ContainerUnloadedCheckboxVisible, EUAddInfoSchema.Constants.ZG_IsUnloaded);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			CusContainersBoundGrid.ColumnStyles.AddRange(
				new ZGridColumnInfo[]
				{
					new ZCheckBoxColumnStyleInfo()
					{
						ColumnName = EUAddInfoSchema.Constants.ZG_IsControl,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
					},
					new ZCheckBoxColumnStyleInfo()
					{
						ColumnName = EUAddInfoSchema.Constants.ZG_IsUnloaded,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
					},
				}
			);
		}
	}
}
