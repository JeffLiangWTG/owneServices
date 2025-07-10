using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Runner
{
	class StdInputRunnerErrorReporter : BaseExceptionReporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "This is a console application")]
		public override void Report(string key, string message, Exception exception)
		{
			Console.WriteLine(message);
			if (exception != null)
			{
				Console.WriteLine(exception.ToString());
			}
			Console.WriteLine("Press enter to continue....");
			Console.ReadLine();
		}
	}
}
