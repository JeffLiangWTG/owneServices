using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.IO;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Pentant
{
	[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]  // Stupid effing CA - I am disposing the innter disposable!
	class PentantFtpOptionsProvider : IProviderOfTriggerFtpOptions, IDisposable
	{
		public PentantFtpOptionsProvider(ICredentials credentialsProvider, string url)
		{
			this.credentialsProvider = credentialsProvider;
			this.url = url;
			TempFolder = new TempDirectory();
		}

		public bool VerboseLogging => true;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int SleepTimeInSeconds => GBCustomsDataRegistry.Instance.PentantSleepTimeInSeconds.Value;

		public string UriString => url;

		public string Username => credentialsProvider.GetCredential(null, null).UserName;

		public string Password => credentialsProvider.GetCredential(null, null).Password;

		public string LocalEnterpriseSharedFolderName => TempFolder.DirectoryName;

		public int FtpReadTimeout => RawDataRegistry.Instance.FTPReadTimeout.Value;

		public int FtpConnectionTimeout => RawDataRegistry.Instance.FTPConnectionTimeout.Value;

		public readonly TempDirectory TempFolder;
		readonly ICredentials credentialsProvider;
		readonly string url;

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]  // Stupid effing CA - I am disposing the innter disposable!
		public void Dispose()
		{
			TempFolder?.Dispose();
		}
	}
}
