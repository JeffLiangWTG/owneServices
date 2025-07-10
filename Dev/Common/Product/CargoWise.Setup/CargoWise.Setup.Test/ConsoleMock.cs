namespace CargoWise.Setup.Test;

internal class ConsoleMock : IDisposable
{
	public ConsoleMock()
	{
		originalOut = Console.Out;
		originalError = Console.Error;
		Console.SetOut(outputWriter);
		Console.SetError(errorWriter);
	}

	public void Dispose()
	{
		Console.SetOut(originalOut);
		Console.SetError(originalError);
	}

	readonly TextWriter originalOut;
	readonly TextWriter originalError;

	readonly StringWriter outputWriter = new StringWriter();
	readonly StringWriter errorWriter = new StringWriter();

	public string Output => outputWriter.GetStringBuilder().ToString();
	public string Error => errorWriter.GetStringBuilder().ToString();
}
