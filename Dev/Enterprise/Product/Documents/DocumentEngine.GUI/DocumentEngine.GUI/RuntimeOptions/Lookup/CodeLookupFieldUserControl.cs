using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	[SuppressBindingMemberBashingTest]
	internal partial class CodeLookupFieldUserControl : RuntimeOptionUserControl
	{
		public CodeLookupFieldUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(FieldFindBox, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override void SetFilter(FilterField filter)
		{
			base.SetFilter(filter);
			FieldFindBox.GetExtension<ILabelCaptionRenderer>().Caption = EscapeMnemonics(filter.DisplayNameLocalized);
			FieldFindBox.BindToList = "BindToFindBoxList";
			FieldFindBox.ModuleID = ((LookupFilterFieldBase)filter).ModuleID; // This SHOULD be here in case the user wants to override the ModuleID upon usage of this control.
			FieldFindBox.SetDataBinding(filter, "ZValue");
		}

		public override Type ExpectedFilterType()
		{
			return typeof(CodeLookupField);
		}
	}
}
