using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class TypeOfDutyProvider
	{
		public TypeOfDutyProvider(ITypeOfDuty typeOfDutyProvider)
		{
			this.typeOfDutyProvider = typeOfDutyProvider;
		}
		readonly ITypeOfDuty typeOfDutyProvider;

		public ZString UnionCode => typeOfDutyProvider.UnionCode;
		public ZString NationalCode => typeOfDutyProvider.NationalCode;
	}
}
