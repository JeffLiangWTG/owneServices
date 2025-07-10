using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public class VisualBoardFormDisplayerStrategy
	{
		public VisualBoardForm OpenNewForm(IVisualBoardProvider boardProvider, Func<BoardSlideshowViewModel, VisualBoardForm> formFunc = null, IEnumerable<string> args = null, Action formShownOrFailedToShowCallback = null)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(OpenNewForm)}");
			using (var licensing = new VisualBoardLicensedComponent())
			{
				licensing.Login();
			}

			return OpenForm(boardProvider, formFunc ?? (viewModel => new VisualBoardForm(viewModel)), args, formShownOrFailedToShowCallback);
		}

		VisualBoardForm OpenForm(IVisualBoardProvider boardProvider, Func<BoardSlideshowViewModel, VisualBoardForm> formFunc, IEnumerable<string> args, Action formShownOrFailedToShowCallback)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(OpenForm)}");
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(VisualBoardFormDisplayerStrategy) };
			var reloadedBoardProvider = boardProvider.ReloadInFactory(factory);
			var viewModel = new BoardSlideshowViewModel(reloadedBoardProvider);

			var form = formFunc(viewModel);

			viewModel.BoardForm = form;

			if (!form.IsDisposed)
			{
				form.LoadPersistedFormArgs(args);
				AddFormToCache(form, reloadedBoardProvider.PK);

				SubscribeToEvents(form, () =>
				{
					formShownOrFailedToShowCallback?.Invoke();
				});

				using (form.SuspendUpdatingSessionStateOnActivation())
				{
					form.Show();
				}

				form.BeginInvokeSafe(() =>
				{
					using (form.SuspendUpdatingSessionStateOnActivation())
					{
						form.Activate();
					}
				});
			}
			return form;
		}

		void SubscribeToEvents(VisualBoardForm form, Action formShownOrFailedToShowCallback)
		{
			EventHandler formShown = null;
			formShown = (s, e) =>
			{
				form.Shown -= formShown;
				formShownOrFailedToShowCallback.Invoke();
				OnShown();
			};
			form.Shown += formShown;

			SubscribeToEventsCore(form);
		}

		protected virtual void SubscribeToEventsCore(VisualBoardForm form)
		{
		}

		protected virtual void OnShown() { }

		static void AddFormToCache(VisualBoardForm form, Guid pkForFormCache)
		{
			var cache = OpenedFormCache.GetInstance();
			if (!cache.Contains(pkForFormCache, VisualBoardFormDisplayer.FormCacheModuleName))
			{
				cache.Add(pkForFormCache, form, VisualBoardFormDisplayer.FormCacheModuleName);
			}
		}
	}
}
