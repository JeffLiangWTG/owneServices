using CargoWise.Windows.UI;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionBoolRelatedItemRegistryControl : RegistryZUserControl
	{
		public CodeDescriptionBoolRelatedItemRegistryControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var dataSourceForBinding = dataSource as CodeDescriptionBoolCollection;
			base.SetDataBinding(dataSourceForBinding, dataMember);
			if (dataSourceForBinding != null)
			{
				SetBoolColumnLayoutIfNeeded();
				SetRelatedItemColumnLayout();
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			try
			{
				SuspendLayout();
				CodeDescriptionBoolRelatedItemGrid.SetReadOnly(readOnly);
			}
			finally
			{
				ResumeLayout(false);
			}
		}

		#region Bool Column

		public void SetupBoolColumn(string boolColumnCaption, bool isBoolColumnVisible)
		{
			this.boolColumnCaption = boolColumnCaption;
			this.isBoolColumnVisible = isBoolColumnVisible;
		}

		void SetBoolColumnLayoutIfNeeded()
		{
			if (isBoolColumnVisible)
			{
				var column = CodeDescriptionBoolRelatedItemGrid.Columns["Bool"];

				if (column != null)
				{
					var columnStyle = column.ColumnStyle;
					columnStyle.HeaderText = boolColumnCaption;

					using (var graphic = CodeDescriptionBoolRelatedItemGrid.CreateGraphics())
					{
						ControlDpiScalingHelper.SetWidth(ref columnStyle, (int)graphic.MeasureString(boolColumnCaption, CodeDescriptionBoolRelatedItemGrid.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), false);
					}
				}
			}
		}

		bool isBoolColumnVisible;
		string boolColumnCaption;

		#endregion

		#region Related Item Column

		public void SetupRelatedItemColumn(string columnCaption)
		{
			//relatedItemColumnCaption = columnCaption;
			isRelatedItemCodeColumnVisible = !string.IsNullOrEmpty(columnCaption); //HACK to switch Code / Description of Related Item
		}

		void SetRelatedItemColumnLayout()
		{
			var columnCode = CodeDescriptionBoolRelatedItemGrid.Columns["RelatedItemCode"];
			if (columnCode != null)
			{
				columnCode.IsVisible = isRelatedItemCodeColumnVisible;
				columnCode.ColumnStyle.Width = isRelatedItemCodeColumnVisible ? ControlDpiScalingHelper.ScaleToCurrentDpiX(100) : 0;
			}

			var columnDesc = CodeDescriptionBoolRelatedItemGrid.Columns["RelatedItemDescription"];
			if (columnDesc != null)
			{
				columnDesc.IsVisible = !isRelatedItemCodeColumnVisible;
				columnDesc.ColumnStyle.Width = isRelatedItemCodeColumnVisible ? 0 : ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			}
		}

		bool isRelatedItemCodeColumnVisible;
		//string relatedItemColumnCaption;

		#endregion

		public object Data
		{
			get { return DataSource; }
			set { SetDataBinding(value, null); }
		}
	}
}
