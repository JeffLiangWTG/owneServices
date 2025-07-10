using System;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class SailingUserControl : ZUserControl
	{
		public SailingUserControl()
		{
			InitializeComponent();
		}

		AsycudaManifestHeader Header => (AsycudaManifestHeader)DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			Header.Sailings.CountChanged -= OnSailingsCountChanged;
			Header.Sailings.CountChanged += OnSailingsCountChanged;

			OnSailingsCountChanged(null, e);
		}

		void OnSailingsCountChanged(object sender, EventArgs e)
		{
			var isLinkedSailing = Header != null && Header.Sailings.Any();

			SailingStatisticsPanel.Visible = isLinkedSailing;
			RefreshStatisticsButton.Visible = isLinkedSailing;
		}

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			var header = Header;

			if (header.AMA_TransportMode.IsEmpty)
			{
				Globals.Message.Show(NoTransportMode);
			}
			else if (!header.CanChangeSailing)
			{
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				var helper = new SailingIFindBox(header, (ZForm)ParentForm);
				helper.ShowModuleFromISailingParent();
			}
		}

		static string NoTransportMode => Res.GetString("C92E4620-2D79-4F08-988A-08CCEA4FA89E", "Please choose an valid transport mode.");

		static string CannotChangeSailingMessage => Res.GetString("D97AD6C5-5FE6-4643-B09D-742BEA047262", "Once messages are sent to customs, you cannot change the sailing.");

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
						Header.ChangeSailing(Header.AMA_ParentId);
					}
				};
			}
			else
			{
				Globals.Message.Show(SailingScheduleNotCreated);
			}
		}

		static string SailingScheduleNotCreated => Res.GetString("73BF375F-9454-44E8-8DA7-1EC440D33328", "There is no Sailing Schedule to edit.");

		void ClearSailingButton_Click(object sender, EventArgs e)
		{
			var header = Header;

			if (!header.CanChangeSailing)
			{
				this.SetReadOnlyIncludingChildren();
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				header.ClearSailing(!header.AMA_OverrideFreightDefaults);
			}
		}

		void RefreshStatisticsButton_Click(object sender, EventArgs e)
		{
			Header.RefreshSailingStatistics();
		}

		protected override void Dispose(bool disposing)
		{
			var header = Header;

			if (header != null)
			{
				header.Sailings.CountChanged -= OnSailingsCountChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
