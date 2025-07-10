using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal partial class MultipleChoiceUserControl : RuntimeOptionUserControl
	{
		public MultipleChoiceUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(DropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override Type ExpectedFilterType()
		{
			return typeof(MultipleChoice);
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			DropEdit.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			SetDataBinding(filter, "");
		}
	}
}
