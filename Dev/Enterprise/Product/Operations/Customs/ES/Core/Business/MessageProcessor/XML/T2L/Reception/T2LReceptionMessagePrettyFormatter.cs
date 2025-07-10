using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionV1Sal;

namespace Enterprise.Customs.ES.Business
{
	public class T2LReceptionMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LrecepcionV1Sal>
	{
		public T2LReceptionMessagePrettyFormatter(T2LrecepcionV1Sal response) : base(response)
		{
		}
	}
}
