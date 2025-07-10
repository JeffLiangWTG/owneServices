using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.SDF;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ProjectCollection = Enterprise.Client.EDI.IncidentManager.Business.ProjectCollection;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgOpportunity : OrgOpportunity, IClientOrgLicenceProvider, IWorkTaskTreeNode, IImportChildRelatedActivityInfoOnAttach, IImportChildRelatedActivityInfoOnDetach
	{
		public EDIOrgOpportunity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P8_RX_NKEstimatedValueCurrency = EDIOrgOpportunityConstants.OpportunityDefaultCurrency;
		}

		#region Client

		public EDIOrgHeader Client
		{
			get { return Factory.Load<EDIOrgHeader>(P8_OH); }
		}

		#endregion

		#region Value Items

		[ChildEditable(true)]
		public new EDIOrgOpportunityValueCollection ValueItems
		{
			get { return (EDIOrgOpportunityValueCollection)base.ValueItems; }
		}

		protected override OrgOpportunityValueCollection GetNewValueItemsCollection()
		{
			return new EDIOrgOpportunityValueCollection(this);
		}

		#endregion

		protected override OrgOpportunityLookups GetNewLookups()
		{
			return new EDIOrgOpportunityLookups(this);
		}

		public new EDIOrgOpportunityLookups Lookups
		{
			get { return (EDIOrgOpportunityLookups)base.Lookups; }
		}

		#region Project Opportunity Pivot

		public ProjectCollection RelatedProjects
		{
			get
			{
				if (relatedProjects == null)
				{
					var localRelatedProjects = new ProjectCollection(Factory);
					var query = new ZQuery(WorkProjectSchema.WKP_P8_Opportunity, PK);
					localRelatedProjects.Load(query);
					relatedProjects = localRelatedProjects;
				}
				return relatedProjects;
			}
		}
		ProjectCollection relatedProjects;

		void RemoveProjectOpportunityPivots()
		{
			var query = new ZQuery(WorkProjectSchema.WKP_P8_Opportunity, PK);
			foreach (var project in Factory.Load<EDIProject>(query))
			{
				project.WKP_P8_Opportunity = ZGuid.Empty;
			}
		}

		#endregion

		#region Business Objects With Related Notes

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				if (businessObjectsWithRelatedNotes == null)
				{
					businessObjectsWithRelatedNotes = new BusinessObject[1];
				}
				businessObjectsWithRelatedNotes[0] = Client;
				return businessObjectsWithRelatedNotes;
			}
		}
		BusinessObject[] businessObjectsWithRelatedNotes;

		#endregion

		#region Note Type

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection types = base.NoteTypesCore;
				types.Add(EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary);
				return types;
			}
		}

		#endregion

		#region IDocumentSupportable Members

		[ChildEditable(true)]
		public StmSystemDefinedFieldWrapperCollection DocDataFieldWrappers
		{
			get
			{
				if (docDataFieldWrappers == null)
				{
					DocumentNote documentNote = DocumentNote.LoadNote(this);
					docDataFieldWrappers = documentNote.SystemDefinedFieldWrappers;
					RegisterEditableChildObject(docDataFieldWrappers);
				}
				return docDataFieldWrappers;
			}
		}

		StmSystemDefinedFieldWrapperCollection docDataFieldWrappers;

		#endregion

		public override void Delete()
		{
			RemoveProjectOpportunityPivots();
			RemovePSQOpportunityPivots();
			OrgOpportunityEx.Delete();
			ValueAnalysisCollection.DeleteAll();
			base.Delete();
		}

		#region IClientOrgLicenceProvider Members

		EDIOrgHeader IClientOrgLicenceProvider.LicenceOrganisation
		{
			get { return Client; }
		}

		ZString IClientOrgLicenceProvider.ReferenceNumber
		{
			get { return P8_OpportunityID; }
		}

		#endregion

		#region Related PSQ's

		ProfessionalServicesQuoteCollection relatedPSQs;

		public ProfessionalServicesQuoteCollection RelatedPSQs
		{
			get
			{
				if (relatedPSQs == null)
				{
					var localRelatedPSQs = new ProfessionalServicesQuoteCollection(Factory);
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProfessionalServicesQuote));
					ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
					pivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);
					pivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, this.PK);
					query.AddSubQuery(pivotSubQuery, JoinCondition.And);
					localRelatedPSQs.Load(query);
					relatedPSQs = localRelatedPSQs;
					RelatedPSQs.ItemAdded += new ProfessionalServicesQuoteCollection.ItemCountChangedEventHandler(relatedPSQs_ItemAdded);
					RelatedPSQs.ItemRemoved += new ProfessionalServicesQuoteCollection.ItemCountChangedEventHandler(RelatedPSQs_ItemRemoved);
				}
				return relatedPSQs;
			}
		}

		void relatedPSQs_ItemAdded(BusinessObject bizO)
		{
			if (!RelatedPSQs.IsLoading)
			{
				ProfessionalServicesQuote psq = bizO as ProfessionalServicesQuote;
				if (psq != null)
				{
					this.CreateNewPSQOpportunityPivot(psq);
					this.HasChanges = true;
				}
			}
		}

		void RelatedPSQs_ItemRemoved(BusinessObject bizO)
		{
			ProfessionalServicesQuote psq = bizO as ProfessionalServicesQuote;
			if (psq != null)
			{
				this.RemovePSQOpportunityPivot(psq);
				this.HasChanges = true;
			}
		}

		public void InitializeFromPSQ(ProfessionalServicesQuote psq)
		{
			P8_OH = psq.IM_OH_Client;
			P8_OpportunityDescription = psq.IM_Description;
			P8_OC = psq.IM_OC_Contact;
			P8_Status = "CRT";
			if (psq.Client != null)
			{
				if (!psq.Client.StaffAssignments.OverallSalesRep.IsEmpty)
				{
					P8_GS_NKPrimarySalesPerson = psq.Client.StaffAssignments.OverallSalesRep;
				}
				else
				{
					OrgStaffAssignmentsCollection staff = new OrgStaffAssignmentsCollection(psq.Client);
					staff.CompanySpecific = false;
					OrgStaffAssignments sales = staff.Cast<OrgStaffAssignments>().FirstOrDefault(s => s.O8_Role == StaffAssignmentRoles.Codes.SalesRep);
					if (sales != null)
					{
						P8_GS_NKPrimarySalesPerson = sales.O8_GS_NKPersonResponsible;
					}
				}
			}

			P8_EstimatedValue = psq.IM_QuoteAmount;
			if (P8_EstimatedValue != 0)
			{
				EDIOrgOpportunityValue valueItem = this.ValueItems.AddNew();
				valueItem.PV_RevenueType = EDIOrgOpportunityValueLookups.ValueTypeConstants.SER;
				valueItem.PV_Value = P8_EstimatedValue;
			}
			P8_RX_NKEstimatedValueCurrency = psq.IM_RX_NKQuoteCurrency;

			P8_Outcome = "OPE";
			P8_RentalMultiplier = 0;
			P8_OpportunityType = EDIDataRegistry.Instance.DefaultOpportunityObjective.Value;
		}

		#region PSQ Opportunity Pivot

		public GenPivot CreateNewPSQOpportunityPivot(ProfessionalServicesQuote psq)
		{
			GenPivot psqOpportunityPivot = Factory.New<GenPivot>();
			psqOpportunityPivot.XX_Relation1ID = psq.PK;
			psqOpportunityPivot.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
			psqOpportunityPivot.XX_Relation2ID = this.PK;
			psqOpportunityPivot.XX_Relation2TableCode = OrgOpportunitySchema.Constants.Prefix;

			return psqOpportunityPivot;
		}

		public GenPivot LoadPSQOpportunityPivot(ProfessionalServicesQuote psq)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, psq.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, this.PK);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);
			GenPivot psqOpportunityPivot = Factory.LoadTop1<GenPivot>(query);
			return psqOpportunityPivot;
		}

		public void RemovePSQOpportunityPivot(ProfessionalServicesQuote psq)
		{
			GenPivot psqOpportunityPivot = LoadPSQOpportunityPivot(psq);
			if (psqOpportunityPivot != null)
			{
				psqOpportunityPivot.Delete();
			}
		}

		void RemovePSQOpportunityPivots()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GenPivot));
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, this.PK);

			GenPivot[] pivots = Factory.Load<GenPivot>(query);
			foreach (GenPivot pivot in pivots)
			{
				pivot.Delete();
			}
		}

		#endregion

		#endregion

		#region MiscServ

		public EdiOrgOpportunityEx OrgOpportunityEx
		{
			get
			{
				if (orgOpportunityEx == null)
				{
					orgOpportunityEx = Factory.LoadTop1<EdiOrgOpportunityEx>(new ZQuery(EdiOrgOpportunityExSchema.EOM_P8, PK));
					if (orgOpportunityEx == null)
					{
						orgOpportunityEx = Factory.New<EdiOrgOpportunityEx>();
						orgOpportunityEx.EOM_P8 = PK;
						orgOpportunityEx.HasChanges = false;
					}
					RegisterEditableChildObject(orgOpportunityEx);
				}
				return orgOpportunityEx;
			}
		}

		EdiOrgOpportunityEx orgOpportunityEx;

		protected override bool P8_EstimatedValue_ReadOnlyCore
		{
			get { return false; }
		}

		[DecimalPlaces(2)]
		public ZDecimal P8_Calc_ContractValueLocal
		{
			get { return GetLocalCurrencyValue(P8_RX_NKEstimatedValueCurrency, P8_EstimatedValue, P8_Calc_ContractValueLocalCurrency); }
		}
		public ZPropertyInfo P8_Calc_ContractValueLocalInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_ContractValueLocal)); }
		}

		public ZString P8_Calc_ContractValueLocalCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityDefaultLocalCurrency; }
		}
		public ZPropertyInfo P8_Calc_ContractValueLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_ContractValueLocalCurrency)); }
		}

		[ResourceStringData("078c7421-c40e-4a86-acf0-4a4f6fea3518", Caption = "Contracted")]
		[DecimalPlaces(0)]
		public override ZDecimal P8_DiscountAmount
		{
			get
			{
				return base.P8_DiscountAmount;
			}

			set
			{
				base.P8_DiscountAmount = value;
			}
		}

		[ResourceStringData("07070ef3-6ecf-4792-9ffc-7ecf7ab33dd1", Caption = "Local Reach")]
		[DecimalPlaces(0)]
		public override ZDecimal P8_RentalMultiplier
		{
			get
			{
				return base.P8_RentalMultiplier;
			}

			set
			{
				base.P8_RentalMultiplier = value;
			}
		}

		public override void UpdateEstimatedValue()
		{
		}

		[ChildEditable(true)]
		public EdiOrgOpportunityValueAnalysisCollection ValueAnalysisCollection
		{
			get
			{
				if (valueAnalysisCollection == null)
				{
					valueAnalysisCollection = new EdiOrgOpportunityValueAnalysisCollection(this);
					valueAnalysisCollection.PopulateDefaultValues();
					foreach (var obj in valueAnalysisCollection)
					{
						obj.HasChanges = false;
					}

					RegisterEditableChildObject(valueAnalysisCollection);
				}

				return valueAnalysisCollection;
			}
		}

		EdiOrgOpportunityValueAnalysisCollection valueAnalysisCollection;

		public ZDecimal P8_Calc_TotalForeignValue
		{
			get { return ValueAnalysisCollection.TotalForeignValue; }
		}
		public ZPropertyInfo P8_Calc_TotalForeignValueInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_TotalForeignValue)); }
		}

		public ZDecimal P8_Calc_TotalLocalValue
		{
			get { return ValueAnalysisCollection.TotalLocalValue; }
		}
		public ZPropertyInfo P8_Calc_TotalLocalValueInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_TotalLocalValue)); }
		}

		public ZString P8_Calc_TotalForeignValueCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityValueAnalysisDefaultCurrency; }
		}
		public ZPropertyInfo P8_Calc_TotalForeignValueCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_TotalForeignValueCurrency)); }
		}

		public ZString P8_Calc_TotalLocalValueCurrency
		{
			get { return EDIOrgOpportunityConstants.OpportunityDefaultLocalCurrency; }
		}
		public ZPropertyInfo P8_Calc_TotalLocalValueCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(P8_Calc_TotalLocalValueCurrency)); }
		}

		public void UpdateTotalValueBinding()
		{
			P8_Calc_TotalForeignValueInfo.RefreshBinding();
			P8_Calc_TotalLocalValueInfo.RefreshBinding();
		}

		public ZDecimal GetLocalCurrencyValue(ZString foreignCurrency, ZDecimal foreignValue, ZString localCurrency)
		{
			var exchangeRateCompany = Factory.Load<GlbCompany>(EDIDataRegistry.Instance.OpportunityExchangeRateCompany.Value);

			if (exchangeRateCompany == null || exchangeRateCompany.GC_RX_NKLocalCurrency != localCurrency)
			{
				return 0m;
			}
			else
			{
				var currencyConverter = CurrencyConverter.New(exchangeRateCompany, Factory, DateForExchangeRate, ExchangeRateType.Sell, 30);
				var foreign = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, foreignCurrency);
				var local = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, localCurrency);
				return currencyConverter.ConvertRounded(new Money(foreignValue, foreign), local).Amount;
			}
		}

		protected override OrgOpportunityValidation GetNewValidation()
		{
			return new EDIOrgOpportunityValidation(this);
		}

		public override ZDateTime P8_DateForExchangeRate
		{
			get
			{
				return base.P8_DateForExchangeRate;
			}

			set
			{
				base.P8_DateForExchangeRate = value;
				RefreshBindingIncludingChildren();
			}
		}

		#endregion

		#region IWorkTaskTreeNode Members

		public ProcessTask CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					currentTask = new CachedProperty<ProcessTask>(Factory, () => TaskFinder.FindCurrentStartableTask());
				}
				return currentTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentTask;

		CurrentTaskFinder TaskFinder => taskFinder ?? (taskFinder = new CurrentTaskFinder(this));
		CurrentTaskFinder taskFinder;

		public ZString Number => P8_OpportunityID;
		public ZPropertyInfo NumberInfo => P8_OpportunityIDInfo;

		public ZString ItemDescription => P8_OpportunityDescription;
		public ZPropertyInfo ItemDescriptionInfo => P8_OpportunityDescriptionInfo;

		ZString IWorkTaskRelatedItem.StatusDescription => StatusDescription;

		public ZString Type => WorkTaskRelatedItemTypes.Opportunity;
		public ZPropertyInfo TypeInfo => P8_OpportunityTypeInfo;

		public ZString ClientCode => Header?.OH_Code ?? ZString.Empty;
		public ZPropertyInfo ClientCodeInfo => GetZPropertyInfo(nameof(ClientCode));

		public ZString ClientName => Header?.OH_FullName ?? ZString.Empty;
		public ZPropertyInfo ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ControllerID ControllerID => ControllerIDs.Opportunity;

		public ZString AssignedStaffCode => P8_GS_NKPrimarySalesPerson;
		public ZPropertyInfo AssignedStaffCodeInfo => P8_GS_NKPrimarySalesPersonInfo;

		public ZString Source => P8_Source;
		public ZPropertyInfo SourceInfo => P8_SourceInfo;

		public ZString Criticality => P8_Status;
		public ZPropertyInfo CriticalityInfo => P8_StatusInfo;

		public ZBool IsClosedOrCancelled => IsClosed;

		public ZDateTime AgreedDeliveryDate => P8_EstimatedCloseDateLocal;

		public ZString CurrentTaskStatus => CurrentTask?.P9_Status ?? string.Empty;

		public ZString CurrentTaskDescription => CurrentTask?.P9_Description ?? string.Empty;

		public ZString CurrentTaskCapabilityCodeDescription => CurrentTask?.CapabilityCode ?? string.Empty;

		public ZString CurrentTaskAssigned => CurrentTask?.P9_GS_NKAssignedStaffMember ?? string.Empty;

		public ZString SelectionCriterion1Code => P8_OpportunityType;
		public ZString SelectionCriterion1 => SelectionCriterion1Code;
		public ZPropertyInfo SelectionCriterion1Info => GetZPropertyInfo(nameof(SelectionCriterion1));

		public ZString SelectionCriterion2Code => Company?.GC_Code ?? ZString.Empty;
		public ZString SelectionCriterion2 => SelectionCriterion2Code;
		public ZPropertyInfo SelectionCriterion2Info => GetZPropertyInfo(nameof(SelectionCriterion2));

		public ZString SelectionCriterion3Code => P8_PackageType;
		public ZString SelectionCriterion3 => SelectionCriterion3Code;
		public ZPropertyInfo SelectionCriterion3Info => GetZPropertyInfo(nameof(SelectionCriterion3));

		public ZString SelectionCriterion4Code => P8_Source;
		public ZString SelectionCriterion4 => SelectionCriterion4Code;
		public ZPropertyInfo SelectionCriterion4Info => GetZPropertyInfo(nameof(SelectionCriterion4));

		public ZString SelectionCriterion5Code => CloseCertaintyAsPercentageString;
		public ZString SelectionCriterion5 => SelectionCriterion5Code;
		public ZPropertyInfo SelectionCriterion5Info => GetZPropertyInfo(nameof(SelectionCriterion5));

		public Type PivotCollectionType => typeof(OrgOpportunityGenPivotCollection);

		public BusinessObjectCollection ChildrenOnlyRelatedItems { get; }
		public BusinessObjectCollection ParentsOnlyRelatedItems { get; }

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion

		#region ImportChildInfo

		protected override void ImportChildInfoOnAttach(IRelatableActivity childActivity)
		{
			base.ImportChildInfoOnAttach(childActivity);
			if (childActivity is SupportIncident childIncident)
			{
				DeleteGenPivotIfExists(childIncident.PK, IncidentMainSchema.Constants.Prefix, PK, OrgOpportunitySchema.Constants.Prefix);
				CreateGenPivot(PK, OrgOpportunitySchema.Constants.Prefix, childIncident.PK, IncidentMainSchema.Constants.Prefix);
			}
		}

		protected override void ImportChildInfoOnDetach(IRelatableActivity childActivity)
		{
			base.ImportChildInfoOnDetach(childActivity);
			if (childActivity is SupportIncident childIncident)
			{
				DeleteGenPivotIfExists(PK, OrgOpportunitySchema.Constants.Prefix, childIncident.PK, IncidentMainSchema.Constants.Prefix);
			}
		}

		void CreateGenPivot(ZGuid relation1ID, string relation1TableCode, ZGuid relation2ID, string relation2TableCode)
		{
			var incidentPivot = Factory.New<GenPivot>();
			incidentPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			incidentPivot.XX_Relation1ID = relation1ID;
			incidentPivot.XX_Relation1TableCode = relation1TableCode;
			incidentPivot.XX_Relation2ID = relation2ID;
			incidentPivot.XX_Relation2TableCode = relation2TableCode;
		}

		void DeleteGenPivotIfExists(ZGuid relation1ID, string relation1TableCode, ZGuid relation2ID, string relation2TableCode)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, relation1ID);
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1TableCode);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, relation2ID);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);

			var genPivot = Factory.LoadTop1<GenPivot>(query);
			genPivot?.Delete();
		}

		#endregion
	}
}
