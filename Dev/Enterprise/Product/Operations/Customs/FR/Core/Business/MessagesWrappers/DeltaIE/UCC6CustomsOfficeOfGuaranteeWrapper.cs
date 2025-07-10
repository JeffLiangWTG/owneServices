using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	internal class UCC6CustomsOfficeOfGuaranteeWrapper : ICustomsOfficeOfGuarantee
	{
		UCC6CustomsOfficeOfGuaranteeWrapper()
		{
		}

		public string ReferenceNumber => string.Empty;

		public static UCC6CustomsOfficeOfGuaranteeWrapper New() => new UCC6CustomsOfficeOfGuaranteeWrapper();
	}
}
