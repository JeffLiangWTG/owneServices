using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal partial class CodeListMultipleChoiceUserControl : RuntimeOptionUserControl
	{
		public CodeListMultipleChoiceUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(DropEdit, new Enterprise.ZArchitecture.GUI.SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override Type ExpectedFilterType()
		{
			return typeof(CodeListMultipleChoice);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			DropEdit.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			SetDataBinding(filter, "");
		}
	}
}
