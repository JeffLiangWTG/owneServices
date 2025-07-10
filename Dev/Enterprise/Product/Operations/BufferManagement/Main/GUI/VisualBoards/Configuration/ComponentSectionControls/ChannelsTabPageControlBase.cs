using System;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class ChannelsTabPageControlBase : ZUserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			parentControl = this.GetParent<BoardSectionConfigControl>();
			if (parentControl != null)
			{
				parentControl.SectionConfigChanged += SectionConfigControl_SectionConfigChanged;

				if (ChannelsControl != null)
				{
					ChannelsControl.ChannelsChanged += ChannelsControl_ChannelsChanged;
				}
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (parentControl != null)
			{
				SectionConfigChanged();
			}
		}

		void ChannelsControl_ChannelsChanged(object sender, EventArgs e)
		{
			if (parentControl != null)
			{
				parentControl.OnSectionConfigChanged();
			}
		}

		void SectionConfigControl_SectionConfigChanged(object sender, EventArgs e)
		{
			SectionConfigChanged();
		}

		void SectionConfigChanged()
		{
			var selectedSection = parentControl.GetSelectedSection();
			if (selectedSection != null)
			{
				OnSectionControlConfigChanged(selectedSection);
			}
		}

		protected virtual ChannelAssignmentControl ChannelsControl
		{
			get { return null; }
		}

		protected virtual BMBoardSectionChannelsViewModel GetChannelsViewModel(BMBoardSection section)
		{
			return null;
		}

		void OnSectionControlConfigChanged(BMBoardSection selectedSection)
		{
			if (ChannelsControl != null)
			{
				ChannelsControl.ViewModel = GetChannelsViewModel(selectedSection);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (parentControl != null)
				{
					parentControl.SectionConfigChanged -= SectionConfigControl_SectionConfigChanged;
				}
				if (ChannelsControl != null)
				{
					ChannelsControl.ChannelsChanged -= ChannelsControl_ChannelsChanged;
				}
			}
		}

		BoardSectionConfigControl parentControl;
	}
}
