using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsNativeBuilderChooser : NctsChooser<INctsNativeBuilder>, INctsNativeBuilder
	{
		public NctsNativeBuilderChooser(NctsHeader nctsHeader, NctsMessageFunctionSet messageFunction)
			: base("EU.NCTS.Business.MessageGeneration.INctsNativeBuilder", GetNctsCountryFromNctsHeader(nctsHeader, messageFunction))
		{
		}

		#region INctsNativeBuilder Members

		public ZString NativeMessage<T>(T nctsHeader, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			var native = ZString.Empty;
			var nativeBuilder = CountrySpecificChooser;
			if (nativeBuilder != null)
			{
				native = nativeBuilder.NativeMessage(nctsHeader, messageFunction, errorCollector);
			}
			return native;
		}

		#endregion
	}
}
