using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.OrgCollectionCalls;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for OrgCollecitonCalls.
	/// </summary>
	public class OrgCollectionCallsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrgCollectionCallsController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get	{ return ModuleIDs.OrgCollectionCalls; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgCollectionCalls; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgCollectionCall); }
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			ZQuery filter = new ZQuery(vw_OrgCollectionCallSchema.CC_GC, Env.CurrentCompany.PK);
			filter.AddToFilter(vw_OrgCollectionCallSchema.PK, sourceEntityPK);
			return factory.LoadTop1<OrgCollectionCall>(filter);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CollectionNotesForm(((OrgCollectionCall)businessEntity).Header);
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ReceivablesCollectionCallsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReceivablesCollectionCallsView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }// Not Applicable for this module.
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; } // Not Applicable for this module.
		}

		public override IZForm ShowNewForm()
		{
			return null; // Not Applicable for this module.
		}

		protected override bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting
		{
			get
			{
				return true;
			}
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return null; // Not Applicable for this module.
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Accounting.Module.Res.GetData("PlugInTabPage|OrgCollectionCalls", "Transactions"); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new CollectionCallsTransactionsPrintingPlugIn(businessEntity);
		}
	}
}
