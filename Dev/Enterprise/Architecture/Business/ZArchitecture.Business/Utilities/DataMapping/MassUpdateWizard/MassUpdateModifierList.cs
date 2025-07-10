using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class MassUpdateModifierList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Overwrite = "OVR";
			public const string Add = "ADD";
			public const string Subtract = "SUB";
			public const string Multiply = "MUL";
			public const string Divide = "DIV";
		}

		public static class Descriptions
		{
			public static string Overwrite => Res.GetString("7f28ce7f-08c9-413f-a0ed-6e2cf330cefa", "Overwrite");
			public static string Add => Res.GetString("b0964c22-19f7-4c95-8387-10190d91f325", "Add");
			public static string Subtract => Res.GetString("8bc01479-aff6-4691-b88d-e248ac4cd052", "Subtract");
			public static string Multiply => Res.GetString("d0e8f49f-af5d-4015-a9e5-a400297ea8c5", "Multiply");
			public static string Divide => Res.GetString("d99db7d3-2d57-4a9f-a97c-d27e3cf42b74", "Divide");
		}

		public MassUpdateModifierList()
		{
			AddPair(Codes.Overwrite, Descriptions.Overwrite);
			AddPair(Codes.Add, Descriptions.Add);
			AddPair(Codes.Subtract, Descriptions.Subtract);
			AddPair(Codes.Multiply, Descriptions.Multiply);
			AddPair(Codes.Divide, Descriptions.Divide);
		}
	}
}
