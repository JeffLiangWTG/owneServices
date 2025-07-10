using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Registry.GUI
{
	public class MarkUpPercentagesRegistryItemEditor : RegistryItemEditor
	{
		public MarkUpPercentagesRegistryItemEditor(IRegistryDataType dataType) : base(dataType)
		{
		}

		protected override Control NewWinFormsEditorPaneCore()
		{
			return new MyMarkUpPercentagesControl();
		}

		protected override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((MyMarkUpPercentagesControl)editorPane).FieldValue;
		}

		protected override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			((MyMarkUpPercentagesControl)editorPane).FieldValue = (string)value;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		#region MyMarkUpPercentagesControl

		public class MyMarkUpPercentagesControl : MarkUpPercentagesContainer
		{
			public string FieldValue
			{
				get { return Data == null ? "" : Data.MarkUpPercentages.ToString(); }
				set
				{
					if (Data == null)
					{
						Data = new MarkUpPercentagesCollectionWrapper(value);
						SetDataBinding(Data, "");
					}
					else
					{
						Data.MarkUpPercentages.RemoveAll();
						Data.MarkUpPercentages.Load(value);
					}
				}
			}

			protected MarkUpPercentagesCollectionWrapper Data;
		}

		#endregion
	}
}
