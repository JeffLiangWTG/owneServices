using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public interface ICodeMappingRepository
	{
		OrgPatternMatchOverride New();
		OrgPatternMatchOverride Load(string relationship, object foreignCode, Guid ownerPK);
	}
}