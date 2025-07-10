using System;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	public class InBondDetailInitiatorTestHelper : IInBondDetailInitiator, IDisposable
	{
		#region IInBondDetailInitiator Members

		public void NotifyUserOfAnInvalidOperation(string text)
		{
			TextResult = text;
		}
		public string TextResult;

		public event EventHandler OnDisposing
		{
			add { onDisposing += value; }
			remove { onDisposing -= value; }
		}
		event EventHandler onDisposing;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (onDisposing != null)
			{
				onDisposing(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
