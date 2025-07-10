using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class LayoutSaveManager
	{
		/// <summary>
		/// Save the changes users have made on Manage Layout form. Users can rename a filter, delete a filter or change SaveColumnLayout setting for a filter.
		/// </summary>
		/// <param name="layoutNamesChanged">first string: original name, second string: changed name</param>
		/// <param name="layoutsDeleted">a list of filter original names that have been deleted</param>
		/// <param name="layoutSaveColumnSettingsChanged">a list of filter original names that have changed 'SaveLayoutColumn'</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "There is no benefit to creating a new type for this.")]
		public void SaveLayoutChanges(IModifyModuleAndGridLayout layoutManageable, Dictionary<ZGuid, string> layoutNamesChanged, IEnumerable<ZGuid> layoutsDeleted, IEnumerable<(ZGuid, ZBool, ZBool)> layoutSaveColumnSettingsChanged, ZGuid gridColourLayoutID)
		{
			ChangeSaveColumnSettings(layoutManageable, layoutSaveColumnSettingsChanged, gridColourLayoutID);

			DeleteLayouts(layoutManageable, layoutsDeleted);

			//This should happen at the last. LayoutName is used to load StmModuleFilter and if the change happens earlier than the other two modifications, then
			//system would not be able to find the related StmModuleFilter record.
			ChangeLayoutNames(layoutManageable, layoutNamesChanged);

			layoutManageable.Factory.Save();
		}

		#region Implementation

		void ChangeSaveColumnSettings(IModifyModuleAndGridLayout layoutManageable, IEnumerable<(ZGuid Pk, ZBool SaveColumnLayout, ZBool SaveGridColour)> layoutSaveColumSettingsChanged, ZGuid gridColourLayoutID)
		{
			foreach (var columnLayout in layoutSaveColumSettingsChanged)
			{
				var userLayoutStorage = layoutManageable.FindLayout(columnLayout.Pk);

				if (userLayoutStorage != null && !userLayoutStorage.IsDeleted)
				{
					if (userLayoutStorage.SaveColumnLayout != columnLayout.SaveColumnLayout)
					{
						userLayoutStorage.SaveColumnLayout = columnLayout.SaveColumnLayout;
					}

					if (userLayoutStorage.SaveGridColourLayout != columnLayout.SaveGridColour)
					{
						userLayoutStorage.SaveGridColourLayout = columnLayout.SaveGridColour;

						if (!userLayoutStorage.SaveGridColourLayout)
						{
							userLayoutStorage.GridColourLayoutID = ZGuid.Empty;
						}
						else if (userLayoutStorage.GridColourLayoutID.IsEmpty && !gridColourLayoutID.IsEmpty)
						{
							userLayoutStorage.GridColourLayoutID = gridColourLayoutID;
						}
					}
				}
			}
		}

		void DeleteLayouts(IModifyModuleAndGridLayout layoutManageable, IEnumerable<ZGuid> layoutsDeleted)
		{
			foreach (var layoutDeletedPK in layoutsDeleted)
			{
				var userLayoutStorage = layoutManageable.FindLayout(layoutDeletedPK);

				if (userLayoutStorage != null)
				{
					var query = new ZQuery();
					query.AddToFilter(StmLinkSchema.STL_LinkType, StmLinkConstants.FavoriteLayoutFilters);
					query.AddToFilter(StmLinkSchema.STL_ItemPK, userLayoutStorage.PK);
					query.AddToFilter(StmLinkSchema.STL_ModuleID, layoutManageable.LayoutSetIdentifierToSaveANewLayoutWith);

					var links = layoutManageable.Factory.Load<StmLink>(query);
					foreach (var link in links)
					{
						link.Delete();
					}

					userLayoutStorage.Delete();
				}
			}
		}

		void ChangeLayoutNames(IModifyModuleAndGridLayout layoutManageable, Dictionary<ZGuid, string> layoutNamesChanged)
		{
			foreach (var layoutPK in layoutNamesChanged.Keys)
			{
				var userLayoutStorage = layoutManageable.FindLayout(layoutPK);

				if (userLayoutStorage != null && !userLayoutStorage.IsDeleted)
				{
					userLayoutStorage.ColumnLayoutName = layoutNamesChanged[layoutPK];
				}
			}
		}

		#endregion
	}
}
