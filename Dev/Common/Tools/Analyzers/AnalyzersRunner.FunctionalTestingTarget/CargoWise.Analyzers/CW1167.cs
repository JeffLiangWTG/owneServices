using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1167
	{
		public void ReportNudgeFailed()
		{
			for (var i = 0; i < 10; i++)
			{
				// CW1167 Db.DisposableActionForDbConnection() should be called as high as possible in the call stack of threading code, or new Http contexts, or entry points
				using (Db.DisposableActionForDbConnection())
				{
				}
			}

			Task.Factory.StartNew(() =>
			{
				DoWork();
			});

			ThreadPool.QueueUserWorkItem(_ =>
			{
				DoAnotherWork();
			});
		}

		void DoWork()
		{
			// CW1167 Db.DisposableActionForDbConnection() should be called as high as possible in the call stack of threading code, or new Http contexts, or entry points
			using (Db.DisposableActionForDbConnection())
			{
			}
		}

		void DoAnotherWork()
		{
			// CW1167 Db.DisposableActionForDbConnection() should be called as high as possible in the call stack of threading code, or new Http contexts, or entry points
			using (Db.DisposableActionForDbConnection())
			{
			}
		}
	}
}
