using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	internal class DatabaseAndS3InfoProvider : IDatabaseAndS3InfoProvider
	{
		public DatabaseAndS3InfoProvider()
		{
			RetrieveS3BucketSize();
		}

		void RetrieveS3BucketSize()
		{
			if (ShouldShowS3BucketInfo)
			{
				try
				{
					var persister = ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3);
					s3BucketSize = persister.GetBucketSizeInMb();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					s3BucketSizeError = Res.GetString("6D835FB9-F8DF-48F1-90C6-243F17E0FA98", "There was a network issue while retrieving bucket size. Please try again later. If this issue persists, please contact your system administrator to check the configuration. Error message:") + " " + ex.Message;
				}
			}
		}

		#region IDatabaseInfoProvider

		public string ServerAlias => Db.ServerName;

		public string ServerMachine => Db.Connection.ServerNameReportedByDatabase;

		public string MainDbName => Db.DatabaseName;

		public int MainConnectionSpid => Db.Connection.SPID;

		public IEnumerable<DbGroupSize> DbGroupSizes
		{
			get
			{
				var dbSizeCollection = new DbSizeInfoCollection();
				var groupSizes = dbSizeCollection.GetDbGroupSizes();
				if (ShouldShowS3BucketInfo)
				{
					var bucketSize = new DbGroupSize
					{
						DbGroup = DbGroupEnum.S3Bucket,
						UsedSizeMb = s3BucketSize,
						DiskSizeMb = s3BucketSize
					};
					groupSizes = groupSizes.Concat(new[] { bucketSize });
				}
				return groupSizes;
			}
		}

		public string S3BucketName => SystemDataRegistry.Instance.DocManagerStorageBucketName.Value.Trim();

		public string S3BucketServiceUrl => SystemDataRegistry.Instance.EDocsStorageServiceUrl.Value.Trim();

		public int S3BucketTimeout => SystemDataRegistry.Instance.EDocsStorageConnectionTimeout.Value;

		public string S3BucketSizeError => s3BucketSizeError;

		public bool ShouldShowS3BucketInfo => (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI) &&
				SystemDataRegistry.Instance.EDocsStorageProvider.Value == Core.Constants.EDocsStorageProviders.Code.S3;

		#endregion

		string s3BucketSizeError;
		long s3BucketSize;
	}
}
