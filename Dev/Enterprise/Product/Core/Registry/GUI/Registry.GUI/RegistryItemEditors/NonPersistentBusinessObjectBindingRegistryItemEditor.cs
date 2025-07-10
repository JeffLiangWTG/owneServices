using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public abstract class NonPersistentBusinessObjectBindingRegistryItemEditor : RegistryItemEditor
	{
		public NonPersistentBusinessObjectBindingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType)
		{
			this.fallbackLevel = fallbackLevel;
			this.factory = factory;
		}

		protected sealed override Control NewWinFormsEditorPaneCore()
		{
			ZUserControl result = NewBoundWinFormsEditorPane();
			result.ParentChanged += new EventHandler(OnControl_ParentChanged);
			return result;
		}

		protected sealed override object GetValueFromEditorPaneCore(Control editorPane)
		{
			return ((IDataBoundControl)editorPane).DataSource;
		}

		protected sealed override void SetValueFromEditorPaneCore(Control editorPane, object value)
		{
			var registryValue = value as IRegistryBusiness;
			if (registryValue != null)
			{
				if (!bound)
				{
					valueToBind = PrepareDataForBinding(registryValue);
				}
				else
				{
					var clonedData = registryValue.Clone(fallbackLevel, factory);
					var preparedData = PrepareDataForBinding(clonedData);
					var containerControl = (RegistryZUserControl)editorPane;

					containerControl.SetDataBinding(preparedData, null);
				}
			}
		}

		protected virtual IRegistryBusiness PrepareDataForBinding(IRegistryBusiness data)
		{
			data.CurrentFallbackLevel = fallbackLevel;
			return data;
		}

		protected abstract RegistryZUserControl NewBoundWinFormsEditorPane();

		void OnControl_ParentChanged(object sender, EventArgs e)
		{
			if (!bound)
			{
				RegistryZUserControl control = (RegistryZUserControl)sender;
				if (control.CurrentDataItem == null)
				{
					if (valueToBind != null)
					{
						control.SetDataBinding(valueToBind, null);
						valueToBind = null;
					}
				}
				bound = true;
			}
		}

		bool bound;
		IBusiness valueToBind;
		readonly FallbackLevel fallbackLevel;
		readonly BusinessObjectFactory factory;

		protected FallbackLevel Level
		{
			get { return fallbackLevel; }
		}

		protected BusinessObjectFactory Factory
			=> factory;
	}
}
