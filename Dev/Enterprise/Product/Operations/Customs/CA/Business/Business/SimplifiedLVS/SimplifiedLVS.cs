using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class SimplifiedLVS : AutoSimplifiedLVS, IBuyerSupplierRelationshipConsumer, ISupportDataImporting
	{
		public SimplifiedLVS(BusinessObjectFactory factory)
			: base(factory)
		{
			BuyerSupplierLinksHelper = new BuyerSupplierLinksHelper<SimplifiedLVS>(this);
			BuyerSupplierLinksHelper.Register();
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoSimplifiedLVS.Schema
		{
			public const string JE_OH_BillTo = "JE_OH_BillTo";
		}

		#endregion

		#region Invoices

		[ChildEditable]
		public JobComInvoiceHeaderCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = new JobComInvoiceHeaderCollection(Factory);
					fInvoices.CountChanged += fInvoices_CountChanged;
					RegisterEditableChildObject(fInvoices);
				}
				return fInvoices;
			}
		}
		JobComInvoiceHeaderCollection fInvoices;

		void fInvoices_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var invoice = (JobComInvoiceHeader)e.BizObject;
			if (invoice != null && e.ItemRemoved)
			{
				if (invoice.IsSimplifiedLVSMode)
				{
					invoice.JobDeclaration.IsSimplifiedLVSMode = false;
				}

				invoice.JZ_InvoiceNumberInfo.ValueChanged -= JZ_InvoiceNumberInfo_ValueChanged;
				invoice.JZ_ValuationDateOverrideInfo.ValueChanged -= JZ_ValuationDateOverrideInfo_ValueChanged;
				invoice.CA_OtherReferenceInfo.ValueChanged -= CA_OtherReferenceInfo_ValueChanged;
				invoice.CA_TimeLimitInfo.ValueChanged -= CA_TimeLimitInfo_ValueChanged;
				invoice.CA_TimeLimitCodeInfo.ValueChanged -= CA_TimeLimitCodeInfo_ValueChanged;
				UnRegisterEditableChildObject(invoice.InvoiceLines);

				invoice.SupplierDocumentaryAddress.DocAddressChanged -= SupplierDocumentaryAddressChanged;
				SupplierDocumentaryAddress.OrganisationPK = originalSupplierDocumentaryAddressOrganisationPK;
				SupplierDocumentaryAddress.E2_OA_Address = originalSupplierDocumentaryAddressE2_OA_Address;
			}
			SupplierDocumentaryAddress.RefreshBinding();
		}

		public JobDeclaration Declaration
		{
			get { return Invoices.Count > 0 ? Invoices[0].JobDeclaration : null; }
		}

		#endregion

		#region Proxy properties

		#region JE_OH_Importer

		[List(nameof(Importers))]
		[RelatedBusinessObject("Importer")]
		public override ZGuid JE_OH_Importer
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_OH_Buyer : base.JE_OH_Importer; }
			set
			{
				var oldValue = base.JE_OH_Importer;
				if (oldValue != value)
				{
					base.JE_OH_Importer = value;
					var importer = value.IsValid ? Importer : null;
					fBillTo = importer == null ? null : importer.DeliveryCustomsBillTo;
					JE_OH_BillToInfo.RefreshBinding();
				}
			}
		}

		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(this.JE_OH_Importer); }
		}

		public override ZPropertyInfo JE_OH_ImporterInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JE_OH_Importer, x => Invoices[0].JZ_OH_BuyerInfo) : base.JE_OH_ImporterInfo; }
		}

		public ConsigneeCollection Importers
		{
			get { return new ConsigneeCollection(Factory); }
		}

		#endregion

		#region JE_GB

		[RelatedBusinessObject("Branch")]
		[List(nameof(Branches))]
		public override ZGuid JE_GB
		{
			get { return Declaration != null ? Declaration.JE_GB : base.JE_GB; }
			set
			{
				var oldValue = base.JE_GB;
				base.JE_GB = value;
				if (oldValue != JE_GB)
				{
					DefaultPortOfClearance();
				}
			}
		}

		public override ZPropertyInfo JE_GBInfo
		{
			get { return Declaration != null ? GetWrappedZPropertyInfo(Schema.JE_GB, x => Declaration.JE_GBInfo) : base.JE_GBInfo; }
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(JE_GB); }
		}

		public GlbBranchDependentCollection Branches
		{
			get { return new GlbBranchDependentCollection(GlbCompany.CurrentCompany, Factory); }
		}

		#endregion

		#region CA_PortOfClearance

		[List(nameof(Offices))]
		[RelatedBusinessObject(nameof(PortOfClearance))]
		public override ZString CA_PortOfClearance
		{
			get { return Declaration != null ? Declaration.JE_CustomsOffice : base.CA_PortOfClearance; }
			set { base.CA_PortOfClearance = value.IsEmpty ? value : value.PadLeft(4, '0'); }
		}

		public override ZPropertyInfo CA_PortOfClearanceInfo
		{
			get { return Declaration != null ? GetWrappedZPropertyInfo(Schema.CA_PortOfClearance, x => Declaration.JE_CustomsOfficeInfo) : base.CA_PortOfClearanceInfo; }
		}

		public ZZRefCusCodeListCombinedCollection Offices
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombined PortOfClearance
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CA_PortOfClearance, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		#endregion

		#region JE_PeriodMonth

		public override ZInt JE_PeriodMonth
		{
			get { return JE_EntryAuthorisationDate.IsValid ? (ZInt)JE_EntryAuthorisationDate.Month : ZInt.Zero; }
			set
			{
				var year = JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.Year : ZDateTime.Today.Year;
				var day = JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.Day : 1;
				JE_EntryAuthorisationDate = new ZDateTime(year, Math.Max(Math.Min(value, 12), 1), day);
			}
		}

		public override ZPropertyInfo JE_PeriodMonthInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_PeriodMonth, x => JE_EntryAuthorisationDateInfo); }
		}

		#endregion

		#region JE_PeriodYear

		public override ZInt JE_PeriodYear
		{
			get { return JE_EntryAuthorisationDate.IsValid ? (ZInt)JE_EntryAuthorisationDate.Year : ZInt.Zero; }
			set
			{
				var month = JE_EntryAuthorisationDate.IsValid ? JE_EntryAuthorisationDate.Month : ZDateTime.Today.Month;
				var year = Math.Max(Math.Min(value, ZDateTime.MaxSmallDateTimeValue.Year - 1), ZDateTime.MinSmallDateTimeValue.Year);
				JE_EntryAuthorisationDate = new ZDateTime(year, month, 1);
			}
		}

		public override ZPropertyInfo JE_PeriodYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_PeriodYear, x => JE_EntryAuthorisationDateInfo); }
		}

		#endregion

		#region JE_EntryAuthorisationDate

		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return Declaration != null ? Declaration.JE_EntryAuthorisationDate : base.JE_EntryAuthorisationDate; }
			set { base.JE_EntryAuthorisationDate = value; }
		}

		public override ZPropertyInfo JE_EntryAuthorisationDateInfo
		{
			get { return Declaration != null ? GetWrappedZPropertyInfo(Schema.JE_EntryAuthorisationDate, x => Declaration.JE_EntryAuthorisationDateInfo) : base.JE_EntryAuthorisationDateInfo; }
		}

		#endregion

		#region JE_GS_NKCusAgent

		[List(nameof(CusAgents))]
		public override ZString JE_GS_NKCusAgent
		{
			get { return Declaration != null ? Declaration.JE_GS_NKCusAgent : base.JE_GS_NKCusAgent; }
			set { base.JE_GS_NKCusAgent = value; }
		}

		public override ZPropertyInfo JE_GS_NKCusAgentInfo
		{
			get { return Declaration != null ? GetWrappedZPropertyInfo(Schema.JE_GS_NKCusAgent, x => Declaration.JE_GS_NKCusAgentInfo) : base.JE_GS_NKCusAgentInfo; }
		}

		public GlbStaffCollection CusAgents
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region JZ_Weight

		[MeasureUnit(Schema.JZ_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JZ_Weight
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_Weight : base.JZ_Weight; }
			set { base.JZ_Weight = value; }
		}

		public override ZPropertyInfo JZ_WeightInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JZ_Weight, x => this.Invoices[0].JZ_WeightInfo) : base.JZ_WeightInfo; }
		}

		#endregion

		#region JZ_WeightUQ

		[List(nameof(WeightUQList))]
		public override ZString JZ_WeightUQ
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_WeightUQ : base.JZ_WeightUQ; }
			set { base.JZ_WeightUQ = value; }
		}

		public CodeDescriptionPairList WeightUQList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public override ZPropertyInfo JZ_WeightUQInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JZ_WeightUQ, x => this.Invoices[0].JZ_WeightUQInfo) : base.JZ_WeightUQInfo; }
		}

		#endregion

		#region CA_LVSCarrier

		[RelatedBusinessObject("Carrier")]
		[List(nameof(CarrierCodes))]
		public override ZString CA_LVSCarrier
		{
			get { return Invoices.Count > 0 ? Invoices[0].CA_LVSCarrier : base.CA_LVSCarrier; }
			set
			{
				var oldValue = base.CA_LVSCarrier;
				base.CA_LVSCarrier = value;
				if (oldValue != CA_LVSCarrier)
				{
					DefaultPortOfClearance();
				}
			}
		}

		public ZZRefCarrierCombinedCollection CarrierCodes => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, ZString.Empty);

		public override ZPropertyInfo CA_LVSCarrierInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.CA_LVSCarrier, x => Invoices[0].CA_LVSCarrierInfo) : base.CA_LVSCarrierInfo; }
		}

		public ZZRefCarrierCombined Carrier
		{
			get { return new ZZRefCarrierCombined.Loader(Factory).LoadFromCode(Core.Constants.CountryCodes.Canada, CA_LVSCarrier); }
		}

		#endregion

		#region JZ_ValuationDateOverride

		public override ZDateTime JZ_ValuationDateOverride
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_ValuationDateOverride : base.JZ_ValuationDateOverride; }
			set
			{
				base.JZ_ValuationDateOverride = value;
				if (JZ_ValuationDateOverride.IsValid)
				{
					JE_EntryAuthorisationDate = new ZDateTime(JZ_ValuationDateOverride.Year, JZ_ValuationDateOverride.Month, 1);
					JE_EntryAuthorisationDateInfo.RefreshBinding();
				}
			}
		}

		public override ZPropertyInfo JZ_ValuationDateOverrideInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JZ_ValuationDateOverride, x => Invoices[0].JZ_ValuationDateOverrideInfo) : base.JZ_ValuationDateOverrideInfo; }
		}

		#endregion

		#region JZ_InvoiceNumber

		public override ZString JZ_InvoiceNumber
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_InvoiceNumber : base.JZ_InvoiceNumber; }
			set { base.JZ_InvoiceNumber = value; }
		}

		public override ZPropertyInfo JZ_InvoiceNumberInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JZ_InvoiceNumber, x => Invoices[0].JZ_InvoiceNumberInfo) : base.JZ_InvoiceNumberInfo; }
		}

		#endregion

		#region JZ_InvoiceDate

		public override ZDateTime JZ_InvoiceDate
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_InvoiceDate : base.JZ_InvoiceDate; }
			set { base.JZ_InvoiceDate = value; }
		}

		public override ZPropertyInfo JZ_InvoiceDateInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.JZ_InvoiceDate, x => Invoices[0].JZ_InvoiceDateInfo) : base.JZ_InvoiceDateInfo; }
		}

		#endregion

		#region CA_OtherReference

		public override ZString CA_OtherReference
		{
			get { return Invoices.Count > 0 ? Invoices[0].CA_OtherReference : base.CA_OtherReference; }
			set { base.CA_OtherReference = value; }
		}

		public override ZPropertyInfo CA_OtherReferenceInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.CA_OtherReference, x => Invoices[0].CA_OtherReferenceInfo) : base.CA_OtherReferenceInfo; }
		}

		#endregion

		#region CA_TimeLimit

		public override ZInt CA_TimeLimit
		{
			get { return Invoices.Count > 0 ? Invoices[0].CA_TimeLimit : base.CA_TimeLimit; }
			set { base.CA_TimeLimit = value; }
		}

		public override ZPropertyInfo CA_TimeLimitInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.CA_TimeLimit, x => Invoices[0].CA_TimeLimitInfo) : base.CA_TimeLimitInfo; }
		}

		#endregion

		#region CA_TimeLimitCode

		[List(nameof(TimeLimitUnits))]
		[ResourceStringData("SimplifiedLVS|CA_TimeLimitCode", Caption = "Time Limit Unit", ShortCaption = "TU", MediumCaption = "Time Unit")]
		public override ZString CA_TimeLimitCode
		{
			get { return Invoices.Count > 0 ? Invoices[0].CA_TimeLimitCode : base.CA_TimeLimitCode; }
			set { base.CA_TimeLimitCode = value; }
		}

		public override ZPropertyInfo CA_TimeLimitCodeInfo
		{
			get { return Invoices.Count > 0 ? GetWrappedZPropertyInfo(Schema.CA_TimeLimitCode, x => Invoices[0].CA_TimeLimitCodeInfo) : base.CA_TimeLimitCodeInfo; }
		}

		public TimeLimitUnitCodes TimeLimitUnits
		{
			get { return Factory.GetCachedValue("LVSTimeLimitUnits", () => new TimeLimitUnitCodes()); }
		}

		#endregion

		#endregion

		#region New Properties

		public string JE_MessageSubType
		{
			get
			{
				var result = ZString.Empty;
				var importerAddInfo = base.JE_OH_Importer.IsEmpty ? null : OrgImpAddInfo.Get(Importer);
				if (importerAddInfo != null && importerAddInfo.ZO_IsLVSConsolidated)
				{
					result = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
				}
				else
				{
					var branch = base.JE_GB.IsEmpty ? null : Factory.Load<GlbBranch>(base.JE_GB);
					if (branch != null)
					{
						result = CACustomsDataRegistry.Instance.ConsolidateByImporter.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty) ?
							LowValueShipmentsTypes.Codes.ConsolidationByImporter : LowValueShipmentsTypes.Codes.TotalConsolidation;
					}
				}

				return result;
			}
		}

		[List(nameof(BillTos))]
		[RelatedBusinessObject("BillTo")]
		public ZGuid JE_OH_BillTo
		{
			get { return fBillTo == null ? ZGuid.Empty : fBillTo.PK; }
		}

		public OrgHeader BillTo
		{
			get { return fBillTo; }
		}
		OrgHeader fBillTo;

		public ZPropertyInfo JE_OH_BillToInfo
		{
			get { return this.GetZPropertyInfo(Schema.JE_OH_BillTo); }
		}

		public OrgHeaderCollection BillTos
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		[ChildEditable]
		[List(nameof(Vendors))]
		public JobDocAddress SupplierDocumentaryAddress
		{
			get { return fSupplierDocumentaryAddress ?? (fSupplierDocumentaryAddress = JobDocAddressForSimplifiedLVS.New(this, DocAddressTypes.Codes.SupplierDocumentaryAddress)); }
		}

		JobDocAddress fSupplierDocumentaryAddress;

		public ConsignorCollection Vendors
		{
			get
			{
				ConsignorCollection vendors = null;
				if (Invoices.Count == 0)
				{
					vendors = new ConsignorCollection(Factory);
					if (!JE_OH_Importer.IsEmpty)
					{
						vendors.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
							"Consignor - Related Consignee", "Property",
							delegate
							{ return JE_OH_Importer; }));
					}
				}
				else
				{
					vendors = Suppliers;
				}
				return vendors;
			}
		}

		public ConsignorCollection Suppliers
		{
			get { return Declaration != null ? Declaration.Lookups.SuppliersList : new ConsignorCollection(Factory); }
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.JE_GB = GlbBranch.CurrentBranch.PK;
			if (CA_PortOfClearance.IsEmpty)
			{
				this.CA_PortOfClearance = CACustomsDataRegistry.Instance.DefaultPortOfClearance.Value;
			}
			this.JE_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			this.JZ_ValuationDateOverride = ZDateTime.Today;
			this.JZ_InvoiceDate = JZ_ValuationDateOverride.AddDays(-1);
			this.JZ_WeightUQ = Core.Constants.Weight.Pounds;
		}

		public override bool ReadOnly
		{
			get { return Declaration != null; }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Customs Codes defaulting
		void DefaultPortOfClearance()
		{
			UNLOCODefaulter.DefaultCustomsCode(() => CA_PortOfClearanceInfo, CACustomsCodeType.Office, false);
		}

		internal UNLOCODefaulter UNLOCODefaulter
		{
			get
			{
				return unlocoDefaulter ?? (unlocoDefaulter = new UNLOCODefaulter(Factory, () => Branch?.HomePort?.RL_CodeInfo, () => TransportMode));
			}
		}
		UNLOCODefaulter unlocoDefaulter;

		ZString TransportMode
		{
			get
			{
				var transportMode = ZString.Empty;
				if (Carrier is ZZRefCarrierCombined carrier)
				{
					var transPortModePair = carrier.TransportModePairList.FirstOrDefault(x => x.Value);
					if (transPortModePair == null)
					{
						var attr = carrier.Attributes.FirstOrDefault(x => x.ZZG_Name == RefTransportModeList.Codes.AIR
									|| x.ZZG_Name == RefTransportModeList.Codes.RAI
									|| x.ZZG_Name == RefTransportModeList.Codes.ROA
									|| x.ZZG_Name == RefTransportModeList.Codes.SEA);
						if (attr != null)
						{
							transportMode = attr.ZZG_Name;
						}
						else
						{
							transportMode = Core.Constants.TransportModes.Road;
						}
					}
					else
					{
						transportMode = transPortModePair.Description;
					}
				}
				return transportMode;
			}
		}

		#endregion

		#region LoadInvoiceHeader

		public bool LoadInvoiceHeader(Func<bool> getAgreementToCreateNewDeclaration)
		{
			if (Invoices.Count == 0)
			{
				var wrapper = new SimplifiedLVSConsolidationOptionsWrapper(this);
				var strategy = new CreateIndividualLVSShipmentsStrategy(wrapper);
				JobDeclaration declaration;

				if (strategy.HasAcknowledged)
				{
					declaration = Factory.New<JobDeclaration>();
					strategy.FillDataForNewDeclaration(declaration);

					declaration.IsSimplifiedLVSMode = true;
					var invoice = declaration.LVXInvoiceHeader;
					SetValueForInvoice(invoice);
					this.Invoices.Add(invoice);
				}
				else
				{
					var strategies = ConsolidationStrategyProvider.GetConsolidationStrategies(wrapper);
					declaration = LVXJobsConsolidateHelper.FindMatchingLVSDeclarationInDb(Factory, strategies);

					if (declaration == null && getAgreementToCreateNewDeclaration())
					{
						declaration = Factory.New<JobDeclaration>();
						declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
						declaration.JE_OH_Importer = this.JE_OH_Importer;
						declaration.JE_GB = this.JE_GB;
						declaration.JE_CustomsOffice = this.CA_PortOfClearance;
						declaration.CA_ProvinceOfClearance = this.PortOfClearance?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province) ?? ZString.Empty;
						declaration.JE_PeriodMonth = this.JE_PeriodMonth;
						declaration.JE_PeriodYear = this.JE_PeriodYear;
						declaration.JE_MessageSubType = this.JE_MessageSubType;
						declaration.JE_GS_NKCusAgent = this.JE_GS_NKCusAgent;
						if (this.CA_AllowOIC)
						{
							declaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
							declaration.CA_AllowOIC = this.CA_AllowOIC;
						}
					}

					if (declaration != null)
					{
						declaration.IsSimplifiedLVSMode = true;
						var invoice = declaration.Invoices.AddNew();
						SetValueForInvoice(invoice);
						this.Invoices.Add(invoice);
						declaration.CA_RequiresMerge = true;
					}
				}
				if (declaration != null)
				{
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				}
			}
			return this.Invoices.Count > 0;
		}

		void SetValueForInvoice(JobComInvoiceHeader invoice)
		{
			if (invoice != null)
			{
				invoice.CA_LVSCarrier = CA_LVSCarrier;
				invoice.CA_PortOfClearance = CA_PortOfClearance;
				invoice.JZ_ValuationDateOverride = JZ_ValuationDateOverride;
				invoice.JZ_InvoiceNumber = JZ_InvoiceNumber;
				invoice.JZ_InvoiceDate = JZ_InvoiceDate;
				invoice.CA_OtherReference = CA_OtherReference;
				invoice.CA_TimeLimit = CA_TimeLimit;
				invoice.CA_TimeLimitCode = CA_TimeLimitCode;
				invoice.JZ_Weight = JZ_Weight;
				invoice.JZ_WeightUQ = JZ_WeightUQ;
				invoice.JZ_OH_Buyer = JE_OH_Importer;
				invoice.SupplierDocumentaryAddress.OrganisationPK = SupplierDocumentaryAddress.OrganisationPK;
				invoice.SupplierDocumentaryAddress.E2_OA_Address = SupplierDocumentaryAddress.E2_OA_Address;
				originalSupplierDocumentaryAddressOrganisationPK = SupplierDocumentaryAddress.OrganisationPK;
				originalSupplierDocumentaryAddressE2_OA_Address = SupplierDocumentaryAddress.E2_OA_Address;

				var supplierBuyerLink = invoice.SupplierBuyerLink;
				ZString incoTerm = supplierBuyerLink == null ? ZString.Empty : OrgSupplierBuyerLink.GetDefaultINCO(invoice.SupplierBuyerLink, invoice.Supplier_Effective, invoice.Importer_Effective, invoice.JobDeclaration.JE_TransportMode, invoice.JobDeclaration.JE_ContainerMode);
				invoice.JZ_IncoTerm = incoTerm.IsEmpty ? new ZString(Core.Constants.IncoTerms.FreeOnBoard) : incoTerm;
				ZString currency = supplierBuyerLink == null ? ZString.Empty : supplierBuyerLink.DefaultCurrency.RX_Code;
				invoice.JZ_RX_NKInvoice_Currency = currency.IsEmpty ? new ZString(Core.Constants.CurrencyCodes.Canada) : currency;

				invoice.JZ_InvoiceNumberInfo.ValueChanged += JZ_InvoiceNumberInfo_ValueChanged;
				invoice.JZ_ValuationDateOverrideInfo.ValueChanged += JZ_ValuationDateOverrideInfo_ValueChanged;
				invoice.CA_OtherReferenceInfo.ValueChanged += CA_OtherReferenceInfo_ValueChanged;
				invoice.CA_TimeLimitInfo.ValueChanged += CA_TimeLimitInfo_ValueChanged;
				invoice.CA_TimeLimitCodeInfo.ValueChanged += CA_TimeLimitCodeInfo_ValueChanged;
				invoice.SupplierDocumentaryAddress.DocAddressChanged += SupplierDocumentaryAddressChanged;

				RegisterEditableChildObject(invoice.InvoiceLines);
			}
		}

		void CA_TimeLimitCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			CA_TimeLimitCodeInfo.RefreshBinding();
		}

		void CA_TimeLimitInfo_ValueChanged(object sender, EventArgs e)
		{
			CA_TimeLimitInfo.RefreshBinding();
		}

		ZGuid originalSupplierDocumentaryAddressOrganisationPK;
		ZGuid originalSupplierDocumentaryAddressE2_OA_Address;

		void JZ_InvoiceNumberInfo_ValueChanged(object sender, EventArgs e)
		{
			JZ_InvoiceNumberInfo.RefreshBinding();
		}

		void CA_OtherReferenceInfo_ValueChanged(object sender, EventArgs e)
		{
			CA_OtherReferenceInfo.RefreshBinding();
		}

		void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			var invoiceSupplierDocumentaryAddress = sender as JobDocAddress;
			if (invoiceSupplierDocumentaryAddress != null)
			{
				SupplierDocumentaryAddress.OrganisationPK = invoiceSupplierDocumentaryAddress.OrganisationPK;
				SupplierDocumentaryAddress.E2_OA_Address = invoiceSupplierDocumentaryAddress.E2_OA_Address;
			}
		}

		void JZ_ValuationDateOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			JZ_ValuationDateOverrideInfo.RefreshBinding();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			var invoice = Invoices.Count > 0 ? Invoices[0] : null;
			if (invoice != null && invoice.JZ_InvoiceAmount.IsEmpty)
			{
				invoice.JZ_InvoiceAmount = invoice.InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_LinePrice);
			}
		}

		#endregion

		#region CreateNewSimplifiedLVS

		public SimplifiedLVS CreateNewSimplifiedLVS(BusinessObjectFactory factory)
		{
			var result = new SimplifiedLVS(factory);
			using (result.SuspendSettingHasChanges())
			{
				result.JE_OH_Importer = base.JE_OH_Importer;
				result.JE_GB = base.JE_GB;
				result.CA_PortOfClearance = base.CA_PortOfClearance;
				result.JE_GS_NKCusAgent = base.JE_GS_NKCusAgent;
				result.CA_LVSCarrier = base.CA_LVSCarrier;
				result.JZ_ValuationDateOverride = base.JZ_ValuationDateOverride;
				result.JE_EntryAuthorisationDate = base.JE_EntryAuthorisationDate;
				result.CA_AllowOIC = base.CA_AllowOIC;
			}

			return result;
		}

		#endregion

		#region Validation

		public bool OnlyValidateCargoListHeaderProperty
		{
			get { return fOnlyValidateCargoListHeaderProperty; }
			set
			{
				fOnlyValidateCargoListHeaderProperty = value;
				if (fOnlyValidateCargoListHeaderProperty)
				{
					UnRegisterEditableChildObject(SupplierDocumentaryAddress);
				}
				else
				{
					RegisterEditableChildObject(SupplierDocumentaryAddress);
				}
			}
		}
		bool fOnlyValidateCargoListHeaderProperty;

		public override void ValidateJE_GB()
		{
			base.ValidateJE_GB();
			MandatoryValidation.CheckEntered(JE_GBInfo);
		}

		public override void ValidateCA_PortOfClearance()
		{
			base.ValidateCA_PortOfClearance();
			MandatoryValidation.CheckEntered(CA_PortOfClearanceInfo);
			ListValidation.ErrorIfInvalidCode(CA_PortOfClearanceInfo);
		}

		public override void ValidateJE_OH_Importer()
		{
			base.ValidateJE_OH_Importer();
			MandatoryValidation.CheckEntered(JE_OH_ImporterInfo);
		}

		public override void ValidateJE_GS_NKCusAgent()
		{
			base.ValidateJE_GS_NKCusAgent();
			MandatoryValidation.CheckEntered(JE_GS_NKCusAgentInfo);
			ListValidation.ErrorIfInvalidCode(JE_GS_NKCusAgentInfo);
		}

		public override void ValidateCA_LVSCarrier()
		{
			base.ValidateCA_LVSCarrier();
			ListValidation.ErrorIfInvalidCode(CA_LVSCarrierInfo);
		}

		public override void ValidateJZ_WeightUQ()
		{
			base.ValidateJZ_WeightUQ();
			ListValidation.ErrorIfInvalidCode(JZ_WeightUQInfo);
		}

		public override void ValidateJZ_ValuationDateOverride()
		{
			base.ValidateJZ_ValuationDateOverride();
			MandatoryValidation.CheckEntered(JZ_ValuationDateOverrideInfo);
		}

		public override void ValidateJZ_InvoiceNumber()
		{
			base.ValidateJZ_InvoiceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(base.JZ_InvoiceNumberInfo);
		}

		public override void ValidateCA_TimeLimit()
		{
			base.ValidateCA_TimeLimit();
			if (!CA_TimeLimitCode.IsEmpty && CA_TimeLimit.IsEmpty)
			{
				CA_TimeLimitInfo.AddMessageError(Res.GetString("fa2b211c-26d0-4cb6-b59d-6b82e2956128", "You must enter a time limit when you have entered a time limit code"));
			}
		}

		public override void ValidateCA_TimeLimitCode()
		{
			base.ValidateCA_TimeLimitCode();
			ValidateCA_TimeLimit();
		}

		void ClearAllNotificationsForUnwrappedProperties()
		{
			base.JE_OH_ImporterInfo.ClearAllNotifications();
			base.JE_GBInfo.ClearAllNotifications();
			base.JE_PeriodYearInfo.ClearAllNotifications();
			base.CA_PortOfClearanceInfo.ClearAllNotifications();
			base.JE_PeriodMonthInfo.ClearAllNotifications();
			base.JE_EntryAuthorisationDateInfo.ClearAllNotifications();
			base.JE_GS_NKCusAgentInfo.ClearAllNotifications();
			base.CA_LVSCarrierInfo.ClearAllNotifications();
			base.JZ_ValuationDateOverrideInfo.ClearAllNotifications();
			base.JZ_InvoiceNumberInfo.ClearAllNotifications();
			base.JZ_InvoiceDateInfo.ClearAllNotifications();
			base.CA_OtherReferenceInfo.ClearAllNotifications();
			base.CA_TimeLimitInfo.ClearAllNotifications();
			base.CA_TimeLimitCodeInfo.ClearAllNotifications();
			base.JZ_WeightInfo.ClearAllNotifications();
			base.JZ_WeightUQInfo.ClearAllNotifications();
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotificationsForUnwrappedProperties();

			if (Invoices.Count == 0)
			{
				if (OnlyValidateCargoListHeaderProperty)
				{
					ValidateCA_LVSCarrier();
					ValidateCA_PortOfClearance();
					ValidateJZ_ValuationDateOverride();
					ValidateJE_GB();
					ValidateJE_GS_NKCusAgent();
				}
				else
				{
					base.RunPreSaveValidationCore();
				}
			}
		}

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		public BuyerSupplierLinksHelper<SimplifiedLVS> BuyerSupplierLinksHelper;

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Importer; }
			set { JE_OH_Importer = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { JE_OH_ImporterInfo.ValueChanged += value; }
			remove { JE_OH_ImporterInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return null; }
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return SupplierDocumentaryAddress.Organisation; }
			set { SupplierDocumentaryAddress.OrganisationPK = value.PK; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { SupplierDocumentaryAddress.DocAddressChanged += value; }
			remove { SupplierDocumentaryAddress.DocAddressChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return Declaration != null ? Declaration.JE_RL_NKFinalDestination : ZString.Empty; }
			set
			{
				if (Declaration != null)
				{
					Declaration.JE_RL_NKFinalDestination = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_RX_NKInvoice_Currency : ZString.Empty; }
			set
			{
				if (Invoices.Count > 0)
				{
					Invoices[0].JZ_RX_NKInvoice_Currency = value;
				}
			}
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.IsSettingDefaultValues
		{
			get { return false; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return ZString.Empty; }
			set { }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add { }
			remove { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return Invoices.Count > 0 ? Invoices[0].JZ_IncoTerm : ZString.Empty; }
			set
			{
				if (Invoices.Count > 0)
				{
					Invoices[0].JZ_IncoTerm = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return ZString.Empty; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreImportBrokerFallback()
		{
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return ZString.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return ShouldPromptToSaveBuyerSupplierRelationship; }
		}

		protected virtual ZBool ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.PromptToSaveBuyerSupplier; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return false; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get { return Declaration != null ? Declaration.JE_TransportMode : ZString.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region ISupportDataImporting Members

		protected bool fIsImportingData;
		public bool IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion
	}
}
