using System;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IProgressManager : IDisposable
	{
		void Start();
		void UpdateStatus(string message);
	}
}
