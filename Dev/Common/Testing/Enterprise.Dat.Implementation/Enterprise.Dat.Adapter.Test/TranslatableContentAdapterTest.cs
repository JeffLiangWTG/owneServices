using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppDomainWrappers.Net;
using CargoWise.IO;
using Dat.Integration;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TranslatableContentAdapterTest : TestCase
	{
		[DeveloperOnlyTest] // Test takes 10-15 minutes to execute
		public void TestTranslatableContentAdapterExportRealDataInNewEnvironnment()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			using (var tempDirectory = new TempDirectory())
			{
				var domainData = new Dictionary<string, object>
				{
					{ "tempDirectory", tempDirectory.DirectoryName },
					{ "binPath", CargoWise.Common.AssemblyLoader.GetBinPath() },
					{ "sourcePath", BaseSourcePath },
				};

				using (var appDomainWrapper = new AppDomainWrapper())
				{
					appDomainWrapper.RunActionInAppDomain(() =>
					{
						using (var exporter = new TranslatableContentAdapter(new TranslatableContentAdapterContext((string)AppDomain.CurrentDomain.GetData("binPath"), (string)AppDomain.CurrentDomain.GetData("sourcePath"), new TaskLogger())))
						{
							exporter.ExportAllContent("GUI", (string)AppDomain.CurrentDomain.GetData("tempDirectory"));
						}
					}, domainData);
				}
				Assert(Directory.GetDirectories(tempDirectory, "*", SearchOption.AllDirectories).Any());
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}
	}
}
