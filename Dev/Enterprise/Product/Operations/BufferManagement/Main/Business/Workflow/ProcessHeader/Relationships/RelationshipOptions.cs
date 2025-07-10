using System;

namespace Enterprise.BufferManagement.Business
{
	[Flags]
	public enum RelationshipOptions
	{
		None = 0,
		CreateOnlyIfLinkDoesNotExist = 1 << 1,
		ReverseExistingRelationship = 1 << 2,
	}
}
