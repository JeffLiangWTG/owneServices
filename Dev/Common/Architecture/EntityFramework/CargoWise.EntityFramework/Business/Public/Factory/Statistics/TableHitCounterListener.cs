#region BaseTestListener
#if DEBUG

using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework
{
	public class TableHitCounterListener : BaseTestListener
	{
		public static TableHitCounterListener Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new TableHitCounterListener();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static TableHitCounterListener fInstance;

		TableHitCounterListener()
		{
		}

		public override void AfterEachTest(DateTime endTime)
		{
			TableHitCounter.StopReportingHitsOnAllTables();
			base.AfterEachTest(endTime);
		}
	}
}

#endif
#endregion
