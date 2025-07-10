using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;

namespace Enterprise.Customs.IE.Business.AES
{
	public class WarehouseProvider : IWarehouse
	{
		public static WarehouseProvider New(string type, string id)
		{
			if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(id))
			{
				return null;
			}
			else
			{
				return new WarehouseProvider(type, id);
			}
		}

		WarehouseProvider(string type, string id)
		{
			Type = type;
			Identifier = id;
		}

		public string Type { get; }
		public string Identifier { get; }
	}
}
