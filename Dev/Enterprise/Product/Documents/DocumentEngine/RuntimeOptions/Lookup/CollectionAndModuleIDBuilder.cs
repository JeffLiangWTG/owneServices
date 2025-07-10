using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

#if DEBUG

using System.Linq;

#endif

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public static class CollectionAndModuleIDBuilder
	{
		public static CollectionProvider GetCollectionAndModuleID(BusinessObjectFactory factory, string lookupTypeName)
		{
			CollectionProvider result = null;
			TypeGetter providerTypeGetter;
			lock (lookupSynchroniser)
			{
				if (Lookup.TryGetValue(lookupTypeName, out providerTypeGetter))
				{
					Type providerType = providerTypeGetter();
					MethodInfo newMethod = providerType.GetMethod("New", BindingFlags.Static | BindingFlags.Public);
					if (newMethod != null)
					{
						result = (CollectionProvider)newMethod.Invoke(null, new object[] { factory });
					}
					else
					{
						result = (CollectionProvider)Activator.CreateInstance(providerType, factory);
					}
				}
			}

			return result;
		}

		delegate Type TypeGetter();

		static Dictionary<string, TypeGetter> Lookup
		{
			get
			{
				lock (lookupSynchroniser)
				{
					if (lookup.Value == null)
					{
						var l = new Dictionary<string, TypeGetter>();
						lookup.Value = l;
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Project, delegate { return typeof(ProjectCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.WorkItem, delegate { return typeof(WorkItemCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Creditor, delegate { return typeof(CreditorCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Address, delegate { return typeof(AddressCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Country, delegate { return typeof(CountryCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Debtor, delegate { return typeof(DebtorCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ServiceLevel, delegate { return typeof(ServiceLevelCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Staff, delegate { return typeof(StaffCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.StaffAndResource, delegate { return typeof(StaffAndResourceCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.MeetingResource, delegate { return typeof(MeetingResourceCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Unloco, delegate { return typeof(RefUNLOCOCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Vessel, delegate { return typeof(RefVesselCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Vehicle, delegate { return typeof(VehicleCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Zone, delegate { return typeof(InternationalZonesCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Company, delegate { return typeof(GlbCompanyCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Importer, delegate { return typeof(ImporterCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Supplier, delegate { return typeof(SupplierCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Exporter, delegate { return typeof(ExporterCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.BankAccount, delegate { return typeof(BankAccountCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ChequeBook, delegate { return typeof(ChequeBookCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.FTZWarehouse, delegate { return typeof(FTZWarehouseCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.DebtorGroup, delegate { return typeof(DebtorGroupCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CreditorGroup, delegate { return typeof(OrgCreditorGroupCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Currency, delegate { return typeof(RefCurrencyCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Branch, delegate { return typeof(GlbBranchCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.LocalBranch, delegate { return typeof(LocalGlbBranchCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Department, delegate { return typeof(GlbDepartmentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.GlAccount, delegate { return typeof(AccGLHeaderCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ReportingBook, delegate { return typeof(ReportingBookCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ReportingBookWithLocalCurrency, delegate { return typeof(ReportingBookWithLocalCurrencyCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.LocalAccount, delegate { return typeof(AccGLAccountDescriptorCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CHSLocalAccount, delegate { return typeof(AccGLAccountDescriptorCHSCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Organisation, delegate { return typeof(OrgHeaderCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Supplierpart, delegate { return typeof(OrgSupplierPartCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Carrier, delegate { return typeof(CarrierCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Groups, delegate { return typeof(GlbGroupCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.HoldCode, delegate { return typeof(WhsInventoryHeldCodeCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SalesTeam, delegate { return typeof(SalesTeamCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ChargeCode, delegate { return typeof(AccChargeCodeCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ContainerCode, delegate { return typeof(RefContainerCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CommodityCode, delegate { return typeof(RefCommodityCodeCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ProductCategory, delegate { return typeof(OrgPartCategoryCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Warehouse, delegate { return typeof(WarehouseCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TransitWarehouse, delegate { return typeof(TransitWarehouseCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.WarehouseClient, delegate { return typeof(WarehouseClientCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.LocationType, delegate { return typeof(WhsLocationTypeCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TransportBooking, delegate { return typeof(TransportBookingCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ShippingProvider, delegate { return typeof(ShippingProviderCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Forwarder, delegate { return typeof(ForwarderCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SettlementGroup, delegate { return typeof(SettlementGroupCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SendingAgent, delegate { return typeof(SendingAgentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ReceivingAgent, delegate { return typeof(ReceivingAgentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TransactionBranch, delegate { return typeof(TransactionBranchCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TransactionDepartment, delegate { return typeof(TransactionDepartmentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.OverseasAgent, delegate { return typeof(OverseasAgentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TransportClient, delegate { return typeof(TransportClientCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ShipsAgencyPrincipal, delegate { return typeof(ShipsAgencyPrincipalProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Campaign, delegate { return typeof(CampaignCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.VoteCampaign, delegate { return typeof(VotingCampaignCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.DepositBatch, ObjectFactory.GetType<Enterprise.Accounting.Integration.IDepositBatchCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.AlternateGLAccount, ObjectFactory.GetType<Enterprise.Accounting.Integration.IAccAlternateGLAccountCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Location, delegate { return typeof(LocationCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Whsarea, delegate { return typeof(WhsAreaCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.WhsPickingArea, delegate { return typeof(WhsPickingAreaCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.WhsPutawayArea, delegate { return typeof(WhsPutawayAreaCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CertificateExam, delegate { return typeof(LearningCentreCampaignCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.JobApplicant, delegate { return typeof(JobApplicantCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Accreditation, delegate { return typeof(GlbAccreditationCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Person, delegate { return typeof(GlbPersonCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SalesExpenseGroups, delegate { return typeof(AccGroupsCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SeaVoyage, delegate { return typeof(JobVoyageCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TradeLane, delegate { return typeof(JobTradeLaneCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Shipment, delegate { return typeof(ShipmentCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.NettingPeriod, delegate { return typeof(NettingPeriodCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USDomesticport, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USForeignport, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSForeignPortCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USCarrier, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCarrierCombinedCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USCountry, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCCountryCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USLicenseType, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSLicenseTypeCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TaxId, delegate { return typeof(AccTaxRateCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TaxMsg, delegate { return typeof(AccInvMsgCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Declaration, ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclarationCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CACarrier, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACarrierCombinedCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CASublocation, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACSubLocationCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CACbsaoffice, ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICACCBSAOfficeCodesCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Dependence, delegate { return typeof(DependenceCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.CommissionAgreement, delegate { return typeof(CommissionAgreementCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Contact, delegate { return typeof(ContactCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.ViewLocation, delegate { return typeof(ViewLocationCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.WarehouseProduct, delegate { return typeof(WarehouseProductCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.GlobalCreditGroup, delegate { return typeof(GlobalCreditGroupCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.DailyStatement, delegate { return typeof(DailyStatementCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.MonthlyStatement, delegate { return typeof(MonthlyStatementCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.USEntryHeader, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSModuleEntryHeaderCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.FDAProductNumber, ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSFDAProductNumberCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.SequenceBook, delegate { return typeof(AccComplianceSequenceCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Communication, delegate { return typeof(CommunicationCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.InquiryManager, delegate { return typeof(InquiryManagerCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.OpportunityManager, delegate { return typeof(OpportunityManagerCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.Quotations, delegate { return typeof(QuotationsCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.QuotedBooking, delegate { return typeof(QuotedBookingCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.OneOffQuotes, delegate { return typeof(OneOffQuotesCollectionProvider); });
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.NCTSDepartureOffices, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.INCTSDepartureOfficesCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.NCTSDestinationOffices, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.NCTS.INCTSDestinationOfficesCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.EUCusTempStorageRegPremises, ObjectFactory.GetType<Enterprise.Integration.Customs.EU.ICusTempStorageRegPremisesProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.TWGoodsLocation, ObjectFactory.GetType<Enterprise.Integration.Customs.TW.ITWGoodsLocationCollectionProvider>);
						l.Add(CollectionProviderTypeCodeDescriptionList.Codes.KRCustomsOffice, ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IKRCustomsOfficeCollectionProvider>);

						if (ClientHookLoader.Instance.ClientHook != null)
						{
							Dictionary<string, Type> providers = ClientHookLoader.Instance.ClientHook.DocumentEngineCollectionProviders;
							if (providers != null)
							{
								foreach (KeyValuePair<string, Type> pair in providers)
								{
									// We need a new variable for the delegate for each loop iteration
									// or else the delegates will all share the same variable
									// and all have the same value - the value after the loop has finished,
									// i.e., all keys will return the last type.
									Type type = pair.Value;
									l.Add(pair.Key, delegate { return type; });
								}
							}
						}
					}
				}
				return lookup.Value;
			}
		}

		static readonly object lookupSynchroniser = new object();

		static readonly Overridable<Dictionary<string, TypeGetter>> lookup = new Overridable<Dictionary<string, TypeGetter>>();

#if DEBUG
		public static void ClearLookupForTest()
		{
			lookup.ResetValue();
		}

		public static void SetLookupForTest(string lookupTypeName, Type collectionProviderType)
		{
			lookup.Value = new Dictionary<string, TypeGetter>();
			lookup.Value.Add(lookupTypeName, delegate { return collectionProviderType; });
		}

		public static List<string> Lookup_Exposed
		{
			get { return Lookup.Keys.ToList(); }
		}
#endif

		public static string GetCollectionProviderNameFromType(Type providerType)
		{
			lock (lookupSynchroniser)
			{
				foreach (KeyValuePair<string, TypeGetter> kvp in Lookup)
				{
					if (kvp.Value() == providerType)
					{
						return kvp.Key;
					}
				}
			}
			return "";
		}
	}
}
