using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCTOCusHAWB : DocCusHAWBBase
	{
		protected DocCTOCusHAWB(CTOCusHAWB hawb, BusinessObjectFactory factory)
			: base(hawb, factory)
		{
		}

		public static DocCTOCusHAWB New(CTOCusHAWB hawb, BusinessObjectFactory factory)
		{
			return hawb == null ? null : new DocCTOCusHAWB(hawb, factory);
		}

		public ZString ConsignorPostalAddress
		{
			get
			{
				AddressFormatter formatter = new AddressFormatter(
					Factory,
					Hawb.CS_ConsignorName,
					Hawb.CS_ConsignorStreet,
					Hawb.CS_ConsignorStreet2,
					Hawb.CS_ConsignorCity,
					Hawb.CS_ConsignorState,
					Hawb.CS_ConsignorPostcode,
					Hawb.ConsignorCountry == null ? null : Hawb.ConsignorCountry.RN_DescMultilingual,
					true);

				return formatter.PostalAddress();
			}
		}

		public ZString ConsigneePostalAddress
		{
			get
			{
				AddressFormatter formatter = new AddressFormatter(
					Factory,
					Hawb.CS_ConsigneeName,
					Hawb.CS_ConsigneeStreet,
					Hawb.CS_ConsigneeStreet2,
					Hawb.CS_ConsigneeCity,
					Hawb.CS_ConsigneeState,
					Hawb.CS_ConsigneePostcode,
					Hawb.ConsigneeCountry == null ? null : Hawb.ConsigneeCountry.RN_DescMultilingual,
					true);

				return formatter.PostalAddress();
			}
		}

		CTOCusHAWB Hawb
		{
			get { return (CTOCusHAWB)WrappedObject; }
		}
	}
}
