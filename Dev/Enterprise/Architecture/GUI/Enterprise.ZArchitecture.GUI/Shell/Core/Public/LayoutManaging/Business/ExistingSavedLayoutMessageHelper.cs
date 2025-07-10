using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	class ExistingSavedLayoutMessageHelper
	{
		internal ExistingSavedLayoutMessageHelper(SaveLayoutBizO layoutToSave)
		{
			if (!layoutToSave.LayoutNameMultilingual.IsEmpty)
			{
				ExistingLayout = GetExistingLayout(layoutToSave);

				if (ExistingLayout != null)
				{
					ExistingLayoutExistsMessage = GetLayoutAlreadyExistsMessage(layoutToSave);
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Calm down. LayoutName is used as an identifier.")]
		static StmModuleFilter GetExistingLayout(SaveLayoutBizO layoutToSave)
		{
			var query = new ZQuery(StmModuleFilterSchema.S9_ModuleID, layoutToSave.S9_ModuleID);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterName, layoutToSave.LayoutName);
			var matches = layoutToSave.Factory.Load<StmModuleFilter>(query);

			if (matches.Any())
			{
				var userDefinedMatch = matches.FirstOrDefault(x => x.IsUserDefinedFilter);

				if (userDefinedMatch != null || layoutToSave.IsUserDefinedFilter)
				{
					return userDefinedMatch ?? matches.First();
				}

				if (layoutToSave.PublishLayout)
				{
					var publishedMatches = matches.Where(x => x.S9_IsPublished);
					return publishedMatches.FirstOrDefault(x => ExistingLayoutConflictsWithProposedLayout(layoutToSave, x.S9_GC));
				}

				var currentUserPk = EnvProxy.Instance.CurrentUser.PK;
				return matches.FirstOrDefault(x => !x.S9_IsPublished && x.S9_RelatedEntityID == currentUserPk && ExistingLayoutConflictsWithProposedLayout(layoutToSave, x.S9_GC));
			}

			return null;
		}

		static bool ExistingLayoutConflictsWithProposedLayout(SaveLayoutBizO proposedLayout, ZGuid existingLayoutCompany)
		{
			return proposedLayout.PublishAcrossAllCompanies || existingLayoutCompany == ZGuid.Empty || existingLayoutCompany == Env.CurrentCompanyPK;
		}

		internal string ExistingLayoutExistsMessage { get; private set; }

		internal bool AllowUserToSaveWithWarning { get; private set; }

		internal bool LayoutExists => !string.IsNullOrEmpty(ExistingLayoutExistsMessage);

		internal bool IsExistingLayoutUserDefinedFilterStrip { get; private set; }

		internal StmModuleFilter ExistingLayout { get; private set; }

		string GetLayoutAlreadyExistsMessage(SaveLayoutBizO layoutToSave)
		{
			var layoutName = layoutToSave.LayoutNameMultilingual.GetUnresolvedString();
			IsExistingLayoutUserDefinedFilterStrip = ExistingLayout.IsUserDefinedFilter;

			if (!layoutToSave.IsUserDefinedFilter)
			{
				if (IsExistingLayoutUserDefinedFilterStrip)
				{
					return Res.GetString("3f2d766e-3b6e-4d9c-a4f7-a72f97a4d78a", "The user-defined filter [{0}] already exists and cannot be overwritten by a saved layout. Please enter a different name.", layoutName);
				}

				if (!layoutToSave.PublishAcrossAllCompanies && ExistingLayout.S9_GC == ZGuid.Empty)
				{
					return Res.GetString("51a2647c-f5dd-4657-b3e2-0553b1d37296", "The filter layout [{0}] already exists and is published across all companies. To overwrite the existing layout, please enable the 'Publish across all companies' option.", layoutName);
				}

				AllowUserToSaveWithWarning = true;
				return Res.GetString("295ec2ba-4b69-4768-9a12-bfca48047483", "The filter layout [{0}] already exists, do you want to overwrite the existing layout?", layoutName);
			}

			if (IsExistingLayoutUserDefinedFilterStrip)
			{
				if (!ExistingLayout.S9_IsPublished && ExistingLayout.S9_RelatedEntityID != EnvProxy.Instance.CurrentUser.PK)
				{
					var otherUser = ExistingLayout.Factory.Load<IGlbStaff>(ExistingLayout.S9_RelatedEntityID);

					return Res.GetString("e32da14a-bd64-4da8-ba55-9d3d51f02a24", "The layout description [{0}] is in use as an unpublished User-Defined Filter. [{0}] was created by {1} and cannot be overwritten by a different user. Please enter a different description.", layoutName, otherUser.GS_FullName);
				}

				if (!EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed && ExistingLayout.CreatorOrEarlyestUser?.PK != EnvProxy.Instance.CurrentUser.PK)
				{
					return EnvProxy.Instance.Security.EditUserDefinedFilters.ErrorMessageForNotAllowed;
				}

				AllowUserToSaveWithWarning = true;
				return Res.GetString("dbea139b-0cb2-408e-8ade-f321837ab460", "The user-defined filter [{0}] already exists, do you want to overwrite the existing user-defined filter?", layoutName);
			}

			return Res.GetString("75701ae0-3fc1-418f-9733-d596b47d7675", "The filter layout [{0}] already exists and cannot be overwritten by a user-defined filter. Please enter a different name.", layoutName);
		}
	}
}
