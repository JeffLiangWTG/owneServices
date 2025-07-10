using System;
using System.Windows.Forms;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;

namespace Enterprise.DataTransfer.Native.Generator
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var winForm = new NativeSchemaGenerationForm();
			winForm.Locator = new DefinitionAssemblyLoader();

			Application.Run(winForm);
		}
	}
}
