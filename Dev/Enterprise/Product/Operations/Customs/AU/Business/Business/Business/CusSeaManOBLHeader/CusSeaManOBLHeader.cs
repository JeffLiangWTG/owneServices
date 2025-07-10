using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[ProvideMetaDataProperty("ShouldBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusSeaManOBLHeader : BaseCusSeaManOBLHeader, ICMRMessageRespondee, Integration.Customs.AU.ICusSeaManOBLHeader
	{
		public CusSeaManOBLHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusSeaManOBLHeaderStatusCalculator(this);
		}

		public readonly CusSeaManOBLHeaderStatusCalculator Calculator;

		#region Static

		#region Load

		public static CusSeaManOBLHeader Load(BusinessObjectFactory factory, ICusSeaManOBLHeaderInfoProvider info)
		{
			ZString lloydsNumber = info.LloydsNumber;
			if (!lloydsNumber.IsEmpty)
			{
				ZString voyageNumber = info.VoyageNumber;
				if (!voyageNumber.IsEmpty)
				{
					ZString oceanBillNumber = info.OceanBillNumber;
					if (!oceanBillNumber.IsEmpty)
					{
						var vessel = factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
						if (vessel != null)
						{
							ZQuery transportHeaderFilter = new ZQuery();
							transportHeaderFilter.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, vessel.RV_Code);
							transportHeaderFilter.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, voyageNumber);
							foreach (CusSeaManTranHead tranHead in factory.Load(typeof(CusSeaManTranHead), transportHeaderFilter))
							{
								foreach (CusSeaManOBLHeader ocean in tranHead.OceanBills)
								{
									if (ocean.BO_OceanBill == oceanBillNumber)
									{
										return ocean;
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		#endregion

		#endregion

		#region Schema

		#pragma warning disable IDE0001 // Prevent simplification to base class
		public new abstract class Schema : Customs.Business.CusSeaManOBLHeader.Schema
		#pragma warning restore IDE0001 // Prevent simplification to base class
		{
			public const string CargoReportStatusCode = "CargoReportStatusCode";
			public const string CargoReportStatusDescription = "CargoReportStatusDescription";
		}

		#endregion

		#region Overrides

		#region Lookups

		public new CusSeaManOBLHeaderLookups Lookups => (CusSeaManOBLHeaderLookups)base.Lookups;

		protected override Customs.Business.CusSeaManOBLHeaderLookups GetNewLookups() => new CusSeaManOBLHeaderLookups(this);

		#endregion

		#region Validation

		protected override Customs.Business.CusSeaManOBLHeaderValidation GetNewValidation() => new CusSeaManOBLHeaderValidation(this);

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BO_HeaderCargoType = CMRImportCargoCodes.Codes.Import;
			if (Env.Registry.ConsolPaymentTerm == Enterprise.Core.Constants.PaymentType.Prepaid)
			{
				BO_PaymentMethod = CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
			else if (Env.Registry.ConsolPaymentTerm == Enterprise.Core.Constants.PaymentType.Collect)
			{
				BO_PaymentMethod = CMRMethodsOfPayment.Codes.Collect;
			}
		}

		#endregion

		#region TransportHeader

		public new CusSeaManTranHead TransportHeader => (CusSeaManTranHead)base.TransportHeader;

		#endregion

		#region Details

		protected override Customs.Business.CusSeaManOBLDetailCollection GetNewDetails() => new CusSeaManOBLDetailCollection(this);

		[ChildEditable(true)]
		public new CusSeaManOBLDetailCollection Details => (CusSeaManOBLDetailCollection)base.Details;

		#endregion

		public override bool ReadOnly
		{
			get { return base.ReadOnly || CargoReportStatus.IsWaiting; }
		}

		public override ZString BO_MessageStatus
		{
			get { return base.BO_MessageStatus; }
			set
			{
				base.BO_MessageStatus = value;
				SetReadOnlyIncludingChildren(CargoReportStatus.IsWaiting);
			}
		}

		#endregion

		#region DataRefresh on Organisations

		void UpdateConsignorPartyDetails(object sender, EventArgs e)
		{
			RefreshBinding();
		}

		void UpdateConsigneePartyDetails(object sender, EventArgs e)
		{
			RefreshBinding();
		}

		#endregion

		#region Properties

		#region ICMRMessageRespondee.Details

		ZString ICMRMessageRespondee.Details
		{
			get
			{
				ZString result = ((ICMRMessageRespondee)TransportHeader).Details;
				result += "Ocean Bill: " + BO_OceanBill + "\r\n";
				return result;
			}
		}

		#endregion

		#region ICMRMessageRespondee.ShortDescription

		ZString ICMRMessageRespondee.ShortDescription
		{
			get { return ((ICMRMessageRespondee)TransportHeader).ShortDescription + " Ocean Bill: " + BO_OceanBill; }
		}

		#endregion

		#region Consignor/Consignee

		#region Address

		public IAddress ConsignorAddress
		{
			get
			{
				IAddress result = null;
				var consignor = this.Consignor;
				if (consignor != null)
				{
					result = new OrgAddressDecider(consignor, CargoAddressType.Pickup);
				}
				return result;
			}
		}

		public IAddress ConsigneeAddress
		{
			get
			{
				IAddress result = null;
				var consignee = this.Consignee;
				if (consignee != null)
				{
					result = new OrgAddressDecider(consignee, CargoAddressType.Delivery);
				}
				return result;
			}
		}

		#endregion

		#region Organisations

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.Consignors))]
		public override ZGuid BO_OH_Consignor
		{
			get { return base.BO_OH_Consignor; }
			set
			{
				if (value != base.BO_OH_Consignor)
				{
					base.BO_OH_Consignor = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.Consignees))]
		public override ZGuid BO_OH_Consignee
		{
			get { return base.BO_OH_Consignee; }
			set
			{
				if (value != base.BO_OH_Consignee)
				{
					base.BO_OH_Consignee = value;
				}
			}
		}

		#endregion

		#region Make Read Only
		protected bool GetShouldBeReadOnly(PropertyDescriptor property)
		{
			var propertyName = property.Name;
			return (!BO_OH_Consignor.IsEmpty && (propertyName == Schema.BO_ConsignorName ||
												propertyName == Schema.BO_ConsignorAddress1 ||
												propertyName == Schema.BO_ConsignorAddress2 ||
												propertyName == Schema.BO_ConsignorCity ||
												propertyName == Schema.BO_ConsignorPostCode ||
												propertyName == Schema.BO_RN_NKConsignorCountryCode)) ||
					(!BO_OH_Consignee.IsEmpty && (propertyName == Schema.BO_ConsigneeName ||
												propertyName == Schema.BO_ConsigneeAddress1 ||
												propertyName == Schema.BO_ConsigneeAddress2 ||
												propertyName == Schema.BO_ConsigneeCity ||
												propertyName == Schema.BO_ConsigneePostCode ||
												propertyName == Schema.BO_RN_NKConsigneeCountryCode));
		}

		#endregion

		#region Consignor Overrides

		public override ZString BO_ConsignorName
		{
			get { return Consignor == null ? base.BO_ConsignorName : Consignor.OH_FullNameTruncated; }
		}

		public override ZString BO_ConsignorAddress1
		{
			get
			{
				IAddress cachedConsignorAddress = ConsignorAddress;
				return cachedConsignorAddress == null ? base.BO_ConsignorAddress1 : cachedConsignorAddress.Address1;
			}
		}

		public override ZString BO_ConsignorAddress2
		{
			get
			{
				IAddress cachedConsignorAddress = ConsignorAddress;
				return cachedConsignorAddress == null ? base.BO_ConsignorAddress2 : cachedConsignorAddress.Address2;
			}
		}

		public override ZString BO_ConsignorPostCode
		{
			get
			{
				IAddress cachedConsignorAddress = ConsignorAddress;
				return cachedConsignorAddress == null ? base.BO_ConsignorPostCode : cachedConsignorAddress.PostCode;
			}
		}

		public override ZString BO_ConsignorCity
		{
			get
			{
				IAddress cachedConsignorAddress = ConsignorAddress;
				return cachedConsignorAddress == null ? base.BO_ConsignorCity : cachedConsignorAddress.City;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.ConsignorCountryCodes))]
		public override ZString BO_RN_NKConsignorCountryCode
		{
			get { return (Consignor == null || Consignor.ClosestPort == null || Consignor.ClosestPort.Country == null) ? base.BO_RN_NKConsignorCountryCode : Consignor.ClosestPort.RL_RN_NKCountryCode; }
		}

		#endregion

		#region Consignee Overrides

		public override ZString BO_ConsigneeName
		{
			get { return Consignee == null ? base.BO_ConsigneeName : Consignee.OH_FullNameTruncated; }
		}

		public override ZString BO_ConsigneeAddress1
		{
			get
			{
				IAddress cachedConsigneeAddress = ConsigneeAddress;
				return cachedConsigneeAddress == null ? base.BO_ConsigneeAddress1 : cachedConsigneeAddress.Address1;
			}
		}

		public override ZString BO_ConsigneeAddress2
		{
			get
			{
				IAddress cachedConsigneeAddress = ConsigneeAddress;
				return cachedConsigneeAddress == null ? base.BO_ConsigneeAddress2 : cachedConsigneeAddress.Address2;
			}
		}

		public override ZString BO_ConsigneePostCode
		{
			get
			{
				IAddress cachedConsigneeAddress = ConsigneeAddress;
				return cachedConsigneeAddress == null ? base.BO_ConsigneePostCode : cachedConsigneeAddress.PostCode;
			}
		}

		public override ZString BO_ConsigneeCity
		{
			get
			{
				IAddress cachedConsigneeAddress = ConsigneeAddress;
				return cachedConsigneeAddress == null ? base.BO_ConsigneeCity : cachedConsigneeAddress.City;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.ConsigneeCountryCodes))]
		public override ZString BO_RN_NKConsigneeCountryCode
		{
			get { return (Consignee == null || Consignee.ClosestPort == null || Consignee.ClosestPort.Country == null) ? base.BO_RN_NKConsigneeCountryCode : Consignee.ClosestPort.RL_RN_NKCountryCode; }
		}

		#endregion

		#endregion

		#region CargoReportStatus

		public MessageCusStatus CargoReportStatus
		{
			get
			{
				if (fCargoReportStatus == null)
				{
					fCargoReportStatus = new MessageCusStatus(BO_MessageStatusInfo, Calculator);
				}
				return fCargoReportStatus;
			}
		}
		MessageCusStatus fCargoReportStatus;

		#endregion

		#region BO_RL_NKLoadPort

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.LoadPorts))]
		public override ZString BO_RL_NKLoadPort
		{
			get
			{
				return base.BO_RL_NKLoadPort;
			}
			set
			{
				base.BO_RL_NKLoadPort = value;

				if (BO_RL_NKOriginPort.IsEmpty)
				{
					BO_RL_NKOriginPort = value;
				}
			}
		}

		#endregion

		#region BO_RL_NKDischargePort

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.DischargePorts))]
		public override ZString BO_RL_NKDischargePort
		{
			get
			{
				return base.BO_RL_NKDischargePort;
			}
			set
			{
				base.BO_RL_NKDischargePort = value;

				if (BO_RL_NKDestinationPort.IsEmpty)
				{
					BO_RL_NKDestinationPort = value;
				}
			}
		}

		#endregion

		#region BO_RL_NKOriginPort

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.OriginPorts))]
		public override ZString BO_RL_NKOriginPort
		{
			get
			{
				return base.BO_RL_NKOriginPort;
			}
			set
			{
				base.BO_RL_NKOriginPort = value;

				BO_RN_NKGoodsCountryOfOrigin = value.SubstringSafe(0, 2);
			}
		}

		#endregion

		#region BO_RL_NKDestinationPort

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.DestinationPorts))]
		public override ZString BO_RL_NKDestinationPort { get => base.BO_RL_NKDestinationPort; set => base.BO_RL_NKDestinationPort = value; }

		#endregion

		#region BO_RN_NKGoodsCountryOfOrigin

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.GoodsCountryOfOrigins))]
		public override ZString BO_RN_NKGoodsCountryOfOrigin { get => base.BO_RN_NKGoodsCountryOfOrigin; set => base.BO_RN_NKGoodsCountryOfOrigin = value; }

		#endregion

		#region BO_PaymentMethod

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.MethodsOfPayment))]
		public override ZString BO_PaymentMethod { get => base.BO_PaymentMethod; set => base.BO_PaymentMethod = value; }

		#endregion

		#region BO_HeaderCargoType

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLHeaderLookups.CargoCodes))]
		public override ZString BO_HeaderCargoType { get => base.BO_HeaderCargoType; set => base.BO_HeaderCargoType = value; }

		#endregion

		#region CanDelete

		public override bool CanDelete
		{
			get
			{
				return BO_MessageStatus.IsEmpty ||
					BO_MessageStatus == CMRBaseStatuses.Codes.WithdrawalAccepted ||
					BO_MessageStatus == CMRBaseStatuses.Codes.NotSent ||
					BO_MessageStatus == CMRBaseStatuses.Codes.OriginalRejected ||
					!(new CMRBaseStatuses().ContainsCode(BO_MessageStatus));
			}
		}

		#endregion

		#endregion
	}
}
