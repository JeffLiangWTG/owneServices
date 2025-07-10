using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Security.Cryptography;
using Enterprise.RemoteDesktopServices.Server;
using Microsoft.CSharp;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	[TestRequiresAdministrativePrivileges("Writes to windows registry under local machine")]
	class WiseCloudSecurityClientTest : RemoteDesktopServicesTest
	{
		public void TestGetClientIPAddressSuccess()
		{
			SetupWcaClient();
			AssertEquals("1.2.3.4", new WiseCloudSecurityClientForTest(domainName, wcaSecurityPrivateKey).GetClientIPAddress("XXXYYYZZZ", "test.user"));
		}

		public void TestGetClientIPAddressError()
		{
			SetupWcaClient("This is the failure message");

			var exception = AssertExceptionThrown<OperationCanceledException>("This is the failure message", () =>
			{
				new WiseCloudSecurityClientForTest(domainName, wcaSecurityPrivateKey).GetClientIPAddress("XXXYYYZZZ", "test.user");
			});

			Assert(exception.InnerException is RDPRemoteException rex && rex.RemoteExceptionType == nameof(IOException));
		}

		public void TestGetClientIPAddressInvalidSignature()
		{
			SetupWcaClient();
			using (var p = new RSACryptoServiceProvider())
			{
				wcaSecurityPrivateKey = p.ToXmlString(true);
			}
			AssertExceptionThrown(typeof(IOException), "WCA message signature failed validation", () => new WiseCloudSecurityClientForTest(domainName, wcaSecurityPrivateKey).GetClientIPAddress("XXXYYYZZZ", "test.user"));
		}

		void SetupWcaClient(string error = null)
		{
			wiseCloudClientPath = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(wiseCloudClientPath);

			using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient\" + domainName))
			{
				wcaClientReg.SetValue("Path", Path.Combine(wiseCloudClientPath, "WiseCloudClient.exe"));
			}
			CreateExe(Path.Combine(wiseCloudClientPath, "WiseCloudClient.exe"), error);
		}

		void CreateExe(string path, string error = null)
		{
			CSharpCodeProvider compiler = new CSharpCodeProvider();
			CompilerParameters options = new CompilerParameters();
			options.ReferencedAssemblies.Add("System.dll");
			options.GenerateExecutable = true;
			options.OutputAssembly = Path.Combine(path);
			string cs;
			if (!string.IsNullOrEmpty(error))
			{
				cs = string.Format(
@"					public static class Program
					{{
						public static void Main(string[] cmd)
						{{
							System.Console.Error.Write(""{0}"");
						}}
					}}", error);
			}
			else
			{
				cs = string.Format(
@"					public static class Program
					{{
						public static void Main(string[] cmd)
						{{
							if (cmd[0] != ""GetClientIp"")
							{{
								System.Console.Error.Write(""Incorrect command line option"");
							}}
							else 
							{{
								var ip = ""1.2.3.4"";
								System.Console.Write(ip);
								System.Console.Write(""\t"");
								var allData = ip + ""\t"" + cmd[1] + ""\t"" + cmd[2];
								using (var p = new System.Security.Cryptography.RSACryptoServiceProvider())
								using (var hashAlgorithm = System.Security.Cryptography.SHA256.Create())
								{{
									p.FromXmlString(""{0}"");
									System.Console.WriteLine(System.Convert.ToBase64String(p.SignData(System.Text.Encoding.UTF8.GetBytes(allData), hashAlgorithm)));
								}}
							}}
						}}
					}}", wcaSecurityPrivateKey);
			}
			CompilerResults result = compiler.CompileAssemblyFromSource(options, cs);
			string[] compilerOutput = new string[result.Output.Count];
			result.Output.CopyTo(compilerOutput, 0);
			AssertEquals(string.Join("\r\n", compilerOutput), 0, result.Errors.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			using var p = RSA.Create();
			wcaSecurityPublicKey = p.ToXmlString(false);
			wcaSecurityPrivateKey = p.ToXmlString(true);
		}

		protected override void TearDown()
		{
			try
			{
				if (!string.IsNullOrEmpty(wiseCloudClientPath) && Directory.Exists(wiseCloudClientPath))
				{
					Directory.Delete(wiseCloudClientPath, true);
				}

				using (var hclm32Reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var wcaClientReg = hclm32Reg.CreateSubKey(@"Software\WiseTech Global\WCAClient"))
				{
					wcaClientReg.DeleteSubKey(domainName, false);
				}
			}
			finally
			{
				base.TearDown();
			}
		}

		string wiseCloudClientPath;
		readonly string domainName = Guid.NewGuid().ToString();
		string wcaSecurityPublicKey;
		string wcaSecurityPrivateKey;

		class WiseCloudSecurityClientForTest : WiseCloudSecurityClient
		{
			public WiseCloudSecurityClientForTest(string domain, string wcaSecurityPublicKey)
			{
				this.domain = domain;
				this.wcaSecurityPublicKey = wcaSecurityPublicKey;
			}

			protected override string GetDomain()
			{
				return domain;
			}

			protected override string WCASecurityProviderPublicKey
			{
				get { return wcaSecurityPublicKey; }
			}

			readonly string domain;
			readonly string wcaSecurityPublicKey;
		}
	}
}

