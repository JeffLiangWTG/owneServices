using System.Collections.Generic;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface IG3RevokeGoodMessageDataProvider : IG3CommonMessageDataProvider
	{
		IG3RevokeGoodHeader Header { get; }
	}

	public interface IG3RevokeGoodHeader : IG3CommonHeader
	{
		IReadOnlyCollection<IG3RevokeMasterConsignment> MasterConsignment { get; }
	}

	public interface IG3RevokeMasterConsignment : IG3MasterConsignment
	{
		IReadOnlyCollection<IG3RevokeHouseConsignment> HouseConsignment { get; }
	}

	public interface IG3RevokeHouseConsignment : IG3HouseConsignment
	{
		IDocumentsCommon AdditionalInformation { get; }
	}
}
