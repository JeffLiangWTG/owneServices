namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1173
	{
		public void BadCode()
		{
			// CW1173 Do not invoke _GetString, _GetData or _GetMultilingualString. They bypass automatic Assembly ID management and do not ensure there is a Resource String for translation.
			_ = Res._GetString(1234, "abcd-1234-efgh-5678", "Default text");
		}
	}
}
