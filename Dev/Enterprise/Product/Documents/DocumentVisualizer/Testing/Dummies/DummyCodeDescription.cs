using CargoWise.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyCodeDescription : ICodeDescription
	{
		public object PK { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
	}
}