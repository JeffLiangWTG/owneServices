using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.GUI.CashBook.DepositBatch;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class DepositBatchController : AccountingTransactionController
	{
		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DepositBatchForm(businessEntity as DepositBatchParent);
		}

		protected override bool ShouldHaveReversedBizo
		{
			get { return false; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.DepositBatch; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DepositBatch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DepositBatchParent); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new DepositBatchParent(Factory);
		}

		protected override IBusiness GetTopLevelBusinessObject(IBusiness sourceEntity)
		{
			DepositBatch batch = sourceEntity as DepositBatch;
			return batch != null ? new DepositBatchParent(Factory, batch) : sourceEntity;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactoryCore2(IBusiness sourceEntity)
		{
			if (sourceEntity is DepositBatchParent parent && parent.IsExistingBatch)
			{
				return parent;
			}

			return base.GetLoadedBusinessEntityInLocalFactoryCore2(sourceEntity);
		}

		protected override ZString CantReverseMessageBoxCaption
		{
			get { return Res.GetString("e5c4c384-0acc-4ead-96c3-a001ffecbb9a", "Deposit Batch"); }
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return factory.Load(AccTransactionHeaderSchema.Constants.Prefix, sourceEntityPK) as DepositBatch;
		}

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseDepositBatch; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewDepositBatch; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewDepositBatch; }
		}

		#endregion
	}
}
