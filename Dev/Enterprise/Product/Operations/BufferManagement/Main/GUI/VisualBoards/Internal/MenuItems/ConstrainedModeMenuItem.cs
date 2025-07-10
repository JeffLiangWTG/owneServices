using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	class ConstrainedModeMenuItem : ZToolStripMenuItem
	{
		internal ConstrainedModeMenuItem(BMBoardSectionViewModel viewModel)
			: base(GetLabel(viewModel))
		{
			boardSectionPK = viewModel.SectionPK;
			Click += ConstrainedModeMenuItem_Click;
		}

		readonly ZGuid boardSectionPK;

		internal event EventHandler ModeChanged;

		void OnModeChanged()
		{
			if (ModeChanged != null)
			{
				ModeChanged(this, EventArgs.Empty);
			}
		}

		void ConstrainedModeMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.BMBoardSwitchToConstrainedMode.IsAllowed)
			{
				if (Text == SwitchToConstrainedModeLabel)
				{
					if (SwitchToConstrainedMode())
					{
						Text = SwitchToNonConstrainedModeLabel;
						OnModeChanged();
					}
				}
				else
				{
					if (SwitchToNonConstrainedMode())
					{
						Text = SwitchToConstrainedModeLabel;
						OnModeChanged();
					}
				}
			}
			else
			{
				Env.Security.BMBoardSwitchToConstrainedMode.ShowError();
			}
		}

		bool SwitchToConstrainedMode()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "ConstrainedModeMenuItem.SwitchToConstrainedMode" };
			var boardSection = factory.Load<BMBoardSection>(boardSectionPK);
			var releaseGroup = boardSection != null ? factory.Load<GlbGroup>(boardSection.SectionConfiguration.ApplicableReleaseGroupPK) : null;

			if (boardSection != null)
			{
				if (releaseGroup == null)
				{
					Globals.Message.ShowError(Res.GetString("ee731f15-08c8-489c-8472-fd30a966a2d4", "Cannot switch to Constrained Mode since there is no Release Group configured for this Visual Board or section."));
					return false;
				}

				if (!boardSection.Component.ChildComponents.Any(c => c.FC_Type == BMComponentTypeList.Codes.Constraint))
				{
					Globals.Message.ShowError(Res.GetString("a470e8dd-aac1-4b75-999d-446c9e17b9be", @"Cannot switch {0} to Constrained Mode since there is no Constraint sub-component within {1}.

A Constraint sub-component is needed for the Release Gate to use as the basis for constrained resources' total capacity.", releaseGroup.GG_Desc, boardSection.Component.FC_Name));
					return false;
				}

				if (!releaseGroup.Staff.Cast<GlbStaff>().Any(s => ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(s, boardSection.Component)))
				{
					Globals.Message.ShowError(Res.GetString("be89459f-4b8a-48f4-8b48-c0097ca9c610", @"Cannot switch {0} to Constrained Mode since the group contains no resources marked as capacity constrained.

Candidate capacity constrained resource channels will have an hourglass next to their name. Right-click the channel to mark it as capacity constrained.", releaseGroup.GG_Desc));
					return false;
				}

				var caption = Res.GetString("ba449d5f-7a81-4991-9729-31db1896c2b5", "Switch to Constrained Mode");
				var message = Res.GetString("ebf5c23e-0a1c-44a1-a02c-e620818c8811", @"Switch {0} to Constrained Mode?

This will cause Visual Boards to display Buffer sub-components, and the Release Gate to use the Constraint sub-component as the basis for constrained resources' total capacity.", releaseGroup.GG_Desc);
				var result = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.OK);

				if (result == DialogResult.OK)
				{
					ConstrainedModeHelper.SwitchToConstrainedMode(releaseGroup, boardSection.MS_FC_Component);
					boardSection.SectionConfiguration.Board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;

					return factory.SaveHandlingZSaveExceptions();
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		bool SwitchToNonConstrainedMode()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "ConstrainedModeMenuItem.SwitchToNonConstrainedMode" };
			var boardSection = factory.Load<BMBoardSection>(boardSectionPK);
			var releaseGroup = boardSection.ReleaseGroup;

			if (releaseGroup != null)
			{
				var link = ConstrainedModeHelper.GetLink(factory, boardSection.MS_FC_Component, boardSection.SectionConfiguration.ApplicableReleaseGroupPK);

				if (link != null && link.FO_IsConstrainedMode)
				{
					var caption = Res.GetString("6be3f4bf-6331-4b8a-8862-f1d31ff0849b", "Switch to non-Constrained Mode");
					var message = Res.GetString("fa9608a2-dc8f-472e-a0ae-7ff1e1d82f44", @"Switch {0} to non-Constrained Mode?

This will cause Visual Boards to no longer display Buffer sub-components, and the Release Gate to use the standard process to determine total capacity for all resources.", releaseGroup.GG_Desc);

					var result = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, DialogResult.OK);
					if (result == DialogResult.OK)
					{
						link.Delete();
						boardSection.SectionConfiguration.Board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;

						return factory.SaveHandlingZSaveExceptions();
					}
					else
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		static string GetLabel(BMBoardSectionViewModel viewModel)
		{
			var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(ConstrainedModeMenuItem) };
			var section = factory.Load<BMBoardSection>(viewModel.SectionPK);
			var releaseGroupPK = section.SectionConfiguration.ApplicableReleaseGroupPK;

			if (releaseGroupPK.IsValid)
			{
				var releaseGroupLink = ConstrainedModeHelper.GetLink(factory, section.MS_FC_Component, releaseGroupPK);

				if (releaseGroupLink != null && releaseGroupLink.FO_IsConstrainedMode)
				{
					return SwitchToNonConstrainedModeLabel;
				}
			}

			return SwitchToConstrainedModeLabel;
		}

		static string SwitchToConstrainedModeLabel
		{
			get { return Res.GetString("ba449d5f-7a81-4991-9729-31db1896c2b5", "Switch to Constrained Mode"); }
		}

		static string SwitchToNonConstrainedModeLabel
		{
			get { return Res.GetString("6be3f4bf-6331-4b8a-8862-f1d31ff0849b", "Switch to non-Constrained Mode"); }
		}
	}
}
