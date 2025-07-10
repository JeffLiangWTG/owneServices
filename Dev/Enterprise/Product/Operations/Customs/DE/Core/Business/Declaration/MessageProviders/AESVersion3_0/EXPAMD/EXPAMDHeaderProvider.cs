using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPAMDHeaderProvider : AESHeaderProvider, IEXPAMDHeader
	{
		public EXPAMDHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
		}
		readonly ExportEntryMessageSendingAction action;

		bool IAESHeader.IsContainerized => GetIsContainerizedCore;

		bool GetIsContainerizedCore => base.IsContainerized && Declaration.JE_ContainerMode != Core.Constants.ContainerModes.LCL;

		IReadOnlyCollection<ITransportEquipment> IAESHeader.TransportEquipments => transportEquipments ?? (transportEquipments = GetIsContainerizedCore ? Declaration.CusContainers
			.Where(x => !x.CO_ContainerNumber.IsEmpty).Select(x => TransportEquipmentProvider.NewOrNull(EntryInstruction, x)).ToArray() : Array.Empty<ITransportEquipment>());
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		string IAESHeader.LocalReferenceNumber => MRN.IsNullOrEmpty() ? LocalReferenceNumber : null;

		IAESParty IAESHeader.Declarant => CachedValueHelper.GetValue(ref declarant, () => EntryInstruction.Constellation3rdDigitIs0() ? PartyProvider.NewOrNull(Declaration.Declarant) : null);
		CachedValue<IAESParty> declarant;

		public IReadOnlyCollection<IEXPAMDLine> Lines
		{
			get
			{
				if (lines == null)
				{
					var shouldSendCusEntryLinePKs = action.EntryLines.Cast<ExportEntryLine>().Where(x => x.ShouldSend).Select(x => x.MessagingObject.PK);
					lines = EntryHeader.MergedLines.Where(x => shouldSendCusEntryLinePKs.Contains(x.PK)).Select(x => new EXPAMDLineProvider(x, this)).ToArray();
				}
				return lines;
			}
		}
		IReadOnlyCollection<IEXPAMDLine> lines;
	}
}
