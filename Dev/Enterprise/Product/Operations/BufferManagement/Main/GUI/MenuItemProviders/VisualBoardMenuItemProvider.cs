using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public class VisualBoardMenuItemProvider : IFilterGridTopLevelMenuItemProvider, IVisualBoardMenuItemProvider
	{
		#region Get Menu Items

		public IEnumerable<MenuItem> GetMenuItems(ZFilterGridModule module)
		{
			var isJobWorkflowsOrTasksModule = module.ID == ModuleIDs.ProcessHeader || module.ID == ModuleIDs.ProcessTasks;

			if ((module.SupportsWorkflow || isJobWorkflowsOrTasksModule) && Env.Security.VisualBoards.IsAllowed && ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				var workflowType = isJobWorkflowsOrTasksModule ? null : module.WorkflowType;

				if (!string.IsNullOrEmpty(workflowType) || isJobWorkflowsOrTasksModule)
				{
					var factory = CreateFactory();
					var applicableSystems = BMSystem.GetAllApplicableBMSystems(workflowType, factory).Cast<IBMSystem>().ToArray();

					if (applicableSystems.Any())
					{
						var factoryForCaching = module.GridCollection?.Factory ?? factory;

						yield return new ZLazyPopulatingMenuItem(BaseMenuItemCaption, () => GetCachedOrNewMenuItems(factoryForCaching, module.ID.Name)) { Name = VisualBoardsMenuItemName };
					}
				}
			}
		}

		static ResourceString BaseMenuItemCaption => ResString.GetMultilingualString("276ac41a-6148-4714-b419-c6f896391f38", "Visual Boards");

		IEnumerable<ZMenuItem> GetCachedOrNewMenuItems(BusinessObjectFactory factory, string moduleName)
		{
			return factory.GetCachedValue("VisualBoardMenuItemProvider.GetCachedOrNewMenuItems." + moduleName, GetTopLevelMenuItems);
		}

		IEnumerable<ZMenuItem> GetTopLevelMenuItems()
		{
			var menuItems = new List<ZMenuItem>();

			AddBoardMenuItems(menuItems);
			AddSlideshowMenuItems(menuItems);

			return menuItems;
		}

		void AddBoardMenuItems(List<ZMenuItem> menuItems)
		{
			var boardDtos = GetBoardDtos().ToArray();

			if (boardDtos.Any())
			{
				var boardMenuItems = CreateBoardMenuItemsFromDtos(boardDtos);
				menuItems.AddRange(boardMenuItems);
			}
		}

		static void AddSlideshowMenuItems(List<ZMenuItem> menuItems)
		{
			var slideshowMenuItems = GetSlideshowMenuItems().Cast<MenuItem>().ToArray();

			if (slideshowMenuItems.Any())
			{
				menuItems.Add(new ZMenuItem("-"));
				var slideshowMenuItem = new ZMenuItem(Res.GetString("133ac41a-6148-4714-b419-c6f896391a21", "Slide Shows"));
				slideshowMenuItem.MenuItems.AddRange(slideshowMenuItems);
				menuItems.Add(slideshowMenuItem);
			}
		}

		IEnumerable<ZMenuItem> CreateBoardMenuItemsFromDtos(BoardDto[] boardDtos)
		{
			var menuItems = new List<ZMenuItem>();

			AddMenuItemsForMyReleaseGroup(boardDtos, menuItems);
			AddMenuItemsForOtherReleaseGroups(boardDtos, menuItems);
			AddMenuItemsForNonReleaseGroupBoards(boardDtos, menuItems);

			if (menuItems.Any())
			{
				menuItems.Insert(0, new ZMenuItem(SearchItemText, (s, e) =>
				{
					ShowBoardPickerForm(boardDtos);
				}));
				menuItems.Insert(1, new ZMenuItem("-"));
			}

			return menuItems;
		}

		static void AddMenuItemsForMyReleaseGroup(BoardDto[] boardDtos, List<ZMenuItem> menuItems)
		{
			var topLevelMenuItemText = ResString.GetMultilingualString("f275e0fb-0684-422f-8dd9-34e070d97d78", "Boards for my Release Groups");
			AddMenuItemsForReleaseGroupRelatedBoards(boardDtos, menuItems, topLevelMenuItemText, dto => dto.IsMemberOfReleaseGroup);
		}

		static void AddMenuItemsForOtherReleaseGroups(BoardDto[] boardDtos, List<ZMenuItem> menuItems)
		{
			var topLevelMenuItemText = ResString.GetMultilingualString("6778d82a-2fbb-4a90-a431-51fbebc31a78", "Boards for other Release Groups");
			AddMenuItemsForReleaseGroupRelatedBoards(boardDtos, menuItems, topLevelMenuItemText, dto => !dto.IsMemberOfReleaseGroup && !string.IsNullOrEmpty(dto.ReleaseGroup));
		}

		static void AddMenuItemsForReleaseGroupRelatedBoards(BoardDto[] boardDtos, List<ZMenuItem> menuItems, MultilingualString topLevelMenuItemText, Func<BoardDto, bool> boardDtoFilter)
		{
			var filteredDtos = boardDtos.Where(boardDtoFilter);
			var menuItemsToAdd = GetBoardMenuItemsGroupedByReleaseGroup(filteredDtos).ToArray();

			if (menuItemsToAdd.Any())
			{
				var topLevelMenuItem = new ZMenuItem(topLevelMenuItemText);
				topLevelMenuItem.MenuItems.AddRange(menuItemsToAdd);
				menuItems.Add(topLevelMenuItem);
			}
		}

		static IEnumerable<MenuItem> GetBoardMenuItemsGroupedByReleaseGroup(IEnumerable<BoardDto> dtos)
		{
			var menuItems = new List<ZMenuItem>();
			var dtosByReleaseGroup = dtos.GroupBy(x => x.ReleaseGroup).OrderBy(x => x.Key);

			foreach (var boardSet in dtosByReleaseGroup.Where(x => x.Any()))
			{
				var groupMenuItem = new ZMenuItem(boardSet.Key);
				menuItems.Add(groupMenuItem);

				var boardMenuItemsForGroup = boardSet.OrderBy(x => x.Name).Select(x => new VisualBoardMenuItem(x));

				foreach (var boardMenuItem in boardMenuItemsForGroup)
				{
					groupMenuItem.MenuItems.Add(boardMenuItem);
				}
			}

			return menuItems;
		}

		static void AddMenuItemsForNonReleaseGroupBoards(BoardDto[] boardDtos, List<ZMenuItem> menuItems)
		{
			var dtosForNonReleaseGroupBoards = boardDtos.Where(x => string.IsNullOrEmpty(x.ReleaseGroup)).ToArray();

			if (dtosForNonReleaseGroupBoards.Any())
			{
				var topLevelMenuItem = new ZMenuItem(ResString.GetMultilingualString("a2c3946b-25bf-47c2-9e0e-33f08dd00783", "Boards not associated with a Release Group"));
				var orderedDtos = dtosForNonReleaseGroupBoards.OrderBy(x => x.IsPublished).ThenBy(x => x.Name);

				var menuItemsForNonReleaseGroupBoards = orderedDtos.Select(GetMenuItemForNonGroupedBoardDto).Cast<MenuItem>().ToArray();
				topLevelMenuItem.MenuItems.AddRange(menuItemsForNonReleaseGroupBoards);
				menuItems.Add(topLevelMenuItem);
			}
		}

		static VisualBoardMenuItem GetMenuItemForNonGroupedBoardDto(BoardDto boardDto)
		{
			var menuItemText = boardDto.IsPublished ? boardDto.Name : Res.GetString("e9960405-85b3-4d78-95e8-a6789fce2fba", "{0} (private)", boardDto.Name);

			return new VisualBoardMenuItem(boardDto.PK, menuItemText);
		}

		static IEnumerable<SlideshowMenuItem> GetSlideshowMenuItems()
		{
			return GetSlideshowDtos().Select(x => new SlideshowMenuItem(x));
		}

		#endregion

		#region Getting Dtos from the database

		static IEnumerable<BoardDto> GetBoardDtos()
		{
			return GetDtos(GetBoardDtoCommand);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static DbCommand GetBoardDtoCommand()
		{
			const string baseQuery = @"
-- VisualBoardMenuItemProvider.GetVisualBoardMenuItems

SELECT		MB_PK as PK,
			MB_Name as Name,
			COALESCE(GG_Desc, '') as ReleaseGroup,
			MB_IsPublished as Published,
			CASE WHEN GK_PK IS NULL THEN 0 ELSE 1 END as IsMemberOfReleaseGroup
FROM		dbo.BMBoard
LEFT JOIN	dbo.GlbGroup ON	MB_GG_ReleaseGroup = GG_PK
LEFT JOIN	dbo.GlbGroupLink ON GK_GG = GG_PK AND GK_GS = @currentUserPk
WHERE		1=1
AND			MB_IsPublished = 1 {0}";

			const string unionQuery = @"
UNION ALL
SELECT		MB_PK,
			MB_Name,
			COALESCE(GG_Desc, ''),
			MB_IsPublished,
			CASE WHEN GK_PK IS NULL THEN 0 ELSE 1 END
FROM		dbo.BMBoard
LEFT JOIN	dbo.GlbGroup ON	MB_GG_ReleaseGroup = GG_PK
LEFT JOIN	dbo.GlbGroupLink ON GK_GG = GG_PK AND GK_GS = @currentUserPk
WHERE		1=1
AND			MB_IsPublished = 0
AND
(
			MB_GS_NKStaffCode = @staffCode
	OR		GK_PK IS NOT NULL
) {0}";

			var query = baseQuery;
			var currentUser = GlbStaff.CurrentUser;

			if (currentUser != null)
			{
				query += unionQuery;
			}

			var currentCompany = GlbCompany.CurrentCompany;
			var companyQueryArg = currentCompany == null ? string.Empty : @"
	OR		MB_GC_Company = @currentCompanyPk";
			var companyQuery = string.Format(CultureInfo.InvariantCulture, @"
AND
(
			MB_GC_Company IS NULL {0}
)", companyQueryArg);

			query = string.Format(CultureInfo.InvariantCulture, query, companyQuery);
			var command = Db.Connection.Command(query); // We want to avoid loading all these business objects just to get some strings for menu items.

			var parameters = new ZSqlParameterCollection();
			if (currentUser != null)
			{
				parameters.Add("@staffCode", currentUser.GS_Code, BMBoardSchema.MB_GS_NKStaffCode);
				parameters.Add("@currentUserPk", currentUser.PK, GlbGroupLinkSchema.GK_GS);
			}
			if (currentCompany != null)
			{
				parameters.Add("@currentCompanyPk", currentCompany.PK, BMBoardSchema.MB_GC_Company);
			}
			command.AddParameters(parameters);

			return command;
		}

		static IEnumerable<BoardDto> GetSlideshowDtos()
		{
			return GetDtos(GetSlideshowDtoCommand);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static DbCommand GetSlideshowDtoCommand()
		{
			const string query = @"
-- VisualBoardMenuItemProvider.GetSlideshowMenuItems

SELECT		MD_PK as PK,
			MD_Name as Name,
			'' as ReleaseGroup,
			MD_IsPublished as Published,
			0 as IsMemberOfReleaseGroup
FROM		dbo.BMBoardSlideshow
WHERE		1=1
AND			MD_IsPublished = 1
ORDER BY	MD_Name";

			return Db.Connection.Command(query); // We want to avoid loading all these business object just to get some strings for menu items.
		}

		static IEnumerable<BoardDto> GetDtos(Func<DbCommand> getCommandFunc)
		{
			using (var command = getCommandFunc())
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new BoardDto(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetInt32(4));
				}
			}
		}

		class BoardDto
		{
			public BoardDto(Guid pk, string name, string releaseGroup, bool isPublished, int isMemberOfReleaseGroupValue)
			{
				PK = pk;
				Name = name;
				ReleaseGroup = releaseGroup;
				IsPublished = isPublished;
				IsMemberOfReleaseGroup = isMemberOfReleaseGroupValue == 1;
			}

			public Guid PK { get; }
			public string Name { get; }
			public string ReleaseGroup { get; }
			public bool IsPublished { get; }
			public bool IsMemberOfReleaseGroup { get; }
		}

		#endregion

		#region IVisualBoardMenuItemProvider Members

		IEnumerable<MenuItem> IVisualBoardMenuItemProvider.GetMenuItems()
		{
			return GetTopLevelMenuItems();
		}

		#endregion

		#region Implementation

		static BusinessObjectFactory CreateFactory()
		{
			return new BusinessObjectFactory { NameForDebugging = "VisualBoardMenuItemProvider" };
		}

		public static string SearchItemText => Res.GetString("9E33F446-8D8A-417B-BABE-062E85C84DD2", "{0} Find a Board", "🔍");

		void ShowBoardPickerForm(BoardDto[] boardDtos)
		{
			var sortedBoards = boardDtos.OrderBy(x => x.Name);
			var elementsForList = sortedBoards.Select(x => BoardPickerViewModel.GetElementForBoardList(x.PK, x.Name)).ToArray();
			var boardList = new CodeDescriptionPairList();
			boardList.AddRange(elementsForList);

			var viewModel = new BoardPickerViewModel(boardList);
			var form = CreateBoardPickerForm(viewModel);
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		protected virtual VisualBoardPickerForm CreateBoardPickerForm(BoardPickerViewModel viewModel)
		{
			return new VisualBoardPickerForm(viewModel);
		}

		bool IFilterGridTopLevelMenuItemProvider.TryGetButtonDetail(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			if (item.Name == VisualBoardsMenuItemName)
			{
				buttonToolTip = Res.GetString("68f709f4-b14f-4e00-b368-4ad9c60eed5c", "Displays a Buffer Management Visual Board, identifying tasks and workflows in a visual management dashboard.");
				buttonImage = buttonImageActive = IconTypes.VisualBoards;

				return true;
			}

			return false;
		}

		public const string VisualBoardsMenuItemName = "VisualBoards";

		#endregion

		#region MenuItem Classes

		abstract class VisualBoardMenuItemBase<T> : ZMenuItem
			where T : BusinessObject, IVisualBoardProvider
		{
			protected VisualBoardMenuItemBase(BoardDto boardDto)
				: this(boardDto.PK, boardDto.Name)
			{
			}

			protected VisualBoardMenuItemBase(Guid providerPk, string menuItemText)
				: base(menuItemText)
			{
				Click += (sender, e) =>
				{
					MainThreadRunner.RunOnMainThread(() =>
					{
						var factory = CreateFactory();
						var board = factory.Load<T>(providerPk);

						VisualBoardFormDisplayer.ShowBoard(board);
					});
				};
			}
		}

		sealed class VisualBoardMenuItem : VisualBoardMenuItemBase<BMBoard>
		{
			internal VisualBoardMenuItem(BoardDto boardDto)
				: base(boardDto)
			{
			}

			internal VisualBoardMenuItem(Guid boardPk, string menuItemText)
				: base(boardPk, menuItemText)
			{
			}
		}

		sealed class SlideshowMenuItem : VisualBoardMenuItemBase<BMBoardSlideshow>
		{
			internal SlideshowMenuItem(BoardDto boardDto)
				: base(boardDto)
			{
			}
		}

		#endregion
	}
}
