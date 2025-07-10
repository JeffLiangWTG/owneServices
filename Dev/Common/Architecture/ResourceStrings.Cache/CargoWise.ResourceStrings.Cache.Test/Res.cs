using System;
using System.Linq;
using System.Reflection;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The test of the ResourceString")]
	class ResTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAbleToUseResourceStringCache()
		{
			Res._GetData(1, "whatever", "whatever");
		}

		public void TestIsEnglish()
		{
			foreach (var fieldInfo in typeof(SharedConstants.Languages).GetFields())
			{
				AssertEquals("Res.IsEnglish(" + fieldInfo.Name + ")", fieldInfo.Name.StartsWith("English"), Res.IsEnglish((string)fieldInfo.GetValue(null)));
			}
		}

		public void TestTemporarilySwitchLanguage()
		{
			const string key = "0274bace-f9ad-46fb-8e3e-4d13858ecdba";
			UInt16 asmid;
			unchecked
			{
				asmid = GetAsmid("Enterprise.ZArchitecture.GUI");
			}
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
			using (Res.TemporarilySwitchLanguage("DE-DE"))
			{
				AssertEquals("&Speichern", Res._GetData(asmid, key, "").Caption);
			}
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
		}

		public void TestUseMockData()
		{
			const string key = "0274bace-f9ad-46fb-8e3e-4d13858ecdba";
			UInt16 asmid;
			unchecked
			{
				asmid = GetAsmid("Enterprise.ZArchitecture.GUI");
			}
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(key, new ResourceStringData(key, "Test"));
				AssertEquals("Test", Res._GetData(asmid, key, "").Caption);

				IResourceStrings grmRes = Res.GetLanguageInstance("DE-DE");
				using (IMockResourceStringCache grmMockData = Res.GetLanguageInstance("DE-DE").UseMockData())
				{
					grmMockData.Put(key, new ResourceStringData(key, "Prufung"));
					AssertEquals("Test", Res._GetData(asmid, key, "").Caption);
					AssertEquals("Prufung", Res.GetLanguageInstance("DE-DE").GetData(asmid, key).Caption);
				}
			}
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
			AssertEquals("&Speichern", Res.GetLanguageInstance("DE-DE").GetData(asmid, key).Caption);
		}

		public static UInt16 GetAsmid(string assemblyName)
		{
			var assemblyIdAttribute = (ResourceStringAssemblyIdAttribute)Assembly.Load(assemblyName).GetCustomAttributes(typeof(ResourceStringAssemblyIdAttribute)).FirstOrDefault();
			AssertNotNull($"The assembly {assemblyName} does not have AssemblyId in its attributes", assemblyIdAttribute);
			AssertNotEquals(@"The assembly {assemblyName} has AssemblyId which is 0", 0, assemblyIdAttribute.Id);
			return assemblyIdAttribute.Id;
		}

		public void TestGetLanguageInstance()
		{
			const string key = "0274bace-f9ad-46fb-8e3e-4d13858ecdba";
			UInt16 asmid;
			unchecked
			{
				asmid = GetAsmid("Enterprise.ZArchitecture.GUI");
			}
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
			IResourceStrings res = Res.GetLanguageInstance("DE-DE");
			AssertEquals("&Speichern", res.GetData(asmid, key).Caption);
			AssertEquals("", Res._GetData(asmid, key, "").Caption);
			using (Res.TemporarilySwitchLanguage("ZH-CN"))
			{
				AssertEquals("&Speichern", res.GetData(asmid, key).Caption);
			}
			AssertSame(res, Res.GetLanguageInstance("DE-DE"));
		}
	}
}

