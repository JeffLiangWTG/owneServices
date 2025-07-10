using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public sealed class IDTypeWrapper : IIDType
	{
		IDTypeWrapper(string value, string schemeID)
		{
			this.value = value;
			this.schemeID = schemeID;
		}

		public static IDTypeWrapper NewOrNull(ZString value, string schemeID = null) => value.IsEmpty ? null : new IDTypeWrapper(value, schemeID);

		public string Value => value;

		public string SchemeID => schemeID;

		readonly string value;
		readonly string schemeID;
	}
}
