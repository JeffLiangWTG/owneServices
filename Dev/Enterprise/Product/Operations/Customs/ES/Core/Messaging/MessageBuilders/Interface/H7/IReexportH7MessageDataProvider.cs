using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IReexportH7MessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString OperationCode { get; }
		IPartyNameProvider Declarant { get; }
		IReadOnlyCollection<ZString> DeclarationMRNCodes { get; }
	}
}
