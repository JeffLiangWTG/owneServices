using System;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public sealed class DummyOperationalActionLog : DummyOperationalActionSectionLog, IOperationalActionLog
	{
		protected override void VerifyCore()
		{
			if (lastMasterCount != lastMasterMax)
			{
				throw new InvalidOperationException("Never reached the end");
			}
		}

		#region IOperationalActionLog Members
		public void SetMasterProgressMax(int max)
		{
			if (masterSet)
			{
				throw new InvalidOperationException("Master progress max already set");
			}

			masterSet = true;
			lastMasterMax = max;
		}

		public void BumpMasterProgress()
		{
			if (lastMasterCount == lastMasterMax)
			{
				throw new InvalidOperationException("Already reached the end");
			}

			base.VerifyCore();
			lastMasterCount++;
			ResetSection();
		}

		#endregion
		bool masterSet;
		int lastMasterMax;
		int lastMasterCount;
		public int LastMasterCountExposedForTest { get => lastMasterCount; set => lastMasterCount = value; }

		public int LastMasterMaxExposedForTest { get => lastMasterMax; set => lastMasterMax = value; }

		public bool MasterSetExposedForTest { get => masterSet; set => masterSet = value; }
	}
}
