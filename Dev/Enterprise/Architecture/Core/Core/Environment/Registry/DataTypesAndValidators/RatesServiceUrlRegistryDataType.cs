using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class RatesServiceUrlRegistryDataType : ServiceUrlRegistryDataType
	{
		public RatesServiceUrlRegistryDataType() : base()
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new RatesServiceUrlRegistryEditorInfo();
		}
	}
}
