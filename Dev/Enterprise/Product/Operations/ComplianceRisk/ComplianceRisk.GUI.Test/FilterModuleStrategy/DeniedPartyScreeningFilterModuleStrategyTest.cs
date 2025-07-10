using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class DeniedPartyScreeningFilterModuleStrategyTest : TestCaseWithFactory
	{
		public void TestNewFilterAdded()
		{
			var crtEnabled = false;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(crtEnabled)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, crtEnabled))
			{
				AssertComplianceFilters(typeof(DummyWithScreeningPartyProvider), expectedFilter: false);
				AssertComplianceFilters(typeof(DummyWithViewQuotedBooking), expectedFilter: false);
				AssertComplianceFilters(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider), expectedFilter: false);
				AssertComplianceFilters(typeof(DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider), expectedFilter: false);
			}

			crtEnabled = true;
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(crtEnabled)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, crtEnabled))
			{
				AssertComplianceFilters(typeof(DummyWithScreeningPartyProvider), expectedFilter: false);
				AssertComplianceFilters(typeof(DummyWithViewQuotedBooking), expectedFilter: false);
				AssertComplianceFilters(typeof(DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider), expectedFilter: true);
				AssertComplianceFilters(typeof(DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider), expectedFilter: true);
			}

			void AssertComplianceFilters(Type bizoType, bool expectedFilter)
			{
				var filters = GetModuleFilterCollection(bizoType);
				var deniedPartyStatusFilter = crtEnabled && expectedFilter ? "Legacy Screening Status" : "Screening Status";

				Assert("Denied Party Status", !(filters[deniedPartyStatusFilter + FilterModuleStrategy.UniqueSuffix] is null));
				AssertEquals("Job Compliance Status", expectedFilter, !(filters["Overall" + FilterModuleStrategy.UniqueSuffix] is null));
				AssertEquals("Party Compliance Risk", expectedFilter, !(filters["Party" + FilterModuleStrategy.UniqueSuffix] is null));
				AssertEquals("Location Compliance Risk", expectedFilter, !(filters["Location" + FilterModuleStrategy.UniqueSuffix] is null));
				AssertEquals("Commodity Compliance Risk", expectedFilter, !(filters["Commodity" + FilterModuleStrategy.UniqueSuffix] is null));
			}

			ModuleFilterCollection GetModuleFilterCollection(Type bizoType)
			{
				var filters = new ModuleFilterCollection();
				var deniedStrategy = new DeniedPartyScreeningFilterModuleStrategy();
				deniedStrategy.RunOnModuleFiltersCreated(filters, bizoType, Factory);
				return filters;
			}
		}

		public void TestAddColumn()
		{
			var strategy = new DeniedPartyScreeningFilterModuleStrategy();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var filterControl = new DummyBizOFilterStripControlForTest(Factory))
			{
				strategy.RunOnFilterControlInitialisation(filterControl, filterControl.GridCollection);

				AssertEquals(0, filterControl.Grid.ColumnStyles.Count);
			}
		}

		public void TestAddColumnWhenEnableComplianceRiskRegistryIsEnabled()
		{
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var filterControl = new DummyBizOFilterStripControlForTest(Factory))
			{
				var strategy = new DeniedPartyScreeningFilterModuleStrategy();
				strategy.RunOnFilterControlInitialisation(filterControl, filterControl.GridCollection);

				AssertEquals(4, filterControl.Grid.ColumnStyles.Count);
				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "Overall"));
				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.Caption == "Job Compliance Status"));

				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "Party"));
				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.Caption == "Party Compliance Risk"));

				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "Location"));
				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.Caption == "Location Compliance Risk"));

				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == "Commodity"));
				AssertNotNull(filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.Caption == "Commodity Compliance Risk"));
			}
		}

		public void TestScreeningStatus_List()
		{
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var strategy = new DeniedPartyScreeningFilterModuleStrategy();
				var statusList = strategy.ScreeningStatus_List;

				AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList()
				{
					new CodeDescriptionPair(ScreeningStatusesList.Codes.Clear,ScreeningStatusesList.Descriptions.Clear),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.JobCleared,ScreeningStatusesList.Descriptions.JobCleared),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.Matched,ScreeningStatusesList.Descriptions.Matched),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.NotScreened,ScreeningStatusesList.Descriptions.NotScreened),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.PermanentClear,ScreeningStatusesList.Descriptions.PermanentClear),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.Unknown,ScreeningStatusesList.Descriptions.Unknown),
					new CodeDescriptionPair(ScreeningStatusesList.Codes.RequiresReview,ScreeningStatusesList.Descriptions.RequiresReview),
				}, statusList);
			}
		}

		public void TestScreeningStatusList_WhenBizOJobDecAndRegistryOn_ShouldReturnNewStatuses()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var fms = new DeniedPartyScreeningFilterModuleStrategyForTest(true);

				AssertContains(ScreeningStatusesList.Codes.JobClearedExternal, fms.ScreeningStatus_List.CodesAsString);
				AssertContains(ScreeningStatusesList.Codes.JobBlockedExternal, fms.ScreeningStatus_List.CodesAsString);
			}
		}

		public void TestScreeningStatusList_WhenBizOJobDecAndRegistryOff_ShouldNotReturnNewStatuses()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var fms = new DeniedPartyScreeningFilterModuleStrategyForTest(true);

				AssertNotContains(ScreeningStatusesList.Codes.JobClearedExternal, fms.ScreeningStatus_List.CodesAsString);
				AssertNotContains(ScreeningStatusesList.Codes.JobBlockedExternal, fms.ScreeningStatus_List.CodesAsString);
			}
		}

		public void TestScreeningStatusList_WhenBizONotJobDec_ShouldNotReturnNewStatuses()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableExternalOverrideJobScreeningStatuses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var fms = new DeniedPartyScreeningFilterModuleStrategyForTest(false);

				AssertNotContains(ScreeningStatusesList.Codes.JobClearedExternal, fms.ScreeningStatus_List.CodesAsString);
				AssertNotContains(ScreeningStatusesList.Codes.JobBlockedExternal, fms.ScreeningStatus_List.CodesAsString);
			}
		}

		#region Implementation

		public class DummyWithQuotedBooking : DummyBusinessObject, IQuotedBooking
		{
			public DummyWithQuotedBooking(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZGuid ClientPK { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ZGuid ViewPK => throw new NotImplementedException();

			public ZString UniqueConsignRef { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public ZString TransportMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ZString Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public IOrgHeader ControllingCustomer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ZGuid CartagePK => throw new NotImplementedException();

			public ZGuid SailingJX { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public IDependentBusinessObjectCollection QuotedBookingContainers => throw new NotImplementedException();

			public ZString PackingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public BusinessObject ForwardingShipment => throw new NotImplementedException();

			public BusinessObject JobSailing => throw new NotImplementedException();

			public BusinessObject Quote => throw new NotImplementedException();

			public IJobHeader Job => throw new NotImplementedException();

			public ZString Name => throw new NotImplementedException();

			public IOrgHeader ContractServiceProvider => throw new NotImplementedException();

			public IRatingContract CarrierContract => throw new NotImplementedException();

			public IRatingContractAllocationLine AllocationRoute => throw new NotImplementedException();

			public ZDateTime ETD => throw new NotImplementedException();

			public ZString LoadPort => throw new NotImplementedException();

			public ZString DischargePort => throw new NotImplementedException();

			public ZString VoyageFlight => throw new NotImplementedException();

			public ZString Vessel => throw new NotImplementedException();

			public IEnumerable<IForwardingContainer> Containers => throw new NotImplementedException();

			public IOrgHeader Client => throw new NotImplementedException();

			public IOrgHeader Consignee => throw new NotImplementedException();

			public IOrgHeader Consignor => throw new NotImplementedException();

			public ZString Via => throw new NotImplementedException();

			ZString IQuotedBooking.Origin => throw new NotImplementedException();

			ZString IQuotedBooking.Destination => throw new NotImplementedException();
		}

		public class DummyWithViewQuotedBooking : DummyBusinessObject, IViewQuotedBooking, IViewComplianceRiskStatusProvider
		{
			public DummyWithViewQuotedBooking(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			#region IDeniedPartyScreeningPartyProvider Members

			public ZGuid VB_JS { get; set; }

			public ZGuid VB_TH { get; set; }

			public ZGuid VB_GC { get; set; }

			public IJobHeader Job { get; }

			IComplianceItemRiskStatusProvider IViewComplianceRiskStatusProvider.GetProviderBusinessObject() => throw new NotImplementedException();

			#endregion
		}

		public class DummyWithScreeningPartyProvider : DummyBusinessObject, IScreeningPartyProvider
		{
			public DummyWithScreeningPartyProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDeniedPartyScreeningPartyProvider Members

			public ScreeningParty[] ScreeningParties
			{
				get { return Array.Empty<ScreeningParty>(); }
			}

			public ZString ScreeningStatus { get; set; }

			#endregion

			public ZString GetWorstScreeningStatus()
			{
				throw new NotImplementedException();
			}

			public ZString GetWorstScreeningStatusUnlessManuallyCleared()
			{
				throw new NotImplementedException();
			}
		}

		public class DummyWithBothQuotedBookingAndComplianceRiskStatusProvider : DummyWithQuotedBooking, IComplianceItemRiskStatusProvider
		{
			public DummyWithBothQuotedBookingAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime EffectiveDate => throw new NotImplementedException();

			public IEnumerable<ScreeningParty> Parties => throw new NotImplementedException();

			public IEnumerable<ScreeningParty> Locations => throw new NotImplementedException();

			public IEnumerable<ComplianceCommodity> Commodities => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZGuid ParentID => throw new NotImplementedException();

			public ZString ParentTableCode => throw new NotImplementedException();

			public bool SupportInitializingComplianceAssessmentStatusWorkflow => throw new NotImplementedException();

			public ComplianceRiskSupport ComplianceRiskSupport => throw new NotImplementedException();

			public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public Func<DocumentDeliveryResultForComplianceWorkflow> CheckComplianceAssessmentRequirements { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
		}

		[ViewComplianceRiskStatusProvider(ProviderBusinessObjectType = typeof(DummyWithBothQuotedBookingAndComplianceRiskStatusProvider))]
		public class DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider : DummyWithViewQuotedBooking
		{
			public DummyWithBothViewQuotedBookingAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider : DummyWithScreeningPartyProvider, IComplianceItemRiskStatusProvider
		{
			public DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IEnumerable<ScreeningParty> Parties => throw new NotImplementedException();

			public IEnumerable<ScreeningParty> Locations => throw new NotImplementedException();

			public IEnumerable<ComplianceCommodity> Commodities => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> SubComplianceRiskStatusProviders => throw new NotImplementedException();

			public IEnumerable<IComplianceItemRiskStatusProvider> ParentComplianceRiskStatusProviders => throw new NotImplementedException();

			public ZDateTime EffectiveDate => throw new NotImplementedException();

			public ZGuid ParentID => throw new NotImplementedException();

			public ZString ParentTableCode => throw new NotImplementedException();

			public ComplianceRiskSupport ComplianceRiskSupport => throw new NotImplementedException();

			public Func<DocumentDeliveryResultForComplianceWorkflow> InitializeComplianceWorkflowPopupIfNeeded { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public Func<DocumentDeliveryResultForComplianceWorkflow> CheckComplianceAssessmentRequirements { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public ComplianceAssessmentPointPairInfo AssessmentPointPairInfo => throw new NotImplementedException();

			public (ZBool IsCurrent, ZDateTime JobEndDate) JobTime => (true, ZDateTime.BrettsBirthday);

			public ZBool IsEnabledComplianceWise => true;

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => throw new NotImplementedException();

			SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => throw new NotImplementedException();
		}

		class DummyBizOFilterStripControlForTest : ZFilterStripControl, IFilterControl
		{
			public DummyBizOFilterStripControlForTest(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			public override IBusinessObjectCollection GridCollection => new DummyWithBothScreeningPartyProviderAndComplianceRiskProviderCollection(factory);
		}

		class DummyWithBothScreeningPartyProviderAndComplianceRiskProviderCollection : BusinessObjectCollection<DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider>
		{
			public DummyWithBothScreeningPartyProviderAndComplianceRiskProviderCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		class DeniedPartyScreeningFilterModuleStrategyForTest : DeniedPartyScreeningFilterModuleStrategy
		{
			public DeniedPartyScreeningFilterModuleStrategyForTest(bool useJobDecBizO)
			{
				BizoType = useJobDecBizO ? typeof(IBaseJobDeclaration) : typeof(OrgHeader);
			}
		}

		#endregion
	}
}
