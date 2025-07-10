using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface ISecurityLoginEventArgsForDocumentApproval : ISecurityLoginEventArgs
	{
		ZBool IsExternalAccountingSystemUsed { get; }

		BusinessObject ParentBusinessObject { get; }

		ZGuid MenuItemPK { get; }

		IReadOnlyList<int> AuthorizationLevel { get; }

		ZString DefaultApprovalRequestReason { get; }

		ZBool IsAccountingRestricted { get; }

		ZBool IsDPSFreightMovementRestricted { get; }

		ZBool IsAviationSecurityFreightMovementRestricted { get; }
	}
}
