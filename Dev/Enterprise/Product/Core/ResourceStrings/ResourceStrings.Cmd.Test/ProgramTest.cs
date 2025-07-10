using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace ResourceStrings.Cmd.Testing
{
	sealed class ProgramTest : TestCase
	{
		public void TestStdIo()
		{
			var startInfo = new ProcessStartInfo(Assembly.GetAssembly(typeof(ResourceStrings.Cmd.Program)).Location);
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardError = true;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			var process = Process.Start(startInfo);
			try
			{
				string cmd;

				process.StandardInput.WriteLine(cmd = "C	zh-CN");
				AssertEquals(cmd, "ZH-CN", process.StandardOutput.ReadLine());

				process.StandardInput.WriteLine(cmd = "L	EN-US	46D4B528-A46D-4f8a-9A5E-97A8035E5765	CAP");
				AssertEquals(cmd, "Checking for current software version", Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));

				process.StandardInput.WriteLine(cmd = "L	ZH-CN	StmALog	CAP");
				AssertEquals(cmd, "事件", Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));

				process.StandardInput.WriteLine(cmd = "L	EN-US	StmMenuItem|SU_DeliveryRestrictionType	FUL");
				AssertEquals(cmd, "Method of Delivery Restriction. Available options “CNH” (Movement Restricted/Credit on Hold), “UDF” (User Defined Conditions) and “NON” (None)", Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));

				process.StandardInput.WriteLine(cmd = "L	EN-US	StmMenuItem|SU_DeliveryRestrictionType	MED");
				AssertEquals(cmd, "Delivery Restriction Type", Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));

				process.StandardInput.WriteLine(cmd = "L	EN-US	StmMenuItem|SU_DeliveryRestrictionType	SHO");
				AssertEquals(cmd, "Restriction", Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));

				process.StandardInput.WriteLine(cmd = "L	EN-US	NOSUCHSTRING	CAP");
				AssertEquals(cmd, string.Empty, Encoding.UTF8.GetString(Convert.FromBase64String(process.StandardOutput.ReadLine())));
			}
			finally
			{
				process.Kill();
			}
		}
	}
}
