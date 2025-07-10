using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CustomsOfficesOfDischargeWrapper : ICustomsOfficesOfDischarge
	{
		CustomsOfficesOfDischargeWrapper(EuOfficeCode officeCode)
		{
			this.officeCode = Argument.NotNull(officeCode, nameof(officeCode));
		}
		readonly EuOfficeCode officeCode;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = officeCode.CY_Data);
		string referenceNumber;

		public static CustomsOfficesOfDischargeWrapper New(EuOfficeCode officeCode) => officeCode == null ? null : new CustomsOfficesOfDischargeWrapper(officeCode);
	}
}
