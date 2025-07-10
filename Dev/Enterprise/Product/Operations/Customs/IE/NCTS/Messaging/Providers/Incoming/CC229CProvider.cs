using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC229C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC229CProvider
	{
		Cc229CType XmlObject { get; }

		public CC229CProvider(Cc229CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString GRN => XmlObject.GuaranteeReference?.Grn ?? ZString.Empty;

		public ZDate InvalidityDate => new ZDate(XmlObject.GuaranteeReference?.InvalidityDate);

		public ZString GuarantorName => XmlObject.Guarantor?.Name ?? ZString.Empty;
		public ZString GuarantorAddress => CachedValueHelper.GetValue(ref fullAddress, () => GetFullAddress(XmlObject.Guarantor?.Address));
		CachedValue<ZString> fullAddress;
		public ZString CustomsOfficeOfGuarantee => XmlObject.GuaranteeReference?.CustomsOfficeOfGuarantee?.ReferenceNumber ?? ZString.Empty;

		static ZString GetFullAddress(AddressType16 address)
		{
			var addressParts = new List<string>();
			if (address != null)
			{
				if (!string.IsNullOrWhiteSpace(address.StreetAndNumber))
				{
					addressParts.Add(address.StreetAndNumber);
				}
				if (!string.IsNullOrWhiteSpace(address.City))
				{
					addressParts.Add(address.City);
				}
				if (!string.IsNullOrWhiteSpace(address.Postcode))
				{
					addressParts.Add(address.Postcode);
				}
				if (!string.IsNullOrEmpty(address.Country.ToString()))
				{
					addressParts.Add(address.Country.ToString().ToUpper());
				}
			}
			return addressParts.Count > 0 ? string.Join(", ", addressParts) : string.Empty;
		}
	}
}
