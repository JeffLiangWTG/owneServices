using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionManagementController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.Commission; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Commission; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ViewCommissionLineGrouping); }
		}

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("Does not support New and Delete functionality");
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (IsSourceDeleted(sourceEntity))
			{
				return null;
			}

			var controller = GetController(sourceEntity, out var entityToShow);

			if (controller != null)
			{
				return LastShownForm = controller.ShowViewForm(entityToShow);
			}

			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (IsSourceDeleted(sourceEntity))
			{
				return null;
			}

			var controller = GetController(sourceEntity, out var entityToShow);

			if (controller != null)
			{
				return LastShownForm = controller.ShowEditForm(entityToShow);
			}

			return null;
		}

		bool IsSourceDeleted(BusinessObject sourceEntity)
		{
			if (sourceEntity is ViewCommissionLineGrouping grouping)
			{
				if (grouping.SourceTableCode == JobHeaderSchema.Constants.Prefix && grouping.Job == null)
				{
					Globals.Message.ShowError(JobHeaderDeletedMessage);
					return true;
				}
			}
			return false;
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			if (action == FormAction.Delete)
			{
				var grouping = (ViewCommissionLineGrouping)sourceEntity;
				ShowDeleteForm(grouping.CommissionLines);

				return LastShownForm;
			}
			else
			{
				return base.ShowLoadedForm(sourceEntity, action);
			}
		}

		protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
		{
			var commissionLines =
				selectedBusinessObjects.Cast<ViewCommissionLineGrouping>()
				.SelectMany(x => x.CommissionLines);

			ShowDeleteForm(commissionLines);
		}

		void ShowDeleteForm(IEnumerable<ViewCommissionLine> commissionLinesInExternalFactory)
		{
			var bulkCancelAction = BulkCancelCommissionLinesAction.NewForDifferentFactory(Factory, commissionLinesInExternalFactory);
			var form = new BulkCancelCommissionLinesForm(bulkCancelAction);
			ShowForm(form);
		}

		public void UndoCancel(BusinessObject[] selectedBusinessObjects)
		{
			var commissionLines =
				selectedBusinessObjects.Cast<ViewCommissionLineGrouping>()
				.SelectMany(x => x.CommissionLines);

			ShowUndoCancelForm(commissionLines);
		}

		void ShowUndoCancelForm(IEnumerable<ViewCommissionLine> commissionLinesInExternalFactory)
		{
			var bulkCancelAction = BulkUndoCancelCommissionLinesAction.NewForDifferentFactory(Factory, commissionLinesInExternalFactory);
			var form = new BulkUndoCancelCommissionLinesForm(bulkCancelAction);
			ShowForm(form);
		}

		ZController GetController(BusinessObject sourceEntity, out BusinessObject entityToShow)
		{
			var grouping = (ViewCommissionLineGrouping)sourceEntity;
			switch (grouping.SourceTableCode)
			{
				case AccTransactionHeaderSchema.Constants.Prefix:
					entityToShow = grouping.Transaction as BusinessObject;
					return AccountingControllerCreator.GetNewController(grouping.Transaction as AccTransactionHeader);

				case JobHeaderSchema.Constants.Prefix:
					entityToShow = grouping.Job;
					return JobController;
			}

			entityToShow = null;
			return null;
		}

		static string JobHeaderDeletedMessage => Res.GetString("8CE9BFB3-16CE-4D55-AB85-8936001E8C28", "This job cannot be opened as the corresponding Job Header has been deleted.");

		ZController JobController
		{
			get { return jobController ?? (jobController = ZControllerFactory.Create(ControllerIDs.JobManagement)); }
		}
		ZController jobController;

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CommissionManagerCancel; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CommissionManagerView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CommissionManagerNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CommissionManagerView; }
		}

		#endregion
	}
}
