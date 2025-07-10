using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM426;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM426Provider
	{
		public IM426Provider(Im426 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Im426 xmlObject;

		public ZString LRN => xmlObject.ImportOperation?.Lrn;
		public ZString MRN => xmlObject.ImportOperation?.Mrn;
		public ZString CustomsRegistrationNumber => xmlObject.ImportOperation?.CustomsRegistrationNumber;
		public ZDateTime DeclarationRegistrationDateAndTime => (xmlObject.ImportOperation?.DeclarationRegistrationDateAndTime).ConvertToZDateTime();
		public ZDateTime PresentationNotificationDueDate => (xmlObject.ImportOperation?.PresentationNotificationDueDate).ConvertToZDateTime();
	}
}
