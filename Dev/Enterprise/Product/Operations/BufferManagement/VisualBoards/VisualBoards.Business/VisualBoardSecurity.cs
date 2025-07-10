using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.VisualBoards.Business
{
	public static class VisualBoardSecurity
	{
		public static bool CanCurrentUserEditBoard(IVisualBoardProvider provider)
		{
			var currentUser = provider.Factory.Load<IGlbStaff>(Env.CurrentUserPK);

			if (currentUser != null)
			{
				if (CurrentUserIsOwner(provider, currentUser))
				{
					return true;
				}

				if (CurrentUserCanEditAnyBoard(provider))
				{
					return true;
				}

				if (CurrentUserIsPartOfBoardOwnerGroup(provider, currentUser))
				{
					if (Env.Security.BMBoardEditTeamBoards.IsAllowed)
					{
						return true;
					}
				}
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "Baseline")]
		public static bool IsVisibleToCurrentCompany(IVisualBoardProvider provider)
		{
			var isVisible = provider.IsVisibleToCurrentCompany;

			if (!isVisible)
			{
				Globals.Message.Show(Res.GetString("3A0C2DB5-2205-464E-B9AA-4B04EE8A8A62", "Cannot open non-global visual boards belonging to other companies."));
			}

			return isVisible;
		}

		static bool CurrentUserIsOwner(IVisualBoardProvider provider, IGlbStaff currentUser)
		{
			return provider.OwnerStaffCode == currentUser.GS_Code;
		}

		static bool CurrentUserCanEditAnyBoard(IVisualBoardProvider provider)
		{
			return provider.EditCheckpoint.IsAllowed;
		}

		static bool CurrentUserIsPartOfBoardOwnerGroup(IVisualBoardProvider provider, IGlbStaff currentUser)
		{
			return !provider.OwnerGroupPK.IsEmpty
				&& currentUser.Groups.Cast<IBusiness>().Any(x => x.Identifier == provider.OwnerGroupPK);
		}
	}
}
