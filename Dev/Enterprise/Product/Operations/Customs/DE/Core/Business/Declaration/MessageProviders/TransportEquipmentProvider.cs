using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class TransportEquipmentProvider : ITransportEquipment
	{
		public static TransportEquipmentProvider NewOrNull(CusEntryInstruction entryInstruction, CusContainer container) => container == null ? null : new TransportEquipmentProvider(entryInstruction, container);

		TransportEquipmentProvider(CusEntryInstruction entryInstruction, CusContainer container)
		{
			this.entryInstruction = entryInstruction;
			this.container = container;
		}
		readonly CusEntryInstruction entryInstruction;
		readonly CusContainer container;

		public string ContainerIdentificationNumber => container.CO_ContainerNumber;

		public IReadOnlyCollection<string> SealIdentifiers => sealIdentifiers ?? (sealIdentifiers = Array.Empty<string>());
		IReadOnlyCollection<string> sealIdentifiers;

		public IReadOnlyCollection<int> DeclarationGoodsItemNumbers => declarationGoodsItemNumbers ?? (declarationGoodsItemNumbers = entryInstruction.InvoiceLines.Where(x => x.IsContainedIn(container)).Select(x => (int)x.CusEntryLine.CL_LineNumber).ToArray());

		public int? NumberOfSeals => throw new NotImplementedException("used in DE.ExitControl");

		IReadOnlyCollection<int> declarationGoodsItemNumbers;
	}
}
