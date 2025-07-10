namespace CargoWise.Bi.Common
{
	using System;

	public interface IBiLogger
	{
		void Complete(string message);
		void Fail(string message);
		void Fail(Exception ex);
		void Fail(string message, Exception ex);
		void StartTask(string message);
		void StartSubtask(string message);
	}
}
