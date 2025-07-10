using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Common.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ObjectFactory = CargoWise.Application.ObjectFactory;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[SuppressClassNamesAreUniqueAcrossAssembliesMessage]
	[TestsSubclassesOf(typeof(ClientHook), ExcludePrivate = true)]
	public abstract class ClientOverrideTest : TransactionedTestCase
	{
		public async Task TestHelpWebPageExists()
		{
			var helpWebPage = ClientHookLoader.Instance.ClientHook.HelpWebPage;
			if (string.IsNullOrEmpty(helpWebPage))
			{
				Assert(true);
			}
			else
			{
				using (var httpClient = new HttpClient())
				{
					var response = await httpClient.GetAsync(helpWebPage);
					{
						AssertEquals("Should have read the web page successfully", HttpStatusCode.OK, response.StatusCode);
					}
				}
			}
		}

		public void TestNotPuttingClassesInWrongNamespace()
		{
			var ass = ClientOverrideType.Assembly;
			AssertEquals("Should be a client dll for this test", true, ClientHookLoader.Instance.IsClientOverrideAssembly(ass));
			foreach (var type in ass.GetTypes())
			{
				if (type.Namespace != null &&
					type.Namespace.StartsWith("Enterprise.") &&
					!type.Namespace.StartsWith("Enterprise.Client") &&
					type.Namespace != "Enterprise.ZArchitecture.Schema" &&
					type.Namespace != "Enterprise.ReflectionTest")
				{
					Fail("Type " + type.FullName + " is not in namespace Enterprise.Client. " +
						"Please put it there for naming convention purposes and also because it makes tests easy to see in the tree pane.");
				}
			}
		}

		#region DbSchemaUpgradeInfo

		public void TestClientDbObjectNamesStartWithClient()
		{
			var failedObjectNames = new ArrayList();

			if (
				ClientHookLoader.Instance.ClientHook.DbSchemaExtensionObjects != null
				&& ClientHookLoader.Instance.Client != Clients.EDI
			)
			{
				foreach (var script in ClientHookLoader.Instance.ClientHook.DbSchemaExtensionObjects.GetAllScripts())
				{
					if (IsInvalidDbObjectName(script))
					{
						failedObjectNames.Add(script.ObjectName);
					}
				}
			}

			ReportClientDbObjectNamesStartWithClientFailures(failedObjectNames);
		}

		bool IsInvalidDbObjectName(DatabaseObjectCreateScript script)
		{
			return script.IsObjectNameSpecified
				&& !script.ObjectName.StartsWith("Client")
				&& !script.ObjectName.StartsWith("vw_Client")
				&& !script.ObjectName.StartsWith("vw_Report_Client")
				&& !script.ObjectName.StartsWith("NR_RX_")
				&& script.ObjectName != "ViewGenericClientJob";
		}

		void ReportClientDbObjectNamesStartWithClientFailures(IList failedObjectNames)
		{
			var failReport = GetFailureReport(failedObjectNames, "The following client-specific database objects don't start with 'Client':");

			if (!string.IsNullOrEmpty(failReport))
			{
				Fail(failReport);
			}
			else
			{
				Assert("No failures reported", true);
			}
		}

		readonly Regex TableCreateRegex = new Regex(@"^\s*CREATE\s+(TABLE|((UNIQUE\s+)?(NON)?CLUSTERED\s+INDEX))\s+(dbo\.)?(?<ObjName>\w+)(\s+|\()", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		readonly Regex TableDropRegex = new Regex(@"^\s*DROP\s+(TABLE\s+|INDEX\s+\w+.)(dbo\.)?(?<ObjName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		[ExpectNoExceptions]
		public void TestClientTableAndIndexCollectionContainsOnlyTableScriptsAndNamesAreConsistent()
		{
			var extensionObjects = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>()?.ExtensionObjects;
			if (extensionObjects is null)
			{
				return;
			}

			AssertObjectCollectionIsConsistent(extensionObjects.TableCreationScripts, TableCreateRegex, TableDropRegex);
		}

		readonly Regex ViewAndRoutineDropRegex = new Regex(@"^\s*DROP\s+(FUNCTION|PROC(EDURE)?|VIEW|TRIGGER)\s+(dbo.)?(?<ObjName>\w+)(\s+|\(|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		[ExpectNoExceptions]
		public void TestClientViewAndRoutineCollectionContainsOnlyViewProcFunctionAndTriggerScriptsAndNamesAndTypesAreConsistent()
		{
			var extensionObjects = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>()?.ExtensionObjects;
			if (extensionObjects is null)
			{
				return;
			}

			AssertObjectCollectionIsConsistent(extensionObjects.ViewAndRoutineCreationScripts.CastArray<DatabaseObjectCreateScript>(), CreateScriptParser.CreateObjRegex, ViewAndRoutineDropRegex);
			AssertViewAndRoutineTypesAreConsistent(extensionObjects.ViewAndRoutineCreationScripts);
		}

		void AssertViewAndRoutineTypesAreConsistent(ImmutableArray<DatabaseViewAndRoutineCreateScript> clientViewsAndRoutines)
		{
			var invalidObjectTypeScripts = new List<string>();

			foreach (var script in clientViewsAndRoutines)
			{
				var typeFromScript = CreateScriptParser.GetObjectType(script.CreateScript);

				if (script.ObjectType != typeFromScript)
				{
					var text = String.Format("\r\n{0}\r\nDefined object type [{1}] != Script Object Type [{2}]", script.ObjectName, script.ObjectType, typeFromScript);
					invalidObjectTypeScripts.Add(text);
				}
			}

			var invalidObjectTypeScriptReport = GetFailureReport(invalidObjectTypeScripts, "The following script types are invalid:");

			AssertEquals("", invalidObjectTypeScriptReport);
		}

		void AssertObjectCollectionIsConsistent(ImmutableArray<DatabaseObjectCreateScript> clientObjects, Regex objectCreateRegex, Regex objectDropRegex)
		{
			var invalidCreateScripts = new List<string>();
			var invalidDropScripts = new List<string>();
			var inconsistentObjectNames = new List<string>();

			foreach (var script in clientObjects)
			{
				AddToObjectFailListsIfAplicable(objectCreateRegex, script.ObjectName, script.CreateScript, invalidCreateScripts, inconsistentObjectNames);
				AddToObjectFailListsIfAplicable(objectDropRegex, script.ObjectName, script.DropScript, invalidDropScripts, inconsistentObjectNames);
			}

			var invalidCreateScriptReport = GetFailureReport(invalidCreateScripts, "The following CREATE scripts are invalid:");
			var invalidDropScriptReport = GetFailureReport(invalidDropScripts, "The following DROP scripts are invalid:");
			var inconsistentObjectNameReport = GetFailureReport(inconsistentObjectNames, "The following object names and scripts are inconsistent:");

			AssertEquals("", invalidCreateScriptReport + invalidDropScriptReport + inconsistentObjectNameReport);
		}

		protected virtual void AddToObjectFailListsIfAplicable(Regex objectRegex, string objectName, string objectScript, List<string> invalidScrips, List<string> inconsistentObjectNames)
		{
			var scriptMatch = objectRegex.Match(objectScript);

			if (scriptMatch.Success)
			{
				var nameFromScript = scriptMatch.Groups["ObjName"].Value;

				if (nameFromScript != objectName)
				{
					var text = String.Format(
						"Object Name [{0}] != Name From Script [{1}]", objectName, nameFromScript);
					inconsistentObjectNames.Add(text);
				}
			}
			else
			{
				invalidScrips.Add("[" + objectName + "] - " + GetScriptStart(objectScript));
			}
		}

		string GetScriptStart(string fullScript)
		{
			var scriptStart = (fullScript.Length > 150) ? fullScript.Substring(0, 150) : fullScript;
			scriptStart = scriptStart.Trim().Replace("\r", "").Replace("\n", " ");
			return scriptStart;
		}

		string GetFailureReport(IList failList, string reportHeader)
		{
			var message = new StringWriter();

			if (failList.Count > 0)
			{
				message.WriteLine(reportHeader);

				foreach (string failMessage in failList)
				{
					message.WriteLine(failMessage);
				}

				message.WriteLine("");
			}

			return message.GetStringBuilder().ToString();
		}

		#endregion

		public void TestNewClientModules_ModuleClassesExist()
		{
			if (ClientHookLoader.Instance.ClientHook.NewClientModules != null)
			{
				foreach (var module in ClientHookLoader.Instance.ClientHook.NewClientModules)
				{
					var assembly = Assembly.Load(module.Info.AssemblyNameForTest);
					AssertNotNull("Assembly should exist", assembly);
					AssertNotNull("Module type should exist", assembly.GetType(module.Info.ClassFullNameForTest));
				}
			}
			else
			{
				Assert("No client modules to test", true);
			}
		}

		public void TestNewClientControllers_ControllerClassesExist()
		{
			if (ClientHookLoader.Instance.ClientHook.NewClientControllers != null)
			{
				foreach (var controller in ClientHookLoader.Instance.ClientHook.NewClientControllers)
				{
					var assembly = Assembly.Load(controller.AssemblyNameForTest);
					AssertNotNull("Assembly should exist", assembly);
					AssertNotNull("Controller type should exist", assembly.GetType(controller.ClassFullNameForTest));
				}
			}
			else
			{
				Assert("No client controllers to test", true);
			}
		}

		public void TestGetModuleOverridesAndGetControllerOverrides_ShouldBeThreadSafe()
		{
			CombineAssertions("Client hooks should not locally store module and controller overrides as it increases the risk for them to being not thread safe - these method should return new objects each time", () => {
				AssertNotEquals(ClientHookLoader.Instance.ClientHook.GetControllerOverrides_ExposedForTest(), ClientHookLoader.Instance.ClientHook.GetControllerOverrides_ExposedForTest());
				AssertNotEquals(ClientHookLoader.Instance.ClientHook.GetGetModuleOverrides_ExposedForTest(), ClientHookLoader.Instance.ClientHook.GetGetModuleOverrides_ExposedForTest());
			});
		}

		public void TestBusinessObjectsAreTypeDecidedCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var builder = new StringBuilder();

			IClientHookLoader loader = ClientHookLoader.Instance;
			if (loader.ClientHook.ClientTypeDeciders != null)
			{
				foreach (var keyValuePair in loader.ClientHook.ClientTypeDeciders)
				{
					var clientTypeDecider = keyValuePair.Value as TypeDecider;
					AssertNotNull("ClientTypeDecider is not of type " + typeof(TypeDecider).FullName, clientTypeDecider);
					var failureMessageForBizO = CheckClientTypeDecider(factory, keyValuePair.Key, clientTypeDecider);
					if (!failureMessageForBizO.IsEmpty)
					{
						builder.AppendLine(failureMessageForBizO);
					}
				}
			}

			if (builder.Length > 0)
			{
				builder.Insert(0, "\r\nThe following BusinessObjects are not type decided correctly:\r\n\r\n");
				Fail(builder.ToString().Trim());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAdditionalRegistryItemSet()
		{
			var itemSet = ClientHookLoader.Instance.ClientHook.AdditionalRegistryItemSet;

			if (itemSet == null)
			{
				Assert(true);
			}
			else
			{
				AssertEquals("AdditionalRegistryItemSet should return an instance that is a subclass of RegistryItemSet.", true, typeof(RegistryItemSet).IsAssignableFrom(itemSet.GetType()));
				var itemSetLocator = ObjectFactory.Get<IRegistryItemSetLocator>();
				AssertCollectionContains("RegistryItemSetLocator.GetRegistryItemSets() should contain the client-specific RegistryItemSet.", itemSet, itemSetLocator.GetRegistryItemSets());
				RegistryTester.AssertItemNamesAreUnique(itemSetLocator.GetAllRegistryItems());
			}
		}

		ZString CheckClientTypeDecider(BusinessObjectFactory factory, Type bizOType, TypeDecider clientTypeDecider)
		{
			var builder = new StringBuilder();

			var baseTypeDeciderForBizO = TypeDeciderWithExposedStaticsForTest.GetTypeDeciderFromType(bizOType);
			if (baseTypeDeciderForBizO != null)
			{
				if (baseTypeDeciderForBizO is CountrySpecificTypeDecider)
				{
					var countrySpecificTypeDecider = (CountrySpecificTypeDecider)baseTypeDeciderForBizO;
					foreach (var countrySpecificType in countrySpecificTypeDecider.CountrySpecificTypes)
					{
						builder.Append(CheckBizOTypeFromTypeDeciders(factory, bizOType, baseTypeDeciderForBizO, clientTypeDecider, countrySpecificType.CountryCode));
					}
				}
				else
				{
					builder.Append(CheckBizOTypeFromTypeDeciders(factory, bizOType, baseTypeDeciderForBizO, clientTypeDecider, ""));
				}
			}

			return builder.ToString();
		}

		ZString CheckBizOTypeFromTypeDeciders(BusinessObjectFactory factory, Type bizOType, TypeDecider baseTypeDecider, TypeDecider clientTypeDecider, ZString countryCode)
		{
			var bizO = factory.New(bizOType);
			var row = ((INeedRow)bizO).Row;

			var failureMessageBuilder = new StringBuilder();
			var countryCodeFailureMessage = "";

			if (!countryCode.IsEmpty)
			{
				StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(countryCode);
				countryCodeFailureMessage = " when the current country code is " + countryCode;
			}
			else
			{
				StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(OriginalCountryCode);
			}

			var typeForNew = clientTypeDecider.GetTypeForNew();
			var baseTypeForNew = baseTypeDecider.GetTypeForNew();
			var typeForBinding = clientTypeDecider.GetTypeForBinding();
			var baseTypeForBinding = baseTypeDecider.GetTypeForBinding();
			var typeForLoad = clientTypeDecider.GetTypeForLoad(row, factory);
			var baseTypeForLoad = baseTypeDecider.GetTypeForLoad(row, factory);

			if (!baseTypeForNew.IsAssignableFrom(typeForNew))
			{
				failureMessageBuilder.AppendFormat("GetTypeForNew(). BusinessObject type '{0}' should sub-class from '{1}'{2}\r\n", typeForNew.FullName, baseTypeForNew.FullName, countryCodeFailureMessage);
			}

			if (!baseTypeForBinding.IsAssignableFrom(typeForBinding))
			{
				failureMessageBuilder.AppendFormat("GetTypeForBinding(). BusinessObject type '{0}' should sub-class from '{1}'{2}\r\n", typeForBinding.FullName, baseTypeForBinding.FullName, countryCodeFailureMessage);
			}

			if (!baseTypeForNew.IsAssignableFrom(typeForNew))
			{
				failureMessageBuilder.AppendFormat("GetTypeForLoad(). BusinessObject type '{0}' should sub-class from '{1}'{2}\r\n", typeForLoad.FullName, baseTypeForLoad.FullName, countryCodeFailureMessage);
			}

			return failureMessageBuilder.ToString();
		}

		protected override void SetUp()
		{
			base.SetUp();

			OriginalCountryCode = StaticCurrentFetcher.Instance.CurrentCompany.Country.RN_Code;
		}

		protected override void TearDown()
		{
			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(OriginalCountryCode);

			base.TearDown();
		}

		protected abstract Type ClientOverrideType { get; }
		ZString OriginalCountryCode;

		#region TypeDeciderWithExposedStaticsForTest

		abstract class TypeDeciderWithExposedStaticsForTest : TypeDecider
		{
			public static new TypeDecider GetTypeDeciderFromType(Type type)
			{
				return TypeDecider.GetTypeDeciderFromType(type);
			}
		}

		#endregion
	}
}
