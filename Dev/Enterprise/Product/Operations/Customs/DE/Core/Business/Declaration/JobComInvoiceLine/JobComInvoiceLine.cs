using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	[SystemDefinedValues]
	public class JobComInvoiceLine : AutoJobComInvoiceLine
		, Integration.Customs.DE.IJobComInvoiceLine
		, IPreviousDocumentParentProvider
		, ISupportingDocumentMaster
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
		{
			public const string JI_CessionFlag = "JI_CessionFlag";
			public const string SupplementaryInformation = "SupplementaryInformation";
			public const string JI_TobaccoStamp = "JI_TobaccoStamp";
			public const string JI_IsMainPack = "JI_IsMainPack";
			public const string AdditionalInfoDescription = "AdditionalInfoDescription";
			public const string ZG_UsualReplacement = "ZG_UsualReplacement";
			public const string ZG_ReimportDate = "ZG_ReimportDate";
			public const string ZG_NetPrice = "ZG_NetPrice";
			public const string OutwardMRN = nameof(JobComInvoiceLine.OutwardMRN);
			public const string OutwardDecisiveDate = nameof(JobComInvoiceLine.OutwardDecisiveDate);
			public const string DgSubstance = nameof(JobComInvoiceLine.DgSubstance);
			public const string JI_NetPrice = nameof(JobComInvoiceLine.JI_NetPrice);
			public const string JI_RX_NKNetPriceCurr = nameof(JobComInvoiceLine.JI_RX_NKNetPriceCurr);

			public const int OutwardMRNMaxLength = 35;
			public const int JI_BondedWhsQuantityDecimalPlaces = 3;
			public const int SupplementaryInformationMaxLength = 100;
			public const int AdditionalInfoDescriptionMaxLength = 100;
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

		public override bool NeedsCustomsQuantity
		{
			get
			{
				var transportMode = Declaration?.JE_TransportMode ?? string.Empty;
				return (transportMode != Core.Constants.TransportModes.FixedTransportInstallations || !IsExport) && base.NeedsCustomsQuantity;
			}
		}

		public override ZGuid JI_CEI
		{
			get => base.JI_CEI;
			set
			{
				var oldValue = JI_CEI;
				base.JI_CEI = value;
				if (!IsCopying && oldValue != JI_CEI)
				{
					ClearCountryOfOriginIfNeeded();
					ClearDecisiveDateIfNeeded();
				}
			}
		}

		public override ZString JI_FormattedProcedure
		{
			get
			{
				return Declaration?.IsUCCCompliant ?? false ?
					DisplayFormatForProcedure(JI_Procedure) :
					JI_Procedure;
			}
			set => JI_Procedure = FormatForProcedure(value).Left(JI_ProcedureInfo.MaxLength);
		}

		static ZString FormatForProcedure(ZString unformattedProcedure)
		{
			return unformattedProcedure.KeepAlphanumericCharacters().Left(7);
		}

		static ZString DisplayFormatForProcedure(ZString unformattedProcedure)
		{
			ZString newProcedure = unformattedProcedure.KeepAlphanumericCharacters();
			ZString dottedProcedure = newProcedure.IsEmpty ? "" : newProcedure.SubstringSafe(0, 2) + " " + newProcedure.SubstringSafe(2, 2) + " " + newProcedure.SubstringSafe(4, 3).Trim();
			return dottedProcedure.Trim(new char[] { ' ' });
		}

		[ResourceStringData("0A7EFFC9-0231-4FF0-83BF-04E664093CE6", ShortCaption = "Pref. Ctry./Rgn.", MediumCaption = "Preferential Ctry./Rgn.", Caption = "Preferential Country/Region")]
		public override ZString ZG_CountryOfSupply
		{
			get => base.ZG_CountryOfSupply;
			set
			{
				var oldValue = ZG_CountryOfSupply;
				base.ZG_CountryOfSupply = value;

				if (!IsCopying && oldValue != ZG_CountryOfSupply)
				{
					if (ZG_CountryOfSupply != JI_CountryOfOrigin)
					{
						UpdateJI_PrimaryPreferenceIfNeeded();
					}
				}
			}
		}

		public override ZString JI_CountryOfOrigin
		{
			get => base.JI_CountryOfOrigin;
			set
			{
				var oldValue = JI_CountryOfOrigin;
				base.JI_CountryOfOrigin = value;

				if (!IsCopying && oldValue != JI_CountryOfOrigin)
				{
					UpdateZG_CountryOfSupplyIfNeeded();
					UpdateJI_StateOrRegionOfOriginIfNeeded();

					if (ZG_CountryOfSupply.IsEmpty)
					{
						UpdateJI_PrimaryPreferenceIfNeeded();
					}
				}
			}
		}

		protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Germany;

		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		protected override string ChargeCodeForOverseasFreight => IsExport ? ChargeCodeList.Codes.OverseasFreight : ImportChargeCodeList.Codes._011;

		protected override string ChargeCodeForOverseasInsurance => IsExport ? ChargeCodeList.Codes.OverseasInsurance : ImportChargeCodeList.Codes._012;

		protected override ZString EffectiveCountryOfOriginCore
		{
			get
			{
				var result = base.EffectiveCountryOfOriginCore;
				if (IsImport && !ZG_CountryOfSupply.IsEmpty)
				{
					result = ZG_CountryOfSupply;
				}
				return result;
			}
		}

		[ResourceStringData("91945042-0965-4B3C-B998-AE4B8ED3C3B8", Caption = "Inbound Reg. No.")]
		public override ZString JI_PreviousEntryNumber
		{
			get => base.JI_PreviousEntryNumber;
			set => base.JI_PreviousEntryNumber = value;
		}

		public ResourceStringData JI_PreviousEntryNumberBondedWarehouseCaption => Res.GetData("f2bb13f3-f2b8-4923-865a-5979d56ba678", "Prev. Entry", "Prev. Entry No.", "Previous Entry No.", "Previous Entry Number");

		[ResourceStringData("285BCCB1-B989-47BD-8091-57E731561D5F", Caption = "Inbound Reg. Pos.")]
		public override ZShort JI_PreviousEntryLineNumber
		{
			get => base.JI_PreviousEntryLineNumber;
			set => base.JI_PreviousEntryLineNumber = value;
		}

		public ResourceStringData JI_PreviousEntryLineNumberBondedWarehouseCaption => Res.GetData("2c876488-0c4a-43a7-9ba1-b5c064510a55", "Prev. Line", "Prev. Entry Line", "Previous Entry Line", "Previous Entry Line No.");

		public override ZString JI_PartNo
		{
			get => base.JI_PartNo;
			set
			{
				var oldValue = JI_PartNo;
				base.JI_PartNo = value;
				if (!IsCopying && oldValue != JI_PartNo)
				{
					UpdateJI_CustomsSecondQuantityIfNeeded();
				}
			}
		}
		public ResourceStringData BondedWhsQuantityAdjustmentResourceDataString => Res.GetData("ABAD52C6-676A-4109-86F4-2B6D19411D7A", "Outward Qty.");

		public ResourceStringData BondedWhsUnitQuantityAdjustmentResourceDataString => Res.GetData("9A0E65A0-C5EB-4146-ADA2-9192BFCE2CB6", "Outward Qty. Unit");

		public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected override Type InvoiceHeaderType => typeof(JobComInvoiceHeader);

		public new CusEntryLine CusEntryLine => (CusEntryLine)base.CusEntryLine;

		[ResourceStringData("6AFD2836-27EC-4A12-B57A-2C8E7F69BE51", Caption = "Outward MRN")]
		[MaxLength(Schema.OutwardMRNMaxLength)]
		public ZString OutwardMRN
		{
			get => OutwardCusEntryNumberWrapper.EntryNumber;
			set
			{
				OutwardCusEntryNumberWrapper.SetEntryNumber(value, OutwardMRNInfo);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutwardMRN();
				}
			}
		}

		public ZPropertyInfo OutwardMRNInfo => GetZPropertyInfo(Schema.OutwardMRN);

		[ResourceStringData("485FBB3E-D79E-44E5-8DE2-2111B821BCF5", Caption = "Decisive Date")]
		public ZDateTime OutwardDecisiveDate
		{
			get => OutwardCusEntryNumberWrapper.ExpiryDate;
			set
			{
				OutwardCusEntryNumberWrapper.SetExpiryDate(value, OutwardDecisiveDateInfo);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOutwardDecisiveDate();
				}
			}
		}

		public ZPropertyInfo OutwardDecisiveDateInfo => GetZPropertyInfo(Schema.OutwardDecisiveDate);

		CusEntryNumberWrapper OutwardCusEntryNumberWrapper => outwardCusEntryNumberWrapper ?? (outwardCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.MovementReferenceNumber));
		CusEntryNumberWrapper outwardCusEntryNumberWrapper;

		[ResourceStringData("75be5027-2b01-4a0f-af9c-d7c77ca664ca", Caption = "DG Substance", ShortCaption = "DG Sub", MediumCaption = "DG Substance")]
		public ZString DgSubstance => Factory.GetValue(ref dgSubstance, () => UNDGs.Where(undg => undg.UNDGSubstance != null).Select(undg => undg.UNDGSubstance.DG_UNNO).OrderBy(s => s).JoinAsString(";"));
		CachedProperty<ZString> dgSubstance;

		public ZPropertyInfo DgSubstanceInfo => GetZPropertyInfo(Schema.DgSubstance);

		protected override void InitialiseUNDGs()
		{
			base.InitialiseUNDGs();
			((System.ComponentModel.IBindingList)UNDGs).ListChanged += delegate
			{
				DgSubstanceInfo.RefreshBinding();
			};
		}

		protected override EU.Business.Declaration.JobComInvoiceLineTaxCollection CreateTaxCollection()
		{
			var taxes = new JobComInvoiceLineTaxCollection(this);
			taxes.MaxCountValidationEnable(9, Res.GetString("6EF4FD6D-EA78-4C9C-ADCC-1A695AE5892C", "You are only allowed a maximum of 9 Special Cases per Invoice Line."));
			return taxes;
		}

		public new JobComInvoiceLineTaxCollection Taxes => (JobComInvoiceLineTaxCollection)base.Taxes;

		public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			if (IsImport)
			{
				return new ImportJobComInvoiceLineValidation(this);
			}
			else if (IsWarehouseAdjustment)
			{
				return new WarehouseAdjustmentJobComInvoiceLineValidation(this);
			}
			else if (IsExport)
			{
				return new ExportJobComInvoiceLineValidation(this);
			}
			else
			{
				return new JobComInvoiceLineValidation(this);
			}
		}

		public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this, false);

		public PreviousDocumentMaster PreviousDocumentMaster => previousDocumentMaster ?? (previousDocumentMaster = new PreviousDocumentMaster(Factory, this));
		PreviousDocumentMaster previousDocumentMaster;

		[ChildEditable]
		public PreviousDocumentCollection PreviousProcedures
		{
			get
			{
				if (previousProcedures == null)
				{
					previousProcedures = new PreviousDocumentCollection(this, true);
					previousProcedures.Load();
					RegisterEditableChildObject(previousProcedures);
				}

				return previousProcedures;
			}
		}
		PreviousDocumentCollection previousProcedures;

		public PreviousDocumentMaster PreviousProcedureMaster => previousProcedureMaster ?? (previousProcedureMaster = new PreviousDocumentMaster(Factory, new PreviousProcedureParentProvider(this)));
		PreviousDocumentMaster previousProcedureMaster;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result.Add(InwardProcessingProduct.CusSupportingInfoType, typeof(InwardProcessingProduct));
			return result;
		}

		protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

		public new InvoiceLineChargeCollection<InvoiceLineCharge> Charges => (InvoiceLineChargeCollection<InvoiceLineCharge>)base.Charges;

		protected override IJobComInvChargeCollection<BaseInvoiceLineCharge> CreateNewInvoiceLineChargeCollection() => new InvoiceLineChargeCollection<InvoiceLineCharge>(this);

		public new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge> ApportionedCharges => (JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>)base.ApportionedCharges;

		protected override IJobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge> CreateNewInvoiceLineApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>(this);

		#region Properties

		[ResourceStringData("4f14789c-04c5-4ebb-be7a-e3d44fca4b98", ShortCaption = "State", MediumCaption = "Federal State", Caption = "Origin Federal State")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.StateOrRegionOfOriginList))]
		public override ZString JI_StateOrRegionOfOrigin
		{
			get => base.JI_StateOrRegionOfOrigin;
			set => base.JI_StateOrRegionOfOrigin = value;
		}

		public override ZString JI_Procedure
		{
			get => base.JI_Procedure;
			set
			{
				base.JI_Procedure = value;
				if (!IsCopying)
				{
					if (value.IsEmpty)
					{
						JI_CessionFlag = ZString.Empty;
					}

					if (IsProcedureInE01OrE02)
					{
						JI_NetPrice = ZDecimal.Zero;
					}
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public ZString Concession => JI_Procedure.SubstringSafe(4);

		public ZBool IsProcedureInE01OrE02 => Concession == CustomsProcedureCodeList.Import.Concession._E01 || Concession == CustomsProcedureCodeList.Import.Concession._E02;

		public ZBool ProcedureNeedSpecialRateCharge
			=> Concession == CustomsProcedureCodeList.Import.Concession._8E6
			|| Concession == CustomsProcedureCodeList.Import.Concession._8E8
			|| Concession == CustomsProcedureCodeList.Import.Concession._8E9;

		[ResourceStringData("43420EE9-0E0B-40F4-A4CA-68FC37CDD3AB", Caption = "Cession Management Flag")]
		[ReadOnlyMember(nameof(JI_CessionFlag_ReadOnly))]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CessionFlagList))]
		public ZString JI_CessionFlag
		{
			get { return AddInfo.ZG_CessionFlag; }
			set { AddInfo.ZG_CessionFlag = value; }
		}

		public ZPropertyInfo JI_CessionFlagInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_CessionFlag, x => AddInfo.ZG_CessionFlagInfo); }
		}

		public bool JI_CessionFlag_ReadOnly
		{
			get { return AddInfoLookups.CessionFlagList.Count == 0; }
		}

		[DecimalPlaces(Schema.JI_BondedWhsQuantityDecimalPlaces)]
		[ResourceStringData("EBBAC81D-2AF4-4BA5-8463-4772AD7BB4B4", Caption = "Warehouse Quantity", MediumCaption = "Warehouse Qty.", ShortCaption = "Whs. Qty.")]
		[ReadOnlyMember(nameof(JI_BondedWhsReadOnly))]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get => base.JI_BondedWhsQuantity;
			set => base.JI_BondedWhsQuantity = value;
		}

		[ReadOnlyMember(nameof(JI_BondedWhsReadOnly))]
		[ResourceStringData("7c7ad4bd-bc69-4a27-8524-560750f5b195", Caption = "Warehouse Unit of Quantity", ShortCaption = "Whs. UQ", MediumCaption = "Warehouse UQ")]
		public override ZString JI_BondedWhsUnitQty
		{
			get => base.JI_BondedWhsUnitQty;
			set => base.JI_BondedWhsUnitQty = value;
		}

		protected ZBool JI_BondedWhsReadOnly => !(IsOutOfWarehouseWarehousing || IsOutOfInwardProcessing || IsWarehouseAdjustment || (EntryInstruction?.IsInwardMovementApplicable ?? false));

		public void ClearInwardMovementQuantityIfRequired()
		{
			if (JI_BondedWhsReadOnly)
			{
				JI_BondedWhsQuantity = ZDecimal.Zero;
				JI_BondedWhsUnitQty = ZString.Empty;
			}
		}

		[ResourceStringData("FAF7AE5F-71B0-4C87-9FD4-CCC5384E518D", Caption = "Supplementary Info")]
		[MaxLength(Schema.SupplementaryInformationMaxLength)]
		public ZString SupplementaryInformation
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.SupplementaryInformation); }
			set
			{
				var oldValue = SupplementaryInformation;
				CheckMaximumLength(SupplementaryInformationInfo, value);
				this.SetSystemDefinedValue(Schema.SupplementaryInformation, value);
				SupplementaryInformationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SupplementaryInformationInfo
		{
			get { return GetZPropertyInfo(Schema.SupplementaryInformation); }
		}

		[ReadOnlyMember(nameof(IsProcedureInE01OrE02))]
		public ZDecimal JI_NetPrice
		{
			get => AddInfo.ZG_NetPrice;
			set
			{
				if (!inProcessDiscountCharge)
				{
					var oldValue = AddInfo.ZG_NetPrice;
					AddInfo.ZG_NetPrice = value;

					if (!IsCopying && oldValue != value)
					{
						ProcessDiscountChargeIfNeeded();
					}
				}
			}
		}

		public ZPropertyInfo JI_NetPriceInfo => GetWrappedZPropertyInfo(nameof(JI_NetPrice), x => AddInfo.ZG_NetPriceInfo);

		public ZString JI_RX_NKNetPriceCurr => JI_RX_NKLinePriceCurr;

		public ZPropertyInfo JI_RX_NKNetPriceCurrInfo => GetZPropertyInfo(nameof(JI_RX_NKNetPriceCurr));

		public override ZString JI_Tariff
		{
			get => base.JI_Tariff;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					var oldValue = JI_Tariff;
					base.JI_Tariff = value;
					if (!IsCopying && JI_Tariff != oldValue)
					{
						JI_TobaccoStampInfo.RefreshBinding();
						Declaration?.CustomsOffices?.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine => new TariffWrapper(invoiceLine));

		[ReadOnlyMember(nameof(JI_TobaccoStamp_ReadOnly))]
		[ResourceStringData("F76A8A1F-1A74-4AF1-AAEE-364FEDBCB0E7", Caption = "Tobacco Stamp")]
		public ZString JI_TobaccoStamp
		{
			get { return AddInfo.ZG_TobaccoStamp; }
			set { AddInfo.ZG_TobaccoStamp = value; }
		}
		public ZPropertyInfo JI_TobaccoStampInfo => GetWrappedZPropertyInfo(Schema.JI_TobaccoStamp, x => AddInfo.ZG_TobaccoStampInfo);

		protected bool JI_TobaccoStamp_ReadOnly => !CusLineTariffDetails.ToList().Any(x => x.UniversalTariff?.GetAttributes("ExciseType").Any(attribute => attribute.ZZ3_Value.EqualsIgnoringCase(CusLineTariffDetailHelper.ExciseTypes._10)) ?? false);

		[MaxLength(Schema.AdditionalInfoDescriptionMaxLength)]
		[ResourceStringData("C80E9C67-62A3-4D96-A0B9-E7A195B5079C", Caption = "Description")]
		public ZString AdditionalInfoDescription
		{
			get { return AdditionalInfos.Count == 0 ? ZString.Empty : AdditionalInfos[0].CSI_Description; }
			set
			{
				var oldValue = AdditionalInfoDescription;
				CheckMaximumLength(AdditionalInfoDescriptionInfo, value);
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						AdditionalInfos.RemoveAndDeleteAll();
					}
					else
					{
						var additionalInfo = AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault() ?? AdditionalInfos.AddNew();
						additionalInfo.CSI_Description = value;
					}
				}
				AdditionalInfoDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AdditionalInfoDescriptionInfo => GetZPropertyInfo(Schema.AdditionalInfoDescription);

		public ZBool JI_IsMainPack { get => AddInfo.ZG_IsMainPack; set => AddInfo.ZG_IsMainPack = value; }

		public ZPropertyInfo JI_IsMainPackInfo => GetWrappedZPropertyInfo(Schema.JI_IsMainPack, x => AddInfo.ZG_IsMainPackInfo);

		public override ZDecimal JI_Weight
		{
			get => base.JI_Weight;
			set
			{
				var oldValue = JI_Weight;
				base.JI_Weight = value;
				if (oldValue != JI_Weight)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString JI_WeightUQ
		{
			get => base.JI_WeightUQ;
			set
			{
				var oldValue = JI_WeightUQ;
				base.JI_WeightUQ = value;
				if (oldValue != JI_WeightUQ)
				{
					InvoiceHeader?.MarkAsNeedingValidation();
					Declaration?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("5E9FDDD2-C6A2-42FF-9C39-84306FF2B7E8", Caption = "Consignor")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConsignorList))]
		public override ZGuid JI_OA_ExporterAddress
		{
			get => base.JI_OA_ExporterAddress;
			set => base.JI_OA_ExporterAddress = value;
		}

		[ResourceStringData("ABDC32F7-2953-4D2D-9FB1-E75B3B2F1EB6", ShortCaption = "Ctry./Rgn. of Exp.", MediumCaption = "Ctry./Rgn. of Export", Caption = "Country/Region of Export")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ExportCountryList))]
		public override ZString JI_RN_NKCountryOfExport
		{
			get => base.JI_RN_NKCountryOfExport;
			set => base.JI_RN_NKCountryOfExport = value;
		}

		#region ContentInformationTypes
		[ChildEditable]
		public ContentInformationTypeCollection ContentInformationTypes
		{
			get
			{
				if (contentInformationTypes == null)
				{
					contentInformationTypes = new ContentInformationTypeCollection(this);
					contentInformationTypes.Load();
					RegisterEditableChildObject(contentInformationTypes);
				}

				return contentInformationTypes;
			}
		}
		ContentInformationTypeCollection contentInformationTypes;

		#endregion

		JobDeclaration IPreviousDocumentParentProvider.JobDeclaration => (JobDeclaration)base.Declaration;

		HugeSequenceNumberGenerator ISupportingDocumentMaster.LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(SupportingDocuments)));
		HugeSequenceNumberGenerator lineNumberGenerator;

		protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
		{
			var result = base.GetCusCodeDataTypes();
			result.Add(CusCodeDataTypeList.Codes.ContentInformationType, typeof(ContentInformationType));
			return result;
		}

		[ResourceStringData("C424C878-EFA8-4967-ABF7-C94290453D3C", Caption = "[38] Net Mass Measure")]
		public override ZDecimal JI_CustomsQuantity
		{
			get => base.JI_CustomsQuantity;
			set => base.JI_CustomsQuantity = value;
		}

		[DecimalPrecision(12)]
		[DecimalPlaces(3)]
		[ResourceStringData("1D199474-2F8E-4838-AB0A-09D4ECC256D0", Caption = "[44] Fourth Qty")]
		public override ZDecimal JI_CustomsFourthQuantity
		{
			get => base.JI_CustomsFourthQuantity;
			set => base.JI_CustomsFourthQuantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString JI_CustomsFourthUnitQty
		{
			get => base.JI_CustomsFourthUnitQty;
			set => base.JI_CustomsFourthUnitQty = value;
		}

		[DecimalPrecision(12)]
		[DecimalPlaces(nameof(JI_CustomsThirdQuantityDecimalPlaces))]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get => base.JI_CustomsThirdQuantity;
			set => base.JI_CustomsThirdQuantity = value;
		}

		ZInt JI_CustomsThirdQuantityDecimalPlaces => IsImport ? 3 : 5;

		public override ZInt MaxNumberOfAdditionalProcedureCode => IsImport ? 99 : 0;

		[ResourceStringData("E0651985-90A8-4E8C-8D8C-5F3121847F76|AdditionalProcedureCodesAsString", Caption = "Additional Procedures")]
		public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

		internal void ClearInwardProcessingDetailsIfNeeded()
		{
			ZG_EconomicConditions = ZString.Empty;
			ZG_IdentificationMeansType = ZString.Empty;
			JI_ExtraInfoForClassification = ZString.Empty;
			InwardProcessingProducts.RemoveAndDeleteAll();
		}

		[ChildEditable]
		public InwardProcessingProductCollection InwardProcessingProducts
		{
			get
			{
				if (inwardProcessingProducts == null)
				{
					inwardProcessingProducts = new InwardProcessingProductCollection(this);
					inwardProcessingProducts.Load();
					RegisterEditableChildObject(inwardProcessingProducts);
				}

				return inwardProcessingProducts;
			}
		}
		InwardProcessingProductCollection inwardProcessingProducts;

		[ResourceStringData("280320ED-2B17-4D3A-9992-AF2A1C0AEE53", Caption = "Economic Condition")]
		[ReadOnlyMember(nameof(IsInwardProcessingWithSimplifiedGrantAuthorizationNotEnabled))]
		public override ZString ZG_EconomicConditions
		{
			get => base.ZG_EconomicConditions;
			set => base.ZG_EconomicConditions = value;
		}

		[MaxLength(nameof(JI_CustomsUnitQty_MaxLength))]
		public override ZString JI_CustomsUnitQty
		{
			get => base.JI_CustomsUnitQty;
			set => base.JI_CustomsUnitQty = value;
		}

		/// If JI_CustomsUnitQty is not editable by user, then for export max length is 4 to be consistent with <see cref="JI_CustomsSecondUnitQty"/>
		int JI_CustomsUnitQty_MaxLength => JI_CustomsUnitQty_ReadOnly && IsExport ? 4 : 3;

		[ResourceStringData("0CFED26E-6A82-44D0-8455-DA8FE9F33A61", Caption = "Identification Means Type")]
		[ReadOnlyMember(nameof(IsInwardProcessingWithSimplifiedGrantAuthorizationNotEnabled))]
		public override ZString ZG_IdentificationMeansType
		{
			get => base.ZG_IdentificationMeansType;
			set => base.ZG_IdentificationMeansType = value;
		}

		bool IsInwardProcessingWithSimplifiedGrantAuthorizationNotEnabled
		{
			get
			{
				var instruction = EntryInstruction;
				return !(instruction != null && instruction.EnabledInwardProcessing && instruction.CEI_SimplifiedGrantAuthorization == SimplifiedGrantAuthorizationList.Codes.J);
			}
		}

		[ResourceStringData("828D1947-3302-4650-80E2-1C1897C01509", Caption = "Description")]
		public override ZString JI_ExtraInfoForClassification
		{
			get => base.JI_ExtraInfoForClassification;
			set => base.JI_ExtraInfoForClassification = value;
		}

		protected override bool JI_ExtraInfoForClassification_ReadOnly => ZG_IdentificationMeansType.IsEmpty;

		public override ZString JI_PrimaryPreference
		{
			get => base.JI_PrimaryPreference;
			set
			{
				var oldValue = JI_PrimaryPreference;
				base.JI_PrimaryPreference = value;
				if (!IsCopying && oldValue != JI_PrimaryPreference)
				{
					DefaultSupportingDocumentForEndUserExemption();
					UpdateZG_CountryOfSupplyIfNeeded();
				}
			}
		}

		internal void ClearCountryOfOriginIfNeeded()
		{
			if (EntryInstructionIsImportNotLUZ)
			{
				JI_RN_NKCountryOfExport = ZString.Empty;
			}
		}

		internal void ClearDecisiveDateIfNeeded()
		{
			if (EntryInstructionIsImportNotLUZ)
			{
				JI_CustomDate1 = ZDate.Empty;
			}
		}

		bool EntryInstructionIsImportNotLUZ => IsImport && (EntryInstruction?.CEI_Style ?? ZString.Empty) != ImportDeclarationTypeList.Codes.LUZ;

		void DefaultSupportingDocumentForEndUserExemption()
		{
			var existingTypes = SupportingDocuments.Cast<SupportingDocument>().Select(x => x.CSI_Code).ToHashSet();
			var requiredTypes = GetRequiredSupportingDocumentTypes();
			var typesToDelete = new HashSet<ZString>();

			foreach (var type in CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation)
			{
				if (requiredTypes.Contains(type))
				{
					if (!existingTypes.Contains(type))
					{
						SupportingDocuments.AddNew().CSI_Code = type;
					}
				}
				else if (existingTypes.Contains(type))
				{
					typesToDelete.Add(type);
				}
			}

			if (typesToDelete.Count > 0)
			{
				SupportingDocuments.Cast<SupportingDocument>().Where(doc => typesToDelete.Contains(doc.CSI_Code)).DeleteAll();
			}
		}

		HashSet<ZString> GetRequiredSupportingDocumentTypes()
		{
			var requiredTypes = new HashSet<ZString>();
			if (!JI_PrimaryPreference.IsEmpty)
			{
				var condition = UniversalTariff?.FilteredConditions.FirstOrDefault(x => x.PreferenceCode == JI_PrimaryPreference);
				if (condition != null)
				{
					requiredTypes = condition.ConditionValues
						.Where(x => x.ConditionValueType.ZX4_ValueType == Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument)
						.Select(x => x.ZX3_Value)
						.ToHashSet();
				}
			}
			return requiredTypes;
		}

		[ResourceStringData("5012B86C-A013-41FA-A685-75BE92832941", Caption = "Pref. Qty.")]
		public override ZInt ZG_QuotaQty
		{
			get => base.ZG_QuotaQty;
			set => base.ZG_QuotaQty = value;
		}

		[MaxLength(nameof(JI_DescriptionMaxLength))]
		public override ZString JI_Description
		{
			get => base.JI_Description;
			set => base.JI_Description = value;
		}

		int JI_DescriptionMaxLength => IsImport ? 240 : IsExport ? (Declaration.IsTransitionPeriodAES30 ? 280 : 512) : 280;

		[ResourceStringData("63C07F9A-9627-4F78-994B-2B4F88644947", Caption = "Usual Replacement")]
		public ZBool ZG_UsualReplacement
		{
			get => AddInfo.ZG_UsualReplacement;
			set => AddInfo.ZG_UsualReplacement = value;
		}

		public ZPropertyInfo ZG_UsualReplacementInfo => GetWrappedZPropertyInfo(Schema.ZG_UsualReplacement, x => AddInfo.ZG_UsualReplacementInfo);

		[ResourceStringData("2C210881-6514-41FD-AE64-516448C327B0", Caption = "Reimport Date")]
		public ZDateTime ZG_ReimportDate
		{
			get => AddInfo.ZG_ReimportDate;
			set => AddInfo.ZG_ReimportDate = value;
		}

		public ZPropertyInfo ZG_ReimportDateInfo => GetWrappedZPropertyInfo(Schema.ZG_ReimportDate, x => AddInfo.ZG_ReimportDateInfo);

		public ZDecimal ZG_NetPrice
		{
			get => AddInfo.ZG_NetPrice;
			set => AddInfo.ZG_NetPrice = value;
		}

		public ZPropertyInfo ZG_NetPriceInfo => GetWrappedZPropertyInfo(Schema.ZG_NetPrice, x => AddInfo.ZG_NetPriceInfo);

		[ResourceStringData("B1CB90A4-3476-42FA-87A3-0A70D9A90553", Caption = "Decisive Date")]
		public override ZDateTime JI_CustomDate1 { get => base.JI_CustomDate1; set => base.JI_CustomDate1 = value; }

		[ResourceStringData("db1d152f-e07a-45a2-b606-b4ea7b94f51f", Caption = "Warehouse Order Number", MediumCaption = "Whs. Order No.", ShortCaption = "Whs. Order")]
		public override ZString JI_BondedWHSOrderNumber { get => base.JI_BondedWHSOrderNumber; set => base.JI_BondedWHSOrderNumber = value; }

		[ResourceStringData("fdf19293-f0f6-450c-a36f-b54a6de89e56", Caption = "Warehouse Order Line", MediumCaption = "Whs. Order Line", ShortCaption = "Whs. Line")]
		public override ZShort JI_BondedWHSOrderLineNumber { get => base.JI_BondedWHSOrderLineNumber; set => base.JI_BondedWHSOrderLineNumber = value; }

		public bool AllLinkedPackagesHaveSamePackTypeAndMarks => Factory.GetValue(ref allLinkedPackagesHaveSamePackTypeAndMarks, () => CheckLinkedPackagesHaveSamePackTpeAndMarks());
		CachedProperty<bool> allLinkedPackagesHaveSamePackTypeAndMarks;

		bool CheckLinkedPackagesHaveSamePackTpeAndMarks()
		{
			var linkedPackages = PackagesPivot.Cast<InvoiceLinePackagePivot>().Select(x => x.Package);
			var firstPackage = linkedPackages.FirstOrDefault();
			return firstPackage == null || linkedPackages.Skip(1).All(x => x.CW_PackType == firstPackage.CW_PackType && x.CW_MarksAndNos == firstPackage.CW_MarksAndNos);
		}

		#endregion

		protected override ZString LanguageForTariffDescriptionCore() => Core.SharedConstants.Languages.German;

		public override ZDecimal JI_Calc_ValueForVat
		{
			get
			{
				return ShouldOverrideValueForVatWhenHasOpfCharge()
					? ValuationCalculator.GetValueForVat(this)
					: base.JI_Calc_ValueForVat;
			}
		}

		bool ShouldOverrideValueForVatWhenHasOpfCharge()
			=> IsImport && Charges.Cast<InvoiceLineCharge>().Any(c => c.J7_ChargeType == ImportChargeCodeList.Codes.OPF);

		public new DeCustomsValuationCalculator ValuationCalculator => (DeCustomsValuationCalculator)base.ValuationCalculator;

		protected override ICustomsValuationCalculator GetValuationCalculatorCore() => new DeCustomsValuationCalculator(this);

		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection(this);

		protected override bool SupportsAdditionalTariffs => true;

		protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore()
		{
			return new InvoiceLineCusLinkPackageCollection(this);
		}

		protected override Customs.Business.InvoiceLinePackageValidation GetNewLinkPackValidationCore(BaseCusLinkPackage linkPackage)
		{
			return new InvoiceLinePackageValidation(linkPackage, this);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			CalculateNetPriceIfNeeded(ZBool.False);
			UpdateJI_CustomsSecondQuantityIfNeeded();
		}

		public void CalculateNetPriceIfNeeded(ZBool manual, InvoiceLineCharge chargeToBeDeleted = null)
		{
			if (manual || JI_NetPrice.IsEmpty)
			{
				if (JI_LinePrice > 0m)
				{
					var invoiceHeader = InvoiceHeader;
					if (invoiceHeader != null && invoiceHeader.Invoice_Currency != null && invoiceHeader.IsHighValueOvrd)
					{
						var discountCharges = Charges.Cast<InvoiceLineCharge>().Where(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount && !x.J7_Amount.IsEmpty && x.PK != chargeToBeDeleted?.PK);
						var count = discountCharges.Count();
						var linePriceAmount = JI_LinePriceMoney.Amount;
						if (count == 1)
						{
							JI_NetPrice = linePriceAmount - discountCharges.First().MoneyInInvoiceCurrency.Amount;
						}
						else if (count == 0)
						{
							JI_NetPrice = linePriceAmount;
						}
					}
				}
			}
		}

		public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => (conditionType, valueType, inputValue) =>
		{
			var result = ZBool.False;
			if ((conditionType.In(new ZString[] { RefCusConditionTypes._420, RefCusConditionTypes._465, RefCusConditionTypes._474, RefCusConditionTypes._475, RefCusConditionTypes._477 }) || conditionType.StartsWith("7")) && IsImportIntoBondedWareHouseOrInwardProcessing)
			{
				result = ZBool.True;
			}
			else
			{
				result = base.EvaluateConditionValue(conditionType, valueType, inputValue);
			}
			return result;
		};

		protected override ZAddress GetNewJI_OA_ConsigneeAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ConsigneeAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZAddress GetNewJI_OA_ExporterAddress_ZAddress()
		{
			var result = base.GetNewJI_OA_ExporterAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;
		}

		protected override ZBool IsBondedWhsQuantityVisibleCore
		{
			get
			{
				var result = false;
				if (IsImport)
				{
					result = IsOutOfWarehouseWarehousing || (EntryInstruction?.IsInwardMovementApplicable ?? false) || (CusProcedure?.IsIntoVATWarehouse() ?? false);
				}
				else if (IsExport)
				{
					result = IsIntoOrOutOfRegimeProcedure;
				}
				else if (IsWarehouseAdjustment)
				{
					result = true;
				}
				return result;
			}
		}

		protected override ZBool IsPreviousEntryNumberVisibleCore
		{
			get
			{
				var result = false;
				if (IsImport || IsExport)
				{
					result = IsOutOfWarehouseWarehousing;
				}
				else if (IsWarehouseAdjustment)
				{
					result = true;
				}
				return result;
			}
		}

		protected override bool AtLeastOneBWHPropertyHasValue
		{
			get
			{
				var result = base.AtLeastOneBWHPropertyHasValue;
				if (IsExport)
				{
					result = result || !JI_BondedWHSOrderNumber.IsEmpty || !JI_BondedWHSOrderLineNumber.IsEmpty;
				}
				return result;
			}
		}

		ZGuid GetDefaultAddress(IOrgHeader orgHeader)
		{
			ZGuid result = ZGuid.Empty;
			if (orgHeader is OrgHeader organisation)
			{
				result = organisation.MainAddress.PK;
			}
			return result;
		}

		ZBool IsImportIntoBondedWareHouseOrInwardProcessing
		{
			get
			{
				if (IsImport)
				{
					var cusProcedure = CusProcedure;
					if (cusProcedure != null)
					{
						return cusProcedure.IsIntoWarehouse() || cusProcedure.IsIntoInwardProcessing();
					}
				}

				return false;
			}
		}

		#region ICusLinkPackageSupporter

		protected override ZBool IsSupportEmptyPackType(BasePackage package)
		{
			var result = base.IsSupportEmptyPackType(package);

			if (!result && IsExport && package is Package pack)
			{
				result = PackageHelper.IsSupportEmptyPackType(Factory, pack.CW_PackType);
			}

			return result;
		}

		#endregion

		void ProcessDiscountChargeIfNeeded()
		{
			var invoiceHeader = InvoiceHeader;
			if (IsImport && invoiceHeader.Invoice_Currency != null && invoiceHeader.IsHighValueOvrd)
			{
				inProcessDiscountCharge = true;

				var discountCharges = Charges.Cast<InvoiceLineCharge>().Where(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.Discount);
				if (JI_NetPrice != JI_LinePrice)
				{
					var amount = JI_LinePrice - JI_NetPrice;
					var discountChargesCount = discountCharges.Count();
					if (discountChargesCount == 0)
					{
						var discountCharge = Charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, amount);
						discountCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
						discountCharge.IsJ7_ExchangeRateUserEnterable = invoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable;
						discountCharge.J7_ExchangeRate = invoiceHeader.JZ_InvoiceCurrExRate;
						discountCharge.J7_ExchangeRateDate = invoiceHeader.JZ_InvoiceDate.Date;
					}
					else if (discountChargesCount == 1)
					{
						discountCharges.First().J7_Amount = amount;
					}
				}
				else
				{
					discountCharges.DeleteAll();
				}

				inProcessDiscountCharge = false;
			}
		}
		ZBool inProcessDiscountCharge = false;

		internal bool IsWarehouseAdjustment => Declaration != null && Declaration.IsWarehouseAdjustment;

		internal bool IsWarehouseAdjustmentOrInwardProcessingAVABR => Declaration != null && Declaration.IsWarehouseAdjustmentOrInwardProcessingAVABR;

		void UpdateZG_CountryOfSupplyIfNeeded()
		{
			if (IsImport && ZG_CountryOfSupply.IsEmpty)
			{
				var isPrefCodeGreaterThanOrEqual200 = int.TryParse(JI_PrimaryPreference, out var primaryPreferenceAsInteger) && primaryPreferenceAsInteger >= 200;
				if (isPrefCodeGreaterThanOrEqual200)
				{
					ZG_CountryOfSupply = JI_CountryOfOrigin;
				}
			}
		}

		void UpdateJI_PrimaryPreferenceIfNeeded()
		{
			if (IsImport)
			{
				var primaryPreferenceList = Lookups.PrimaryPreferenceList;
				if (primaryPreferenceList.Count == 1)
				{
					JI_PrimaryPreference = ((CodeDescriptionPairList)primaryPreferenceList)[0].Code;
				}
			}
		}

		void UpdateJI_StateOrRegionOfOriginIfNeeded()
		{
			if (IsExport && !JI_CountryOfOrigin.IsEmpty && JI_CountryOfOrigin != Core.Constants.CountryCodes.Germany)
			{
				JI_StateOrRegionOfOrigin = OriginFederalStateList.Codes.Ursprungsausland;
			}
		}

		void UpdateJI_CustomsSecondQuantityIfNeeded()
		{
			var invoiceUQ = JI_InvoiceUQ;
			if (!invoiceUQ.IsEmpty && Part is OrgSupplierPart part && part.OP_StockKeepingUnit == invoiceUQ && Pivot is CusClassPartPivot pivot)
			{
				var secondQtyInPivot = pivot.CI_SecondQty;
				if (!secondQtyInPivot.IsEmpty)
				{
					JI_CustomsSecondQuantity = secondQtyInPivot * JI_InvoiceQuantity;
				}
			}
		}

		internal void ClearFieldsForWarehouseAdjustment()
		{
			JI_Tariff = ZString.Empty;
			JI_SupplementaryCode1 = ZString.Empty;
			JI_SupplementaryCode2 = ZString.Empty;
			JI_Description = ZString.Empty;
			JI_CountryOfOrigin = ZString.Empty;
			JI_PrimaryPreference = ZString.Empty;
			JI_StateOrRegionOfOrigin = ZString.Empty;

			ZG_CountryOfSupply = ZString.Empty;
			JI_LinePrice = ZDecimal.Zero;
			JI_CessionFlag = ZString.Empty;
			JI_PartNo = ZString.Empty;
			JI_RH_NKCommodity_Code = ZString.Empty;
			JI_ConcessionOrder = ZString.Empty;
			ZG_QuotaQty = ZInt.Zero;
			ZG_QuotaUQ = ZString.Empty;
			SupplementaryInformation = ZString.Empty;

			JI_Weight = ZDecimal.Zero;
			JI_WeightUQ = ZString.Empty;
			JI_NetWeight = ZDecimal.Zero;
			JI_NetWeightUQ = ZString.Empty;
			JI_CustomsQuantity = ZDecimal.Zero;
			JI_CustomsUnitQty = ZString.Empty;
			JI_CustomsSecondQuantity = ZDecimal.Zero;
			JI_CustomsSecondUnitQty = ZString.Empty;
			JI_CustomsThirdQuantity = ZDecimal.Zero;
			JI_CustomsThirdUnitQty = ZString.Empty;
			JI_CustomsFourthQuantity = ZDecimal.Zero;
			JI_CustomsFourthUnitQty = ZString.Empty;
			JI_TobaccoStamp = ZString.Empty;
		}

		protected override bool IncludedInUniversalXMLCore => !ExcludeFromUniversalXML;

		bool ExcludeFromUniversalXML => IsIntoOrOutOfRegimeProcedure && CusEntryLine != null && CustomsStatusAttributeHelper.ShouldCancelBondedWhs(Factory, CusEntryLine.ZG_CustomsStatus, Core.Constants.CountryCodes.Germany, ZDateTime.UtcToday);

		protected override bool IncludeEntryDetailsInUniversalXMLCore => CusEntryLine != null && (CustomsStatusAttributeHelper.ShouldUpdateBondedWhs(Factory, CusEntryLine.ZG_CustomsStatus, Core.Constants.CountryCodes.Germany, ZDateTime.UtcToday)
			|| CusEntryLine.RandomLine.IsExport || CusEntryLine.RandomLine.IsWarehouseAdjustmentOrInwardProcessingAVABR);

		public bool IsExportWarehouseVisible => IsIntoOrOutOfRegimeProcedure || !JI_BondedWhsQuantity.IsEmpty || !JI_BondedWhsUnitQty.IsEmpty || !JI_PreviousEntryNumber.IsEmpty || !JI_PreviousEntryLineNumber.IsEmpty || !JI_BondedWHSOrderNumber.IsEmpty || !JI_BondedWHSOrderLineNumber.IsEmpty;

		protected override bool UseUniversalConditionCheck => (EntryInstruction == null || EntryInstruction.CEI_Style != ImportDeclarationTypeList.Codes.AVABR) && base.UseUniversalConditionCheck;
	}
}
