using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class TaricAdditionalCodeWrapper : ITaricAdditionalCode
	{
		TaricAdditionalCodeWrapper(string additionalCode)
		{
			this.additionalCode = Argument.NotNull(additionalCode, nameof(additionalCode));
		}

		public static TaricAdditionalCodeWrapper New(string additionalCode) => additionalCode == null ? null : new TaricAdditionalCodeWrapper(additionalCode);

		public string TaricAdditionalCode => taricAdditionalCode ?? (taricAdditionalCode = additionalCode);
		string taricAdditionalCode;

		readonly string additionalCode;
	}
}
