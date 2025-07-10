using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class TransportEquipmentWrapper : ITransportEquipment
	{ 
		protected TransportEquipmentWrapper(NctsCusInBondContainer container, ICollection<string> goodsItemsNumbers)
		{
			this.container = Argument.NotNull(container, nameof(container));
			this.goodsItemsNumbers = Argument.NotNull(goodsItemsNumbers, nameof(goodsItemsNumbers));
		}
		protected readonly NctsCusInBondContainer container;
		protected readonly ICollection<string> goodsItemsNumbers;

		public static TransportEquipmentWrapper New(NctsCusInBondContainer container, ICollection<string> goodsItemsNumbers) => new TransportEquipmentWrapper(container, goodsItemsNumbers);

		public virtual string ContainerIdentificationNumber => containerIdentificationNumber ?? (containerIdentificationNumber = container.BC_ContainerNum);
		string containerIdentificationNumber;

		public ICollection<IGoodsReference> GoodsReference => goodsReference ?? (goodsReference = GetGoodsReferenceCollection());
		ICollection<IGoodsReference> goodsReference;

		protected virtual ICollection<IGoodsReference> GetGoodsReferenceCollection()
		{
			var listOfGoodReferenceWrapper = new Collection<IGoodsReference>();

			goodsItemsNumbers.ForEach(goodsItemsNumber => listOfGoodReferenceWrapper.Add(GoodsReferenceWrapper.New(goodsItemsNumber)));

			return listOfGoodReferenceWrapper;
		}

		public string NumberOfSeals => numberOfSeals ?? (numberOfSeals = GetNumberOfSeals());
		string numberOfSeals;

		protected virtual string GetNumberOfSeals()
		{
			if (container is FRNctsDepartureHeaderContainer departureHeaderContainer)
			{
				return departureHeaderContainer.TotalSealCount.ToString();
			}
			else if (container is NctsArrivalHeaderContainer arrivalHeaderContainer)
			{
				return arrivalHeaderContainer.TotalSealCount.ToString();
			}
			else
			{
				return null;
			}
		}

		public ICollection<ISeal> Seal => seal ?? (seal = GetSealCollection());
		ICollection<ISeal> seal;

		protected virtual ICollection<ISeal> GetSealCollection()
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

			if (container is FRNctsDepartureHeaderContainer departureHeaderContainer)
			{
				list.AddRange(departureHeaderContainer.AdditionalSeals.Select(s => SealWrapper.New(s.BK_SealNumber)));
			}
			else if (container is NctsArrivalHeaderContainer arrivalHeaderContainer)
			{
				list.AddRange(arrivalHeaderContainer.Seals.Select(s => SealWrapper.New(s.BK_SealNumber)));
			}

			return new Collection<ISeal>(list.ToArray());
		}
	}
}
