using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM099Provider : IIM099Provider
	{
		public IM099Provider(Im099 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Im099 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZDateTime DateLimitOfResponse => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.DateLimitOfResponse);

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public ZString CustomsOfficeLodgement => xmlObject.Declaration?.CustomsOffices?.CustomsOfficeLodgement;
	}
}
