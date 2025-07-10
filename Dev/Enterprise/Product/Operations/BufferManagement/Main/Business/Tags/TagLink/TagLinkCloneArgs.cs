using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class TagLinkCloneArgs : BusinessObjectCloneArgs
	{
		public TagLinkCloneArgs(ZGuid parentID, bool suspendAddTagValidation, bool isRuleUsageScopeValidationSuspended, IEnumerable<string> columnNamesToExcludeFromCopy)
			: base(columnNamesToExcludeFromCopy)
		{
			ParentID = parentID;
			SuspendAddTagValidation = suspendAddTagValidation;
			SuspendRuleUsageScopeValidation = isRuleUsageScopeValidationSuspended;
		}

		public ZGuid ParentID { get; }
		public bool SuspendAddTagValidation { get; }
		public bool SuspendRuleUsageScopeValidation { get; }
	}
}
