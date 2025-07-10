using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionManager : NonPersistentBusinessObject, IMenuEditable, IObsoleteValidation, IDocumentSupportable
	{
		public OperationalActionManager(BusinessObjectFactory factory, OperationalActionContext context)
			: base(factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			this.context = context;
		}

		public string ModuleName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context.ModuleName; }
		}

		public OperationalActionCollection Actions
		{
			get
			{
				if (actions == null)
				{
					actions = new OperationalActionCollection(Factory, context);
					actions.EditingMode = EditingMode;
					actions.Load();
					actions.Sort<OperationalAction>(new ActionComparer());
					RegisterEditableChildObject(actions);
				}
				return actions;
			}
		}

		public MenuEditingMode EditingMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return editingMode; }
			set
			{
				editingMode = value;
				if (actions != null)
				{
					actions.EditingMode = value;
				}
			}
		}

		public OperationalActionSupporter ActionSupporter
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context.Supporter; }
		}

		public OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new ManagerDocumentSupporter(this); }
		}

		#endregion

		OperationalActionCollection actions;
		MenuEditingMode editingMode;
		readonly OperationalActionContext context;
	}
}
