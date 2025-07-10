using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	abstract class VisualiserComponentControlFactory<TVisualiserComponent, TControl>
		where TVisualiserComponent : VisualiserComponent
		where TControl : Control
	{
		internal TControl Create(VisualiserComponent component)
		{
			return CreateCore(((TVisualiserComponent)component));
		}
		protected abstract TControl CreateCore(TVisualiserComponent component);
	}

	class VisualiserComponentControlFactory : VisualiserComponentControlFactory<VisualiserComponent, Control>
	{
		protected override Control CreateCore(VisualiserComponent component)
		{
			Control result;

			if (component is VisualiserComponentLabel)
			{
				result = new VisualiserComponentLabelControlFactory().Create(component);
			}
			else if (component is VisualiserComponentTextBox)
			{
				result = new VisualiserComponentTextBoxControlFactory().Create(component);
			}
			else if (component is VisualiserComponentImage)
			{
				result = new VisualiserComponentImageControlFactory().Create(component);
			}
			else if (component is VisualiserComponentGrid)
			{
				result = new VisualiserComponentGridControlFactory().Create(component);
			}
			else if (component is VisualiserComponentBorder)
			{
				result = null;
			}
			else
			{
				throw new ArgumentException("Invalid VisualiserComponent.", nameof(component));
			}

			return result;
		}
	}
}
