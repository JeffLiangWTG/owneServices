using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAeMHDocAddress : JobDocAddress, IShortSequenceNumberLine
	{
		public CAeMHDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Proxy Properties

		#region E2_AddressType

		[List(nameof(AddressTypeList))]
		public override ZString E2_AddressType
		{
			get { return base.E2_AddressType; }
			set
			{
				var oldValue = E2_AddressType;
				base.E2_AddressType = value;
				ReCaluclateAddressSequence(E2_AddressType, oldValue);
			}
		}

		void ReCaluclateAddressSequence(ZString newAddressType, ZString oldAddressType)
		{
			if (newAddressType != oldAddressType)
			{
				var house = Parent as CusCAeMHHouse;
				if (house != null)
				{
					if (newAddressType == DocAddressTypes.Codes.NotifyParty)
					{
						house.NotifyPartySequenceNumberGenerator.RecalculateWhenAdded(this);
					}
					else if (oldAddressType == DocAddressTypes.Codes.NotifyParty)
					{
						house.NotifyPartySequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
					}
					else if (newAddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress)
					{
						house.DeliveryAddressSequenceNumberGenerator.RecalculateWhenAdded(this);
					}
					else if (oldAddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress)
					{
						house.DeliveryAddressSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
					}
				}
			}
		}

		public CodeDescriptionPairList AddressTypeList
		{
			get { return GetAddressTypeList(Factory); }
		}

		internal static CodeDescriptionPairList GetAddressTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("eMHAddressTypeList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, DocAddressTypes.Descriptions.ConsigneeDocumentaryAddress);
				result.AddPair(DocAddressTypes.Codes.ConsignorDocumentaryAddress, DocAddressTypes.Descriptions.ConsignorDocumentaryAddress);
				result.AddPair(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, DocAddressTypes.Descriptions.ConsigneePickupDeliveryAddress);
				result.AddPair(DocAddressTypes.Codes.NotifyParty, DocAddressTypes.Descriptions.NotifyParty);
				result.AddPair(DocAddressTypes.Codes.ImportBroker, DocAddressTypes.Descriptions.ImportBroker);
				result.AddPair(DocAddressTypes.Codes.ReceivingForwarderAddress, DocAddressTypes.Descriptions.ReceivingForwarderAddress);
				result.AddPair(DocAddressTypes.Codes.Carrier, DocAddressTypes.Descriptions.Carrier);
				result.AddPair(DocAddressTypes.Codes.Warehouse, DocAddressTypes.Descriptions.Warehouse);
				result.AddPair(DocAddressTypes.Codes.PlaceOfConsolidation, DocAddressTypes.Descriptions.PlaceOfConsolidation);
				result.AddPair(DocAddressTypes.Codes.Consolidator, DocAddressTypes.Descriptions.Consolidator);
				return result;
			});
		}

		#endregion

		internal static ZString GetAddressCaption(ZString addressType)
		{
			switch (addressType)
			{
				case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
					return Res.GetString("D51ADF1E-A346-455E-9A62-CAAD37A13AB6", "Consignee");
				case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
					return Res.GetString("0A345480-BDC9-4338-8BE5-A692F5E0B6A7", "Shipper");
				case DocAddressTypes.Codes.ConsigneePickupDeliveryAddress:
					return Res.GetString("AF6ACC5F-40E1-4665-93AD-095EBBDB08A0", "Delivery Address");
				case DocAddressTypes.Codes.NotifyParty:
					return Res.GetString("3403BAC4-4DA5-4E5A-B3DC-029ACFB235ED", "Notify Party");
				case DocAddressTypes.Codes.ImportBroker:
					return Res.GetString("941E6877-919C-4FA4-85CC-254E65B2F1A1", "Broker");
				case DocAddressTypes.Codes.Warehouse:
					return Res.GetString("F3F1887A-740B-4218-9DE6-88C936740822", "Warehouse");
				case DocAddressTypes.Codes.Carrier:
					return Res.GetString("584E3526-E165-4B3D-98A2-F03A86E08B3F", "Carrier");
				case DocAddressTypes.Codes.ReceivingForwarderAddress:
					return Res.GetString("000D7103-1A2A-460A-8F6D-9A562EB98136", "Forwarder");
				case DocAddressTypes.Codes.PlaceOfConsolidation:
					return DocAddressTypes.Descriptions.PlaceOfConsolidation;
				case DocAddressTypes.Codes.Consolidator:
					return DocAddressTypes.Descriptions.Consolidator;
				default:
					return string.Empty;
			}
		}

		#endregion

		#region Implemenation

		public override void Delete()
		{
			if (!IsRowDeletedOrDetachedOrNull)
			{
				ReCalculateAddressSequenceWhenDeleted();
			}
			base.Delete();
		}

		void ReCalculateAddressSequenceWhenDeleted()
		{
			var house = Parent as CusCAeMHHouse;
			if (house != null)
			{
				if (E2_AddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress)
				{
					house.DeliveryAddressSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
				else if (E2_AddressType == DocAddressTypes.Codes.NotifyParty)
				{
					house.NotifyPartySequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				}
			}
		}

		#endregion

		#region ISequenceNumberLine

		ZGuid ISequenceNumberLine.FKToHeader
		{
			get { return base.E2_ParentID; }
		}

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return ZShort.ParseSafe(base.E2_AddressSequence.ToString(), ZShort.Zero); }
			set { base.E2_AddressSequence = ZByte.ParseSafe(value.ToString(), ZByte.Zero); }
		}

		#endregion

		#region Lookups

		public new CAeMHDocAddressLookups Lookups
		{
			get { return (CAeMHDocAddressLookups)base.Lookups; }
		}
		protected override JobDocAddressLookups GetNewLookups()
		{
			return new CAeMHDocAddressLookups(this);
		}
		#endregion
	}
}
