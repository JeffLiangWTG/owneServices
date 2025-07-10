using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Moq;

namespace Enterprise.DocumentScanning.Business.Testing.AssemblyData
{
	sealed class AssemblyDataLoaderTest : TestCaseWithFactory
	{
		public void TestCountryAssemblyDataProvidersUseTypeDecidersWhenNecessary()
		{
			var assemblyDataProviders = AssemblyDataLoader.GetAssemblyDataProviders().GroupBy(a => a.DocManagerCode);
			CombineAssertions(() =>
			{
				foreach (var providers in assemblyDataProviders)
				{
					AssertEquals($@"All AssemblyDataProviders for DocManagerCode '{providers.Key}' must specify the same base type, and use a TypeDecider to ensure the correct type is loaded.
{string.Join("\r\n", providers.Select(x => x.TypeName))}", 1, providers.Select(p => new AssemblyDataHolder(p).BusinessObjectType).Distinct().Count());
				}
			});
			Assert("Avoid Empty Test", true);
		}

		public void TestLoadRegardlessOfCompany()
		{
			var list = new List<KeyValuePair<string, IAssemblyData>>();
			foreach (KeyValuePair<string, IAssemblyData> o in (new AssemblyDataLoader().LoadRegardlessOfCompany()))
			{
				list.Add(o);
			}
			Assert(list.Count != 0);
		}

		public void TestLoadingNoCountriesDoesNotThrowException()
		{
			var companyWithNoCountryCode = Factory.New<GlbCompany>();
			companyWithNoCountryCode.GC_RN_NKCountryCode = string.Empty;
			AssertNoExceptionThrown("AssemblyDataLoader.Load() will not throw exception when Company's Country code is empty string.", () => new AssemblyDataLoader(companyWithNoCountryCode).Load());
		}

		public void TestLoadingDifferentCountries()
		{
			List<KeyValuePair<string, IAssemblyData>> listSIN, listSYD;
			GlbCompany singapore = Factory.Load<GlbCompany>(new Guid("22C79B3E-CD3E-4CA1-8FC9-6DA7AB1BD061"));
			listSIN = new List<KeyValuePair<string, IAssemblyData>>();
			foreach (KeyValuePair<string, IAssemblyData> o in (new AssemblyDataLoader(singapore).Load()))
			{
				listSIN.Add(o);
			}

			GlbCompany sydney = Factory.Load<GlbCompany>(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
			listSYD = new List<KeyValuePair<string, IAssemblyData>>();
			foreach (KeyValuePair<string, IAssemblyData> o in (new AssemblyDataLoader(sydney).Load()))
			{
				listSYD.Add(o);
			}
			Assert(listSIN.Count != listSYD.Count);
		}

		public void TestLoad_DuplicateDocManagerCode()
		{
			ErrorReporter.Clear();

			var assemblyDataProviderAttributes = new[]
			{
				new AssemblyDataProviderAttribute(AssemblyLoader.LoadAssembly("Enterprise.MasterFiles.Business").GetType("Enterprise.MasterFiles.Business.OrganisationData"), "IAP"),
				new AssemblyDataProviderAttribute(AssemblyLoader.LoadAssembly("Enterprise.MasterFiles.Business").GetType("Enterprise.CustomerService.Business.IncidentApprovalData"), "IAP"),
			};
			var assemblyMetaDataReaderMock = new Mock<IAssemblyMetaDataReader>();
			assemblyMetaDataReaderMock.Setup(m => m.GetAttributes<AssemblyDataProviderAttribute>(false)).Returns(assemblyDataProviderAttributes);

			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				var sydney = Factory.Load<GlbCompany>(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
				new AssemblyDataLoader(sydney).Load();

				AssertEndsWith("Error will occur for duplicate DocManagerCode and a full build may fix it", "have same DocManagerCode 'IAP'.\r\nIf you are in debug mode, please try to perform a full build with QGL. If the issue persists, please send the error report.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}
	}
}
