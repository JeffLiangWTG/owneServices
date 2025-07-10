using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[CodeProperty(nameof(DummyUniqueRef))]
	public class DummyBizObjThatImplementIComplianceItemRiskStatusProvider : NonPersistentBusinessObject, IComplianceItemRiskStatusProvider
	{
		public DummyBizObjThatImplementIComplianceItemRiskStatusProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		ZGuid parentID;
		public ZGuid ParentID
		{
			get
			{
				if (parentID == ZGuid.Empty)
				{
					parentID = PK;
				}

				return parentID;
			}
			set
			{
				parentID = value;
			}
		}

		public ZString ParentTableCode => "JS";

		public ComplianceRiskSupport ComplianceRiskSupport { get; } = ComplianceRiskSupport.FullySupported;

		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public ZString DummyUniqueRef { get; set; } = nameof(DummyUniqueRef);

		public ComplianceRiskPlugInBusinessObject ComplianceRiskPlugInBusinessObjectForTest => fComplianceRiskPlugInBusinessObjectForTest ?? (fComplianceRiskPlugInBusinessObjectForTest = new ComplianceRiskPlugInBusinessObject(this));

		ComplianceRiskPlugInBusinessObject fComplianceRiskPlugInBusinessObjectForTest;

		public bool? MockIsInDatabase { get; set; }

		public override bool IsInDatabase => MockIsInDatabase ?? base.IsInDatabase;

		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

		public ZBool IsEnabledComplianceWise => true;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
	}

	public class DummyBizObjThatImplementPartyAndLocationAndCommodityProvider : DummyBizObjThatImplementIComplianceItemRiskStatusProvider, ICompliancePartyRiskStatusProvider, IComplianceLocationRiskStatusProvider, IComplianceCommodityRiskStatusProvider
	{
		public DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(BusinessObjectFactory factory)
			: base(factory)
		{
			SetComplianceRiskStatusForTest();
		}

		public IList<ScreeningParty> PartyList { get; set; } = new List<ScreeningParty>();

		public IEnumerable<IScreeningParty> Parties => PartyList;

		public ComplianceRiskStatus ComplianceRiskStatus { get; private set; }

		readonly IList<ScreeningParty> countryList = new List<ScreeningParty>();

		public IEnumerable<IComplianceLocation> Locations => countryList;

		public IEnumerable<IComplianceCommodity> Commodities => commodityList;

		public ZDateTime EffectiveDate => new (2022, 10, 20);

		readonly IList<ComplianceCommodity> commodityList = new List<ComplianceCommodity>();

		public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => new();

		public void AddCommoditiesForTest(params ComplianceCommodity[] complianceCommodities)
		{
			foreach (var complianceCommodity in complianceCommodities)
			{
				commodityList.Add(complianceCommodity);
			}
		}

		void SetComplianceRiskStatusForTest()
		{
			ComplianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			ComplianceRiskStatus.COR_ParentID = PK;
			ComplianceRiskStatus.COR_ParentTableCode = "JS";
			ComplianceRiskStatus.COR_OverallRisk = "CLR";
			ComplianceRiskStatus.COR_PartyRisk = "CLR";
			ComplianceRiskStatus.COR_LocationRisk = "CLR";
		}

		public void AddPartiesForTest(params ScreeningParty[] screeningParties)
		{
			foreach (var screeningParty in screeningParties)
			{
				PartyList.Add(screeningParty);
			}
		}

		public void AddCountriesForTest(params ScreeningParty[] screeningParties)
		{
			foreach (var screeningParty in screeningParties)
			{
				countryList.Add(screeningParty);
			}
		}

		public ZBool IsEditingCommoditySupported => ZBool.True;

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
	}

	public class DummyBizObjThatImplementIComplianceCommodityRiskStatusProviderWithInvalidValue : NonPersistentBusinessObject, IComplianceCommodityRiskStatusProvider
	{
		public DummyBizObjThatImplementIComplianceCommodityRiskStatusProviderWithInvalidValue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZGuid ParentID => Guid.Empty;

		public ZString ParentTableCode => ZString.Empty;

		public ComplianceRiskSupport ComplianceRiskSupport => ComplianceRiskSupport.None;

		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public IEnumerable<IComplianceCommodity> Commodities => null;

		public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => null;

		public ZDateTime EffectiveDate => ZDateTime.Invalid;

		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

		public ZBool IsEnabledComplianceWise => true;

		public ZBool IsEditingCommoditySupported => ZBool.True;

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor => CommodityRiskCalculateFactor.All;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => throw new NotImplementedException();
	}

	public class DummyBizObjThatImplementICompliancePartyRiskStatusProviderWithInvalidValue : NonPersistentBusinessObject, ICompliancePartyRiskStatusProvider
	{
		public DummyBizObjThatImplementICompliancePartyRiskStatusProviderWithInvalidValue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZGuid ParentID => Guid.Empty;

		public ZString ParentTableCode => ZString.Empty;

		public ComplianceRiskSupport ComplianceRiskSupport => ComplianceRiskSupport.None;

		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

		public ZBool IsEnabledComplianceWise => true;

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties => null;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
	}

	public class DummyBizObjThatImplementIComplianceLocationRiskStatusProviderWithInvalidValue : NonPersistentBusinessObject, IComplianceLocationRiskStatusProvider
	{
		public DummyBizObjThatImplementIComplianceLocationRiskStatusProviderWithInvalidValue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZGuid ParentID => Guid.Empty;

		public ZString ParentTableCode => ZString.Empty;

		public ComplianceRiskSupport ComplianceRiskSupport => ComplianceRiskSupport.None;

		public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

		public ZBool IsEnabledComplianceWise => true;

		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations => null;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
	}
}
