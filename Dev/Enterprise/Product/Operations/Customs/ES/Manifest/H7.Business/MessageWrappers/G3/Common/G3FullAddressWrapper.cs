using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3FullAddressWrapper : IG3FullAddress
	{
		public static G3FullAddressWrapper New(OrgAddress declarant) => declarant == null ? null : new G3FullAddressWrapper(declarant);

		G3FullAddressWrapper(OrgAddress declarant)
		{
			this.declarant = declarant;
		}

		readonly OrgAddress declarant;

		public ZString Street => declarant.OA_Address1 + " " + declarant.OA_Address2;

		public ZString StreetAddLine => string.Empty;

		public ZString Number => declarant.OA_Address1 + " " + declarant.OA_Address2;

		public ZString POBox => string.Empty;

		public ZString SubDivision => string.Empty;

		public ZString Country => declarant.Country.Code;

		public ZString PostCode => declarant.OA_PostCode;

		public ZString City => declarant.City;
	}
}
