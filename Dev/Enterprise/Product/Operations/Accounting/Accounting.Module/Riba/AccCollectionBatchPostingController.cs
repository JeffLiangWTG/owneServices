using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class AccCollectionBatchPostingController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccCollectionBatchForm(((AccCollectionBatchPoster)businessEntity).Batch);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CreateCollectionOrderBatch; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccCollectionBatchPosting; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccCollectionBatch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccCollectionBatchPoster); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new AccCollectionBatchPoster(Factory);
		}
	}
}
