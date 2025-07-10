using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface IReceptionG5MessageDataProvider : IG5CommonMessageDataProvider
	{
		ZString MRN { get; }
	}
}
