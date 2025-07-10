using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	public class ProcessTemplateCustomFieldsControl : CustomPropertiesControl
	{
		public ProcessTemplateCustomFieldsControl()
		{
			this.NothingSetupMessageLabelText = Res.GetString("ProcessTemplateCustomFieldsControl|69ece986-b123-472a-91ad-c64737c74913", "To make use of this tab, please setup Transport Booking Instruction custom fields in Workflow Manager.");
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember) && dataSource != null)
			{
				BindingManagerBase bindingManager = BindingContext[dataSource, dataMember];
				CurrencyManager currencyManager = bindingManager as CurrencyManager;
				if (currencyManager != null && currencyManager.List is BusinessObject)
				{
					dataSource = currencyManager.List;
				}
				else
				{
					dataSource = bindingManager.Position >= 0 ? bindingManager.GetCurrent() : null;
				}
			}

			if (dataSource is BusinessObject && dataSource is IWorkflowProviderCore)
			{
				if (parent != null)
				{
					DetachHandlers();

					if (DataSource != null)
					{
						parent.UnRegisterEditableChildObject((BusinessObject)DataSource);
					}
				}

				parent = (BusinessObject)dataSource;
				parent.UnRegisterCustomBusinessObject();

				if (processTaskTemplateLoader == null)
				{
					processTaskTemplateLoader = (IProcessTaskTemplateLoader)Activator.CreateInstance(ObjectFactory.GetType<IProcessTaskTemplateLoader>(), parent.Factory);
				}
				processTaskTemplate = processTaskTemplateLoader.FindMatches((IWorkflowProviderCore)parent).Matches.FirstOrDefault();

				if (workflowDescriptorLoader == null)
				{
					workflowDescriptorLoader = ObjectFactory.Get<IWorkflowDescriptorLoader>();
				}
				workflowDescriptor = workflowDescriptorLoader.GetWorkflowDescriptor(((IWorkflowProviderCore)parent).WorkflowType);

				AttachHandlers();

				var customFieldProvider = parent as ICustomFieldProvider;
				if (customFieldProvider != null)
				{
					var cusObj = customFieldProvider.GetCustomBusinessObject(true);

					if (((IDynamicBusinessObject)cusObj).PropertyNames.Length > 0)
					{
						parent.RegisterEditableChildObject(cusObj);
						base.SetDataBinding(cusObj, "");
					}
					else
					{
						base.SetDataBinding(null, "");
					}
				}
				else
				{
					base.SetDataBinding(null, "");
				}
			}
			else if (dataSource == null)
			{
				if (parent != null && DataSource != null)
				{
					parent.UnRegisterEditableChildObject((BusinessObject)DataSource);
				}

				base.SetDataBinding(null, "");
			}
		}

		void AttachHandlers()
		{
			if (parent != null && workflowDescriptor != null)
			{
				foreach (string propertyName in workflowDescriptor.GetPropertiesThatAffectWorkflow())
				{
					parent.ZPropertyInfoHash[propertyName].ValueChanged += PropertyThatAffectWorkflowChanged;
				}
			}
			var affectedPropertyProvider = parent as IWorkflowAffectedPropertyProvider;
			if (affectedPropertyProvider != null && affectedPropertyProvider.PropertyThatAffectWorkflowChanged != null)
			{
				foreach (var property in affectedPropertyProvider.PropertyThatAffectWorkflowChanged)
				{
					property.ValueChanged += PropertyThatAffectWorkflowChanged;
				}
			}

			var customLabelsConfigOrgProvider = parent as ICustomLabelsConfigOrgProvider;
			if (customLabelsConfigOrgProvider != null)
			{
				customLabelsConfigOrgProvider.ConfigOrgChanged += PropertyThatAffectCustomLabelsChanged;
			}
		}

		void DetachHandlers()
		{
			if (parent != null && workflowDescriptor != null)
			{
				foreach (string propertyName in workflowDescriptor.GetPropertiesThatAffectWorkflow())
				{
					parent.ZPropertyInfoHash[propertyName].ValueChanged -= PropertyThatAffectWorkflowChanged;
				}
			}

			var affectedPropertyProvider = parent as IWorkflowAffectedPropertyProvider;
			if (affectedPropertyProvider != null && affectedPropertyProvider.PropertyThatAffectWorkflowChanged != null)
			{
				foreach (var property in affectedPropertyProvider.PropertyThatAffectWorkflowChanged)
				{
					property.ValueChanged -= PropertyThatAffectWorkflowChanged;
				}
			}

			var customLabelsConfigOrgProvider = parent as ICustomLabelsConfigOrgProvider;
			if (customLabelsConfigOrgProvider != null)
			{
				customLabelsConfigOrgProvider.ConfigOrgChanged -= PropertyThatAffectCustomLabelsChanged;
			}
		}

		void PropertyThatAffectWorkflowChanged(object sender, EventArgs e)
		{
			ResetDataBinding(false);

			if (sender is IWorkflowProvider provider)
			{
				(provider.WorkflowItems as IRefreshProcessTaskCollection)?.RefreshProcessTaskCollection(false);
			}
		}

		void PropertyThatAffectCustomLabelsChanged(object sender, EventArgs e)
		{
			ResetDataBinding(true);
		}

#if DEBUG
		public bool ResetDataBindingDone;
		protected virtual
#endif
		void ResetDataBinding(bool customOrgChanged)
		{
			if (!IsDisposed && !parent.IsDeleted && (customOrgChanged || processTaskTemplateLoader == null || processTaskTemplate == null || processTaskTemplateLoader.FindMatches((IWorkflowProviderCore)parent).Matches.FirstOrDefault() != processTaskTemplate))
			{
				SetDataBinding(parent, "");

#if DEBUG
				ResetDataBindingDone = true;
#endif
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DetachHandlers();
			}

			base.Dispose(disposing);
		}

		BusinessObject parent;

#if DEBUG
		protected
#endif
		IProcessTaskTemplateLoader processTaskTemplateLoader;
		IProcessTaskTemplate processTaskTemplate;

		IWorkflowDescriptorLoader workflowDescriptorLoader;
		IWorkflowDescriptor workflowDescriptor;
	}
}
