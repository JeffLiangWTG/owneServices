using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface IExpCancelG5MessageDataProvider : IG5GenericMessageDataProvider
	{
		ZString MRN { get; }
		IG5SimplifiedHeader Header { get; }
	}
}
