using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.ZClientCCP.GUI
{
	public class CCPMenu : Customs.AU.Declaration.GUI.EDIMenu
	{
		static Customs.AU.Declaration.GUI.EDIMenu NewDelegate()
		{
			return new CCPMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected void DataImporter_FileRowProcessed(object sender, ProcessedEventArgs e)
		{
			DataTransferForm.SetProcessProgress(e.PercentageComplete, e.ProcessedCount, e.FailureCount, e.LogEntry);
		}

		protected void DataImporter_ProcessCompleted(object sender, EventArgs e)
		{
			DataTransferForm.FinishProcess();
		}

		protected DataTransferForm DataTransferForm;
	}
}
