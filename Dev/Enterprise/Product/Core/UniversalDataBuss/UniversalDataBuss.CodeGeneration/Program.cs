using System;

namespace UniversalDataBuss.CodeGeneration
{
	class Program
	{
		static void Main(string[] args)
		{
			DataObjectGenerator.Generate();
			Console.WriteLine("CodeGeneration: Complete.");
		}
	}
}
