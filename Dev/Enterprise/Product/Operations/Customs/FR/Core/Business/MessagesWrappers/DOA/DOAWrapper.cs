using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Messaging.Interfaces.DOA;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DOA
{
	public class DOAWrapper : IDOA
	{
		public DOAWrapper(DOADataObject doa)
		{
			this.doa = Argument.NotNull(doa, nameof(doa));
		}

		readonly DOADataObject doa;

		public ZString PortSystem => doa.PortSystem;

		public ZString MessageType => MessageSubTypeList.Codes.DOA;

		public ZString SenderUser => doa.SendingPartySICCode;

		public ZString SenderTiersProf => doa.SendingPartyID;

		public ZString RecipientUser => doa.RecipientSICCode;

		public ZString RecipientTiersProf => doa.RecipientID;

		public ZString CTOUser => doa.CTOPartySICCode;

		public ZString DeclarationNumber => doa.DeclarationNumber;

		public ZString DeclarationType => doa.DeclarationType;

		public ZString CommonAccessReference => doa.CommonAccessRef;

		public ZString EquipmentReference => doa.EquipmentRef;

		public IReadOnlyCollection<ZString> Containers => doa.Containers?.Select(x => x.Number).ToList();

		public ZString DeclarationReference => doa.DeclarationReference;

		public ZString DeclarationStatus => doa.DeclarationStatus;

		public ZString JobReference => doa.JobNumber;

		public ZBool Prelodged => doa.IsPrelodged;

		public ZString CustomsDepartureOffice => doa.CustomsOfficeCodeOfDeparture?.Code ?? ZString.Empty;

		public ZString CustomsDestinationOffice => doa.CustomsOfficeCodeOfDestination?.Code ?? ZString.Empty;

		public ZInt TotalNumberOfPackages => doa.TotalNumberOfPacks;

		public ZInt TotalGrossWeightInKilograms => doa.TotalGrossWeightInKilograms.ToZInt();

		public ZInt TotalNetWeightInKilograms => doa.TotalNetWeightInKilograms.ToZInt();

		public ZBool HasSeal => doa.HasSeal;

		public ZInt HarborDuesAmount => doa.PortDuesAmount.ToZInt();

		public ZString HarborDuesCurrency => doa.PortDuesCurrency?.Code ?? ZString.Empty;

		public IEnumerable<CodeDescriptionPair> Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					tariffs = doa.Tariffs?.Select(x => new CodeDescriptionPair(x.Code.ToString(), x.Description)).ToArray() ?? Array.Empty<CodeDescriptionPair>();
				}
				return tariffs;
			}
		}
		CodeDescriptionPair[] tariffs;
	}
}
