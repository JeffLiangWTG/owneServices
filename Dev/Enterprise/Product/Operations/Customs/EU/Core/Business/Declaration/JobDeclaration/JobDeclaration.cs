using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.DeclarationStatusUpdater;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using Enterprise.Customs.EU.Business.MessageCalculators;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.EUCommonConstants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EUExitControl;
using EFTAUniversalReferenceConstants = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[SystemDefinedValues]
	[VisualizableDocumentsSupportable(nameof(JobDeclarationEUVisualizableDocumentSupporter))]
	public partial class JobDeclaration : AutoEUJobDeclaration
		, ICanBeImportOrExport
		, Integration.Customs.EU.IJobDeclaration
		, IInvoicesProvider
		, ILandedCostHeader
		, IEuOfficeCodeProvider
		, ICusAddInfoTypeSupporter
		, IAdditionalInfosProviderWithValidationDecider
		, ISupportingDocumentsProvider
		, IPreviousDocumentsProviderWithValidationDecider
		, ICusSupportingInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IEntryStyleCalculatorFallbackInfoProvider
		, IUcc6ValueProvider
		, ICommonInvoiceDataProvider
		, IValidationModesSupporter
		, ICusGoodsLocationProvider
	{
		public new class Schema : AutoEUJobDeclaration.Schema
		{
			public const string JE_Calc_DateOfClearance = "JE_Calc_DateOfClearance";
			public const string JE_EntryStyle = "JE_EntryStyle";
			public const int ZG_VATDeferTypeMaxLength = 1;
			public const string PackTypes = "PackTypes";
			public const string CustomsDocStatus = nameof(JobDeclaration.CustomsDocStatus);
			public const string CustomsDocStatusDesc = nameof(JobDeclaration.CustomsDocStatusDesc);
			public const string ExitPresentationStatus = nameof(JobDeclaration.ExitPresentationStatus);
			public const string ExitPresentationStatusDesc = nameof(JobDeclaration.ExitPresentationStatusDesc);
		}

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly JobDeclarationTypeDecider TypeDecider = new JobDeclarationTypeDecider();

		public override IEnumerable<string> StmALogProxyFieldsNames
		{
			get
			{
				foreach (var fieldName in base.StmALogProxyFieldsNames)
				{
					yield return fieldName;
				}

				yield return Schema.JE_Calc_DateOfClearance;
			}
		}

		protected override void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration newDeclaration, CloneType cloneType)
		{
			base.CleanUpNewDeclarationAfterCloneCore(newDeclaration, cloneType);
			newDeclaration.ImporterDocumentaryAddress.E2_OA_Address = ImporterDocumentaryAddress.E2_OA_Address;
			newDeclaration.SupplierDocumentaryAddress.E2_OA_Address = SupplierDocumentaryAddress.E2_OA_Address;
			newDeclaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ContainerTerminalOperatorDocAddress.E2_OA_Address;
			newDeclaration.WarehouseDocAddress.E2_OA_Address = WarehouseDocAddress.E2_OA_Address;
			newDeclaration.DepotDocAddress.E2_OA_Address = DepotDocAddress.E2_OA_Address;
			newDeclaration.ContainerYardDocAddress.E2_OA_Address = ContainerYardDocAddress.E2_OA_Address;
			var euDeclaration = (JobDeclaration)newDeclaration;
			euDeclaration.SupervisingOfficeDocAddress.E2_OA_Address = SupervisingOfficeDocAddress.E2_OA_Address;
			euDeclaration.ZG_StyleOfEntrySOE = ZString.Empty;
		}

		#region JE Properties

		public override ZGuid JE_JS
		{
			get => base.JE_JS;
			set
			{
				bool hasChanged = base.JE_JS != value;
				base.JE_JS = value;
				if (hasChanged && !IsCopying)
				{
					MarkInvoicesAsNeedingValidation();
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GB
		{
			get => base.JE_GB;
			set
			{
				var oldValue = JE_GB;
				base.JE_GB = value;
				if (!IsCopying && oldValue != JE_GB)
				{
					MarkInvoicesAsNeedingValidation();
					Packages.MarkAsNeedingValidation();
					CustomsOffices.MarkAsNeedingValidation();
					MarkFeesAsNeedingValidation();
					AddInfoChild.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid JE_GC
		{
			get { return base.JE_GC; }
			set
			{
				bool hasChanged = base.JE_GC != value;
				base.JE_GC = value;
				if (hasChanged)
				{
					MarkInvoicesAsNeedingValidation();
					Packages.MarkAsNeedingValidation();
					CustomsOffices.MarkAsNeedingValidation();
					MarkFeesAsNeedingValidation();
				}
			}
		}

		public override ZString JE_GoodsDestination
		{
			get => base.JE_GoodsDestination;
			set
			{
				var oldValue = JE_GoodsDestination;
				base.JE_GoodsDestination = value;
				if (!IsCopying && oldValue != JE_GoodsDestination)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override bool EquipmentsRequired => IsUCC6AndIsExport;

		[ReadOnlyMember(nameof(AgentsReferenceReadOnly))]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_AgentsReference", Caption = "Agents Reference", ShortCaption = "Agents Ref.")]
		public override ZString JE_AgentsReference { get => base.JE_AgentsReference; set => base.JE_AgentsReference = value; }

		protected virtual bool AgentsReferenceReadOnly => false;

		[ResourceStringData("EU.JobDeclaration.JE_VesselName", Caption = "[21] Vessel")]
		public override ZString JE_VesselName
		{
			get => base.JE_VesselName;
			set
			{
				bool hasChanged = JE_VesselName != value;
				base.JE_VesselName = value;
				if (hasChanged && !IsCopying)
				{
					ZString transportCountry = TransportCountryCalculator.TransportCountry(this);
					if (!transportCountry.IsEmpty)
					{
						JE_RN_NKTransportNationality = transportCountry;
					}
				}
			}
		}

		[ResourceStringData("EU.JobDeclaration.JE_AircraftRegistration", Caption = "Aircraft Registration Number", ShortCaption = "Aircraft Reg No.")]
		public override ZString JE_AircraftRegistration { get => base.JE_AircraftRegistration; set => base.JE_AircraftRegistration = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_MessageType", Caption = "Entry Type")]
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;

				if (oldValue != value)
				{
					if (ShowApplyDefaultMessageTypeFromSupplierOrImporterDialog(value))
					{
						base.JE_MessageType = value;

						CalculateEntryStyleIfNeeded();
						EmptyTaxTypeForAllInvoiceLinesIfNotImport();
						EmptyEntryInstructionsGuaranteesIfNecessary();
						DefaultIsHighValueOverride();
						ResetImpInvoiceLineQuantities();
						Packages.MarkAsNeedingValidation();
					}
				}
			}
		}

		void ResetImpInvoiceLineQuantities()
		{
			if (IsExport)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x =>
				{
					x.JI_CustomsFourthQuantityInfo.ClearValue();
					x.JI_CustomsFifthQuantityInfo.ClearValue();
				});
			}
		}

		void EmptyEntryInstructionsGuaranteesIfNecessary()
		{
			foreach (CusEntryInstruction entryInstruction in CustomsEntryInstructions)
			{
				entryInstruction.EmptyGuaranteesIfNecessary();
			}
		}

		void DefaultIsHighValueOverride()
		{
			if (IsImport && DV1DetailsSupport && Enterprise.Customs.EU.Registry.EUCustomsDataRegistry.Instance.DefaultDV1.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty))
			{
				ZG_IsHighValueOvrd = ZBool.True;
			}
			else if (!IsImport || !DV1DetailsSupport)
			{
				ZG_IsHighValueOvrd = ZBool.False;
			}
		}

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var oldValue = JE_ApplicationCode;
				base.JE_ApplicationCode = value;
				if (oldValue != JE_ApplicationCode)
				{
					CustomsOffices.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(JE_ShipmentIncoTermPlace_ReadOnly))]
		[ResourceStringData("EUJobDeclaration|JE_ShipmentIncoTermPlace", Caption = "[20.2] Place")]
		public override ZString JE_ShipmentIncoTermPlace { get => base.JE_ShipmentIncoTermPlace; set => base.JE_ShipmentIncoTermPlace = value; }

		public ZBool JE_ShipmentIncoTermPlace_ReadOnly => JE_ShipmentIncoTermPlace_ReadOnlyCore;
		protected virtual ZBool JE_ShipmentIncoTermPlace_ReadOnlyCore => AgreedPlaceCodeSupport && IsAgreedUnloco;

		internal ZBool IsAgreedUnloco => (EUD_AgreedPlaceCode.Length == 5 ? new RefUNLOCO.Loader(Factory).Load(EUD_AgreedPlaceCode) : (ZG_AgreedPlaceCode.Length == 5 ? new RefUNLOCO.Loader(Factory).Load(ZG_AgreedPlaceCode) : null)) != null;

		[ResourceStringData("EUJobDeclaration|JE_ShipmentIncoTerm", Caption = "[20.1] Incoterm")]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set
			{
				var oldValue = JE_ShipmentIncoTerm;
				base.JE_ShipmentIncoTerm = value;
				if (!IsCopying && oldValue != JE_ShipmentIncoTerm)
				{
					if (!JE_ShipmentIncoTerm.IsEmpty)
					{
						ZG_AgreedPlaceCode = GetNumericIncoTermModeCodeIncoTermAndFlux(JE_ShipmentIncoTerm);
						ClearEUD_AgreedPlaceCodeIfNeeded();
						AddInfoChild?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.AgreedPlaceCodeList))]
		public override ZString JE_AgreedPlaceCode { get => base.JE_AgreedPlaceCode; set => base.JE_AgreedPlaceCode = value; }

		[ResourceStringData("EUJobDeclaration|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code", FullDescription = "Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)")]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				var oldValue = ZG_AgreedPlaceCode;
				base.ZG_AgreedPlaceCode = value;
				if (!IsCopying && oldValue != ZG_AgreedPlaceCode)
				{
					if (!ZG_AgreedPlaceCode.IsEmpty)
					{
						if (IsAgreedUnloco && AgreedPlaceCodeSupport)
						{
							JE_ShipmentIncoTermPlace = ZString.Empty;
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.RegionOfDestinationList))]
		public override ZString JE_RegionOfDestination { get => base.JE_RegionOfDestination; set => base.JE_RegionOfDestination = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|ZG_RegionOfDestination", Caption = "Region of Destination", ShortCaption = "Dest. Region")]
		public override ZString ZG_RegionOfDestination { get => base.ZG_RegionOfDestination; set => base.ZG_RegionOfDestination = value; }

		[ResourceStringData("EUJobDeclaration|EUD_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code", FullDescription = "Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)")]
		public override ZString EUD_AgreedPlaceCode { get => base.EUD_AgreedPlaceCode; set => base.EUD_AgreedPlaceCode = value; }

		void ClearEUD_AgreedPlaceCodeIfNeeded()
		{
			if (!AgreedPlaceCodeSupportAndVisible && AgreedPlaceCodeSupport)
			{
				EUD_AgreedPlaceCode = ZString.Empty;
			}
		}

		public ZBool AgreedPlaceCodeSupportAndVisible => AgreedPlaceCodeSupportAndVisibleCore;

		protected virtual ZBool AgreedPlaceCodeSupportAndVisibleCore => JE_ShipmentIncoTerm != Core.Constants.IncoTerms.Other && AgreedPlaceCodeSupport;

		internal bool AgreedPlaceCodeSupport => AgreedPlaceCodeSupportCore;

		protected virtual bool AgreedPlaceCodeSupportCore => IsUCC6;

		public bool EUD_AgreedPlaceCodeValidationSupport => EUD_AgreedPlaceCodeValidationSupportCore;

		protected virtual bool EUD_AgreedPlaceCodeValidationSupportCore => true;

		public bool ZG_AgreedPlaceCodeValidationSupport => ZG_AgreedPlaceCodeValidationSupportCore;

		protected virtual bool ZG_AgreedPlaceCodeValidationSupportCore => true;

		public override void DefaultIncoTerm()
		{
			var (incoterm, incotermPlace, incotermMode) = GetDefaultINCOTermWithPlaceAndModeFromSupplierBuyerLink();

			if (!incoterm.IsEmpty)
			{
				JE_ShipmentIncoTerm = incoterm;
			}

			if (!incotermMode.IsEmpty)
			{
				ZG_AgreedPlaceCode = GetNumericIncoTermModeCodeFromWtgCode(incotermMode);
			}

			if (!incotermPlace.IsEmpty)
			{
				JE_ShipmentIncoTermPlace = incotermPlace;
			}
		}

		public (ZString incoterm, ZString incotermPlace, ZString incotermMode) GetDefaultINCOTermWithPlaceAndModeFromSupplierBuyerLink()
		{
			return OrgSupplierBuyerLink.GetDefaultINCOTermWithPlaceAndMode(Consignor, Consignee, JE_RL_NKFinalDestination.Left(2), TransportMode, ContainerMode);
		}

		public virtual ZString GetNumericIncoTermModeCodeFromWtgCode(string pfIncotermMode)
		{
			ZString result = "";
			if (Country.IsPartOfEuropeanUnion)
			{
				switch (pfIncotermMode)
				{
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS:
						result = "1";
						break;
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH:
						result = "2";
						break;
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT:
						result = "3";
						break;
					default:
						break;
				}
			}
			else
			{
				switch (pfIncotermMode)
				{
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS:
						result = "3";
						break;
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH:
						result = "2";
						break;
					case OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT:
						result = "1";
						break;
					default:
						break;
				}
			}
			return result;
		}

		public virtual ZString GetNumericIncoTermModeCodeIncoTermAndFlux(string pfIncoterm)
		{
			var result = ZString.Empty;
			if (!AgreedPlaceCodeSupport)
			{
				if (IsImport)
				{
					switch (pfIncoterm)
					{
						case Core.Constants.IncoTerms.ExWorks:
						case Core.Constants.IncoTerms.FreeAlongsideShip:
						case Core.Constants.IncoTerms.FreeOnBoard:
							result = "3";
							break;
						case Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded:
						case Core.Constants.IncoTerms.DeliveredAtPlace:
						case Core.Constants.IncoTerms.DeliveredDutyPaid:
							result = "1";
							break;
						default:
							break;
					}
				}
				else
				{
					switch (pfIncoterm)
					{
						case Core.Constants.IncoTerms.ExWorks:
						case Core.Constants.IncoTerms.FreeAlongsideShip:
						case Core.Constants.IncoTerms.FreeOnBoard:
							result = "1";
							break;
						case Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded:
						case Core.Constants.IncoTerms.DeliveredAtPlace:
						case Core.Constants.IncoTerms.DeliveredDutyPaid:
							result = "3";
							break;
						default:
							break;
					}
				}
			}
			return result;
		}

		[ResourceStringData("20290633-3A15-4AB6-822A-E15047A85A9E", Caption = "Representative")]
		[ResourceStringData("0C52E7EA-BF4C-4AAD-AA99-AF4918197C78", Caption = "Representative", ShortCaption = "Represent.", FullDescription = "[13 06 000 000] Representative", MultipleKey = CaptionKeyImportUCC6)]
		public override ZGuid JE_OA_Representative
		{
			get => base.JE_OA_Representative;
			set => base.JE_OA_Representative = value;
		}

		public OrgAddress RepresentativeOrgAddress => Factory.Load<OrgAddress>(JE_OA_Representative);

		[ResourceStringData("1ACE9F47-F0CF-4810-83D1-6F7260D6DCE3", Caption = "Seller")]
		[ResourceStringData("ACA4D626-58A7-4889-9AB9-87D76FABEE05", Caption = "Seller", FullDescription = "[13 08 000 000] Seller", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JE_OA_SellerAddress
		{
			get => base.JE_OA_SellerAddress;
			set => base.JE_OA_SellerAddress = value;
		}

		public override ZGuid JE_OH_Importer
		{
			get => base.JE_OH_Importer;
			set
			{
				MarkInvoicesAsNeedingValidation();
				base.JE_OH_Importer = value;
			}
		}

		protected virtual bool ShowApplyDefaultMessageTypeFromSupplierOrImporterDialog(ZString newMessageType)
		{
			if (ShowChangeMessageTypePopup &&
				EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var oldMessageType = JE_MessageType;
				return ShowChangeMessageTypePopup = Globals.Message.Show(Res.GetString("E5319031-4DD6-4DA3-B7FB-64F88884DBBF",
					"This change in the importer or supplier address will cause the flux (entry type - import/export/etc.) to change from {0} to {1}, which will cause a large amount of defaulting to occur. Would you like to allow this update in flux?", oldMessageType, newMessageType),
					Res.GetString("0cf03d24-1d67-43cc-9754-f0d5b204aeeb", "Apply Default Entry Type"), ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning, ZDialogResult.Yes) == ZDialogResult.Yes;
			}
			else
			{
				return true;
			}
		}

		protected override void DefaultMessageTypeFromSupplierOrImporter(MessageTypeDefaultingTriggerSource source)
		{
			if (ShowChangeMessageTypePopup || !EUCustomsDataRegistry.Instance.CheckChangeMessageTypeFromSupplierOrImporter.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				base.DefaultMessageTypeFromSupplierOrImporter(source);
			}
			ShowChangeMessageTypePopup = false;
		}

		bool ShowChangeMessageTypePopup { get; set; }

		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set
			{
				MarkInvoicesAsNeedingValidation();
				base.JE_OH_Supplier = value;
			}
		}

		[ResourceStringData("53E43694-BE7B-4051-B097-1225F42E7139", Caption = "Manufacturer")]
		public override ZGuid JE_OA_ManufacturerAddress
		{
			get => base.JE_OA_ManufacturerAddress;
			set
			{
				var oldValue = JE_OA_ManufacturerAddress;
				base.JE_OA_ManufacturerAddress = value;
				if (!IsCopying && JE_OA_ManufacturerAddress != oldValue)
				{
					UpdateManufacturerFromAddress();
				}
			}
		}

		[ResourceStringData("C9B43F14-61F5-403A-97C9-FBE654B6A536", Caption = "Buyer")]
		[ResourceStringData("66519BD8-4C55-45D7-834F-6EEBE8A2DCF7", Caption = "Buyer", FullDescription = "[13 09 000 000] Buyer", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JE_OH_Buyer
		{
			get => base.JE_OH_Buyer;
			set { base.JE_OH_Buyer = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOffices))]
		public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

		[ResourceStringData("EU.JobDeclaration.JE_TransportMode", Caption = "[25] Transport")]
		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && JE_TransportMode != oldValue)
				{
					if (TransportMeansDependency == EUCommonConstants.TransportModeSource.TransportModeAtBorder)
					{
						SetDefaultInlandTransportCodeIfRequired();
					}
					DefaultIATALoadPort();
					SetDefaultBorderTransportToIDForTransportMode();
					Validation.ValidateJE_ShipmentIncoTerm();
				}
			}
		}

		[ResourceStringData("EU.JobDeclaration.JE_TransportMeans", Caption = "Type of ID", MediumCaption = "ID Type", FullDescription = "[19 06 061 000] Arrival transport means < Type of identification", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JE_TransportMeans
		{
			get => base.JE_TransportMeans;
			set => base.JE_TransportMeans = value;
		}

		[ResourceStringData("EU.JobDeclaration.ZG_Box18TransportID", Caption = "[18] Trans. ID (Inland)", MediumCaption = "Trans. ID (Inland)", ShortCaption = "Transport ID", FullDescription = "[18} Inland Transport Identification")]
		public override ZString ZG_Box18TransportID
		{
			get => base.ZG_Box18TransportID;
			set => base.ZG_Box18TransportID = value;
		}

		[ResourceStringData("EU.JobDeclaration.JE_TransportModeInland", Caption = "[26] Inland M.O.T", MediumCaption = "Inland M.O.T.", ShortCaption = "Inland")]
		[ResourceStringData("EU.JobDeclaration.JE_TransportModeInland|IMPUCC6", Caption = "Inland M.O.T", MediumCaption = "Inland M.O.T.", ShortCaption = "Inland", FullDescription = "[19 04 001 000] Inland mode of transport", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportTypeList))]
		[MaxLength(3)]
		public override ZString JE_TransportModeInland
		{
			get => base.JE_TransportModeInland;
			set
			{
				var oldValue = JE_TransportModeInland;
				base.JE_TransportModeInland = value;
				if (!IsCopying && oldValue != JE_TransportModeInland)
				{
					if (TransportMeansDependency == EUCommonConstants.TransportModeSource.InlandTransportMode)
					{
						SetDefaultInlandTransportCodeIfRequired();
					}
					InlandTransports.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(nameof(JE_TransportIDInlandMaxLength))]
		public override ZString JE_TransportIDInland
		{
			get => base.JE_TransportIDInland;
			set => base.JE_TransportIDInland = value;
		}

		protected int JE_TransportIDInlandMaxLength => IsInAesTransitionPeriod ? AesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength : JobDeclarationSchema.JE_TransportIDInland.MaxLength;

		protected int JE_Trailer1RegNoMaxLength => IsInAesTransitionPeriod ? AesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength : JobDeclarationSchema.JE_Trailer1RegNo.MaxLength;

		protected int JE_Trailer2RegNoMaxLength => IsInAesTransitionPeriod ? AesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength : JobDeclarationSchema.JE_Trailer2RegNo.MaxLength;

		bool IsInAesTransitionPeriod => IsUCC6AndIsExport && IsTransitionPeriodAES30;

		public const int AesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength = 27;

		[MaxLength(nameof(JE_Trailer1RegNoMaxLength))]
		public override ZString JE_Trailer1RegNo { get => base.JE_Trailer1RegNo; set => base.JE_Trailer1RegNo = value; }

		[MaxLength(nameof(JE_Trailer2RegNoMaxLength))]
		public override ZString JE_Trailer2RegNo { get => base.JE_Trailer2RegNo; set => base.JE_Trailer2RegNo = value; }

		void SetDefaultInlandTransportCodeIfRequired()
		{
			if (IsUCC6AndIsImport || IsUCC6AndIsExport)
			{
				JE_TransportMeans = AddInfoLookups.InlandTransportCodeList.DefaultCode;
			}
		}

		internal TransportModeSource TransportMeansDependency => TransportMeansDependencyCore;

		protected virtual TransportModeSource TransportMeansDependencyCore => TransportModeSource.None;

		internal ZString TransportModeValueForTransportMeans => TransportMeansDependency switch
		{
			EUCommonConstants.TransportModeSource.TransportModeAtBorder => JE_TransportMode,
			EUCommonConstants.TransportModeSource.InlandTransportMode => JE_TransportModeInland,
			_ => ZString.Empty
		};

		[ResourceStringData("EU.JobDeclaration.JE_RN_NKTransportNationality", Caption = "[21] Nationality")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportCountryList))]
		public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportCountryList))]
		public override ZString JE_RN_NKTransportNationalityInland { get => base.JE_RN_NKTransportNationalityInland; set => base.JE_RN_NKTransportNationalityInland = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_CustomsProfile", Caption = "Profile")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ProfileList))]
		public override ZString JE_CustomsProfile { get => base.JE_CustomsProfile; set => base.JE_CustomsProfile = value; }

		[ResourceStringData("FEC7B6D0-0499-4ED5-8E08-2F622A218364", Caption = "Port of First EU Arrival", MediumCaption = "First EU Arrival", ShortCaption = "EU Arrival")]
		public override ZString JE_RL_NKPortOfFirstArrival { get => base.JE_RL_NKPortOfFirstArrival; set => base.JE_RL_NKPortOfFirstArrival = value; }

		public override ZDateTime JE_DateOfArrival
		{
			get => base.JE_DateOfArrival;
			set
			{
				base.JE_DateOfArrival = value;
				MarkInvoicesAsNeedingValidation();
			}
		}

		public override ZString JE_RL_NKPortOfLoading
		{
			get => base.JE_RL_NKPortOfLoading;
			set
			{
				var oldValue = JE_RL_NKPortOfLoading;
				base.JE_RL_NKPortOfLoading = value;
				if (!IsCopying && oldValue != JE_RL_NKPortOfLoading)
				{
					DefaultIATALoadPort();
				}
			}
		}

		[ResourceStringData("EU.JobDeclaration.JE_RL_NKOrigin", ShortCaption = "[15] Dispatch", Caption = "[15] Country/Region of Dispatch", FullDescription = "[15] Country/Region of Dispatch of the goods")]
		[ResourceStringData("9CE4433A-E974-4DB4-BDB5-FB4946D91757", Caption = "Dispatch", MultipleKey = CaptionKeyExportUCC6)]
		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				base.JE_RL_NKOrigin = value;
				if (!IsCopying)
				{
					JE_GoodsOrigin = GetDefaultTerritory(JE_RL_NKOrigin);
				}
			}
		}

		[ResourceStringData("EU.JobDeclaration.JE_GoodsOrigin", ShortCaption = "[15] Dispatch", Caption = "[15] Country/Region of Dispatch", FullDescription = "[15] Country/Region of Dispatch of the goods")]
		public override ZString JE_GoodsOrigin { get => base.JE_GoodsOrigin; set => base.JE_GoodsOrigin = value; }

		[ResourceStringData("EU.JobDeclaration.JE_RL_NKFinalDestination", Caption = "[17] Destination")]
		[ResourceStringData("77D1CE48-EED6-4EB6-BABC-60F4ED6EA063", Caption = "Destination", MultipleKey = CaptionKeyExportUCC6)]
		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set
			{
				base.JE_RL_NKFinalDestination = value;
				if (!IsCopying)
				{
					JE_GoodsDestination = GetDefaultTerritory(JE_RL_NKFinalDestination);
				}
			}
		}

		public ZString GetDefaultTerritory(ZString unloco)
		{
			var defaultTerritory = ZString.Empty;
			if (!unloco.IsEmpty)
			{
				var dataGrouping = GetDefaultDataGroupingCode();
				defaultTerritory = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, dataGrouping, RefCusMapTypeList.Codes.EUCTY, unloco, ZDateTime.Today);
				if (defaultTerritory.IsEmpty)
				{
					var trimmedCountry = unloco.Left(2);
					defaultTerritory = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, dataGrouping, RefCusMapTypeList.Codes.EUCTY, trimmedCountry, ZDateTime.Today);
					if (defaultTerritory.IsEmpty)
					{
						var convertedTerritory = ConvertTerritory(trimmedCountry);
						defaultTerritory = convertedTerritory == ZString.Empty ? trimmedCountry : convertedTerritory;
					}
				}
			}
			return defaultTerritory;
		}

		protected virtual ZString ConvertTerritory(string country)
		{
			var convertTerritoryQuery = new ZDBOnlyQuery(typeof(CusRefTradeGroupView));

			var dataGroupingQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping);
			dataGroupingQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritories);
			var tradegroupQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup);

			tradegroupQuery.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, country);
			dataGroupingQuery.AddSubQuery(CusRefTradeGroupViewSchema.PK, CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, tradegroupQuery, JoinCondition.And);

			var tradegroupCountryCodeQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode);

			var tradeGroupPKQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefTradeGroupViewSchema.PK);
			tradeGroupPKQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritoryCountries);
			tradeGroupPKQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, "EUN");

			tradegroupCountryCodeQuery.AddSubQuery(CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, CusRefTradeGroupViewSchema.PK, tradeGroupPKQuery, JoinCondition.And);

			dataGroupingQuery.AddSubQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, tradegroupCountryCodeQuery, JoinCondition.And);

			convertTerritoryQuery.AddSubQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, dataGroupingQuery, JoinCondition.And);

			var territory = Factory.Load<CusRefTradeGroupView>(convertTerritoryQuery).FirstOrDefault();
			if (territory != null)
			{
				return territory.ZZA_ZZZ_NKDataGrouping.Left(2);
			}
			else
			{
				return string.Empty;
			}
		}

		[MaxLength(17)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Locations))]
		[ResourceStringData("EUJobDeclaration|JE_LocationOfGoods", Caption = "[30] Goods Location")]
		public override ZString JE_LocationOfGoods { get => base.JE_LocationOfGoods; set => base.JE_LocationOfGoods = value; }

		[ResourceStringData("EUJobDeclaration|JE_IATALoadPort", Caption = "[61] Foreign Airport Code", FullDescription = "Box 61, foreign airport code. Filter is limited by country of loading.")]
		public override ZString JE_IATALoadPort { get => base.JE_IATALoadPort; set => base.JE_IATALoadPort = value; }

		[ResourceStringData("EUJobDeclaration|JE_DeclarantType", Caption = "[14] Rep. Type")]
		public override ZString JE_DeclarantType { get => base.JE_DeclarantType; set => base.JE_DeclarantType = value; }

		#endregion

		#region JE_Calc Properties

		public ZDateTime JE_Calc_DateOfClearance => GetDateOfClearanceCore();

		protected virtual ZDateTime GetDateOfClearanceCore()
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Table, JobDeclarationSchema.Constants.TableName);
			query.AddToFilter(GetDateOfClearanceQueryForStmALog());
			query.AddToFilter(StmALogSchema.SL_Parent, this.PK);
			var log = Factory.LoadTop1<StmALog>(query);
			return log == null ? ZDateTime.Empty : log.SL_EventTime;
		}

		#endregion

		#region New Properties

		#region JE_MessageSubType Breakout Fields
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryStyleList))]
		[MaxLength(2)]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_EntryStyle", Caption = "[1a] Entry Style")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_EntryStyle|UCC6", Caption = "Declaration Type", FullDescription = "[11 01 001 000] Declaration Type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public virtual ZString JE_EntryStyle
		{
			get => JE_MessageSubType;
			set
			{
				var oldValue = JE_EntryStyle;
				CheckMaximumLength(JE_EntryStyleInfo, value);
				JE_MessageSubType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_EntryStyle();
					Validation.ValidateJE_RL_NKPortOfLoading();
				}

				if (oldValue != JE_EntryStyle)
				{
					EmptyIsSecurityDeclarationIfNecessary();
				}
				JE_EntryStyleInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_EntryStyleInfo => GetZPropertyInfo(Schema.JE_EntryStyle);

		public ZBool IsEntryStyleExportToSpecialTerritory => JE_EntryStyle == EntryStyleListExport.Codes.ExportToSpecialTerritory;

		#endregion

		#region IEntryStyleCalculatorFallbackInfoProvider Members

		ZString IEntryStyleCalculatorFallbackInfoProvider.GetEntrySubStyleForCommonTransit(RefCountry country) => GetEntrySubStyleForCommonTransit(country);

		ZString IEntryStyleCalculatorFallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment() => EntryStyleForInwardProcessingVATPayment;

		#endregion

		public ZString PackTypes => Factory.GetValue(ref packTypes, delegate
		{
			var result = ZString.Empty;
			if (Packages.Count != 0)
			{
				var firstPackageType = Packages[0].CW_PackType;
				result = Packages.Cast<BasePackage>().All(x => x.CW_PackType == firstPackageType) ? firstPackageType : (ZString)MessageStatusList.Codes.MultipleStatus;
			}
			return result;
		});
		CachedProperty<ZString> packTypes;

		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|ZG_IsTrainingDeclaration", Caption = "Training Entry")]
		public override ZBool ZG_IsTrainingDeclaration
		{
			get => base.ZG_IsTrainingDeclaration;
			set => base.ZG_IsTrainingDeclaration = value;
		}

		[ResourceStringData("690700F3-5824-441C-841C-0758142A0979", Caption = "D.V.1?")]
		public override ZBool ZG_IsHighValueOvrd { get => base.ZG_IsHighValueOvrd; set => base.ZG_IsHighValueOvrd = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ShipmentTypeList))]
		public override ZString JE_ShipmentType { get => base.JE_ShipmentType; set => base.JE_ShipmentType = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|ZG_ShipmentType", Caption = "Shipment Type")]
		public override ZString ZG_ShipmentType { get => base.ZG_ShipmentType; set => base.ZG_ShipmentType = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|ZG_LCPDepart", Caption = "EIDR Departure")]
		public override ZDateTime ZG_LCPDepart { get => base.ZG_LCPDepart; set => base.ZG_LCPDepart = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|ZG_LCPInspect", Caption = "EIDR Inspection")]
		public override ZDateTime ZG_LCPInspect { get => base.ZG_LCPInspect; set => base.ZG_LCPInspect = value; }

		[ChildEditable]
		public InlandTransportCollection InlandTransports
		{
			get
			{
				if (inlandTransports == null)
				{
					inlandTransports = GetNewInlandTransportCollection();
					inlandTransports.Load();
					RegisterEditableChildObject(inlandTransports);
				}

				return inlandTransports;
			}
		}
		InlandTransportCollection inlandTransports;

		protected virtual InlandTransportCollection GetNewInlandTransportCollection() => new InlandTransportCollection(this);

		#region Itinerary Countries

		[ChildEditable(true)]
		public ItineraryCountryCollection ItineraryCountries
		{
			get
			{
				if (itineraryCountries == null)
				{
					itineraryCountries = GetNewItineraryCountriesCollection();
					itineraryCountries.Load();
					RegisterEditableChildObject(itineraryCountries);
					itineraryCountries.HasChangesChanged += ItineraryCountries_OnContentChanged;
				}
				return itineraryCountries;
			}
		}
		ItineraryCountryCollection itineraryCountries;

		protected virtual ItineraryCountryCollection GetNewItineraryCountriesCollection() => new ItineraryCountryCollection(this);

		bool isPopulatingFromIdentifier;

		public ZString UniqueVoyageIdentifier
		{
			get => uniqueVoyageIdentifier;
			set
			{
				var oldValue = UniqueVoyageIdentifier;
				if (isPopulatingFromIdentifier || oldValue == value)
				{
					return;
				}

				try
				{
					isPopulatingFromIdentifier = true;
					ItineraryCountries.HasChanges = false;
					using (ItineraryCountries.SuspendSettingHasChanges())
					{
						ItineraryCountries.RemoveAndDeleteAll();
						uniqueVoyageIdentifier = value;
						ItineraryCountries.PopulateItineraryCountryCollection();
					}
					ItineraryCountries.SetReadOnlyIncludingChildren(ItineraryCountriesReadonly);
				}
				finally
				{
					isPopulatingFromIdentifier = false;
				}
				UniqueVoyageIdentifierInfo.RefreshBinding(oldValue);
			}
		}
		ZString uniqueVoyageIdentifier;

		void ItineraryCountries_OnContentChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (isPopulatingFromIdentifier || !e.ObjectJustWasChanged)
			{
				return;
			}

			var newUniqueVoyageIdentifier = string.Concat(ItineraryCountries.Cast<ItineraryCountry>().OrderBy(c => c.CY_Order).Select(c => c.CY_Code));
			uniqueVoyageIdentifier = newUniqueVoyageIdentifier;
		}

		public ZString ItineraryCountryList => string.Join(" ", ItineraryCountries.Select(c => c.CY_Code));

		public ZPropertyInfo UniqueVoyageIdentifierInfo => GetZPropertyInfo(nameof(UniqueVoyageIdentifier));

		public override ZBool JE_OverrideFreightDefaults
		{
			get => base.JE_OverrideFreightDefaults;
			set
			{
				var oldValue = JE_OverrideFreightDefaults;
				base.JE_OverrideFreightDefaults = value;
				if (!IsCopying && oldValue != JE_OverrideFreightDefaults)
				{
					ItineraryCountries.SetReadOnlyIncludingChildren(ItineraryCountriesReadonly);
				}
			}
		}

		public bool ItineraryCountriesReadonly => !JE_OverrideFreightDefaults;

		#endregion

		#endregion

		#region SubLocation

		[MaxLength(3)]
		public virtual ZString SubLocation
		{
			get { return base.JE_SubLocationOfGoods.Trim(); }
			set
			{
				CheckMaximumLength(SubLocationInfo, value);
				JE_SubLocationOfGoods = JE_LocationOfGoods + value.PadRight(3);
				SubLocationInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SubLocationInfo => GetZPropertyInfo(nameof(SubLocation), SubLocationFriendlyName);
		protected virtual string SubLocationFriendlyName => (NoResString)"Sub Location";

		#endregion

		#region Declarant
		[ResourceStringData("C317F9E7-2477-4827-AE23-3F4ABF9DA60C", Caption = "[14] Declarant", FullDescription = "[14] Declarant. Name of the declarant controlling this declaration.")]
		[ResourceStringData("A1A29A7E-EE68-4858-A83B-CB416D0C8469", Caption = "Declarant", FullDescription = "[13 05 000 000] Declarant", MultipleKey = CaptionKeyImportUCC6)]
		public override ZGuid JE_OA_DeclarantAddress
		{
			get => base.JE_OA_DeclarantAddress;
			set
			{
				base.JE_OA_DeclarantAddress = value;
				SetRepresentationTypeIfMatchingEORICodes();
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public ZAddress JE_OA_Declarant_ZAddress => fJE_OA_Declarant_ZAddress ?? (fJE_OA_Declarant_ZAddress = GetNewfJE_OA_Declarant_ZAddress());
		ZAddress fJE_OA_Declarant_ZAddress;

		ZAddress GetNewfJE_OA_Declarant_ZAddress() => new ZAddress(JE_OA_DeclarantAddressInfo);

		public OrgAddress DeclarantOrgAddress => Factory.Load<OrgAddress>(base.JE_OA_DeclarantAddress);

		public virtual bool IsDeclarantAddressRequired => JE_DeclarantType != RepresentationTypeList.Codes._1Self;

		#endregion

		[ResourceStringData("085A92CA-36C3-47C9-B63F-B46FB99F8D07", Caption = "Customs Doc. Status", FullDescription = "Customs Document Status", ShortCaption = "Doc. Status")]
		public ZString CustomsDocStatus => Factory.GetValue(ref customsDocStatusCached, () =>
		{
			var result = ZString.Empty;
			if (IsExport)
			{
				var statuses = CustomsEntryInstructions.SelectMany(x => x.RequestedDocuments.Cast<RequestedDocument>()).Where(x => x.CSI_Type.EqualsIgnoringCase(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument)).Select(x => x.CSI_Status).GroupBy(x => x).Select(x => x.Key).ToHashSet();
				switch (statuses.Count)
				{
					case 0:
						//do nothing
						break;
					case 1:
						result = statuses.First();
						break;
					default:
						if (statuses.Contains(RequestedDocumentStatusList.Codes.RequestOpened))
						{
							result = RequestedDocumentStatusList.Codes.RequestOpened;
						}
						else if (statuses.Count == 2 && statuses.Contains(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived) && statuses.Contains(RequestedDocumentStatusList.Codes.RequestCancelled))
						{
							result = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
						}
						else if (statuses.Contains(RequestedDocumentStatusList.Codes.PhysicallyPresentDocument))
						{
							result = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
						}
						break;
				}
			}
			return result;
		});
		CachedProperty<ZString> customsDocStatusCached;

		public ZPropertyInfo CustomsDocStatusInfo => GetZPropertyInfo(Schema.CustomsDocStatus);

		[ResourceStringData("A86E2051-AAF1-4399-8F5E-9499D88A230A", Caption = "Customs Doc. Status Description", FullDescription = "Customs Document Status Description", MediumCaption = "Customs Doc. Status Desc.", ShortCaption = "Doc. Status Desc.")]
		public ZString CustomsDocStatusDesc => Factory.GetValue(ref customsDocStatusDescCached, () => Factory.GetCachedValue<RequestedDocumentStatusList>().GetDescriptionFromCode(CustomsDocStatus));
		CachedProperty<ZString> customsDocStatusDescCached;

		public ZPropertyInfo CustomsDocStatusDescInfo => GetZPropertyInfo(Schema.CustomsDocStatusDesc);

		#region ExitPresentationStatus
		[ResourceStringData("{F7695006-9893-467E-BC6E-695D86DB7718}", Caption = "Exit Presentation Status")]
		public ZString ExitPresentationStatus => Factory.GetValue(ref exitPresentationStatusCached, () =>
		{
			var result = ZString.Empty;
			if (IsExport)
			{
				var statusList = ExitReports.Where(r => r.CER_Type == ExitReportTypeList.Codes.Presentation).Select(r => r.CER_Status).Distinct().Take(2).ToArray();
				if (statusList.Length > 0)
				{
					result = statusList.Length > 1 ? (ZString)AESEntryStatusList.Codes.MultipleStatus : statusList[0];
				}
			}
			return result;
		});
		CachedProperty<ZString> exitPresentationStatusCached;

		public ZPropertyInfo ExitPresentationStatusInfo => GetZPropertyInfo(Schema.ExitPresentationStatus);

		[ResourceStringData("{706CEAB1-DF3F-4EB6-8EA9-468088D481BB}", Caption = "Exit Presentation Status Description", MediumCaption = "Exit Presentation Status Desc.", ShortCaption = "Exit Pres. Status Desc.")]
		public ZString ExitPresentationStatusDesc => Factory.GetValue(ref exitPresentationStatusDescCached, () => Factory.GetCachedValue<AESEntryStatusList>().GetDescriptionFromCode(ExitPresentationStatus));
		CachedProperty<ZString> exitPresentationStatusDescCached;

		public ZPropertyInfo ExitPresentationStatusDescInfo => GetZPropertyInfo(Schema.ExitPresentationStatus);
		#endregion

		public IReadOnlyList<ICusExitHeader> ExitHeaders => Factory.GetValue(ref exitHeadersCached, () =>
		{
			var exitHeaderLoader = ObjectFactory.Get<ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", Factory);
			return exitHeaderLoader.Load(!IsInDatabase, PK, TablePrefix); // TODO: Use JE_ClusterKey when CXH_ClusterKey has been updated to use parent clusterkey
		});
		CachedProperty<IReadOnlyList<ICusExitHeader>> exitHeadersCached;

		public IReadOnlyList<ICusExitReport> ExitReports => Factory.GetValue(ref exitReportsCached, () =>
		{
			ICusExitReport[] result = null;
			var exitHeaders = ExitHeaders;
			if (exitHeaders.Count > 0)
			{
				var query = new ZQuery(CusExitReportSchema.CER_ClusterKey, exitHeaders.Select(x => x.CXH_ClusterKey));
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				result = Factory.Load<ICusExitReport>(query);
			}
			return result ?? Array.Empty<ICusExitReport>();
		});
		CachedProperty<IReadOnlyList<ICusExitReport>> exitReportsCached;

		protected override IReadOnlyList<string> MultipleKeysToUseCore
		{
			get
			{
				var keys = new List<string>();

				if (IsBLT)
				{
					keys.Add(CaptionKeyBLT);
				}

				if (IsUCC6)
				{
					if (IsExport)
					{
						keys.Add(CaptionKeyExportUCC6);
					}
					else if (IsImport)
					{
						keys.Add(CaptionKeyImportUCC6);
					}
					else
					{
						keys.Add(CaptionKeyUCC);
					}
				}
				else if (IsUCC5)
				{
					keys.Add(CaptionKeyUCC);
				}
				else
				{
					keys.Add(CaptionKeySAD);
				}

				return keys.ToArray();
			}
		}
		protected bool IsBLT => JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		public virtual ZBool ExitControlTabVisible => !IsImport;

		public virtual ZBool ZG_GatewayVisible => false;

		public virtual ZBool AircraftRegistrationNumberVisible => false;

		public ZBool ContainerControlCheckboxVisible => ContainerControlCheckboxVisibleCore;

		protected virtual ZBool ContainerControlCheckboxVisibleCore => false;

		public ZBool ContainerUnloadedCheckboxVisible => ContainerUnloadedCheckboxVisibleCore;

		protected virtual ZBool ContainerUnloadedCheckboxVisibleCore => false;

		public override ZDateTime DateOfValuation => !EarliestCustomsEntryIssueDate.IsEmpty ? EarliestCustomsEntryIssueDate : CachedTodaysDate;

		public override ZBool BondedWarehouseEditable => !AreMultipleEntryInstructionsAllowed && base.BondedWarehouseEditable;

		public override bool ContainerModeVisible => true;

		public virtual bool AdditionalSealsRequired => IsExport;

		public bool IsTransportMeansImoShipIdentificationNumber => JE_TransportMeans.EqualsIgnoringCase(TransportMeansList.Codes.ImoShipIdentificationNumber);

		public bool IsUCC6AndIsImport => IsUCC6 && IsImport;

		public bool IsUCC6AndIsExport => IsUCC6 && IsExport;

		public bool IsUCCCompliant => IsUCC6 || IsUCC5;

		public bool IsUCC6 => Configuration.IsUCC6(this);

		public bool IsUCC5 => Configuration.IsUCC5(this);

		public bool IsTransitionPeriodAES30 => Configuration.IsTransitionPeriodAES30(this);

		public bool UseIDDDocument => Configuration.UseIDDDocument(this);

		void DefaultIATALoadPort()
		{
			if (SupportIATALoadPortDefaulting && IsImport && IsAir)
			{
				JE_IATALoadPort = PortOfLoading?.RL_IATA ?? ZString.Empty;
			}
		}

		protected override bool SupportMultipleWarehouseEntryCore => true;

		protected virtual bool SupportIATALoadPortDefaulting => true;

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			base.JE_MessageTypeChanged(oldValue, newValue);
			HandleDocAddressChanged(ImporterDocumentaryAddress, ImporterDocAddressRequirement, DocAddressType.ImporterDocumentaryAddress, JE_OH_ImporterInfo, SuspendImporterDocumentaryAddressDefaulting);
			HandleDocAddressChanged(SupplierDocumentaryAddress, SupplierDocAddressRequirement, DocAddressType.SupplierDocumentaryAddress, JE_OH_SupplierInfo, SuspendSupplierDocumentaryAddressDefaulting);

			CustomsOffices.DefaultMandatoryCustomsOffices();
			EmptyIsSecurityDeclarationIfNecessary();
		}

		public static ZQuery GetDateOfClearanceQueryForStmALog()
		{
			// Allows for log{code=CLR} or log{code=CES,Ref=CLR}
			var clearedOneQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			clearedOneQuery.AddToFilter(StmALogSchema.SL_Reference, Events.CustomsCleared.Code);
			var clearedTwoQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
			var clearedBothQuery = new ZQuery();
			clearedBothQuery.AddToFilter(clearedOneQuery);
			clearedBothQuery.AddToFilter(clearedTwoQuery, JoinCondition.Or);
			return clearedBothQuery;
		}

		protected internal new Event CustomsClearedEventType => base.CustomsClearedEventType;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

		protected override bool IsDefaultImporterDocAddressesEnabled => !IsImporterDocumentaryAddressDefaultingSuspended;

		protected override void ImporterDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.ImporterDocumentaryAddressChanged(sender, e);
			HandleDocAddressChanged(ImporterDocumentaryAddress, ImporterDocAddressRequirement, DocAddressType.ImporterDocumentaryAddress, JE_OH_ImporterInfo, SuspendImporterDocumentaryAddressDefaulting);
		}

		protected override bool IsDefaultSupplierDocAddressesEnabled => !IsSupplierDocumentaryAddressDefaultingSuspended;

		protected override void SupplierDocumentaryAddressChanged(object sender, EventArgs e)
		{
			base.SupplierDocumentaryAddressChanged(sender, e);
			HandleDocAddressChanged(SupplierDocumentaryAddress, SupplierDocAddressRequirement, DocAddressType.SupplierDocumentaryAddress, JE_OH_SupplierInfo, SuspendSupplierDocumentaryAddressDefaulting);
		}

		void HandleDocAddressChanged(JobDocAddress address, JobDocAddressRequirement requirement, DocAddressType addressType, ZPropertyInfo je_Oh_WhateverInfo, Func<IDisposable> suspendDocumentaryAddressDefaulting)
		{
			DecorateDocAddressRequirement(requirement, addressType);  // To wire up for validation
			MarkInvoicesAsNeedingValidation();  // TO ensure we have a party at header or line level, not neither.
			address.Validation.ValidateE2_OA_Address();  // fire the validation that we just wired
			var organisationPK = address.OrganisationPK;
			if ((ZGuid)je_Oh_WhateverInfo.Value != organisationPK && IsPersistent)
			{
				ShowChangeMessageTypePopup = (!JE_OH_Importer.IsEmpty && !JE_OH_Supplier.IsEmpty && !organisationPK.IsEmpty);

				using (suspendDocumentaryAddressDefaulting())
				{
					je_Oh_WhateverInfo.Value = organisationPK;  // Setting JE_OH_xxx updates the xxxDocumentaryAddress, but we need this bit to make sure it synchs backwards.  We do so via ZPropertyInfo and not by naked ZGuid because the latter is passed as what VB6 people would call 'ByVal'
				}
			}
			SetRepresentationTypeIfMatchingEORICodes();
		}

		protected override void FlushImporterDocumentaryAddressIfBlank(ZGuid je_oh_importer) { }

		protected override void FlushSupplierDocumentaryAddressIfBlank(ZGuid je_oh_supplier) { }

		protected virtual void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.CustomsWarehouseAddress:
					requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
						+=
						delegate
						{
							Validation.ValidateWarehouseDocAddressForeignKey();
						};
					break;
			}
		}

		protected virtual void SetRepresentationTypeIfMatchingEORICodes()
		{
			if (IsDeclarantSameAsLocalClientBasedOnEori())
			{
				JE_DeclarantType = GetDeclarantTypeForMatchingEORICodes();
			}
		}

		public bool IsDeclarantSameAsLocalClientBasedOnEori()
		{
			bool retVal = false;
			if (Declarant != null)
			{
				var declarantEuIdentificationNumber = Declarant.Header.GetEuIdentificationNumber();
				if (!string.IsNullOrEmpty(declarantEuIdentificationNumber))
				{
					string eoriOfLocalClient = "";

					if (IsImport)
					{
						eoriOfLocalClient = ImporterDocumentaryAddress.GetEuIdentificationNumber();
					}
					else if (IsExport)
					{
						eoriOfLocalClient = SupplierDocumentaryAddress.GetEuIdentificationNumber();
					}

					if (declarantEuIdentificationNumber == eoriOfLocalClient)
					{
						if (IsUnRegisteredOrPrivateOrBlankEori(declarantEuIdentificationNumber))
						{
							retVal = true;
						}
					}
				}
			}
			return retVal;
		}

		protected virtual string GetDeclarantTypeForMatchingEORICodes() => RepresentationTypeList.Codes._1Self;

		protected virtual string GetDeclarantTypeByDefault() => RepresentationTypeList.Codes._2Direct;

		bool IsUnRegisteredOrPrivateOrBlankEori(ZString eoriCode)
		{
			return !eoriCode.IsEmpty
					&&
					!eoriCode.Equals(EuEoriProviderAndValidator.UnregForEori(Factory))
					&&
					!eoriCode.Equals(EuEoriProviderAndValidator.PrivateEoriReg(Factory))
					&&
					!eoriCode.Equals(this.CountryCode + EuEoriProviderAndValidator.UnregForEori(Factory))
					&&
					!eoriCode.Equals(this.CountryCode + EuEoriProviderAndValidator.PrivateEoriReg(Factory));
		}

		#region Guarantee collection

		[ChildEditable(true)]
		public GuaranteeForDeclarationCollection Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					guarantees = GetGuaranteesCore();
					guarantees.Load();
					RegisterEditableChildObject(guarantees);
				}
				return guarantees;
			}
		}
		GuaranteeForDeclarationCollection guarantees;

		protected virtual GuaranteeForDeclarationCollection GetGuaranteesCore() => new GuaranteeForDeclarationCollection(this);

		#endregion

		#region CustomsOffices, OfficeOfExit

		[ChildEditable(true)]
		public EuOfficeCodeCollection CustomsOffices
		{
			get
			{
				if (customsOffices == null)
				{
					customsOffices = GetCustomsOffices();
					customsOffices.Load();
					RegisterEditableChildObject(customsOffices);
				}
				return customsOffices;
			}
		}
		EuOfficeCodeCollection customsOffices;

		[ChildEditable(true)]
		public IBusinessObjectCollection<ICusCodeData> CustomsOfficeCollection => CustomsOffices;

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => CustomsOffices.Cast<EuOfficeCode>();

		protected virtual EuOfficeCodeCollection GetCustomsOffices()
		{
			return new EuOfficeCodeCollection(this);
		}

		public virtual ZString OfficeOfExit
		{
			get { return IsOfficeOfExitMeaningfulForDeclaration ? ((JobDeclarationCustomsOfficeRequirementHelper)CustomsOfficeRequirementHelper).GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExit) : ZString.Empty; }
			set { throw new NotImplementedException(); }
		}

		protected virtual bool IsOfficeOfExitMeaningfulForDeclaration => IsExport;

		public virtual ZString OfficeOfEntry
		{
			get { return IsImport ? ((JobDeclarationCustomsOfficeRequirementHelper)CustomsOfficeRequirementHelper).GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent) : ZString.Empty; }
			set { throw new NotImplementedException(); }
		}

		#endregion

		protected override void SetupImporterDocumentaryAddress(JobDocAddress importerDocumentaryAddress)
		{
			base.SetupImporterDocumentaryAddress(importerDocumentaryAddress);
			importerDocumentaryAddress.OrgHeaderAfterChange += ImporterDocumentaryAddressOnOrgHeaderAfterChange;
		}

		void ImporterDocumentaryAddressOnOrgHeaderAfterChange(object sender, EventArgs e) => CalculateEntryStyleIfNeeded();

		protected override void SetupSupplierDocumentaryAddress(JobDocAddress supplierDocumentaryAddress)
		{
			base.SetupSupplierDocumentaryAddress(supplierDocumentaryAddress);
			supplierDocumentaryAddress.OrgHeaderAfterChange += SupplierDocumentaryAddressOnOrgHeaderAfterChange;
		}

		void SupplierDocumentaryAddressOnOrgHeaderAfterChange(object sender, EventArgs e) => CalculateEntryStyleIfNeeded();

		protected override void ImporterDeliveryAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			base.ImporterDeliveryAddressChanged(oldAddressPK, newAddressPK);
			CalculateEntryStyleIfNeeded();
		}

		protected override void SupplierPickupAddressChanged(ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			base.SupplierPickupAddressChanged(oldAddressPK, newAddressPK);
			CalculateEntryStyleIfNeeded();
		}

		void CalculateEntryStyleIfNeeded()
		{
			var entryStyleCalculationStrategy = GetEntryStyleCalculationStrategy();
			var entryStyle = entryStyleCalculationStrategy.Calculate();
			if (!entryStyle.IsEmpty)
			{
				JE_EntryStyle = entryStyle;
			}
		}

		protected virtual IEntryStyleCalculationStrategy GetEntryStyleCalculationStrategy()
		{
			if (IsImport)
			{
				return new ImportEntryStyleCalculationStrategy(Factory, CountryCode, SupplierDocumentaryAddress.Country, this);
			}
			if (IsExport)
			{
				return new ExportEntryStyleCalculationStrategy(Factory, CountryCode, ImporterDocumentaryAddress.Country, this);
			}
			return new EmptyEntryStyleCalculationStrategy();
		}

		protected virtual ZString GetEntrySubStyleForCommonTransit(RefCountry country) => EntryStyleListImport.Codes.ImportFromEFTAMember;
		protected virtual ZString EntryStyleForInwardProcessingVATPayment => GetEntryStyleByEntryType();

		protected virtual ZString GetEntryStyleByEntryType()
		{
			var entryStyle = ZString.Empty;
			if (IsImport)
			{
				entryStyle = EntryStyleListImport.Codes.ImportNormal;
			}
			else if (IsExport)
			{
				entryStyle = EntryStyleListExport.Codes.ExportNormal;
			}

			return entryStyle;
		}

		void UpdateManufacturerFromAddress()
		{
			JE_OH_Manufacturer = ManufacturerAddress?.Header?.PK ?? ZGuid.Empty;
		}

		public void MarkInvoicesAsNeedingValidation()
		{
			foreach (JobComInvoiceHeader inv in Invoices)
			{
				inv.MarkAsNeedingValidation();
				MarkInvoiceLinesAsNeedingValidation(inv);
			}
		}

		internal void MarkFeesAsNeedingValidation() => ActiveEntryHeaders?.Cast<CusEntryHeader>()
														.ForEach(header => header.AllEntryLines.Cast<CusEntryLine>()
														.ForEach(line => line.Fees.MarkAsNeedingValidationIncludingChildren()));

		void MarkInvoiceLinesAsNeedingValidation(JobComInvoiceHeader inv) => inv.InvoiceLines.MarkAsNeedingValidation();

		public void MarkInvoiceLinesAsNeedingValidation() => InvoiceLines.MarkAsNeedingValidation();

		protected virtual MessageChangedStatusDeterminerToDictateWhetherSavingAllowed GetMessageChangedStatusDeterminerForDictatingWhetherSavingAllowed() => null;

		protected virtual bool RequiresMessageSaveValidation =>
			CustomsEntryHeaders.Any(x => x.Messages.IsWaitingForAResponse && !x.IsFailedFromTransmission) ||
			(CustomsEntryHeaders.HasAnyEntryWhichMessagesCannotBeChanged && CustomsEntryHeaders.Any(x => !x.IsFailedFromTransmission));

		public ContinueWithSave CanSaveBasedOnAwaitingMessageAndAnyChangesThatWouldAffectMessages()
		{
			if (RequiresMessageSaveValidation)
			{
				var saveChangesAllower = GetMessageChangedStatusDeterminerForDictatingWhetherSavingAllowed();
				if (saveChangesAllower != null)
				{
					return saveChangesAllower.AllowSave ? ContinueWithSave.Yes : ContinueWithSave.No;
				}
			}
			// not waiting or country doesn't have its own MessageChangedStatusDeterminerToDictateWhetherSavingAllowed, so allow save
			return ContinueWithSave.Yes;
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer() => new DeclarationValueChangedAnnouncer(this);

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			// At the time of writing, 2008-12-17, a GB declaration is an EU declaration, but if we subclass for other countries then they will need to either use Customs.Business.JobDeclarationSynchroniser or do their own JobDeclarationSynchroniser
			if (this is JobDeclaration)
			{
				return new JobDeclarationSynchroniser(this);
			}
			else
			{
				return base.GetNewShipmentSynchroniser();
			}
		}

		public override bool UseGenPivotForRelatedDeclarations => true;

		public override bool ShouldCopyProcedureFromPreviousInvoiceLine => CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.GetValueWithoutFallback(Guid.Empty, Branch?.EntityPK.ToGuid() ?? GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);

		#region Cloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobDeclaration.Schema.JE_UCR);

			return result;
		}

		#endregion

		#region Container helpers

		public override ZBool ContainersRequired
		{
			get { return ContainersAlwaysRequired || IsContainerised; }
		}

		public override ZBool IsContainerised
		{
			get { return base.IsContainerised || JE_ContainerMode == Enterprise.Core.Constants.ContainerModes.ULD; }
		}

		public override bool ShouldDeleteContainers
		{
			get { return !ContainersRequired; }
		}

		#endregion

		protected override JobDocAddressRequirement GetNewWarehouseDocAddressRequirement()
		{
			var result = base.GetNewWarehouseDocAddressRequirement();
			DecorateDocAddressRequirement(result, DocAddressType.CustomsWarehouseAddress);
			return result;
		}

		#region Exporter DocAddress

		public JobDocAddress ExporterDocAddress
		{
			get
			{
				if (fExporterDocAddress == null || fExporterDocAddress.IsDeleted)
				{
					if (fExporterDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fExporterDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(ExporterDocAddress_ValueChanged);
						}
					}

					fExporterDocAddress = DocAddresses.FindOrCreateWithRequirement(ExporterDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fExporterDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(ExporterDocAddress_ValueChanged);
					}
				}
				return fExporterDocAddress;
			}
		}

		protected virtual void ExporterDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fExporterDocAddress;
		JobDocAddressRequirement fExporterDocAddressRequirement;

		public JobDocAddressRequirement ExporterDocAddressRequirement
		{
			get
			{
				if (fExporterDocAddressRequirement == null)
				{
					fExporterDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Exporter, ContactType.Administration);

					fExporterDocAddressRequirement.ValidateOrganisationPK += ExporterDocAddressRequirement_ValidateOrganisationPK;
					DocAddressManager.AddRequirement(fExporterDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fExporterDocAddressRequirement, DocAddressType.Exporter);
				return fExporterDocAddressRequirement;
			}
		}

		protected virtual void ExporterDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
		}

		#endregion

		#region Representative DocAddress
		public JobDocAddress RepresentativeDocAddress
		{
			get
			{
				if (fRepresentativeDocAddress == null || fRepresentativeDocAddress.IsDeleted)
				{
					if (fRepresentativeDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fRepresentativeDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(RepresentativeDocAddress_ValueChanged);
						}
					}

					fRepresentativeDocAddress = DocAddresses.FindOrCreateWithRequirement(RepresentativeDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fRepresentativeDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(RepresentativeDocAddress_ValueChanged);
					}
				}
				return fRepresentativeDocAddress;
			}
		}

		protected virtual void RepresentativeDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fRepresentativeDocAddress;
		JobDocAddressRequirement fRepresentativeDocAddressRequirement;

		public JobDocAddressRequirement RepresentativeDocAddressRequirement
		{
			get
			{
				if (fRepresentativeDocAddressRequirement == null)
				{
					fRepresentativeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Representative, ContactType.Administration);
					DocAddressManager.AddRequirement(fRepresentativeDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fRepresentativeDocAddressRequirement, DocAddressType.Representative);
				return fRepresentativeDocAddressRequirement;
			}
		}
		#endregion

		#region Selling Party DocAddress
		public JobDocAddress SellingPartyDocAddress
		{
			get
			{
				if (fSellingPartyDocAddress == null || fSellingPartyDocAddress.IsDeleted)
				{
					if (fSellingPartyDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fSellingPartyDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(SellingPartyDocAddress_ValueChanged);
						}
					}

					fSellingPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(SellingPartyDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fSellingPartyDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(SellingPartyDocAddress_ValueChanged);
					}
				}
				return fSellingPartyDocAddress;
			}
		}

		protected virtual void SellingPartyDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fSellingPartyDocAddress;
		JobDocAddressRequirement fSellingPartyDocAddressRequirement;

		public JobDocAddressRequirement SellingPartyDocAddressRequirement
		{
			get
			{
				if (fSellingPartyDocAddressRequirement == null)
				{
					fSellingPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.SellingParty, ContactType.Administration);
					DocAddressManager.AddRequirement(fSellingPartyDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fSellingPartyDocAddressRequirement, DocAddressType.SellingParty);
				return fSellingPartyDocAddressRequirement;
			}
		}
		#endregion

		#region ContractualPartnerDocAddress

		public JobDocAddress ContractualPartnerDocAddress
		{
			get
			{
				if (contractualPartnerDocAddress == null || contractualPartnerDocAddress.IsDeleted)
				{
					contractualPartnerDocAddress = DocAddresses.FindOrCreateWithRequirement(ContractualPartnerDocAddressRequirement);
				}
				return contractualPartnerDocAddress;
			}
		}

		JobDocAddress contractualPartnerDocAddress;

		JobDocAddressRequirement ContractualPartnerDocAddressRequirement
		{
			get
			{
				if (contractualPartnerDocAddressRequirement == null)
				{
					contractualPartnerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ContractualPartner, ContactType.Consignor);
					contractualPartnerDocAddressRequirement.ValidateOrganisationPK += ContractualPartnerDocAddressRequirement_ValidateOrganisationPK;
					DocAddressManager.AddRequirement(contractualPartnerDocAddressRequirement);
				}
				return contractualPartnerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement contractualPartnerDocAddressRequirement;

		protected virtual void ContractualPartnerDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
		}

		#endregion

		#region CarrierEUBorderAddress

		public JobDocAddress CarrierEUBorderDocAddress
		{
			get
			{
				if (carrierEUBorderDocAddress == null || carrierEUBorderDocAddress.IsDeleted)
				{
					carrierEUBorderDocAddress = DocAddresses.FindOrCreateWithRequirement(CarrierEUBorderDocAddressRequirement);
				}
				return carrierEUBorderDocAddress;
			}
		}

		JobDocAddress carrierEUBorderDocAddress;

		public JobDocAddressRequirement CarrierEUBorderDocAddressRequirement
		{
			get
			{
				if (carrierEUBorderDocAddressRequirement == null)
				{
					carrierEUBorderDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Carrier);
					DocAddressManager.AddRequirement(carrierEUBorderDocAddressRequirement);
				}
				DecorateDocAddressRequirement(carrierEUBorderDocAddressRequirement, DocAddressType.Carrier);
				return carrierEUBorderDocAddressRequirement;
			}
		}
		JobDocAddressRequirement carrierEUBorderDocAddressRequirement;

		#endregion

		#region Notify party
		public JobDocAddress NotifyPartyDocAddress
		{
			get
			{
				if (fNotifyPartyDocAddress == null || fNotifyPartyDocAddress.IsDeleted)
				{
					if (fNotifyPartyDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fNotifyPartyDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(NotifyPartyDocAddress_ValueChanged);
						}
					}

					fNotifyPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(NotifyPartyDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fNotifyPartyDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(NotifyPartyDocAddress_ValueChanged);
					}
				}
				return fNotifyPartyDocAddress;
			}
		}

		protected virtual void NotifyPartyDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fNotifyPartyDocAddress;
		JobDocAddressRequirement fNotifyPartyDocAddressRequirement;

		public JobDocAddressRequirement NotifyPartyDocAddressRequirement
		{
			get
			{
				if (fNotifyPartyDocAddressRequirement == null)
				{
					fNotifyPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty, ContactType.Administration);
					DocAddressManager.AddRequirement(fNotifyPartyDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fNotifyPartyDocAddressRequirement, DocAddressType.NotifyParty);
				return fNotifyPartyDocAddressRequirement;
			}
		}
		#endregion

		#region Supervising office
		#region SupervisingOfficeDocAddress
		public virtual JobDocAddress SupervisingOfficeDocAddress
		{
			get
			{
				if (fSupervisingOfficeDocAddress == null || fSupervisingOfficeDocAddress.IsDeleted)
				{
					if (fSupervisingOfficeDocAddress != null)
					{
						foreach (ZPropertyInfo propertyInfo in fSupervisingOfficeDocAddress.ZPropertyInfoHash)
						{
							propertyInfo.ValueChanged -= new EventHandler(SupervisingOfficeDocAddress_ValueChanged);
						}
					}

					fSupervisingOfficeDocAddress = DocAddresses.FindOrCreateWithRequirement(SupervisingOfficeDocAddressRequirement);

					foreach (ZPropertyInfo propertyInfo in fSupervisingOfficeDocAddress.ZPropertyInfoHash)
					{
						propertyInfo.ValueChanged += new EventHandler(SupervisingOfficeDocAddress_ValueChanged);
					}
				}
				return fSupervisingOfficeDocAddress;
			}
		}

		protected virtual void SupervisingOfficeDocAddress_ValueChanged(object sender, EventArgs e)
		{
		}

		JobDocAddress fSupervisingOfficeDocAddress;
		JobDocAddressRequirement fSupervisingOfficeDocAddressRequirement;

		public JobDocAddressRequirement SupervisingOfficeDocAddressRequirement
		{
			get
			{
				if (fSupervisingOfficeDocAddressRequirement == null)
				{
					fSupervisingOfficeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsSupervisingOffice, ContactType.Administration);
					DocAddressManager.AddRequirement(fSupervisingOfficeDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fSupervisingOfficeDocAddressRequirement, DocAddressType.CustomsSupervisingOffice);
				return fSupervisingOfficeDocAddressRequirement;
			}
		}
		#endregion

		// We do not cache this because the declarant or the declarant's spoff may quite reasonably be edited in by the user when the dec is open
		public ZGuid GetSpoffFromDeclarant()
		{
			if (Declarant != null)
			{
				OrgHeader declarant = Declarant.Header;
				OrgHeader spoff = declarant.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsOffice, RelatedPartyDirectionList.Codes.Forwarder);
				if (spoff != null && spoff.MainAddress != null)
				{
					return spoff.MainAddress.PK;
				}
			}
			return ZGuid.Empty;
		}

		public void SetSupervisingOfficeFromDeclarant()
		{
			SupervisingOfficeDocAddress.E2_OA_Address = GetSpoffFromDeclarant();
		}
		#endregion

		#region DefermentPartyDocAddress
		public virtual JobDocAddress DefermentPartyDocAddress
		{
			get
			{
				if (fDefermentPartyDocAddress == null || fDefermentPartyDocAddress.IsDeleted)
				{
					fDefermentPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(DefermentPartyDocAddressRequirement);

					if (fDefermentPartyDocAddress != null)
					{
						SetupDefermentPartyDocAddress(fDefermentPartyDocAddress);
					}
				}
				return fDefermentPartyDocAddress;
			}
		}
		JobDocAddress fDefermentPartyDocAddress;

		public JobDocAddressRequirement DefermentPartyDocAddressRequirement
		{
			get
			{
				if (fDefermentPartyDocAddressRequirement == null)
				{
					fDefermentPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.DefermentParty, ContactType.Administration);
					fDefermentPartyDocAddressRequirement.ValidateOrganisationPK += DefermentPartyDocAddressRequirement_ValidateOrganisationPK;
					DocAddressManager.AddRequirement(fDefermentPartyDocAddressRequirement);
				}
				DecorateDocAddressRequirement(fDefermentPartyDocAddressRequirement, DocAddressType.DefermentParty);
				return fDefermentPartyDocAddressRequirement;
			}
		}
		JobDocAddressRequirement fDefermentPartyDocAddressRequirement;

		protected virtual void DefermentPartyDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var info = parent.OrganisationPKInfo;
			if (IsImport)
			{
				var organisation = parent.Organisation;
				if (organisation != null)
				{
					if (!ExistsDefermentAccount(organisation))
					{
						info.AddMessageError(Res.GetString("61A387E0-2290-452F-B826-C180BF87D76B", "The Deferment Party must have a Deferment Account Number"));
					}

					if (organisation.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Length == 0)
					{
						info.AddMessageError(Res.GetString("BAD0C0EF-A871-40A9-A415-0780CA8AAD29", "The Deferment Party must have a Registration Number / Code of Type 'EOR'"));
					}
				}
			}
		}

		protected virtual ZBool ExistsDefermentAccount(OrgHeader defermentParty)
		{
			return !defermentParty.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, CountryCode).IsEmpty;
		}

		protected virtual void SetupDefermentPartyDocAddress(JobDocAddress defermentPartyDocAddress)
		{
			// If anything changes - Required for MarkAsNeedingValidation on the Parent object (if required for LightValidation)
			defermentPartyDocAddress.DocAddressChanged += new EventHandler(DefermentPartyDocAddressChanged);
			defermentPartyDocAddress.OnRelationshipFieldsChanged += new EventHandler(DefermentPartyDocAddressChanged);
			defermentPartyDocAddress.OrgHeaderAfterChange += new EventHandler(DefermentPartyDocAddressChanged);
		}

		protected virtual void DefermentPartyDocAddressChanged(object sender, EventArgs e)
		{
		}
		#endregion

		#region Merge Functionality

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override bool HasSplitEntriesCore => false;

		protected override bool UseDeclarationContainersIfNoneFoundOnEntryCore => false;

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);
		#endregion

		#region FEC challenge add infos

		/// <summary>
		/// If you want Chief to accept the entry even with outstanding fec challenges, resend with this set to true
		/// </summary>
		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_RouteFrequested)]
		[ResourceStringData("Enterprise.Customs.EU.Business.JobDeclaration|JE_RouteFRequested", Caption = "Request route F")]
		public override ZBool JE_RouteFRequested { get => base.JE_RouteFRequested; set => base.JE_RouteFRequested = value; }

		#endregion

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{   // Allows EU to dictate its own cloning, allowing us to hook EU-specific post-clone crap on after the real guts of the clone.
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected override ZQuery GetValidCusEntryNumFilter()
		{
			List<ZGuid> parentIds = new List<ZGuid>();
			foreach (CusEntryHeader entryHeader in this.CustomsEntryHeaders)
			{
				if (entryHeader.ShouldBeIncludedInCusEntryNumberFilter)
				{
					parentIds.Add(entryHeader.PK);
				}
			}

			// This will show the REAL ENTRY NUMBER FROM CUSTOMS not the (d)ucr, not the mucr, not the mrn....
			ZQuery entryNumberQuery = new ZQuery();
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryNumberTypes.EU.MasterUCR);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			if (ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilter)
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.NotEqual, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			}
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, parentIds);
			return entryNumberQuery;
		}

		public ZBool ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilter => ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilterCore;
		protected virtual ZBool ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilterCore => false;

		#region Save Events

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateUCRIfNeeded();
		}

		protected override void OnFactorySaving()
		{
			SetAppropriateSupplierAndImporter();
			base.OnFactorySaving();
		}

		void SetAppropriateSupplierAndImporter()
		{
			SetOrgIfDifferent(JE_OH_SupplierInfo, SupplierDocumentaryAddress, SetJE_OH_SupplierBaseValueOnly);
			SetOrgIfDifferent(JE_OH_ImporterInfo, ImporterDocumentaryAddress, SetJE_OH_ImporterBaseValueOnly);
		}

		void SetOrgIfDifferent(ZPropertyInfo pkInfo, JobDocAddress docAddress, Action<ZGuid> setBaseValueOnly)
		{
			if (IsPersistent && (!IsInDatabase || pkInfo.HasChanges || !docAddress.IsInDatabase || docAddress.E2_OA_AddressInfo.HasChanges || docAddress.E2_AddressOverrideInfo.HasChanges))
			{
				var oldValue = (ZGuid)pkInfo.Value;
				var isAddressValid = docAddress.E2_OA_Address.IsValid;

				var newValue = docAddress.E2_AddressOverride || !isAddressValid ? ZGuid.Empty : docAddress.OrganisationPK;

				if (newValue != oldValue)
				{
					setBaseValueOnly(newValue);
				}
			}
		}

		bool IsSupplierDocumentaryAddressDefaultingSuspended => supplierDocumentaryAddressDefaultingIndex > 0;
		byte supplierDocumentaryAddressDefaultingIndex;
		IDisposable SuspendSupplierDocumentaryAddressDefaulting() => new DisposableAction(() => supplierDocumentaryAddressDefaultingIndex++, () => supplierDocumentaryAddressDefaultingIndex--);

		bool IsImporterDocumentaryAddressDefaultingSuspended => importerDocumentaryAddressDefaultingIndex > 0;
		byte importerDocumentaryAddressDefaultingIndex;
		IDisposable SuspendImporterDocumentaryAddressDefaulting() => new DisposableAction(() => importerDocumentaryAddressDefaultingIndex++, () => importerDocumentaryAddressDefaultingIndex--);

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					JE_EntryStatus = (ZString)JE_EntryStatusInfo.OriginalValue;
					JE_EntrySubmittedDate = (ZDateTime)JE_EntrySubmittedDateInfo.OriginalValue;
					JE_MessageStatus = (ZString)JE_MessageStatusInfo.OriginalValue;
				}
				else
				{
					JE_EntryStatus = ZString.Empty;
					JE_EntrySubmittedDate = ZDateTime.Empty;
					JE_MessageStatus = ZString.Empty;
					JE_UCR = ZString.Empty;
				}
			}
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				JE_UCRInfo.RefreshBinding();
			}
		}

		#endregion

		#region Declaration UCR

		[ReadOnlyMember(nameof(UCRReadOnly))]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.JobDeclaration|JE_UCR", Caption = "DUCR", FullDescription = "Declaration UCR")]
		public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }

		protected virtual bool UCRReadOnly => false;

		protected internal void PopulateUCRIfNeeded()
		{
			if (!IsInDatabase && JE_UCR.IsEmpty)
			{
				UpdateDucrIfNotLocked(true);
			}
		}

		public void UpdateDucrIfNotLocked(bool force = false) => UpdateDucrIfNotLockedCore(force);

		protected virtual void UpdateDucrIfNotLockedCore(bool force = false)
		{
			if (force || !UCRReadOnly)
			{
				JE_UCR = GetNewDUCR().Left(JE_UCRInfo.MaxLength);
			}
		}

		#endregion

		public const string DeclarationReferencePlaceHolder = "<<DECRF>>";  // Short so that the place holder doesn't occupy more characters than the job number for which it stands. Otherwise box 7 gets truncated because the literal string "<<SOMELONGPLACEHOLDER>>/ClientReferenceHere" is longer than "B00001000/ClientReferenceHere" and what we get in the file itself is "B00001000/ClientRefere"

		#region public SupportingDocumentCollection SupportingDocuments
		ISupportingDocumentCollection<SupportingDocument> ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get { return supportingDocuments ?? (supportingDocuments = GetSupportingDocuments()); }
		}
		SupportingDocumentCollection supportingDocuments;

		SupportingDocumentCollection GetSupportingDocuments()
		{
			var result = CreateNewSupportingDocumentCollection();

			if (MaxSupportingDocuments != -1)
			{
				result.EnableMaxCountValidationWithMessageError(MaxSupportingDocuments, warnAtHalfway: false, SupportingDocumentsValidationMessage);
			}

			result.Load();
			RegisterEditableChildObject(result);

			return result;
		}
		public virtual int MaxSupportingDocuments => -1;
		public virtual ZString SupportingDocumentsValidationMessage { get; }

		protected virtual SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		public ZBool IsInEuropeanCustomsUnionOrInheritsFromEU
		{
			get
			{
				var customsJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);
				return ObjectFactory.Get<Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(customsJurisdiction);
			}
		}
		#endregion

		#region public AdditionalInfoCollection AdditionalInfos
		IAdditionalInfoCollection<AdditionalInfo> IAdditionalInfosProvider.AdditionalInfos => AdditionalInfos;
		IAdditionalInfoValidationDecider IAdditionalInfosProviderWithValidationDecider.ValidationDecider => Configuration.GetAdditionalInfoValidationDecider(this);

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		AdditionalInfoCollection fAdditionalInfos;

		AdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		public IReadOnlyList<string> AdditionalInfoKeys => CreateEntryCreationStrategy().GetAdditionalInfoKeys();

		protected virtual AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);
		#endregion

		#region public PreviousDocumentCollection PreviousDocuments
		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments => fPreviousDocuments ?? (fPreviousDocuments = GetPreviousDocuments());
		PreviousDocumentCollection fPreviousDocuments;

		public IReadOnlyList<string> PreviousDocumentKeys => CreateEntryCreationStrategy().GetPreviousDocumentHeaderKeys();

		PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		IPreviousDocumentValidationDecider IPreviousDocumentsProviderWithValidationDecider.ValidationDecider => Configuration.GetPreviousDocumentValidationDecider(this);
		#endregion

		public const string CaptionKeySAD = "SAD8B4B9-01BB-4693-9B2F-64F745738CB4";
		public const string CaptionKeyUCC = "UCC55590-1B9F-40E3-ACED-4B8E75E10DB9";
		public const string CaptionKeyImportUCC5 = "IMPUCC51-BBE8-4E31-B488-E88DF7D1B4F0";
		public const string CaptionKeyExportUCC6 = "EXPUCC61-9438-4B0B-84D5-8C56700CD990";
		public const string CaptionKeyImportUCC6 = "IMPUCC63-26F2-41DA-8AFD-1F49A93B7BDF";
		public const string CaptionKeyBLT = "BLTC695B-C0EA-4888-92BD-0EFD5AEC8C9A";
		public const string CaptionKeyChargesExport = "C83ACD3D-D7DD-4C8A-8FB2-9A70BEEF2CF0";

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (JobDeclaration)base.CloneInternal(args);

			var supInfoArgs = CustomsBusinessObjectCloneArgs.GetCloneArgs(CloneType.DeepTemplateCopy,
																			typeof(CusSupportingInfo),
																			args.AlternativeFactoryToInstantiateCloneIn);

			foreach (SupportingDocument supportingDocument in SupportingDocuments.ToArray())
			{
				result.SupportingDocuments.Add(supportingDocument.Clone(supInfoArgs));
			}
			foreach (AdditionalInfo additionalInfo in AdditionalInfos.ToArray())
			{
				result.AdditionalInfos.Add(additionalInfo.Clone(supInfoArgs));
			}
			foreach (PreviousDocument previousDocument in PreviousDocuments.ToArray())
			{
				result.PreviousDocuments.Add(previousDocument.Clone(supInfoArgs));
			}
			return result;
		}

		#region Implementation

		public virtual EntryCreationStrategy CreateEntryCreationStrategy()
		{
			return new EntryCreationStrategy(this);
		}

		protected override bool IsUNDGSupportedOnInvoiceLines
		{
			get { return IsExport; }
		}

		protected override void DeriveDeclarationStatusCore()
		{
			DeriveCommonDeclarationStatus();
		}

		protected override void DeriveExportDeclarationStatus()
		{
			DeriveCommonDeclarationStatus();
		}

		protected override void DeriveImportDeclarationStatus()
		{
			DeriveCommonDeclarationStatus();
		}

		void DeriveCommonDeclarationStatus()
		{
			if (!DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(CountryCode, JE_GC))
			{
				DeclarationEntryStatusUpdater.Update(this);
			}
			DeclarationMessageStatusUpdater.Update(this);
			DeclarationEntrySubmittedDateUpdater.Update(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JE_PaymentMethod = ZString.Empty;
			JE_DeclarantType = GetDeclarantTypeByDefault();
			ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
			if (Branch?.OrgProxy is OrgHeader orgHeader)
			{
				SetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy(orgHeader);
			}
		}

		protected virtual void SetDefaultValuesForDeclarantAndRepresentativeWithOrgProxy(OrgHeader orgHeaderOfOrgProxy)
		{
			var proxyMainAddress = orgHeaderOfOrgProxy.MainAddress.PK;
			if (!proxyMainAddress.IsEmpty)
			{
				JE_OA_DeclarantAddress = proxyMainAddress;
			}
		}

		public ZString TransportIDLabel => GetTransportIDLabel();

		protected virtual ZString GetTransportIDLabel()
		{
			return Res.GetString("6AE8B83B-B93D-46FA-BF43-7972BC7680AF", "[21] Transport ID");
		}

		[ResourceStringData("fd21d354-b5aa-4c91-942c-220a903424f2", Caption = "Flight", IsApplicableMember = nameof(IsAir))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		[MaxLength(nameof(JE_OwnerRefMaxLength))]
		[ResourceStringData("70e0b711-c0a2-4169-92f7-1fd7b0ca19ef", Caption = "[7] Declarant\'s Ref", ShortCaption = "[7] Dec. Ref")]
		public override ZString JE_OwnerRef
		{
			get => base.JE_OwnerRef;
			set => base.JE_OwnerRef = value;
		}

		protected virtual int JE_OwnerRefMaxLength => 21;

		public ZString TradersOwnReferenceFullForBox7
		{
			get { return TradersOwnReferenceFullForBox7Core; }
		}

		protected virtual ZString TradersOwnReferenceFullForBox7Core
		{
			get { return JE_OwnerRef.IsEmpty ? JE_DeclarationReference : JE_OwnerRef; }
		}

		ZString GetNewDUCR()
		{
			var result = ZString.Empty;
			var branch = Branch;
			if (branch != null)
			{
				PopulateJE_DeclarationReferenceIfNeeded();
				ZString year = ZDate.Today.Year.ToString();
				result = GetFormattedDUCR(year.Right(1), GetEoriForDucr(), GetDucrSecondHalfReferenceNumber());
			}
			return result;
		}

		protected virtual ZString GetFormattedDUCR(ZString year, ZString eori, ZString reference)
		{
			return string.Format("{0}{1}-{2}", year, eori, reference);
		}

		/// <summary>
		/// a) Goods imported to EU from third country, then reexported to another third country. The import record will have a transhipment request, the serial number for this must be used in the export declaration's DUCR. This links the thorugh movements.
		/// b) If plain export, generate a DUCR from the job or client reference number.
		/// </summary>
		ZString GetDucrSecondHalfReferenceNumber()
		{
			var clientOrDeclarationNumber = (ClientReferenceForDucr.IsEmpty ? JE_DeclarationReference : ClientReferenceForDucr);
			var baseHawb = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, PK));
			return (baseHawb != null && !baseHawb.CS_TranshipmentEntryNum.IsEmpty && IsExport)
				? baseHawb.CS_TranshipmentEntryNum  //(a)
				: clientOrDeclarationNumber;        //(b)
		}

		#region Government Contractor DocAddress
		public JobDocAddress GovernmentContractorDocAddress
		{
			get
			{
				if (governmentContractorDocAddress == null || governmentContractorDocAddress.IsDeleted)
				{
					governmentContractorDocAddress = DocAddresses.FindOrCreateWithRequirement(GovernmentContractorDocAddressRequirement);
				}
				return governmentContractorDocAddress;
			}
		}
		JobDocAddress governmentContractorDocAddress;

		JobDocAddressRequirement GovernmentContractorDocAddressRequirement
		{
			get
			{
				if (governmentContractorDocAddressRequirement == null)
				{
					governmentContractorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.GovernmentContractor, ContactType.Administration);
					DocAddressManager.AddRequirement(governmentContractorDocAddressRequirement);
				}
				return governmentContractorDocAddressRequirement;
			}
		}
		JobDocAddressRequirement governmentContractorDocAddressRequirement;
		#endregion

		#region Place Of Loading DocAddress
		public JobDocAddress PlaceOfLoadingDocAddress
		{
			get
			{
				if (placeOfLoadingDocAddress == null || placeOfLoadingDocAddress.IsDeleted)
				{
					placeOfLoadingDocAddress = DocAddresses.FindOrCreateWithRequirement(PlaceOfLoadingDocAddressRequirement);
				}
				return placeOfLoadingDocAddress;
			}
		}
		JobDocAddress placeOfLoadingDocAddress;

		JobDocAddressRequirement PlaceOfLoadingDocAddressRequirement
		{
			get
			{
				if (placeOfLoadingDocAddressRequirement == null)
				{
					placeOfLoadingDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CustomsPlaceOfLoading, ContactType.Administration);
					DocAddressManager.AddRequirement(placeOfLoadingDocAddressRequirement);
				}
				return placeOfLoadingDocAddressRequirement;
			}
		}
		JobDocAddressRequirement placeOfLoadingDocAddressRequirement;

		#endregion

		protected override DocAddressType[] SupportedAddressTypesCore
		{
			get
			{
				var result = new List<DocAddressType>(base.SupportedAddressTypesCore);
				result.Add(DocAddressType.CustomsSupervisingOffice);
				result.Add(DocAddressType.GovernmentContractor);
				result.Add(DocAddressType.CustomsPlaceOfLoading);
				result.Add(DocAddressType.SellingParty);
				result.Add(DocAddressType.Exporter);
				result.Add(DocAddressType.Representative);
				result.Add(DocAddressType.DefermentParty);
				result.Add(DocAddressType.ContractualPartner);
				result.Add(DocAddressType.Carrier);
				return result.ToArray();
			}
		}

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			var result = base.GetDocAddressRequirement(addressType);
			if (result == null)
			{
				switch (addressType)
				{
					case DocAddressType.CustomsSupervisingOffice:
						result = SupervisingOfficeDocAddressRequirement;
						result.GetRegistrationNumberResult =
							(JobDocAddress docAddress) =>
							{
								return new RegistrationNumberResult(docAddress.Factory, true, delegate
								{
									OrgHeader org = SupervisingOfficeDocAddress.Organisation;
									ZString traderID = org != null ? org.GetEuIdentificationNumber() : ZString.Empty;
									return new RegistrationNumber() { Number = traderID, NumberType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori };
								});
							};
						break;
					case DocAddressType.GovernmentContractor:
						result = GovernmentContractorDocAddressRequirement;
						break;

					case DocAddressType.CustomsPlaceOfLoading:
						result = PlaceOfLoadingDocAddressRequirement;
						result.GetRegistrationNumberResult =
							(JobDocAddress docAddress) =>
							{
								return new RegistrationNumberResult(docAddress.Factory, true, delegate
								{
									return new RegistrationNumber() { Number = PlaceOfLoadingDocAddress.Address != null ? PlaceOfLoadingDocAddress.Address.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.BrokerageSiteID, Core.Constants.CountryCodes.Germany) : ZString.Empty, NumberType = OrgCusCode.CodeTypes.BrokerageSiteID };
								});
							};
						break;
					case DocAddressType.SellingParty:
						result = SellingPartyDocAddressRequirement;
						break;
					case DocAddressType.Exporter:
						result = ExporterDocAddressRequirement;
						break;
					case DocAddressType.Representative:
						result = RepresentativeDocAddressRequirement;
						break;
					case DocAddressType.DefermentParty:
						result = DefermentPartyDocAddressRequirement;
						break;
				}
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation() => new JobDeclarationValidation(this);

		public override void Delete()
		{
			using (GetDV1DetailLineNumberRenumberingSuspender())
			{
				DV1Details.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		public DeclarationConfiguration Configuration => CachedValueHelper.GetValue(ref configuration, () => DeclarationConfiguration.GetConfiguration(Factory, GetDefaultDataGroupingCode()));
		CachedValue<DeclarationConfiguration> configuration;

		public ZBool DV1DetailsSupport => Configuration.DV1DetailsSupport(this);

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.JobDeclarationFetchStrategy(this);
		}

		void EmptyTaxTypeForAllInvoiceLinesIfNotImport() => InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.EmptyTaxTypeIfNotImport());

		#endregion

		public ZString BarrierPort => BarrierPortCore;

		protected virtual ZString BarrierPortCore => IsExport ? JE_RL_NKPortOfLoading : (IsImport ? JE_RL_NKPortOfArrival : ZString.Empty);

		protected override void SetPortOfFirstArrival(ZString arrival)
		{
			if (IsFirstArrivalDateAndPortUsed && JE_RL_NKPortOfFirstArrival.IsEmpty)
			{
				JE_RL_NKPortOfFirstArrival = arrival;
			}
		}

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Header;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
			foreach (ICanBeImportOrExport invoiceHeader in Invoices)
			{
				invoiceHeader.ValidatePreviousDocuments();
			}
		}

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;
		string ICanBeImportOrExport.DataGroupingCode => GetDefaultDataGroupingCode();

		protected override IValueSetStrategy GetValueSetStrategy() => new JobDeclarationValueSetStrategy(this);

		public ZString SupplierTraderId => SupplierTraderIdCore;

		protected virtual ZString SupplierTraderIdCore
		{
			get
			{
				OrgHeader supplier = Supplier;
				if (supplier != null)
				{
					return supplier.GetEuIdentificationNumber();
				}
				return ZString.Empty;
			}
		}

		public ZString ImporterTraderId => ImporterTraderIdCore;

		protected virtual ZString ImporterTraderIdCore
		{
			get
			{
				OrgHeader importer = Importer;
				if (importer != null)
				{
					return importer.GetEuIdentificationNumber();
				}
				return ZString.Empty;
			}
		}

		public ZString DeclarantTraderId
		{
			get
			{
				if (Declarant == null)
				{
					return ZString.Empty;
				}

				OrgHeader orgDeclarant = Factory.Load<OrgHeader>(Declarant.OA_OH);
				if (orgDeclarant != null)
				{
					return orgDeclarant.GetEuIdentificationNumber();
				}
				return ZString.Empty;
			}
		}

		/// <summary>
		/// Will return either the declarant entered (in box 14, misc tab) or the branch OrgProxy
		/// </summary>
		public OrgAddress Declarant
		{
			get
			{
				OrgAddress orgDeclarant = DeclarantOrgAddress;
				if (orgDeclarant != null)
				{
					return orgDeclarant;
				}

				GlbBranch branch = Branch;
				if (branch != null)
				{
					if (branch.OrgProxy != null)
					{
						return branch.OrgProxy.MainAddress;
					}
				}
				return null;
			}
		}

		[ResourceStringData("6C96D8FE-688C-4C93-8FD3-A380FD699512", Caption = "MRN", MultipleKey = CaptionKeyBLT)]
		[ResourceStringData("9B5BCAC8-074A-4A22-BB28-17615556A8BF", Caption = "Entry Number")]
		public override ZString DeclarationNumber
		{
			get => base.DeclarationNumber;
			set => base.DeclarationNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BorderTransportMeansList))]
		public override ZString JE_BorderTransportMeans
		{
			get => base.JE_BorderTransportMeans;
			set => base.JE_BorderTransportMeans = value;
		}

		[ResourceStringData("014F33AA-EC0B-4E00-A6A2-AB8887CFB85F", Caption = "Border T.O.ID.")]
		public override ZString ZG_BorderTransportMeans
		{
			get => base.ZG_BorderTransportMeans;
			set => base.ZG_BorderTransportMeans = value;
		}

		/// <summary>
		/// Converted from CW (AIR, SEA) into EU numeric codes (4, 1, etc)
		/// </summary>
		public ZString ModeOfTransportAtTheBorder => TransportModeTranslator.TranslateToWCOCode(JE_TransportMode);

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
					return TransportTypeGenericList.Codes.Air;
				case TransportTypeList.Codes.Sea:
					return TransportTypeGenericList.Codes.Sea;
				case TransportTypeList.Codes.Rail:
					return TransportTypeGenericList.Codes.Rail;
				case TransportTypeList.Codes.Road:
					return TransportTypeGenericList.Codes.Road;
				case TransportTypeList.Codes.FixedTransportInstallations:
					return TransportTypeGenericList.Codes.Other;
				case TransportTypeList.Codes.OwnPropulsion:
					return TransportTypeGenericList.Codes.Other;
				case TransportTypeList.Codes.InlandWaterwayTransport:
					return TransportTypeGenericList.Codes.Sea;
				case TransportTypeList.Codes.Mail:
					return TransportTypeGenericList.Codes.PostMail;
				default:
					return ZString.Empty;
			}
		}

		public virtual ZString RepresentationTypeNo
		{
			get
			{
				switch (JE_DeclarantType.ToString())
				{
					case RepresentationTypeList.Codes._1Self:
						return "1";
					case RepresentationTypeList.Codes._2Direct:
						return "2";
					case RepresentationTypeList.Codes._3Indirect:
						return "3";
					default:
						return "";
				}
			}
		}

		public bool IsNCTS => JE_MessageType == "NCT"; //MessageTypeList.Codes.NCTSMovement

		public bool IsEMCS => JE_ApplicationCode == "EMC";

		public CusEntryHeader FirstActiveEntryHeaderWithEntryNum => ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => !x.EntryNumber.IsEmpty);

		protected override LandedCostingHelper GetLandedCostingHelperCore() => new EuLandedCostingHelper();

		#region DUCR generation helpers

		ZString GetEoriForDucr()
		{
			var result = ZString.Empty;

			if (Declarant is OrgAddress declarant)
			{
				var party = declarant.Header;
				var partyAddress = declarant;
				if (UseClientEoriForDucr)
				{
					if (IsImport)
					{
						var importer = Importer;
						if (importer != null)
						{
							var importerAddress = ImporterAddress ?? importer.MainAddress;
							party = importerAddress?.Header;
							partyAddress = importerAddress;
						}
					}
					else if (IsExport)
					{
						var supplier = Supplier;
						if (supplier != null)
						{
							var supplierAddress = SupplierAddress ?? supplier.MainAddress;
							party = supplierAddress?.Header;
							partyAddress = supplierAddress;
						}
					}
				}

				if (partyAddress != null)
				{
					result = partyAddress.GetEuIdentificationNumber();
				}
				else if (party != null)
				{
					result = party.GetEuIdentificationNumber();
				}
			}
			return result;
		}

		public bool DucrGenerationOptionsReadOnly
		{
			get { return !JE_UCR.IsEmpty && UCRReadOnly; }
		}

		[ReadOnlyMember(nameof(DucrGenerationOptionsReadOnly))]
		public ZBool UseClientEoriForDucr
		{
			get { return JE_UseOwnerRefAsQuarantineRef; }
			set
			{
				JE_UseOwnerRefAsQuarantineRef = value;
				UpdateDucrIfNotLocked();
			}
		}
		public ZPropertyInfo UseClientEoriForDucrInfo
		{
			get { return GetZPropertyInfo("JE_UseOwnerRefAsQuarantineRef", "Use Client EORI for DUCR"); }
		}

		[ReadOnlyMember(nameof(DucrGenerationOptionsReadOnly))]
		[MaxLength(19)] // 35 chars (length of DUCR) less preamble (1 (year), 14 (EORI), 1 (hyphen) = 16). 35-16=19.
		public ZString ClientReferenceForDucr
		{
			get { return JE_Folio; }
			set
			{
				var valueToSet = value.Replace("/", "");   // slash means part suffix
				CheckMaximumLength(ClientReferenceForDucrInfo, valueToSet);
				JE_Folio = valueToSet;
				UpdateDucrIfNotLocked();
				ClientReferenceForDucrInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ClientReferenceForDucrInfo
		{
			get { return GetZPropertyInfo(nameof(ClientReferenceForDucr), "Client Reference for DUCR"); }
		}

		#endregion

		public EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstander(CusEntryHeader header) => GetEntryFeePaymentPartyUnderstanderCore(header);

		protected virtual EntryFeePaymentPartyUnderstander GetEntryFeePaymentPartyUnderstanderCore(CusEntryHeader header) => null;

		public IStatusChecker GetStatusChecker() => GetStatusCheckerCore();

		protected virtual IStatusChecker GetStatusCheckerCore() => null;

		public JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidation(JobComInvoiceHeader jobComInvoiceHeader)
			=> GetJobComInvoiceHeaderValidationCore(jobComInvoiceHeader);

		protected virtual JobComInvoiceHeaderValidation GetJobComInvoiceHeaderValidationCore(JobComInvoiceHeader jobComInvoiceHeader)
			=> new JobComInvoiceHeaderValidation(jobComInvoiceHeader);

		public JobComInvoiceLineValidation GetJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineValidationCore(invoiceLine);
		protected virtual JobComInvoiceLineValidation GetJobComInvoiceLineValidationCore(JobComInvoiceLine invoiceLine) => new JobComInvoiceLineValidation(invoiceLine);

		public JobComInvoiceLineLookups GetJobComInvoiceLineLookups(JobComInvoiceLine invoiceLine) => GetJobComInvoiceLineLookupsCore(invoiceLine);
		protected virtual JobComInvoiceLineLookups GetJobComInvoiceLineLookupsCore(JobComInvoiceLine invoiceLine) => new JobComInvoiceLineLookups(invoiceLine);

		public CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader) => GetCusEntryHeaderDocumentSupporterCore(entryHeader);
		protected virtual CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporterCore(CusEntryHeader entryHeader) => new CusEntryHeaderDocumentSupporter(entryHeader);

		public CusEntryHeaderValidation GetCusEntryHeaderValidation(CusEntryHeader entryHeader) => GetCusEntryHeaderValidationCore(entryHeader);
		protected virtual CusEntryHeaderValidation GetCusEntryHeaderValidationCore(CusEntryHeader entryHeader) => new CusEntryHeaderValidation(entryHeader);

		public EUAddInfoValidation GetAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => GetAddInfoJobComInvoiceLineValidationCore(addInfoJobComInvoiceLine);
		protected virtual EUAddInfoValidation GetAddInfoJobComInvoiceLineValidationCore(AddInfoJobComInvoiceLine addInfo) => new AddInfoJobComInvoiceLineValidation(addInfo);

		public IJobComInvoiceLineValueCalculator GetJobComInvoiceLineCalculator(JobComInvoiceLine jobComInvoiceLine) => GetJobComInvoiceLineCalculatorCore(jobComInvoiceLine);
		protected virtual IJobComInvoiceLineValueCalculator GetJobComInvoiceLineCalculatorCore(JobComInvoiceLine invoiceLine) => new JobComInvoiceLineValueCalculator(invoiceLine);

		public IValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategy(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => GetAddInfoJobComInvoiceLineValueSetStrategyCore(addInfoJobComInvoiceLine);
		protected virtual IValueSetStrategy GetAddInfoJobComInvoiceLineValueSetStrategyCore(AddInfoJobComInvoiceLine addInfoJobComInvoiceLine) => null;

		public IValueSetStrategy GetJobComInvoiceLineValueSetStrategy(JobComInvoiceLine line) => GetJobComInvoiceLineValueSetStrategyCore(line);
		protected virtual IValueSetStrategy GetJobComInvoiceLineValueSetStrategyCore(JobComInvoiceLine line) => new JobComInvoiceLineValueSetStrategy();

		internal IValueSetStrategy GetSupportingDocumentValueSetStrategy(SupportingDocument supportingDocument) => GetSupportingDocumentValueSetStrategyCore(supportingDocument);
		protected virtual IValueSetStrategy GetSupportingDocumentValueSetStrategyCore(SupportingDocument supportingDocument) => null;

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes() => GetCusAddInfoTypesCore();
		protected virtual IDictionary<ZString, Type> GetCusAddInfoTypesCore() => new Dictionary<ZString, Type>();

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();
		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			result.Add(CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument));
			result.Add(CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		protected void SetDefaultBorderTransportToIDForTransportMode()
		{
			if (ShouldSetDefaultBorderTransportToIDForTransportMode())
			{
				if (!AddInfoLookups.BorderTransportMeansList.ContainsCode(ZG_BorderTransportMeans))
				{
					ZG_BorderTransportMeans = AddInfoLookups.BorderTransportMeansList.DefaultCode ?? ZString.Empty;
				}
				else if (AddInfoLookups.BorderTransportMeansList.DefaultCode == null)
				{
					ZG_BorderTransportMeans = ZString.Empty;
				}
			}
		}

		protected virtual bool ShouldSetDefaultBorderTransportToIDForTransportMode() => Configuration.IsUCC6(this);

		protected override bool SupportEquipmentsCore => true;
		protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;
		protected override bool SupportsJobComInvoiceLineTaxCore => true;

		protected override ZString PackageMarksAndNumbersAlwaysRequiredValidationMessageCore
		{
			get
			{
				return Shipment != null
					? Res.GetString("EU.PackageMarksAndNumbersAlwaysRequiredValidationMessageCore.Shipment", "Package marks are required.  When this field is read only, it is not necessary to override the default values from the shipment to make this field editable.  Instead package marks should be supplied on the shipment's packing tab.")
					: Res.GetString("EU.PackageMarksAndNumbersAlwaysRequiredValidationMessageCore.Standalone", "Package marks are required.");
			}
		}
		protected override bool SupportUseOwnerRefAsQuarantineRefUsageCore => true;

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}

		protected override Customs.Business.MostInterestingLegProvider MostInterestingLegProviderCore => new MostInterestingLegProvider(this);

		protected override Customs.Business.BondedWarehousingHelper GetNewBondedWarehousingHelper()
		{
			return new BondedWarehousingHelper(this);
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		protected override bool ShouldUpdateOutwardLinesWithInventoryDetailsCore => IsInventorySelectionEnabled;

		protected override bool SupportInwardProcessingCore => SupportsBondedWarehousingCore;

		protected override bool IsInventorySelectionEnabledCore => Factory.GetValue(ref isInventorySelectionEnabledCore, () => SupportsBondedWarehousingCore
							&& CustomsEntryInstructions.Any(cei => cei.Warehouse?.Header.CompanyData is OrgCompanyData companyData
							&& (companyData.OB_IMUsedBondedWhs
							|| companyData.OB_CusInventoryForTemporaryImports
							|| companyData.OB_CusInventoryForTemporaryExports
							|| companyData.OB_CusInventoryForInwardProcessing
							|| companyData.OB_CusInventoryForOutwardProcessing
							)));
		CachedProperty<bool> isInventorySelectionEnabledCore;

		protected override bool IsWarehouseOrderFunctionActivatedCore => CustomsDataRegistry.Instance.SupportWarehouseOrderLines.Value;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch use is justifiable")]
		protected override ZString GetContainerModeForDeclarationCore(ZString transportMode, ZString shipmentPackingMode)
		{
			var result = ZString.Empty;

			switch (shipmentPackingMode)
			{
				case Core.Constants.ContainerModes.FCL:
				case Core.Constants.ContainerModes.LCL:
				case Core.Constants.ContainerModes.BreakBulk:
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
				case Core.Constants.ContainerModes.RollOnRollOff:
				case Core.Constants.ContainerModes.ULD:
				case Core.Constants.ContainerModes.Loose:
				case Core.Constants.ContainerModes.FTL:
				case Core.Constants.ContainerModes.LTL:
					result = shipmentPackingMode;
					break;

				case Core.Constants.ContainerModes.Containerised:
					result = Core.Constants.ContainerModes.Containerised;
					break;

				case Core.Constants.ContainerModes.BuyersConsol:
				case Core.Constants.ContainerModes.AgentConsol:
				case Core.Constants.ContainerModes.ShippersConsol:
					result = IsAir ? Core.Constants.ContainerModes.NonContainerised : Core.Constants.ContainerModes.Containerised;
					break;

				default:
					result = Core.Constants.ContainerModes.NonContainerised;
					break;
			}

			return result;
		}

		public bool IsWarehouseNeeded => InvoiceLines.OfType<JobComInvoiceLine>().Any(i => i.IsIntoOrOutOfRegimeProcedure);

		#region CusEntryInstruction and JE_DeclarationType - make sure to only create CEI on set, not on get, so that it can be deleted when JE_DeclarationType is set to blank.

		public override void MakeNonPersistent()
		{
			base.MakeNonPersistent();
			var cei = CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().FirstOrDefault();
			if (cei != null)
			{
				cei.MakeNonPersistent();  // Just to make some stupid standalone invoice form basher tests shut up
			}

			AddInfoChild.MakeNonPersistent();
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return GetCusCodeDataTypesCore();
		}

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CountryOfRoutingCode, typeof(ItineraryCountry));
			result.Add(CusCodeDataTypeList.Codes.OfficeCode, typeof(EuOfficeCode));
			result.Add(CusCodeDataTypeList.Codes.TransportInland, typeof(InlandTransport));
			return result;
		}

		#endregion

		public ZString Box30LocationOfGoodsForDocumentsAndMessaging => Box30LocationOfGoodsForDocumentsAndMessagingCore;

		protected virtual ZString Box30LocationOfGoodsForDocumentsAndMessagingCore
		{
			get
			{
				var location = JE_LocationOfGoods.IsEmpty ? ZString.Empty : new ZString(CountryCode + JE_LocationOfGoods);
				return location + SubLocation;
			}
		}

		public ZString Box18IdentityOfTransportAtDepartureForDocumentsAndMessaging => Box18IdentityOfTransportAtDepartureForDocumentsAndMessagingCore;

		protected virtual ZString Box18IdentityOfTransportAtDepartureForDocumentsAndMessagingCore => IsExport ? ZG_Box18TransportID : ZString.Empty;

		#region VATDefer

		public VATDeferStrategy VATDeferStrategy => GetVATDeferStrategyCore();

		protected virtual VATDeferStrategy GetVATDeferStrategyCore() => new VATDeferStrategy(this);

		[MaxLength(1)]
		[ResourceStringData("EU.JobDeclaration.JE_PaymentMethod", Caption = "Payment Party")]
		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set => base.JE_PaymentMethod = value;
		}

		[MaxLength(JobDeclaration.Schema.ZG_VATDeferTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeferTypeList))]
		public override ZString JE_VATDeferType
		{
			get => base.JE_VATDeferType;
			set => base.JE_VATDeferType = value;
		}

		[ResourceStringData("EU.JobDeclaration.ZG_VATDeferType", Caption = "VAT")]
		public override ZString ZG_VATDeferType
		{
			get => base.ZG_VATDeferType;
			set => base.ZG_VATDeferType = value;
		}

		#endregion

		#region CustomsOfficeRequirementHelper

		public CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		JobDeclarationCustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

		#endregion

		public CusGuaranteeHeader CustomsGuarantee => GetCustomsGuaranteeCore;

		protected virtual CusGuaranteeHeader GetCustomsGuaranteeCore => null;

		public Money AmountToGuarantee
		{
			get
			{
				var entries = ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_EntryStatus != EntryStatusList.Codes.Cancelled && x.CH_EntryStatus != EntryStatusList.Codes.Clear).ToArray();
				var guarantees = entries.SelectMany(x => x.AmountAndTypeToBeGuaranteeds, (ce, a) => new { ce, a });
				var amountToGuarantee = Money.Empty;

				foreach (var amount in guarantees)
				{
					amountToGuarantee = amount.ce.CurrencyConverter.Add(amountToGuarantee, new Money(amount.a.AmountInDeclarationCurrency, amount.ce.LocalCurrency));
				}

				return amountToGuarantee;
			}
		}

		public Money RemainingGuaranteeBalance => new Money(CustomsGuarantee?.CPH_Calc_TotalBalanceIncludingPending.Amount ?? ZDecimal.Zero, RefCurrency.LoadFromCurrencyCode(Factory, CustomsGuarantee?.CPH_UnitOfMeasure ?? ZString.Empty) ?? LocalCurrency);

		#region DV1Details

		[ChildEditable(true)]
		public CusDV1DetailCollection DV1Details => fDV1Details ?? (fDV1Details = GetDV1Details());
		CusDV1DetailCollection fDV1Details;

		CusDV1DetailCollection GetDV1Details()
		{
			var result = CreateNewDV1DetailsCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusDV1DetailCollection CreateNewDV1DetailsCollection() => new CusDV1DetailCollection(this);

		#endregion

		#region DV1DetailLineNumberGenerator

		public IEnumerable<IShortSequenceNumberLine> DV1DetailLines => new TypedEnumerable<IShortSequenceNumberLine>(DV1Details);

		public IDisposable GetDV1DetailLineNumberRenumberingSuspender() => DV1DetailLineNumberGenerator.GetLineNumberSuspender();

		public ShortSequenceNumberGenerator DV1DetailLineNumberGenerator => dv1DetailLineNumberGenerator ?? (dv1DetailLineNumberGenerator = new ShortSequenceNumberGenerator(() => DV1DetailLines));

		ShortSequenceNumberGenerator dv1DetailLineNumberGenerator;

		#endregion

		public TransportModeTranslator TransportModeTranslator => transportModeTranslator ?? (transportModeTranslator = GetTransportModeTranslator());

		protected virtual TransportModeTranslator GetTransportModeTranslator() => new TransportModeTranslator();

		TransportModeTranslator transportModeTranslator;

		public CusAuthorizationUsageUpdater AuthorizationUsageUpdater => GetNewCusAuthorisationUsageUpdaterCore();

		protected virtual CusAuthorizationUsageUpdater GetNewCusAuthorisationUsageUpdaterCore() => new CusAuthorizationUsageUpdater(this);

		protected override DeclarationDocManagerInfo GetNewDocManagerInfo() => new EUDeclarationDocManagerInfo(this);

		public class EUDeclarationDocManagerInfo : DeclarationDocManagerInfo
		{
			public EUDeclarationDocManagerInfo(JobDeclaration parent)
				: base(parent)
			{
			}

			JobDeclaration Declaration
			{
				get { return (JobDeclaration)BusinessEntity; }
			}

			protected override BusinessObject[] GetEDocsChildrenForAFreightJobToDisplayCore()
			{
				var result = base.GetEDocsChildrenForAFreightJobToDisplayCore().ToList();

				var exitDetailQuery = new ZDBOnlyQuery(typeof(CusExitDetail));
				var exitControlHeaderFilter = new ZDBOnlySubQuery(typeof(CusExitControlHeader), CusExitControlHeaderSchema.PK);
				exitControlHeaderFilter.AddToFilter(CusExitControlHeaderSchema.CEH_ParentID, Declaration.PK);
				exitDetailQuery.AddSubQuery(CusExitDetailSchema.CED_CEH, exitControlHeaderFilter, JoinCondition.And);

				result.AddRange(Declaration.Factory.Load<CusExitDetail>(exitDetailQuery));
				result.AddRange(Declaration.ExitReports.Cast<BusinessObject>());

				return result.ToArray();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CommunityTransitStatusIDList))]
		public override ZString JE_CTStatusID { get => base.JE_CTStatusID; set => base.JE_CTStatusID = value; }

		[ResourceStringData("EUJobDeclarationUserControl|88952fb9-53a2-4532-94a1-fd63c7422162", Caption = "CT Status", MediumCaption = "CT Status", ShortCaption = "CT Status")]
		public override ZString ZG_CTStatusID
		{
			get => base.ZG_CTStatusID;
			set
			{
				var oldValue = ZG_CTStatusID;
				base.ZG_CTStatusID = value;
				if (!IsCopying && oldValue != ZG_CTStatusID)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_RL_NKPortOfLoading();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SecurityTypeList))]
		public override ZString JE_TypeOfSecurity { get => base.JE_TypeOfSecurity; set => base.JE_TypeOfSecurity = value; }

		[ResourceStringData("290884C3-A415-48F7-AC84-F44473AB0B94", Caption = "Security", FullDescription = "[11 07 000 000] Security")]
		public override ZString ZG_TypeOfSecurity { get => base.ZG_TypeOfSecurity; set => base.ZG_TypeOfSecurity = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SpecificCircumstanceIndicatorList))]
		public override ZString JE_SpecificCircumstanceIndicator { get => base.JE_SpecificCircumstanceIndicator; set => base.JE_SpecificCircumstanceIndicator = value; }

		[ResourceStringData("7A3E0341-7E40-4508-B34A-84F240F549CC", Caption = "Circumstance")]
		public override ZString ZG_SpecificCircumstanceIndicator { get => base.ZG_SpecificCircumstanceIndicator; set => base.ZG_SpecificCircumstanceIndicator = value; }

		[ResourceStringData("EUJobDeclaration|JE_TotalWeight", Caption = "Total Weight", ShortCaption = "Weight")]
		public override ZDecimal JE_TotalWeight { get => base.JE_TotalWeight; set => base.JE_TotalWeight = value; }

		[ResourceStringData("EUJobDeclaration|JE_TotalVolume", Caption = "Total Volume", MediumCaption = "Volume", ShortCaption = "Vol")]
		public override ZDecimal JE_TotalVolume { get => base.JE_TotalVolume; set => base.JE_TotalVolume = value; }

		protected virtual bool ZG_StyleOfEntrySOE_ReadOnly => true;
		protected virtual bool ZG_Gateway_ReadOnly => true;

		public void ClearInvoiceValuesIfSame(IZType decValue, string invoiceFieldName)
		{
			if (IsPersistent)
			{
				EffectiveValueManager.ClearValueIfSame(decValue, invoiceFieldName, Invoices.Cast<JobComInvoiceHeader>());
			}
		}

		public void ClearInvoiceLineValuesIfSame(IZType decValue, string invoiceLineFieldName)
		{
			if (IsPersistent)
			{
				EffectiveValueManager.ClearValueIfSame(decValue, invoiceLineFieldName, InvoiceLines.Cast<JobComInvoiceLine>());
			}
		}

		#region InlandTransportLineNumberGenerator

		public IEnumerable<IShortSequenceNumberLine> InlandTransportLines => new TypedEnumerable<IShortSequenceNumberLine>(InlandTransports);

		public IDisposable GetInlandTransportLineNumberRenumberingSuspender() => InlandTransportLineNumberGenerator.GetLineNumberSuspender();

		public ShortSequenceNumberGenerator InlandTransportLineNumberGenerator => inlandTransportLineNumberGenerator ?? (inlandTransportLineNumberGenerator = new ShortSequenceNumberGenerator(() => InlandTransportLines));
		ShortSequenceNumberGenerator inlandTransportLineNumberGenerator;

		#endregion

		[ResourceStringData("EUJobDeclaration|ZG_IsSecurityDeclaration", Caption = "Security")]
		public override ZBool ZG_IsSecurityDeclaration { get => base.ZG_IsSecurityDeclaration; set => base.ZG_IsSecurityDeclaration = value; }

		void EmptyIsSecurityDeclarationIfNecessary()
		{
			if (!IsSecurityAllowed())
			{
				ZG_IsSecurityDeclaration = false;
			}
		}

		public ZBool IsSecurityAllowed() => IsSecurityAllowedCore();

		protected virtual ZBool IsSecurityAllowedCore() => IsUCC6AndIsExport;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Box18TransportCountryList))]
		public override ZString JE_Box18TransportNationality { get => base.JE_Box18TransportNationality; set => base.JE_Box18TransportNationality = value; }

		[ResourceStringData("5FD41D36-AF77-4F0B-BC00-2CA35AF30679", Caption = "[18] Nationality")]
		public override ZString ZG_Box18TransportNationality { get => base.ZG_Box18TransportNationality; set => base.ZG_Box18TransportNationality = value; }

		[ResourceStringData("26322682-5D29-4DB8-B212-F6D0372C5FFE", Caption = "Duty Payer")]
		[ResourceStringData("CCA0C92E-3FC9-42D3-BD10-FFCDCD3E5916", Caption = "Duty Payer", FullDescription = "[13 21 000 000] Person paying the customs duty", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid JE_OH_DutyPayer { get => base.JE_OH_DutyPayer; set => base.JE_OH_DutyPayer = value; }

		protected override Type GoodsLocationTypeCore => CusGoodsLocation.TypeDecider.GetTypeForCountryCode(CountryCode);

		public bool SupportsCalculateInsurance => SupportsCalculateInsuranceCore;

		protected virtual bool SupportsCalculateInsuranceCore => IsImport;

		#region IUcc6ValueProvider

		bool IUcc6ValueProvider.IsExport => IsExport;

		bool IUcc6ValueProvider.IsImport => IsImport;

		#endregion

		#region IValidationModesSupporter

		public void RecalculateValidationModesOnAllLevel()
		{
			CustomsEntryInstructions.ForEach(x => x.ValidationModesCalculator.RecalculateValidationModes());
			ValidationModesCalculator.RecalculateValidationModes();
		}

		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					ValidationModesCalculator.RecalculateValidationModes();
				}
				return fValidationModes.Value;
			}
			set
			{
				fValidationModes = value;
			}
		}
		ValidationModes? fValidationModes;

		public DeclarationValidationModesCalculator ValidationModesCalculator => validationModesCalculator ?? (validationModesCalculator = CreateNewValidationModesCalculator());
		DeclarationValidationModesCalculator validationModesCalculator;

		protected virtual DeclarationValidationModesCalculator CreateNewValidationModesCalculator() => new DeclarationValidationModesCalculator(this);

		public void ValidateGoodsLocationDescription()
		{
			ValidateGoodsLocationDescriptionCore();
		}

		public bool IsOriginalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Original); }
		}

		public bool IsAmendmentValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Amendment); }
		}
		#endregion

		#region ICusGoodsLocationProvider

		public CusGoodsLocation GoodsLocation
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = GetGoodsLocation();
					RegisterEditableChildObject(goodsLocation);
				}
				return goodsLocation;
			}
		}
		protected CusGoodsLocation goodsLocation;

		protected virtual CusGoodsLocation GetGoodsLocation() => Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Declaration);

		[ResourceStringData("081ACF7D-9225-43D8-A25C-6A44276B2A1B", Caption = "Location of Goods")]
		public ZString GoodsLocationDescription => GoodsLocation?.DisplayText ?? ZString.Empty;

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		void ICusGoodsLocationProvider.ValidateGoodsLocationDescription()
		{
			ValidateGoodsLocationDescriptionCore();
		}

		public virtual void ValidateGoodsLocationDescriptionCore()
		{
		}

		ZString ICusGoodsLocationProvider.ProviderKey => CountryCode + GoodsLocationProviderApplications.Codes.JobDeclaration;

		#endregion

		#region MapSelectedInventoryFromTS
		public bool MapSelectedInventoryFromTS(CusTempStorageSelectableRegLineCollection selectedLines)
		{
			if (selectedLines == null)
			{
				return false;
			}

			foreach (CusTempStorageSelectableRegLine line in selectedLines)
			{
				CusEntryInstruction entryInstruction = GetEntryInstruction(line);
				if (entryInstruction == null)
				{
					return false;
				}

				JobComInvoiceHeader invoiceHeader = GetOrCreateInvoiceHeader(entryInstruction);
				BasePackage pack = CreatePackageIfNeeded(line);

				int countLines = 1;
				List<JobComInvoiceLine> invLines = new List<JobComInvoiceLine>();
				foreach (RegLineItemQuantity itemQuantity in line.RegLineItemQuantities)
				{
					JobComInvoiceLine invoiceLine = CreateInvoiceLine(invoiceHeader, entryInstruction, line, itemQuantity);
					AddPrevOrSupportDocs(invoiceLine, entryInstruction, line, itemQuantity);

					if (line.PackageType == EFTAUniversalReferenceConstants.PackageType.Frame)
					{
						AddVehicle(invoiceLine, line);
					}
					else
					{
						LinkPackage(invoiceLine, pack, ref countLines);
					}
					invLines.Add(invoiceLine);

					if (countLines == line.RegLineItemQuantities.Count)
					{
						ZDecimal totalWeigth = 0;

						foreach (JobComInvoiceLine invLine in invLines)
						{
							totalWeigth += invLine.JI_Weight;
						}

						ZDecimal weigthDiff = line.GrossWeightToDraw - totalWeigth;

						if (weigthDiff != 0)
						{
							invoiceLine.JI_Weight += weigthDiff;
						}
					}

					countLines++;
				}
			}

			return true;

			CusEntryInstruction GetEntryInstruction(CusTempStorageSelectableRegLine line)
			{
				var inst = CustomsEntryInstructions
					.FirstOrDefault(e => e.GoodsLocation.Address.AuthorisationNumber == line.CustomsLocation)
					?? CustomsEntryInstructions
					.FirstOrDefault(e => e.GoodsLocation.Address.AuthorisationNumber == ZString.Empty);

				return inst;
			}

			JobComInvoiceHeader GetOrCreateInvoiceHeader(CusEntryInstruction entryInstruction)
			{
				return (JobComInvoiceHeader)(entryInstruction.Invoices.FirstOrDefault() ?? Invoices.FirstOrDefault() ?? Invoices.AddNew());
			}

			BasePackage CreatePackageIfNeeded(CusTempStorageSelectableRegLine line)
			{
				if (line.PackageType == EFTAUniversalReferenceConstants.PackageType.Frame)
				{
					return null;
				}

				var bill = Bills
					.Where(b => b.CU_BillType == BillTypeList.Codes.HouseBill)
					.FirstOrDefault()
					?? Bills
					.Where(b => b.CU_BillType == BillTypeList.Codes.MasterBill)
					.FirstOrDefault();

				BasePackage pack;
				if (bill == null)
				{
					bill = Bills.AddNew();
					bill.CU_BillType = BillTypeList.Codes.HouseBill;
					bill.CU_BillNum = UniversalReferenceConstants.BillNumber.TBA;
					pack = Packages.First(p => p.Bill == bill);
				}
				else
				{
					pack = Packages.AddNew();
					pack.CW_HouseBill = bill.CU_BillUniqueCode;
				}

				pack.CW_MarksAndNos = line.PackageMarks;
				pack.CW_PackType = line.PackageType;
				pack.CW_PackQty = line.PackagesToDraw;

				return pack;
			}

			JobComInvoiceLine CreateInvoiceLine(
				JobComInvoiceHeader invoiceHeader,
				CusEntryInstruction entryInstruction,
				CusTempStorageSelectableRegLine line,
				RegLineItemQuantity itemQuantity)
			{
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;

				if (line.GrossWeightOnHand != 0)
				{
					invoiceLine.JI_Weight = (line.GrossWeightToDraw * itemQuantity.SRV_GrossWeight) / line.GrossWeightOnHand;
				}

				var regitem = itemQuantity.RegLineItem;
				invoiceLine.JI_Tariff = (regitem != null ? regitem.SRI_Tariff : ZString.Empty);
				invoiceLine.JI_Description = (regitem != null ? regitem.SRI_GoodsDescription : ZString.Empty);
				invoiceLine.ZG_CusNumber = (regitem != null ? regitem.SRI_CusC4Number : ZString.Empty);

				return invoiceLine;
			}

			void AddPrevOrSupportDocs(
				JobComInvoiceLine invoiceLine,
				CusEntryInstruction entryInstruction,
				CusTempStorageSelectableRegLine line,
				RegLineItemQuantity itemQuantity)
			{
				var regitem = itemQuantity.RegLineItem;

				if (JE_MessageType == MessageTypeList.Codes.Import
					|| (JE_MessageType == MessageTypeList.Codes.Export
						&& entryInstruction.CEI_SubStyle == UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS))
				{
					PreviousDocument prevDoc = invoiceLine.PreviousDocuments.AddNew();

					if (entryInstruction.CEI_Style == ImportDeclarationTypeList.H2)
					{
						prevDoc.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._337;
					}
					else if (entryInstruction.CEI_SubStyle == UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS)
					{
						prevDoc.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N337;
					}
					else
					{
						prevDoc.CSI_Code = UniversalReferenceConstants.PreviusDocumentType.SummaryDeclaration;
					}

					prevDoc.CSI_SubType = PreviousDocumentClassList.Codes.SummaryDeclaration;
					prevDoc.CSI_ReferenceNumber = line.TSDNumber;
					prevDoc.CSI_LineNo = regitem.SRI_GoodsItemNumber;
				}
				else if (JE_MessageType == MessageTypeList.Codes.Export
						 && entryInstruction.CEI_SubStyle != UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS)
				{
					SupportingDocument supDoc = invoiceLine.SupportingDocuments.AddNew();
					supDoc.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes._1217;
					supDoc.CSI_ReferenceNumber = line.TSDNumber;
				}
			}

			void AddVehicle(JobComInvoiceLine invoiceLine, CusTempStorageSelectableRegLine line)
			{
				var parts = line.PackageMarks.Split(':');
				var vehicle = invoiceLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = parts.Length > 0 ? parts[0] : string.Empty;
				vehicle.CVH_BrandName = parts.Length > 1 ? parts[1] : string.Empty;
				vehicle.CVH_ModelName = parts.Length > 2 ? parts[2] : string.Empty;
				foreach (BaseCusLinkPackage linkPack in invoiceLine.PackagesForInvoiceLinesForBindingOnly)
				{
					linkPack.IsLinked = false;
				}
			}

			void LinkPackage(JobComInvoiceLine invoiceLine, BasePackage pack, ref int countLines)
			{
				var linkPackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Where(l => l.Package == pack).FirstOrDefault();

				linkPackage.IsLinked = true;
				if (countLines == 1)
				{
					linkPackage.PackQty = pack.CW_PackQty;
				}
			}
		}
		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (IsExport && ExitHeaders.Count > 0)
				{
					result.AddRange(ExitHeaders.Cast<BusinessObject>());
					if (ExitReports.Count > 0)
					{
						result.AddRange(ExitReports.Cast<BusinessObject>());
					}
				}
				return result.ToArray();
			}
		}

		public bool IsRequestedProcedureEnable => IsImport && IsRequestedProcedureEnableCore;
		protected virtual bool IsRequestedProcedureEnableCore => false;

		public bool ShouldCheckLegalByDeclarantType => Configuration.ShouldCheckLegalByDeclarantType;

		protected override bool SkipDuplicateTopGroupInvoiceOnUniversalCopy => true;

		public virtual bool AllowUCC6PropertiesWithUCC5 => IsUCC6;

		public virtual bool UseDutyPayerAndDefermentPartyInUXML => false;

		public virtual bool AllowGoodsLocationFromImport => false;

		public bool IsSupplementaryMenuVisible => IsSupplementaryMenuVisibleCore;
		protected virtual bool IsSupplementaryMenuVisibleCore => false;

		public ZBool ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible => TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(CountryCode) && GoodsLocationIsInPremises;

		ZBool GoodsLocationIsInPremises
		{
			get
			{
				var alreadyCheckNumbers = new HashSet<ZString>();
				var result = CustomsEntryInstructions.Select(e => e.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty).Any(number =>
							!number.IsEmpty
							&& alreadyCheckNumbers.Add(number)
							&& TemporaryStorageHelper.IsLocationManagedInPremises(Factory, number));

				return result;
			}
		}

		[ResourceStringData("10569E14-60DE-43DB-B3B4-D980BE7882F6", Caption = "CSP")]
		public override ZString ZG_Gateway
		{
			get => base.ZG_Gateway;
			set => base.ZG_Gateway = value;
		}

		#region IAddInfoChildSupporter Members

		protected override BusinessObject GetAddInfoChild() => AddInfoChild;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => JobEUDeclarationSchema.EUD_JE;

		#endregion

		protected override string[] RelatedDeclarationTypes => new[] { string.Empty, EUCommonConstants.DeclarationRelationshipType.SupplementaryDeclaration };
	}
}
