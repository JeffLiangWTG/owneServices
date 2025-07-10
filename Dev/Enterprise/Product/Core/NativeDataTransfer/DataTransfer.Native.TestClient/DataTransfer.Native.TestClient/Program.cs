using System;
using System.IO;
using System.Windows.Forms;

namespace Enterprise.DataTransfer.Native.TestClient
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 4)
			{
				try
				{
					var wrapper = new ServiceClientWrapper(args[1]);
					var text = File.ReadAllText(args[2]);
					var result = args[0] == "Retrieve" ? wrapper.Retrieve(text) : wrapper.Update(text);
					File.WriteAllText(args[3], result);
				}
#pragma warning disable ENT0001
				catch (Exception ex) // CriticalExceptionIsHandled Reason = Top Level
#pragma warning restore ENT0001
				{
					File.WriteAllText(args[3], ex.ToString());
				}
			}
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				var winForm = new TestClientForm();

				Application.Run(winForm);
			}
		}
	}
}
