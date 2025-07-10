using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	class DummyBusinessObjectForTest
	{
		public DummyBusinessObjectForTest(LVXBulkConsolidateProcessor processor)
		{
			this.processor = processor;
			this.processor.OnProgress += new BulkConsolidateProgressEventHandler(DataProcess_OnProgress);
			this.processor.OnNotify += DataProcess_OnNotify;
			this.messages = new List<ZString>();
		}
		protected readonly LVXBulkConsolidateProcessor processor;

		void DataProcess_OnProgress(object sender, BulkConsolidateProgressEventArgs e)
		{
			processValue = e.percentComplete;
			if (processValue == 80)
			{
				processor.Cancel();
			}
		}

		internal void Reset()
		{
			messages.Clear();
			processValue = 0;
		}

		void DataProcess_OnNotify(string message)
		{
			messages.Add(message);
		}
		internal List<ZString> messages;
		internal int processValue;
	}
}
