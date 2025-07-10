using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Code")]
	public class HarmonisedCodeWrapper : GenericWrapper
	{
		public HarmonisedCodeWrapper(IHarmonisedCode harmonisedCode, BusinessObjectFactory factory)
			: base(harmonisedCode as BusinessObject, factory)
		{
			harmonisedCodeBO = harmonisedCode;
			countryCode = harmonisedCodeBO == null ? ZString.Empty : harmonisedCodeBO.Country;
		}

		public ZString Code
		{
			get { return harmonisedCodeBO == null ? ZString.Empty : harmonisedCodeBO.Code; }
		}

		public CountryWrapper Country
		{
			get { return country ?? (country = new CountryWrapper(countryCode, Factory)); }
		}
		CountryWrapper country;

		#region Implementation

		readonly IHarmonisedCode harmonisedCodeBO;
		readonly ZString countryCode;

		#endregion
	}
}
