using System;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public interface ICodeMappingHelper
	{
		void CreateCodeMapping(CodeMapping codeMapping);
		string MapLocalCode(string relationship, object foreignCode, string ownerCode);
		string MapLocalCode(string relationship, object foreignCode, Guid ownerPK);
		string MapLocalCode(OrgPatternMatchOverride patternMatchOverride);
		Guid FindOwnerOrgPK(string ownerOrgCode);
	}
}