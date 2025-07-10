using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC029C;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC029CProvider
	{
		Cc029CType XmlObject { get; }

		public CC029CProvider(Cc029CType xmlObject)
		{
			XmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZString LRN => XmlObject.TransitOperation?.Lrn ?? ZString.Empty;

		public ZDateTime ReleaseDate => (XmlObject.TransitOperation?.ReleaseDate).ConvertToZDateTime();

		public ZString AdditionalDeclarationType => XmlObject.TransitOperation?.AdditionalDeclarationType ?? ZString.Empty;

		public ZString ControlResultCode => XmlObject.ControlResult?.Code ?? ZString.Empty;

		public ZDateTime ControlResultDate => (XmlObject.ControlResult?.Date).ConvertToZDateTime();

		public ZString ControlResultController => XmlObject.ControlResult?.ControlledBy ?? ZString.Empty;

		public ZString ControlResultText => XmlObject.ControlResult?.Text ?? ZString.Empty;
	}
}
