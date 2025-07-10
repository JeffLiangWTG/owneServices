using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM099Provider : IIM099Provider
	{
		public IM099Provider(Im099 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im099 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZDateTime DateLimitOfResponse => (xmlObject.ImportOperation?.DateLimitOfResponse).ConvertToZDateTime();

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;

		public ZString CustomsOfficeOfPresentation => xmlObject.CustomsOfficeOfPresentation?.ReferenceNumber;

		public ZString SupervisingCustomsOffice => xmlObject.SupervisingCustomsOffice?.ReferenceNumber;

		public ZString CustomsOfficeLodgement => xmlObject.CustomsOfficeLodgement?.ReferenceNumber;
	}
}
