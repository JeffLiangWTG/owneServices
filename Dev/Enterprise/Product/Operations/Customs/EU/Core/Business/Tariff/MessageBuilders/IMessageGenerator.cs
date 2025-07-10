using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.EU.Business.MessageBuilders
{
	public interface IMessageGenerator<T> where T : BusinessObject
	{
		IBuilderResult Generate(T bizO);
		ZString MakePrettyForInterpretation(EDIMessage message);
		void PutReferenceNumberIntoMessageFromPlaceholder(EDIMessage message, ZString messageText, T bizO);
	}
}
