namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	public class CW1082
	{
		public void MethodWith7Params(int param1, int param2, int param3, int param4, int param5, int param6, int param7)
		{ }

		//CW1082W:DoNotUseTooManyArguments
		public void MethodWith8Params(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8)
		{ }

		//CW1082W:DoNotUseTooManyArguments
		public void MethodWith16Params(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13, int param14, int param15, int param16)
		{ }

		//CW1082:DoNotUseTooManyArguments
		public void MethodWith17Params(int param1, int param2, int param3, int param4, int param5, int param6, int param7, int param8, int param9, int param10, int param11, int param12, int param13, int param14, int param15, int param16, int param17)
		{ }
	}
}
