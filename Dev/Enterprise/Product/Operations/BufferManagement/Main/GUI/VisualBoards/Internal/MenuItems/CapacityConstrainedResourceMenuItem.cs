using System;
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
	class CapacityConstrainedResourceMenuItem : ZToolStripMenuItem
	{
		internal CapacityConstrainedResourceMenuItem(GlbStaff resource, BMComponent buffer, BMBoardSectionViewModel boardSectionViewModel)
			: base(GetLabel(resource, buffer))
		{
			resourcePK = resource.PK;
			bufferPK = buffer.PK;
			this.boardSectionViewModel = boardSectionViewModel;

			Click += CapacityConstrainedResourceMenuItem_Click;
		}

		readonly ZGuid resourcePK;
		readonly ZGuid bufferPK;
		readonly BMBoardSectionViewModel boardSectionViewModel;

		static string GetLabel(GlbStaff resource, BMComponent buffer)
		{
			if (ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer))
			{
				return MarkAsNonCCRLabel;
			}
			else
			{
				return MarkAsCCRLabel;
			}
		}

		void CapacityConstrainedResourceMenuItem_Click(object sender, EventArgs e)
		{
			if (Text == MarkAsCCRLabel)
			{
				if (MarkAsCCR())
				{
					Text = MarkAsNonCCRLabel;
				}
			}
			else if (MarkAsNonCCR())
			{
				Text = MarkAsCCRLabel;
			}
		}

		bool MarkAsCCR()
		{
			if (EnsureHasSecurity())
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "CapacityConstrainedResourceMenuItem.MarkAsCCR" };
				var buffer = factory.Load<BMComponent>(bufferPK);
				var resource = factory.Load<GlbStaff>(resourcePK);

				if (buffer != null && resource != null)
				{
					if (ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer))
					{
						var message = Res.GetString("4a9cb54c-1422-4bfd-aa7f-fc578ddb837c", "{0} is already marked as capacity constrained.", resource.GS_FullName);
						Globals.Message.Show(message, CannotMarkAsCapacityConstrained, MessageBoxButtons.OK, DialogResult.OK);
					}
					else
					{
						return MarkAsCCR(resource, buffer);
					}
				}
			}

			return false;
		}

		bool MarkAsCCR(GlbStaff resource, BMComponent buffer)
		{
			var resourceLink = buffer.GetOrCreateResourceLink(resource.GS_Code);
			var message = Res.GetString("35c78d75-4c09-4076-b8c0-ae46e8c1d4b9", @"Mark {0} as capacity constrained?", resource.GS_FullName);

			if (resourceLink.FD_CapacityConstraintDetectedUtc.IsValid)
			{
				message += System.Environment.NewLine + Res.GetString("24863a7c-1292-4a4c-8406-af428d1fb694", "{0} was detected as capacity constrained {1}.",
					/*0*/ resource.GS_FullName,
					/*1*/ resourceLink.TimeConsideredCCR);
			}

			var result = Globals.Message.Show(message, MarkAsCCRLabel, MessageBoxButtons.OKCancel, DialogResult.OK);
			if (result == DialogResult.OK)
			{
				resource.DesignateAsCCR(buffer);
				UpdateBoardSystemLastEditTimeUtc(resource.Factory);

				resource.Factory.Save();
				return true;
			}
			else
			{
				return false;
			}
		}

		bool MarkAsNonCCR()
		{
			if (EnsureHasSecurity())
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "CapacityConstrainedResourceMenuItem.MarkAsNonCCR" };
				var buffer = factory.Load<BMComponent>(bufferPK);
				var resource = factory.Load<GlbStaff>(resourcePK);

				if (buffer != null && resource != null)
				{
					if (!ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, buffer))
					{
						Globals.Message.Show(Res.GetString("94577f18-76d3-477d-a1a4-301f4f499daf", "{0} is already marked as non-capacity constrained.", resource.GS_FullName));
						return true;
					}
					else
					{
						resource.DesignateAsNonCCR(buffer);
						UpdateBoardSystemLastEditTimeUtc(resource.Factory);

						factory.Save();

						Globals.Message.Show(Res.GetString("c4721eef-f797-4034-b1b2-1274fd5dccdc", "Marked {0} as non-capacity constrained.", resource.GS_FullName));

						return true;
					}
				}
			}

			return false;
		}

		void UpdateBoardSystemLastEditTimeUtc(BusinessObjectFactory factory)
		{
			var board = factory.Load<BMBoard>(boardSectionViewModel.BoardViewModel.BoardPK);
			board.MB_SystemLastEditTimeUtc = ZDateTime.UtcNow;
		}

		static bool EnsureHasSecurity()
		{
			if (!Env.Security.BMBoardConstrainedResource.IsAllowed)
			{
				Env.Security.BMBoardConstrainedResource.ShowError();
				return false;
			}

			return true;
		}

		static string MarkAsCCRLabel
		{
			get { return Res.GetString("a689e928-84c4-4199-b923-a65df03310b3", "Mark as capacity constrained"); }
		}

		static string MarkAsNonCCRLabel
		{
			get { return Res.GetString("d64450e0-e4e0-4d41-8ec1-fb057689d0e8", "Mark as non-capacity constrained"); }
		}

		static string CannotMarkAsCapacityConstrained
		{
			get { return Res.GetString("65151d6c-de4a-483b-b9d8-4beb22a4a2fc", "Cannot mark as capacity constrained"); }
		}
	}
}
