using System;

namespace Enterprise.DataTransfer
{
	public interface IProgressSupporter
	{
		event EventHandler Progress;

		void OnProgress();
	}
}
