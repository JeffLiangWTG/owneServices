using System;
using System.Threading;

namespace Enterprise.RemotePrinting.Client
{
	public delegate void RunApplicationDelegate();

	public class SingletonApplicationRunner
	{
		static readonly object setupLock = new object();

		public bool Run(RunApplicationDelegate runApp, string configName)
		{
			var webServiceUrl = GetWebServiceUrl(configName);
			var urlKey = webServiceUrl?.ToUpper()
				.Replace(Uri.UriSchemeHttps.ToUpper(), "")
				.Replace(Uri.UriSchemeHttp.ToUpper(), "");

			MutexKey = "Global\\Enterprise.RemotePrinting.Client." + configName + ".C953B27C6C294859A2E4C12C7B57E34F";
			MutexKeyURL = "Global\\Enterprise.RemotePrinting.Client." + urlKey + ".C953B27C6C294859A2E4C12C7B57E34F";
			bool lockAcquired = TryGetAppLock();

			if (lockAcquired)
			{
				try
				{
					runApp();
				}
				finally
				{
					ReleaseLock();
				}
			}

			return lockAcquired;
		}

		protected virtual string GetWebServiceUrl(string configName)
		{
			var webClientConfiguration = ConnectionRegistryManager.Instance.GetWebClientConfiguration(configName);
			return webClientConfiguration.WebServiceUrl;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One WebPrint is a seperate entity regardless of product")]
		public bool TryGetAppLock()
		{
			bool acquired = false;

			lock (setupLock)
			{
				mutexConfigName = new Mutex(false, MutexKey);
				BlockCurrentThread(mutexConfigName, AnotherCopyRunningMessageUsingSameConfig);

				mutexUrl = new Mutex(false, MutexKeyURL);
				BlockCurrentThread(mutexUrl, AnotherCopyRunningMessageUsingSameUrl);
			}

			return acquired;

			void BlockCurrentThread(Mutex mutex, string errorMessage)
			{
				try
				{
					if (!mutex.WaitOne(TimeSpan.Zero, false))
					{
						AnotherCopyRunningMessage = errorMessage;
						acquired = false;
					}
					else
					{
						acquired = true;
					}
				}
				catch (AbandonedMutexException)
				{
					acquired = true;
				}
			}
		}

		public void ReleaseLock()
		{
			lock (setupLock)
			{
				if (mutexConfigName != null)
				{
					mutexConfigName.ReleaseMutex();
					mutexConfigName.Close();
				}
				if (mutexUrl != null)
				{
					mutexUrl.ReleaseMutex();
					mutexUrl.Close();
				}
			}
		}

		Mutex mutexConfigName;
		Mutex mutexUrl;
		string MutexKey
		{
			get => mutexKey;
			set => mutexKey = value;
		}
		string mutexKey;

		string MutexKeyURL
		{
			get => mutexKeyUrl;
			set => mutexKeyUrl = value;
		}
		string mutexKeyUrl;

		public string AnotherCopyRunningMessage
		{
			get => anotherCopyRunningMessage;
			set => anotherCopyRunningMessage = value;
		}
		string anotherCopyRunningMessage;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One WebPrint is a seperate entity regardless of product")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		public const string AnotherCopyRunningMessageUsingSameConfig = "Another instance of the CargoWise One WebPrint Client is already running for this configuration.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One WebPrint is a seperate entity regardless of product")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		public const string AnotherCopyRunningMessageUsingSameUrl = "Another instance of the CargoWise One WebPrint Client is already running for this web url.";
	}
}
