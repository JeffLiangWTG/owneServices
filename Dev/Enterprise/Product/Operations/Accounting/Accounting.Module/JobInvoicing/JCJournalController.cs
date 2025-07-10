using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class JCJournalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.JCCostingJournal; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JCJournalHeader); }
		}

		#region Show Form Overrides

		protected override bool DisallowMultiDeleteBecauseDeleteIsNotWhatIsReallyHappeningInAccounting
		{
			get
			{
				return true;
			}
		}

		public override IZForm ShowNewForm()
		{
			return null;
			// Do Nothing - View Only
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			// Do Nothing - View Only
			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			// Do Nothing - View Only
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			// Do Nothing - View Only
			return null;
		}

		#endregion

		#region Implementation

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new JobCostingJournalForm((JCJournalHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.JCJournalView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
