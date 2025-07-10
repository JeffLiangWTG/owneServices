using System;
using System.IO;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface INativeObjectSerializer
	{
		Stream SerializeToScavengingOrganization(Guid pk, string tableName);
	}
}
