using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class CheckBoxEditorView : IEditorView
	{
		public CheckBoxEditorView(IDynamicContentLayoutElement content, IMacroBusinessObjectProperty property)
		{
			Argument.NotNull(content, nameof(content));
			Argument.NotNull(property, nameof(property));

			this.content = content;
			this.property = property;
		}

		readonly IDynamicContentLayoutElement content;
		readonly IMacroBusinessObjectProperty property;

		public IDynamicContentLayoutElement Content
		{
			get { return content; }
		}

		public object Control
		{
			get { return null; }
		}

		public bool WaitForUserInput
		{
			get { return false; }
		}

		public void BeginEdit()
		{
		}

		public void EndEdit()
		{
			var converter = ZBoolTypeConverter.Instance;
			if (property.Value != null && converter.CanConvertFrom(property.PropertyType))
			{
				ZBool newValue = !(ZBool)converter.ConvertFrom(property.Value);
				property.Value = newValue;
			}
		}

		public void CancelEdit()
		{
		}

		public bool IsPopup
		{
			get { return false; }
		}

		#region IDisposable members

		public void Dispose()
		{
		}

		#endregion
	}
}
