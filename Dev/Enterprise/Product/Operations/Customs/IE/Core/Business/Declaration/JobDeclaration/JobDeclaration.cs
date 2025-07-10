using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclaration : AutoIEJobDeclaration
		, Integration.Customs.IE.IJobDeclaration
		, IInvoicesProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoIEJobDeclaration.Schema
		{
			public const string CountryOfExport = nameof(JobDeclaration.CountryOfExport);
			public const string DeclarationType = nameof(JobDeclaration.DeclarationType);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			SaveForNoAmendCheck();
		}

		protected override ZString DefaultDataGroupingCore => JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1) && IsImport
			? Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5
			: base.DefaultDataGroupingCore;

		protected override ZString DefaultDataGroupingForCusProcedureCore => base.DefaultDataGroupingCore;

		protected override ZString DefaultDataGroupingForTariffsCore => base.DefaultDataGroupingCore;

		protected override ZString DefaultDataGroupingForDutyRateCodesCore => base.DefaultDataGroupingCore;

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			switch (JE_MessageType.ToUpperInvariant())
			{
				case IEJobMessageTypeList.Codes.Export:
					return new ExportDeclarationJobDocAddressValidation(addressToValidate, this);
				case IEJobMessageTypeList.Codes.ExitSummary:
					return new ExitSummaryDeclarationJobDocAddressValidation(addressToValidate, this);
				case IEJobMessageTypeList.Codes.Import:
					return new ImportDeclarationJobDocAddressValidation(addressToValidate, this);
				default:
					return null;
			}
		}

		protected override ZString GetEntrySubStyleForCommonTransit(RefCountry country) => IsUCC5 ? EntryStyleListImport.Codes.ImportNormal : base.GetEntrySubStyleForCommonTransit(country);

		public bool IsITFApplicationCode => JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.Interfaced);

		public bool IsV1OrV2ApplicationCode => JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V1) || JE_ApplicationCode.EqualsIgnoringCase(ImportDeclarationApplicationCodeList.Codes.V2);

		public bool IsUCC5AndIsImport => IsUCC5 && IsImport;

		public bool IsMessageTypeExport => JE_MessageType.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Export);

		public bool IsMessageTypeImport => JE_MessageType.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Import);

		public bool IsExitSummary => JE_MessageType.EqualsIgnoringCase(IEJobMessageTypeList.Codes.ExitSummary);

		public bool IsReExport => JE_MessageType.EqualsIgnoringCase(IEJobMessageTypeList.Codes.ReExport);

		public override ZBool IsExport => IsReExport || IsExitSummary || base.IsExport;

		protected override bool AgreedPlaceCodeSupportCore => !IsExport && !JE_ShipmentIncoTerm.EqualsIgnoringCase(Core.Constants.IncoTerms.Other);

		protected override bool EUD_AgreedPlaceCodeValidationSupportCore => !IsUCC5AndIsImport;

		static bool isApplicableForDefaultIncotermPlace(CusEntryInstruction instruction) =>
			instruction.IsH1 || instruction.IsH3 || instruction.IsH4 || instruction.IsH5;
		public bool AgreedPlaceUsesUNLOCO => IsImport && CustomsEntryInstructions.Any(isApplicableForDefaultIncotermPlace) && string.IsNullOrEmpty(JE_ShipmentIncoTermPlace);

		public override bool IsNonTransportDeclarationType => !IsMessageTypeExport && !IsMessageTypeImport;

		public bool IsExpressConsignmentsOfExitSummary => ZG_SpecificCircumstanceIndicator.EqualsIgnoringCase(SpecificCircumstanceIndicatorForUCCList.Codes.A20);

		protected override IReadOnlyList<string> MultipleKeysToUseCore
		{
			get
			{
				var keys = new List<string>();

				if (IsBLT)
				{
					keys.Add(CaptionKeyBLT);
				}

				switch (JE_MessageType.ToUpperInvariant())
				{
					case IEJobMessageTypeList.Codes.ExitSummary:
						keys.Add(CaptionKeyExportUCC6EXS);
						keys.Add(JobDeclaration.CaptionKeyExportUCC6);
						break;
					case IEJobMessageTypeList.Codes.Export:
					case IEJobMessageTypeList.Codes.ReExport:
						keys.Add(JobDeclaration.CaptionKeyExportUCC6);
						break;
					case IEJobMessageTypeList.Codes.Import:
						if (IsUCC5)
						{
							keys.Add(JobDeclaration.CaptionKeyImportUCC5);
						}
						else if (IsUCC6)
						{
							keys.Add(JobDeclaration.CaptionKeyImportUCC6);
						}
						break;
					default:
						return base.MultipleKeysToUseCore;
				}

				return keys.ToArray();
			}
		}

		public const string CaptionKeyImportV1 = "BD5285D4-2F2E-4014-80CC-6267FA6DC8B4";
		public const string CaptionKeyExportUCC6EXS = "EXPUCC6EXS9A3-43D7-A41F-129B54AAC2A2";

		protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
		{
			DefaultJE_ApplicationCode();
			base.JE_MessageTypeChanged(oldValue, newValue);
			AddDefaultInstructionWhenMessageTypeChanges(newValue);
			DefaultLocationTypesWhenMessageTypeChanges();
			CustomsOffices.MarkAsNeedingValidation();
			var isExport = IsExport;
			Packages.Cast<Package>().ForEach(p =>
			{
				if (isExport)
				{
					p.ClearPackQty(p.IsBulk);
				}
				p.MarkAsNeedingValidation();
			});
			SetDefaultOwner();
			SetDefaultRegionOfDestination();
			SetDefaultInvoiceLinesRegionOfDestination();
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.MarkChargesAsNeedingValidation());
			RefreshIncotermAndChargeFactory();
		}

		protected override ZString GetEntryStyleByEntryType()
			=> JE_MessageType.EqualsIgnoringCase(IEJobMessageTypeList.Codes.Export)
			? MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion
			: (string)base.GetEntryStyleByEntryType();

		public override ZString JE_ApplicationCode
		{
			get => base.JE_ApplicationCode;
			set
			{
				var oldValue = JE_ApplicationCode;
				base.JE_ApplicationCode = value;
				if (oldValue != JE_ApplicationCode)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.MarkChargesAsNeedingValidation());
					Packages.Cast<Package>().ForEach(p => p.MarkAsNeedingValidation());
				}
			}
		}

		[ResourceStringData("IE.JobDeclaration.JE_TransportMeans", Caption = "Type of ID")]
		[ResourceStringData("IE.JobDeclaration.JE_TransportMeans|UCC6", Caption = "Type of ID", FullDescription = "[19 05 061 000] Type of Identification", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JE_TransportMeans
		{
			get => base.JE_TransportMeans;
			set => base.JE_TransportMeans = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_TransportModeInland", Caption = "Inland M.O.T")]
		public override ZString JE_TransportModeInland
		{
			get => base.JE_TransportModeInland;
			set
			{
				var oldValue = JE_TransportModeInland;
				base.JE_TransportModeInland = value;
				if (!IsCopying && oldValue != JE_TransportModeInland)
				{
					SetDefaultTransportMeansForInlandTransportMode();
				}
			}
		}

		[ResourceStringData("IE.JobDeclaration.JE_TransportMode", Caption = "Trans. Mode")]
		[ResourceStringData("IE.JobDeclaration.JE_TransportMode|UCC6", Caption = "Trans. Mode", FullDescription = "[19 03 001 000] Transport Mode at Border", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.JE_TransportMode|UCC6", Caption = "Trans. Mode", FullDescription = "[19 03 001 000] Transport Mode at Border", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				var oldValue = JE_TransportMode;
				base.JE_TransportMode = value;
				if (!IsCopying && oldValue != JE_TransportMode)
				{
					ClearTransportNationalities();
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.MarkChargesAsNeedingValidation());
					RefreshIncotermAndChargeFactory();
				}
			}
		}

		void ClearTransportNationalities()
		{
			if (!IsTransportNationalityMandatory)
			{
				JE_RN_NKTransportNationality = ZString.Empty;
			}

			if (!Box18TransportNationalityRequired)
			{
				ZG_Box18TransportNationality = ZString.Empty;
			}
		}

		[ResourceStringData("IE.JobDeclaration.ZG_BorderTransportMeans|EXP", Caption = "Trans. ID.", MediumCaption = "Transport ID", FullDescription = "[19 08 061 000] Transport Identification Means at Border", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.ZG_BorderTransportMeans|IMP", Caption = "Border T.O.ID.", FullDescription = "[19 05 061 000] Departure Transport Means < Type of Identification", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_BorderTransportMeans { get => base.ZG_BorderTransportMeans; set => base.ZG_BorderTransportMeans = value; }

		[ResourceStringData("IE.JobDeclaration.JE_RN_NKTransportNationality", Caption = "Nationality")]
		[ResourceStringData("IE.JobDeclaration.JE_RN_NKTransportNationality|UCC6", Caption = "Nationality", FullDescription = "[19 08 062 000] Nationality", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JE_RN_NKTransportNationality
		{
			get => base.JE_RN_NKTransportNationality;
			set => base.JE_RN_NKTransportNationality = value;
		}

		[ResourceStringData("B894E567-C7C9-4F09-AB6F-743FEA71FDE5", Caption = "Nationality")]
		public override ZString ZG_Box18TransportNationality
		{
			get => base.ZG_Box18TransportNationality;
			set => base.ZG_Box18TransportNationality = value;
		}

		[ResourceStringData("e3f33d82-e691-4d7a-a1e7-a6295574bdaf|IE|JE_PaymentMethod", Caption = "Payment Method")]
		[ResourceStringData("IE.JobDeclaration.ImportUCC5.JE_PaymentMethod", ShortCaption = "[4/8] Pay. Method", MediumCaption = "[4/8] Pref. Payment Method", Caption = "[4/8] Preferred Payment Method", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set
			{
				var oldValue = JE_PaymentMethod;
				base.JE_PaymentMethod = value;
				if (oldValue != JE_PaymentMethod && IsImport)
				{
					InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.CusLineTariffDetails.Cast<CusLineTariffDetail>().ForEach(detail =>
					{
						detail.ZG_MethodOfPayment = JE_PaymentMethod;
						detail.ZG_MethodOfPaymentInfo.RefreshBinding();
					}));
				}
			}
		}

		[ResourceStringData("IE.JobDeclaration.JE_DeclarantType", Caption = "Rep. Status")]
		[ResourceStringData("IE.JobDeclaration.ImportUCC5.JE_DeclarantType", ShortCaption = "Rep. Status", MediumCaption = "[3/21] Rep. Status", Caption = "[3/21] Rep. Status Code", FullDescription = "[3/21] Representative Status Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC5)]
		public override ZString JE_DeclarantType
		{
			get => base.JE_DeclarantType;
			set => base.JE_DeclarantType = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_VesselName", Caption = "Vessel")]
		public override ZString JE_VesselName { get => base.JE_VesselName; set => base.JE_VesselName = value; }

		[ResourceStringData("IE.JobDeclaration.JE_EntryStyle", Caption = "Declaration Type")]
		[ResourceStringData("IE.JobDeclaration.JE_EntryStyle|UCC6", Caption = "Declaration Type", FullDescription = "[11 01 001 000] Declaration Type", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JE_EntryStyle
		{
			get => base.JE_EntryStyle;
			set => base.JE_EntryStyle = value;
		}

		public bool IsCoJob => JE_EntryStyle == MessageSubTypeListExp.Codes.TradeOfUnionGoodsBetweenEuCustomsTerritoryNotCoveredByTheCouncilDirectives2006112EcOr2008118Ec;

		[ResourceStringData("IE.JobDeclaration.JE_ContainerMode", Caption = "Container")]
		[ResourceStringData("IE.JobDeclaration.JE_ContainerMode|UCC6", Caption = "Container", FullDescription = "[19 01 001 000] Container Indicator", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.JE_ContainerMode|UCC6", Caption = "Container", FullDescription = "[19 01 001 000] Container Indicator", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString JE_ContainerMode
		{
			get => base.JE_ContainerMode;
			set => base.JE_ContainerMode = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_TotalNoOfPacks", Caption = "No. Pkgs.", FullDescription = "Number of Packages")]
		public override ZInt JE_TotalNoOfPacks
		{
			get => base.JE_TotalNoOfPacks;
			set => base.JE_TotalNoOfPacks = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_OwnerRef|UCC6", Caption = "Owner Ref.", FullDescription = "Owner's Reference", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("8FC595F1-6F88-485B-AB80-0CC7DB37FEC7", Caption = "Declarant's Ref", ShortCaption = "Dec. Ref")]
		public override ZString JE_OwnerRef { get => base.JE_OwnerRef; set => base.JE_OwnerRef = value; }

		[ResourceStringData("IE.JobDeclaration.JE_VoyageFlightNo|UCC6", Caption = "Flight No.", FullDescription = "[19 08 017 000] Flight Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		#region Goods Location

		[ResourceStringData("IE.EXP.JobDeclaration.JE_LocationOfGoods", Caption = "Goods Location", FullDescription = "Goods Location - [16 15 036 000] UN/LOCODE", MultipleKey = CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.JE_LocationOfGoods", Caption = "Goods Location")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCollection))]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set => base.JE_LocationOfGoods = value;
		}

		[MaxLength(3)]
		[ResourceStringData("IE.JobDeclaration.JE_SubLocationOfGoods", Caption = "Additional Identifier", ShortCaption = "Add. Identifier")]
		public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

		[ResourceStringData("IE.EXP.JobDeclaration.JE_LocationQualifier", Caption = "Qualifier", FullDescription = "[16 15 046 000] Qualifier of the identification", MultipleKey = CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.JE_LocationQualifier", Caption = "Qualifier")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationQualifierList))]
		public override ZString JE_LocationQualifier { get => base.JE_LocationQualifier; set => base.JE_LocationQualifier = value; }

		[ResourceStringData("IE.EXP.JobDeclaration.JE_LocationOtherInformation", Caption = "Type", FullDescription = "[16 15 045 000] Type of Location", MultipleKey = CaptionKeyExportUCC6)]
		[ResourceStringData("IE.JobDeclaration.JE_LocationOtherInformation", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationTypeList))]
		public override ZString JE_LocationOtherInformation { get => base.JE_LocationOtherInformation; set => base.JE_LocationOtherInformation = value; }

		[ResourceStringData("IE.JobDeclaration.JE_Calc_LocationOfGoodsCtry", Caption = "Country", ShortCaption = "Ctry.")]
		public ZString JE_Calc_LocationOfGoodsCtry => Core.Constants.CountryCodes.Ireland;

		public ZPropertyInfo JE_Calc_LocationOfGoodsCtryInfo => GetZPropertyInfo(nameof(JE_Calc_LocationOfGoodsCtry));

		#endregion

		#region DefermentPartyDocAddress

		protected override void DefermentPartyDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			base.DefermentPartyDocAddressRequirement_ValidateOrganisationPK(validation);

			if (validation.Parent.Organisation == null && CustomsEntryInstructions.Any(DefermentPartyDocAddressRequirement_IsApplicableForBR2073))
			{
				validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("6AB99231-F180-40CF-91B6-D50696F8A8C5", "[BR2073] Deferment Party (Person Providing a Guarantee) is required when H3, H4, or (H1 when Requested Procedure is 44)."));
			}
		}

		static bool DefermentPartyDocAddressRequirement_IsApplicableForBR2073(CusEntryInstruction entryInstruction) => entryInstruction.CEI_Style.ToUpperInvariant().ToString() switch
		{
			ImportDeclarationTypeList.Codes.H3 or ImportDeclarationTypeList.Codes.H4 => true,
			ImportDeclarationTypeList.Codes.H1 => entryInstruction.IsProcedureCode44,
			_ => false
		};

		#endregion

		[ResourceStringData("IE.JobDeclaration.JE_ShipmentIncoTerm", Caption = "Incoterm")]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set => base.JE_ShipmentIncoTerm = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_ShipmentIncoTermPlace", Caption = "Place")]
		public override ZString JE_ShipmentIncoTermPlace
		{
			get => base.JE_ShipmentIncoTermPlace;
			set => base.JE_ShipmentIncoTermPlace = value;
		}

		[ResourceStringData("IE.JobDeclaration.JE_UCR", Caption = "[2/4] UCR", MultipleKey = CaptionKeyImportUCC5)]
		[ResourceStringData("IE.JobDeclaration.JE_UCR", ShortCaption = "UCR", Caption = "[12 08 001 000] UCR", MultipleKey = CaptionKeyImportUCC6)]
		public override ZString JE_UCR
		{
			get => base.JE_UCR;
			set => base.JE_UCR = value;
		}

		protected override void UpdateDucrIfNotLockedCore(bool force = false)
		{
		}

		[ResourceStringData("3d59f044-5ff8-4011-bdb1-8fdf1f32f15f", Caption = "Office of Lodgement")]
		[ResourceStringData("B322F6F1-3BD6-480B-AC19-ECB412AF5367", Caption = "Office of Export", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
		[ResourceStringData("A47BD2C8-CBA8-4E69-9E2F-765481578262", Caption = "Office of Lodgement", MultipleKey = JobDeclaration.CaptionKeyExportUCC6EXS)]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set => base.JE_CustomsOffice = value;
		}

		[ResourceStringData("87F9B0AD-0240-4402-910A-FD4E3398D75B", Caption = "Declarant")]
		[ResourceStringData("87F9B0AD-0240-4402-910A-FD4E3398D75C", Caption = "Declarant", FullDescription = "[13 05 000 000] Declarant", MultipleKey = CaptionKeyExportUCC6)]
		[ResourceStringData("B9BC348F-63C9-43FD-8571-F022D16DC5F0", ShortCaption = "Declarant", MediumCaption = "[3/18] Declarant", Caption = "[3/18] Declarant ID", FullDescription = "[3/18] Declarant Identification Number", MultipleKey = CaptionKeyImportUCC5)]
		public override ZGuid JE_OA_DeclarantAddress
		{
			get => base.JE_OA_DeclarantAddress;
			set => base.JE_OA_DeclarantAddress = value;
		}

		[ResourceStringData("95D7AC8C-E28E-403A-9D39-83FC35717323", Caption = "Representative", ShortCaption = "Represent.")]
		[ResourceStringData("95D7AC8C-E28E-403A-9D39-83FC35717324", Caption = "Representative", ShortCaption = "Represent.", FullDescription = "[13 06 000 000] Representative", MultipleKey = CaptionKeyExportUCC6)]
		[ResourceStringData("3CBE947C-97AC-43B2-93FD-1C0B2282F464", ShortCaption = "Representative", MediumCaption = "[3/19] Representative", Caption = "[3/19 & 3/20] Representative (ID)", FullDescription = "[3/19] Representative & [3/20] Representative Identification Number", MultipleKey = CaptionKeyImportUCC5)]
		public override ZGuid JE_OA_Representative
		{
			get => base.JE_OA_Representative;
			set => base.JE_OA_Representative = value;
		}

		[ResourceStringData("B2F116F9-9C63-488B-8D19-206D5D0D45B2", Caption = "Seller")]
		[ResourceStringData("941C4B7C-61C6-4588-AB04-A0874443A8F1", ShortCaption = "Seller", MediumCaption = "[3/24] Seller", Caption = "[3/24 & 3/25] Seller (ID)", FullDescription = "[3/24] Buyer & [3/25] Seller Identification Number", MultipleKey = CaptionKeyImportUCC5)]
		public override ZGuid JE_OA_SellerAddress
		{
			get => base.JE_OA_SellerAddress;
			set => base.JE_OA_SellerAddress = value;
		}

		[ResourceStringData("69A857E0-8120-4854-A850-9D0C8FFA632F", Caption = "Buyer")]
		[ResourceStringData("893C9ACD-333F-4EF1-A7E3-A35F4FD80BD9", ShortCaption = "Buyer", MediumCaption = "[3/26] Buyer", Caption = "[3/26 & 3/27] Buyer (ID)", FullDescription = "[3/26] Buyer & [3/27] Buyer Identification Number", MultipleKey = CaptionKeyImportUCC5)]
		public override ZGuid JE_OH_Buyer
		{
			get => base.JE_OH_Buyer;
			set { base.JE_OH_Buyer = value; }
		}

		[ResourceStringData("F19A7BBA-2485-4BBD-AE88-DFE4FD048474", ShortCaption = "Duty Payer", MediumCaption = "[3/46] Duty Payer", Caption = "[3/46] Duty Payer ID", FullDescription = "[3/46] Person paying the customs duty identification number", MultipleKey = CaptionKeyImportUCC5)]
		public override ZGuid JE_OH_DutyPayer
		{
			get => base.JE_OH_DutyPayer;
			set => base.JE_OH_DutyPayer = value;
		}

		[ReadOnly(true)]
		public override ZString JE_EntryStatus
		{
			get { return base.JE_EntryStatus; }
			set { base.JE_EntryStatus = value; }
		}

		public override ZString JE_GoodsOrigin
		{
			get => base.JE_GoodsOrigin;
			set
			{
				var oldValue = JE_GoodsOrigin;
				base.JE_GoodsOrigin = value;
				if (!IsCopying)
				{
					var newValue = JE_GoodsOrigin;
					if (oldValue != newValue)
					{
						ClearInvoiceLineValuesIfSame(newValue, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
						SetLocationOfGoodsDefault();
					}
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		void SetLocationOfGoodsDefault()
		{
			if (JE_LocationOfGoods.IsEmpty && JE_RL_NKOrigin.Left(2).EqualsIgnoringCase(Core.Constants.CountryCodes.Ireland) && IsExport)
			{
				JE_LocationOfGoods = JE_RL_NKOrigin;
			}
		}

		[ResourceStringData("4DE99019-FCDD-4EBA-869F-28EA5C7AFCAC", Caption = "Destination")]
		public override ZString JE_RL_NKFinalDestination
		{
			get => base.JE_RL_NKFinalDestination;
			set => base.JE_RL_NKFinalDestination = value;
		}

		[ResourceStringData("786AA697-616D-4975-8EBC-27401F4EBCD1", Caption = "Dispatch")]
		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set
			{
				var oldCountryOfExport = CountryOfExport;
				base.JE_RL_NKOrigin = value;
				if (!IsCopying)
				{
					var newCountryOfExport = CountryOfExport;
					if (oldCountryOfExport != newCountryOfExport)
					{
						ClearInvoiceLineValuesIfSame(newCountryOfExport, JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
					}
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override ZDateTime JE_DateAtOrigin
		{
			get => base.JE_DateAtOrigin;
			set
			{
				var oldValue = JE_DateAtOrigin;
				base.JE_DateAtOrigin = value;
				if (!IsCopying)
				{
					var newValue = JE_DateAtOrigin;
					if (oldValue != newValue)
					{
						CreateOrUpdateInstructionAdditionalInfoCode1D23(newValue);
					}
				}
			}
		}

		[ResourceStringData("E7D2936C-9D43-4F47-80D9-80581D3BE5FC", Caption = "Account Number")]
		public override ZString JE_DefermentAccountNumber
		{
			get => base.JE_DefermentAccountNumber;
			set => base.JE_DefermentAccountNumber = value;
		}

		void CreateOrUpdateInstructionAdditionalInfoCode1D23(ZDateTime dateAtOrigin)
		{
			if (dateAtOrigin.IsValid && IsMessageTypeExport)
			{
				var referenceNumber = JE_DateAtOrigin.ToString(Constants.DateTimeFormat.AdditionalReferenceDateTime);
				foreach (var instruction in CustomsEntryInstructions)
				{
					var additionalInfo = instruction.AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault(x => x.IsAnAdditionalReference && x.CSI_Code == Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture);
					if (additionalInfo != null)
					{
						additionalInfo.CSI_ReferenceNumber = referenceNumber;
					}
					else
					{
						additionalInfo = instruction.AdditionalInfos.AddNew();
						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						additionalInfo.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
						additionalInfo.CSI_ReferenceNumber = referenceNumber;
					}
				}
			}
		}

		public override ZDateTime JE_DateAtFinalDestination
		{
			get => base.JE_DateAtFinalDestination;
			set
			{
				var oldValue = JE_DateAtFinalDestination;
				base.JE_DateAtFinalDestination = value;
				if (!IsCopying)
				{
					var newValue = JE_DateAtFinalDestination;
					if (oldValue != newValue)
					{
						CreateOrUpdateInstructionSupportingDocumentCode1D24(newValue);
					}
				}
			}
		}

		void CreateOrUpdateInstructionSupportingDocumentCode1D24(ZDateTime dateAtFinalDestination)
		{
			if (dateAtFinalDestination.IsValid && IsMessageTypeImport)
			{
				var referenceNumber = JE_DateAtFinalDestination.ToString(Constants.DateTimeFormat.AdditionalReferenceDateTime);
				foreach (var instruction in CustomsEntryInstructions)
				{
					instruction.CreateAndPopulateAdditionalRefType1D24();
				}
			}
		}

		public ZString CountryOfExport
		{
			get { return JE_RL_NKOrigin.Left(2); }
			set { JE_RL_NKOrigin = value; }
		}

		public ZPropertyInfo CountryOfExportInfo => GetWrappedZPropertyInfo(Schema.CountryOfExport, (x) => JE_RL_NKOriginInfo);

		public ZString OfficeOfExitCustomsOffice => CustomsOffices.GetOfficeOfExit()?.CY_Data ?? ZString.Empty;
		public ZString PresentationCustomsOffice => CustomsOffices.GetPresentationOffice()?.CY_Data ?? ZString.Empty;
		public ZString SupervisingCustomsOffice => CustomsOffices.GetSupervisingOffice()?.CY_Data ?? ZString.Empty;

		public bool IsAllPreliminaryDeclaration => Factory.GetValue(ref isAllPreliminaryDeclarationCached, () => CustomsEntryInstructions.All(x =>
		{
			var subStyle = x.CEI_SubStyle;
			return subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA || subStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;
		}));
		CachedProperty<bool> isAllPreliminaryDeclarationCached;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;
		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);
		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public new OfficeCodeCollection CustomsOffices => (OfficeCodeCollection)base.CustomsOffices;

		public List<ZGuid> OverlappingPackageInvoiceLines;

		public Dictionary<ZGuid, ZString> OneMainPackInvoiceLinePerPackageValidationResult;

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			Customs.Business.JobDeclarationValidation result;

			switch (JE_MessageType.ToUpperInvariant())
			{
				case IEJobMessageTypeList.Codes.Import:
					if (IsUCC6)
					{
						result = new ImportUCC6JobDeclarationValidation(this);
					}
					else
					{
						result = new ImportJobDeclarationValidation(this);
					}
					break;
				case IEJobMessageTypeList.Codes.Export:
					result = new ExportJobDeclarationValidation(this);
					break;
				case IEJobMessageTypeList.Codes.ReExport:
					result = new ReExportJobDeclarationValidation(this);
					break;
				case IEJobMessageTypeList.Codes.ExitSummary:
					result = new ExitSummaryJobDeclarationValidation(this);
					break;
				default:
					result = new JobDeclarationValidation(this);
					break;
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			return IsImport
				? new ImportJobDeclarationLookups(this)
				: IsExport
					? new ExportJobDeclarationLookups(this)
					: new JobDeclarationLookups(this);
		}

		public void LogCustomsCommenced(string reference)
		{
			var stmALog = LogsOfDeclarationOrShipment.MostRecentLogByEventTimeExcludingEstimated(CustomsCommencedEventType, reference, GetBranchQuery());
			if (stmALog == null)
			{
				LogsOfDeclarationOrShipment.AddNew(CustomsCommencedEventType, reference);
				if (ShouldLogCustomsCommencedDatail)
				{
					JE_CustomsCommencedDate = ZDateTime.Now;
					JE_GS_NKCustomsCommencedUser = GlbStaff.CurrentUser.GS_Code;
				}
			}
		}

		protected override bool IsLookupsCachedInBase => false;

		protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this, new CusEntryInstructionComparer());

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Ireland;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IValueSetStrategy GetValueSetStrategy() => IsImport ? new ImportJobDeclarationValueSetStrategy(this) : base.GetValueSetStrategy();

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);

		protected override ZString Box18IdentityOfTransportAtDepartureForDocumentsAndMessagingCore => JE_TransportIDInland;

		protected override ZString Box30LocationOfGoodsForDocumentsAndMessagingCore => JE_LocationOfGoods;

		protected override EU.Business.Declaration.CusEntryHeaderDocumentSupporter GetCusEntryHeaderDocumentSupporterCore(EU.Business.Declaration.CusEntryHeader entryHeader) => new CusEntryHeaderDocumentSupporter((CusEntryHeader)entryHeader);

		protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[CusCodeDataTypeList.Codes.OfficeCode] = typeof(OfficeCode);
			return result;
		}

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType) => new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

		protected override string GetIApportionInvoiceHolderCountryContextCore()
		{
			return Core.Constants.CountryCodes.Ireland + this.GetIncoTermChargeFactoryCacheKey();
		}

		protected override bool IsSupplementaryMenuVisibleCore => IsUCC5;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("IE"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		protected override EuOfficeCodeCollection GetCustomsOffices() => new OfficeCodeCollection(this);

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		public new MergeManager MergeManager => (MergeManager)base.MergeManager;

		public override EU.Business.Declaration.EntryCreationStrategy CreateEntryCreationStrategy()
		{
			if (IsExport)
			{
				return new ExportEntryCreationStrategy(this);
			}
			else if (IsImport)
			{
				if (IsUCC5)
				{
					return new ImportUCC5EntryCreationStrategy(this);
				}
				else
				{
					return new ImportEntryCreationStrategy(this);
				}
			}
			else
			{
				return base.CreateEntryCreationStrategy();
			}
		}

		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer()
		{
			return new DeclarationValueChangedAnnouncer(this);
		}

		public override ZBool ExitControlTabVisible => base.IsExport || IsReExport;

		public override ZBool AircraftRegistrationNumberVisible => ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._41 && (IsAir || IsOwnPropulsion_Air) && IsUCC6;

		public ZBool IsOwnPropulsion_Air => IsExport && JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion && (ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._40 || ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._41);

		public ZBool IsOwnPropulsion_Sea => IsExport && JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion && (ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10 || ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._11);

		public bool TransportIDRequiredWhenExport => IsExport && ((JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion && !IsOwnPropulsion_Air && !IsOwnPropulsion_Sea) || JE_TransportMode == Core.Constants.TransportModes.Road || JE_TransportMode == Core.Constants.TransportModes.FixedTransportInstallations || JE_TransportMode == Core.Constants.TransportModes.Mail);

		public bool IsContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference => Factory.GetValue(ref isContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference, () =>
		{
			var instructions = CustomsEntryInstructions.Where(p => p.CEI_SubStyle != EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF);
			var result = false;

			if (instructions.Any())
			{
				var instructionAdditionalReferences_1D95 = instructions.SelectMany(p => p.AdditionalInfos.Cast<AdditionalInfo>()).Where(Is1D95AdditionalReferences);
				if (instructionAdditionalReferences_1D95.Any())
				{
					result = true;
				}
				else
				{
					var invoiceAdditionalReferences_1D95 = instructions.SelectMany(p => p.Invoices.SelectMany(inv => inv.AdditionalInfos.Cast<AdditionalInfo>())).Where(Is1D95AdditionalReferences);
					if (invoiceAdditionalReferences_1D95.Any())
					{
						result = true;
					}
					else
					{
						var invoiceLineAdditionalReferences_1D95 = instructions.SelectMany(p => p.InvoiceLines.SelectMany(line => ((JobComInvoiceLine)line).AdditionalInfos.Cast<AdditionalInfo>())).Where(Is1D95AdditionalReferences);
						if (invoiceLineAdditionalReferences_1D95.Any())
						{
							result = true;
						}
					}
				}
			}

			return result;

			bool Is1D95AdditionalReferences(AdditionalInfo additionalInfo) => additionalInfo.IsAnAdditionalReference && additionalInfo.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.RoRoUnaccompaniedTrailerRegistrationNumber);
		});
		CachedProperty<bool> isContaining_InstructionSubStyleNot_Y_And_1D95AdditionalReference;

		#region Visibilities and Labels

		public bool TransportIDRequiredWhenImport => IsImport && (JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion || JE_TransportMode == Core.Constants.TransportModes.Road || JE_TransportMode == Core.Constants.TransportModes.Rail || JE_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport);

		public bool TransportIDRequired => TransportIDRequiredWhenExport || TransportIDRequiredWhenImport;

		public bool TransportIDInlandWaterwayRequired => (JE_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport || JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion) && IsUCC6 && ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._81;

		public bool TransportIDInlandWaterwayENIRequired => (JE_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport || JE_TransportMode == Core.Constants.TransportModes.OwnPropulsion) && IsUCC6 && ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._80;

		public bool IsTransportNationalityMandatory => IsExport
			? !ActiveTransportMeansID.IsEmpty
			: (IsAir || IsSea || TransportIDRequired || TransportIDInlandWaterwayRequired || TransportIDInlandWaterwayENIRequired) && IsTransportNationalityMandatoryImport;

		bool IsTransportNationalityMandatoryImport =>
			(IsUCC6 || !(DeclarationType.EqualsIgnoringCase("H2") && string.IsNullOrEmpty(JE_RN_NKTransportNationality)));

		internal bool Box18TransportNationalityRequired => IsAir || IsSea;

		public bool IsJE_TransportMeansRequired => IsExport && IsOwnPropulsionInland;

		#endregion

		protected void SetDefaultTransportMeansForInlandTransportMode()
		{
			if (AddInfoLookups.TransportMeansList is CodeDescriptionPairList transportMeansList)
			{
				if (!transportMeansList.ContainsCode(JE_TransportMeans))
				{
					JE_TransportMeans = transportMeansList.DefaultCode ?? ZString.Empty;
				}
				else if (transportMeansList.DefaultCode == null)
				{
					JE_TransportMeans = ZString.Empty;
				}
			}
		}

		public ZString ActiveTransportMeansID
		{
			get
			{
				string identificationNumber;

				if (IsExport)
				{
					switch (ZG_BorderTransportMeans)
					{
						case ExportBorderTransportMeansList.Codes._10:
							identificationNumber = JE_LloydsIMO;
							break;
						case ExportBorderTransportMeansList.Codes._40:
							identificationNumber = JE_VoyageFlightNo;
							break;
						case ExportBorderTransportMeansList.Codes._41:
							identificationNumber = JE_AircraftRegistration;
							break;
						default:
							identificationNumber = JE_VesselName;
							break;
					}
				}
				else
				{
					identificationNumber = JE_VesselName;
				}
				return identificationNumber;
			}
		}

		public bool IsUCC6EnabledForImport => IECustomsDataRegistry.Instance.IsUCC6EnabledForImport.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty);

		void AddDefaultInstructionWhenMessageTypeChanges(ZString messageType)
		{
			var defaultDeclarationType = CusEntryInstructionStyleHelper.GetDefaultDeclarationType(messageType);
			if (!defaultDeclarationType.IsEmpty)
			{
				CusEntryInstruction instruction = null;
				switch (CustomsEntryInstructions.Count)
				{
					case 0:
						instruction = CustomsEntryInstructions.AddNew();
						break;
					case 1:
						instruction = CustomsEntryInstructions[0];
						if (
							(instruction.IsInDatabase && instruction.FirstActiveEntry is CusEntryHeader entryHeader && !entryHeader.CH_Status.IsEmpty) ||
							instruction.Lookups.DeclarationTypeList.ContainsCode(instruction.CEI_Style)
						)
						{
							instruction = null;
						}
						break;
				}
				if (instruction != null)
				{
					instruction.CEI_Style = defaultDeclarationType;
				}
			}
		}

		protected override void DefaultJE_ApplicationCode()
		{
			if (IsExport)
			{
				base.DefaultJE_ApplicationCode();
			}
			else if (IsImport)
			{
				var submissionType = GetInterfaceSubmissionType();
				switch (submissionType)
				{
					case DeclarationApplicationCodeList.Codes.Builtin:
					case DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted:
						JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
						break;
					default:
						JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
						break;
				}
			}
			else
			{
				JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			}
		}

		protected override bool IsBuiltinSubmissionType(string type)
		{
			return IsImport ? type == ImportDeclarationApplicationCodeList.Codes.V1 || type == ImportDeclarationApplicationCodeList.Codes.V2 : type == DeclarationApplicationCodeList.Codes.Builtin;
		}

		protected override bool SupportMultipleBuiltInTypes => IsImport && IsUCC6EnabledForImport;

		void DefaultLocationTypesWhenMessageTypeChanges()
		{
			if (IsExport)
			{
				JE_LocationOtherInformation = Constants.TypeOfLocation.DesignatedLocation;
				JE_LocationQualifier = Constants.LocationQualifier.UNLoco;
			}
		}

		void SetDefaultOwner()
		{
			if (!IsImport)
			{
				foreach (var instruction in CustomsEntryInstructions)
				{
					instruction.CEI_OH_OwnerInfo.ClearValue();
				}
			}
		}

		void SetDefaultInvoiceLinesRegionOfDestination()
		{
			if (!IsImport)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.ZG_RegionOfDestination = ZString.Empty);
			}
		}

		void SetDefaultRegionOfDestination()
		{
			if (!IsImport)
			{
				ZG_RegionOfDestination = ZString.Empty;
			}
		}

		#region Original Values for no-amend check

		internal ZGuid OriginalSupplier { get; private set; }
		internal ZGuid OriginalRepresentative { get; private set; }
		internal ZGuid OriginalDeclarant { get; private set; }
		internal ZString OriginalDeclarationType { get; private set; }
		internal ZString OriginalPresentationOffice { get; private set; }
		internal ZString OriginalCustomsOffice { get; private set; }
		internal ZString OriginalExitOffice { get; private set; }
		internal ZString OriginalSupervisingOffice { get; private set; }

		void SaveForNoAmendCheck()
		{
			if ((IsExport || IsImport) && !JE_EntryStatus.IsEmpty)
			{
				OriginalSupplier = JE_OH_Supplier;
				OriginalRepresentative = JE_OA_Representative;
				OriginalDeclarant = JE_OA_DeclarantAddress;
				OriginalDeclarationType = JE_EntryStyle;
				OriginalPresentationOffice = PresentationCustomsOffice;
				OriginalCustomsOffice = JE_CustomsOffice;
				OriginalExitOffice = OfficeOfExitCustomsOffice;
				OriginalSupervisingOffice = SupervisingCustomsOffice;
			}
		}

		#endregion

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper() => new JobDeclarationTestDataHelper(this);

		public class JobDeclarationTestDataHelper : JobDeclarationBusinessObjectTestDataHelper
		{
			public JobDeclarationTestDataHelper(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != nameof(CustomsOffices))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}
#endif

		[ResourceStringData("IEJobDeclaration|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code")]
		public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

		protected override bool ZG_AgreedPlaceCodeValidationSupportCore => false;

		protected override string GetDeclarantTypeForMatchingEORICodes() => ZString.Empty;

		protected override ZString TradersOwnReferenceFullForBox7Core => ZString.Join(", ", GetTradersOwnReferenceFullForBox7());

		ZString[] GetTradersOwnReferenceFullForBox7()
		{
			var result = new List<ZString>();
			if (!JE_OwnerRef.IsEmpty)
			{
				result.Add(JE_OwnerRef);
			}
			result.AddRange(Invoices.Where(p => !p.JZ_UCR.IsEmpty).Select(p => p.JZ_UCR));
			return result.ToArray();
		}

		protected override DeclarationInventorySelectionHeader GetNewInventorySelectionHeader()
		{
			return new InventorySelectionHeader(this);
		}

		public override bool AllowUCC6PropertiesWithUCC5 => true;

		public override bool UseDutyPayerAndDefermentPartyInUXML => true;

		public override bool AllowGoodsLocationFromImport => IsImport;

		protected override ZBool SupportValidateCustomsMessagingCore => SupportEntryDeclarationMessageCore;

		#region IJobDeclarationMessageSupporter Members

		protected override ZBool SupportEntryDeclarationMessageCore => IsExport || (IsUCC5AndIsImport && IsSendToCustomsForImportEnabled);

		protected override ZString GetReasonForNotSupportEntryDeclarationMessageCore()
		{
			var importExportMessage = Res.GetString("35D5E299-DDFA-42CA-A2F7-C3F8D5D3D4A2", "The Send Entry/Declaration Message trigger is only supported for Export or Import-UCC5 Declaration.");
			if (IsImport)
			{
				if (!IsSendToCustomsForImportEnabled)
				{
					return Res.GetString("08CF2E9A-A559-4F0B-A99D-5A11DFCA99C4", "You have to enable 'Send To Customs' for Imports Registry to support Send Entry/Declaration Message trigger for Imports.");
				}
				else if (!IsUCC5)
				{
					return importExportMessage;
				}
			}
			else if (!IsExport)
			{
				return importExportMessage;
			}

			return ZString.Empty;
		}

		protected override IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			if (IsExport)
			{
				return new AutoIM515MessageProcessor(this);
			}
			else if (IsUCC5AndIsImport && IsSendToCustomsForImportEnabled)
			{
				return new AutoIM415MessageProcessor(this);
			}

			return null;
		}

		bool IsSendToCustomsForImportEnabled => IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.Value;

		#endregion

		[ResourceStringData("07BB2FFB-93AC-4D73-AC84-1681FF98EEAB", Caption = "Declaration Type")]
		public ZString DeclarationType => Factory.GetValue(ref declarationTypeCached, () =>
		{
			var ceiStyles = CustomsEntryInstructions.Select(e => e.CEI_Style).ToList();
			return string.Join(",", ceiStyles);
		});
		CachedProperty<ZString> declarationTypeCached;
	}
}
