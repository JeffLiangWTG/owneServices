using CargoWise.Integration;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class IsNotBusinessObjectForTest : ICodeDescription
	{
		public IsNotBusinessObjectForTest(string code)
		{
			Code = code;
		}

		public object PK => null;

		public string Code { get; }

		public string Description => "DescriptionTest";
	}
}
