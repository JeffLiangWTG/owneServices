using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Billing.StlCollector.Retriever.Testing;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Billing.StlCollector.Retriever
{
	sealed class CustomStlCollectorFactoryTest : TestCaseWithFactory
	{
		public void TestCreateScripts()
		{
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			{
				var factory = new CustomStlCollectorFactory();
				var loadedScripts = factory.CreateScripts(Factory);

				AssertEquals("Were any scripts loaded?",
					true, loadedScripts.Any());
				AssertEquals("Are there any scripts which do not implement IStlItem?",
					false, loadedScripts.Any(s => !typeof(IStlItem).IsAssignableFrom(s.GetType())));
				AssertEquals("Are there any scripts with a blank code?",
					false, loadedScripts.Any(s => string.IsNullOrWhiteSpace(s.Code)));
				AssertEquals("Are there any scripts where StlGrain = Daily?",
					true, loadedScripts.Any(s => s.StlGrain == StlDataGrain.Daily));

				foreach (var item in ScriptsLoadedViaReflection)
				{
					var matchingItem = loadedScripts.SingleOrDefault(i => i.Code.Equals(item.Code, StringComparison.InvariantCultureIgnoreCase));
					AssertNotNull(item.Code, matchingItem);
					AssertEquals("Type should match", item.GetType(), matchingItem.GetType());
				}
				AssertEquals("Total number of scripts", ScriptsLoadedViaReflection.Length, loadedScripts.Count());
			}
		}

		static readonly Lazy<IStlItem[]> scriptsLoadedViaReflection = new Lazy<IStlItem[]>(() =>
		{
			var dynamicBaseType = typeof(RefStlScriptRetriever);
			var scriptBaseType = typeof(BaseStlScript);
			var scriptBaseAssembly = scriptBaseType.Assembly;
			string scriptBaseNamespace = scriptBaseType.Namespace;

			var query =
				from t in scriptBaseAssembly.GetTypes()
				where
					t.IsClass
					&& !t.IsAbstract
					&& !t.IsNestedPrivate
					&& t.GetInterface(nameof(IStlItem)) != null
					&& !t.IsSubclassOf(dynamicBaseType)
					&& t.Namespace.StartsWith(scriptBaseNamespace)
					&& t.Namespace != scriptBaseNamespace
				select t;

			return query.AsEnumerable().Select(scriptType => (IStlItem)Activator.CreateInstance(scriptType)).ToArray();
		});

		public static IStlItem[] ScriptsLoadedViaReflection => scriptsLoadedViaReflection.Value;

		public void TestCreateScriptsActiveOnly()
		{
			var testList = new ListObject();
			testList.Add(new DummyScriptItem(isActive: true, "ACTIVE"));
			testList.Add(new DummyScriptItem(isActive: false, "INACTIVE"));

			using (ObjectFactory.Substitute("StlCustomCollectorsList", testList))
			{
				var factory = new CustomStlCollectorFactory();
				var loadedScripts = factory.CreateScripts(Factory);
				Assert("Active script should be loaded", loadedScripts.Any(s => s.Code.Equals("ACTIVE")));
				Assert("Inactive script should not be loaded", !loadedScripts.Any(s => s.Code.Equals("INACTIVE")));
			}
		}

		/// <summary>
		/// Every new price item (here represented by its collecting script) needs to be added to:
		///   - [ediprod.db.corporate.cargowise.com].[ediProd].[dbo].[ClientLicencePriceItem]
		///     -- WHERE L7_L6 = (SELECT L6_PK FROM dbo.ClientLicencePriceHeader WHERE L6_PricelistVersion = 'STL')
		///   - [billing.db.corporate.cargowise.com].[billing].[analysis].[PriceItem]
		/// </summary>
		public void TestFeatureCodesAreUnique()
		{
			var allScripts = new CustomStlCollectorFactory().CreateScripts(Factory);

			var duplicateCodeScripts =
				from s in allScripts
				join dup in (
					from s in allScripts
					group s by s.Code into grp
					where grp.Count() > 1
					select new { Code = grp.Key })
				on s.Code equals dup.Code
				orderby s.Code, s.Feature
				select "    [" + s.Code + "] " + s.Feature;

			Assert(
				"The following scripts have duplicated feature codes:\r\n\r\n" + string.Join("\r\n", duplicateCodeScripts) + "\r\n",
				!duplicateCodeScripts.Any());
		}

		/// <summary>
		/// AI3 = 3rd Party Accounting Interface
		/// </summary>
		public void TestOldFeatureCodesNotReused()
		{
			var allScripts = new CustomStlCollectorFactory().CreateScripts(Factory);

			var oldCodeList = new string[]
			{
				"AI3",
				"ACE",
				"ACI",
				"ACT",
				"CFS",
				"ECI",
				"ICB",
				"ICG",
				"ORD",
				"PRO",
				"REC",
				"WOR",
			};

			var reusedCodeScripts =
				from s in allScripts
				where oldCodeList.Contains(s.Code)
				orderby s.Code, s.Feature
				select "    [" + s.Code + "] " + s.Feature;

			Assert(
				"The following scripts are reusing old feature codes:\r\n\r\n" + string.Join("\r\n", reusedCodeScripts) + "\r\n",
				!reusedCodeScripts.Any());
		}

		/// <summary>
		/// STL = STL Milestone mock price item.
		/// XXX, TRN, MON, MCO = Reserved for tests.
		/// </summary>
		public void TestReservedCodesNotUsed()
		{
			var allScripts = new CustomStlCollectorFactory().CreateScripts(Factory);
			var reservedCodeList = new string[] { "STL", "XXX", "TRN", "MON", "MCO" };

			var reusedCodeScripts =
				from s in allScripts
				where reservedCodeList.Contains(s.Code)
				orderby s.Code, s.Feature
				select "    [" + s.Code + "] " + s.Feature;

			Assert(
				"The following scripts are using reserved codes:\r\n\r\n" + string.Join("\r\n", reusedCodeScripts) + "\r\n",
				!reusedCodeScripts.Any());
		}

		public void TestFeatureNamesAreUnique()
		{
			var allScripts = new CustomStlCollectorFactory().CreateScripts(Factory);

			var duplicateFeatureNameScripts =
				from s in allScripts
				join dup in
					(
						from s in allScripts
						group s by s.Feature into grp
						where grp.Count() > 1
						select new { Feature = grp.Key })
				on s.Feature equals dup.Feature
				orderby s.Feature, s.Code
				select "    " + s.Feature + " [" + s.Code + "]";

			Assert(
				"The following scripts have duplicated feature names:\r\n\r\n" + string.Join("\r\n", duplicateFeatureNameScripts) + "\r\n",
				!duplicateFeatureNameScripts.Any());
		}

		public void TestAllStlCollectionScriptsAreInTheCorrectNamespace()
		{
			var scriptBaseType = typeof(BaseStlScript);
			var scriptBaseAssembly = scriptBaseType.Assembly;
			string scriptBaseNamespace = scriptBaseType.Namespace;

			var scriptsInDifferentNamespaceTree =
				from t in scriptBaseAssembly.GetTypes()
				where
					t.IsClass
					&& !t.IsAbstract
					&& t.IsSubclassOf(scriptBaseType)
					&& !t.Namespace.StartsWith(scriptBaseNamespace)
				select t.FullName;

			Assert(
				"The following concrete BaseStlScript sub-classes are in a namespace which is not under [" + scriptBaseNamespace + "]:\r\n\r\n" + string.Join("\r\n", scriptsInDifferentNamespaceTree) + "\r\n",
				!scriptsInDifferentNamespaceTree.Any());

			var scriptsInBaseNamespace =
				from t in scriptBaseAssembly.GetTypes()
				where
					t.IsClass
					&& !t.IsAbstract
					&& t.IsSubclassOf(scriptBaseType)
					&& t.Namespace == scriptBaseNamespace
					&& !t.FullName.Contains("Enterprise.Billing.StlCollector.Retriever.Scripts.StlScriptTests")
					&& t.FullName != "Enterprise.Billing.StlCollector.Retriever.Scripts.RefStlScriptRetriever"
				select t.FullName;

			Assert(
				"The following concrete BaseStlScript sub-classes are in the base namespace:\r\n\r\n" + string.Join("\r\n", scriptsInBaseNamespace) + "\r\n",
				!scriptsInBaseNamespace.Any());
		}
	}
}
