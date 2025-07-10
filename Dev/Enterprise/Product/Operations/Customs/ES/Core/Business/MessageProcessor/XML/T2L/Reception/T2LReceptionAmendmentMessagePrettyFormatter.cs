using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionModificaV1Sal;

namespace Enterprise.Customs.ES.Business
{
	public class T2LReceptionAmendmentMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LrecepcionModificaV1Sal>
	{
		public T2LReceptionAmendmentMessagePrettyFormatter(T2LrecepcionModificaV1Sal response) : base(response)
		{
		}
	}
}
