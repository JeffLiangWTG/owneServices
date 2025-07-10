using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC228CGuarantorProvider
	{
		public CC228CGuarantorProvider(GuarantorType03 guarantor)
		{
			this.guarantor = Argument.NotNull(guarantor, nameof(guarantor));
		}
		readonly GuarantorType03 guarantor;

		public ZString IdentificationNumber => guarantor.IdentificationNumber;

		public ZString Name => guarantor.Name;

		public AddressProvider Address
		{
			get
			{
				var xmlAddress = guarantor.Address;
				return xmlAddress == null ? null : addressCached ?? (addressCached = new AddressProvider
				{
					Postcode = xmlAddress.Postcode,
					City = xmlAddress.City,
					Country = xmlAddress.Country.ToString().ToUpper(),
				});
			}
		}
		AddressProvider addressCached;
	}
}
