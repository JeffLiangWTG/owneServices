using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Manifest.Business
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill
		: ASYCUDA.Business.AsycudaBill,
		Integration.Customs.ASYCUDA.ILManifest.IAsycudaBill,
		ICusSupportingInfoTypeSupporter,
		IAdditionalBusinessObjectFetchStrategyProvider
	{
		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string ABL_Condition = "ABL_Condition";
			public const int ABL_SequenceNumberMaxValue = 999;
			public const int ABL_ManifestQtyMaxValue = 99999999;
		}

		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		#region AdditionalInfos

		[ChildEditable(true)]
		public IAsycudaAdditionalInfoCollection AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		IAsycudaAdditionalInfoCollection fAdditionalInfos;

		IAsycudaAdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAsycudaAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual IAsycudaAdditionalInfoCollection CreateNewAsycudaAdditionalInfoCollection()
		{
			return new AsycudaAdditionalInfoCollection(this);
		}

		#endregion

		#region TransportDocuments
		[ChildEditable]
		public IAsycudaTransportDocumentInfoCollection TransportDocuments => fTransportDocuments ??= GetTransportDocuments();
		IAsycudaTransportDocumentInfoCollection fTransportDocuments;

		IAsycudaTransportDocumentInfoCollection GetTransportDocuments()
		{
			var result = CreateNewAsycudaTransportDocumentsCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual IAsycudaTransportDocumentInfoCollection CreateNewAsycudaTransportDocumentsCollection()
		{
			return new AsycudaTransportDocumentInfoCollection(this);
		}

		#endregion

		public override ZString[] ConsigneeRegNoTypes()
		{
			return Header.IsImport ? new ZString[] { OrgCusCode.CodeTypes.VATCode } : base.ConsigneeRegNoTypes();
		}

		public override ZString[] ShipperRegNoTypes()
		{
			return Header.IsExport ? new ZString[] { OrgCusCode.CodeTypes.VATCode } : base.ShipperRegNoTypes();
		}

		public ResourceStringData ConsigneeRegoNoCaption => Header.IsImport ? Res.GetData("8647AEFF-C090-4E74-848E-5F7F91405F55", "VAT No.")
													: Res.GetData("C96BFF94-87D9-4237-B018-ABEAD0F2AEF7", "Reg.No");

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BillKindList))]
		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set => base.ABL_BolType = value;
		}

		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set
			{
				var oldValue = ABL_BillNumber;
				base.ABL_BillNumber = value;

				if (!IsCopying
					&& oldValue != ABL_BillNumber
					&& ABL_BolType == AsycudaBillKindList.Codes.HWB
					&& !Header.IsRoad)
				{
					TransportDocuments.EnsureTransportDocumentType(TransportDocsTypeList.Codes._705, ABL_BillNumber);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaBill.Destination", Caption = "Destination")]
		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set => base.ABL_RL_NKFinalDestination = value;
		}

		[Mandatory]
		public override ZShort ABL_SequenceNumber
		{
			get => base.ABL_SequenceNumber;
			set
			{
				SetPropertyValue(ABL_SequenceNumberInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateABL_SequenceNumber();
				}
			}
		}

		[Mandatory]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaBill.ABL_RL_NKPortOfDischarge", Caption = "Discharge Port")]
		public override ZString ABL_RL_NKPortOfDischarge
		{
			get => base.ABL_RL_NKPortOfDischarge;
			set => base.ABL_RL_NKPortOfDischarge = value;
		}

		[ResourceStringData("IL.AsycudaBill.ABL_Volume", Caption = "Volume")]
		public override ZDecimal ABL_Volume
		{
			get => base.ABL_Volume;
			set => base.ABL_Volume = value;
		}

		public bool ABL_GrossWeightUQ_ReadOnly => true;

		[ReadOnlyMember(nameof(ABL_GrossWeightUQ_ReadOnly))]
		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set => base.ABL_GrossWeightUQ = value;
		}

		public bool ABL_VolumeUQ_ReadOnly => true;

		[ReadOnlyMember(nameof(ABL_VolumeUQ_ReadOnly))]
		public override ZString ABL_VolumeUQ
		{
			get => base.ABL_VolumeUQ;
			set => base.ABL_VolumeUQ = value;
		}

		[ResourceStringData("IL.AsycudaBill.ABL_OA_Shipper", Caption = "Consignor")]
		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var aBL_OA_Shipper = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && aBL_OA_Shipper != ABL_OA_Shipper)
				{
					UpdateShipperRegNo();
					PopulateAdditionalInfoPartnerVat();
				}
			}
		}

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperName", Caption = "Consignor Name")]
		public override ZString ABL_ShipperName { get => base.ABL_ShipperName; set => base.ABL_ShipperName = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperStreet1", Caption = "Consignor Street 1")]
		public override ZString ABL_ShipperStreet1 { get => base.ABL_ShipperStreet1; set => base.ABL_ShipperStreet1 = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperStreet2", Caption = "Consignor Street 2")]
		public override ZString ABL_ShipperStreet2 { get => base.ABL_ShipperStreet2; set => base.ABL_ShipperStreet2 = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperCity", Caption = "Consignor City")]
		public override ZString ABL_ShipperCity { get => base.ABL_ShipperCity; set => base.ABL_ShipperCity = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperState", Caption = "Consignor State")]
		public override ZString ABL_ShipperState { get => base.ABL_ShipperState; set => base.ABL_ShipperState = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_ShipperPostcode", Caption = "Consignor Postcode")]
		public override ZString ABL_ShipperPostcode { get => base.ABL_ShipperPostcode; set => base.ABL_ShipperPostcode = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_RN_NKShipperCountry", Caption = "Consignor Country/Region")]
		public override ZString ABL_RN_NKShipperCountry { get => base.ABL_RN_NKShipperCountry; set => base.ABL_RN_NKShipperCountry = value; }

		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee)
				{
					PopulateAdditionalInfoPartnerVat();
				}
			}
		}

		[ResourceStringData("IL.AsycudaBill.ABL_ManifestQty", Caption = "Quantity")]
		public override ZInt ABL_ManifestQty { get => base.ABL_ManifestQty; set => base.ABL_ManifestQty = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_GrossWeight", Caption = "Weight")]
		public override ZDecimal ABL_GrossWeight { get => base.ABL_GrossWeight; set => base.ABL_GrossWeight = value; }

		[ResourceStringData("IL.AsycudaBill.ABL_CustomsValue", Caption = "Goods Value")]
		public override ZDecimal ABL_CustomsValue { get => base.ABL_CustomsValue; set => base.ABL_CustomsValue = value; }

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("IL.AsycudaBill.ABL_Condition", Caption = "Condition")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ConditionList))]
		public ZString ABL_Condition
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.ABL_Condition); }
			set
			{
				CheckMaximumLength(ABL_ConditionInfo, value);
				this.SetSystemDefinedValue(Schema.ABL_Condition, value);
				if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
				{
					validationForRegularBill.ValidateABL_Condition();
				}
				ABL_ConditionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ABL_ConditionInfo => GetZPropertyInfo(Schema.ABL_Condition);

		public override void OnSaving()
		{
			base.OnSaving();

			new AsycudaBillForwarderSubDealNumberManager(this).AssignSubDealNumberToTransportDocument();
		}

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		public new IAsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (IAsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

		public ZString ParentDealNumber => TransportDocuments.FirstOrDefault(x => x.CSI_Code == TransportDocsTypeList.Codes.IL2)?.CSI_ReferenceNumber ?? ZString.Empty;

		protected override ManifestBase.IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Israel;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ Common.IL.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AsycudaBaseAdditionalInfo) },
				{ Common.IL.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			PopulateFDN();
		}

		void PopulateFDN()
		{
			if (Shipment is null)
			{
				return;
			}

			var il1 = TransportDocuments.FirstOrDefault(s => s.CSI_Code == TransportDocsTypeList.Codes.IL1);
			var fdn = Shipment.Numbers.GetFirstReferenceNumberByTypeAndCountry(IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, countryCode: CountryCodes.Israel);

			if (il1 is not null && !il1.CSI_ReferenceNumber.IsEmpty)
			{
				if (fdn is null)
				{
					fdn = Shipment.Numbers.AddNew();
					fdn.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
					fdn.CE_RN_NKCountryCode = CountryCodes.Israel;
				}

				fdn.CE_EntryNum = il1.CSI_ReferenceNumber.Left(fdn.CE_EntryNumInfo.MaxLength);
			}
			else if (fdn is not null)
			{
				var fdnValue = fdn.CE_EntryNum;
				Shipment.Numbers.RemoveAndDelete(fdn);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Shipment.Logs.AddNew(AutoEvents.DeletedARecordInTheSystem, $"FDN: <{fdnValue}>");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		void PopulateAdditionalInfoPartnerVat()
		{
			RemoveAllConsigneeVatNumberRecords();
			var partnerVatNumber = GetPartnerVatNumber();

			if (!partnerVatNumber.IsEmpty)
			{
				var asycudaAdditionalInfo = GetEmptyAdditionalInfoOrAddNew();

				asycudaAdditionalInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.PartnerVatNumber;
				asycudaAdditionalInfo.CSI_Description = partnerVatNumber;
			}
		}

		ZString GetPartnerVatNumber()
		{
			var customsCodes = (string)Header.AMA_Nature switch
			{
				ShipmentTypeList.Codes.Import23 => Consignee?.Header?.CustomsCodes,
				ShipmentTypeList.Codes.Export22 => Shipper?.Header?.CustomsCodes,
				_ => null
			};

			return customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;
		}

		void RemoveAllConsigneeVatNumberRecords()
			=> AdditionalInfos.Cast<AsycudaAdditionalInfo>()
			.Where(additionalInfo => additionalInfo.CSI_Code == Constants.AsycudaAdditionalInfoCodes.PartnerVatNumber).DeleteAll();

		AsycudaAdditionalInfo GetEmptyAdditionalInfoOrAddNew()
		{
			if (AdditionalInfos.Cast<AsycudaAdditionalInfo>()
				.FirstOrDefault(additionalInfo => additionalInfo.CSI_Code.IsEmpty && additionalInfo.CSI_ReferenceNumber.IsEmpty && additionalInfo.CSI_Description.IsEmpty)
				is AsycudaAdditionalInfo emptyAsycudaAdditionalInfo)
			{
				return emptyAsycudaAdditionalInfo;
			}

			return AdditionalInfos.AddNew();
		}

		void UpdateShipperRegNo()
		{
			if (Header.IsImport)
			{
				var customsCodes = Shipper?.Header?.CustomsCodes;

				var cscCode = customsCodes?.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.SupplierCode, CountryCodes.Israel)?.OK_CustomsRegNo;
				var dubCode = customsCodes?.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.DataUniversalNumberingSystem)?.FirstOrDefault()?.OK_CustomsRegNo;
				var exporterTypeIdAdditionalInfo = AdditionalInfos.Cast<AsycudaAdditionalInfo>().FirstOrDefault(additionalInfo => additionalInfo.CSI_Code == Constants.AsycudaAdditionalInfoCodes.ExporterTypeID);

				if (cscCode.HasValue && !cscCode.Value.IsEmpty)
				{
					ABL_ShipperRegNo = cscCode.Value;

					AddOrUpdateExporterTypeID(exporterTypeIdAdditionalInfo, "1"); // TODO: when WI00720718 completed, change to "1" to the corresponding Code
				}
				else if (dubCode.HasValue && !dubCode.Value.IsEmpty)
				{
					ABL_ShipperRegNo = dubCode.Value;
					AddOrUpdateExporterTypeID(exporterTypeIdAdditionalInfo, "3"); // TODO: when WI00720718 completed, change to "3" to the corresponding Code
				}
				else
				{
					ABL_ShipperRegNo = ZString.Empty;
					if (exporterTypeIdAdditionalInfo != null)
					{
						AdditionalInfos.RemoveAndDelete(exporterTypeIdAdditionalInfo);
					}
				}
			}
		}

		void AddOrUpdateExporterTypeID(AsycudaAdditionalInfo exporterTypeIdAdditionalInfo, ZString code)
		{
			if (exporterTypeIdAdditionalInfo == null)
			{
				exporterTypeIdAdditionalInfo = AdditionalInfos.AddNew();
				exporterTypeIdAdditionalInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ExporterTypeID;
			}
			exporterTypeIdAdditionalInfo.CSI_ReferenceNumber = code;
		}
	}
}
