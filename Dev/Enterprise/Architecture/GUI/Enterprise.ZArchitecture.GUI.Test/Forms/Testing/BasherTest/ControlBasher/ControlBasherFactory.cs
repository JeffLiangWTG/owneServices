using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class ControlBasherFactory
	{
		public static IControlBasher[] Get(Control control)
		{
			var result = new List<IControlBasher>();

			if (control is ZCodeFindBox)
			{
				result.Add(new ZCodeFindBoxControlBasher());
			}
			else if (control is ZTimeZoneFindBox)
			{
				result.Add(new ZTimeZoneFindBoxControlBasher());
			}
			else if (control is ZAutoCompleteFindBox)
			{
				result.Add(new ZAutoCompleteFindBoxBasher());
			}
			else if (control is ZDropEdit)
			{
				result.Add(new ZDropEditControlBasher());
			}
			else if (control is ZGrid)
			{
				result.Add(new ZGridControlBasher());
			}
			else if (control is ZModuleButtonGrid)
			{
				result.Add(new ZModuleButtonGridBasher());
			}
			else if (control is CheckBox)
			{
				result.Add(new CheckBoxControlBasher());
			}
			else if (control is ZDateEdit)
			{
				result.Add(new ZDateEditControlBasher());
			}
			else if (control is ZButton)
			{
				result.Add(new ZButtonControlBasher());
			}
			else if (control is TextBox)
			{
				result.Add(new TextBoxControlBasher());
			}
			else if (control is Label)
			{
				result.Add(new LabelControlBasher());
			}
			else if (control is TabPage)
			{
				result.Add(new TabPageControlBasher());
			}
			else if (control is PictureBox)
			{
				result.Add(new PictureBoxControlBasher());
			}
			else if (control is ZTranslatableTextControl)
			{
				result.Add(new ZTranslatableTextControlBasher());
			}

			if (control is ZTextBox || control is ZRichTextBox)
			{
				result.Add(new TextTemplatesControlBasher());
			}

			// No else is intentional, read carefully.
			if (control is ContainerControl)
			{
				result.Add(new ContainerControlBasher());
			}

			// resource strings checking
			if (ResourceStringControlBasher.IsRequired(control))
			{
				result.Add(new ResourceStringControlBasher());
			}

			if (SuspendableLayoutControlBasher.ShouldBash(control))
			{
				result.Add(new SuspendableLayoutControlBasher());
			}

#if !WINZOR

			// DPI awareness checks should be done on every control used in the system
			if (DpiAwareControlBasher.NeedsDpiAwarenessValidation(control))
			{
				result.Add(new DpiAwareControlBasher());
			}

#endif

			return result.ToArray();
		}

		public static IControlBasher[] GetForVerifyingResourceStringAttributeForBoundProperty()
		{
			return new IControlBasher[] { new BoundPropertyResourceStringDataAttributeBasher() };
		}
	}
}
