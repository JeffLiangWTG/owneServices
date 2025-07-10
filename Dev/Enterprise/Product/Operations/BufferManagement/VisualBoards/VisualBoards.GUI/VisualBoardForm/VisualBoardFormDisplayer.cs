using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public static class VisualBoardFormDisplayer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public static VisualBoardForm ShowBoard(IVisualBoardProvider boardProvider, Func<BoardSlideshowViewModel, VisualBoardForm> formFunc = null, IEnumerable<string> args = null, Action formShownShowingOrFailedToShowCallback = null)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(VisualBoardFormDisplayer)}.{nameof(ShowBoard)}");
			var boardPk = boardProvider.PK;

			try
			{
				ReportVisualBoardOpen(boardProvider);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce($"Error to ReportVisualBoardOpen Board: PK:{boardPk} Name:{boardProvider.Name}", ex);
			}

			var formCache = OpenedFormCache.GetInstance();
			if (formCache.Contains(boardPk, FormCacheModuleName))
			{
				formCache.SwitchToCachedForm(boardPk, FormCacheModuleName);
				formShownShowingOrFailedToShowCallback?.Invoke();
				return null;
			}

			if (!boardsShowing.TryAdd(boardPk))
			{
				formShownShowingOrFailedToShowCallback?.Invoke();
				return null;
			}

			var strategy =
#if DEBUG
				overriddenStrategyGetter_ForTest.IsOverriden ? overriddenStrategyGetter_ForTest.Value() :
#endif
				new VisualBoardFormDisplayerStrategy();

			void formShownOrFailedToShow()
			{
				RemoveBoardFromCache(boardPk);
				formShownShowingOrFailedToShowCallback?.Invoke();
			}

			return strategy.OpenNewForm(boardProvider, formFunc: formFunc, args: args, formShownOrFailedToShowCallback: formShownOrFailedToShow);
		}

		static void ReportVisualBoardOpen(IVisualBoardProvider boardProvider)
		{
			var collector = ObjectFactory.New<IPAVEUsageCollector>();
			var boards = boardProvider.Boards.ToArray();

			if (boards.Length == 0)
			{
				return;
			}

			var isSlideshow = boards.Length > 1 || boards[0].BoardPK != boardProvider.PK;

			foreach (var board in boards.Where(b => b.Sections != null))
			{
				var eventData = new List<KeyValuePair<string, string>>();

				if (isSlideshow)
				{
					eventData.Add(new KeyValuePair<string, string>((NoResString)"Slideshow PK", boardProvider.PK.ToString()));
					eventData.Add(new KeyValuePair<string, string>((NoResString)"Slideshow Name", boardProvider.Name));
				}

				eventData.Add(new KeyValuePair<string, string>("PK", board.BoardPK.ToString()));
				eventData.Add(new KeyValuePair<string, string>((NoResString)"Name", board.BoardName));

				var sections = board.Sections.ToArray();

				var sectionTypeCount = sections
				.GroupBy(s => s.MS_SectionType)
				.Select(group => new KeyValuePair<string, string>(group.Key.ToString(), group.Count().ToString()));

				eventData.AddRange(sectionTypeCount);

				var componentTypeCount = sections
					.Where(s => s.MS_SectionType == "CMP" && s.Component != null)
					.GroupBy(s => s.Component.FC_Type)
					.Select(group => new KeyValuePair<string, string>(group.Key.ToString(), group.Count().ToString()));

				eventData.AddRange(componentTypeCount);

				collector.ReportVisualBoardOpen(eventData.ToDictionary(kv => kv.Key, kv => kv.Value));
			}
		}

		static void RemoveBoardFromCache(Guid boardPk)
		{
			boardsShowing.TryRemove(boardPk);
		}

		static readonly ConcurrentHashSet<Guid> boardsShowing = new ConcurrentHashSet<Guid>();

		internal static string FormCacheModuleName
		{
			get { return "VisualBoard"; }
		}

		#region For Test
#if DEBUG

		static readonly Overridable<Func<VisualBoardFormDisplayerStrategy>> overriddenStrategyGetter_ForTest = new Overridable<Func<VisualBoardFormDisplayerStrategy>>();

		public static IDisposable TemporarilyOverrideFormDisplayerStrategy_ForTest(VisualBoardFormDisplayerStrategy strategy)
		{
			return TemporarilyOverrideFormDisplayerStrategy_ForTest(() => strategy);
		}

		public static IDisposable TemporarilyOverrideFormDisplayerStrategy_ForTest(Func<VisualBoardFormDisplayerStrategy> strategyGetter)
		{
			overriddenStrategyGetter_ForTest.Value = strategyGetter;
			return new DisposableAction(() => overriddenStrategyGetter_ForTest.Value = null);
		}

#endif
		#endregion

	}
}
