using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class TransportEquipmentWrapper : ITransportEquipment
	{
		TransportEquipmentWrapper(TemporaryStorageContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}

		public readonly TemporaryStorageContainer container;

		public string ContainerIdentificationNumber => containerIdentificationNumber ?? (containerIdentificationNumber = container.ACN_ContainerNumber);
		string containerIdentificationNumber;

		public string ContainerPackedStatus => containerPackedStatus ?? (containerPackedStatus = container.ACN_EmptyFullIndicator);
		string containerPackedStatus;

		public string NumberOfSeals => numberOfSeals ?? (numberOfSeals = Seal.Count.ToString());
		string numberOfSeals;

		public ICollection<ISeal> Seal => seal ?? (seal = GetSealCollection());
		ICollection<ISeal> seal;

		ICollection<ISeal> GetSealCollection()
		{
			var list = new List<SealWrapper>();

			if (!container.ACN_Seal1.IsEmpty)
			{
				list.Add(SealWrapper.New(container.ACN_Seal1));
			}
			if (!container.ACN_Seal2.IsEmpty)
			{
				list.Add(SealWrapper.New(container.ACN_Seal2));
			}
			if (!container.ACN_Seal3.IsEmpty)
			{
				list.Add(SealWrapper.New(container.ACN_Seal3));
			}

			list.AddRange(container.AdditionalSeals.Select(s => SealWrapper.New(s.BK_SealNumber)));

			return new Collection<ISeal>(list.ToArray());
		}

		public static TransportEquipmentWrapper New(TemporaryStorageContainer container) => container == null ? null : new TransportEquipmentWrapper(container);
	}
}
