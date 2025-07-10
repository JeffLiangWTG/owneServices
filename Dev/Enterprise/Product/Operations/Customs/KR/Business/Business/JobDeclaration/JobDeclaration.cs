using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public partial class JobDeclaration : AutoKRJobDeclaration, ICusCodeDataTypeSupporter
		, ICurrencyConverterDataProvider
		, IInvoicesProvider
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobDeclaration.Schema
		{
			public const string CustomsMessageRemarks = "CustomsMessageRemarks";
			public const int CustomsMessageRemarksMaxLength = 500;
			public const string DetailedGoodsDescriptionRemarks = "DetailedGoodsDescriptionRemarks";
			public const int DetailedGoodsDescriptionRemarksMaxLength = 200;
			public const int CustomsOfficeMaxLength = 3;
			public const int LocationQualifierMaxLength = 5;
			public const int LocationOtherInformationMaxLength = 8;
			public const int MRNJ3_ReferenceNumberMaxLength = 11;
			public const int CustomsBrokerCommentCodeMaxLength = 1;
			public const int D87InvoiceCurrencyMaxLength = 3;
			public const int D87HBSplitDecIndMaxLength = 1;
			public const int CarnetCertificateNoMaxLength = 35;
			public const int CustomsLoadPortMaxLength = 2;
			public const int ImporterTypeMaxLength = 1;
			public const int JE_AuthorNameMaxLength = 12;
			public const int JE_AuditorNameMaxLength = 12;
			public const int KR_TaxOfficeMaxLength = 3;
			public const int YesNoCodeListMaxLength = 1;
		}

		[BusinessObjectTestExclude]
		public override ZGuid JE_OA_DeclarantAddress
		{
			get => base.JE_OA_DeclarantAddress;
			set
			{
				throw new InvalidOperationException("KR does not make use of the column.");
			}
		}

		[MaxLength(nameof(JE_ExportGoodsTypeMaxLength))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransactionTypeCodeList))]
		[ResourceStringData("CFC1C6EC-30D5-49AA-ACF2-E9D52CA57339", Caption = "Transaction Type", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("F32DBFEC-1A72-4C3F-8226-E209638CD24F", Caption = "Goods Type", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("F7A1155A-240E-42F6-8770-1598B7420B21", Caption = "Carnet Use", MultipleKey = KRJobMessageTypeList.Codes.Carnet)]
		public override ZString JE_ExportGoodsType
		{
			get => base.JE_ExportGoodsType;
			set
			{
				foreach (JobComInvoiceLine invoiceLine in FilteredInvoiceLines)
				{
					invoiceLine.MarkAsNeedingValidation();
				}

				var oldValue = JE_ExportGoodsType;
				base.JE_ExportGoodsType = value;
				if (!IsCopying && oldValue != JE_ExportGoodsType && !IsValidationSuspended)
				{
					Invoices?.MarkAsNeedingValidation();
				}

				if (JE_ProcedureTypeSetM && !IsEntrySentOrAccepted)
				{
					JE_ProcedureType = DeclarationProcedureTypeList.Codes.M;
				}
			}
		}
		int JE_ExportGoodsTypeMaxLength => (JE_MessageType == KRJobMessageTypeList.Codes.LocalExport || JE_MessageType == KRJobMessageTypeList.Codes.Carnet || JE_MessageType == KRJobMessageTypeList.Codes.PersonalItems) ? 1 : 3;

		[ResourceStringData("F6FA2C5B-0DE8-497E-BC5B-537DE163C300", Caption = "Has Items", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[MaxLength(Schema.YesNoCodeListMaxLength)]
		public ZString PIDHasItems
		{
			get { return JE_ExportGoodsType; }
			set
			{
				var oldValue = PIDHasItems;

				if (oldValue != value)
				{
					var args = new PIDHasItemsEventArgs(value);
					OnHasItemsChanging?.Invoke(this, args);

					if (!args.Cancel)
					{
						CheckMaximumLength(PIDHasItemsInfo, value);
						JE_ExportGoodsType = value;
						InvoiceLines.RemoveAndDeleteAll();

						if (PIDHasItems != YesNoList.Codes.Yes)
						{
							InvoiceLines.AddNew();
						}
					}
					JE_ExportGoodsTypeInfo.RefreshBinding();
					PIDHasItemsInfo.RefreshBinding();
				}
			}
		}
		public bool HasPIDItemLinesAboutToLose(ZString newHasItems) => IsPersonalItemDeclaration && PIDHasItems == YesNoList.Codes.Yes && newHasItems != YesNoList.Codes.Yes && InvoiceLines.Count > 0;

		public event CancelEventHandler OnHasItemsChanging;
		public ZPropertyInfo PIDHasItemsInfo => GetZPropertyInfo(nameof(PIDHasItems));

		public class PIDHasItemsEventArgs : CancelEventArgs
		{
			public PIDHasItemsEventArgs(ZString newHasItemsValue) : base(false)
			{
				this.PIDHasItemsNewValue = newHasItemsValue;
			}

			public readonly ZString PIDHasItemsNewValue;
		}
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				var oldValue = JE_MessageType;
				base.JE_MessageType = value;
				SetValidationModesBasedOnData();
				if (!IsCopying && oldValue != JE_MessageType)
				{
					if (IsSeparateDeclaration)
					{
						JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					}
					if (IsD87)
					{
						CreateInvoicesAndInvoiceLinesWhenMessageTypeIsD87();
					}
					RefreshIncotermAndChargeFactory();
					RefreshCusEntryInstructionProvider();
					DeleteTransportDetailsForNonTransportJob();
					RemoveEntryDateIfNotRelevant();
					RemoveDataIfNotRelevant();
					if (IsNonTransportDeclarationType)
					{
						RemovePortOfLoading();
						RemovePortOfArrival();
					}
					if (Is5SM)
					{
						CreateInvoice();
					}
					if (IsPersonalItemDeclaration)
					{
						var invoice = CreateInvoice();
						invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
						PIDHasItems = YesNo.Yes;
						CreateOrGetOFTCharge();
						InitializePIDQuestions();
					}
					if (!IsValidationSuspended)
					{
						DeclarationRefs?.MarkAsNeedingValidation();
						RefreshExRateToLatestRateAvailableIfNeeded();
						foreach (JobComInvoiceLine line in InvoiceLines)
						{
							line.VehicleNumbers.MarkAsNeedingValidation();
							line.RenewGAApprovalDataCollectionByTariff();
						}
					}
				}
			}
		}
		void CreateInvoicesAndInvoiceLinesWhenMessageTypeIsD87()
		{
			var invoice = CreateInvoice();
			invoice.InvoiceLines.AddNew();
		}

		void DeleteTransportDetailsForNonTransportJob()
		{
			if (IsNonTransportDeclarationType)
			{
				JE_TransportMode = ZString.Empty;
				JE_ContainerMode = ZString.Empty;
				CusContainers.RemoveAndDeleteAll();
			}
		}

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				base.JE_TransportMode = value;
				SetDefaultValueOfJE_ContainerPackMode();
			}
		}

		public override ZString JE_ContainerMode
		{
			get => base.JE_ContainerMode;
			set
			{
				base.JE_ContainerMode = value;
				SetDefaultValueOfJE_ContainerPackMode();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOfficeList))]
		[ResourceStringData("0445333B-F436-4193-8EDE-C059E286924C", Caption = "Customs Office")]
		[MaxLength(Schema.CustomsOfficeMaxLength)]
		public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

		public override ZGuid JE_OH_Forwarder
		{
			get => base.JE_OH_Forwarder;
			set
			{
				var oldValue = JE_OH_Forwarder;
				base.JE_OH_Forwarder = value;
				if (!IsCopying && oldValue != JE_OH_Forwarder && !IsValidationSuspended)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BondedAreaCodeList))]
		[ResourceStringData("544F7C68-2662-4DA9-A12C-70F96A3948DE", Caption = "Bonded Area")]
		[ResourceStringData("F4EAB96E-2030-4D35-8E49-1640B6EE80AB", Caption = "Bonded Area", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[MaxLength(Schema.LocationOtherInformationMaxLength)]
		public override ZString JE_LocationOtherInformation
		{
			get => base.JE_LocationOtherInformation;
			set
			{
				base.JE_LocationOtherInformation = value;
				if (!JE_LocationOtherInformation.IsEmpty)
				{
					SetBondedAreaName();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.JE_TotalNoOfPacksPackType_List))]
		public override ZString JE_TotalNoOfPacksPackType
		{
			get => base.JE_TotalNoOfPacksPackType;
			set
			{
				var oldValue = JE_TotalNoOfPacksPackType;
				base.JE_TotalNoOfPacksPackType = value;
				SetDefaultValueOfJE_ContainerPackMode();
				if (!IsCopying && oldValue != JE_TotalNoOfPacksPackType && !IsValidationSuspended)
				{
					CustomsEntryHeaders.MarkAsNeedingValidation();
					Invoices.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("7F23BB1C-31F9-4C62-BB9D-2BB98E091ED7", Caption = "Payment Type")]
		public override ZString JE_PaymentMethod
		{
			get => base.JE_PaymentMethod;
			set
			{
				var oldValue = JE_PaymentMethod;
				base.JE_PaymentMethod = value;
				if (!IsCopying && oldValue != JE_PaymentMethod && !IsValidationSuspended)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JE_PaymentMethod = ZString.Empty;
		}
		protected override string DefaultTotalNoOfPacksPackType => string.Empty;

		[MaxLength(Schema.CustomsLoadPortMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryCollection))]
		[ResourceStringData("B83626E2-0355-497D-AE78-04882E24B313", Caption = "Country of Dep.")]
		public override ZString JE_CustomsLoadPort { get => base.JE_CustomsLoadPort; set { base.JE_CustomsLoadPort = value; } }

		[ResourceStringData("14D03B05-D7FE-42CB-9634-ACA1A03EB885", Caption = "Exporter")]
		public override ZGuid JE_OH_Exporter { get => base.JE_OH_Exporter; set => base.JE_OH_Exporter = value; }

		[ResourceStringData("5C8DA7AA-4765-431F-BF1A-B087C2CC4B38", Caption = "Exporter")]
		public override ZGuid JE_OA_SellerAddress { get => base.JE_OA_SellerAddress; set => base.JE_OA_SellerAddress = value; }

		[ResourceStringData("0C5F9599-FDFB-4312-9EB0-9CD181D920AC", Caption = "UCR")]
		[ReadOnlyMember(nameof(JE_UCR_ReadOnly))]
		public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }
		internal bool JE_UCR_ReadOnly => !EarliestCustomsEntryIssueDate.IsEmpty;

		[ResourceStringData("97BB6F00-A666-478A-96EF-138FDFB1FDD5", Caption = "Return Reason")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ReturnReasonList))]
		public override ZString JE_ReturnReason { get => base.JE_ReturnReason; set => base.JE_ReturnReason = value; }

		[ResourceStringData("F9A4A729-3BCC-4155-AB8C-DFB921BFB21D", Caption = "Return Type")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ReturnTypeList))]
		public override ZString JE_ReturnType { get => base.JE_ReturnType; set => base.JE_ReturnType = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SouthNorthTradeYNList))]
		[ResourceStringData("22BD18BE-1785-47F6-832D-8AE2A2D1AD41", Caption = "South North Trade")]
		public override ZString JE_TradeIndicatorWithKP
		{
			get => base.JE_TradeIndicatorWithKP;
			set
			{
				base.JE_TradeIndicatorWithKP = value;
				foreach (var invoiceLine in InvoiceLines)
				{
					invoiceLine.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("1B51CFC5-E5F6-4CCA-BDF7-6E64D70C9F88", Caption = "South North Trade Area")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SouthNorthTradeIdentificationCodeList))]
		public override ZString JE_TradeIDWithKP { get => base.JE_TradeIDWithKP; set => base.JE_TradeIDWithKP = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoldTradeTransactionYCodeList))]
		[ResourceStringData("72A239B2-AD5A-4E3B-848A-30DD73D0545C", Caption = "Gold Trade Transaction YN")]
		public override ZString JE_GoldTrade { get => base.JE_GoldTrade; set => base.JE_GoldTrade = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclarationPlanList))]
		[ResourceStringData("008ADB73-7FD0-4067-82B2-1117123BC890", Caption = "Plan Type")]
		public override ZString JE_DeclarationPlan
		{
			get => base.JE_DeclarationPlan;
			set
			{
				base.JE_DeclarationPlan = value;
				MarkAsNeedingValidationIncludingChildren();
			}
		}

		[MaxLength("MessageSubTypeMaxLength")]
		[ResourceStringData("ED163960-DAC9-4D53-A89A-069069E11655", Caption = "Export Type", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		[ResourceStringData("ECF17103-7F56-446B-9FB2-FB30629F1110", Caption = "Declaration Type", MultipleKey = KRJobMessageTypeList.Codes.LocalExport)]
		[ResourceStringData("F3207C98-46C7-4798-994E-D97B55D5E53A", Caption = "Declaration Type", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ResourceStringData("0BC933B8-177C-4B1E-92BA-4390E5712B4D", Caption = "Stay Period", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZString JE_MessageSubType
		{
			get => base.JE_MessageSubType;
			set
			{
				var oldValue = base.JE_MessageSubType;
				base.JE_MessageSubType = value;
				RemoveEntryDateIfNotRelevant();
				RemoveDataIfNotRelevant();
				if (LocalExportTransactionNatureCodeList.IsChangedTo5DPOr5DQ(oldValue, value))
				{
					SetLocationOfGoods();
				}
				if (!IsInspectionDateRelevant)
				{
					InspectionDate = ZDateTime.Empty;
				}
			}
		}

		[ResourceStringData("B2681267-05B5-4501-AA9A-F0C53207CA92", Caption = "Port of Loading", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZString JE_RL_NKPortOfLoading { get => base.JE_RL_NKPortOfLoading; set => base.JE_RL_NKPortOfLoading = value; }
		[ResourceStringData("F2633814-2E16-4427-85AE-C0B2350BA652", Caption = "Foreign City", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[ResourceStringData("DAEBF8A6-DA4D-4E44-80FD-184ED4078FAB", Caption = "Port of Origin", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZString JE_RL_NKOrigin { get => base.JE_RL_NKOrigin; set => base.JE_RL_NKOrigin = value; }

		[ResourceStringData("2070DB89-4C42-43D7-B81F-63F8DE2D78ED", Caption = "Foreign Carrier", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZGuid JE_OH_ShippingLine { get => base.JE_OH_ShippingLine; set => base.JE_OH_ShippingLine = value; }

		[ResourceStringData("2EBD97E0-3B4B-4F58-9723-4A4C0276898D", Caption = "Total Qty", MultipleKey = KRJobMessageTypeList.Codes.Carnet)]
		public override ZInt JE_TotalNoOfPieces { get => base.JE_TotalNoOfPieces; set => base.JE_TotalNoOfPieces = value; }

		public override ZString JE_OwnerRef { get => base.JE_OwnerRef; set => base.JE_OwnerRef = value; }

		public bool IsInspectionDateRelevant => IsExport && JE_MessageSubType != ExportTypeCodeList.Codes.G;

		public override ZGuid JE_GB
		{
			get => base.JE_GB;
			set
			{
				if (value != base.JE_GB)
				{
					brokerAddress = null;
				}
				base.JE_GB = value;
			}
		}
		[ResourceStringData("53850A6F-4B7B-495D-8A96-B6E6AEA87B69", Caption = "Supplier", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		public override ZGuid JE_OH_Supplier
		{
			get => base.JE_OH_Supplier;
			set
			{
				base.JE_OH_Supplier = value;
				if (IsNonTransportDeclarationType)
				{
					RemovePortOfLoading();
				}

				if (IsPersistent && !IsCopying && JE_OH_Supplier.IsValid)
				{
					if (IsExport)
					{
						JE_OA_SupplierAddress = Supplier?.GetCustomsAddressThenMainAddress().PK ?? ZGuid.Empty;
					}
					else
					{
						foreach (JobComInvoiceHeader invoice in Invoices)
						{
							invoice.SetDefaultSupplierFromDeclaration();
						}
					}
				}
			}
		}

		[ResourceStringData("3F73B9A9-25D5-4B49-B9B1-916D879802C1", Caption = "Supplier")]
		public override ZGuid JE_OA_SupplierAddress { get => base.JE_OA_SupplierAddress; set => base.JE_OA_SupplierAddress = value; }

		[ResourceStringData("53744EE6-D0D9-4B12-A401-101A7572F659", Caption = "Manufacturer")]
		public override ZGuid JE_OH_Manufacturer
		{
			get => base.JE_OH_Manufacturer;
			set
			{
				base.JE_OH_Manufacturer = value;

				if (IsPersistent && !IsCopying && JE_OH_Manufacturer.IsValid)
				{
					foreach (var invoice in Invoices)
					{
						if (invoice.Manufacturer == null)
						{
							invoice.JZ_OH_Manufacturer = JE_OH_Manufacturer;
						}
					}
				}
			}
		}

		[ResourceStringData("4223A0C3-B7C8-40C2-A858-F43F7A9C11A6", Caption = "Manufacturer")]
		public override ZGuid JE_OA_ManufacturerAddress
		{
			get => base.JE_OA_ManufacturerAddress;
			set
			{
				base.JE_OA_ManufacturerAddress = value;

				if (IsPersistent && !IsCopying && JE_OA_ManufacturerAddress.IsValid)
				{
					foreach (var invoice in Invoices)
					{
						if (invoice.ManufacturerAddress == null)
						{
							invoice.JZ_OA_ManufacturerAddress = JE_OA_ManufacturerAddress;
						}
					}
				}
			}
		}

		[ResourceStringData("BF09304A-3CC5-464D-85C9-ACFFA0042E30", Caption = "Importer", MultipleKey = KRJobMessageTypeList.Codes.ValuationDeclaration)]
		[ResourceStringData("255C60EC-A458-4C8C-8641-B7B287552ADD", Caption = "Owner", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZGuid JE_OH_Importer
		{
			get => base.JE_OH_Importer;
			set
			{
				var oldValue = JE_OH_Importer;
				base.JE_OH_Importer = value;
				if (IsNonTransportDeclarationType)
				{
					RemovePortOfArrival();
				}
				if (IsImport)
				{
					SetDefaultValueToDutyPayer();
				}

				if (IsPersistent && !IsCopying && JE_OH_Importer.IsValid)
				{
					if (IsImport)
					{
						JE_OA_ImporterAddress = Importer?.GetCustomsAddressThenMainAddress().PK ?? ZGuid.Empty;
					}
					else
					{
						foreach (var invoice in Invoices)
						{
							if (invoice.Buyer == null)
							{
								invoice.JZ_OH_Buyer = JE_OH_Importer;
							}
						}
					}
				}

				if (oldValue != JE_OH_Importer)
				{
					VDAuthor = ZGuid.Empty;
					VDAuditor = ZGuid.Empty;
				}
			}
		}

		public override ZString JE_VoyageFlightNo
		{
			get => base.JE_VoyageFlightNo;
			set
			{
				base.JE_VoyageFlightNo = value;

				SetJE_RN_NKTransportNationality();
			}
		}

		[ResourceStringData("D905F581-A0F3-455C-850A-ADE02B703FED", Caption = "Importer")]
		public override ZGuid JE_OA_ImporterAddress { get => base.JE_OA_ImporterAddress; set => base.JE_OA_ImporterAddress = value; }

		[ResourceStringData("3CC7EB43-BB83-4C24-828F-8AA6AA4CB7B4", Caption = "Vessel Country")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryCollection))]
		public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

		public override ZString JE_VesselName
		{
			get => base.JE_VesselName;
			set
			{
				var oldValue = base.JE_VesselName;
				base.JE_VesselName = value;
				if (IsLocalExport && oldValue != JE_VesselName)
				{
					RegenerateMRNNumber(JE_MRNType);
				}
				SetJE_RN_NKTransportNationality();
			}
		}

		void SetJE_RN_NKTransportNationality()
		{
			if (IsImport && !VesselCountryCode.IsEmpty)
			{
				JE_RN_NKTransportNationality = VesselCountryCode;
			}
		}

		[MaxLength(nameof(CarnetCertificateNumberMaxLength))]
		[ResourceStringData("0E26A460-9197-4B6F-981A-9DA79BC73AFF", Caption = "Carnet Certificate No.", MultipleKey = ElectronicDocumentTypeList.Codes._D87)]
		public override ZString JE_AgentsReference { get => base.JE_AgentsReference; set => base.JE_AgentsReference = value; }
		int CarnetCertificateNumberMaxLength => IsD87 ? 20 : JobDeclaration.Schema.JE_AgentsReferenceMaxLength;

		[ResourceStringData("F4EDB5DE-B263-4FD6-B649-EC9D9B7EA1F2", Caption = "Effective To Date", MultipleKey = ElectronicDocumentTypeList.Codes._D87)]
		public override ZDate JE_EntryDate { get => base.JE_EntryDate; set => base.JE_EntryDate = value; }

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new JobDeclarationSynchroniser(this);

		[MaxLength(Schema.CustomsMessageRemarksMaxLength)]
		public ZString CustomsMessageRemarks
		{
			get => CustomsMessageRemarksNote.Text;
			set => CustomsMessageRemarksNote.SetNoteText(this, CustomsMessageRemarksInfo, value);
		}
		public ZPropertyInfo CustomsMessageRemarksInfo => GetZPropertyInfo(Schema.CustomsMessageRemarks);

		[MaxLength(Schema.DetailedGoodsDescriptionRemarksMaxLength)]
		[ResourceStringData("468C84A2-6C5E-48D0-8624-6B52770F85F3", Caption = "Representative Product Name")]
		public ZString DetailedGoodsDescriptionRemarks
		{
			get => DetailedGoodsDescriptionNote.Text;
			set => DetailedGoodsDescriptionNote.SetNoteText(this, DetailedGoodsDescriptionRemarksInfo, value);
		}

		public ZPropertyInfo DetailedGoodsDescriptionRemarksInfo => GetZPropertyInfo(Schema.DetailedGoodsDescriptionRemarks);

		[ResourceStringData("6A6AB46B-5E5E-43EC-BAFE-22A520879776", Caption = "Industrial Park Code")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.IndustrialParkCodeList))]
		public ZString IndustrialParkCode
		{
			get
			{
				var result = ManufacturerAddress?.CustomsCodes.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.IndustrialParkCode)?.OK_CustomsRegNo ?? ZString.Empty;
				if (!IsDeclarationProcedureTypeE && result.IsEmpty)
				{
					result = ManufacturerDefaultCode.IndustrialParkCode;
				}
				return result;
			}
		}
		public ZPropertyInfo IndustrialParkCodeInfo => GetZPropertyInfo(nameof(IndustrialParkCode));

		[ResourceStringData("B49F8552-E89A-41F6-A7E9-553BB6EF768F", Caption = "Final Bonded Warehouse")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BondedAreaCodeList))]
		public ZString FinalBondedWarehouse
		{
			get
			{
				var result = ContainerTerminalOperatorDocAddress?.Address?.CustomsCodes.GetCustomsRegNo(IdentificationType.ControlledPremisesID, Core.Constants.CountryCodes.KoreaSouth) ?? ZString.Empty;
				if (!IsDeclarationProcedureTypeE && result.IsEmpty)
				{
					result = ContainerTerminalOperatorDefaultCode.GetFinalBondedAreaCode(JE_CustomsOffice);
				}
				return result;
			}
		}
		public ZPropertyInfo FinalBondedWarehouseInfo => GetZPropertyInfo(nameof(FinalBondedWarehouse));

		public ZInt ImportTotalPackQty
		{
			get
			{
				var qty = 0;
				foreach (var instruction in CustomsEntryInstructions)
				{
					qty += instruction.CEI_PackQty;
				}

				return qty;
			}
		}
		public void SetJobDocAddressParentAsNeedingValidation()
		{
			if (ContainerTerminalOperatorDocAddress != null)
			{
				ContainerTerminalOperatorDocAddress.MarkParentAsNeedingValidation = true;
			}
		}

		HiddenTextNote CustomsMessageRemarksNote => customsMessageRemarksNote ?? (customsMessageRemarksNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description));
		HiddenTextNote customsMessageRemarksNote;

		HiddenTextNote DetailedGoodsDescriptionNote => detailedGoodsDescriptionNote ?? (detailedGoodsDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
		HiddenTextNote detailedGoodsDescriptionNote;

		public int MessageSubTypeMaxLength => IsExport ? 1 : 2;

		protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Stevedore:
					return StevedoreDocAddressRequirement;
				default:
					return base.GetDocAddressRequirement(addressType);
			}
		}

		public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			switch (JE_MessageType)
			{
				case KRJobMessageTypeList.Codes.LocalExport:
					return new LocalExportJobDocAddressValidation(addressToValidate, this);
				default:
					return null;
			}
		}

		[ResourceStringData("BE6C49D5-96B7-4227-AD10-1E95DBBA66A3", Caption = "Stevedore")]
		public ZGuid StevedoreCompanyAddress
		{
			get
			{
				return StevedoreCompany.E2_OA_Address;
			}
			set
			{
				StevedoreCompany.E2_OA_Address = value;
				StevedoreCompanyAddressInfo.RefreshBinding();
				if (IsLocalExport)
				{
					((LEXJobDeclarationValidation)Validation).ValidateStevedoreCompanyAddress();
				}
			}
		}

		ZAddress GetNewStevedoreCompanyE2_OA_Address_ZAddress()
		{
			return new ZAddress(StevedoreCompanyAddressInfo);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress StevedoreCompanyAddress_ZAddress
		{
			get
			{
				if (fStevedoreCompanyAddress_ZAddress == null)
				{
					fStevedoreCompanyAddress_ZAddress = GetNewStevedoreCompanyE2_OA_Address_ZAddress();
				}

				return fStevedoreCompanyAddress_ZAddress;
			}
		}

		ZAddress fStevedoreCompanyAddress_ZAddress;

		public virtual ZPropertyInfo StevedoreCompanyAddressInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(nameof(StevedoreCompanyAddress));
			}
		}

		public JobDocAddress StevedoreCompany
		{
			get
			{
				if (stevedoreJobDocAddress == null || stevedoreJobDocAddress.IsDeleted)
				{
					stevedoreJobDocAddress = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.Stevedore));
					stevedoreJobDocAddress.AdditionalValidation = PiggyBackedDocAddressValidation(stevedoreJobDocAddress);
				}

				return stevedoreJobDocAddress;
			}
		}
		JobDocAddress stevedoreJobDocAddress;

		JobDocAddressRequirement StevedoreDocAddressRequirement
		{
			get
			{
				if (stevedoreDocAddressRequirement == null)
				{
					stevedoreDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Stevedore);
					DocAddressManager.AddRequirement(stevedoreDocAddressRequirement);
				}
				return stevedoreDocAddressRequirement;
			}
		}
		JobDocAddressRequirement stevedoreDocAddressRequirement;

		public OrgAddress PayerAddress
		{
			get
			{
				if (payerAddress == null && DutyPayer != null)
				{
					payerAddress = DutyPayer.Addresses.CustomsAddress;
					if (payerAddress == null)
					{
						payerAddress = DutyPayer.Addresses.MainAddress;
					}
				}
				return payerAddress;
			}
		}
		OrgAddress payerAddress;

		public override bool UseImporterAddress => true;

		public OrgAddress BrokerAddress
		{
			get
			{
				if (brokerAddress == null && Branch != null)
				{
					var branchOrgProxy = Branch?.OrgProxy;
					if (branchOrgProxy != null)
					{
						brokerAddress = GetCustomsAddress(branchOrgProxy);
					}
					if (brokerAddress == null)
					{
						var companyOrgProxy = Company?.OrgProxy;
						if (companyOrgProxy != null)
						{
							brokerAddress = GetCustomsAddress(companyOrgProxy) ?? companyOrgProxy.MainAddress;
						}
					}
				}
				return brokerAddress;
			}
		}
		OrgAddress brokerAddress;

		public ZString BrokerRepresentativeName => BrokerAddress.Header.GetRepresentativeName();

		OrgAddress GetCustomsAddress(OrgHeader organisation)
		{
			return organisation.Addresses.Cast<OrgAddress>().SingleOrDefault(x => x.IsCustomsAddress);
		}

		#region CusVehicle
		[ChildEditable]
		CusVehicleCollection VehiclesCollection
		{
			get
			{
				if (vehiclesCollection == null)
				{
					vehiclesCollection = new CusVehicleCollection(this);
					vehiclesCollection.Load();
					RegisterEditableChildObject(vehiclesCollection);
				}
				return vehiclesCollection;
			}
		}
		CusVehicleCollection vehiclesCollection;

		CusVehicle VehicleData
		{
			get
			{
				if (vehicleData?.IsDeleted ?? true)
				{
					vehicleData = (CusVehicle)VehiclesCollection.FirstOrDefault();
				}
				return vehicleData;
			}
		}
		CusVehicle vehicleData;

		void VehicleDataIfRequired()
		{
			if (VehicleData == null)
			{
				VehiclesCollection.AddNew();
			}
		}
		[ResourceStringData("2841E344-54FC-4B67-A87E-4270C3F6C303", Caption = "Name")]
		[MaxLength(CusVehicle.Schema.CVH_ModelNameMaxLength)]
		public ZString ModelName
		{
			get
			{
				return VehicleData?.CVH_ModelName ?? ZString.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				CheckMaximumLength(ModelNameInfo, value);
				VehicleData.CVH_ModelName = value;
				ModelNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ModelNameInfo
		{
			get { return VehicleData == null ? GetZPropertyInfo(nameof(ModelName)) : GetWrappedZPropertyInfo(nameof(ModelName), x => vehicleData.CVH_ModelNameInfo); }
		}

		[ResourceStringData("6DC38BD0-88CF-48A3-A4E5-5B4F36FD4229", Caption = "VIN Reg. No.")]
		[MaxLength(CusVehicle.Schema.CVH_RegistrationNumberMaxLength)]
		public ZString VehicleIdentificationNumber
		{
			get
			{
				return VehicleData?.CVH_VehicleIdentificationNumber ?? ZString.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				CheckMaximumLength(VehicleIdentificationNumberInfo, value);
				VehicleData.CVH_VehicleIdentificationNumber = value;
				VehicleIdentificationNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VehicleIdentificationNumberInfo
		{
			get { return VehicleData == null ? GetZPropertyInfo(nameof(VehicleIdentificationNumber)) : GetWrappedZPropertyInfo(nameof(VehicleIdentificationNumber), x => vehicleData.CVH_VehicleIdentificationNumberInfo); }
		}

		[ResourceStringData("B3FB57D4-B3C9-4EE4-9F63-2E801CD2C551", Caption = "Exhaust Volume")]
		public ZShort EngineCapacity
		{
			get
			{
				return VehicleData?.CVH_EngineCapacity ?? ZShort.Zero;
			}
			set
			{
				VehicleDataIfRequired();
				VehicleData.CVH_EngineCapacity = value;
			}
		}
		[ResourceStringData("CF45E4D8-0EBB-41EF-99EC-2430B05A226D", Caption = "Model Year")]
		[MaxLength(CusVehicle.Schema.CVH_ModelYearMaxLength)]
		public ZString ModelYear
		{
			get
			{
				return VehicleData?.CVH_ModelYear ?? ZString.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				CheckMaximumLength(ModelYearInfo, value);
				VehicleData.CVH_ModelYear = value;
				ModelYearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ModelYearInfo
		{
			get { return VehicleData == null ? GetZPropertyInfo(nameof(ModelYear)) : GetWrappedZPropertyInfo(nameof(ModelYear), x => vehicleData.CVH_ModelYearInfo); }
		}

		[ResourceStringData("B7B777DD-24EF-48FB-927A-A8261C742F0E", Caption = "Manufacturing Country")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CountryCollection))]
		[MaxLength(AutoCusVehicle.Schema.CVH_RN_NKCountryOfManufactureMaxLength)]
		public ZString CountryOfManufacture
		{
			get
			{
				return VehicleData?.CVH_RN_NKCountryOfManufacture ?? ZString.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				CheckMaximumLength(CountryOfManufactureInfo, value);
				VehicleData.CVH_RN_NKCountryOfManufacture = value;
				CountryOfManufactureInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CountryOfManufactureInfo
		{
			get { return VehicleData == null ? GetZPropertyInfo(nameof(CountryOfManufacture)) : GetWrappedZPropertyInfo(nameof(CountryOfManufacture), x => vehicleData.CVH_RN_NKCountryOfManufactureInfo); }
		}

		[ResourceStringData("B66951B5-F723-487E-833A-FE9A28CE3032", Caption = "Seat Capacity")]
		public ZByte SeatingCapacity
		{
			get
			{
				return VehicleData?.CVH_Seats ?? ZByte.Zero;
			}
			set
			{
				VehicleDataIfRequired();
				VehicleData.CVH_Seats = value;
			}
		}
		[ResourceStringData("EF19AA33-EA4A-4563-B944-1054A8EFD845", Caption = "First Registration Date")]
		public ZDate DateOfFirstRegistration
		{
			get
			{
				return VehicleData?.CVH_DateOfFirstRegistration ?? ZDate.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				VehicleData.CVH_DateOfFirstRegistration = value;
			}
		}
		[ResourceStringData("091CB7E4-1233-400B-9A7D-F3BE5D06CA0C", Caption = "Current Registration Date")]
		public ZDate DateOfCurrentRegistration
		{
			get
			{
				return VehicleData?.CVH_DateOfCurrentRegistration ?? ZDate.Empty;
			}
			set
			{
				VehicleDataIfRequired();
				VehicleData.CVH_DateOfCurrentRegistration = value;
			}
		}

		#endregion

		[ChildEditable]
		public CusPersonCollection Persons
		{
			get
			{
				if (persons == null)
				{
					persons = new CusPersonCollection(this);
					persons.Load();
					RegisterEditableChildObject(persons);
				}
				return persons;
			}
		}
		CusPersonCollection persons;

		[ChildEditable]
		public TransportMeansCollection TransportMeans
		{
			get
			{
				if (transportMeans == null)
				{
					transportMeans = new TransportMeansCollection(this);
					transportMeans.Load();
					RegisterEditableChildObject(transportMeans);
				}
				return transportMeans;
			}
		}
		TransportMeansCollection transportMeans;

		[ChildEditable]
		public PersonalItemDecQuestionCollection PersonalItemDecQuestions
		{
			get
			{
				if (personalItemDecQuestions == null)
				{
					personalItemDecQuestions = new PersonalItemDecQuestionCollection(this);
					personalItemDecQuestions.Load();
					RegisterEditableChildObject(personalItemDecQuestions);
				}
				return personalItemDecQuestions;
			}
		}
		PersonalItemDecQuestionCollection personalItemDecQuestions;
		#region PIDItems
		[ResourceStringData("A88744EC-3D41-4FB9-9DF4-906310E71BBC", Caption = "Domestic Carrier")]
		public override ZGuid DeliveryOrPickupCartageCoPK
		{
			get => base.DeliveryOrPickupCartageCoPK;
			set
			{
				base.DeliveryOrPickupCartageCoPK = value;
				DeliveryOrPickupCartageCoPKInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidateDeliveryOrPickupCartageCoPK();
				}
			}
		}
		public new ZPropertyInfo DeliveryOrPickupCartageCoPKInfo => GetZPropertyInfo(nameof(DeliveryOrPickupCartageCoPK));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("8345EF08-5CF7-4C16-A1E3-31ED4E868726", Caption = "Weapon")]
		public ZString PIDWeapon
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingWeapon)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDWeaponInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingWeapon).CY_Data = value;
				PIDWeaponInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDWeapon();
				}
			}
		}
		public ZPropertyInfo PIDWeaponInfo => GetZPropertyInfo(nameof(PIDWeapon));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("09FE39C9-13D3-4E79-88BC-30EE6AABA1B3", Caption = "Drug")]
		public ZString PIDDrug
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingDrug)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDDrugInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingDrug).CY_Data = value;
				PIDDrugInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDDrug();
				}
			}
		}
		public ZPropertyInfo PIDDrugInfo => GetZPropertyInfo(nameof(PIDDrug));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("17D2A0E3-F65A-468E-AE76-49DF602F9FA5", Caption = "Animals")]
		public ZString PIDAnimal
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingLiveAnimal)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDAnimalInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingLiveAnimal).CY_Data = value;
				PIDAnimalInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDAnimal();
				}
			}
		}
		public ZPropertyInfo PIDAnimalInfo => GetZPropertyInfo(nameof(PIDAnimal));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("F7CA3B9F-2D5A-4ACA-BE11-8AECC7387146", Caption = "Endangered Items")]
		public ZString PIDEndangeredItems
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingEndangeredSpecies)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDEndangeredItemsInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingEndangeredSpecies).CY_Data = value;
				PIDEndangeredItemsInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDEndangeredItems();
				}
			}
		}
		public ZPropertyInfo PIDEndangeredItemsInfo => GetZPropertyInfo(nameof(PIDEndangeredItems));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("40B15DA8-192C-423B-8C85-26FF61F91599", Caption = "Counterfeit")]
		public ZString PIDCounterfeit
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingCounterfeitItem)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDCounterfeitInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingCounterfeitItem).CY_Data = value;
				PIDCounterfeitInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDCounterfeit();
				}
			}
		}
		public ZPropertyInfo PIDCounterfeitInfo => GetZPropertyInfo(nameof(PIDCounterfeit));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("57421035-746A-41BC-905B-B73AA4A99BC0", Caption = "Commercial Use Items")]
		public ZString PIDCommercialUseItems
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingCommercialUse)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDCommercialUseItemsInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingCommercialUse).CY_Data = value;
				PIDCommercialUseItemsInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDCommercialUseItems();
				}
			}
		}
		public ZPropertyInfo PIDCommercialUseItemsInfo => GetZPropertyInfo(nameof(PIDCommercialUseItems));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("7BE704AD-0557-4470-B659-A30918A9851B", Caption = "Excess Time Limit Items")]
		public ZString PIDExcessTimeLimitItems
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingItemBeyondDeclarationDueDate)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDExcessTimeLimitItemsInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingItemBeyondDeclarationDueDate).CY_Data = value;
				PIDExcessTimeLimitItemsInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDExcessTimeLimitItems();
				}
			}
		}
		public ZPropertyInfo PIDExcessTimeLimitItemsInfo => GetZPropertyInfo(nameof(PIDExcessTimeLimitItems));

		[MaxLength(Schema.YesNoCodeListMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[ResourceStringData("DFD17FB8-A114-4795-AACC-6E8B3D61FBD8", Caption = "Pornography")]
		public ZString PIDPornography
		{
			get
			{
				return GetPIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingPornography)?.CY_Data ?? ZString.Empty;
			}
			set
			{
				CheckMaximumLength(PIDPornographyInfo, value);
				GetOrCreatePIDDeclarationQuestion(Import008DecQuestion.DecQuestion.PossessingPornography).CY_Data = value;
				PIDPornographyInfo.RefreshBinding();
				if (IsPersonalItemDeclaration && !IsValidationSuspended)
				{
					var validation = (PIDJobDeclarationValidation)Validation;
					validation.ValidatePIDPornography();
				}
			}
		}
		public ZPropertyInfo PIDPornographyInfo => GetZPropertyInfo(nameof(PIDPornography));

		PersonalItemDecQuestion GetPIDDeclarationQuestion(string code) => PersonalItemDecQuestions.Where(x => x.CY_Code == code).FirstOrDefault();

		PersonalItemDecQuestion GetOrCreatePIDDeclarationQuestion(string code)
		{
			var result = GetPIDDeclarationQuestion(code);
			if (result == null)
			{
				result = PersonalItemDecQuestions.AddNew();
				result.CY_Code = code;
				result.CY_Data = Constants.YesNo.No;
			}
			return result;
		}

		void InitializePIDQuestions()
		{
			PIDWeapon = YesNo.No;
			PIDDrug = YesNo.No;
			PIDAnimal = YesNo.No;
			PIDEndangeredItems = YesNo.No;
			PIDCounterfeit = YesNo.No;
			PIDCommercialUseItems = YesNo.No;
			PIDExcessTimeLimitItems = YesNo.No;
			PIDPornography = YesNo.No;
		}

		[ResourceStringData("FDF39BFC-E8BD-46F5-81B1-4D91D2D00DB8", Caption = "Freight (KRW)")]
		[DecimalPlaces(Constants.DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal PIDFreightAmount
		{
			get
			{
				return GetOFTCharge()?.J7_Amount ?? ZDecimal.Zero;
			}
			set
			{
				var oFTCharge = CreateOrGetOFTCharge();
				oFTCharge.J7_Amount = value;
				oFTCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
				PIDFreightAmountInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo PIDFreightAmountInfo => GetZPropertyInfo(nameof(PIDFreightAmount));

		InvoiceCharge CreateOrGetOFTCharge()
		{
			oFTCharge ??= GetOFTCharge();
			if (oFTCharge == null)
			{
				oFTCharge = Invoices[0].Charges.AddNew();
				oFTCharge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasFreight;
			}
			return oFTCharge;
		}
		InvoiceCharge oFTCharge;

		InvoiceCharge GetOFTCharge() => Invoices[0].Charges.Cast<InvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == Common.CustomsChargeTypeList.Codes.OverseasFreight);
		#endregion
		JobComInvoiceHeader CreateInvoice() => Invoices.AddNew();

		public bool IsLocalExport => JE_MessageType == KRJobMessageTypeList.Codes.LocalExport;

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.TransportMean, typeof(TransportMeans) },
				{ CusCodeDataTypeList.Codes.PersonalItemDecQuestion, typeof(PersonalItemDecQuestion) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public ZDateTime GetEntryIssueDate(ZGuid entryPK)
		{
			ZDateTime result;
			if (!EntryIssueDates.TryGetValue(entryPK, out result))
			{
				result = ZDateTime.Today;
			}
			return result;
		}

		public void RefreshEntryIssueDates()
		{
			entryIssueDates = null;
		}

		Dictionary<ZGuid, ZDateTime> EntryIssueDates
		{
			get
			{
				if (entryIssueDates == null)
				{
					entryIssueDates = new Dictionary<ZGuid, ZDateTime>();
					foreach (CusEntryHeader entry in CustomsEntryHeaders)
					{
						if (entry.CusEntryNumber != null && !entry.CusEntryNumber.CE_IssueDate.IsEmpty)
						{
							entryIssueDates.Add(entry.PK, entry.CusEntryNumber.CE_IssueDate);
						}
					}
				}
				return entryIssueDates;
			}
		}
		Dictionary<ZGuid, ZDateTime> entryIssueDates;

		public ZDateTime LastestCustomsEntryIssueDate
		{
			get
			{
				ZDateTime zDateTime = ZDateTime.Empty;
				foreach (CusEntryHeader customsEntryHeader in CustomsEntryHeaders)
				{
					var cusEntryNumber = customsEntryHeader.CusEntryNumber;
					if (cusEntryNumber != null && (zDateTime.IsEmpty || cusEntryNumber.CE_IssueDate > zDateTime))
					{
						zDateTime = cusEntryNumber.CE_IssueDate;
					}
				}

				return zDateTime;
			}
		}
		public ZString VesselCountryCode
		{
			get
			{
				string vesselCountryCode = null;
				if (IsAir && !JE_VoyageFlightNo.IsEmpty)
				{
					var airlineCode = JE_VoyageFlightNo.SubstringSafe(0, 2);
					if (airlineCode.Length == 2)
					{
						var validAirline = RefAirline.LoadFromAirline2LetterCode(Factory, airlineCode);
						vesselCountryCode = validAirline?.RM_RN_NKAirlineCountry;
					}
				}
				else if (IsSea && !JE_VesselName.IsEmpty)
				{
					var validVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, JE_VesselName));
					vesselCountryCode = validVessel?.RV_RN_NKCountryOfReg;
				}
				return vesselCountryCode;
			}
		}

		public ZString VesselCountryKRCCode => GetCountryKRCCode(Factory, VesselCountryCode);
		public ZString DepartureCountryKRCCode => GetCountryKRCCode(Factory, JE_CustomsLoadPort);
		public RefCountry DepartureCountry => RefCountry.LoadFromCountryCode(Factory, JE_CustomsLoadPort);

		static ZString GetCountryKRCCode(BusinessObjectFactory factory, ZString countryCode)
		{
			ZString result = ZString.Empty;
			if (!countryCode.IsEmpty)
			{
				result = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.RefCusMap.CountryKRCCode, countryCode, ZDateTime.Now);
			}
			return result;
		}

		public ZString CustomsOfficeRefundDepartment
		{
			get
			{
				ZString result = CustomsOffice?.GetAttribute(Messaging.Constants.ZZ.CodeListAttributeNames.RefundDepartment) ?? ZString.Empty;
				if (result.IsEmpty)
				{
					result = JE_CustomsDivision;
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombined CustomsOffice
		{
			get
			{
				return MessageFunctions.GetRefCusCodeList(Factory, JE_CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			}
		}

		public ZString BondedAreaName => JE_LocationOtherInformation.IsEmpty ? ZString.Empty : MessageFunctions.GetRefCusCodeListDescription(Factory, JE_LocationOtherInformation, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode);

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ExporterTypeList))]
		[ResourceStringData("FD943F25-F416-43DD-9A3B-92ED67B10263", Caption = "Exporter Type")]
		public override ZString JE_ExporterType
		{
			get => base.JE_ExporterType;
			set
			{
				base.JE_ExporterType = value;
				Invoices?.MarkAsNeedingValidation();
				MarkAsNeedingValidation();
			}
		}

		[MaxLength(nameof(JE_ProcedureType_MaxLength))]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.DeclarationProcedureTypeList))]
		[ResourceStringData("17D04563-588D-4E79-AF85-979ABC7EA076", Caption = "Declaration Type")]
		[ResourceStringData("2E94F1C3-6D5C-4CD8-8A7A-6789658CD92E", Caption = "Import Type", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ReadOnlyMember(nameof(JE_ProcedureTypeReadOnly))]
		public override ZString JE_ProcedureType
		{
			get => base.JE_ProcedureType;
			set
			{
				base.JE_ProcedureType = value;
				MarkAsNeedingValidationIncludingChildren();
				if (DeclarationProcedureTypeCodeList.IsImportFromBondedAreaInKR(JE_ProcedureType) && JE_CustomsLoadPort != Core.Constants.CountryCodes.KoreaSouth)
				{
					JE_CustomsLoadPort = Core.Constants.CountryCodes.KoreaSouth;
				}
			}
		}
		public int JE_ProcedureType_MaxLength => IsImport ? 2 : 1;
		public ZBool IsDeclarationProcedureTypeE => JE_ProcedureType == DeclarationProcedureTypeList.Codes.E;
		public ZBool IsDeclarationProcedureTypeB => JE_ProcedureType == DeclarationProcedureTypeList.Codes.B;
		public ZBool IsDeclarationProcedureTypeM => JE_ProcedureType == DeclarationProcedureTypeList.Codes.M;

		internal bool JE_ProcedureTypeSetM => JE_ExportGoodsType == TransactionTypeCodeList.Codes._71 || JE_ExportGoodsType == TransactionTypeCodeList.Codes._78 || JE_ExportGoodsType == TransactionTypeCodeList.Codes._79;

		bool JE_ProcedureTypeReadOnly => IsExport && (IsEntrySentOrAccepted || JE_ProcedureTypeSetM);

		public bool IsEntrySentOrAccepted
		{
			get
			{
				bool result = false;
				foreach (CusEntryHeader customsEntryHeader in CustomsEntryHeaders)
				{
					if (CustomsMessageStatusTypeList.IsWaitingForResponse(customsEntryHeader.CH_Status) || customsEntryHeader.HasBeenLodgedAtCustoms)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public IDNumberAndType PayerBusinessNumber
		{
			get
			{
				if (DutyPayer == null)
				{
					return null;
				}
				return DutyPayer.GetIsIndividual() ?
					PayerAddress.GetRegistrationIDNumber(IdentificationType.KoreanRegNoForResident)
					: PayerAddress.GetRegistrationIDNumber(IdentificationType.CorporationCode);
			}
		}

		public ZString UNIPASSDeclarantID => KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty);
		public ZPropertyInfo UNIPASSDeclarantIDInfo => GetZPropertyInfo(nameof(UNIPASSDeclarantID));

		static ZString[] FirstDigitsOfSelfDeclaringOwnerUnipassID => new ZString[] { "6", "7", "8", "P" };

		public bool IsSelfDeclaringOwner
		{
			get
			{
				var firstDigit = UNIPASSDeclarantID.SubstringSafe(0, 1);

				return FirstDigitsOfSelfDeclaringOwnerUnipassID.Contains(firstDigit);
			}
		}

		#region UnderbondMovementEntryNumber
		Common.CusEntryNumber entryNumberUDM;

		Common.CusEntryNumber UnderbondMovementEntryNumber
		{
			get
			{
				if (entryNumberUDM == null)
				{
					foreach (Common.CusEntryNumber entryNumber in AdditionalReferenceNumbers)
					{
						entryNumber.MarkParentAsNeedingValidation = true;
					}
					entryNumberUDM = AdditionalReferenceNumbers.Cast<Common.CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == Messaging.Constants.UnderbondMovement);
				}
				return entryNumberUDM;
			}
		}
		public void SetUDMEntryNumberParentAsNeedingValidation()
		{
			if (UnderbondMovementEntryNumber != null)
			{
				UnderbondMovementEntryNumber.MarkParentAsNeedingValidation = true;
			}
		}
		void CreateOrDeleteEntryNumberUDMIfNeeded(IZType value)
		{
			if (entryNumberUDM == null && !value.IsEmpty)
			{
				entryNumberUDM = AdditionalReferenceNumbers.AddNew();
				entryNumberUDM.MarkParentAsNeedingValidation = true;
				entryNumberUDM.CE_EntryType = Messaging.Constants.UnderbondMovement;
			}
			else if (entryNumberUDM != null && value.IsEmpty)
			{
				entryNumberUDM.Delete();
				entryNumberUDM = null;
			}
		}

		[MaxLength(15)]
		[ResourceStringData("2B16589A-B918-4C03-9524-F6DCAD0A60BF", Caption = "Bonded Area ID", FullDescription = "Location ID In Bonded Area")]
		public override ZString JE_LocationIDInBondedArea { get => base.JE_LocationIDInBondedArea; set => base.JE_LocationIDInBondedArea = value; }

		[ResourceStringData("2ED277C0-068D-40BE-914A-FC687771E0E9", Caption = "Bonded Area Arr.", FullDescription = "Underbond Movement Arr.", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		[ResourceStringData("E83BF6DA-B4DD-423C-BACF-32AAA0FA3F0B", Caption = "Bonded Period", FullDescription = "Bonded Transportation Period", MultipleKey = KRJobMessageTypeList.Codes.Export)]
		public ZDateTime UnderbondMovementArrivalDate
		{
			get => UnderbondMovementEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				CreateOrDeleteEntryNumberUDMIfNeeded(value);
				if (UnderbondMovementEntryNumber != null)
				{
					UnderbondMovementEntryNumber.CE_IssueDate = value;
				}

				if (IsExport && !IsValidationSuspended)
				{
					var validation = (EXPJobDeclarationValidation)Validation;
					validation.ValidateUnderbondMovementArrivalDate();
				}
				else if (IsImport && !IsValidationSuspended)
				{
					var validation = (IMPJobDeclarationValidation)Validation;
					validation.ValidateUnderbondMovementArrivalDate();
				}

				UnderbondMovementArrivalDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnderbondMovementArrivalDateInfo => GetZPropertyInfo(nameof(UnderbondMovementArrivalDate));

		public ZDateTime UnderbondMovementDepartureDate
		{
			get => UnderbondMovementEntryNumber?.CE_ExpiryDate ?? ZDateTime.Empty;
			set
			{
				CreateOrDeleteEntryNumberUDMIfNeeded(value);
				if (UnderbondMovementEntryNumber != null)
				{
					UnderbondMovementEntryNumber.CE_ExpiryDate = value;
				}

				if (IsExport && !IsValidationSuspended)
				{
					var validation = (EXPJobDeclarationValidation)Validation;
					validation.ValidateUnderbondMovementDepartureDate();
				}

				UnderbondMovementDepartureDateInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo UnderbondMovementDepartureDateInfo => GetZPropertyInfo(nameof(UnderbondMovementDepartureDate));

		#endregion

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get
			{
				var result = ZDateTime.Today;
				if (Entries.Count > 0 && EarliestCustomsEntryIssueDate != ZDateTime.Empty)
				{
					var messageType = JE_MessageType;
					if (IsLocalExport)
					{
						if (LocalExportTransactionNatureCodeList.Is5DP(JE_MessageSubType))
						{
							messageType = ElectronicDocumentTypeList.Codes._5DP;
						}
						else if (LocalExportTransactionNatureCodeList.Is5DQ(JE_MessageSubType))
						{
							messageType = ElectronicDocumentTypeList.Codes._5DQ;
						}
					}

					if (!CustomsEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_MessageType == messageType).Any(entry => entry.CusEntryNumber.CE_IssueDate != EarliestCustomsEntryIssueDate))
					{
						result = EarliestCustomsEntryIssueDate;
					}
				}
				return result;
			}
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType => ExchangeRateTypeDecider.GetExchangeRateType(JE_MessageType);

		int ICurrencyConverterDataProvider.MaximumDaysToFallback => 0;
		GlbCompany ICurrencyConverterDataProvider.Company => Company;
		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride => LocalCurrencyCode;
		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride => IsReciprocalRatesConstant;
		#endregion

		protected override ZBool IsReciprocalRatesCore => IsReciprocalRatesConstant;

		static bool IsReciprocalRatesConstant => true;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsDivisonList))]
		[ResourceStringData("0449A4B6-9E37-40C4-A82A-D142D34964A6", Caption = "Department", FullDescription = "Customs Department")]
		public override ZString JE_CustomsDivision
		{
			get => base.JE_CustomsDivision;
			set
			{
				base.JE_CustomsDivision = value;
				MarkAsNeedingValidation();
			}
		}

		[ResourceStringData("67D23F7B-7647-4443-8BCD-EF7961F78848", Caption = "Dest. Country")]
		public override ZString JE_GoodsDestination
		{
			get => base.JE_GoodsDestination;
			set
			{
				base.JE_GoodsDestination = value;
				MarkAsNeedingValidationIncludingChildren();
			}
		}

		JobService ExtraInspectionData
		{
			get
			{
				if (extraInspectionData == null || extraInspectionData.IsDeleted || extraInspectionData.ES_ServiceCode != Core.Constants.FreightServiceType.Codes.ExtraInspection)
				{
					foreach (JobService service in DocsAndCartage.Services)
					{
						service.MarkParentAsNeedingValidation = true;
					}
					extraInspectionData = DocsAndCartage.Services.Cast<JobService>().FirstOrDefault(x => x.ES_ServiceCode == Core.Constants.FreightServiceType.Codes.ExtraInspection);
				}
				return extraInspectionData;
			}
		}
		JobService extraInspectionData;

		public void SetExtraInspectionDataParentAsNeedingValidation()
		{
			if (ExtraInspectionData != null)
			{
				ExtraInspectionData.MarkParentAsNeedingValidation = true;
			}
		}

		[ResourceStringData("BA35C410-50CE-4049-BDBB-3F0EBB0A2C70", Caption = "Inspection Date", FullDescription = "Preferred Inspection Date")]
		public ZDateTime InspectionDate
		{
			get
			{
				return ExtraInspectionData?.ES_Booked ?? ZDateTime.Empty;
			}
			set
			{
				if (ExtraInspectionData == null)
				{
					extraInspectionData = DocsAndCartage.Services.AddNew();
					extraInspectionData.MarkParentAsNeedingValidation = true;
					extraInspectionData.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;
				}
				ExtraInspectionData.ES_Booked = value;
				InspectionDateInfo.RefreshBinding();

				if (IsExport && !IsValidationSuspended)
				{
					var validation = (EXPJobDeclarationValidation)Validation;
					validation.ValidateInspectionDate();
				}
			}
		}
		public ZPropertyInfo InspectionDateInfo => GetZPropertyInfo(nameof(InspectionDate));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SimpleDRWAppList))]
		[ResourceStringData("EB5A2B4B-6073-49DB-AA96-3DA78A4AF48B", Caption = "Auto Drawback")]
		public override ZString JE_SimpleDRWApp
		{
			get => base.JE_SimpleDRWApp;
			set
			{
				base.JE_SimpleDRWApp = value;
				Invoices?.MarkAsNeedingValidation();
				MarkAsNeedingValidation();

				if (JE_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD)
				{
					foreach (JobComInvoiceHeader invoice in Invoices)
					{
						invoice.JZ_DRWApplicantType = DrawbackApplicantTypeList.Codes.Manufacturer;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OutOfHoursDecIndList))]
		[ResourceStringData("F49776AD-E564-49FF-80C9-CFDFBC7D02F4", Caption = "Out Of Hours", FullDescription = "Out Of Hours Declaration")]
		public override ZString JE_OutOfHoursDecInd { get => base.JE_OutOfHoursDecInd; set => base.JE_OutOfHoursDecInd = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GoodsConditionList))]
		[ResourceStringData("710EEE21-9361-4EE1-8036-E649034EB595", Caption = "Goods Status")]
		public override ZString JE_GoodsCondition { get => base.JE_GoodsCondition; set => base.JE_GoodsCondition = value; }

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PortOfLoadings))]
		[ResourceStringData("8B011342-466C-4720-BC5C-24EBD69D6E1E", Caption = "Port of Transshipment")]
		public override ZString JE_TransshipmentPort
		{
			get => base.JE_TransshipmentPort;
			set => base.JE_TransshipmentPort = value;
		}

		public ZString TransshipmentYN => JE_TransshipmentPort.IsEmpty ? YesNo.No : YesNo.Yes;
		public ZString TransshipmentCountryCode => JE_TransshipmentPort.SubstringSafe(0, 2);

		[ResourceStringData("4627EDEE-7D76-4FB1-9124-A111AC542509", Caption = "Arrival Date", ShortCaption = "Arr.", MediumCaption = "Arrival")]
		public override ZDateTime JE_TransshipmentDate { get => base.JE_TransshipmentDate; set => base.JE_TransshipmentDate = value; }

		[ResourceStringData("74C81EE3-F37F-4D79-87BA-25B5DC0C67D7", Caption = "Goods Loc. Address", FullDescription = "Goods Location Address")]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set => base.JE_LocationOfGoods = value;
		}

		[ResourceStringData("340A2136-4984-4D06-8DE4-DA8505C6E863", Caption = "Goods Loc. Details", FullDescription = "Goods Location Details")]
		public override ZString JE_SubLocationOfGoods
		{
			get => base.JE_SubLocationOfGoods;
			set => base.JE_SubLocationOfGoods = value;
		}

		[ResourceStringData("93140B56-1DEB-45E5-A75B-1B96626E1CBD", Caption = "Goods Loc. Postcode", FullDescription = "Goods Location Post code")]
		[MaxLength(Schema.LocationQualifierMaxLength)]
		public override ZString JE_LocationQualifier
		{
			get => base.JE_LocationQualifier;
			set => base.JE_LocationQualifier = value;
		}

		[LightValidationTestExempt]
		public override ZString JE_AddInfo
		{
			get => base.JE_AddInfo;
			set => base.JE_AddInfo = value;
		}

		[LightValidationTestExempt]
		public override ZString JE_NAddInfo
		{
			get => base.JE_NAddInfo;
			set => base.JE_NAddInfo = value;
		}

		[MaxLength(Schema.KR_TaxOfficeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TaxOfficeList))]
		[ResourceStringData("660288FC-A767-4DEB-81F4-6AB42EA8DD14", Caption = "Tax Office")]
		public override ZString JE_TaxOffice { get => base.JE_TaxOffice; set => base.JE_TaxOffice = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ContainerPackKRList))]
		[ResourceStringData("6A34E93A-1199-4BA6-B012-B45EF704C136", Caption = "Container Pack")]
		[BusinessObjectTestExclude]
		public override ZString JE_ContainerPackMode
		{
			get => base.JE_ContainerPackMode;
			set
			{
				base.JE_ContainerPackMode = value;
				JE_ContainerPackModeInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OrgContactCollectionList))]
		[ResourceStringData("663FADCC-FF7D-4145-90D9-F13FDB10834D", Caption = "Name")]
		public ZGuid VDAuthor
		{
			get
			{
				if (vDAuthor.IsEmpty && Importer != null)
				{
					vDAuthor = Importer.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.Name == JE_AuthorName)?.PK ?? ZGuid.Empty;
				}
				return vDAuthor;
			}
			set
			{
				if (SetNonPersistentPropertyValue(VDAuthorInfo, ref vDAuthor, value))
				{
					var author = Factory.Load<OrgContact>(vDAuthor);
					JE_AuthorName = author?.Name.Left(Schema.JE_AuthorNameMaxLength) ?? ZString.Empty;
					JE_AuthorPhone = author?.OC_Phone ?? ZString.Empty;
					JE_AuthorJobTitle = author?.OC_Title ?? ZString.Empty;
					VDAuditorInfo.RefreshBinding();
				}
			}
		}
		ZGuid vDAuthor;

		public ZPropertyInfo VDAuthorInfo => GetZPropertyInfo(nameof(VDAuthor));

		[ResourceStringData("B5936578-38D9-41F1-981B-B69C2B6B8324", Caption = "Phone")]
		public override ZString JE_AuthorPhone
		{
			get => base.JE_AuthorPhone; set { base.JE_AuthorPhone = value; }
		}

		[ResourceStringData("C457C163-8BDE-4517-908F-A8B2F80613E3", Caption = "Job Title")]
		public override ZString JE_AuthorJobTitle { get => base.JE_AuthorJobTitle; set => base.JE_AuthorJobTitle = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.OrgContactCollectionList))]
		[ResourceStringData("663FADCC-FF7D-4145-90D9-F13FDB10834D", Caption = "Name")]
		public ZGuid VDAuditor
		{
			get
			{
				if (vDAuditor.IsEmpty && Importer != null)
				{
					vDAuditor = Importer.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.Name == JE_AuditorName)?.PK ?? ZGuid.Empty;
				}
				return vDAuditor;
			}
			set
			{
				if (SetNonPersistentPropertyValue(VDAuditorInfo, ref vDAuditor, value))
				{
					var auditor = Factory.Load<OrgContact>(vDAuditor);
					JE_AuditorName = auditor?.Name.Left(Schema.JE_AuditorNameMaxLength) ?? ZString.Empty;
					JE_AuditorPhone = auditor?.OC_Phone ?? ZString.Empty;
					JE_AuditorJobTitle = auditor?.OC_Title ?? ZString.Empty;
					VDAuditorInfo.RefreshBinding();
				}
			}
		}
		ZGuid vDAuditor;

		public ZPropertyInfo VDAuditorInfo => GetZPropertyInfo(nameof(VDAuditor));

		[ResourceStringData("B5936578-38D9-41F1-981B-B69C2B6B8324", Caption = "Phone")]
		public override ZString JE_AuditorPhone { get => base.JE_AuditorPhone; set => base.JE_AuditorPhone = value; }

		[ResourceStringData("C457C163-8BDE-4517-908F-A8B2F80613E3", Caption = "Job Title")]
		public override ZString JE_AuditorJobTitle { get => base.JE_AuditorJobTitle; set => base.JE_AuditorJobTitle = value; }

		void SetDefaultValueOfJE_ContainerPackMode()
		{
			if (JE_ContainerPackMode.IsEmpty)
			{
				if (JE_TransportMode == Core.Constants.TransportModes.Sea && JE_ContainerMode == Core.Constants.ContainerModes.Bulk)
				{
					JE_ContainerPackMode = ContainerPackModeCodeList.Codes.BU;
				}
				else if (JE_TotalNoOfPacksPackType == PackageKindCodeList.Codes.RO)
				{
					JE_ContainerPackMode = ContainerPackModeCodeList.Codes.RO;
				}
			}
		}

		public ZString KRPortOfLoading
		{
			get
			{
				var result = ZString.Empty;
				if (JE_TransportMode == TransportTypeList.Codes.Air)
				{
					result = PortOfLoading?.RL_IATA ?? ZString.Empty;
				}
				else if (JE_TransportMode == TransportTypeList.Codes.Sea)
				{
					result = JE_RL_NKPortOfLoading;
				}
				else if (JE_TransportMode == TransportTypeList.Codes.Mail)
				{
					result = !JE_IATALoadPort.IsEmpty ? JE_IATALoadPort : JE_RL_NKPortOfLoading;
				}
				return result;
			}
		}
		public ZString LoadPortNameInKorean => ExtensionMethods.GetLoadPortNameInKorean(Factory, KRPortOfLoading);

		JobDecRefs JobDecRefsData
		{
			get
			{
				if (jobDecRefsData?.IsDeleted ?? true)
				{
					jobDecRefsData = DeclarationRefs.Where(item => item.J3_ReferenceType == Constants.MRN)?.FirstOrDefault();
				}
				return jobDecRefsData;
			}
		}
		JobDecRefs jobDecRefsData;

		[ReadOnlyMember(nameof(IsSpecialMRNType))]
		[MaxLength(Schema.MRNJ3_ReferenceNumberMaxLength)]
		[ResourceStringData("a7b9311e-533b-4c41-afff-a3de60476b17", Caption = "MRN No")]
		public ZString MRNJ3_ReferenceNumber
		{
			get => JobDecRefsData?.J3_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (JobDecRefsData == null)
				{
					var declarationRef = DeclarationRefs.AddNew();
					declarationRef.J3_ReferenceType = Constants.MRN;
				}

				CheckMaximumLength(MRNJ3_ReferenceNumberInfo, value);
				JobDecRefsData.J3_ReferenceNumber = value;
				MRNJ3_ReferenceNumberInfo.RefreshBinding();
				if (!IsValidationSuspended && IsLocalExport)
				{
					((LEXJobDeclarationValidation)Validation).ValidateMRNNo();
				}
			}
		}

		[ResourceStringData("5D3A13C9-82F9-4472-B0E9-372EE1C82AA4", Caption = "Total Invoice Amount")]
		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal TotalInvoiceAmountValue => TotalInvoiceAmount.Amount;

		bool IsSpecialMRNType => MRNTypeList.IsSpecialType(JE_MRNType);
		public ZPropertyInfo MRNJ3_ReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(MRNJ3_ReferenceNumber)); }
		}
		[ResourceStringData("47399cc1-e652-428c-8c31-703a135a7afc", Caption = "Vessel Radio Call Sign")]
		public ZString VesselRadioCallSign => Vessel?.RV_RadioCallSign ?? ZString.Empty;
		public bool IsLocalExportToSeaVessel => JE_MessageType == KRJobMessageTypeList.Codes.LocalExport && LocalExportTransactionNatureCodeList.IsSea(JE_MessageSubType);
		public bool IsLocalExportToAirplane => JE_MessageType == KRJobMessageTypeList.Codes.LocalExport && LocalExportTransactionNatureCodeList.IsAir(JE_MessageSubType);

		#region override
		protected override bool SupportContainerEntryHeaderPivot => true;
		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;
		protected override bool IsCustomsLineAmendmentATotalReplacement => false;
		protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => false;
		protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore => false;
		protected override bool SupportDeclarationRefs => true;
		protected override bool ShouldKeepDeletedLinesOnAmendmentCore => false;

		protected override bool SupportEntrySnpashotsCore => true;
		public override bool SupportInvoiceLineRefs => true;

		protected override ZString LocalCurrencyCodeCore
		{
			get { return Core.Constants.CurrencyCodes.KoreaRepublicOf; }
		}

		protected override bool HasSplitEntriesCore => true;
		public ZBool IsEntryInstructionRelevant => IsImport || IsExport;
		public override ZBool AreMultipleEntryInstructionsAllowed => IsEntryInstructionRelevant;
		protected override DocumentSupporter CreateNewDocumentSupporter() => new JobDeclarationDocumentSupporter(this);
		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);
		void RefreshCusEntryInstructionProvider()
		{
			CustomsEntryInstructionProvider?.CustomsEntryInstructions?.RemoveAndDeleteAll();
		}
		protected override bool IsEntryInstructionRequiredCore => IsEntryInstructionRelevant;
		protected override ZString GetMessageTypeForDocumentFilter() => JE_MessageType;
		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);
		protected override string GetIApportionInvoiceHolderCountryContextCore() => CountryCode + this.GetIncoTermChargeFactoryCacheKey();

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType);
		}

		protected override void ResetValuesOnTemplateCopyAfterClone(BaseJobDeclaration declaration, CloneType cloneType)
		{
			base.ResetValuesOnTemplateCopyAfterClone(declaration, cloneType);

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				using (invoiceLine.GetValidationSuspender())
				{
					using (invoiceLine.SuspendSettingHasChanges())
					{
						invoiceLine.KR_HighestGAApprovalSeqNo = ZShort.Zero;
						invoiceLine.KR_HighestVehicleSeqNo = ZShort.Zero;
						invoiceLine.JI_SequenceNumber = ZShort.Zero;
					}
				}
			}
		}

		protected override void WarehouseDocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			base.WarehouseDocAddress_DocAddressChanged(sender, e);
			SetLocationOfGoods();
		}

		void SetLocationOfGoods()
		{
			SetBondedAreaCode();
			SetBondedAreaName();
			SetBondedAreaAddress();
			SetBondedAreaPostCode();
		}

		bool IsBondedAreaCodeRelevant => !IsLocalExport || LocalExportTransactionNatureCodeList.Is5DP(JE_MessageSubType);
		void SetBondedAreaCode()
		{
			if (IsBondedAreaCodeRelevant)
			{
				if (JE_LocationOtherInformation.IsEmpty)
				{
					var cusCode = WarehouseDocAddress.Address?.CustomsCodes.FirstOrDefault(x => x.OK_CodeType == Constants.IdentificationType.ControlledPremisesID);
					if (cusCode != null)
					{
						JE_LocationOtherInformation = cusCode.OK_CustomsRegNo.SubstringSafe(0, 35);
					}
					else if (WarehouseDocAddress.E2_GovRegNumType == Constants.IdentificationType.ControlledPremisesID)
					{
						JE_LocationOtherInformation = WarehouseDocAddress.E2_GovRegNum;
					}
				}
			}
			else
			{
				JE_LocationOtherInformation = ZString.Empty;
			}
		}

		bool IsBondedAreaNameRelevant => IsExport || IsLocalExport && LocalExportTransactionNatureCodeList.Is5DP(JE_MessageSubType);

		void SetBondedAreaName()
		{
			if (IsBondedAreaNameRelevant)
			{
				JE_SubLocationOfGoods = (JE_LocationOtherInformation.IsEmpty ? WarehouseDocAddress.CompanyName : BondedAreaName).Left(Schema.JE_SubLocationOfGoodsMaxLength);
			}
			else
			{
				JE_SubLocationOfGoods = ZString.Empty;
			}
		}

		void SetBondedAreaAddress()
		{
			if (IsExport)
			{
				JE_LocationOfGoods = JE_LocationOfGoods.IsEmpty ? WarehouseDocAddress.Address1 : JE_LocationOfGoods;
			}
			else
			{
				JE_LocationOfGoods = ZString.Empty;
			}
		}

		void SetBondedAreaPostCode()
		{
			if (IsExport)
			{
				JE_LocationQualifier = JE_LocationQualifier.IsEmpty ? WarehouseDocAddress.Postcode : JE_LocationQualifier;
			}
			else
			{
				JE_LocationQualifier = ZString.Empty;
			}
		}

		public override bool IsNonTransportDeclarationType => IsLocalExport || IsMiscDeclaration;

		protected override bool JE_MessageType_ReadOnlyCore => base.JE_MessageType_ReadOnlyCore || IsMiscDeclaration;

		public bool IsMiscDeclaration => IsD87 || IsPersonalItemDeclaration || Is5SM;

		public override bool UseSupplierAddress => true;

		protected override bool IsContainerPackingRequiredCore => false;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.IATALoadPortKRList))]
		[ResourceStringData("526FBFAB-1BCF-4F8D-AFE0-B13D3153CC9E", Caption = "IATA")]
		public override ZString JE_IATALoadPort { get => base.JE_IATALoadPort; set => base.JE_IATALoadPort = value; }
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CarrierCodeCollection))]
		[ResourceStringData("DE04D0BF-DFE0-4764-B4A9-8AB926A90396", Caption = "Carrier KRC", ShortCaption = "KRC")]
		public override ZString JE_CarrierCode { get => base.JE_CarrierCode; set => base.JE_CarrierCode = value; }
		[ResourceStringData("878601DC-1174-4DFC-984B-9AF794AE6995", Caption = "Departure", ShortCaption = "Dep.")]
		[ResourceStringData("F16E84F8-9B56-44D8-A770-58D5F92BBF93", Caption = "Start Date", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		public override ZDateTime JE_ExportDate { get => base.JE_ExportDate; set => base.JE_ExportDate = value; }
		[ResourceStringData("D7A08850-6E9A-4B1D-9A83-BCAD8D8A36AD", Caption = "Arrival Date", MultipleKey = KRJobMessageTypeList.Codes.PersonalItems)]
		[ResourceStringData("47D91432-72CA-4F39-AF24-60F779B47510", Caption = "Arrival Date", ShortCaption = "Arr.", MediumCaption = "Arrival", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public override ZDateTime JE_DateOfArrival { get => base.JE_DateOfArrival; set => base.JE_DateOfArrival = value; }
		[ResourceStringData("233C7100-2AE8-4C5D-BED1-072F82D0F3A0", Caption = "Scheduled Sailing Days")]
		public override ZInt JE_VoyageDuration { get => base.JE_VoyageDuration; set => base.JE_VoyageDuration = value; }

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.MRNTypeList))]
		[ResourceStringData("BB2CBE34-660E-4627-98AC-208C23500AAA", Caption = "MRN Type")]
		public override ZString JE_MRNType
		{
			get => base.JE_MRNType;
			set
			{
				var oldValue = base.JE_MRNType;
				base.JE_MRNType = value;
				if (oldValue != JE_MRNType)
				{
					RegenerateMRNNumber(value);
				}
			}
		}
		void RegenerateMRNNumber(string mrnType)
		{
			switch (mrnType)
			{
				case MRNTypeList.Codes.NewVessel:
					MRNJ3_ReferenceNumber = (EarliestCustomsEntryIssueDate.IsValid ? EarliestCustomsEntryIssueDate : ZDateTime.Today).ToString("yy") + "ZZZZZZZZZ";
					break;
				case MRNTypeList.Codes.ChangeOfQualification:
					MRNJ3_ReferenceNumber = "3" + VesselRadioCallSign;
					break;
				case MRNTypeList.Codes.ScheduledToArrive:
					MRNJ3_ReferenceNumber = "4" + VesselRadioCallSign;
					break;
			}
		}
		protected override IReadOnlyList<string> MultipleKeysToUseCore => new string[] { JE_MessageType };
		IInvoicesProviderValueChangedAnnouncer IInvoicesProviderValueChangedAnnouncerProvider.GetValueChangedAnnouncer() => new DeclarationValueChangedAnnouncer(this);

		public override ZBool BondedWarehouseEditable => IsLocalExport || base.BondedWarehouseEditable;

		public override ZString JE_PaidBy
		{
			get => base.JE_PaidBy;
			set
			{
				base.JE_PaidBy = value;
				if (IsImport)
				{
					SetDefaultValueToDutyPayer();
				}
			}
		}

		void SetDefaultValueToDutyPayer()
		{
			switch (JE_PaidBy)
			{
				case PaidByCodeList.Codes.CLI:
					JE_OH_DutyPayer = JE_OH_Importer;
					break;
				case PaidByCodeList.Codes.OTH:
					if (AreImporterDutyPayerTheSame)
					{
						JE_OH_DutyPayer = ZGuid.Empty;
					}
					break;
			}
		}

		[MaxLength(Schema.ImporterTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ImporterTypeList))]
		[ResourceStringData("FDAB865D-0074-41F0-A3A1-FB07BAD76489", Caption = "Importer Type")]
		public ZString ImporterType
		{
			get
			{
				var result = ImporterTypeCodeList.ConvertPaidByCode(JE_PaidBy);
				return string.IsNullOrEmpty(result) ? result : result.Substring(0, Schema.ImporterTypeMaxLength);
			}
			set
			{
				CheckMaximumLength(ImporterTypeInfo, value);
				JE_PaidBy = PaidByCodeList.ConvertImporterTypeCode(value);
				ImporterTypeInfo.RefreshBinding();
				if (IsImport && !IsValidationSuspended)
				{
					var validation = (IMPJobDeclarationValidation)Validation;
					validation.ValidateImporterType();
				}
			}
		}
		public ZPropertyInfo ImporterTypeInfo => GetZPropertyInfo(nameof(ImporterType));
		[ReadOnlyMember(nameof(IsDutyPayerReadOnly))]
		[ResourceStringData("27BCD737-C805-4A58-A203-144572CB4A5C", Caption = "Payer")]
		public override ZGuid JE_OH_DutyPayer
		{
			get => base.JE_OH_DutyPayer;
			set
			{
				var oldValue = base.JE_OH_DutyPayer;
				base.JE_OH_DutyPayer = value;
				if (oldValue != value)
				{
					payerAddress = null;
				}
			}
		}

		bool IsDutyPayerReadOnly => IsImport && JE_PaidBy == PaidByCodeList.Codes.CLI;
		public bool AreImporterDutyPayerTheSame
		{
			get
			{
				var importerBusinessRegNo = Importer?.GetRegistrationNumber(IdentificationType.BusinessRegNo) ?? ZString.Empty;
				var payerBusinessRegNo = DutyPayer?.GetRegistrationNumber(IdentificationType.BusinessRegNo) ?? ZString.Empty;
				return (JE_OH_Importer.IsValid && JE_OH_Importer == JE_OH_DutyPayer) ||
							(!importerBusinessRegNo.IsEmpty && importerBusinessRegNo == payerBusinessRegNo);
			}
		}
		#endregion

		public ZString GetLocalExportMessageType()
		{
			var result = ZString.Empty;
			if (IsLocalExport && !JE_MessageSubType.IsEmpty)
			{
				if (LocalExportTransactionNatureCodeList.Is5DP(JE_MessageSubType))
				{
					result = ElectronicDocumentTypeList.Codes._5DP;
				}
				else if (LocalExportTransactionNatureCodeList.Is5DQ(JE_MessageSubType))
				{
					result = ElectronicDocumentTypeList.Codes._5DQ;
				}
			}

			return result;
		}

		[ResourceStringData("763B2C58-B585-4313-A877-00CB77C65E92", Caption = "Blanket Declaration")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		public override ZString JE_IsBlanketDeclaration { get => base.JE_IsBlanketDeclaration; set => base.JE_IsBlanketDeclaration = value; }

		[ResourceStringData("43101695-242C-4EA9-8D70-300262D16E67", Caption = "Crew Count")]
		public override ZInt JE_NoOfCrew { get => base.JE_NoOfCrew; set => base.JE_NoOfCrew = value; }

		public void RemoveEntryDateIfNotRelevant()
		{
			if (!IsLoadingDateRelevant)
			{
				JE_EntryDate = ZDate.Empty;
			}
		}

		void RemoveDataIfNotRelevant()
		{
			if (!IsLocalExportToSeaVessel && !IsPersonalItemDeclaration)
			{
				JE_VoyageDuration = ZInt.Zero;
				JE_NoOfCrew = ZInt.Zero;

				Persons.RemoveAndDeleteAll();
				TransportMeans.RemoveAndDeleteAll();
			}
		}
		public bool IsLoadingDateRelevant => IsLocalExport && LocalExportTransactionNatureCodeList.Is5DQ(JE_MessageSubType) && JE_MessageSubType != LocalExportTransactionNatureCodeList.Codes._08;

		public bool HasInvoiceLinesEligibleForSimpleDrawback
		{
			get
			{
				if (!hasInvoiceLinesEligibleForSimpleDrawback.HasValue)
				{
					hasInvoiceLinesEligibleForSimpleDrawback = InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsEligibleForSimpleDrawback);
				}
				return hasInvoiceLinesEligibleForSimpleDrawback.HasValue && hasInvoiceLinesEligibleForSimpleDrawback.Value;
			}
		}
		bool? hasInvoiceLinesEligibleForSimpleDrawback;

		public void RefreshInvoiceLinesEligibleForSimpleDrawback()
		{
			hasInvoiceLinesEligibleForSimpleDrawback = null;
		}

		public ValidationModes ValidationMode { get; set; }

		public bool IsValidationModeSetForDetailedFTA => ValidationMode.HasFlag(ValidationModes.DetailedFTA);
		public bool IsValidationModeSetFor934 => ValidationMode.HasFlag(ValidationModes.ValuationDeclaration);
		public bool IsValidationModeSetFor5FN => ValidationMode.HasFlag(ValidationModes.TaxExemptionSpecificDutyRate);
		public bool IsValidationModeSetForD72 => ValidationMode.HasFlag(ValidationModes.ExtendReExport);

		public void SetValidationModesBasedOnData()
		{
			switch (JE_MessageType)
			{
				case KRJobMessageTypeList.Codes.Export:
					ValidationMode = ValidationModes.Export;
					break;
				case KRJobMessageTypeList.Codes.LocalExport:
					ValidationMode = ValidationModes.LocalExport;
					break;
				case KRJobMessageTypeList.Codes.Import:
					ValidationMode = ValidationModes.Import;
					break;
				case KRJobMessageTypeList.Codes.PersonalItems:
					ValidationMode = ValidationModes.PersonalItemDec;
					break;
				case KRJobMessageTypeList.Codes.Carnet:
					ValidationMode = ValidationModes.CarnetCertificate;
					break;
			}
		}

		public void SetValidationModeOnElectronicMessaging(string electronicMessageType)
		{
			switch (electronicMessageType)
			{
				case ElectronicDocumentTypeList.Codes._5SC:
				case ElectronicDocumentTypeList.Codes._105:
					ValidationMode |= ValidationModes.FTA;
					break;
				case ElectronicDocumentTypeList.Codes._DHR:
				case ElectronicDocumentTypeList.Codes._DHS:
					ValidationMode |= ValidationModes.DetailedFTA;
					break;
				case ElectronicDocumentTypeList.Codes._934:
					ValidationMode |= ValidationModes.ValuationDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._D72:
					ValidationMode |= ValidationModes.ExtendReExport;
					break;
				case ElectronicDocumentTypeList.Codes._5UA:
					ValidationMode |= ValidationModes.PenaltyExemption;
					break;
				case ElectronicDocumentTypeList.Codes._5UL:
					ValidationMode |= ValidationModes.RefundRequest;
					break;
				case ElectronicDocumentTypeList.Codes._5TM:
					ValidationMode |= ValidationModes.GoldVATDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5SM:
					ValidationMode |= ValidationModes.ValuationTemplate;
					break;
				case ElectronicDocumentTypeList.Codes._5SG:
					ValidationMode |= ValidationModes.ExtendFinalPriceDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5SI:
					ValidationMode |= ValidationModes.MailDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5FN:
					ValidationMode |= ValidationModes.TaxExemptionSpecificDutyRate;
					break;
				case ElectronicDocumentTypeList.Codes._5BD:
					ValidationMode |= ValidationModes.GoodsRemovalBeforeRelease;
					break;
				case ElectronicDocumentTypeList.Codes._5BA:
				case ElectronicDocumentTypeList.Codes._5BB:
					ValidationMode |= ValidationModes.AgreedRateForAllLines;
					break;
				case ElectronicDocumentTypeList.Codes._5TE:
					ValidationMode |= ValidationModes.SynchronizationRequest;
					break;
			}
		}

		public void RemoveValidationModeOnElectronicMessaging(string electronicMessageType)
		{
			switch (electronicMessageType)
			{
				case ElectronicDocumentTypeList.Codes._5SC:
				case ElectronicDocumentTypeList.Codes._105:
					ValidationMode &= ~ValidationModes.FTA;
					break;
				case ElectronicDocumentTypeList.Codes._DHR:
				case ElectronicDocumentTypeList.Codes._DHS:
					ValidationMode &= ~ValidationModes.DetailedFTA;
					break;
				case ElectronicDocumentTypeList.Codes._934:
					ValidationMode &= ~ValidationModes.ValuationDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._D72:
					ValidationMode &= ~ValidationModes.ExtendReExport;
					break;
				case ElectronicDocumentTypeList.Codes._5UA:
					ValidationMode &= ~ValidationModes.PenaltyExemption;
					break;
				case ElectronicDocumentTypeList.Codes._5UL:
					ValidationMode &= ~ValidationModes.RefundRequest;
					break;
				case ElectronicDocumentTypeList.Codes._5TM:
					ValidationMode &= ~ValidationModes.GoldVATDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5SM:
					ValidationMode &= ~ValidationModes.ValuationTemplate;
					break;
				case ElectronicDocumentTypeList.Codes._5SG:
					ValidationMode &= ~ValidationModes.ExtendFinalPriceDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5SI:
					ValidationMode &= ~ValidationModes.MailDeclaration;
					break;
				case ElectronicDocumentTypeList.Codes._5FN:
					ValidationMode &= ~ValidationModes.TaxExemptionSpecificDutyRate;
					break;
				case ElectronicDocumentTypeList.Codes._5BD:
					ValidationMode &= ~ValidationModes.GoodsRemovalBeforeRelease;
					break;
				case ElectronicDocumentTypeList.Codes._5BA:
				case ElectronicDocumentTypeList.Codes._5BB:
					ValidationMode &= ~ValidationModes.AgreedRateForAllLines;
					break;
				case ElectronicDocumentTypeList.Codes._5TE:
					ValidationMode &= ~ValidationModes.SynchronizationRequest;
					break;
			}
		}

		public bool IsRefundRequestValidationOn => ValidationMode.HasFlag(ValidationModes.RefundRequest);
		public bool IsTaxExemptionSpecificDutyRateValidationOn => ValidationMode.HasFlag(ValidationModes.TaxExemptionSpecificDutyRate);
		public bool IsMailDeclarationValidationOn => ValidationMode.HasFlag(ValidationModes.MailDeclaration);

		void RemovePortOfLoading()
		{
			JE_RL_NKPortOfLoading = ZString.Empty;
			JE_RL_NKOrigin = ZString.Empty;
		}

		void RemovePortOfArrival()
		{
			JE_RL_NKPortOfArrival = ZString.Empty;
			JE_RL_NKFinalDestination = ZString.Empty;
		}

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new BaseCusContainerCollection<CusContainer>(this, base.Factory);
		}

		JobDecRefs JobDecRefs257Data
		{
			get
			{
				if (jobDecRefs257Data?.IsDeleted ?? true)
				{
					jobDecRefs257Data = DeclarationRefs.Where(item => item.J3_ReferenceType == AdditionalInformationStatementCodes_929._257)?.FirstOrDefault();
				}
				return jobDecRefs257Data;
			}
		}
		JobDecRefs jobDecRefs257Data;

		[MaxLength(Schema.CustomsBrokerCommentCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsBrokerCommentCode1List))]
		[ResourceStringData("6830f1ea-854b-4950-bf32-f6282011d75a", Caption = "Customs Broker Comment")]
		public ZString CustomsBrokerCommentCode1
		{
			get => JobDecRefs257Data?.J3_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (JobDecRefs257Data == null)
				{
					var declarationRef = DeclarationRefs.AddNew();
					declarationRef.J3_ReferenceType = AdditionalInformationStatementCodes_929._257;
				}

				CheckMaximumLength(CustomsBrokerCommentCode1Info, value);
				JobDecRefs257Data.J3_ReferenceNumber = value;
				CustomsBrokerCommentCode1Info.RefreshBinding();
				if (IsImport && !IsValidationSuspended)
				{
					var validation = (IMPJobDeclarationValidation)Validation;
					validation.ValidateCustomsBrokerCommentCode1();
				}
			}
		}
		public ZPropertyInfo CustomsBrokerCommentCode1Info => GetZPropertyInfo(nameof(CustomsBrokerCommentCode1));

		JobDecRefs JobDecRefs258Data
		{
			get
			{
				if (jobDecRefs258Data?.IsDeleted ?? true)
				{
					jobDecRefs258Data = DeclarationRefs.Where(item => item.J3_ReferenceType == AdditionalInformationStatementCodes_929._258)?.FirstOrDefault();
				}
				return jobDecRefs258Data;
			}
		}
		JobDecRefs jobDecRefs258Data;

		[MaxLength(Schema.CustomsBrokerCommentCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsBrokerCommentCode2List))]
		public ZString CustomsBrokerCommentCode2
		{
			get => JobDecRefs258Data?.J3_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (JobDecRefs258Data == null)
				{
					var declarationRef = DeclarationRefs.AddNew();
					declarationRef.J3_ReferenceType = AdditionalInformationStatementCodes_929._258;
				}

				CheckMaximumLength(CustomsBrokerCommentCode2Info, value);
				JobDecRefs258Data.J3_ReferenceNumber = value;
				CustomsBrokerCommentCode2Info.RefreshBinding();
				if (IsImport && !IsValidationSuspended)
				{
					var validation = (IMPJobDeclarationValidation)Validation;
					validation.ValidateCustomsBrokerCommentCode2();
				}
			}
		}
		public ZPropertyInfo CustomsBrokerCommentCode2Info => GetZPropertyInfo(nameof(CustomsBrokerCommentCode2));

		JobDecRefs JobDecRefs259Data
		{
			get
			{
				if (jobDecRefs259Data?.IsDeleted ?? true)
				{
					jobDecRefs259Data = DeclarationRefs.Where(item => item.J3_ReferenceType == AdditionalInformationStatementCodes_929._259)?.FirstOrDefault();
				}
				return jobDecRefs259Data;
			}
		}
		JobDecRefs jobDecRefs259Data;

		[MaxLength(Schema.CustomsBrokerCommentCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsBrokerCommentCode3List))]
		public ZString CustomsBrokerCommentCode3
		{
			get => JobDecRefs259Data?.J3_ReferenceNumber ?? ZString.Empty;
			set
			{
				if (JobDecRefs259Data == null)
				{
					var declarationRef = DeclarationRefs.AddNew();
					declarationRef.J3_ReferenceType = AdditionalInformationStatementCodes_929._259;
				}

				CheckMaximumLength(CustomsBrokerCommentCode2Info, value);
				JobDecRefs259Data.J3_ReferenceNumber = value;
				CustomsBrokerCommentCode3Info.RefreshBinding();
				if (IsImport && !IsValidationSuspended)
				{
					var validation = (IMPJobDeclarationValidation)Validation;
					validation.ValidateCustomsBrokerCommentCode3();
				}
			}
		}
		public ZPropertyInfo CustomsBrokerCommentCode3Info => GetZPropertyInfo(nameof(CustomsBrokerCommentCode3));

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ImportDealingTypeCodeList))]
		[ResourceStringData("8599C730-80AD-4735-801D-542FF6706100", Caption = "Transaction Type")]
		public override ZString JE_TradeType
		{
			get => base.JE_TradeType;
			set
			{
				base.JE_TradeType = value;
				Invoices?.MarkAsNeedingValidation();
				MarkAsNeedingValidation();
			}
		}
		JobComInvoiceHeader D87Invoice => IsD87 ? Invoices[0] : null;

		[BusinessObjectTestExclude]
		[ResourceStringData("15570DF9-4672-4F55-BA7C-F60CD169DA8D", Caption = "Total Amount")]
		[DecimalPlaces(Constants.DecimalPlacesConstants.AmountInKRW)]
		public ZDecimal D87InvoiceAmount
		{
			get
			{
				return D87Invoice?.JZ_InvoiceAmount ?? ZDecimal.Zero;
			}
			set
			{
				if (D87Invoice != null)
				{
					D87Invoice.JZ_InvoiceAmount = value;
					InvoiceLines[0].JI_LinePrice = value;
					D87InvoiceAmountInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo D87InvoiceAmountInfo => GetZPropertyInfo(nameof(D87InvoiceAmount));

		[MaxLength(Schema.D87InvoiceCurrencyMaxLength)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CurrencyList))]
		public ZString D87InvoiceCurrency
		{
			get
			{
				return D87Invoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty;
			}
			set
			{
				if (D87Invoice != null)
				{
					CheckMaximumLength(D87InvoiceCurrencyInfo, value);
					D87Invoice.JZ_RX_NKInvoice_Currency = value;
					D87InvoiceCurrencyInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo D87InvoiceCurrencyInfo => GetZPropertyInfo(nameof(D87InvoiceCurrency));

		[ReadOnlyMember(nameof(IsD87HBSplitDecIndReadOnly))]
		[MaxLength(Schema.D87HBSplitDecIndMaxLength)]
		[ResourceStringData("BBE34FEC-976C-476B-8AE0-D0F667633DE7", Caption = "House Bill Split")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.YesNoCodeList))]
		[BusinessObjectTestExclude]
		public ZString D87HBSplitDecInd
		{
			get
			{
				var result = ZString.Empty;
				if (IsD87 && Bills[0] != null)
				{
					result = Bills[0].CU_HBSplitDecInd;
				}
				return result;
			}
			set
			{
				if (IsD87 && !JE_HouseBill.IsEmpty)
				{
					Bills[0].CU_HBSplitDecInd = value;
					D87HBSplitDecIndInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo D87HBSplitDecIndInfo => GetZPropertyInfo(nameof(D87HBSplitDecInd));
		internal bool IsD87HBSplitDecIndReadOnly => JE_HouseBill.IsEmpty && Bills.Count == 0;

		public ZBool IsPersonalItemDeclaration => JE_MessageType == KRJobMessageTypeList.Codes.PersonalItems;
		public ZBool IsD87 => JE_MessageType == KRJobMessageTypeList.Codes.Carnet;
		public ZBool Is5SM => JE_MessageType == KRJobMessageTypeList.Codes.ValuationDeclaration;
		public ZBool IsSeparateDeclaration => IsPersonalItemDeclaration || IsD87 || Is5SM;

		public ZBool IsInvoiceLineSequenceNumberUsed => IsExport || IsImport;

		public ZString RepresentativeProductName => Notes.GetAllNotes().Cast<StmNote>().FirstOrDefault(x => x.ST_Description == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description)?.ST_NoteText ?? ZString.Empty;
		public ZPropertyInfo RepresentativeProductNameInfo => GetZPropertyInfo(nameof(RepresentativeProductName));

		public ZBool IsImporterInformationRequired => !ImportDeclarationTypeCodeList.IsSimpleDeclarationType(JE_MessageSubType) || JE_TradeType == ImportDealingTypeCodeList.Codes._15;

		OrgHeaderWrapper dutyPayerWrapper;
		public OrgHeaderWrapper DutyPayerWrapper => dutyPayerWrapper ??= OrgHeaderWrapper.New(DutyPayer);
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BankTypeList))]
		public ZString BankCode => DutyPayerWrapper.ZO_BankCode;
		public ZString BankAccountNo => DutyPayerWrapper.ZO_BankAccNo;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LateDecPenaltyDateCodeList))]
		[ResourceStringData("4293C14E-B5D8-4C3B-B3E4-1D5D275C59DC", Caption = "Late Declaration Date Type")]
		public override ZString JE_LateDecPenaltyDateCode { get => base.JE_LateDecPenaltyDateCode; set => base.JE_LateDecPenaltyDateCode = value; }

		[ResourceStringData("0EADCCC4-824C-4470-B262-633396B11D2C", Caption = "Missed Declaration Rate")]
		public override ZShort JE_MissedDecPenaltyRate { get => base.JE_MissedDecPenaltyRate; set => base.JE_MissedDecPenaltyRate = value; }

#if DEBUG
		#region FillWithValidTestData

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper() => new JobDeclarationTestDataHelper(this);

		public class JobDeclarationTestDataHelper : JobDeclarationBusinessObjectTestDataHelper
		{
			public JobDeclarationTestDataHelper(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
			{
				if (collectionProperty.Name != nameof(Persons))
				{
					base.FillDependentCollectionWithData(collection, kind, collectionProperty, propertyPath);
				}
			}
		}

		#endregion
#endif
	}

	[Flags]
	public enum ValidationModes
	{
		None = 0,
		Export = 1,
		LocalExport = 2,
		Import = 4,
		FTA = 8,
		DetailedFTA = 16,
		ValuationDeclaration = 32,
		CarnetCertificate = 64,
		ExtendReExport = 128,
		PenaltyExemption = 256,
		RefundRequest = 512,
		GoldVATDeclaration = 1024,
		ValuationTemplate = 2048,
		ExtendFinalPriceDeclaration = 4096,
		MailDeclaration = 8192,
		TaxExemptionSpecificDutyRate = 16384,
		GoodsRemovalBeforeRelease = 32768,
		AgreedRateForAllLines = 65536,
		PersonalItemDec = 131072,
		SynchronizationRequest = 262144
	}
}
