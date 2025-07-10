using System;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class SailingUserControl : ZUserControl
	{
		public SailingUserControl()
		{
			InitializeComponent();
		}

		JPAFRHeader Header
		{
			get { return (JPAFRHeader)DataSource; }
		}

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			var header = Header;
			if (header.IsAnyBillAlreadyRegistered || header.IsAnyBillHaveMessagingInProgress)
			{
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				var helper = new SailingIFindBox(header, (ZForm)ParentForm);
				helper.ShowModuleFromISailingParent();
			}
		}

		void ClearSailingButton_Click(object sender, EventArgs e)
		{
			var header = Header;
			if (header.IsAnyBillAlreadyRegistered || header.IsAnyBillHaveMessagingInProgress)
			{
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				header.ChangeSailing(ZGuid.Empty);
			}
		}

		void EditSailingButton_Click(object sender, EventArgs e)
		{
			var sailing = Header.Sailing;
			var voyage = sailing != null ? sailing.Voyage : null;
			if (voyage != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.JobSeaVoyage);
				var editForm = controller.ShowEditForm(voyage);
				editForm.Closed += delegate
				{
					if (Header != null)
					{
						Header.ChangeSailing(Header.JPH_ParentId);
					}
				};
#if DEBUG
				LastOpenedFormForTest = editForm;
#endif
			}
			else
			{
				Globals.Message.Show(Res.GetString("7F511FB7-8E29-441A-9717-600DF0496568", "There is no Sailing Schedule to edit."));
			}
		}

		void RefreshStatisticsButton_Click(object sender, EventArgs e)
		{
			Header.RefreshSailingStatistics();
		}

		static string CannotChangeSailingMessage => Res.GetString("86B67E67-950D-4637-A163-7A1AD4160BB2", "Messages are sent to customs or in progress, you cannot change the sailing.");
	}
}

namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class SailingUserControl
	{
		public IZForm LastOpenedFormForTest { get; private set; }
	}
}
