using System;

namespace CargoWise.Common
{
	public interface IBackgroundDataSaverFactory
	{
		IBackgroundDataSaver Create(string saveFilePath, TimeSpan saveFrequency, BackgroundDataFileAction saveAction, Action<Exception> backgroundExceptionHandler);
	}

	public class BackgroundDataSaverFactory : IBackgroundDataSaverFactory
	{
		public IBackgroundDataSaver Create(string saveFilePath, TimeSpan saveFrequency, BackgroundDataFileAction saveAction, Action<Exception> backgroundExceptionHandler)
		{
			return new BackgroundDataSaver(saveFilePath, saveFrequency, saveAction, backgroundExceptionHandler);
		}
	}
}