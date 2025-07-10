using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.GUI
{
	interface IDocumentViewExHost
	{
		string Text { get; set; }
		void ShowDocumentViewEx(IDocumentViewEx view);
		void ShowLogsView(IStmALogParent logParent);
		void Focus();
		void Close();
	}
}
