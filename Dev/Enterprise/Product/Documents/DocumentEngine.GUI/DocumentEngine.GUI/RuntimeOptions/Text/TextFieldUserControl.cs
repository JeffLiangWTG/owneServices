using System;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	/// <summary>
	/// Summary description for TextField.
	/// </summary>
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal partial class TextFieldUserControl : RuntimeOptionUserControl
	{
		public TextFieldUserControl()
		{
			InitializeComponent();
		}

		public override void SetFilter(FilterField filter)
		{
			FieldTextBox.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			FieldTextBox.GetExtension<ILabelCaptionRenderer>().Options = StringRenderingOptions.Truncate;
			base.SetDataBinding(filter, "");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			// manual binding
		}

		public override Type ExpectedFilterType()
		{
			return typeof(TextField);
		}
	}
}
