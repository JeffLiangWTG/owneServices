using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class TestBulkStatusUpdatingForm : BulkStatusUpdatingForm
	{
		public TestBulkStatusUpdatingForm(IQueueFilterBusinessObject filterBizObj, BusinessObject[] itemsToBulkUpdate) : base(filterBizObj, itemsToBulkUpdate)
		{
		}

		public TestBulkStatusUpdatingForm(IQueueFilterBusinessObject filterBizObj, IProcessQueueParent[] itemsToBulkUpdate) : base(filterBizObj, itemsToBulkUpdate)
		{
		}

		public TestBulkStatusUpdatingForm(BulkStatusUpdating businessEntity) : base(businessEntity)
		{
		}

		public bool ShowDialogWasCalled;
		public override void ShowDialog(IWin32Window owner)
		{
			Show();
			ShowDialogWasCalled = true;
		}

		public new ZArchitecture.GUI.ZButton BulkUpdateButton
		{
			get
			{
				return base.BulkUpdateButton;
			}
		}

		public new ZArchitecture.GUI.ZButton CloseButton
		{
			get
			{
				return base.CloseButton;
			}
		}
	}
}
