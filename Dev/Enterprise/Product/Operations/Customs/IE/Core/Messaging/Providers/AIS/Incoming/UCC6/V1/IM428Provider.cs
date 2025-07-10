using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1
{
	public class IM428Provider : IIM428Provider
	{
		public IM428Provider(Im428 xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Im428 xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration.Lrn;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString AdditionalDeclarationType => xmlObject.Declaration?.AdditionalDeclarationType;

		public ZDateTime DeclarationAcceptanceDateAndTime => DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime(xmlObject.Declaration?.AcceptanceDate);

		public ZString PreferredPaymentMethod => xmlObject.Declaration?.PreferredPaymentMethod;

		public ZString Remarks => xmlObject.Declaration?.Remarks;

		public void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				var cusEntry = CusEntryNumber.LoadOrCreate((BusinessObject)messageAttachee, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Ireland);
				cusEntry.CE_EntryNum = MovementReferenceNumber;

				aisMessageAttachee.MovementReferenceNumberSetter(MovementReferenceNumber, DeclarationAcceptanceDateAndTime);
			}
		}
	}
}
