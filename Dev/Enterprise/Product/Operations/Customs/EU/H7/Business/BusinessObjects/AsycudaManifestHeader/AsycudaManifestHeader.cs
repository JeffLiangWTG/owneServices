using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	[TestExcludeWorkflowProviderHasTestCase]
	public class AsycudaManifestHeader :
		ASYCUDA.Business.AsycudaManifestHeader,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		Integration.Customs.ICusCodeDataTypeSupporter,
		IDataGroupingProvider,
		Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader,
		IRelatedJob,
		IWorkflowProvider,
		IValidateForCustomsMessagingSupporter,
		ITemplateCopyable
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaManifestHeaderTypeDecider TypeDecider = new AsycudaManifestHeaderTypeDecider();

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ApplicationCode = DefaultApplicationCode;
			AMA_ManifestType = EUH7ManifestTypes.Codes.EH7;

			if (Branch?.OrgProxy?.MainAddress is OrgAddress proxyMainAddress)
			{
				AMA_OA_Declarant = proxyMainAddress.PK;
			}
		}

		protected virtual string DefaultApplicationCode => ManifestBase.ApplicationCodeTypeList.Codes.EuH7;

		public override void OnSaving()
		{
			if (PresentationOfficeCode != null && PresentationOfficeCode.CY_Data.IsEmpty)
			{
				PresentationOfficeCode.Delete();
			}

			base.OnSaving();
		}

		protected override ZString GetDefaultCountryCode() => Env.CurrentCompany.Country.Code;

		public override ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var numberGeneratorTarget = new EUH7JobNumberGeneratorTarget();
			var generator = new NumberGenerator
			{
				Factory = factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.EUH7ManifestJobReference,
				FountainGetter = Env.NumberFountains.GetManifestJobReferenceGeneratorFountain,
				PrimaryTarget = numberGeneratorTarget
			};
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return numberGeneratorTarget.Value.ToUpper();
		}

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public ValidationConfiguration ValidationConfiguration => validationConfiguration ??= GetNewValidationConfiguration();
		ValidationConfiguration validationConfiguration;

		protected virtual ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

		public override bool HasContainers => false;

		public override ZBool SupportMultipleCustomsNumbers => ZBool.True;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;
		public new IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		protected sealed override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => CreateNewEUH7AsycudaBillCollection();
		protected virtual IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new H7ApplicationBusinessProvider ApplicationBusinessProvider => base.ApplicationBusinessProvider as H7ApplicationBusinessProvider;

		protected override bool NeedPersonsTabCore => false;

		#region Properties

		[MaxLength(Schema.AMA_CustomsOfficeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsOffices))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.PresentationOffice", Caption = "Presentation Office", MediumCaption = "Pres. Office", ShortCaption = "Pres. Off.", FullDescription = "Code identifying the Customs Office of goods presentation.")]
		public ZString PresentationOffice
		{
			get => PresentationOfficeCode?.CY_Data ?? ZString.Empty;
			set
			{
				var currentOfficeCode = PresentationOfficeCode;
				var oldValue = currentOfficeCode?.CY_Data ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentOfficeCode == null)
					{
						currentOfficeCode = LoadPresentationOfficeCode(createIfMissing: true);
					}

					currentOfficeCode.CY_Data = value;
					PresentationOfficeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo PresentationOfficeInfo => GetZPropertyInfo(nameof(PresentationOffice));

		public PresentationOfficeCode PresentationOfficeCode
		{
			get
			{
				if (presentationOfficeCode == null || presentationOfficeCode.IsDeleted)
				{
					LoadPresentationOfficeCode(createIfMissing: false);
				}

				return presentationOfficeCode;
			}
		}
		PresentationOfficeCode presentationOfficeCode;

		PresentationOfficeCode LoadPresentationOfficeCode(bool createIfMissing)
		{
			presentationOfficeCode = createIfMissing
				? EuOfficeCode.LoadOrCreate<PresentationOfficeCode>(this, EuOfficeCodesTypes.Codes.OfficeOfPresentation)
				: EuOfficeCode.Load<PresentationOfficeCode>(this, EuOfficeCodesTypes.Codes.OfficeOfPresentation);

			if (presentationOfficeCode != null)
			{
				RegisterEditableChildObject(presentationOfficeCode);
				presentationOfficeCode.CY_DataInfo.ValueChanged += delegate
				{ MarkAsNeedingValidation(); };
			}

			return presentationOfficeCode;
		}

		public virtual string RepresentativeStatusCode
		{
			get
			{
				switch (AMA_AgentType)
				{
					case EUH7AgentTypes.Codes.DIR:
						return DirectRepresentaton;
					case EUH7AgentTypes.Codes.IND:
						return IndirectRepresentation;
					default:
						return string.Empty;
				}
			}
		}

		const string DirectRepresentaton = "2";
		const string IndirectRepresentation = "3";

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Branch", Caption = "Branch", MediumCaption = "Branch", ShortCaption = "Br.", FullDescription = "Company branch code associated with the declaration.")]
		public override ZGuid AMA_GB { get => base.AMA_GB; set => base.AMA_GB = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DeclarantList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Declarant", Caption = "Declarant", MediumCaption = "Declarant", ShortCaption = "Decl.", FullDescription = "The importer/legal declarant submitting the declaration.")]
		public override ZGuid AMA_OA_Declarant { get => base.AMA_OA_Declarant; set => base.AMA_OA_Declarant = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.RepresentativeList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Representative", Caption = "Representative", MediumCaption = "Representative", ShortCaption = "Rep.", FullDescription = "The representative (agent) submitting the declaration on the importer's/legal declarant's behalf.")]
		public override ZGuid AMA_OA_Representative { get => base.AMA_OA_Representative; set => base.AMA_OA_Representative = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.PresenterList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.AMA_OA_Presenter", Caption = "Presenter", MediumCaption = "Pres.", ShortCaption = "Pres.", FullDescription = "The presenter of the goods.")]
		public override ZGuid AMA_OA_Presenter
		{
			get => base.AMA_OA_Presenter;
			set => base.AMA_OA_Presenter = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.MemberState", Caption = "Member State", MediumCaption = "Member State", ShortCaption = "Member St.", FullDescription = "ISO 3166-1 alpha-2 code of the EU Member State the declaration is submitted to.")]
		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set => base.AMA_RN_NKCountry = value;
		}

		protected override ZString GetDataGroupingCore() => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(AMA_RN_NKCountry);

		[MaxLength(35)]
		public override ZString AMA_PaymentAccountNumber
		{
			get => base.AMA_PaymentAccountNumber;
			set => base.AMA_PaymentAccountNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.TransportMode", Caption = "Transport Mode", MediumCaption = "Trans. Mode", ShortCaption = "Mode", FullDescription = "The transport mode for the shipment.")]
		public override ZString AMA_TransportMode { get => base.AMA_TransportMode; set => base.AMA_TransportMode = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Voyage", Caption = "Flight/Voyage", MediumCaption = "Fl./Voy.", ShortCaption = "F/V", FullDescription = "The flight or voyage number associated with the shipment.")]
		public override ZString AMA_Voyage { get => base.AMA_Voyage; set => base.AMA_Voyage = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.VesselName", Caption = "Vessel", MediumCaption = "Vessel", ShortCaption = "Vessel", FullDescription = "Name of vessel.")]
		public override ZString AMA_VesselName { get => base.AMA_VesselName; set => base.AMA_VesselName = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.LloydsNumber", Caption = "Vessel IMO Number", MediumCaption = "Vessel IMO No.", ShortCaption = "IMO No.")]
		public override ZString AMA_LloydsNumber { get => base.AMA_LloydsNumber; set => base.AMA_LloydsNumber = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.RadioCallSign", Caption = "Radio Call Sign", MediumCaption = "Call Sign", ShortCaption = "Call Sign", FullDescription = "Maritime call sign assigned to a vessel used for radio transmissions.")]
		public override ZString AMA_RadioCallSign { get => base.AMA_RadioCallSign; set => base.AMA_RadioCallSign = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.ConveyanceNationality", Caption = "Conveyance Country/Region", MediumCaption = "Convey. Ctry/Rgn.", ShortCaption = "Conv. Ctry/Rgn.", FullDescription = "The country/region of conveyance.")]
		public override ZString AMA_RN_NKConveyanceNationality { get => base.AMA_RN_NKConveyanceNationality; set => base.AMA_RN_NKConveyanceNationality = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.VehicleRegistration", Caption = "Vehicle Registration", MediumCaption = "Vehicle Reg.", ShortCaption = "Vehicle", FullDescription = "The registration number of the vehicle the shipment is transported on.")]
		public override ZString AMA_VehicleRegistration { get => base.AMA_VehicleRegistration; set => base.AMA_VehicleRegistration = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Trailer1RegNo", Caption = "Trailer 1", MediumCaption = "Tr. 1", ShortCaption = "Tr. 1", FullDescription = "The registration number of the trailer attached to the vehicle the shipment is transported on (if applicable).")]
		public override ZString AMA_Trailer1RegNo { get => base.AMA_Trailer1RegNo; set => base.AMA_Trailer1RegNo = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Trailer1RegCountry", Caption = "Trailer 1 Country", MediumCaption = "Tr. 1 Country", ShortCaption = "Tr. 1 Ctry.", FullDescription = "The issuing country of the registration number of the trailer attached to the vehicle the shipment is transported on (if applicable).")]
		public override ZString AMA_RN_NKTrailer1RegCountry { get => base.AMA_RN_NKTrailer1RegCountry; set => base.AMA_RN_NKTrailer1RegCountry = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Trailer2RegNo", Caption = "Trailer 2", MediumCaption = "Tr. 2", ShortCaption = "Tr. 2", FullDescription = "The registration number of the second trailer attached to the vehicle the shipment is transported on (if applicable).")]
		public override ZString AMA_Trailer2RegNo { get => base.AMA_Trailer2RegNo; set => base.AMA_Trailer2RegNo = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.Trailer2RegCountry", Caption = "Trailer 2 Country", MediumCaption = "Tr. 2 Country", ShortCaption = "Tr. 2 Ctry.", FullDescription = "The issuing country of the registration number of the second trailer attached to the vehicle the shipment is transported on (if applicable).")]
		public override ZString AMA_RN_NKTrailer2RegCountry { get => base.AMA_RN_NKTrailer2RegCountry; set => base.AMA_RN_NKTrailer2RegCountry = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.PortOfLoading", Caption = "Load Port", MediumCaption = "Load Port", ShortCaption = "Load", FullDescription = "The place where shipments are loaded and secured aboard a vessel/aircraft. It may or may not be the same as the Port of Origin.")]
		public override ZString AMA_RL_NKPortOfLoading { get => base.AMA_RL_NKPortOfLoading; set => base.AMA_RL_NKPortOfLoading = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.PortOfFirstArrival", Caption = "Port Of First Arrival", MediumCaption = "First Arrival", ShortCaption = "1st Arr. Port", FullDescription = "First Port of Arrival")]
		public override ZString AMA_RL_NKPortOfFirstArrival { get => base.AMA_RL_NKPortOfFirstArrival; set => base.AMA_RL_NKPortOfFirstArrival = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.PortOfDischarge", Caption = "Discharge Port", MediumCaption = "Discharge", ShortCaption = "Disc.", FullDescription = "A place where a ship, vehicle, aircraft, cargo, emergency service, or person discharges or unloads some or all of its shipments.")]
		public override ZString AMA_RL_NKPortOfDischarge { get => base.AMA_RL_NKPortOfDischarge; set => base.AMA_RL_NKPortOfDischarge = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.ActualArrival", Caption = "Actual Date of Arrival", MediumCaption = "Act. Arrival", ShortCaption = "ATA")]
		public override ZDateTime AMA_A_ARV { get => base.AMA_A_ARV; set => base.AMA_A_ARV = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.CustomsOffice", Caption = "Customs Office", MediumCaption = "Cus. Office", ShortCaption = "Cus. Off.", FullDescription = "Code identifying the Customs Office associated with the declaration.")]
		public override ZString AMA_CustomsOffice { get => base.AMA_CustomsOffice; set => base.AMA_CustomsOffice = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.AgentType", Caption = "Representative Status", MediumCaption = "Rep. Status", ShortCaption = "Rep. Status", FullDescription = "The relevant code representing the status of the representative.")]
		public override ZString AMA_AgentType { get => base.AMA_AgentType; set => base.AMA_AgentType = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.PaymentMethod", Caption = "Payment Method", MediumCaption = "Payment Method", ShortCaption = "Payment Method", FullDescription = "The method of payment for any applicable duties/taxes.")]
		public override ZString AMA_PaymentMethod { get => base.AMA_PaymentMethod; set => base.AMA_PaymentMethod = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader.ApplicationCode", Caption = "Submit Type", MediumCaption = "Submit Type", ShortCaption = "Submit Type", FullDescription = "The declaration specification version of the relevant Customs authority used to submit the declaration.")]
		public override ZString AMA_ApplicationCode { get => base.AMA_ApplicationCode; set => base.AMA_ApplicationCode = value; }

		#endregion

		#region Captions

		public override ResourceStringData VoyageFlightNoLabel =>
			IsAir ? Res.GetData("506bf103-4acd-414b-848d-4d72716583cf", "Fl.", "Flight", "Flight", "The flight number the shipment is transported on.")
			: IsSea ? Res.GetData("25dc892f-c339-4f8b-8840-d4b92c433a3d", "Voy.", "Voyage", "Voyage", "Unique identifier assigned to a specific journey or trip undertaken by a vessel.")
			: IsRail ? Res.GetData("55087644-e548-489c-9f5c-56a26b808341", "Jrny.", "Journey", "Journey", "The Journey number associated with the shipment.")
			: Res.GetData("374e0196-63a9-41c9-b293-e3c77b7506bd", "F/V/J", "Fl./Voy./Jrny.", "Flight/Voyage/Journey", "The flight, voyage or journey number associated with the shipment.");

		#endregion

		#region Additional Document

		[ChildEditable(true)]
		public IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments
		{
			get
			{
				if (additionalDocuments == null)
				{
					additionalDocuments = CreateNewAdditionalDocumentCollection();
					additionalDocuments.Load();
					RegisterEditableChildObject(additionalDocuments);
				}

				return additionalDocuments;
			}
		}

		IAdditionalDocumentCollection<AdditionalDocument> additionalDocuments;

		protected virtual IAdditionalDocumentCollection<AdditionalDocument> CreateNewAdditionalDocumentCollection() => new AdditionalDocumentCollection<AdditionalDocument>(this);

		#endregion

		#region Supporting Document

		[ChildEditable(true)]
		public ISupportingDocumentCollection<SupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = CreateNewSupportingDocumentCollection();
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}

				return supportingDocuments;
			}
		}
		ISupportingDocumentCollection<SupportingDocument> supportingDocuments;

		protected virtual ISupportingDocumentCollection<SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

		#endregion

		#region Previous Document

		[ChildEditable(true)]
		public IPreviousDocumentCollection<PreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = CreateNewPreviousDocumentCollection();
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}

				return previousDocuments;
			}
		}
		IPreviousDocumentCollection<PreviousDocument> previousDocuments;

		protected virtual IPreviousDocumentCollection<PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		#endregion

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ H7CusSupportingInfoTypeList.Codes.AdditionalDocument, AdditionalDocumentType },
				{ H7CusSupportingInfoTypeList.Codes.SupportingDocument, SupportingDocumentType },
				{ H7CusSupportingInfoTypeList.Codes.PreviousDocument, PreviousDocumentType },
			};
		}
		protected virtual Type AdditionalDocumentType => typeof(AdditionalDocument);
		protected virtual Type SupportingDocumentType => typeof(SupportingDocument);
		protected virtual Type PreviousDocumentType => typeof(PreviousDocument);

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.OfficeCode, typeof(PresentationOfficeCode) }
			};
		}

		#endregion

		#region IWorkflowProvider

		protected override ProcessTaskCollection CreateNewProcessTaskCollection() => new ProcessTaskCollection<EUH7AsycudaManifestHeaderProcessTask, AsycudaManifestHeader>(this);

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.EUH7AsycudaManifestHeaderWorkflowDescriptorCode;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, AMA_RN_NKCountry, ZString.Empty);

			return result;
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => AMA_JobReference;
		ZString IRelatedJob.JobDescription => HumanReadableName;
		ZString IRelatedJob.JobStatus => AMA_CustomsStatus;
		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EU.EUH7;
		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			AMA_RN_NKCountry = GetDefaultCountryCode();
			AMA_ApplicationCode = "LVC";
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_RL_NKPortOfDischarge = "ERXXX";
			AMA_Nature = "IMP";
			AMA_JobReference = "EU123456";
		}
#endif

		#region ITemplateCopyable

		public IBusiness TemplateCopy() => Clone();

		#endregion

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>
			{
				AsycudaManifestHeaderSchema.Constants.AMA_Voyage,
				AsycudaManifestHeaderSchema.Constants.AMA_JobReference
			};
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (AsycudaManifestHeader)base.CloneInternal(args);

			templateCopy.AMA_RL_NKPortOfLoading = AMA_RL_NKPortOfLoading;
			templateCopy.AMA_RL_NKPortOfDischarge = AMA_RL_NKPortOfDischarge;
			templateCopy.PresentationOffice = PresentationOffice;

			foreach (var bill in Bills)
			{
				templateCopy.Bills.Add(bill.Clone());
			}

			return templateCopy;
		}

		#endregion

	}
}
