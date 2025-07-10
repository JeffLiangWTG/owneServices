using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Dash.Integration
{
	public interface IDashParseRequestService
	{
		IParseRequest Get(Guid storageDocPk);

		IParseRequest Create(IeDoc storageDoc);

		bool Exists(Guid storageDocPk);
	}
}
