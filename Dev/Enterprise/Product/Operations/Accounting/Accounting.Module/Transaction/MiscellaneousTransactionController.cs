using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.TransactionView;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for OVP, EXX and DSC
	/// </summary>
	public abstract class MiscellaneousTransactionController : AccountingTransactionController
	{
		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (CollectionForDefaultsAndValidation != null &&
				CollectionForDefaultsAndValidation.Count != 0 &&
				(CollectionForDefaultsAndValidation is TransactionHeaderCollection))
			{
				return ((TransactionHeaderCollection)CollectionForDefaultsAndValidation)[0];
			}
			else
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactoryCore2(IBusiness sourceEntity)
		{
			IBusiness loadedEntity = Factory.Load(TypeOfTopLevelBusinessObject, sourceEntity.Identifier);
			if (loadedEntity == null && !sourceEntity.IsInDatabaseIncludingChildren)
				// required for displaying unposted misc transactions in matching form
			{
				loadedEntity = sourceEntity;
			}
			//LoadedEntity.SetReadOnlyIncludingChildren(true);
			return loadedEntity;
		}

		// If it is the same BusinessEntity and a form is already open
		// then show the already opened form
		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			if (IsFormShownFor(businessEntity))
			{
				SwitchToFormFor(businessEntity);
				return LastShownForm;
			}
			else
			{
				return base.ShowFormForNewEntityCore(businessEntity);
			}
		}

		#region Implementation

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			if (businessEntity is TransactionHeader)
			{
				return new TransactionViewForm((TransactionHeader)businessEntity);
			}
			else
			{
				return null;
			}
		}

		protected override bool ShouldHaveReversedBizo
		{
			get { return false; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }	// TODO: Choose the correct check point	
		}

		/// <summary>
		/// not required, never use it
		/// </summary>
		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		/// <summary>
		/// not required, never use it
		/// </summary>
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		/// <summary>
		/// not required, never use it
		/// </summary>
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}