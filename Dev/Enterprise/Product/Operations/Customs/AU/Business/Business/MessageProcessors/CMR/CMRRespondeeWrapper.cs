using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	// This class is utter crap. Needs to be replaced with an interface.
	public class CMRRespondeeWrapper : ICMRMessageRespondee
	{
		public static CMRRespondeeWrapper GetWrapper(BusinessObject objectToWrap)
		{
			return new CMRRespondeeWrapper(objectToWrap);
		}

		public CMRRespondeeWrapper(BusinessObject objectToWrap)
		{
			if (objectToWrap == null)
			{
				throw new ArgumentNullException(nameof(objectToWrap));
			}

			WrappedObject = objectToWrap;
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (WrappedObject is ForwardingConsol)
				{
					return ((ForwardingConsol)WrappedObject).Messages;
				}
				else if (WrappedObject is ForwardingShipment)
				{
					return ((ForwardingShipment)WrappedObject).Messages;
				}
				else if (WrappedObject is ConsolidatedDeclaration)
				{
					return ((ConsolidatedDeclaration)WrappedObject).Messages;
				}
				else if (WrappedObject is JobVoyage)
				{
					return ((JobVoyage)WrappedObject).Messages;
				}
				else if (WrappedObject is VoyageDestination)
				{
					return ((VoyageDestination)WrappedObject).Messages;
				}
				else if (WrappedObject is CFSContainerWrapper)
				{
					return ((CFSShipmentWrapper)WrappedObject).Messages;
				}
				else if (WrappedObject is CFSShipmentWrapper)
				{
					return ((CFSShipmentWrapper)WrappedObject).Messages;
				}
				else if (WrappedObject is ICMRMessageRespondee)
				{
					return ((ICMRMessageRespondee)WrappedObject).Messages;
				}
				else if (WrappedObject is PackingGroup)
				{
					return ((PackingGroup)WrappedObject).Messages;
				}
				else if (WrappedObject is OrgHeader)
				{
					var orgWrapper = new OrgHeaderWrapper((OrgHeader)WrappedObject);
					return orgWrapper.Messages;
				}
				throw new ApplicationException("Unable to match object '" + WrappedObject.GetType().FullName + "'.");
			}
		}

		public ZString Details
		{
			get
			{
				if (WrappedObject is ForwardingConsol)
				{
					return new FreightConsolWrapper(((ForwardingConsol)WrappedObject)).Details;
				}
				else if (WrappedObject is ForwardingShipment)
				{
					return GetDetails((ForwardingShipment)WrappedObject);
				}
				else if (WrappedObject is ICMRMessageRespondee)
				{
					return ((ICMRMessageRespondee)WrappedObject).Details;
				}
				return ZString.Empty;
			}
		}

		static ZString GetDetails(ForwardingShipment shipment)
		{
			var resultBuilder = new ZStringBuilder();
			if (!shipment.JS_UniqueConsignRef.IsEmpty)
			{
				resultBuilder.Append("Shipment #: " + shipment.JS_UniqueConsignRef + "\r\n");
			}

			if (!shipment.JS_HouseBill.IsEmpty)
			{
				resultBuilder.Append("House Bill: " + shipment.JS_HouseBill + "\r\n");
			}

			if (!shipment.CustomsEntryNumberType.IsEmpty)
			{
				resultBuilder.Append("Entry Type: " + shipment.CustomsEntryNumberType + "\r\n");
			}

			if (!shipment.CustomsEntryNumber.IsEmpty)
			{
				resultBuilder.Append("Entry Number: " + shipment.CustomsEntryNumber + "\r\n");
			}

			return resultBuilder.ToString();
		}

		public ZString ShortDescription
		{
			get
			{
				if (WrappedObject is ForwardingConsol)
				{
					return new FreightConsolWrapper(((ForwardingConsol)WrappedObject)).ShortDescription;
				}
				else if (WrappedObject is ForwardingShipment)
				{
					var shipment = (ForwardingShipment)WrappedObject;
					return shipment.JS_UniqueConsignRef + " House Bill: " + shipment.JS_HouseBill;
				}
				else if (WrappedObject is OrgHeader)
				{
					var orgHeader = (OrgHeader)WrappedObject;
					return orgHeader.OH_Code + " " + orgHeader.OH_FullNameTruncated;
				}
				else if (WrappedObject is ICMRMessageRespondee)
				{
					return ((ICMRMessageRespondee)WrappedObject).ShortDescription;
				}

				return ZString.Empty;
			}
		}

		public readonly BusinessObject WrappedObject;
	}
}
