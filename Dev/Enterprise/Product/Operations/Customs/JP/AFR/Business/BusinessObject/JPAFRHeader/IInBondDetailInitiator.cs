using System;

namespace Enterprise.Customs.JP.AFR.Business
{
	public interface IInBondDetailInitiator
	{
		void NotifyUserOfAnInvalidOperation(string text);
		event EventHandler OnDisposing;
	}
}
