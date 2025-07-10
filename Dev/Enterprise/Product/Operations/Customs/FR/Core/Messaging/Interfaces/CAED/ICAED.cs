using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CAED
{
	public interface ICAED : IDeclarationBase
	{
		ZString PortSystem { get; }

		ZString SenderUser { get; }

		ZString SenderTiersProf { get; }

		ZString RecipientUser { get; }

		ZString RecipientTiersProf { get; }

		ZString CTOUser { get; }

		ZString DeclarationType { get; }

		ZString CommonAccessReference { get; }

		ZString JobReference { get; }

		ZString CustomsDepartureOffice { get; }

		ZInt TotalNumberOfPackages { get; }

		ZInt HarborDuesAmount { get; }

		ZString HarborDuesCurrency { get; }

		ZString DeclarantsSIRETNumber { get; }

		List<ZString> Containers { get; }

		ZBool AppliesToAllPacks { get; }

		ZString Port { get; }
	}
}
