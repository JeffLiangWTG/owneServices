using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CustomsOfficeOfGuaranteeWrapper : ICustomsOfficeOfGuarantee
	{
		CustomsOfficeOfGuaranteeWrapper(GuaranteeForEntryInstruction guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		readonly GuaranteeForEntryInstruction guarantee;

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = guarantee.PW_BondFiledPort);
		string referenceNumber;

		public static CustomsOfficeOfGuaranteeWrapper New(GuaranteeForEntryInstruction guarantee) => guarantee == null ? null : new CustomsOfficeOfGuaranteeWrapper(guarantee);
	}
}
