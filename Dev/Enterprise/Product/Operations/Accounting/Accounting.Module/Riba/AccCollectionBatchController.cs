using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccCollectionBatchController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccCollectionBatchController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccCollectionBatch; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccCollectionBatch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccCollectionBatch); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccCollectionBatchForm((AccCollectionBatch)businessEntity);
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("49f6971d-5731-42f5-8cd4-d354cbdbe1fc", "Collection batch can only be created from the receivable module"));
			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			AccCollectionBatch batch = sourceEntity as AccCollectionBatch;
			if (batch.ACB_IsCancelled)
			{
				return base.ShowViewForm(sourceEntity);
			}
			else
			{
				return base.ShowEditForm(sourceEntity);
			}
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			AccCollectionBatch batch = sourceEntity as AccCollectionBatch;
			if (batch.ACB_IsCancelled)
			{
				Globals.Message.ShowError(Res.GetString("24bf1142-92cc-47f8-89af-155b815095bb", "This batch is already canceled."));
				return null;
			}
			else
			{
				return base.ShowDeleteForm(batch);
			}
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CollectionBatchCancel; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CollectionBatchEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CollectionBatchView; }
		}
	}
}
