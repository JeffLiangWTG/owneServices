using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContainerPenaltyWrapperCollection : GenericWrapperCollection<ContainerPenaltyWrapper>
	{
		#region Constructors

		public ContainerPenaltyWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Consol

		public ContainerPenaltyWrapperCollection(CommonConsol consolBO, BusinessObjectFactory factory, ContainerPenaltyDirection direction = ContainerPenaltyDirection.Both)
			: base(factory)
		{
			if (consolBO == null)
			{
				return;
			}

			switch (direction)
			{
				case ContainerPenaltyDirection.Import:
					AddConsolContainerImportPenalties(consolBO, factory);
					break;

				case ContainerPenaltyDirection.Export:
					AddConsolContainerExportPenalties(consolBO, factory);
					break;

				case ContainerPenaltyDirection.Both:
				default:
					AddConsolContainerImportPenalties(consolBO, factory);
					AddConsolContainerExportPenalties(consolBO, factory);
					break;
			}
		}

		void AddConsolContainerImportPenalties(CommonConsol consolBO, BusinessObjectFactory factory)
		{
			foreach (ContainerPenalty penaltyBO in consolBO.Containers.Cast<CommonContainer>().SelectMany(containerBO => containerBO.ImportPenalties))
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		void AddConsolContainerExportPenalties(CommonConsol consolBO, BusinessObjectFactory factory)
		{
			foreach (ContainerPenalty penaltyBO in consolBO.Containers.Cast<CommonContainer>().SelectMany(containerBO => containerBO.ExportPenalties))
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		#endregion

		#region Shipment

		public ContainerPenaltyWrapperCollection(CommonShipment shipmentBO, BusinessObjectFactory factory, ContainerPenaltyDirection direction = ContainerPenaltyDirection.Both)
			: base(factory)
		{
			if (shipmentBO == null)
			{
				return;
			}

			switch (direction)
			{
				case ContainerPenaltyDirection.Import:
					AddShipmentDeliveryPenalties(shipmentBO, factory);
					break;

				case ContainerPenaltyDirection.Export:
					AddShipmentPickupPenalties(shipmentBO, factory);
					break;

				case ContainerPenaltyDirection.Both:
				default:
					AddShipmentDeliveryPenalties(shipmentBO, factory);
					AddShipmentPickupPenalties(shipmentBO, factory);
					break;
			}
		}

		void AddShipmentDeliveryPenalties(CommonShipment shipmentBO, BusinessObjectFactory factory)
		{
			foreach (var penaltyBO in shipmentBO.DeliveryPenalties)
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		void AddShipmentPickupPenalties(CommonShipment shipmentBO, BusinessObjectFactory factory)
		{
			foreach (var penaltyBO in shipmentBO.PickupPenalties)
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		#endregion

		#region Declarations

		public ContainerPenaltyWrapperCollection(BaseJobDeclaration declarationBO, BusinessObjectFactory factory, ContainerPenaltyDirection direction = ContainerPenaltyDirection.Both)
			: base(factory)
		{
			if (declarationBO == null)
			{
				return;
			}

			switch (direction)
			{
				case ContainerPenaltyDirection.Import:
					AddDeclarationImportPenalties(declarationBO, factory);
					break;

				case ContainerPenaltyDirection.Export:
					AddDeclarationExportPenalties(declarationBO, factory);
					break;

				case ContainerPenaltyDirection.Both:
				default:
					AddDeclarationImportPenalties(declarationBO, factory);
					AddDeclarationExportPenalties(declarationBO, factory);
					break;
			}
		}

		void AddDeclarationImportPenalties(BaseJobDeclaration declarationBO, BusinessObjectFactory factory)
		{
			foreach (ContainerPenalty penaltyBO in declarationBO.CusContainers.SelectMany(cusContainerBO => cusContainerBO.ImportPenalties))
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		void AddDeclarationExportPenalties(BaseJobDeclaration declarationBO, BusinessObjectFactory factory)
		{
			foreach (ContainerPenalty penaltyBO in declarationBO.CusContainers.SelectMany(cusContainerBO => cusContainerBO.ExportPenalties))
			{
				var wrapper = new ContainerPenaltyWrapper(penaltyBO, factory);
				Add(wrapper);
			}
		}

		#endregion
	}

	#endregion

	public enum ContainerPenaltyDirection
	{
		Import,
		Export,
		Both
	}
}
