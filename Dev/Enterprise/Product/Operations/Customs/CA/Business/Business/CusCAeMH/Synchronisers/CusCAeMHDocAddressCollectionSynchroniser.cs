using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHDocAddressCollectionSynchroniser : GenericCollectionSynchroniser<CusCAeMHDocAddressSynchroniser>
	{
		public CusCAeMHDocAddressCollectionSynchroniser(ForwardingShipment source, CusCAeMHHouse destination)
			: base(source, destination, new CusCAeMHShipmentDocAddressCollectionView(source.DocAddresses), destination.DocAddresses)
		{ }

		protected new CusCAeMHHouse Destination
		{
			get { return (CusCAeMHHouse)base.Destination; }
		}

		new ForwardingShipment Source
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected override bool CompareBizosEqual(BusinessObject source, BusinessObject destination)
		{
			var srcDoc = (JobDocAddress)source;
			var destDoc = (CAeMHDocAddress)destination;
			var result = false;
			if (!srcDoc.E2_AddressOverride && !destDoc.E2_AddressOverride)
			{
				result = srcDoc.E2_OA_Address == destDoc.E2_OA_Address &&
					srcDoc.E2_AddressType == CusCAeMHDocAddressSynchroniser.GetDestinationAddressType(destDoc.E2_AddressType);
			}
			else if (srcDoc.E2_AddressOverride && destDoc.E2_AddressOverride)
			{
				return srcDoc.E2_AddressType == CusCAeMHDocAddressSynchroniser.GetDestinationAddressType(destDoc.E2_AddressType)
					&& srcDoc.E2_CompanyName == destDoc.E2_CompanyName && srcDoc.E2_Address1 == destDoc.E2_Address1
					&& srcDoc.E2_Address2 == destDoc.E2_Address2 && srcDoc.E2_Postcode == destDoc.E2_Postcode
					&& srcDoc.E2_City == destDoc.E2_City && srcDoc.E2_Contact == destDoc.E2_Contact
					&& srcDoc.E2_Phone == destDoc.E2_Phone && srcDoc.E2_Fax == destDoc.E2_Fax && srcDoc.E2_Email == destDoc.E2_Email;
			}
			return result;
		}

		protected override CusCAeMHDocAddressSynchroniser CreateNewSynchroniser(BusinessObject destination, BusinessObject source)
		{
			return new CusCAeMHDocAddressSynchroniser((CAeMHDocAddress)destination, (JobDocAddress)source);
		}

		protected override void DeleteBizo(BusinessObject bizo)
		{
			var address = (CAeMHDocAddress)bizo;
			if (Source.DocAddresses.Cast<JobDocAddress>().Any(x => x.E2_AddressType == address.E2_AddressType) &&
				Destination.DocAddresses.Cast<CAeMHDocAddress>().Any(x => x.E2_AddressType == address.E2_AddressType))
			{
				Destination.DocAddresses.RemoveAndDelete(bizo);
			}
		}

		#region CusCAeMHShipmentDocAddressCollectionView

		internal class CusCAeMHShipmentDocAddressCollectionView : BusinessObjectCollectionView<BusinessObject>
		{
			public CusCAeMHShipmentDocAddressCollectionView(JobDocAddressDependentCollection docAdressCollection)
				: base(docAdressCollection)
			{ }

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return ShipmentDocAddressTypes.Contains(((JobDocAddress)element).E2_AddressType);
			}

			IEnumerable<ZString> ShipmentDocAddressTypes
			{
				get
				{
					yield return DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
					yield return DocAddressTypes.Codes.ConsignorDocumentaryAddress;
					yield return DocAddressTypes.Codes.ConsigneePickupDeliveryAddress;
					yield return DocAddressTypes.Codes.NotifyParty;
				}
			}
		}

		#endregion
	}
}
