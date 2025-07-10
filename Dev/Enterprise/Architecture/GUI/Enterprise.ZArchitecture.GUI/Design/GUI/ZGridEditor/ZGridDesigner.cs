using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Enterprise.ZArchitecture.GUI.Design
{
	internal class ZGridDesigner : ControlDesigner, IWindowsFormsEditorService, ITypeDescriptorContext
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		public ZGridDesigner()
		{
			var customiseColumnStylesVerb = new DesignerVerb("Customize ColumnStyles", new EventHandler(CustomiseColumnStyles));
			Verbs.Add(customiseColumnStylesVerb);
		}

		ZGridColumnStyleEditor Editor;

		void CustomiseColumnStyles(object sender, EventArgs e)
		{
			Editor = new ZGridColumnStyleEditor();
			Editor.EditValue(this, this, ColumnStyles);
		}

		protected virtual ArrayList ColumnStyles
		{
			get { return (ArrayList)TypeDescriptor.GetProperties(Control)["ColumnStyles"].GetValue(Control); }// ((ZGrid)Control).ColumnStyles; }
		}

		#region IServiceProvider Members

		object IServiceProvider.GetService(Type typeOfService)
		{
			return typeOfService == typeof(IWindowsFormsEditorService) ? this : this.GetService(typeOfService);
		}

		#endregion

		#region IWindowsFormsEditorService Members

		public void DropDownControl(Control control)
		{
		}

		public void CloseDropDown()
		{
		}

		public DialogResult ShowDialog(Form dialog)
		{
			return dialog.ShowDialog();
		}

		#endregion

		#region ITypeDescriptorContext Members

		public void OnComponentChanged()
		{
			var changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			if (changeService != null)
			{
				changeService.OnComponentChanged(Control, PropertyDescriptor, null, ColumnStyles);
			}
		}

		public IContainer Container
		{
			get { return ((IComponent)Control).Site.Container; }
		}

		public bool OnComponentChanging()
		{
			var changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			if (changeService != null)
			{
				changeService.OnComponentChanging(Control, PropertyDescriptor);
				return true;
			}
			else
			{
				return false;
			}
		}

		public object Instance
		{
			get { return Control; }
		}

		public virtual PropertyDescriptor PropertyDescriptor
		{
			get { return TypeDescriptor.GetProperties(Control).Find("ColumnStyles", false); }
		}

		#endregion
	}
}