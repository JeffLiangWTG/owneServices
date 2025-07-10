using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class ParentAndChildCodeDescriptionBoolControl : RegistryZUserControl
	{
		public ParentAndChildCodeDescriptionBoolControl(ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo)
		{
			InitializeComponent();

			this.EditorInfo = editorInfo;

			ParentGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = editorInfo.ParentListCaption;
			ChildGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = editorInfo.ChildListCaption;

#if DEBUG
			TypeDescriptor.AddAttributes(ParentGroupBox, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(ChildGroupBox, new SuppressFormsLocalizedTestAttribute());
#endif

			ParentGrid.SetupColumns(editorInfo.ParentListEditorInfo.BoolColumnCaption, editorInfo.ParentListEditorInfo.IsBoolColumnVisible, editorInfo.ParentListEditorInfo.IsCodeColumnVisible);
			ChildGrid.SetupColumns(editorInfo.ChildListEditorInfo.BoolColumnCaption, editorInfo.ChildListEditorInfo.IsBoolColumnVisible, editorInfo.ChildListEditorInfo.IsCodeColumnVisible);

			GridSplitter.MinSize = ParentGroupBox.Height;
			GridSplitter.MinExtra = ChildGridPanel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ParentGrid.ReadOnly = EditorInfo.IsParentListReadOnly || readOnly;
			ChildGrid.ReadOnly = readOnly;
		}

		readonly ParentAndChildCodeDescriptionBoolRegistryEditorInfo EditorInfo;
	}
}
