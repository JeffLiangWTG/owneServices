using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class IdTypeProvider : IIdType
	{
		public static IdTypeProvider New(ZString type, ZString id) => new IdTypeProvider(type, id);

		IdTypeProvider(ZString type, ZString id)
		{
			this.type = type;
			this.id = id;
		}
		readonly ZString type;
		readonly ZString id;

		public string Type => type;

		public string Id => id;
	}
}
