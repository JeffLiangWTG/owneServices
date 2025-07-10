using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CollectionAndModuleIDBuilderTest : TestCaseWithFactory
	{
		public void TestAllCollectionCodesDefinedInSchemaXML()
		{
			var list = new CollectionProviderTypeCodeDescriptionList();
			foreach (var providerTypeString in CollectionAndModuleIDBuilder.Lookup_Exposed)
			{
				Assert(string.Format(@"CollectionTypeCode:{0} should be defined in CollectionProviderTypeCodeDescriptionList.xml for documentation", providerTypeString), list.ContainsCode(providerTypeString));
			}
		}

		public void TestLists()
		{
			AssertListCorrect(typeof(CreditorCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Creditor);
			AssertListCorrect(typeof(AddressCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Address);
			AssertListCorrect(typeof(CountryCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Country);
			AssertListCorrect(typeof(DebtorCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Debtor);
			AssertListCorrect(typeof(ServiceLevelCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ServiceLevel);
			AssertListCorrect(typeof(StaffCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Staff);
			AssertListCorrect(typeof(StaffAndResourceCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.StaffAndResource);
			AssertListCorrect(typeof(MeetingResourceCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.MeetingResource);
			AssertListCorrect(typeof(RefUNLOCOCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Unloco);
			AssertListCorrect(typeof(RefVesselCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Vessel);
			AssertListCorrect(typeof(VehicleCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Vehicle);
			AssertListCorrect(typeof(InternationalZonesCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Zone);
			AssertListCorrect(typeof(GlbCompanyCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Company);
			AssertListCorrect(typeof(ImporterCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Importer);
			AssertListCorrect(typeof(ExporterCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Exporter);
			AssertListCorrect(typeof(SupplierCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Supplier);
			AssertListCorrect(typeof(BankAccountCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.BankAccount);
			AssertListCorrect(typeof(ChequeBookCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ChequeBook);
			AssertListCorrect(typeof(DebtorGroupCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.DebtorGroup);
			AssertListCorrect(typeof(OrgCreditorGroupCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.CreditorGroup);
			AssertListCorrect(typeof(RefCurrencyCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Currency);
			AssertListCorrect(typeof(GlbBranchCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Branch);
			AssertListCorrect(typeof(GlbDepartmentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Department);
			AssertListCorrect(typeof(AccGLHeaderCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.GlAccount);
			AssertListCorrect(typeof(AccGLAccountDescriptorCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.LocalAccount);
			AssertListCorrect(typeof(AccGLAccountDescriptorCHSCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.CHSLocalAccount);
			AssertListCorrect(typeof(OrgHeaderCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
			AssertListCorrect(typeof(OrgSupplierPartCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Supplierpart);
			AssertListCorrect(typeof(CarrierCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Carrier);
			AssertListCorrect(typeof(GlbGroupCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Groups);
			AssertListCorrect(typeof(WhsInventoryHeldCodeCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.HoldCode);
			AssertListCorrect(typeof(SalesTeamCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SalesTeam);
			AssertListCorrect(typeof(AccChargeCodeCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ChargeCode);
			AssertListCorrect(typeof(RefContainerCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode);
			AssertListCorrect(typeof(RefCommodityCodeCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.CommodityCode);
			AssertListCorrect(typeof(OrgPartCategoryCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ProductCategory);
			AssertListCorrect(typeof(WarehouseCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Warehouse);
			AssertListCorrect(typeof(TransitWarehouseCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TransitWarehouse);
			AssertListCorrect(typeof(FTZWarehouseCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.FTZWarehouse);
			AssertListCorrect(typeof(WarehouseClientCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.WarehouseClient);
			AssertListCorrect(typeof(WhsLocationTypeCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.LocationType);
			AssertListCorrect(typeof(ShippingProviderCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ShippingProvider);
			AssertListCorrect(typeof(ForwarderCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Forwarder);
			AssertListCorrect(typeof(SettlementGroupCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SettlementGroup);
			AssertListCorrect(typeof(SendingAgentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SendingAgent);
			AssertListCorrect(typeof(ReceivingAgentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ReceivingAgent);
			AssertListCorrect(typeof(TransactionBranchCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TransactionBranch);
			AssertListCorrect(typeof(TransactionDepartmentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TransactionDepartment);
			AssertListCorrect(typeof(OverseasAgentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.OverseasAgent);
			AssertListCorrect(typeof(TransportClientCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TransportClient);
			AssertListCorrect(typeof(TransportBookingCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TransportBooking);
			AssertListCorrect(typeof(ShipsAgencyPrincipalProvider), CollectionProviderTypeCodeDescriptionList.Codes.ShipsAgencyPrincipal);
			AssertListCorrect(typeof(CampaignCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Campaign);
			AssertListCorrect(typeof(VotingCampaignCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.VoteCampaign);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Accounting.Integration.IDepositBatchCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.DepositBatch);
			AssertListCorrect(typeof(LocationCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Location);
			AssertListCorrect(typeof(WhsAreaCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Whsarea);
			AssertListCorrect(typeof(LearningCentreCampaignCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.CertificateExam);
			AssertListCorrect(typeof(JobApplicantCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.JobApplicant);
			AssertListCorrect(typeof(GlbAccreditationCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Accreditation);
			AssertListCorrect(typeof(GlbPersonCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Person);
			AssertListCorrect(typeof(AccGroupsCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SalesExpenseGroups);
			AssertListCorrect(typeof(JobVoyageCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SeaVoyage);
			AssertListCorrect(typeof(JobTradeLaneCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TradeLane);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.USDomesticport);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCarrierCombinedCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.USCarrier);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCCountryCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.USCountry);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSLicenseTypeCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.USLicenseType);
			AssertListCorrect(typeof(AccTaxRateCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TaxId);
			AssertListCorrect(typeof(AccInvMsgCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.TaxMsg);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACarrierCombinedCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.CACarrier);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACSubLocationCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.CASublocation);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACCBSAOfficeCodesCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.CACbsaoffice);
			AssertListCorrect(typeof(ShipmentCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Shipment);
			AssertListCorrect(typeof(NettingPeriodCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.NettingPeriod);
			AssertListCorrect(typeof(DependenceCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Dependence);
			AssertListCorrect(typeof(CommissionAgreementCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.CommissionAgreement);
			AssertListCorrect(typeof(ContactCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Contact);
			AssertListCorrect(typeof(ViewLocationCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ViewLocation);
			AssertListCorrect(typeof(WarehouseProductCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.WarehouseProduct);
			AssertListCorrect(typeof(WhsPickingAreaCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.WhsPickingArea);
			AssertListCorrect(typeof(WhsPutawayAreaCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.WhsPutawayArea);
			AssertListCorrect(typeof(DailyStatementCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.DailyStatement);
			AssertListCorrect(typeof(MonthlyStatementCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.MonthlyStatement);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSModuleEntryHeaderCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.USEntryHeader);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAProductNumberCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.FDAProductNumber);
			AssertListCorrect(typeof(AccComplianceSequenceCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.SequenceBook);
			AssertListCorrect(typeof(CommunicationCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Communication);
			AssertListCorrect(typeof(InquiryManagerCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.InquiryManager);
			AssertListCorrect(typeof(OpportunityManagerCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.OpportunityManager);
			AssertListCorrect(typeof(QuotationsCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.Quotations);
			AssertListCorrect(typeof(QuotedBookingCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.QuotedBooking);
			AssertListCorrect(ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRCustomsOfficeCollectionProvider>(), CollectionProviderTypeCodeDescriptionList.Codes.KRCustomsOffice);
			AssertListCorrect(typeof(ReportingBookCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ReportingBook);
			AssertListCorrect(typeof(ReportingBookWithLocalCurrencyCollectionProvider), CollectionProviderTypeCodeDescriptionList.Codes.ReportingBookWithLocalCurrency);
		}

		public void TestLookupGetterIsThreadSafe()
		{
			var exceptionsOccured = new List<Exception>();
			var lookupGetters = new Thread[100];
			for (int i = 0; i < lookupGetters.Length; i++)
			{
				lookupGetters[i] = new Thread(() =>
				{
					try
					{
						var lookup = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.CACbsaoffice);
						CollectionAndModuleIDBuilder.ClearLookupForTest();
					}
					catch (Exception e)
					{
						exceptionsOccured.Add(e);
					}
				});
			}
			for (int i = 0; i < lookupGetters.Length; i++)
			{
				lookupGetters[i].Start();
			}
			for (int i = 0; i < lookupGetters.Length; i++)
			{
				lookupGetters[i].Join();
			}

			AssertEquals(0, exceptionsOccured.Count);
		}

		class ClientHookForTest : TestClientHook
		{
			public override Dictionary<string, Type> DocumentEngineCollectionProviders
			{
				get
				{
					Dictionary<string, Type> result = new Dictionary<string, Type>();
					result.Add("provider 1", typeof(CollectionProvider1));
					result.Add("provider 2", typeof(CollectionProvider2));
					return result;
				}
			}
		}

		class CollectionProvider1 : CollectionProvider
		{
			public CollectionProvider1(BusinessObjectFactory factory) : base(factory) { }
			protected override IBusinessObjectCollection CreateCollection()
			{
				throw new NotImplementedException();
			}

			public override ModuleIdentifier ModuleID
			{
				get { throw new NotImplementedException(); }
			}
		}

		class CollectionProvider2 : CollectionProvider
		{
			public CollectionProvider2(BusinessObjectFactory factory) : base(factory) { }
			protected override IBusinessObjectCollection CreateCollection()
			{
				throw new NotImplementedException();
			}

			public override ModuleIdentifier ModuleID
			{
				get { throw new NotImplementedException(); }
			}
		}

		public void TestClientList()
		{
			CollectionAndModuleIDBuilder.ClearLookupForTest();

			ClientHook testClientHook = new ClientHookForTest();
			using (ClientHookLoader.Instance.OverrideClientHookForTest(testClientHook))
			{
				AssertEquals(typeof(CollectionProvider1), CollectionAndModuleIDBuilder.GetCollectionAndModuleID(null, "provider 1").GetType());
				AssertEquals(typeof(CollectionProvider2), CollectionAndModuleIDBuilder.GetCollectionAndModuleID(null, "provider 2").GetType());
			}
		}

		void AssertListCorrect(Type collectionProviderType, string name)
		{
			AssertEquals("Provider type is incorrect", collectionProviderType, CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, name).GetType());
			AssertEquals("Can't get " + name + " collection Provider", name, CollectionAndModuleIDBuilder.GetCollectionProviderNameFromType(collectionProviderType));
		}
	}
}
