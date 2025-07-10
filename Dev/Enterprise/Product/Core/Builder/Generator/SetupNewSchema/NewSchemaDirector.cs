namespace Enterprise.Builder.Generator
{
	public class NewSchemaDirector
	{
		public NewSchemaDirector(GeneratorOutputDirectory outputDirectory)
		{
			this.outputDirectory = outputDirectory;
		}
		readonly GeneratorOutputDirectory outputDirectory;

		public bool DoSetup(bool isAutoRegen, bool showGui, RegenActions regenType)
		{
			if (showGui)
			{
				return NewSchemaForm.OpenSetup(isAutoRegen, outputDirectory, regenType);
			}
			else
			{
				ConsoleActivator.EnsureConsole();
				var controller = new Controller(new ConsoleProgressLogger(), outputDirectory);
				return controller.DoGeneration(regenType);
			}
		}
	}
}
