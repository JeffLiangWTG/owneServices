using CargoWise.Windows.UI;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class CommunicationStatusRegistryControl : RegistryZUserControl
	{
		public CommunicationStatusRegistryControl()
		{
			InitializeComponent();
		}

		#region Columns

		public void SetupColumns(string boolColumnCaption)
		{
			this.boolColumnCaption = boolColumnCaption;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var dataSourceForBinding = dataSource as CommunicationStatusCollection;
			base.SetDataBinding(dataSourceForBinding, dataMember);

			if (dataSourceForBinding != null)
			{
				SetBoolColumnLayout();
			}
		}

		void SetBoolColumnLayout()
		{
			var column = CodeDescriptionBoolGrid.Columns["Bool"];

			if (column != null)
			{
				var columnStyle = column.ColumnStyle;
				columnStyle.HeaderText = boolColumnCaption;

				using (var graphic = CodeDescriptionBoolGrid.CreateGraphics())
				{
					ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(boolColumnCaption, CodeDescriptionBoolGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(booleanColumnCaptionRightPadding), false);
				}
			}
		}

		const int booleanColumnCaptionRightPadding = 25;

		string boolColumnCaption;

		#endregion

		public object Data
		{
			get { return DataSource; }
			set { SetDataBinding(value, null); }
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CodeDescriptionBoolGrid.ReadOnly = readOnly;
		}
	}
}
