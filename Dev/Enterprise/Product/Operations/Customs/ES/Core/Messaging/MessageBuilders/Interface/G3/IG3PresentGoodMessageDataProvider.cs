using System.Collections.Generic;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface IG3PresentGoodMessageDataProvider : IG3CommonMessageDataProvider
	{
		IG3PresentGoodHeader Header { get; }
	}

	public interface IG3PresentGoodHeader : IG3CommonHeader
	{
		IReadOnlyCollection<IG3PresentGoodMasterConsignment> MasterConsignment { get; }
	}

	public interface IG3PresentGoodMasterConsignment : IG3MasterConsignment
	{
		IReadOnlyCollection<IG3HouseConsignment> HouseConsignment { get; }
	}
}
