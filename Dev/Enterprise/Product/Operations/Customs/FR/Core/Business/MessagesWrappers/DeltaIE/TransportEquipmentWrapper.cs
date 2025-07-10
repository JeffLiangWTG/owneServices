using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class TransportEquipmentWrapper : ITransportEquipment
	{
		TransportEquipmentWrapper(ZString containerNumber, List<string> entryLineNumbers)
		{
			this.containerNumber = containerNumber;
			this.entryLineNumbers = entryLineNumbers;
		}
		readonly ZString containerNumber;
		readonly List<string> entryLineNumbers;

		public string ContainerIdentificationNumber => containerIdentificationNumber ?? (containerIdentificationNumber = containerNumber);
		string containerIdentificationNumber;

		public ICollection<IGoodsReference> GoodsReference => goodsReference ?? (goodsReference = GetGoodsReferenceCollection());
		ICollection<IGoodsReference> goodsReference;

		ICollection<IGoodsReference> GetGoodsReferenceCollection()
		{
			var listOfGoodReferenceWrapper = new Collection<IGoodsReference>();

			entryLineNumbers.ForEach(x => listOfGoodReferenceWrapper.Add(GoodsReferenceWrapper.New(x)));

			return listOfGoodReferenceWrapper;
		}

		public static TransportEquipmentWrapper New(ZString containerNumber, List<string> entryLineNumbers) => new TransportEquipmentWrapper(containerNumber, entryLineNumbers);
	}
}
