using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IdTypeProvider : IIdType
	{
		IdTypeProvider(string type, string id)
		{
			Type = type;
			Id = id;
		}

		public static IdTypeProvider New(string type, string id)
		{
			return new IdTypeProvider(type, id);
		}

		public string Type { get; }

		public string Id { get; }
	}
}
