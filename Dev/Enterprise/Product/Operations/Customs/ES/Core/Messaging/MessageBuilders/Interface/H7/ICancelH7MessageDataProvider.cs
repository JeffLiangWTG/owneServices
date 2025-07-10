using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public interface ICancelH7MessageDataProvider : IESEDIMessageCollectionProvider
	{
		ZString DeclarationMRN { get; }
	}
}
