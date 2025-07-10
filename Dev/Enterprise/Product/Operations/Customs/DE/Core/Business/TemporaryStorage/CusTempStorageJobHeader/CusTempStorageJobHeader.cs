using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader>;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageJobHeader : EU.Business.CusTempStorage.CusTempStorageJobHeader,
		Integration.Customs.DE.ICusTempStorageJobHeader
	{
		public CusTempStorageJobHeader(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.CusTempStorage.CusTempStorageJobHeader.Schema
		{
			public const string MostRecentlyModifiedDeclarationType = nameof(CusTempStorageJobHeader.MostRecentlyModifiedDeclarationType);
			public const string MostRecentlyModifiedDeclarationMessageStatus = nameof(CusTempStorageJobHeader.MostRecentlyModifiedDeclarationMessageStatus);
			public const string MostRecentlyModifiedDeclarationRegistrationNumber = nameof(CusTempStorageJobHeader.MostRecentlyModifiedDeclarationRegistrationNumber);
		}

		#region Properties

		public ZBool IsReExport => SJH_AppCode == TemporaryStorageApplicationCodeList.Codes.REX;

		public bool IsAir => SJH_TransportMode == TransportTypeList.Codes.Air;

		public bool IsSea => SJH_TransportMode == TransportTypeList.Codes.Sea;

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[MaxLength(17)]
		[ResourceStringData("4d0d0169-d5a5-459e-9177-a0861184f742", Caption = "Border Transport Information", ShortCaption = "Border Transport Info.")]
		public override ZString SJH_TransportMeansDescription { get => base.SJH_TransportMeansDescription; set => base.SJH_TransportMeansDescription = value; }

		[MaxLength(4)]
		[ReadOnlyMember(nameof(ContainerNotSupported))]
		public override ZInt SJH_ContainerCount { get => base.SJH_ContainerCount; set => base.SJH_ContainerCount = value; }

		[ResourceStringData("2D10C5F6-BEAE-4270-8990-6E25FCC2DFD7", Caption = "Customs Office")]
		[ReadOnlyMember(nameof(ATIssueDateSet))]
		public override ZString SJH_CustomsOffice { get => base.SJH_CustomsOffice; set => base.SJH_CustomsOffice = value; }

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[ResourceStringData("7DB55DC9-635E-4A80-B39C-9B935CC1A605", Caption = "Transport Mode")]
		public override ZString SJH_TransportMode
		{
			get => base.SJH_TransportMode;
			set
			{
				var oldValue = base.SJH_TransportMode;

				if (oldValue != value)
				{
					base.SJH_TransportMode = value;
				}
			}
		}

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[MaxLength(30)]
		public override ZString SJH_TransportRegNo
		{
			get => base.SJH_TransportRegNo;
			set
			{
				var oldValue = base.SJH_TransportRegNo;

				if (oldValue != value)
				{
					base.SJH_TransportRegNo = value;
				}
			}
		}

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[MaxLength(30)]
		[ResourceStringData("30E57326-F488-4BD3-8ACA-75607A65F857", Caption = "Previous Ref. Number")]
		public override ZString SJH_PreviousReferenceNumber { get => base.SJH_PreviousReferenceNumber; set => base.SJH_PreviousReferenceNumber = value; }

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		public override ZString SJH_PreviousReferenceType { get => base.SJH_PreviousReferenceType; set => base.SJH_PreviousReferenceType = value; }

		[ResourceStringData("E5C0E5FF-8D8F-43FA-BF99-60A2B0BC6100", Caption = "Departure Date")]
		public override ZDate SJH_DepartureDate { get => base.SJH_DepartureDate; set => base.SJH_DepartureDate = value; }

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[ResourceStringData("24A34EDB-EC29-4120-8C49-81545BB64D84", Caption = "Arrival Date")]
		public override ZDate SJH_ArrivalDate { get => base.SJH_ArrivalDate; set => base.SJH_ArrivalDate = value; }

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		public override ZDateTime SJH_PresentationDate { get => base.SJH_PresentationDate; set => base.SJH_PresentationDate = value; }

		[ResourceStringData("E99A96FC-4B68-4C10-8CFF-144D5A718AB8", Caption = "Additional Information")]
		public override ZString SJH_AdditionalInformation { get => base.SJH_AdditionalInformation; set => base.SJH_AdditionalInformation = value; }

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		public override ZString SJH_RL_NKLoading { get => base.SJH_RL_NKLoading; set => base.SJH_RL_NKLoading = value; }

		[ResourceStringData("e90dc6ff-2f11-4a4f-aded-2d255a04aac5", Caption = "Customer")]
		public override ZGuid SJH_OH_Customer
		{
			get => base.SJH_OH_Customer;
			set => base.SJH_OH_Customer = value;
		}

		[ReadOnlyMember(nameof(ATIssueDateSet))]
		[ResourceStringData("EC78A07D-F139-4A6C-8249-EB4C74D3429C", Caption = "Presenter")]
		public override ZGuid SJH_OA_Presenter
		{
			get => base.SJH_OA_Presenter;
			set
			{
				var oldValue = base.SJH_OA_Presenter;
				base.SJH_OA_Presenter = value;
				if (!IsCopying && oldValue != base.SJH_OA_Presenter && SJH_NCTSFlag && NCTSFlagNotSupported)
				{
					SJH_NCTSFlag = false;
				}
			}
		}

		public ZString PresenterEoriNumber => Presenter?.Header.GetEUEoriDetails() ?? ZString.Empty;

		public ZString PresenterEoriBranch => Presenter.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[ResourceStringData("AEF6BFD8-8939-484C-A9CC-7BD131592A97", Caption = "Representative")]
		public override ZGuid SJH_OA_Representative { get => base.SJH_OA_Representative; set => base.SJH_OA_Representative = value; }

		public ZString RepresentativeEoriNumber => Representative?.Header.GetEUEoriDetails() ?? ZString.Empty;

		public ZString RepresentativeEoriBranch => Representative.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);

		[ReadOnlyMember(nameof(NCTSFlagNotSupported))]
		public override ZBool SJH_NCTSFlag { get => base.SJH_NCTSFlag; set => base.SJH_NCTSFlag = value; }

		public bool NCTSFlagNotSupported
		{
			get
			{
				var result = true;
				var presenter = Presenter?.Header;
				if (presenter != null)
				{
					result = CusAuthorisationHeader.Loader.GetAuthorisationNumber(Factory, Core.Constants.CountryCodes.Germany, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, ZDate.Today, presenter.PK).IsEmpty;
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(RegHeaderWithATNumberExists))]
		[ResourceStringData("8365F08D-16CF-4031-BE3E-1FF82D87EEC4", Caption = "Transport Means")]
		public override ZString SJH_TransportMeansCode
		{
			get => base.SJH_TransportMeansCode;
			set
			{
				if (SJH_TransportMeansCode != value)
				{
					base.SJH_TransportMeansCode = value;
					if (!IsCopying)
					{
						DefaultContainerCountIfNeeded();
						if (SJH_TransportMeansCode == TemporaryStorageTransportMeansList.Codes.Aircraft)
						{
							SJH_TransportMode = TransportTypeList.Codes.Air;
						}
					}
				}
			}
		}

		public void SetDefaultValuesForSumA()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy != null)
			{
				SJH_OH_Customer = orgProxy.PK;
				SJH_OA_Presenter = orgProxy.MainAddress.PK;
			}
			SJH_PresentationDate = ZDateTime.Now;
			SJH_PreviousReferenceType = PreviousReferenceType.Codes._OHNE;
		}

		public override ZString SJH_JobReference
		{
			get => base.SJH_JobReference;
			set
			{
				var oldValue = base.SJH_JobReference;
				base.SJH_JobReference = value;
				if (!IsValidationSuspended && oldValue != base.SJH_JobReference)
				{
					Validation.ValidateSJH_ReferenceNumber();
				}
			}
		}

		void DefaultContainerCountIfNeeded()
		{
			if (ContainerNotSupported)
			{
				SJH_ContainerCount = 0;
			}
		}

		bool ContainerNotSupported => SJH_TransportMeansCode != TemporaryStorageTransportMeansList.Codes.Truck &&
		SJH_TransportMeansCode != TemporaryStorageTransportMeansList.Codes.Vessel &&
		SJH_TransportMeansCode != TemporaryStorageTransportMeansList.Codes.Wagon;

		bool ATIssueDateSet => !CUSPRLCusTempStorageDec?.CusEntryNumber.CE_IssueDate.IsEmpty ?? false;

		bool RegHeaderWithATNumberExists
		{
			get
			{
				var result = false;
				var atbNumber = CUSPRLCusTempStorageDec?.CusEntryNumber.CE_EntryNum ?? ZString.Empty;
				if (!atbNumber.IsEmpty)
				{
					result = Factory.Exists(typeof(CusTempStorageRegHeader), CusTempStorageRegHeader.GetLoadQuery(atbNumber));
				}
				return result;
			}
		}

		#endregion

		#region CusTempStorageDecsWithValidData

		public CusTempStorageDecCollectionView CusTempStorageDecsWithValidData => cusTempStorageDecsWithValidData ?? (cusTempStorageDecsWithValidData = new CusTempStorageDecCollectionView(CusTempStorageDecs));

		CusTempStorageDecCollectionView cusTempStorageDecsWithValidData;

		#endregion

		#region CusTempStorageDecs

		public new CusTempStorageDecCollection CusTempStorageDecs => (CusTempStorageDecCollection)base.CusTempStorageDecs;

		protected override EU.Business.CusTempStorage.CusTempStorageDecCollection CreateNewCusTempStorageDecs() => new CusTempStorageDecCollection(this);

		CusTempStorageDec MostRecentlyModifiedDeclaration =>
			Factory.GetValue(ref mostRecentlyModifiedDeclaration,
				() => CusTempStorageDecs.Cast<CusTempStorageDec>()
					.OrderByDescending(x => x.STH_SystemLastEditTimeUtc)
					.FirstOrDefault());
		CachedProperty<CusTempStorageDec> mostRecentlyModifiedDeclaration;

		public ZString MostRecentlyModifiedDeclarationType => MostRecentlyModifiedDeclaration?.STH_DeclarationType ?? ZString.Empty;

		public ZString MostRecentlyModifiedDeclarationMessageStatus => MostRecentlyModifiedDeclaration?.STH_MessageStatus ?? ZString.Empty;

		public ZString MostRecentlyModifiedDeclarationRegistrationNumber => MostRecentlyModifiedDeclaration?.CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
		#endregion

		#region REXDISTempStorageDec

		public REXDISCusTempStorageDec REXDISCusTempStorageDec
		{
			get
			{
				if (rexCusTempStorageDec == null)
				{
					rexCusTempStorageDec = REXDISCusTempStorageDec.Load(this);
					RegisterEditableChildObject(rexCusTempStorageDec);
				}
				return rexCusTempStorageDec;
			}
		}
		REXDISCusTempStorageDec rexCusTempStorageDec;

		#endregion

		#region CUSPRLTempStorageDec

		public CUSPRLCusTempStorageDec CUSPRLCusTempStorageDec
		{
			get
			{
				if (fCUSPRLCusTempStorageDec == null)
				{
					fCUSPRLCusTempStorageDec = CUSPRLCusTempStorageDec.Load(this);
					RegisterEditableChildObject(fCUSPRLCusTempStorageDec);
				}
				return fCUSPRLCusTempStorageDec;
			}
		}
		CUSPRLCusTempStorageDec fCUSPRLCusTempStorageDec;

		#endregion

		#region CUSPCSTempStorageDecs

		[ChildEditable(true)]
		public CUSPCSCusTempStorageDecCollection CUSPCSCusTempStorageDecs
		{
			get
			{
				if (fCUSPCSCusTempStorageDecs == null)
				{
					fCUSPCSCusTempStorageDecs = new CUSPCSCusTempStorageDecCollection(this);
					fCUSPCSCusTempStorageDecs.Load();
					RegisterEditableChildObject(fCUSPCSCusTempStorageDecs);
				}
				return fCUSPCSCusTempStorageDecs;
			}
		}
		CUSPCSCusTempStorageDecCollection fCUSPCSCusTempStorageDecs;

		#endregion

		#region CHGSPOCusTempStorageDecs

		[ChildEditable(true)]
		public CHGSPOCusTempStorageDecCollection CHGSPOCusTempStorageDecs
		{
			get
			{
				if (fCHGSPOCusTempStorageDecs == null)
				{
					fCHGSPOCusTempStorageDecs = new CHGSPOCusTempStorageDecCollection(this);
					fCHGSPOCusTempStorageDecs.Load();
					RegisterEditableChildObject(fCHGSPOCusTempStorageDecs);
				}
				return fCHGSPOCusTempStorageDecs;
			}
		}
		CHGSPOCusTempStorageDecCollection fCHGSPOCusTempStorageDecs;

		#endregion

		#region CHGTSTCusTempStorageDecs

		[ChildEditable(true)]
		public CHGTSTCusTempStorageDecCollection CHGTSTCusTempStorageDecs
		{
			get
			{
				if (fCHGTSTCusTempStorageDecs == null)
				{
					fCHGTSTCusTempStorageDecs = new CHGTSTCusTempStorageDecCollection(this);
					fCHGTSTCusTempStorageDecs.Load();
					RegisterEditableChildObject(fCHGTSTCusTempStorageDecs);
				}
				return fCHGTSTCusTempStorageDecs;
			}
		}
		CHGTSTCusTempStorageDecCollection fCHGTSTCusTempStorageDecs;

		#endregion

		#region CHGOFFCusTempStorageDecs

		[ChildEditable(true)]
		public CHGOFFCusTempStorageDecCollection CHGOFFCusTempStorageDecs
		{
			get
			{
				if (fCHGOFFCusTempStorageDecs == null)
				{
					fCHGOFFCusTempStorageDecs = new CHGOFFCusTempStorageDecCollection(this);
					fCHGOFFCusTempStorageDecs.Load();
					RegisterEditableChildObject(fCHGOFFCusTempStorageDecs);
				}
				return fCHGOFFCusTempStorageDecs;
			}
		}
		CHGOFFCusTempStorageDecCollection fCHGOFFCusTempStorageDecs;

		#endregion

		#region PRLCONCusTempStorageDecs

		[ChildEditable(true)]
		public PRLCONCusTempStorageDecCollection PRLCONCusTempStorageDecs
		{
			get
			{
				if (fPRLCONCusTempStorageDecs == null)
				{
					fPRLCONCusTempStorageDecs = new PRLCONCusTempStorageDecCollection(this);
					fPRLCONCusTempStorageDecs.Load();
					RegisterEditableChildObject(fPRLCONCusTempStorageDecs);
				}
				return fPRLCONCusTempStorageDecs;
			}
		}

		PRLCONCusTempStorageDecCollection fPRLCONCusTempStorageDecs;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderValidation GetNewValidation()
		{
			if (IsReExport)
			{
				return new REXDISCusTempStorageJobHeaderValidation(this);
			}
			else
			{
				return new SumACusTempStorageJobHeaderValidation(this);
			}
		}

		public new CusTempStorageJobHeaderValidation Validation => (CusTempStorageJobHeaderValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups GetNewLookups() => new CusTempStorageJobHeaderLookups(this);

		public new CusTempStorageJobHeaderLookups Lookups => (CusTempStorageJobHeaderLookups)base.Lookups;

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.SumA;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateFormattedNumberPropertyIfRequired(SJH_JobReferenceInfo, Env.NumberFountains.DETempStorageJobReference);
			CusTempStorageDecs.ReloadFromLocalCache();
		}

		protected override ZString HumanReadableNameCore => SJH_JobReference;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Message sending message error validation

		public NotificationCollection GetHeaderAndTempStorageDecMessageErrors(CusTempStorageDec dec)
		{
			var notificationCollection = new NotificationCollection();

			foreach (var notification in new CustomsNotificationCollector(this, false, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName))
			{
				AddMessageErrorToCollection(notification);
			}

			foreach (var notification in new CustomsNotificationCollector(dec, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName))
			{
				AddMessageErrorToCollection(notification);
			}
			return notificationCollection;

			void AddMessageErrorToCollection(INotification notification)
			{
				var notificationMessage = notification.Message;
				if (notification.Type == CargoWise.EntityFramework.NotificationType.MessageError && !notificationCollection.Contains(notificationMessage))
				{
					notificationCollection.Add(notification);
				}
			}
		}

		#endregion
	}
}
