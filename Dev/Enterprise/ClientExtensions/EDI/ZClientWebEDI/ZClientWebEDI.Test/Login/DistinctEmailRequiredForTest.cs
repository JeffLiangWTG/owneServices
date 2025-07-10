using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class DistinctEmailRequiredForTest : DistinctEmailRequired
	{
		public void DoPageLoad()
		{
			try
			{
				base.OnLoad(EventArgs.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException)
				{
					throw;
				}
			}
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForTest();
			result.OnCustomSessionStart();
			return result;
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}