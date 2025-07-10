using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public sealed class CodeTypeWrapper : ICodeType
	{
		CodeTypeWrapper(ZString value)
		{
			this.value = value;
		}
		readonly ZString value;

		public static CodeTypeWrapper NewOrNull(ZString value) => value.IsEmpty ? null : new CodeTypeWrapper(value);

		public string Value => value;
	}
}
