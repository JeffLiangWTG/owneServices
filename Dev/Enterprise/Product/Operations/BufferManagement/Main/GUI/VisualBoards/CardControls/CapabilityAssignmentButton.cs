using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class CapabilityAssignmentButton : ZButton
	{
		public CapabilityAssignmentButton(ITaskCardComponentParent parent)
		{
			this.parent = parent;
			Visible = parent.IsPreview || (parent.CardContent.CardType == CardType.Task && parent.Task.RequiresResourceWithCapability);

			if (Visible)
			{
				assignToStaff = GetAssignToStaff();
				if (assignToStaff == null)
				{
					Visible = false;
				}
				else
				{
					Text = GetLabel(assignToStaff);
				}
				requiredCapability = parent.Task.RequiredCapability;
			}

			parent.StatusUpdated += Parent_StatusUpdated;
			SetTooltip();
		}

		readonly ITaskCardComponentParent parent;
		readonly GlbStaff assignToStaff;
		readonly GlbCapability requiredCapability;
		readonly MultiActionButtonDialogWrapper<CrossChannelTaskAssignments> dialogWrapper = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

#if DEBUG

		public MultiActionButtonDialogWrapper<CrossChannelTaskAssignments> DialogWrapperForTest
		{
			get { return dialogWrapper; }
		}

#endif

		BusinessObjectFactory Factory
		{
			get { return parent.Task.Factory; }
		}

		GlbStaff GetAssignToStaff()
		{
			var channel = parent.Cell != null ? parent.Cell.Channel ?? parent.Cell.SecondaryChannel : null;
			return GetAssignToStaff(Factory, channel);
		}

		static GlbStaff GetAssignToStaff(BusinessObjectFactory factory, IVisualBoardChannel channel)
		{
			return channel != null && channel.EntityType == ChannelTypeList.Codes.Resource ? factory.Load<GlbStaff>(channel.EntityPK) : GlbStaff.CurrentUser;
		}

		static string GetLabel(GlbStaff assignToStaff)
		{
			return assignToStaff.PK != GlbStaff.CurrentUser.PK
				? Res.GetString("df5d8a86-a228-483b-a60a-daa3c183ddb8", "Assign to {0}", assignToStaff.GS_Code)
				: Res.GetString("d2d6609d-1d7b-43f5-83cc-e68e9ba2717f", "Claim");
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (!parent.IsPreview)
			{
				var result = HandleButtonClick(dialogWrapper);
				if (result)
				{
					Visible = false;
				}
			}
		}

		public bool HandleButtonClick(IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> userNotificationProvider)
		{
			var currentChannel = parent.Cell != null ? parent.Cell.Channel ?? parent.Cell.SecondaryChannel : null;
			var currentChannelSafe = currentChannel ?? new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled);
			var staff = GetAssignToStaff(Factory, currentChannel);
			var sectionViewModel = parent is ControlCustomisationViewModel ? ((ControlCustomisationViewModel)parent).ViewModel : null;
			var message = Res.GetString("fcebcaae-5877-4e79-9aaa-75710e5deae7", "Would you like to assign tasks related to '{0}' that are not yet started to '{1}'?", parent.Task.P9_Description, staff.GS_FullName);
			var result = ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(parent.Task, currentChannelSafe, sectionViewModel, userNotificationProvider, message, t => HandleTaskAssignmentCore(t, currentChannelSafe), currentChannelSafe);

			return result != CrossChannelTaskAssignments.None;
		}

		static void HandleTaskAssignmentCore(ProcessTask processTask, IVisualBoardChannel currentChannel)
		{
			var staff = GetAssignToStaff(processTask.Factory, currentChannel);
			processTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "ToolTip must be set this way, otherwise it doesn't work. Not Good!")]
		void SetTooltip()
		{
			if (requiredCapability != null && Visible)
			{
				var personToAssignTask = assignToStaff.PK == GlbStaff.CurrentUser.PK
					? Res.GetString("d83515b5-d4b2-433b-9851-417df9bb128a", "directly to yourself")
					: Res.GetString("f9525c3a-3ed8-488a-bf2b-930e5129645a", "to {0}", assignToStaff.GS_FullName);

				var tooltip = ResString.GetMultilingualString("51514fba-c4ab-47d0-bb10-e831059114f8", "This task is not assigned to a particular channel, however is configured to require the {0} capability.\r\nBy clicking this button, you will assign this task {1}.",
					requiredCapability.G4_Description,
					personToAssignTask);

				ToolTipService.SetToolTip(this, tooltip);
			}
		}

		void Parent_StatusUpdated(object sender, EventArgs e)
		{
			Visible = false;
		}
	}
}
