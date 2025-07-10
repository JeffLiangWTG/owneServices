using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	[CodeProperty("Code"), DescriptionProperty("Name")]
	public class Port : NonPersistentBusinessObject
	{
		public Port(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList.Factory)
		{
			this.cusCodeList = cusCodeList;
		}
		readonly ZZRefCusCodeListCombined cusCodeList;

		public ZString Code => cusCodeList.ZZD_Code;

		public ZString Type => cusCodeList.GetAttribute(UniversalReferenceConstants.PortAttributes.Type);

		public ZString Name => cusCodeList.ZZD_Description;

		public bool IsSea => Type == PortTypeList.Codes.SeaPort;

		public static ZQuery GetFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString transportMode)
		{
			var airPortTypeCodes = GetPortAttributeTypes(transportMode);

			var attributesFilter = Enumerable.Empty<RefCusCodeListAttributeFilter>();
			if (!transportMode.IsEmpty && airPortTypeCodes.Any())
			{
				attributesFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(UniversalReferenceConstants.PortAttributes.Type, JoinCondition.And, airPortTypeCodes) };
			}

			return ZZRefCusCodeListCombined.Loader.GetFilter(factory, dataGroupingCode, UniversalReferenceConstants.RefCusCodeListType.Port, ZDateTime.Now, attributesFilter, false);
		}

		static ZString[] GetPortAttributeTypes(ZString transportMode)
		{
			var typeCodes = new PortTypeList().GetAllCodesZString();
			switch (transportMode)
			{
				case TransportTypeList.Codes.Air:
					typeCodes = new ZString[] { PortTypeList.Codes.DesignatedAirport, PortTypeList.Codes.CertificateOfAgreementAirport, PortTypeList.Codes.MilitaryAirport, PortTypeList.Codes.CertificateOfAgreement3rdCountryOnly };
					break;
				case TransportTypeList.Codes.Sea:
					typeCodes = new ZString[] { PortTypeList.Codes.SeaPort, PortTypeList.Codes.CollectorsOffice, PortTypeList.Codes.FreeZone, PortTypeList.Codes.InlandClearanceDepot };
					break;
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Rail:
					typeCodes = new ZString[] { PortTypeList.Codes.CollectorsOffice, PortTypeList.Codes.FreeZone, PortTypeList.Codes.InlandClearanceDepot, PortTypeList.Codes.OverseasMailOffice, PortTypeList.Codes.RoadPort };
					break;
				case TransportTypeList.Codes.Mail:
					typeCodes = new ZString[] { PortTypeList.Codes.OverseasMailOffice };
					break;
			}

			return typeCodes;
		}

		public static Port LoadByCode(BusinessObjectFactory factory, ZString dataGroupingCode, ZString portCode)
		{
			var query = GetFilter(factory, dataGroupingCode, ZString.Empty);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, portCode);

			var cusCodeList = factory.LoadTop1<ZZRefCusCodeListCombined>(query);
			return cusCodeList != null ? new Port(cusCodeList) : null;
		}
	}
}
