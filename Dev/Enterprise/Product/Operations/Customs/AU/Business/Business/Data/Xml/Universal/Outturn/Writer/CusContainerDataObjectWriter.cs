using System.Linq;
using Enterprise.Customs.DataTransfer.Universal.Outturn;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusContainerDataObjectWriter : DataObjectWriter<DepotCusOutturn, Container>
	{
		public CusContainerDataObjectWriter(IDataWritingManager writeManager, CusContainerLinkManager manager) : base(writeManager)
		{
			this.manager = manager;
		}
		readonly CusContainerLinkManager manager;

		protected override Container PopulateDataObject(DepotCusOutturn sourceBO)
		{
			var containerNumber = sourceBO.C5_ContainerNumber;
			if (!manager.ContainerNumberAndLink.ContainsKey(containerNumber))
			{
				int maxNumber = manager.ContainerNumberAndLink.Any() ? manager.ContainerNumberAndLink.Select(c => c.Value).Max() : 0;
				var newContainerLink = ++maxNumber;
				manager.ContainerNumberAndLink.Add(containerNumber, newContainerLink);
				return new Container
				{
					ContainerNumber = sourceBO.C5_ContainerNumber,
					Link = newContainerLink,
					ContainerType = new ContainerType
					{
						Code = sourceBO.C5_CargoType,
						Description = sourceBO.Lookups.CargoTypes.GetDescriptionFromCode(sourceBO.C5_CargoType)
					},
					Seal = sourceBO.C5_ContainerSeal,
					IsSealOk = sourceBO.C5_SealIntactIndicator
				};
			}
			return null;
		}
	}
}
