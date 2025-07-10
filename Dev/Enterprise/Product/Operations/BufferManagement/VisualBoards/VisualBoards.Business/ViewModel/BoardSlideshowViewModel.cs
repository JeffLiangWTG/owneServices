using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.VisualBoards.Business
{
	public class BoardSlideshowViewModel : NonPersistentBusinessObject, IObsoleteValidation, IFilterable
	{
		public BoardSlideshowViewModel(IVisualBoardProvider source, bool isPreview = false)
			: this(source, null, null, isPreview)
		{
		}

		public BoardSlideshowViewModel(IVisualBoardProvider source, IVisualBoardForm board, BoardFactoryProvider factoryProvider, bool isPreview = false)
		{
			IsPreview = isPreview;
			BoardForm = board;

			var sectionTypes = GetAllSectionTypesOnThisSlideshow(source);
			const string componentSection = "CMP";  // Hard coded constant to avoid adding a reference to BufferManagement.Business
			const string workshopAttendeeSection = "WSA"; // Hard coded constant to avoid adding a reference to Workflow Modeler

			hasComponentSections = sectionTypes.Any(s => s.In(componentSection, workshopAttendeeSection));

			foreach (var sectionType in sectionTypes)
			{
				var serviceProvider = SectionDescriptorProvider.Get(sectionType) as IBoardFactoryServiceProvider;

				if (serviceProvider != null)
				{
					foreach (var service in serviceProvider.GetFactoryServices(source))
					{
						services.Add(service);
					}
				}
			}

			slideshowFrames = source.Boards.ToArray();

			FactoryProvider = factoryProvider ?? new SharedBoardFactoryProvider(this);
			Source = source;

			RefreshSeconds = source.StartingRefreshIntervalInMinutes * 60;

			if (RefreshSeconds == 0)
			{
				RefreshSeconds = DefaultBoardRefreshSeconds;
			}

			SubscribeFullRefreshForced();
		}

		readonly ISlideShowFrame[] slideshowFrames;

		public IVisualBoardForm BoardForm { get; set; }
		public BoardFactoryProvider FactoryProvider { get; set; }

		public IVisualBoardProvider Source { get; }

		public IDispatcher Dispatcher { get; set; }

		public IDataRefreshBusSubscriber DataRefreshBusSubscriber { get; set; }

		public bool IsPreview { get; }

		#region Navigation

		public BoardViewModel CurrentBoardViewModel
		{
			get { return BoardViewModels[currentBoardIndex]; }
		}

		public BoardViewModel PreviousBoardViewModel
		{
			get { return BoardViewModels[previousBoardIndex]; }
		}

		public void MoveForward()
		{
			using (CurrentBoardChanging())
			{
				previousBoardIndex = currentBoardIndex;
				currentBoardIndex = GetNextPosition();
			}
		}

		public void MoveBackwards()
		{
			using (CurrentBoardChanging())
			{
				previousBoardIndex = currentBoardIndex;
				currentBoardIndex = GetPreviousPosition();
			}
		}

		IDisposable CurrentBoardChanging()
		{
			CurrentBoardViewModel.FullRefreshForced -= CurrentBoardViewModel_FullRefreshForced;

			return new DisposableAction(SubscribeFullRefreshForced);
		}

		void SubscribeFullRefreshForced()
		{
			CurrentBoardViewModel.FullRefreshForced += CurrentBoardViewModel_FullRefreshForced;
		}

		void CurrentBoardViewModel_FullRefreshForced(object sender, EventArgs e)
		{
			if (BoardForm == null)
			{
				ErrorReporter.ReportOnce("Somehow a BoardSlideshowViewModel was constructed without a reference to its board form.");
			}
			else
			{
				BoardForm.NotifyFullReloadRequired();
			}
		}

		int GetPreviousPosition()
		{
			var index = currentBoardIndex - 1;
			if (index < 0)
			{
				index = BoardViewModels.Length - 1;
			}

			return index;
		}

		int GetNextPosition()
		{
			var index = currentBoardIndex + 1;
			if (index >= BoardViewModels.Length)
			{
				index = 0;
			}

			return index;
		}

		int currentBoardIndex;
		int previousBoardIndex;

		public void ReloadCurrentBoardViewModel()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "BMBoardSlideShowViewModel.ReloadCurrentBoardViewModel", RefreshEnabled = false };
			var board = factory.Load<IBMBoard>(CurrentBoardViewModel.BoardPK);
			if (board != null)
			{
				var currentIntervalInSeconds = CurrentBoardViewModel.IntervalInSlideShowSeconds;
				BoardViewModels[currentBoardIndex] = new BoardViewModel(board, this) { IntervalInSlideShowSeconds = currentIntervalInSeconds };
			}
		}

		public bool HasMultipleBoards => BoardViewModels.Length > 1;

		public bool HasComponentSections => hasComponentSections;

		readonly bool hasComponentSections;

		public bool IsSlideshow => Source is IBMBoardSlideshow;

		#endregion

		#region Related Objects

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public BoardViewModel[] BoardViewModels
		{
			get
			{
				if (boardViewModels == null)
				{
					var viewModels = new List<BoardViewModel>();

					foreach (var boardDetail in slideshowFrames)
					{
						var board = Source.Factory.Load<IBMBoard>(boardDetail.BoardPK);
						if (board != null)
						{
							viewModels.Add(new BoardViewModel(board, this) { IntervalInSlideShowSeconds = boardDetail.DisplayTimeSeconds });
						}
					}

					boardViewModels = viewModels.ToArray();
				}

				return boardViewModels;
			}
		}

		BoardViewModel[] boardViewModels;

		public void ReloadBoards()
		{
			boardViewModels = null;
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RefreshSeconds { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int DefaultBoardRefreshSeconds = 600;

		#endregion

		#region IFilterable Members

		public FilterManager FilterManager
		{
			get { return filterManager ?? (filterManager = new FilterManager(this)); }
			set { filterManager = value; }
		}

		FilterManager filterManager;

		public bool IsInBoardMeeting => FilterManager.IsApplied(typeof(BoardMeetingModeFilter));

		string IFilterable.Name
		{
			get { return Res.GetString("2405f579-b54b-49c8-bc69-add06da9e2f6", "Visual Board Slide Show"); }
		}

		IFilterable IFilterable.Parent
		{
			get { return null; }
		}

		IEnumerable<IFilterable> IFilterable.Children
		{
			get { return BoardViewModels; }
		}

		void IFilterable.RefreshFilters(bool requiresFullRedraw)
		{
			foreach (var child in ((IFilterable)this).Children)
			{
				child.RefreshFilters(requiresFullRedraw);
			}
		}

		void IFilterable.RefreshSelectedFilters(IEnumerable<IBoardFilter> filters)
		{
			foreach (var child in ((IFilterable)this).Children)
			{
				child.RefreshSelectedFilters(filters);
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var viewModel = new BoardSlideshowViewModel(Source, BoardForm, FactoryProvider) { Dispatcher = Dispatcher };

			lock (ServiceLocker)
			{
				foreach (var service in Services)
				{
					viewModel.services.Add(service);
				}
			}

			return viewModel;
		}

		#endregion

		#region Services

		public void RefreshServices(BoardServiceStalenessPolicy stalenessPolicy)
		{
			lock (ServiceLocker)
			{
				foreach (var service in Services)
				{
					if ((service.StalenessPolicy & stalenessPolicy) == stalenessPolicy)
					{
						service.ClearCache();
					}
				}
			}
		}

		public void ReplaceService<T>(T service)
			where T : IBoardFactoryService
		{
			lock (ServiceLocker)
			{
				services.RemoveWhere(s => s is T);
				services.Add(service);
			}
		}

		internal IEnumerable<IBoardFactoryService> Services => services;
		readonly HashSet<IBoardFactoryService> services = new HashSet<IBoardFactoryService>();

#if DEBUG
		public HashSet<IBoardFactoryService> Services_ExposedForTest => services;
#endif

		public object ServiceLocker { get; } = new object();

		static IEnumerable<string> GetAllSectionTypesOnThisSlideshow(IVisualBoardProvider source)
		{
			return source.Boards.Select(b => b.BoardPK)
				.Distinct()
				.Select(pk => source.Factory.Load<IBMBoard>(pk))
				.SelectMany(b => b.Sections)
				.Select(s => s.MS_SectionType.ToString())
				.Distinct();
		}

		#endregion
	}
}
