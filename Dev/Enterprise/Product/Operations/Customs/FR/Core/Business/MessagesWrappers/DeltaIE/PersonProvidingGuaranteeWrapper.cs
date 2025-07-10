using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class PersonProvidingGuaranteeWrapper : IPersonProvidingGuarantee
	{
		PersonProvidingGuaranteeWrapper()
		{
		}

		public string IdentificationNumber => string.Empty;

		public static PersonProvidingGuaranteeWrapper New() => new PersonProvidingGuaranteeWrapper();
	}
}
