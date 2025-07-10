using System.IO;
using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public interface IExternalPersister
	{
		(Stream stream, string versionId) RetrieveStream(ZGuid primaryKey);

		Stream RetrieveStream(ZGuid primaryKey, ZString versionId);

		(bool isSaved, string versionId) SaveStream(Stream stream, ZGuid primaryKey);

		bool Delete(ZGuid primaryKey);

		long GetObjectSize(ZGuid primaryKey);

		long GetBucketSizeInMb();

		bool AllowWrite { get; }
	}
}
