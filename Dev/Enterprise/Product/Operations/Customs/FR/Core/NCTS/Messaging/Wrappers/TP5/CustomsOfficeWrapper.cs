using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CustomsOfficeWrapper : ICustomsOffice
	{
		CustomsOfficeWrapper(string customsOfficeReference)
		{
			this.customsOfficeReference = Argument.NotNull(customsOfficeReference, nameof(customsOfficeReference));
		}
		readonly string customsOfficeReference;

		public string ReferenceNumber => customsOfficeReference;

		public static CustomsOfficeWrapper New(string customsOfficeReference) => customsOfficeReference == null ? null : new CustomsOfficeWrapper(customsOfficeReference);
	}
}
