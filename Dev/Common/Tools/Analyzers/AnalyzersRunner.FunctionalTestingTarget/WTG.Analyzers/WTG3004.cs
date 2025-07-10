namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	static class WTG3004
	{
		public static void ArrayCreator()
		{
			//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
			var o = new object[0];

			//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
			var s = new string[0];

			//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
			var i = new int[0];

			//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
			var ni = new int?[0];
		}

		public static void GenericArrayCreator<T>()
		{
			//WTG3004:Prefer Array.Empty<T>() over creating a new empty array
			var t = new T[0];
		}
	}
}
