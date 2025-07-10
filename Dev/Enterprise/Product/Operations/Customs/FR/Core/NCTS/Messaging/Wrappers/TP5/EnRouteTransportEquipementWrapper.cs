using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EnRouteTransportEquipmentWrapper : ITransportEquipment
	{
		EnRouteTransportEquipmentWrapper(NctsContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}
		readonly NctsContainer container;

		public static EnRouteTransportEquipmentWrapper New(NctsContainer container) => new EnRouteTransportEquipmentWrapper(container);

		public string ContainerIdentificationNumber => containerIdentificationNumber ?? (containerIdentificationNumber = container.BC_ContainerNum);
		string containerIdentificationNumber;

		public ICollection<IGoodsReference> GoodsReference => goodsReference ?? (goodsReference = GetGoodsReferenceCollection());
		ICollection<IGoodsReference> goodsReference;

		ICollection<IGoodsReference> GetGoodsReferenceCollection()
		{
			var listOfGoodReferenceWrapper = new Collection<IGoodsReference>();
			container.ItemNumbers.Where(x => !x.CY_DataNumeric.IsEmpty).ForEach(goodsItemsNumber => listOfGoodReferenceWrapper.Add(GoodsReferenceWrapper.New(goodsItemsNumber.CY_DataNumeric.ToString())));

			return listOfGoodReferenceWrapper;
		}

		public string NumberOfSeals => numberOfSeals ?? (numberOfSeals = container.TotalSealCount.ToString());
		string numberOfSeals;

		public ICollection<ISeal> Seal => seal ?? (seal = GetSealCollection());
		ICollection<ISeal> seal;

		ICollection<ISeal> GetSealCollection()
		{
			var list = new List<SealWrapper>();

			if (!container.BC_Seal1.IsEmpty)
			{
				list.Add(SealWrapper.New(container.BC_Seal1));
			}
			if (!container.BC_Seal2.IsEmpty)
			{
				list.Add(SealWrapper.New(container.BC_Seal2));
			}

			container.Seals.Where(x => !x.BK_SealNumber.IsEmpty).ForEach(y => list.Add(SealWrapper.New(y.BK_SealNumber)));

			return new Collection<ISeal>(list.ToArray());
		}
	}
}
