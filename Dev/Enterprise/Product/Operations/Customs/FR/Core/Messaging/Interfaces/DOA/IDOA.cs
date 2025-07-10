using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Messaging.Interfaces.DOA
{
	public interface IDOA : IDeclarationBase
	{
		ZString PortSystem { get; }

		ZString SenderUser { get; }

		ZString SenderTiersProf { get; }

		ZString RecipientUser { get; }

		ZString RecipientTiersProf { get; }

		ZString CTOUser { get; }

		ZString DeclarationNumber { get; }

		ZString DeclarationType { get; }

		ZString CommonAccessReference { get; }

		ZString EquipmentReference { get; }

		IReadOnlyCollection<ZString> Containers { get; }

		ZString DeclarationReference { get; }

		ZString DeclarationStatus { get; }

		ZString JobReference { get; }

		ZBool Prelodged { get; }

		ZString CustomsDepartureOffice { get; }

		ZString CustomsDestinationOffice { get; }

		ZInt TotalNumberOfPackages { get; }

		ZInt TotalGrossWeightInKilograms { get; }

		ZInt TotalNetWeightInKilograms { get; }

		ZBool HasSeal { get; }

		ZInt HarborDuesAmount { get; }

		ZString HarborDuesCurrency { get; }

		IEnumerable<CodeDescriptionPair> Tariffs { get; }
	}
}
