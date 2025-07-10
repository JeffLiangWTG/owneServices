using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IIE347Message
	{
		IIE347MessageBody Body { get; }
	}
}
