using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class ContainerWrapper : IContainer
	{
		public ContainerWrapper(ZString containerNumber)
		{
			this.containerNumber = containerNumber;
		}
		readonly ZString containerNumber;

		string IContainer.ContainerNumber => containerNumber;
	}
}
