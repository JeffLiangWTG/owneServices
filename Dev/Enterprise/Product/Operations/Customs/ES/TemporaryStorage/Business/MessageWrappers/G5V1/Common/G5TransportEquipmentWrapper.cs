using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5TransportEquipmentWrapper : IG5TransportEquipment
	{
		public G5TransportEquipmentWrapper(TemporaryStorageContainer cont)
		{
			container = Argument.NotNull(cont, nameof(cont));
		}
		protected TemporaryStorageContainer container;

		public ZString Id => container.ACN_ContainerNumber;

		public ZString PackedStatus => container.ACN_EmptyFullIndicator == EmptyFullIndicatorList.Codes.NotEmpty ? container.ACN_EmptyFullIndicator : ZString.Empty;

		public IReadOnlyCollection<ZString> SealIds
		{
			get
			{
				if (sealIds == null)
				{
					var sealList = new List<ZString>();
					var additionalSeals = new List<ZString>();

					sealList = GetSealList(container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3);
					additionalSeals = GetSealList(container.AdditionalSeals.Select(p => p.BK_SealNumber).ToArray());
					additionalSeals.Sort();

					sealList.AddRange(additionalSeals);

					sealIds = sealList.Distinct().ToList().AsReadOnly();
				}
				return sealIds;
			}
		}
		IReadOnlyCollection<ZString> sealIds;

		protected List<ZString> GetSealList(params ZString[] seals)
		{
			var list = new List<ZString>();
			foreach (var seal in seals)
			{
				if (!seal.IsEmpty)
				{
					list.Add(seal);
				}
			}
			return list.Distinct().ToList();
		}
	}
}
