using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
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
