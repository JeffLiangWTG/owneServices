using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Documents;
using Enterprise.Customs.FR.Messaging.Interfaces.CAED;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CAED
{
	public class CAEDWrapper : ICAED
	{
		public CAEDWrapper(CAEDDataObject caed)
		{
			this.caed = Argument.NotNull(caed, nameof(caed));
		}

		readonly CAEDDataObject caed;

		public ZString PortSystem => caed.PortSystem;

		public ZString MessageType => MessageSubTypeList.Codes.CAED;

		public ZString SenderUser => caed.SendingPartySICCode;

		public ZString SenderTiersProf => caed.SendingPartyID;

		public ZString RecipientUser => caed.RecipientSICCode;

		public ZString RecipientTiersProf => caed.RecipientID;

		public ZString CTOUser => caed.CTOPartySICCode;

		public ZString DeclarationType => caed.DeclarationType;

		public ZString CommonAccessReference => caed.CommonAccessRef;

		public ZString JobReference => caed.JobNumber;

		public ZString CustomsDepartureOffice => caed.CustomsOfficeCodeOfDeparture?.Code ?? ZString.Empty;

		public ZInt TotalNumberOfPackages => caed.TotalNumberOfPacks;

		public ZInt HarborDuesAmount => caed.PortDuesAmount.ToZInt();

		public ZString HarborDuesCurrency => caed.PortDuesCurrency?.Code ?? ZString.Empty;

		public List<ZString> Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new List<ZString>();
					if (caed.Containers != null)
					{
						foreach (var container in caed.Containers.Where(x => !x.IsEmpty))
						{
							containers.Add(container);
						}
					}
				}
				return containers;
			}
		}

		public ZString DeclarantsSIRETNumber => caed.DeclarantsSIRETNumber;

		public ZBool AppliesToAllPacks => caed.AppliesToAllPacks;

		public ZString Port => caed.Port;

		List<ZString> containers;
	}
}
