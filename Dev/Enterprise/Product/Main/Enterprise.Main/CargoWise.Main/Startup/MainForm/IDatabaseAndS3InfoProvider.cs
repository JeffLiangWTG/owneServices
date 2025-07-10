using System.Collections.Generic;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Startup
{
	/// <summary>
	/// Interface for DatabaseInfoForm to provide data to display on UI.
	/// </summary>
	public interface IDatabaseAndS3InfoProvider
	{
		public string ServerAlias { get; }
		public string ServerMachine { get; }
		public string MainDbName { get; }
		public int MainConnectionSpid { get; }
		public IEnumerable<DbGroupSize> DbGroupSizes { get; }
		public string S3BucketName { get; }
		public string S3BucketServiceUrl { get; }
		public int S3BucketTimeout { get; }
		public string S3BucketSizeError { get; }
		public bool ShouldShowS3BucketInfo { get; }
	}
}
