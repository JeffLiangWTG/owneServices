using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	public partial class BulkStatusUpdatingForm : ZChildForm
	{
		public BulkStatusUpdatingForm(IQueueFilterBusinessObject filterBizObj, BusinessObject[] itemsToBulkUpdate)
			: this(filterBizObj, ToProcessQueueParentArray(itemsToBulkUpdate))
		{
		}

		public BulkStatusUpdatingForm(IQueueFilterBusinessObject filterBizObj, IProcessQueueParent[] itemsToBulkUpdate)
			: this(BulkStatusUpdating.New(filterBizObj, itemsToBulkUpdate))
		{
		}

		public BulkStatusUpdatingForm(BulkStatusUpdating businessEntity)
			: base(businessEntity)
		{
		}

		public new BulkStatusUpdating BusinessEntity
		{
			get { return base.BusinessEntity as BulkStatusUpdating; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1084:DoNotUseNewVirtual", Justification = "Baseline")]
		public new virtual void ShowDialog(IWin32Window owner)
		{
			base.ShowDialog(owner);
		}

		#region Button Event Handlers

		void OnBulkUpdate_Click(object sender, System.EventArgs e)
		{
			int selectedCount = BusinessEntity.ItemsToBulkUpdate.Length;
			NotificationBuffer notify = new NotificationBuffer();
			int updatedCount = BusinessEntity.BulkUpdate(notify);

			if (notify.HasErrors)
			{
				Globals.Message.ShowError(notify.AsString);
			}
			else
			{
				Globals.Message.ShowInformation(notify.AsString);
				Close();
			}
		}

		void OnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion

		#region Implementation

		protected override void SetVisibleCore(bool value)
		{
			if (value && BusinessEntity.ItemsToBulkUpdate.Length == 0)
			{
				Globals.Message.ShowWarning(
					"You must select one or more items to bulk update.\r\n" +
					"\r\n" +
					"To select multiple items, hold down the Control key and click the bar to the left of each item you wish to select.\r\n" +
					"Press Ctrl-A to select all visible items.");
			}
			else
			{
				base.SetVisibleCore(value);
			}
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return "Bulk Status Update"; }
		}

		static IProcessQueueParent[] ToProcessQueueParentArray(BusinessObject[] processQueueParents)
		{
			IProcessQueueParent[] result = new IProcessQueueParent[processQueueParents.Length];
			processQueueParents.CopyTo(result, 0);
			return result;
		}

		#endregion
	}
}
