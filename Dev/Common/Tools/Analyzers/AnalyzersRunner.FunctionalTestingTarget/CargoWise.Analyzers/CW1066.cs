namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1066
	{
		public void Method(int x)
		{
			switch (x)
			{
				case 0:
					break;

				default:
					//CW1066:Goto Default Or Case Rule
					goto case 0;
				}
			}
	}
}
