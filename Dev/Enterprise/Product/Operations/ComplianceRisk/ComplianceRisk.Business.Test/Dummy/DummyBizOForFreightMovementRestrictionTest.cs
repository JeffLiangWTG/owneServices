using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class DummyBizOForFreightMovementRestrictionTest : NonPersistentBusinessObject, ICreditControlledDocumentDelivery, ICompliancePartyRiskStatusProvider
	{
		public DummyBizOForFreightMovementRestrictionTest(BusinessObjectFactory factory, string overallRisk, string partyRisk, string locationRisk, string commodityRisk) : this(factory)
		{
			SetComplianceRiskStatusForTest(overallRisk, partyRisk, locationRisk, commodityRisk);
		}

		public DummyBizOForFreightMovementRestrictionTest(BusinessObjectFactory factory) : base(factory)
		{
			GetDocumentLogin?.Invoke(null, null);
		}

		#region IComplianceItemRiskStatusProvider

		public IEnumerable<IScreeningParty> Parties => Enumerable.Empty<ScreeningParty>();

		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();

		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();

		public ZGuid ParentID => PK;

		public ZString ParentTableCode => "JS";

		public ComplianceRiskSupport ComplianceRiskSupport => throw new NotImplementedException();

		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => new();

		#endregion

		#region ICreditControlledDocumentDelivery

		public bool IsDPSFreightMovementRestricted { get; set; }

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		public OrgHeader[] OrganisationsForCreditChecks => throw new NotImplementedException();

		public string DescriptionOfOrganisationBeingCheckedForCredit => throw new NotImplementedException();

		public CustomMessageBoxCallback DocumentLoginMessageBoxCallback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public Logs Logs => throw new NotImplementedException();

		public string[] JobNumber => throw new NotImplementedException();

		public ZGuid Identifier => new ZGuid();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => throw new NotImplementedException();

		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

		public ZBool IsEnabledComplianceWise => true;

		public event EventHandler<SecurityLoginEventArgs> GetDocumentLogin;

		public ScreeningParty[] GetScreeningParties()
		{
			throw new NotImplementedException();
		}

		public void RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			throw new NotImplementedException();
		}

		#endregion

		void SetComplianceRiskStatusForTest(string overallRisk, string partyRisk, string locationRisk, string commodityRisk)
		{
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = PK;
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_OverallRisk = overallRisk;
			complianceRiskStatus.COR_PartyRisk = partyRisk;
			complianceRiskStatus.COR_LocationRisk = locationRisk;
			complianceRiskStatus.COR_CommodityRisk = commodityRisk;

			Factory.Save();
		}

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
	}
}
