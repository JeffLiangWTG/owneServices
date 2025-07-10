using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Integration.SadH
;

namespace Enterprise.Customs.GB.Business.Messaging
{
	class Container : IContainer
	{
		public Container(NonPersistentCusContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}
			this.container = container;
		}
		readonly NonPersistentCusContainer container;

		#region IContainer Members

		CargoWise.Types.ZString IContainer.ContainerNumber
		{
			get { return container.ContainerNumber; }
		}

		#endregion
	}
}
