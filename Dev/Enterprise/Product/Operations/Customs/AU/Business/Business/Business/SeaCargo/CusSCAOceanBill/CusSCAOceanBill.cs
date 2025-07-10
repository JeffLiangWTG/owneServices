using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[UserDefinedValues]
	[SystemDefinedValues]
	[CodeProperty(CusSCAOceanBill.Schema.CB_OceanBill), DescriptionProperty(CusSCAOceanBill.Schema.CB_OceanBill)]
	public class CusSCAOceanBill : BaseCusSCAOceanBill,
		IManifestProvider,
		ISeaCargoConsolInfo,
		ICMRMessageRespondee,
		IMessageManageableBizObj,
		Integration.Customs.AU.ICusSCAOceanBill,
		IDataExportCSVFileNameProvider,
		IWorkflowProvider,
		IWorkflowTriggerEventSource,
		ICustomFieldProvider,
		IAUCusUnderbondUnionCollectionParent,
		IConsignmentKeyChangeInhibitor,
		IDocManagerSupportIncudingRelatedObjects,
		ITriggerActionMessagingSupporterProvider,
		ITriggerActionMessagingSupporter,
		IValidateForCustomsMessagingSupporter,
		IScanMasterBillProvider,
		IControllerIDProvider
	{
		public new class Schema : AutoCusSCAOceanBill.Schema
		{
			public const string CB_OverrideFreightDefaults = "CB_OverrideFreightDefaults";
			public const string CustomsShipmentStatusFilter = "CustomsShipmentStatusFilter";
			public const string CustomsMessageStatusFilter = "CustomsMessageStatusFilter";
			public const string InvalidHouseBillsOnlyFilter = "InvalidHouseBillsOnlyFilter";
		}

		public CusSCAOceanBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZString[] ApplicationCodes
		{
			get
			{
				return new ZString[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
			}
		}

		public override ZString[] ApplicationCodesForBase
		{
			get { return ApplicationCodes; }
		}

		public static CusSCAOceanBill Load(ForwardingConsol consol)
		{
			return (CusSCAOceanBill)new Loader(consol.Factory).LoadFromConsolAndApplicationCode(consol, ApplicationCodes);
		}

		public static new readonly TypeDecider TypeDecider = new CusSCAOceanBillTypeDecider();

		#region New Properties

		public override bool CanDelete
		{
			get
			{
				foreach (CusSCAHouse house in HouseBills)
				{
					if (!house.CanDelete)
					{
						return false;
					}
				}

				foreach (CusSCAPivot pivot in Pivots)
				{
					foreach (CusUnderbond underbond in pivot.Underbonds)
					{
						if (underbond == null || !underbond.CanDelete)
						{
							return false;
						}
					}
				}

				foreach (CusSCAContainer container in Containers)
				{
					foreach (CusUnderbond underbond in container.Underbonds)
					{
						if (underbond == null || !underbond.CanDelete)
						{
							return false;
						}
					}
				}

				return true;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("3229AAF4-15AA-445D-8BA7-4B4D2C2B4F54", "There are Pack Lines or House bills that have messages attached to this ocean bill."); }
		}

		#endregion

		#region Overrides

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusSCAOceanBillFetchStrategy(this);

		protected override OrgHeader GetEffectiveResponsiblePartyOrgHeader()
		{
			return !CB_ResponsiblePartyID.IsEmpty ? new OrgHeader.Loader(Factory).LoadDBOrganisations(Core.Constants.CountryCodes.Australia, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, CB_ResponsiblePartyID).FirstOrDefault() : null;
		}

		public override void DefaultFromConsol()
		{
			var consol = Consol;
			if (consol != null)
			{
				var synchroniser = new CMRSeaCargoSynchroniser(consol, this);
				synchroniser.DefaultOceanBillDetailsAndContainers();
			}
		}

		public override void LoadHouseBills()
		{
			HouseBills.Load();
		}

		#endregion

		#region Related BusinessObjects

		#region UnregisterHouseBillsFromEditableChildren

		public void UnregisterHouseBillsFromEditableChildren()
		{
			canRegisterEditableChildren = false;
			UnRegisterEditableChildObject(HouseBills);
			UnRegisterEditableChildObject(Containers);
			UnRegisterEditableChildObject(Pivots);
			UnRegisterEditableChildObject(AllUnderbonds);
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			if (canRegisterEditableChildren || !IsHouseBillOrDependantEditableChild(child))
			{
				base.RegisterEditableChildObject(child);
			}

			if (child is GenAddOnColumn systemDefinedValue && systemDefinedValue.XA_Name == Schema.CB_OverrideFreightDefaults)
			{
				HookOverrideFreightDefaultsOnUpdated(systemDefinedValue);
			}
		}

		public override void UnRegisterEditableChildObject(IBusiness child)
		{
			base.UnRegisterEditableChildObject(child);

			if (child is GenAddOnColumn systemDefinedValue && systemDefinedValue.XA_Name == Schema.CB_OverrideFreightDefaults)
			{
				UnhookOverrideFreightDefaultsOnUpdated(systemDefinedValue);
			}
		}

		protected bool IsHouseBillOrDependantEditableChild(IBusiness child) => (child is CusSCAHouseCollection) || (child is CusSCAContainerCollection) || (child is CusSCAPivotCollection) || (child is Customs.Business.CusUnderbondUnionCollection);

		bool canRegisterEditableChildren = true;

		#endregion

		#region HouseBills

		[ChildEditable(true)]
		public CusSCAHouseCollection HouseBills
		{
			get
			{
				if (fHouseBills == null)
				{
					fHouseBills = GetNewCusSCAHouseCollection();
					RegisterEditableChildObject(fHouseBills);
					fHouseBills.Load();
					fHouseBills.IsManagedForDataRefresh = true;
					fHouseBills.CountChanged += OnHouseBillsCountChanged;
				}
				return fHouseBills;
			}
		}
		CusSCAHouseCollection fHouseBills;

		protected CusSCAHouseCollection GetNewCusSCAHouseCollection()
		{
			return new CusSCAHouseCollection(this, Factory);
		}

		#endregion

		#region Filtered House Bills

		public CusSCAHouseFilteredCollection FilteredHouseBills
		{
			get
			{
				if (filteredHouseBills == null)
				{
					filteredHouseBills = GetNewFilteredHouseBillsCollection();
					filteredHouseBills.Rebuild();
				}
				return filteredHouseBills;
			}
		}
		CusSCAHouseFilteredCollection filteredHouseBills;

		protected CusSCAHouseFilteredCollection GetNewFilteredHouseBillsCollection()
		{
			return new CusSCAHouseFilteredCollection(this);
		}

		void OnHouseBillsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			filteredHouseBills?.Rebuild();
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAOceanBillLookups.CustomsShipmentStatusList))]
		[BusinessObjectTestExclude]
		[MaxLength(CusSCAHouse.Schema.CA_ShipmentStatusMaxLength)]
		public ZString CustomsShipmentStatusFilter
		{
			get
			{
				return customsShipmentStatusFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(CustomsShipmentStatusFilterInfo, ref customsShipmentStatusFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateCustomsShipmentStatusFilter();
						}
						CustomsShipmentStatusFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZString customsShipmentStatusFilter;

		public ZPropertyInfo CustomsShipmentStatusFilterInfo => GetZPropertyInfo(Schema.CustomsShipmentStatusFilter);

		[List(nameof(Lookups) + "." + nameof(CusSCAOceanBillLookups.CustomsMessageStatusList))]
		[BusinessObjectTestExclude]
		[MaxLength(CusSCAHouse.Schema.CA_MessageStatusMaxLength)]
		public ZString CustomsMessageStatusFilter
		{
			get
			{
				return customsMessageStatusFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(CustomsMessageStatusFilterInfo, ref customsMessageStatusFilter, value))
					{
						if (!IsValidationSuspended)
						{
							Validation.ValidateCustomsMessageStatusFilter();
						}
						CustomsMessageStatusFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZString customsMessageStatusFilter;

		public ZPropertyInfo CustomsMessageStatusFilterInfo => GetZPropertyInfo(Schema.CustomsMessageStatusFilter);

		[BusinessObjectTestExclude]
		public ZBool InvalidHouseBillsOnlyFilter
		{
			get
			{
				return invalidHouseBillsOnlyFilter;
			}
			set
			{
				using (SuspendSettingHasChanges())
				{
					if (SetNonPersistentPropertyValue(InvalidHouseBillsOnlyFilterInfo, ref invalidHouseBillsOnlyFilter, value))
					{
						InvalidHouseBillsOnlyFilterInfo.RefreshBinding();
					}
				}
			}
		}
		ZBool invalidHouseBillsOnlyFilter;

		public ZPropertyInfo InvalidHouseBillsOnlyFilterInfo => GetZPropertyInfo(Schema.InvalidHouseBillsOnlyFilter);

		#endregion // Filtered House Bills

		#region Containers

		[ChildEditable]
		public CusSCAContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new CusSCAContainerCollection(this, Factory);
					RegisterEditableChildObject(fContainers);
					fContainers.Load();
					fContainers.IsManagedForDataRefresh = true;
				}
				return fContainers;
			}
		}
		protected CusSCAContainerCollection fContainers;

		#endregion

		#region Pivots

		[ChildEditable]
		public CusSCAPivotCollection Pivots
		{
			get
			{
				if (fPivots == null)
				{
					fPivots = new CusSCAPivotCollection(this);
					RegisterEditableChildObject(fPivots);
				}
				return fPivots;
			}
		}
		CusSCAPivotCollection fPivots;

		public CusSCAPivotCollection GetPivotsIfAlreadyLoaded() => fPivots;

		#endregion

		#endregion

		#region Properties

		public override ZString CB_ApplicationCode
		{
			get => base.CB_ApplicationCode;
			set
			{
				var oldValue = base.CB_ApplicationCode;
				base.CB_ApplicationCode = value;
				if (!IsMarkingAsNeedingValidationSuspended && !IsCopying && oldValue != CB_ApplicationCode)
				{
					Pivots.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(CB_OH_ShippingLine_ReadOnly))]
		public override ZGuid CB_OH_ShippingLine
		{
			get => base.CB_OH_ShippingLine;
			set
			{
				var oldValue = base.CB_OH_ShippingLine;
				base.CB_OH_ShippingLine = value;
				var newValue = CB_OH_ShippingLine;
				if (!IsCopying && oldValue != newValue)
				{
					ResetCustomBusinessObject();
				}

				if (ShippingLine != null && !ShippingLine.PrimaryRegistrationNumber.Number.IsEmpty)
				{
					var primaryRegNum = ShippingLine.PrimaryRegistrationNumber.Number.Replace(" ", "");
					if (primaryRegNum.Length <= CusSCAOceanBill.Schema.CB_PrincipalIDMaxLength)
					{
						base.CB_PrincipalID = primaryRegNum;
					}
					else
					{
						if (base.CB_PrincipalID.IsEmpty)
						{
							base.CB_PrincipalID = new ZString("???");
						}
					}
				}
			}
		}

		public bool CB_OH_ShippingLine_ReadOnly => !OverrideFreightDefaults;

		[ReadOnlyMember(nameof(CB_RL_NKPortOfDischarge_ReadOnly))]
		public override ZString CB_RL_NKPortOfDischarge
		{
			get => base.CB_RL_NKPortOfDischarge;
			set
			{
				var oldValue = base.CB_RL_NKPortOfDischarge;
				base.CB_RL_NKPortOfDischarge = value;
				var newValue = CB_RL_NKPortOfDischarge;
				if (!IsCopying && oldValue != newValue)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public bool CB_RL_NKPortOfDischarge_ReadOnly => !OverrideFreightDefaults;

		[ReadOnlyMember(nameof(CB_RL_NKPortOfLoading_ReadOnly))]
		public override ZString CB_RL_NKPortOfLoading
		{
			get => base.CB_RL_NKPortOfLoading;
			set
			{
				var oldValue = base.CB_RL_NKPortOfLoading;
				base.CB_RL_NKPortOfLoading = value;
				var newValue = CB_RL_NKPortOfLoading;
				if (!IsCopying && oldValue != newValue)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public bool CB_RL_NKPortOfLoading_ReadOnly => !OverrideFreightDefaults;

		public void SetVessel(RefVessel vessel)
		{
			CB_VesselName = vessel.RV_Code;
			CB_LloydsIMO = vessel.RV_LloydsNumber;
		}

		public override ZString CB_VesselName
		{
			get
			{
				return base.CB_VesselName;
			}
			set
			{
				base.CB_VesselName = value;
				var lloydsIMO = VesselName?.RV_LloydsNumber;
				if (lloydsIMO.HasValue && CB_LloydsIMO != lloydsIMO.Value)
				{
					CB_LloydsIMO = lloydsIMO.Value;
				}
			}
		}

		public override ZString CB_LloydsIMO
		{
			get { return base.CB_LloydsIMO; }
			set
			{
				base.CB_LloydsIMO = value;
				if (!value.IsEmpty && CB_VesselName.IsEmpty)
				{
					var vessels = RefVessel.LookupVesselsByLloyds(value, Factory);
					if (vessels != null && vessels.Length == 1)
					{
						CB_VesselName = vessels[0].RV_Code;
					}
				}
			}
		}

		public override ZString CB_PrincipalID
		{
			get
			{
				return base.CB_PrincipalID;
			}
			set
			{
				if (value.Length <= CusSCAOceanBill.Schema.CB_PrincipalIDMaxLength)
				{
					base.CB_PrincipalID = value;
				}
				else
				{
					if (base.CB_PrincipalID.IsEmpty)
					{
						base.CB_PrincipalID = new ZString("???");
					}
				}

				OrgCusCode[] aBNsOfShipper = (OrgCusCode[])Factory.Load(typeof(OrgCusCode), new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, value));
				foreach (OrgCusCode aBNOfShipper in aBNsOfShipper)
				{
					if (aBNOfShipper.Header.OH_IsShippingConsortium || aBNOfShipper.Header.OH_IsShippingLine || aBNOfShipper.Header.OH_IsShippingProvider)
					{
						base.CB_OH_ShippingLine = aBNOfShipper.OK_OH;
					}
				}
			}
		}

		public override ZBool CB_IsBureau
		{
			get
			{
				return base.CB_IsBureau;
			}
			set
			{
				base.CB_IsBureau = value;
				foreach (CusUnderbond underbond in AllUnderbonds)
				{
					if (!underbond.AcknowledgedByCustoms)
					{
						underbond.C4_IsBureau = value;
					}
				}
			}
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var bizORL = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				bizORL.AddRange(HouseBills.ToArray(HouseBills.TypeOfElements));
				bizORL.AddRange(Containers.ToArray(Containers.TypeOfElements));
				bizORL.AddRange(Pivots.ToArray());
				bizORL.AddRange(AllUnderbonds.ToArray(AllUnderbonds.TypeOfElements));
				return (BusinessObject[])bizORL.ToArray(typeof(BusinessObject));
			}
		}

		public override void Delete()
		{
			Containers.RemoveAndDeleteAll();
			HouseBills.RemoveAndDeleteAll();
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public bool IsTranshipment
		{
			get
			{
				return !CB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Australia);
			}
		}

		#region OverrideFreightDefaults

		public ZBool OverrideFreightDefaults
		{
			get => Consol == null || CB_OverrideFreightDefaults;
			set => CB_OverrideFreightDefaults = value;
		}

		public ZPropertyInfo OverrideFreightDefaultsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OverrideFreightDefaults), x => CB_OverrideFreightDefaultsInfo); }
		}

		public ZBool OverrideFreightDefaultsVisible => Consol != null;

		public ZBool CB_OverrideFreightDefaults
		{
			get
			{
				if (!cb_OverrideFreightDefaults.HasValue)
				{
					cb_OverrideFreightDefaults = this.GetSystemDefinedValue<ZString>(Schema.CB_OverrideFreightDefaults) == "Y";
				}
				return cb_OverrideFreightDefaults.Value;
			}
			set
			{
				if (value != CB_OverrideFreightDefaults)
				{
					var cancelEventArgs = new CancelEventArgs(false);
					OverrideFreightDefaultsChanging?.Invoke(this, cancelEventArgs);

					if (!cancelEventArgs.Cancel)
					{
						// using a ZString here because a ZBool column is deleted when its false and we need it to exist to register for synch over the data refresh bus
						this.SetSystemDefinedValue(Schema.CB_OverrideFreightDefaults, (ZString)(value ? "Y" : "N"));
						OnOverrideFreightDefaultsUpdated(this, null);
					}
					CB_OverrideFreightDefaultsInfo.RefreshBinding();
				}
			}
		}
		bool? cb_OverrideFreightDefaults;

		public ZPropertyInfo CB_OverrideFreightDefaultsInfo
		{
			get { return GetZPropertyInfo(Schema.CB_OverrideFreightDefaults); }
		}

		public event CancelEventHandler OverrideFreightDefaultsChanging;
		public event EventHandler<ZBool> OverrideFreightDefaultsChanged;

		void OnOverrideFreightDefaultsUpdated(object sender, EventArgs e)
		{
			cb_OverrideFreightDefaults = null;
			OverrideFreightDefaultsChanged?.Invoke(sender, CB_OverrideFreightDefaults);
			RefreshBindingIncludingChildren();
			HouseBills.RefreshBindingIncludingChildren();
		}

		void HookOverrideFreightDefaultsOnUpdated(GenAddOnColumn systemDefinedValue)
		{
			if (systemDefinedValue != null && overrideFreightDefaultsGenAddOnColumn != systemDefinedValue)
			{
				UnhookOverrideFreightDefaultsOnUpdated(overrideFreightDefaultsGenAddOnColumn);

				overrideFreightDefaultsGenAddOnColumn = systemDefinedValue;
				overrideFreightDefaultsGenAddOnColumn.UpdatedByDataRefresh += OnOverrideFreightDefaultsUpdated;
			}
		}

		void UnhookOverrideFreightDefaultsOnUpdated(GenAddOnColumn systemDefinedValue)
		{
			if (systemDefinedValue != null && overrideFreightDefaultsGenAddOnColumn == systemDefinedValue)
			{
				overrideFreightDefaultsGenAddOnColumn.UpdatedByDataRefresh -= OnOverrideFreightDefaultsUpdated;
				overrideFreightDefaultsGenAddOnColumn = null;
			}
		}

		GenAddOnColumn overrideFreightDefaultsGenAddOnColumn;

		public bool CB_DateOfArrival_ReadOnly => !OverrideFreightDefaults;
		public bool CB_DateOfDeparture_ReadOnly => !OverrideFreightDefaults;
		public bool CB_LloydsIMO_ReadOnly => !OverrideFreightDefaults;
		public bool CB_OceanBill_ReadOnly => !OverrideFreightDefaults;
		public bool CB_VesselName_ReadOnly => !OverrideFreightDefaults;
		public bool CB_Voyage_ReadOnly => !OverrideFreightDefaults;

		#endregion

		public BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}
		BusinessObjectFactory fReadOnlyFactory;

		public void UpdateReadOnly()
		{
			OnElementChanged();
		}

		public ZString DischargeCTOID;
		public ZString StevadoreID;
		public ZString BerthCode;

		public ZString CheckCustomsMessagePreconditions()
		{
			ZString result = "";
			if (HasChanges || !IsInDatabase)
			{
				result += "Please save this record before sending messages";
			}
			ZString sendersID = SeaCargoSenderIdRetriever.GetSenderID(Branch, Schema.TableName);
			if (sendersID.IsEmpty)
			{
				result += "Customs Sender ID has not been configured in the registry (Company/Branch)/AUCustoms/Customs Sender ID.\n";
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			using (SuspendMarkingAsNeedingValidation())
			{
				base.SetDefaultValues();
				CB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				CB_GB = GlbBranch.CurrentBranch.PK;
				CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			}
			if (GlbCompany.CurrentCompany.OrgProxy != null && GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber != null && GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number != "")
			{
				CB_ResponsiblePartyID = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Replace(" ", "").Left(CB_ResponsiblePartyIDInfo.MaxLength);
			}
		}

		public new CusSCAOceanBillLookups Lookups
		{
			get { return (CusSCAOceanBillLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSCAOceanBillLookups GetNewLookups()
		{
			return new CusSCAOceanBillLookups(this);
		}

		public new CusSCAOceanBillValidation Validation
		{
			get { return (CusSCAOceanBillValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAOceanBillValidation GetNewValidation()
		{
			return new CusSCAOceanBillValidation(this);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (Consol == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#region Business Objects

		EDIMessageCollection fMessageCollection;
		public EDIMessageCollection MessageCollection
		{
			get
			{
				if (fMessageCollection == null)
				{
					fMessageCollection = new EDIMessageCollection(this, Factory);
					fMessageCollection.Load();
					fMessageCollection.IsManagedForDataRefresh = true;
				}
				return fMessageCollection;
			}
		}

		EDIMessageCollection ICMRMessageRespondee.Messages
		{
			get { return MessageCollection; }
		}

		EDIMessageCollection IManifestProvider.Messages
		{
			get { return MessageCollection; }
		}

		event EventHandler IManifestProvider.CustomsManifestVisibilityChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public ZString Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("OCEAN BILL DETAILS:\r\n");
				if (!CB_OceanBill.IsEmpty)
				{
					builder.Append("Ocean Bill: " + CB_OceanBill + "\r\n");
				}

				if (PortOfLoading != null)
				{
					builder.Append("Load Port: " + PortOfLoading.Code + "\r\n");
				}

				if (PortOfFirstArrival != null)
				{
					builder.Append("Port of First Arrival: " + PortOfFirstArrival.Code + "\r\n");
				}

				if (PortOfDischarge != null)
				{
					builder.Append("Discharge Port: " + PortOfDischarge.Code + "\r\n");
				}

				if (!CB_VesselName.IsEmpty)
				{
					builder.Append("Vessel: " + CB_VesselName + "\r\n");
				}

				if (!CB_Voyage.IsEmpty)
				{
					builder.Append("Voyage: " + CB_Voyage + "\r\n");
				}

				return builder.ToString();
			}
		}

		public ZString ShortDescription
		{
			get { return CB_OceanBill.IsEmpty ? ZString.Empty : new ZString("Ocean Bill: " + CB_OceanBill); }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		protected EDIMessageCollection fMessages;

		#endregion

		#region Lists

		public RefUNLOCOCollection PortOfLoadingList
		{
			get
			{
				RefUNLOCOCollection result = new RefUNLOCOCollection(Factory);
				return result;
			}
		}

		public RefUNLOCOCollection PortOfDischargeList
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public RefCountryCollection CountriesWithSeaPorts
		{
			get
			{
				if (fCountriesWithSeaPorts == null)
				{
					ZDBOnlyQuery countriesQuery = new ZDBOnlyQuery(typeof(RefCountry));
					ZDBOnlySubQuery countriesSubQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_RN_NKCountryCode);
					countriesSubQuery.AddToFilter(RefUNLOCOSchema.RL_HasSeaport, ZBool.True);
					countriesQuery.AddSubQuery(RefCountrySchema.RN_Code, countriesSubQuery, JoinCondition.And);

					fCountriesWithSeaPorts = new RefCountryCollection(Factory);
					fCountriesWithSeaPorts.AdditionalFilter = countriesQuery;
				}
				return fCountriesWithSeaPorts;
			}
		}
		protected RefCountryCollection fCountriesWithSeaPorts;

		#endregion

		#region IManifestProvider

		public IManifestProvider ManifestProvider
		{
			get { return this; }
		}

		#endregion

		#region ISeaCargoConsolInfo

		BusinessObject ISeaCargoConsolInfo.TopLevelObject
		{
			get { return this; }
		}

		public bool IsVisible
		{
			get { return true; }
		}

		public ZGlobalMutex Mutex
		{
			get { return fMutext ?? (fMutext = new ZGlobalMutex(CusSCAOceanBillSendSEAMutex.Instance, PK.ToString())); }
		}
		ZGlobalMutex fMutext;

		public void UnlockMutexIfNeeded() { }

		public CusSCAOceanBill OceanBill
		{
			get { return this; }
		}

		public SeaCargoSynchroniser SeaCargoSynchroniser
		{
			get { return null; }
		}

		bool ISeaCargoConsolInfo.CanSynchronise
		{
			get { return false; }
		}

		bool ISeaCargoConsolInfo.NoSynchronisingWillOccur
		{
			get { return false; }
		}

		ZString ISeaCargoConsolInfo.SynchroniseFailureMessage
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IMessageManageableBizObj Members

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusSCAOceanBillMessageManager(this);
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return CB_OceanBill; }
		}

		#endregion

		#region IWorkflowProviderCore

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusSCAOceanBillProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>(this);
		}

		protected override IColumnValueRanker GetTemplateSelectionCriteriaCore()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_GB, CB_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, CB_OH_ShippingLine, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, CB_RL_NKPortOfDischarge, CB_RL_NKPortOfDischarge.Left(2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, CB_RL_NKPortOfLoading, CB_RL_NKPortOfLoading.Left(2), ZString.Empty);
			return result;
		}

		#endregion // IWorkflowProviderCore

		#region IWorkflowTriggerEventSource

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				var branch = Branch;
				return branch != null ? branch.Company : null;
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var consol = Consol;
				return consol != null
					? new IWorkflowProviderCore[] { consol }
					: Array.Empty<IWorkflowProviderCore>();
			}
		}

		#endregion // IWorkflowTriggerEventSource

		#region ICustomFieldProvider
		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}

		CustomBusinessObject customBusinessObject;

		protected void ResetCustomBusinessObject()
		{
			customBusinessObject = null;
			OnResetCustomBusinessObject?.Invoke();
		}

		public delegate void OnResetCustomBusinessObjecDelegate();
		public OnResetCustomBusinessObjecDelegate OnResetCustomBusinessObject;
		#endregion

		#region BulkAllocateReferenceNumbersForChildBills

		public void BulkAllocateReferenceNumbersForChildBills()
		{
			var query = new ZQuery(CusSCAHouseSchema.CA_CB, PK);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var childBills = Factory.Load<CusSCAHouse>(query).Where(x =>
					!x.IsDeleted && !x.IsDeleting && x.CA_BGMReference.IsEmpty &&
					x.Shipment == null)
				.ToArray();
			if (childBills.Length > 0)
			{
				var messageReferences = Env.NumberFountains.CusSCAHouseNumber.GetNextsFormatted(Factory, childBills.Length);
				for (int index = 0; index < messageReferences.Length; index++)
				{
					childBills[index].CA_BGMReference = messageReferences[index];
				}
			}
		}

		#endregion

		#region Scheduled messages 

		public string ScheduledMessagesConfirmationText => CargoHelper.ScheduledMessagesConfirmationText(DeferredScheduledMessagesDateTimeString, "Original Sea Cargo Reports");
		public ZString DeferredScheduledMessagesDateTimeString => CargoHelper.DeferredScheduledMessagesDateTimeString(DeferredScheduledMessagesDateTime);

		const string CanberraUNLOCO = "AUCBR";

		public ZDateTime DeferredScheduledMessagesDateTime => CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Sea, CB_RL_NKPortOfDischarge, CB_DateOfArrival);

		LogsForNominatedEvent AllDeferredScheduledMessageLogs
		{
			get { return allDeferredScheduledMessages ?? (allDeferredScheduledMessages = new LogsForNominatedEvent(this.GetLogs(), Events.DeferredScheduledMessage)); }
		}
		LogsForNominatedEvent allDeferredScheduledMessages;

		public bool HasDeferredScheduledMessageLog
		{
			get { return AllDeferredScheduledMessageLogs.Count > 0; }
		}

		public ZString DeferredScheduledDateForDisplayInCanberraTime
		{
			get
			{
				return HasDeferredScheduledMessageLog ? string.Format("Messaging deferred until {0}",
					EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(CanberraUNLOCO, AllDeferredScheduledMessageLogs[0].SL_EventTime.ToDateTime()).ToString("dd-MMM-yyyy HH:mm"))
					: string.Empty;
			}
		}

		public ZPropertyInfo DeferredScheduledDateForDisplayInCanberraTimeInfo
		{
			get { return GetZPropertyInfo(nameof(DeferredScheduledDateForDisplayInCanberraTime)); }
		}

		public void CancelDeferredScheduledMessageLogs()
		{
			AllDeferredScheduledMessageLogs.CancelAll();
			if (DeferredScheduledMessagesEventChanged != null)
			{
				DeferredScheduledMessagesEventChanged();
			}
		}

		public void AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent()
		{
			AddDeferredScheduledMessagesEvent();
			if (DeferredScheduledMessagesEventChanged != null)
			{
				DeferredScheduledMessagesEventChanged();
			}
		}

		protected void AddDeferredScheduledMessagesEvent()
		{
			CancelDeferredScheduledMessageLogs();
			var deferredScheduledMessagesDateTime = DeferredScheduledMessagesDateTime;
			var newLog = AllDeferredScheduledMessageLogs.AddNew("SEACR", deferredScheduledMessagesDateTime.IsEmpty ? ZDateTimeOffset.UtcNow :
					EnvProxy.Instance.Time.GetUtcFromUnlocoTime(CanberraUNLOCO, deferredScheduledMessagesDateTime.ToDateTime()));
			using (((IUpdateFieldsLock)newLog).LockForUpdatingKeyFields())
			{
				newLog.SL_IsEstimate = true;
			}
		}

		public event Action DeferredScheduledMessagesEventChanged;

		public override void OnSaving()
		{
			base.OnSaving();
			var deferredScheduledMessageTimeMayHaveChanged =
				(ZDateTime)CB_DateOfArrivalInfo.OriginalValue != CB_DateOfArrival ||
				(ZString)CB_RL_NKPortOfDischargeInfo.OriginalValue != CB_RL_NKPortOfDischarge;
			if (deferredScheduledMessageTimeMayHaveChanged && HasDeferredScheduledMessageLog)
			{
				AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
			}
		}

		public ZGlobalMutex SendSEACRMutex
		{
			get { return sendSEACRMutex ?? (sendSEACRMutex = new ZGlobalMutex(CusSCAOceanBillSendSEACRMutex.Instance, PK.ToString())); }
		}
		ZGlobalMutex sendSEACRMutex;

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		[ChildEditable]
		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollectionForSeaCargo(this);
					RegisterEditableChildObject(allUnderbonds);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		internal CusUnderbondUnionCollection GetAllUnderbondsIfAlreadyLoaded() => allUnderbonds;

		ICusUnderbondDependentCollectionParent[] ICusUnderbondUnionCollectionParent.GetAllPossibleCollectionProviders()
		{
			ArrayList result = new ArrayList();
			result.AddRange(Containers);
			result.AddRange(Pivots);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region IConsignmentKeyChangeInhibitor Members

		bool IConsignmentKeyChangeInhibitor.ShouldStopKeyFieldsChange
		{
			get { return ShouldStopKeyFieldsChange; }
		}

		bool ShouldStopKeyFieldsChange
		{
			get
			{
				if (shouldStopKeyFieldsChange == null)
				{
					shouldStopKeyFieldsChange = false;

					foreach (CusSCAHouse housebill in HouseBills)
					{
						if (!CMRStatusHelper.IsAcceptableStatusesForKeyValueChange(Factory, housebill.CA_MessageStatus))
						{
							shouldStopKeyFieldsChange = true;
							break;
						}
					}
				}
				return shouldStopKeyFieldsChange.Value;
			}
		}
		bool? shouldStopKeyFieldsChange;

		internal void RefreshShouldStopKeyFieldsChangeCalculation()
		{
			shouldStopKeyFieldsChange = null;
		}

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference
		{
			get { return this; }
		}

		IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var house in HouseBills)
			{
				yield return house;
			}
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		protected override DocManagerInfo GetDocManagerInfo() => new DocManagerIncludingRelatedObjectsInfo(this, Core.Constants.DocManagerCodes.SCAOceanBill);

		#endregion

		#region IScanMasterBillProvider

		ZString IScanMasterBillProvider.MasterBill
		{
			get { return CB_OceanBill; }
		}

		ZBool IScanMasterBillProvider.IsStandAlone
		{
			get { return IsStandAlone; }
		}

		ZBool IsStandAlone
		{
			get { return Consol == null; }
		}

		ZString IScanMasterBillProvider.MasterHouseBill
		{
			get { return CB_MasterHouseBill; }
		}

		IEnumerable<CusUnderbond> IScanMasterBillProvider.Underbonds
		{
			get
			{
				var containerNumbers = Containers.Cast<CusSCAContainer>().Select(x => x.CN_ContainerNumber);
				foreach (CusUnderbond underbond in GetDCLContainerUnderbond(containerNumbers))
				{
					yield return underbond;
				}
				if (IsStandAlone)
				{
					foreach (var consolOceanBill in GetMatchingOceanBillsForContainers(false, containerNumbers.ToArray()))
					{
						foreach (var underbond in consolOceanBill.GetDCLContainerUnderbond(containerNumbers))
						{
							yield return underbond;
						}
					}
				}
			}
		}

		IEnumerable<CusUnderbond> GetDCLContainerUnderbond(IEnumerable<ZString> containerNumbers)
		{
			foreach (CusUnderbond underbond in AllUnderbonds)
			{
				if (underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination && underbond.ContainerLinked != null
					&& containerNumbers.Contains(underbond.ContainerLinked.CN_ContainerNumber))
				{
					yield return underbond;
				}
			}
		}

		IEnumerable<IScanHouseBillProvider> IScanMasterBillProvider.GetChildBills(CusUnderbond underbond)
		{
			var containerNo = underbond != null ? underbond.ContainerNumber : ZString.Empty;
			if (!containerNo.IsEmpty)
			{
				return HouseBills.Cast<CusSCAHouse>().Where(x => !x.CA_IsMasterHouse && x.Pivot.Cast<CusSCAPivot>().Select(pivot => pivot.Container).Cast<CusSCAContainer>().Any(y => y.CN_ContainerNumber == containerNo));
			}
			return Enumerable.Empty<IScanHouseBillProvider>();
		}

		internal IEnumerable<CusSCAOceanBill> GetMatchingOceanBillsForContainers(ZBool standAlone, ZString[] containerNumbers)
		{
			var sanitizedContainerNumbers = containerNumbers.Where(x => !string.IsNullOrWhiteSpace(x));
			if (!CB_OceanBill.IsEmpty && sanitizedContainerNumbers.Any())
			{
				var containerQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAContainerSchema.CN_CB);
				containerQuery.AddToFilter(CusSCAContainerSchema.CN_ContainerNumber, sanitizedContainerNumbers);
				var query = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
				query.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, CB_OceanBill);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentId, standAlone ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, DBNull.Value);
				query.AddSubQuery(containerQuery, JoinCondition.And);
				return Factory.Load<CusSCAOceanBill>(query);
			}
			return Enumerable.Empty<CusSCAOceanBill>();
		}

		internal CusSCAOceanBill GetStandAloneOceanBill(ZString houseBill)
		{
			if (!CB_OceanBill.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
				query.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, CB_OceanBill);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ParentId, DBNull.Value);
				query.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, houseBill);
				var standAloneOceanBill = Factory.Load<CusSCAOceanBill>(query);
				if (standAloneOceanBill.Length > 1)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There are more than one stand alone sea cargo ocean bill for house bill '{0}'", houseBill));
				}

				if (standAloneOceanBill.Length == 0)
				{
					return null;
				}

				return standAloneOceanBill[0];
			}
			else
			{
				return null;
			}
		}

		void IScanMasterBillProvider.CreateSurplusConsignment(BusinessObjectFactory factory, OutturnLine outturn)
		{
			var container = Containers.Cast<CusSCAContainer>().FirstOrDefault(x => x.CN_ContainerNumber == outturn.Underbond.ContainerNumber);
			if (container != null)
			{
				var house = factory.New<CusSCAHouse>();
				HouseBills.Add(house);
				house.CA_HouseBill = outturn.ConsignmentRef;
				var pivot = house.Pivot.AddNew();
				pivot.CV_CN = container.PK;
				pivot.CV_GoodsDescription = "SURPLUS GOODS";
				pivot.CV_MarksAndNumbers = "SURPLUS GOODS";
				pivot.CV_PackageCount = outturn.Count;
				pivot.CV_PackageType = CMRPackageTypes.Codes.Package;
				pivot.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
				outturn.HouseBill = house;
				outturn.ManifestInfo = pivot;
			}
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerType)
		{
			return this;
		}

		#endregion

		#region ITriggerActionMessagingSupporterProvider

		ITriggerActionMessagingSupporter ITriggerActionMessagingSupporterProvider.GetSupporter(string triggerType)
		{
			return this;
		}

		#endregion

		#region ITriggerActionMessagingSupporter

		void ITriggerActionMessagingSupporter.SendMessage(INotifications notifications, ZString queuedUserNK, ZString triggerAction)
		{
			using (var processorJob = new SeaCargoProcessorJob(this))
			{
				new CargoMessagingTriggerActionProcessor(Env.Registry.RawRegistry.SeaCargoSendErrorsToGroup).Process(processorJob, new NotificationBuffer(), queuedUserNK, new LoggerNotificationsWrapper(notifications));
			}

			AddDeferredScheduledMessagesEvent();
		}

		class LoggerNotificationsWrapper : ILogger
		{
			public LoggerNotificationsWrapper(INotifications notifications)
			{
				this.notifications = notifications;
			}

			readonly CargoWise.ComponentModel.NotificationType informationNotificationType = new CargoWise.ComponentModel.NotificationType(0, false);
			readonly INotifications notifications;

			public void Log(LogType type, string message)
			{
				switch (type)
				{
					case LogType.Warning:
						notifications.AddWarning(message);
						break;
					case LogType.Error:
						notifications.AddError(message);
						break;
					default:
						notifications.Add(new Notification(informationNotificationType, message));
						break;
				}
			}

			public void Log(LogType type, string message, Exception ex) => Log(type, message);
		}

		#endregion

		#region Mutex

		class CusSCAOceanBillSendSEAMutex : MutexID
		{
			public static CusSCAOceanBillSendSEAMutex Instance { get; } = new CusSCAOceanBillSendSEAMutex();

			CusSCAOceanBillSendSEAMutex()
				: base("CusSCAOceanBillSendSEAMutex", "CusSCAOceanBill SEACR Message Sending Lock.")
			{ }
		}

		#endregion

		#region IControllerIDProvider

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.AU.SeaCargoStandAloneController; }
		}

		#endregion
	}
}
