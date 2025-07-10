using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	internal class ContainerWrapper : IContainer
	{
		readonly ZString containerNumber;

		public ContainerWrapper(ZString containerNumber)
		{
			this.containerNumber = containerNumber;
		}

		ZString IContainer.ContainerNumber => containerNumber;
	}
}
