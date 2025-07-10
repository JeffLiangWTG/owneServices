using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ABLEntryNum = Enterprise.Customs.ASYCUDA.Business.ABLEntryNum;

namespace Enterprise.Customs.EU.H7.Business
{
	[CodeProperty(nameof(AsycudaBill.CodeProperty))]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill :
		ASYCUDA.Business.AsycudaBill,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		IDataGroupingProvider,
		ICusGoodsLocationProvider,
		Integration.Customs.ASYCUDA.EUH7.IAsycudaBill,
		IDocManagerSupport,
		ISupportingDocObject,
		IAdditionalProcedureParent,
		Integration.Customs.ICusCodeDataTypeSupporter,
		ICanBeImportOrExport
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			LockBillIfConverted();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_GrossWeightUQ = Weight.Kilograms;
			ABL_RX_NKGoodsValueCurrency = ABL_RX_NKTransportValueCurrency = ABL_RX_NKInsuranceValueCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		public void LockBillIfConverted()
		{
			if (!ReadOnly && HasBeenConvertedToStandaloneDeclaration)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public new class Schema : ManifestBase.AsycudaBill.Schema
		{
			public const string LocalReferenceNumber = nameof(AsycudaBill.LocalReferenceNumber);
			public const string MovementReferenceNumber = nameof(AsycudaBill.MovementReferenceNumber);
		}

		#region GenAddOn

		public static class GenAddOnColumnConstants
		{
			public const string ContainerNumberColumnName = "EUH7_ContainerNumber";
			public const int ContainerNumberMaxLength = 20;
		}

		#endregion

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_TransportValue", Caption = "Transport Value", ShortCaption = "Transp. Val.", MediumCaption = "Transp. Val.", FullDescription = "Transport value of the goods.")]
		[DecimalPlaces(2)]
		public override ZDecimal ABL_TransportValue
		{
			get => base.ABL_TransportValue;
			set => base.ABL_TransportValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RX_NKTransportValueCurrency", Caption = "Transport Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Transport Value.")]
		public override ZString ABL_RX_NKTransportValueCurrency
		{
			get => base.ABL_RX_NKTransportValueCurrency;
			set => base.ABL_RX_NKTransportValueCurrency = value;
		}

		[DecimalPlaces(2)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_InsuranceValue", Caption = "Insurance Value", ShortCaption = "Ins. Val.", MediumCaption = "Ins. Val.", FullDescription = "Insurance value of the goods.")]
		public override ZDecimal ABL_InsuranceValue
		{
			get => base.ABL_InsuranceValue;
			set => base.ABL_InsuranceValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RX_NKInsuranceValueCurrency", Caption = "Insurance Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Insurance Value.")]
		public override ZString ABL_RX_NKInsuranceValueCurrency
		{
			get => base.ABL_RX_NKInsuranceValueCurrency;
			set => base.ABL_RX_NKInsuranceValueCurrency = value;
		}

		[MaxLength(70)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneeName", Caption = "Consignee Name", ShortCaption = "CNE Name", MediumCaption = "Consignee Name", FullDescription = "Consignee full name and where applicable the legal form of the party.")]
		public override ZString ABL_ConsigneeName
		{
			get => base.ABL_ConsigneeName;
			set => base.ABL_ConsigneeName = value;
		}

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneeCity", Caption = "Consignee City", ShortCaption = "CNE City", MediumCaption = "Consignee City", FullDescription = "City name of the consignee party's address.")]
		public override ZString ABL_ConsigneeCity
		{
			get => base.ABL_ConsigneeCity;
			set => base.ABL_ConsigneeCity = value;
		}

		[MaxLength(70)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperName", Caption = "Shipper Name", ShortCaption = "Ship. Name", MediumCaption = "Shipper Name", FullDescription = "Shipper full name and where applicable the legal form of the party.")]
		public override ZString ABL_ShipperName
		{
			get => base.ABL_ShipperName;
			set => base.ABL_ShipperName = value;
		}

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperCity", Caption = "Shipper City", ShortCaption = "Ship. City", MediumCaption = "Shipper City", FullDescription = "City name of the seller party's address.")]
		public override ZString ABL_ShipperCity
		{
			get => base.ABL_ShipperCity;
			set => base.ABL_ShipperCity = value;
		}

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_UCRNumber", Caption = "UCR Number", ShortCaption = "UCR", MediumCaption = "UCR No.", FullDescription = "Unique Consignment Reference (UCR) number assigned to the declaration.")]
		public override ZString ABL_UCRNumber
		{
			get => base.ABL_UCRNumber;
			set => base.ABL_UCRNumber = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerRegNoType", Caption = "IOSS Number", ShortCaption = "IOSS No.")]
		public override ZString ABL_SellerRegNoType
		{
			get => base.ABL_SellerRegNoType;
			set => base.ABL_SellerRegNoType = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_BillStatus", Caption = "Customs Status", ShortCaption = "Cus. Status", MediumCaption = "Cus. Status", FullDescription = "Code identifying the customs status of the declaration.")]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set
			{
				if (base.ABL_BillStatus != value)
				{
					base.ABL_BillStatus = value;

					PublishCustomsStatusChangedEventIfLinkedToHVLV();
				}
			}
		}

		void PublishCustomsStatusChangedEventIfLinkedToHVLV()
		{
			var conversionLog = Header.Logs.MostRecentLogByEventTime(AutoEvents.Transferred, x =>
				x.Parameters.Count == 2
				&& x.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType)
				&& eventType == ShipmentTypes.HighVolumeLowValue
			);

			if (conversionLog != null && ABL_SystemCreateTimeUtc <= conversionLog.SL_EventTimeUtc)
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, CustomsStatusLogSubscriber.PublishCustomsStatusChangedEventService),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, ABL_BillStatus)
				};

				Logs.AddNew(AutoEvents.CustomsEntryStatus, ZDateTimeOffset.Now, parameters);
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.IncotermList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_Incoterm", Caption = "Incoterm", ShortCaption = "INCO", MediumCaption = "INCO", FullDescription = "Incoterm relevant to the commercial invoice.")]
		public override ZString ABL_Incoterm
		{
			get => base.ABL_Incoterm;
			set => base.ABL_Incoterm = value;
		}

		protected override bool ABL_BillStatus_ReadOnly => true;

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SequenceNumber", Caption = "Sequence Number", ShortCaption = "Seq No.", MediumCaption = "Seq No.", FullDescription = "Bill sequence number.")]
		public override ZShort ABL_SequenceNumber
		{
			get => base.ABL_SequenceNumber;
			set => base.ABL_SequenceNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_BillNumber", Caption = "Bill Number", ShortCaption = "Bill No.", MediumCaption = "Bill No.", FullDescription = "The Transport Document Number used to identify the relevant Consignment on the declaration.")]
		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set => base.ABL_BillNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RL_NKOrigin", Caption = "Origin", ShortCaption = "Origin", MediumCaption = "Origin", FullDescription = "The UNLOCO of the port from which the Consignment first departs.")]
		public override ZString ABL_RL_NKOrigin
		{
			get => base.ABL_RL_NKOrigin;
			set => base.ABL_RL_NKOrigin = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RL_NKFinalDestination", Caption = "Final Destination", ShortCaption = "Dest.", MediumCaption = "Destination", FullDescription = "The UNLOCO of the port where the Consignment is intended to go to.")]
		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set => base.ABL_RL_NKFinalDestination = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_GoodsDescription", Caption = "Goods Description (on Bill)", ShortCaption = "Desc.", MediumCaption = "Description", FullDescription = "Description of goods, as stipulated on the manifest.")]
		public override ZString ABL_GoodsDescription
		{
			get => base.ABL_GoodsDescription;
			set => base.ABL_GoodsDescription = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ManifestQty", Caption = "Quantity (on Bill)", ShortCaption = "Qty.", MediumCaption = "Quantity", FullDescription = "Number of pieces manifested.")]
		public override ZInt ABL_ManifestQty
		{
			get => base.ABL_ManifestQty;
			set => base.ABL_ManifestQty = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ManifestUQ", Caption = "Quantity Unit", ShortCaption = "UQ", MediumCaption = "Qty. UQ", FullDescription = "Measurement unit of the number of pieces manifested.")]

		public override ZString ABL_ManifestUQ
		{
			get => base.ABL_ManifestUQ;
			set => base.ABL_ManifestUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_GrossWeight", Caption = "Gross Weight", ShortCaption = "Wt.", MediumCaption = "Weight", FullDescription = "Gross Weight of the Consignment.")]

		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set => base.ABL_GrossWeight = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_GrossWeightUQ", Caption = "Weight Unit", ShortCaption = "UQ", MediumCaption = "Wt. UQ", FullDescription = "Measurement unit of the Gross Weight of the Consignment.")]
		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set => base.ABL_GrossWeightUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_Volume", Caption = "Volume (on Bill)", ShortCaption = "Vol.", MediumCaption = "Volume", FullDescription = "Manifested volume.")]
		public override ZDecimal ABL_Volume
		{
			get => base.ABL_Volume;
			set => base.ABL_Volume = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_VolumeUQ", Caption = "Volume Unit", ShortCaption = "Vol. UQ", MediumCaption = "Vol. UQ", FullDescription = "Measurement unit of the manifested volume.")]
		public override ZString ABL_VolumeUQ
		{
			get => base.ABL_VolumeUQ;
			set => base.ABL_VolumeUQ = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_MarksAndNumbers", Caption = "Marks and Numbers (on Bill)", ShortCaption = "Marks", MediumCaption = "Marks & Nums.", FullDescription = "Free form description of the marks and numbers stipulated on the manifest.")]
		public override ZString ABL_MarksAndNumbers
		{
			get => base.ABL_MarksAndNumbers;
			set => base.ABL_MarksAndNumbers = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_Remarks", Caption = "Remarks", ShortCaption = "Remarks", MediumCaption = "Remarks", FullDescription = "Free form description of the remarks stipulated on the manifest.")]
		public override ZString ABL_Remarks
		{
			get => base.ABL_Remarks;
			set => base.ABL_Remarks = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.CustomsJobNumber", Caption = "Customs Job Number", ShortCaption = "Job No.", MediumCaption = "Cus. Job No.", FullDescription = "A system-generated number to uniquely identify a customs job in CW1.")]
		public override ZString CustomsJobNumber
		{
			get => base.CustomsJobNumber;
			set => base.CustomsJobNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_GoodsValue", Caption = "Goods Value", ShortCaption = "Goods Val.", MediumCaption = "Goods Val.", FullDescription = "Intrinsic value of the goods.")]
		[DecimalPlaces(2)]
		public override ZDecimal ABL_GoodsValue
		{
			get => base.ABL_GoodsValue;
			set => base.ABL_GoodsValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RX_NKGoodsValueCurrency", Caption = "Goods Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Goods Value.")]
		public override ZString ABL_RX_NKGoodsValueCurrency
		{
			get => base.ABL_RX_NKGoodsValueCurrency;
			set => base.ABL_RX_NKGoodsValueCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipmentType", Caption = "Additional Declaration Type", ShortCaption = "Add. Decl. Type", MediumCaption = "Add. Decl. Type", FullDescription = "Code indicating the type of declaration. ")]
		public override ZString ABL_ShipmentType
		{
			get => base.ABL_ShipmentType;
			set => base.ABL_ShipmentType = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.AdditionalProcedureList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_Procedure", Caption = "Additional Procedure(s)", ShortCaption = "ACP", MediumCaption = "Add. Proc.", FullDescription = "Code identifying any relevant additional procedure(s) associated with the bill.")]
		public override ZString ABL_Procedure
		{
			get => base.ABL_Procedure;
			set => base.ABL_Procedure = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_CargoStatus", Caption = "Cargo Status", ShortCaption = "Cargo St.", MediumCaption = "Cargo St.", FullDescription = "Code indicating the fulfillment status of the shipment.")]
		public override ZString ABL_CargoStatus
		{
			get => base.ABL_CargoStatus;
			set => base.ABL_CargoStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_CarrierReference", Caption = "Carrier Reference", ShortCaption = "Carr. Ref.", MediumCaption = "Carrier Ref.", FullDescription = "Reference number assigned by the carrier to the means of transport on which the goods are directly loaded at the time of presentation at the customs office where the destination formalities are completed.")]
		public override ZString ABL_CarrierReference
		{
			get => base.ABL_CarrierReference;
			set => base.ABL_CarrierReference = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ConsigneeOrgPK", Caption = "Consignee", ShortCaption = "CNE", MediumCaption = "Consignee", FullDescription = "The party to whom the goods are shipped.")]
		public override ZGuid ConsigneeOrgPK
		{
			get => base.ConsigneeOrgPK;
			set => base.ConsigneeOrgPK = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneeStreet1", Caption = "Consignee Street 1", ShortCaption = "CNE St. 1", MediumCaption = "Consignee Street 1", FullDescription = "Name of the street of the consignee party's address and the number of the building or facility.")]
		public override ZString ABL_ConsigneeStreet1
		{
			get => base.ABL_ConsigneeStreet1;
			set => base.ABL_ConsigneeStreet1 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneeStreet2", Caption = "Consignee Street 2", ShortCaption = "CNE St. 2", MediumCaption = "Consignee Street 2", FullDescription = "Name of the street of the consignee party's address and the number of the building or facility.")]
		public override ZString ABL_ConsigneeStreet2
		{
			get => base.ABL_ConsigneeStreet2;
			set => base.ABL_ConsigneeStreet2 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneePostcode", Caption = "Consignee Postcode", ShortCaption = "CNE PC", MediumCaption = "Consignee PC", FullDescription = "Postcode of the consignee party's address.")]
		public override ZString ABL_ConsigneePostcode
		{
			get => base.ABL_ConsigneePostcode;
			set => base.ABL_ConsigneePostcode = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneeState", Caption = "Consignee State", ShortCaption = "CNE St.", MediumCaption = "Consignee State", FullDescription = "State code of the consignee party's address.")]
		public override ZString ABL_ConsigneeState
		{
			get => base.ABL_ConsigneeState;
			set => base.ABL_ConsigneeState = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RN_NKConsigneeCountry", Caption = "Consignee Country/Region", ShortCaption = "CNE Ctry/Rgn.", MediumCaption = "Consignee Ctry/Rgn.", FullDescription = "ISO 3166-1 alpha-2 country code of the consignee party's address.")]
		public override ZString ABL_RN_NKConsigneeCountry
		{
			get => base.ABL_RN_NKConsigneeCountry;
			set => base.ABL_RN_NKConsigneeCountry = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ConsigneePhone", Caption = "Consignee Phone", ShortCaption = "CNE Ph.", MediumCaption = "Consignee Phone", FullDescription = "Telephone number of of the consignee party.")]
		public override ZString ABL_ConsigneePhone
		{
			get => base.ABL_ConsigneePhone;
			set => base.ABL_ConsigneePhone = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_CustomsValue", Caption = "Customs Value", ShortCaption = "Cus. Val.", MediumCaption = "Cus. Val.", FullDescription = "Customs value of the goods.")]
		[DecimalPlaces(2)]
		public override ZDecimal ABL_CustomsValue
		{
			get => base.ABL_CustomsValue;
			set => base.ABL_CustomsValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RX_NKCustomsValueCurrency", Caption = "Customs Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Customs Value.")]
		public override ZString ABL_RX_NKCustomsValueCurrency
		{
			get => base.ABL_RX_NKCustomsValueCurrency;
			set => base.ABL_RX_NKCustomsValueCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.DiscountValue", Caption = "Discount Value", ShortCaption = "Disc. Val.", MediumCaption = "Disc. Val.", FullDescription = "Discount value of the goods.")]
		public override ZDecimal DiscountValue
		{
			get => base.DiscountValue;
			set => base.DiscountValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.DiscountValueCurrency", Caption = "Discount Value Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Discount Value.")]
		public override ZString DiscountValueCurrency
		{
			get => base.DiscountValueCurrency;
			set => base.DiscountValueCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.OtherChargesValue", Caption = "Other Charges", ShortCaption = "Oth. Ch.", MediumCaption = "Oth. Ch.", FullDescription = "Value of any other charges associated with the goods.")]
		public override ZDecimal OtherChargesValue
		{
			get => base.OtherChargesValue;
			set => base.OtherChargesValue = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.OtherChargesValueCurrency", Caption = "Other Charges Currency", ShortCaption = "Curr.", MediumCaption = "Currency", FullDescription = "Currency code associated with the Other Charges.")]
		public override ZString OtherChargesValueCurrency
		{
			get => base.OtherChargesValueCurrency;
			set => base.OtherChargesValueCurrency = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_PrepaidCollect", Caption = "Prepaid/Collect", ShortCaption = "PPD/CLT", MediumCaption = "PPD/CLT", FullDescription = "Code denoting the responsibility of the freight expenses associated with the Consignment.")]
		public override ZString ABL_PrepaidCollect
		{
			get => base.ABL_PrepaidCollect;
			set => base.ABL_PrepaidCollect = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.NotifyPartyOrgPK", Caption = "Notify Party", ShortCaption = "Notify Party", MediumCaption = "Notify Party", FullDescription = "The party to be notified at entry of the arrival of the goods, as stipulated in the master bill of lading or master air waybill.")]
		public override ZGuid NotifyPartyOrgPK
		{
			get => base.NotifyPartyOrgPK;
			set => base.NotifyPartyOrgPK = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyName", Caption = "Notify Party Name", ShortCaption = "NP Name", MediumCaption = "Notify Party Name", FullDescription = "Notify Party full name and where applicable the legal form of the party.")]
		public override ZString ABL_NotifyPartyName
		{
			get => base.ABL_NotifyPartyName;
			set => base.ABL_NotifyPartyName = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyStreet1", Caption = "Notify Party Street 1", ShortCaption = "NP St. 1", MediumCaption = "Notify Party Street 1", FullDescription = "Name of the street of the notify party's address and the number of the building or facility.")]
		public override ZString ABL_NotifyPartyStreet1
		{
			get => base.ABL_NotifyPartyStreet1;
			set => base.ABL_NotifyPartyStreet1 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyStreet2", Caption = "Notify Party Street 2", ShortCaption = "NP St. 2", MediumCaption = "Notify Party Street 2", FullDescription = "Name of the street of the notify party's address and the number of the building or facility.")]
		public override ZString ABL_NotifyPartyStreet2
		{
			get => base.ABL_NotifyPartyStreet2;
			set => base.ABL_NotifyPartyStreet2 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyCity", Caption = "Notify Party City", ShortCaption = "NP City", MediumCaption = "Notify Party City", FullDescription = "City name of the notify party's address.")]
		public override ZString ABL_NotifyPartyCity
		{
			get => base.ABL_NotifyPartyCity;
			set => base.ABL_NotifyPartyCity = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyPostcode", Caption = "Notify Party Postcode", ShortCaption = "NP PC", MediumCaption = "Notify Party PC", FullDescription = "Postcode of the notify party's address.")]
		public override ZString ABL_NotifyPartyPostcode
		{
			get => base.ABL_NotifyPartyPostcode;
			set => base.ABL_NotifyPartyPostcode = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyState", Caption = "Notify Party State", ShortCaption = "NP St.", MediumCaption = "Notify Party State", FullDescription = "State code of the notify party's address.")]
		public override ZString ABL_NotifyPartyState
		{
			get => base.ABL_NotifyPartyState;
			set => base.ABL_NotifyPartyState = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RN_NKNotifyPartyCountry", Caption = "Notify Party Country/Region", ShortCaption = "NP Ctry/Rgn.", MediumCaption = "Notify Party Ctry/Rgn.", FullDescription = "ISO 3166-1 alpha-2 country code of the notify party's address.")]
		public override ZString ABL_RN_NKNotifyPartyCountry
		{
			get => base.ABL_RN_NKNotifyPartyCountry;
			set => base.ABL_RN_NKNotifyPartyCountry = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_NotifyPartyPhone", Caption = "Notify Party Phone", ShortCaption = "NP Ph.", MediumCaption = "Notify Party Phone", FullDescription = "Telephone number of of the notify party.")]
		public override ZString ABL_NotifyPartyPhone
		{
			get => base.ABL_NotifyPartyPhone;
			set => base.ABL_NotifyPartyPhone = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.SellerOrgPK", Caption = "Seller", ShortCaption = "Seller", MediumCaption = "Seller", FullDescription = "The party selling the goods.")]
		public override ZGuid SellerOrgPK
		{
			get => base.SellerOrgPK;
			set => base.SellerOrgPK = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerName", Caption = "Seller Name", ShortCaption = "Sell. Name", MediumCaption = "Seller Name", FullDescription = "Seller full name and where applicable the legal form of the party.")]
		public override ZString ABL_SellerName
		{
			get => base.ABL_SellerName;
			set => base.ABL_SellerName = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerStreet1", Caption = "Seller Street 1", ShortCaption = "Sell. St. 1", MediumCaption = "Seller Street 1", FullDescription = "Name of the street of the seller party's address and the number of the building or facility.")]
		public override ZString ABL_SellerStreet1
		{
			get => base.ABL_SellerStreet1;
			set => base.ABL_SellerStreet1 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerStreet2", Caption = "Seller Street 2", ShortCaption = "Sell. St. 2", MediumCaption = "Seller Street 2", FullDescription = "Name of the street of the seller party's address and the number of the building or facility.")]
		public override ZString ABL_SellerStreet2
		{
			get => base.ABL_SellerStreet2;
			set => base.ABL_SellerStreet2 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerCity", Caption = "Seller City", ShortCaption = "Sell. City", MediumCaption = "Seller City", FullDescription = "City name of the seller party's address.")]
		public override ZString ABL_SellerCity
		{
			get => base.ABL_SellerCity;
			set => base.ABL_SellerCity = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerPostcode", Caption = "Seller Postcode", ShortCaption = "Sell. PC", MediumCaption = "Seller PC", FullDescription = "Postcode of the seller party's address.")]
		public override ZString ABL_SellerPostcode
		{
			get => base.ABL_SellerPostcode;
			set => base.ABL_SellerPostcode = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerState", Caption = "Seller State", ShortCaption = "Sell. St.", MediumCaption = "Seller State", FullDescription = "State code of the seller party's address.")]
		public override ZString ABL_SellerState
		{
			get => base.ABL_SellerState;
			set => base.ABL_SellerState = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RN_NKSellerCountry", Caption = "Seller Country/Region", ShortCaption = "Sell. Ctry/Rgn.", MediumCaption = "Seller Ctry/Rgn.", FullDescription = "ISO 3166-1 alpha-2 country code of the seller party's address.")]
		public override ZString ABL_RN_NKSellerCountry
		{
			get => base.ABL_RN_NKSellerCountry;
			set => base.ABL_RN_NKSellerCountry = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_SellerPhone", Caption = "Seller Phone", ShortCaption = "Sell. Ph.", MediumCaption = "Seller Phone", FullDescription = "Telephone number of of the seller party.")]
		public override ZString ABL_SellerPhone
		{
			get => base.ABL_SellerPhone;
			set => base.ABL_SellerPhone = value;
		}

		[MaxLength(17)]
		public override ZString ABL_ShipperRegNo
		{
			get => base.ABL_ShipperRegNo;
			set => base.ABL_ShipperRegNo = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ShipperOrgPK", Caption = "Shipper", ShortCaption = "Shipper", MediumCaption = "Shipper", FullDescription = "The party consigning the goods as stipulated in the transport contract by the party ordering the transport.")]
		public override ZGuid ShipperOrgPK
		{
			get => base.ShipperOrgPK;
			set => base.ShipperOrgPK = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperStreet1", Caption = "Shipper Street 1", ShortCaption = "Ship. St. 1", MediumCaption = "Shipper Street 1", FullDescription = "Name of the street of the seller party's address and the number of the building or facility.")]
		public override ZString ABL_ShipperStreet1
		{
			get => base.ABL_ShipperStreet1;
			set => base.ABL_ShipperStreet1 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperStreet2", Caption = "Shipper Street 2", ShortCaption = "Ship. St. 2", MediumCaption = "Shipper Street 2", FullDescription = "Name of the street of the seller party's address and the number of the building or facility.")]
		public override ZString ABL_ShipperStreet2
		{
			get => base.ABL_ShipperStreet2;
			set => base.ABL_ShipperStreet2 = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperPostcode", Caption = "Shipper Postcode", ShortCaption = "Ship. PC", MediumCaption = "Shipper PC", FullDescription = "Postcode of the seller party's address.")]
		public override ZString ABL_ShipperPostcode
		{
			get => base.ABL_ShipperPostcode;
			set => base.ABL_ShipperPostcode = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperState", Caption = "Shipper State", ShortCaption = "Ship. St.", MediumCaption = "Shipper State", FullDescription = "State code of the seller party's address.")]
		public override ZString ABL_ShipperState
		{
			get => base.ABL_ShipperState;
			set => base.ABL_ShipperState = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_RN_NKShipperCountry", Caption = "Shipper Country/Region", ShortCaption = "Ship. Ctry/Rgn.", MediumCaption = "Shipper Ctry/Rgn.", FullDescription = "ISO 3166-1 alpha-2 country code of the seller party's address.")]
		public override ZString ABL_RN_NKShipperCountry
		{
			get => base.ABL_RN_NKShipperCountry;
			set => base.ABL_RN_NKShipperCountry = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ShipperPhone", Caption = "Shipper Phone", ShortCaption = "Ship. Ph.", MediumCaption = "Shipper Phone", FullDescription = "Telephone number of of the seller party.")]
		public override ZString ABL_ShipperPhone
		{
			get => base.ABL_ShipperPhone;
			set => base.ABL_ShipperPhone = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_MessageStatus", Caption = "Message Status", ShortCaption = "Msg. Status", MediumCaption = "Msg. Status", FullDescription = "Code identifying the status of the last customs declaration message sent to the relevant Customs authority.")]
		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set => base.ABL_MessageStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_LocationInformation", Caption = "Location Information", ShortCaption = "Loc. Info", MediumCaption = "Location Info.", FullDescription = "Any additional location information.")]
		public override ZString ABL_LocationInformation
		{
			get => base.ABL_LocationInformation;
			set => base.ABL_LocationInformation = value;
		}

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_OA_ContainerAgent", Caption = "Agent", ShortCaption = "Agent", MediumCaption = "Agent", FullDescription = "The party acting on behalf of the shipper or consignor.")]
		public override ZGuid ABL_OA_ContainerAgent
		{
			get => base.ABL_OA_ContainerAgent;
			set => base.ABL_OA_ContainerAgent = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ContainerModeList))]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ABL_ContainerMode", Caption = "Container Mode", ShortCaption = "Cont. M.", MediumCaption = "Container M.", FullDescription = "The Container Mode.")]
		public override ZString ABL_ContainerMode { get => base.ABL_ContainerMode; set => base.ABL_ContainerMode = value; }

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.ContainerNumber", Caption = "Container", ShortCaption = "Cont. No.", MediumCaption = "Container No.", FullDescription = "The Container Number.")]
		[MaxLength(GenAddOnColumnConstants.ContainerNumberMaxLength)]
		public ZString ContainerNumber
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.ContainerNumberColumnName);
			set
			{
				var oldValue = ContainerNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(ContainerNumberInfo, value);
					this.SetSystemDefinedValue(GenAddOnColumnConstants.ContainerNumberColumnName, value);
					ContainerNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

		public override ZString[] SellerRegNoTypes() => new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration };

		public ZString CodeProperty => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}|{1}", Header.AMA_JobReference, ABL_BillNumber);

		public new IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (IAsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		public ValidationConfiguration ValidationConfiguration => validationConfiguration ??= GetNewValidationConfiguration();
		ValidationConfiguration validationConfiguration;

		protected virtual ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override ManifestBase.IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new ManifestBase.AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

		public new ManifestBase.IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (ManifestBase.IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

		public virtual string RequiredCurrencyCode => CurrencyCodes.EuropeanUnion;

		public ZDecimal SumOfGoodsValue => Factory.GetValue(ref sumOfGoodsValueCached, () =>
		{
			ZDecimal sumOfGoodsValue = 0m;
			var requiredCurrency = RefCurrency.LoadFromCurrencyCode(Factory, RequiredCurrencyCode);
			var currencyDict = new Dictionary<string, RefCurrency>();
			var currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, true);

			foreach (var goodItem in PackedItems)
			{
				if (goodItem.API_RX_NKGoodsValueCurrency != RequiredCurrencyCode)
				{
					if (!currencyDict.TryGetValue(goodItem.API_RX_NKGoodsValueCurrency, out var currency))
					{
						currency = RefCurrency.LoadFromCurrencyCode(Factory, goodItem.API_RX_NKGoodsValueCurrency);
						currencyDict.Add(goodItem.API_RX_NKGoodsValueCurrency, currency);
					}
					var money = new Money(goodItem.API_GoodsValue, currency);
					money = currencyConverter.ConvertRounded(money, requiredCurrency);

					sumOfGoodsValue += money.Amount;
				}
				else
				{
					sumOfGoodsValue += goodItem.API_GoodsValue;
				}
			}

			return sumOfGoodsValue;
		});

		CachedProperty<ZDecimal> sumOfGoodsValueCached;

		protected override ZString HumanReadableShortcutNameCore => HumanReadableName;

		#region Additional Documents

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

		#region Additional Infos

		[ChildEditable(true)]
		public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = CreateAdditionalInfoCollection();
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}
		IAdditionalInfoCollection<AdditionalInfo> additionalInfos;

		protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

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

		public virtual SupportingDocSendingObject GetSupportingDocSendingObject()
		{
			return new SupportingDocSendingObject(this);
		}

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
				{ H7CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) },
				{ H7CusSupportingInfoTypeList.Codes.AdditionalDocument, typeof(AdditionalDocument) },
				{ H7CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ H7CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region LocalReferenceNumber

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.LocalReferenceNumber", Caption = "Local Reference Number", ShortCaption = "LRN", MediumCaption = "LRN", FullDescription = "A system-generated local reference number to uniquely identify each single declaration.")]
		[MaxLength(AutoCusEntryNum.Schema.CE_EntryNumMaxLength)]
		public ZString LocalReferenceNumber
		{
			get => LRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var currentLRNEntryNumber = LRNEntryNumber;
				var oldValue = currentLRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentLRNEntryNumber == null)
					{
						currentLRNEntryNumber = LoadLRNEntryNumber(createIfMissing: true);
					}

					currentLRNEntryNumber.CE_EntryNum = value;
					LocalReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(nameof(LocalReferenceNumber));

		ABLEntryNum LRNEntryNumber
		{
			get
			{
				if (lrnEntryNumber == null || lrnEntryNumber.IsDeleted)
				{
					LoadLRNEntryNumber(createIfMissing: false);
				}
				return lrnEntryNumber;
			}
		}

		ABLEntryNum lrnEntryNumber;

		protected ABLEntryNum LoadLRNEntryNumber(bool createIfMissing)
		{
			lrnEntryNumber = LoadOrCreateLRNEntryNumber(createIfMissing);
			if (lrnEntryNumber != null)
			{
				RegisterEditableChildObject(lrnEntryNumber);
				lrnEntryNumber.CE_EntryNumInfo.ValueChanged += delegate
				{ MarkAsNeedingValidation(); };
				lrnEntryNumber.CE_IssueDateInfo.ValueChanged += delegate
				{ MarkAsNeedingValidation(); };
			}
			return lrnEntryNumber;
		}

		protected virtual ABLEntryNum LoadOrCreateLRNEntryNumber(bool createIfMissing)
		{
			return createIfMissing
				? Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, Header.AMA_RN_NKCountry)
				: Common.CusEntryNumber.Load<ABLEntryNum>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, Header.AMA_RN_NKCountry);
		}

		#endregion

		#region Movement Reference Number

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.MovementReferenceNumber", Caption = "Movement Reference Number", ShortCaption = "MRN", MediumCaption = "MRN", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		public ZString MovementReferenceNumber
		{
			get => CustomsEntryNumber;
			set => CustomsEntryNumber = value;
		}

		public ZPropertyInfo MovementReferenceNumberInfo => GetWrappedZPropertyInfo(nameof(MovementReferenceNumber), (x) => CustomsEntryNumberInfo);

		public override ZString FilterForSingleEntryType => CusEntryNumberTypes.Standard.MovementReferenceNumber;

		protected override bool CustomsEntryNumber_ReadOnly => true;

		#endregion

		#region Requested Document

		[ChildEditable(true)]
		public RequestedDocumentCollection RequestedDocuments
		{
			get
			{
				if (requestedDocuments == null)
				{
					requestedDocuments = new RequestedDocumentCollection(this);
					requestedDocuments.Load();
					requestedDocuments.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(requestedDocuments);
				}

				return requestedDocuments;
			}
		}

		RequestedDocumentCollection requestedDocuments;

		public bool IsDocumentationRequested => Factory.GetValue(ref isDocumentationRequested, GetIsDocumentationRequested);
		CachedProperty<bool> isDocumentationRequested;

		protected virtual bool GetIsDocumentationRequested()
		{
			return RequestedDocuments.Cast<RequestedDocument>().Any(doc => doc.CSI_Status == RequestedDocumentStatusList.Codes.RequestOpened);
		}

		#endregion

		#region Convert To Stand Alone Declaration

		public bool HasBeenConvertedToStandaloneDeclaration => !EntrySummaryReferenceNumber.IsEmpty && !ABL_IsActive;

		[MaxLength(50)]
		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill.StandAloneDeclarationReferenceNumber", ShortCaption = "Stand Alone Decl.", MediumCaption ="Stand Alone Decl.", Caption = "Stand Alone Declaration", FullDescription = "The Job Number of the Stand Alone Declaration linked to the H7 Bill.")]
		public ZString EntrySummaryReferenceNumber
		{
			get => ENSEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				var currentENSEntryNumber = ENSEntryNumber;
				var oldValue = currentENSEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
				if (value.IsValid && value != oldValue)
				{
					if (currentENSEntryNumber == null)
					{
						ensEntryNumber = Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, CusEntryNumberTypes.EU.EntrySummary, GetCountryCode());
						RegisterEditableChildObject(ensEntryNumber);
					}

					ensEntryNumber.CE_EntryLineReference = value;
					EntrySummaryReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		[ReadOnly(true)]
		ABLEntryNum ENSEntryNumber
		{
			get
			{
				if (ensEntryNumber == null || ensEntryNumber.IsDeleted)
				{
					ensEntryNumber = Common.CusEntryNumber.Load<ABLEntryNum>(this, CusEntryNumberTypes.EU.EntrySummary, GetCountryCode());
					if (ensEntryNumber != null)
					{
						RegisterEditableChildObject(ensEntryNumber);
					}
				}

				return ensEntryNumber;
			}
		}

		ABLEntryNum ensEntryNumber;

		protected bool EntrySummaryReferenceNumber_ReadOnly => true;

		public ZPropertyInfo EntrySummaryReferenceNumberInfo => GetZPropertyInfo(nameof(EntrySummaryReferenceNumber));

		public ZBool CanConvertToStandAloneDeclaration => !HasBeenConvertedToStandaloneDeclaration;

		public ZBool CanEditStandAloneDeclaration => HasBeenConvertedToStandaloneDeclaration;

		public JobDeclaration StandAloneDeclaration
		{
			get
			{
				if (standAloneDeclaration == null)
				{
					var referenceNumber = EntrySummaryReferenceNumber;
					if (referenceNumber.IsEmpty)
					{
						return null;
					}

					var query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, referenceNumber);
					standAloneDeclaration = Factory.LoadTop1<JobDeclaration>(query);
				}
				return standAloneDeclaration;
			}
		}
		JobDeclaration standAloneDeclaration;

		public string GetReleasedStatus() => GetReleasedStatusCore();

		protected virtual string GetReleasedStatusCore() => AISEntryStatusList.Codes.Released;

		public string GetReleasedStatusDescription() => GetReleasedStatusDescriptionCore();

		protected virtual string GetReleasedStatusDescriptionCore() => AISEntryStatusList.Descriptions.Released;

		#endregion

		#region AdditionalProcedureCode

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public virtual EU.Business.AdditionalProcedureCodeCollection AdditionalProcedureCodes
		{
			get
			{
				if (additionalProcedureCodes == null)
				{
					additionalProcedureCodes = new AdditionalProcedureCodeCollection(this);
					RegisterEditableChildObject(additionalProcedureCodes);
					additionalProcedureCodes.Load();
				}

				return additionalProcedureCodes;
			}
		}
		AdditionalProcedureCodeCollection additionalProcedureCodes;

		CodeDescriptionPairList IAdditionalProcedureParent.AdditionalProcedureCodeList => Lookups.AdditionalProcedureList;

		ZString IAdditionalProcedureParent.MainProcedure => ABL_Procedure;

		ZString IAdditionalProcedureParent.MainProcedurePrefix => (ABL_Procedure != ZString.Empty) ? ABL_Procedure.Left(4) : ZString.Empty;

		[ResourceStringData("Enterprise.Customs.EU.H7.Business.AsycudaBill|AdditionalProcedureCodesAsString", Caption = "Additional Procedure Code(s)", MediumCaption = "Add. Procedure(s)", ShortCaption = "Add. Proc.", FullDescription = "Code identifying any relevant additional procedure(s) associated with the Bill.")]
		public virtual ZString AdditionalProcedureCodesAsString => AdditionalProcedureCodes.AsString;

		public ZPropertyInfo AdditionalProcedureCodesAsStringInfo => GetZPropertyInfo(nameof(AdditionalProcedureCodesAsString));

		int IAdditionalProcedureParent.MaxNumberOfAdditionalProcedureCode => 99;

		BusinessObject IAdditionalProcedureParent.BusinessObject => this;

		public bool AdditionalProcedureContainsC07()
		{
			return ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C07 ||
				ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C07F48 ||
				ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C07F49;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes => new Dictionary<ZString, Type>()
		{
			{ CusCodeDataTypeList.Codes.AdditionalProcedureCode, typeof(AdditionalProcedureCode) },
		};

		#endregion

		#region ICanBeImportOrExport

		ZBool ICanBeImportOrExport.IsImport => IsImport;

		ZBool ICanBeImportOrExport.IsExport => IsExport;

		string ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		string ICanBeImportOrExport.TrueCountryCode => CountryCode;

		string ICanBeImportOrExport.DataGroupingCode => Header.DataGrouping;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		#endregion

		protected override void DefaultOnCountryChanged()
		{
			CalculateShipmentTypeCore(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaBillFetchStrategy(this);

		#region ICusGoodsLocationProvider

		EU.Business.CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => CusGoodsLocation;

		public CusGoodsLocation CusGoodsLocation
		{
			get
			{
				if (cusGoodsLocation == null)
				{
					cusGoodsLocation = Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					RegisterEditableChildObject(cusGoodsLocation);
				}
				return cusGoodsLocation;
			}
		}
		CusGoodsLocation cusGoodsLocation;

		[ResourceStringData("48827515-82b2-4c8b-8932-376264e44cb8", Caption = "Location of Goods", ShortCaption = "Location", MediumCaption = "Location", FullDescription = "Location where the goods may be examined. The location must be precise enough to allow Customs to carry out the physical control of the goods.")]
		public ZString GoodsLocationDescription
		{
			get
			{
				if (cusGoodsLocation == null || cusGoodsLocation.IsDeleted)
				{
					cusGoodsLocation = Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.Departure);
					if (cusGoodsLocation != null)
					{
						RegisterEditableChildObject(cusGoodsLocation);
					}
				}
				return cusGoodsLocation?.DisplayText ?? ZString.Empty;
			}
		}

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		public ZString ProviderKey => CountryCode + GoodsLocationProviderApplications.Codes.H7Declaration;

		public void ValidateGoodsLocationDescription()
		{
			if (Validation is AsycudaBillValidationForRegularBill regularBillValidation)
			{
				regularBillValidation.ValidateGoodsLocationDescription();
			}
		}

		public void ValidateAdditionalProcedureCodeAsString()
		{
			if (Validation is AsycudaBillValidationForRegularBill regularBillValidation)
			{
				regularBillValidation.ValidateAddititionalProcedureCodeAsString();
			}
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ??= new AsycudaBillDocManagerInfo(this); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore() => true;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>
			{
				AsycudaBillSchema.Constants.ABL_BillNumber,
				AsycudaBillSchema.Constants.ABL_MessageStatus,
				AsycudaBillSchema.Constants.ABL_BillStatus,
				AsycudaBillSchema.Constants.ABL_UCRNumber
			};
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (AsycudaBill)base.CloneInternal(args);

			templateCopy.DiscountValue = DiscountValue;
			templateCopy.DiscountValueCurrency = DiscountValueCurrency;
			templateCopy.OtherChargesValue = OtherChargesValue;
			templateCopy.OtherChargesValueCurrency = OtherChargesValueCurrency;

			var clonedGoodsLocation = (CusGoodsLocation)CusGoodsLocation.Clone();
			clonedGoodsLocation.CGL_ParentID = templateCopy.PK;

			CloneChildCollection(Packs, templateCopy.Packs);
			CloneChildCollection(AdditionalDocuments, templateCopy.AdditionalDocuments);
			CloneChildCollection(SupportingDocuments, templateCopy.SupportingDocuments);
			CloneChildCollection(PreviousDocuments, templateCopy.PreviousDocuments);
			CloneItemsAndPackPackedItemPivot(templateCopy.PackedItems, templateCopy.Packs);
			return templateCopy;
		}

		void CloneChildCollection(IBusinessObjectCollection collection, IBusinessObjectCollection clonedColection)
		{
			foreach (BusinessObject element in collection)
			{
				var clonedElement = element.Clone();
				clonedColection.Add(clonedElement);
			}
		}

		void CloneItemsAndPackPackedItemPivot(ManifestBase.IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> clonedItems, IAsycudaPackCollection<AsycudaPack, AsycudaBill> clonedPacks)
		{
			foreach (var item in PackedItems)
			{
				var clonedItem = (AsycudaPackedItem)item.Clone();
				var pivotCollection = item.PackagesPivot;
				foreach (ManifestBase.AsycudaPackPackedItemPivot pivot in pivotCollection)
				{
					var clonedPack = (AsycudaPack)clonedPacks.Find(new ZQuery(AsycudaPackSchema.APA_LineNo, pivot.Pack.APA_LineNo)).FirstOrDefault();
					clonedItem.ToggleLinkageWithPackage(clonedPack, true);
				}

				clonedItems.Add(clonedItem);
			}
		}

		#endregion
	}
}
