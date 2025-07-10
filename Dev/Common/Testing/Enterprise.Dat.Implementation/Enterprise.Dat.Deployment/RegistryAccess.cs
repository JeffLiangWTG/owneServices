using CargoWise.Data;

namespace Enterprise.Dat.Implementation
{
	public static class RegistryAccess
	{
		public static StringDbRegistryItem ExpectedClientDll => new StringDbRegistryItem("EXPECTED_CLIENT_DLL");
		public static StringDbRegistryItem ClientDocumentName => new StringDbRegistryItem("ClientDocumentName");
	}
}
