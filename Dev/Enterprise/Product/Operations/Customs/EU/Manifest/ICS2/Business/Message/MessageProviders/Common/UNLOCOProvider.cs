using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class UNLOCOProvider : IUNLOCO
	{
		public UNLOCOProvider(string unlocode, string country)
		{
			this.unlocode = unlocode;
			this.country = country;
		}

		readonly string unlocode;
		readonly string country;

		public string Location => null;

		public string Unlocode => unlocode;

		public string Country => country;
	}
}
