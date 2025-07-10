using System.Collections.Generic;
using Enterprise.DocumentEngine;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Services.OperationalActions.GUI
{
	public sealed partial class OperationalActionCustomizationForm : ZForm, IMenuCustomisationForm
	{
		public OperationalActionCustomizationForm(OperationalActionManager businessEntity)
			: base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtons);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		public override string FormHeading
		{
			get { return Res.GetString("OperationalActionCustomizationForm|Caption", "{0} - Customize Operational Actions", Manager.ModuleName); }
		}

		public OperationalAction SelectedAction
		{
			get { return customisationControl.SelectedAction; }
		}

		#region IMenuCustomisationForm Members

		IMenuEditable IMenuCustomisationForm.EditableBusinessEntity
		{
			get { return Manager; }
		}

		#endregion

		#region Implementation

		public override ODisplayMode DisplayMode
		{
			get { return base.DisplayMode; }
			set
			{
				if (value == ODisplayMode.Browse)
				{
					value = ODisplayMode.NewSaved;
				}

				base.DisplayMode = value;
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (Manager != null)
			{
				OperationalActionContext context = Manager.Context;
				OperationalActionSupporter supporter = context.Supporter;

				customisationControl.ShowDocumentsTab = context.SupportsDocuments;
				customisationControl.ShowFieldsTab = supporter.SupportsBulkUpdates;
			}
		}

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new DevTools.FieldBlobXmlTool());
			tools.Add(new DevTools.MethodBlobXmlTool());
			tools.Add(new DevTools.FilterDevTool());
		}

		OperationalActionManager Manager
		{
			get { return (OperationalActionManager)BusinessEntity; }
		}

		#endregion
	}
}
