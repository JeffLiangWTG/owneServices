using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ControlBag : IControlBag
	{
		readonly Dictionary<string, ControlReference> controlsByName = new Dictionary<string, ControlReference>();

		/// <summary>
		/// Creates a template control that contains individual controls that can be used in layout.
		/// </summary>
		protected abstract Control CreateTemplate();

		protected ControlReference RegisterControl(string controlName)
		{
			var controlReference = new ControlReference(this, controlName);
			controlsByName.Add(controlName, controlReference);
			return controlReference;
		}

		public void CreateControls(Control container, IDictionary<ControlReference, Control> controlsByReference)
		{
			using (var template = CreateTemplate())
			{
				var zContainer = container as ZUserControl;

				var controls = new List<Control>();

				foreach (var control in template.Controls.Cast<Control>())
				{
					var controlName = control.Name;

#if DEBUG
					if (!controlsByName.ContainsKey(controlName))
					{
						throw new InvalidOperationException(FormattableString.Invariant($"Template control '{template}' contains control with name '{controlName}' that was not declared in the bag '{this}'."));
					}
#endif

					var cref = new ControlReference(this, controlName);
					controlsByReference.Add(cref, control);
					controls.Add(control);
				}

				foreach (var control in controls)
				{
					UpdateBinding(zContainer, control);
					control.Parent = container;
				}
			}
		}

		void UpdateBinding(ZUserControl zContainer, Control control)
		{
			string bindingMember = control.GetBindingMember();
			if (!string.IsNullOrEmpty(bindingMember))
			{
				zContainer?.BindingSource.SetBindingMember(control, bindingMember);
			}

			if (control is ZGroupBox || control is ZPanel)
			{
				foreach (var childControl in control.Controls.OfType<Control>())
				{
					UpdateBinding(zContainer, childControl);
				}
			}
		}

		public bool ContainsControl(string name)
		{
			return controlsByName.ContainsKey(name);
		}

		public IReadOnlyCollection<ControlReference> Controls => controlsByName.Values;

		public Control TemplateControl => CreateTemplate();
	}
}
