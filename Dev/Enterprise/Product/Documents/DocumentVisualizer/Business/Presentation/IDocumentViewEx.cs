using System;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentViewEx : IDisposable
	{
		string Text { get; set; }
		IMenuItemCollection MenuItems { get; }

		void IncreaseZoom(int factor);
		void DecreaseZoom(int factor);

		void CreateView();
		void ToggleStatusPanelVisibility(bool visible);
		void SetStatusPanelCaption(string caption);
		void ShowWatermark(string text);
		void ShowLogsView(IStmALogParent logParent);
		void NotifyMouseDown();
		void NotifyExiting();
		void Refresh();
		void Focus();
		void Exit();
	}
}
