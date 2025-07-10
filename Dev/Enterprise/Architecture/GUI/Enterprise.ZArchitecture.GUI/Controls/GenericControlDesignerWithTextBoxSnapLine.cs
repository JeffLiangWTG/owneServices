using System;
using System.Collections;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;
namespace Enterprise.ZArchitecture.GUI.Internal
{
	abstract class GenericControlDesignerWithTextBoxSnapLine<T> : ControlDesigner
	{
		public override IList SnapLines
		{
			get
			{
				using (var textBoxDesigner = (ControlDesigner)Activator.CreateInstance(Type.GetType("System.Windows.Forms.Design.TextBoxBaseDesigner, System.Design")))
				{
					var usercontrol = (T)Component;
					textBoxDesigner.Initialize(GetTextBox(usercontrol));

					var result = base.SnapLines;
					foreach (SnapLine snapLine in textBoxDesigner.SnapLines)
					{
						result.Add(snapLine);
					}
					return result;
				}
			}
		}

		protected abstract TextBox GetTextBox(T userControl);
	}
}