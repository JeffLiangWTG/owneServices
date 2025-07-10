using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocBillofLadingContainerCollection : DocumentWrapperCollection<DocBillofLadingContainer>
	{
		public DocBillofLadingContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool ContainsContainerWithNumber(ZString containerNumber)
		{
			foreach (DocBillofLadingContainer currentContainer in this)
			{
				if (currentContainer.ContainerNumber == containerNumber)
				{
					return true;
				}
			}

			return false;
		}

		public ZBool ContainsContainerWithNumberAndDeliveryMode(ZString containerNumber, ZString deliveryMode)
		{
			foreach (DocBillofLadingContainer currentContainer in this)
			{
				if (currentContainer.ContainerNumber == containerNumber && currentContainer.Container.DeliveryMode == deliveryMode)
				{
					return true;
				}
			}

			return false;
		}
	}
}
