using System.Collections.Generic;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRContainerDataObjectWriter : DataObjectWriter<JPAFRContainer, Container>
	{
		public JPAFRContainerDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Container PopulateDataObject(JPAFRContainer containerBO)
		{
			var billBO = containerBO.Bill;
			var isShippingLineEntry = billBO != null && billBO.IsShippingLineEntry;
			var containerData = new Container(writeManager.WriterStrategy)
			{
				ContainerNumber = containerBO.JPC_ContainerNum,
				Seal = containerBO.JPC_Seal1,
				SecondSeal = containerBO.JPC_Seal2,
				IsEmptyContainer = containerBO.JPC_IsEmpty
			};

			var containerType = containerBO.ContainerType;
			if (containerType != null)
			{
				containerData.ContainerType = ContainerType.New(containerType);
				containerData.TotalHeight = containerType.RC_Height;
				containerData.TotalWidth = containerType.RC_Width;
				containerData.TotalLength = containerType.RC_Length;
			}

			containerData.SetAddInfoCollection(() =>
			{
				var addInfoList = new List<AddInfo>(new[]
				{
					new AddInfo()
					{
						Key = AddInfoConstants.Container.ContainerOwnershipCode,
						Value = containerBO.JPC_OwnershipCode
					}
				});

				if (isShippingLineEntry)
				{
					addInfoList.Add(new AddInfo()
					{
						Key = AddInfoConstants.Container.ContainerTypeOfService,
						Value = containerBO.JPC_TypeOfService
					});
					addInfoList.Add(new AddInfo()
					{
						Key = AddInfoConstants.Container.ContainerVanningType,
						Value = containerBO.JPC_VanningType
					});
					addInfoList.Add(new AddInfo()
					{
						Key = AddInfoConstants.Container.ContainerCCCApplicationId,
						Value = containerBO.JPC_CCCApplicationId
					});
					addInfoList.Add(new AddInfo()
					{
						Key = AddInfoConstants.Container.ContainerSearchExclusionId,
						Value = containerBO.JPC_SearchExclusionId
					});
				}
				return addInfoList;
			});
			return containerData;
		}
	}
}
