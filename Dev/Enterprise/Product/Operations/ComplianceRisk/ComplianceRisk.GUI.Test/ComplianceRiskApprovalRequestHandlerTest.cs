using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskApprovalRequestHandlerTest : TestCaseWithFactory
	{
		public void TestIsRestrictedForMultiStep()
		{
			AssertIsRestrictedForMultiStep((s => s.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions), true);
			AssertIsRestrictedForMultiStep((s => s.ShipmentsComplianceAllowOverrideFreightMovementRestrictions), true);
			AssertIsRestrictedForMultiStep((s => s.BookingsComplianceAllowOverrideFreightMovementRestrictions), true);
			AssertIsRestrictedForMultiStep((s => s.CustomsComplianceAllowOverrideFreightMovementRestrictions), true);
			AssertIsRestrictedForMultiStep((s => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr), true);
			AssertIsRestrictedForMultiStep((s => s.OverrideRestrictionOfAviationSecurityFreightMovementRestricted), false);

			void AssertIsRestrictedForMultiStep(Func<SecurityCore, SecurityCheckpoint> func, bool expected)
			{
				var complianceRiskApprovalRequestHandler = new ComplianceRiskApprovalRequestHandler();
				var checkpointList = new List<Func<SecurityCore, SecurityCheckpoint>>
				{
					func
				};
				var loginBisObject = new SecurityLogin(checkpointList);
				complianceRiskApprovalRequestHandler.Initialize(loginBisObject, null);

				var args = new TestSecurityLoginEventArgs() { IsDPSFreightMovementRestricted = true };
				var isRestricted = complianceRiskApprovalRequestHandler.IsRestrictedForMultiStep(args);
				AssertEquals(expected, isRestricted);
			}
		}

		class TestSecurityLoginEventArgs : ISecurityLoginEventArgsForDocumentApproval
		{
			public ZBool IsExternalAccountingSystemUsed { get; set; }
			public BusinessObject ParentBusinessObject { get; set; }
			public ZGuid MenuItemPK { get; set; }
			public IReadOnlyList<int> AuthorizationLevel { get; set; }
			public ZString DefaultApprovalRequestReason { get; set; }
			public ZBool IsAccountingRestricted { get; set; }
			public ZBool IsDPSFreightMovementRestricted { get; set; }
			public ZBool IsAviationSecurityFreightMovementRestricted { get; set; }
			public bool IsAllowedToProceed { get; set; }
			public ZString AuthorisingStaffLogin { get; set; }
		}
	}
}
