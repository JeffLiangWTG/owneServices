// Db.Connection referenced explicitly as running in a new AppDomain causes a new Db.Connection to be used.
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ReflectionTest.Utilities;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Resources;
using WTG.DevTools.Definitions;

using AssemblyRetriever = Enterprise.ZArchitecture.Core.Testing.AssemblyRetriever;
using BuildXml = CargoWise.BuildTools.BuildXml;

namespace Enterprise.ReflectionTest
{
	[DoNotAddToTestTree]
	public class ReflectionTestHelper : TransactionedTestCase
	{
		static readonly Regex REX_ClientDll = new Regex
		(
			//starts with ZClient or ZClientWeb case insensitive
			//then has upper letter then 2 upper letters or digits
			//then dot(.) or comma(,) or end of string
			@"^(?i:(ZClient|ZClientWeb))(\p{Lu}(\p{Lu}|\d){2})(\.|,|$)",
			RegexOptions.ExplicitCapture | RegexOptions.Compiled
		);
		static readonly Regex REX_TestClass = new Regex
		(
			//contains .Test or +Test
			@"(\.|\+)Test",
			RegexOptions.ExplicitCapture | RegexOptions.Compiled
		);

		public const string NetCoreTargetFrameworkPropertyName = "CWNetCoreTargetFramework";
		public const string NetCoreWindowsTargetFrameworkPropertyName = "CWNetCoreWindowsTargetFramework";

		#region Metadata Tests

		public void TestModuleIDSpecifiedExists()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObjectCollection));

			foreach (Type type in retriever.Retrieve())
			{
				using (OverrideClientAssembly(type.Assembly))
				{
					ModuleIDAttribute attribute = ReflectionUtil.GetAttribute<ModuleIDAttribute>(type);

					if (attribute == null)
					{
						continue;
					}

					bool found = ModuleIDs.AllExcludingClientModules.Any(moduleID => moduleID.Name == attribute.ModuleIdName);
					if (!found)
					{
						ClientHook clientHook = ClientHookLoader.Instance.ClientHook;
						if (clientHook != null)
						{
							found = Array.Exists(clientHook.NewClientModules, module => module.ID.Name == attribute.ModuleIdName);
						}
					}

					Assert(string.Format("{0} has wrong ModuleID attribute. ModuleID with name {1} doesn't exist", type.FullName, attribute.ModuleIdName), found);
				}
			}
		}

		#endregion

		#region Coding Rules

		public void TestAssembliesStrongNamed()
		{
			new CommonAssemblyInfoTest().TestAssembliesStrongNamed(CommonAssemblyEnabledAssemblies);
		}

		protected string[] AssembliesExceptFlexCel
		{
			get
			{
				List<string> result = new List<string>();
				foreach (string assemblyName in Assemblies)
				{
					if (assemblyName != "FlexCel")
					{
						result.Add(assemblyName);
					}
				}
				return result.ToArray();
			}
		}

		protected string[] AssembliesExceptNUnitFlexCelAndGenerated
		{
			get
			{
				List<string> result = new List<string>();
				foreach (string assemblyName in Assemblies)
				{
					if (assemblyName != "NUnitCore"
						&& !assemblyName.StartsWith("FlexCel")
						&& !assemblyName.StartsWith("XmlDiffPatch")
						&& !assemblyName.StartsWith("VsnetUrl")
						&& !assemblyName.EndsWith(".XmlSerializers"))
					{
						result.Add(assemblyName);
					}
				}
				return result.ToArray();
			}
		}

		// System.IO.FileNotFoundException: Could not load file or assembly 'Microsoft.WindowsCE.Forms, Version=3.5.0.0
		string[] MicrosoftWindowsCEFormsAssemblies
		{
			get
			{
				return new[]
				{
					"Enterprise.LocalTransport.Mobile.Client",
					"Enterprise.LocalTransport.Mobile.GUI",
					"Enterprise.LocalTransport.Mobile.Client.Business",
					"Enterprise.LocalTransport.Mobile.Client.Data"
				};
			}
		}

		protected string[] CommonAssemblyEnabledAssemblies =>
			AssembliesExceptNUnitFlexCelAndGenerated
				.Where(assemblyName => !IsNotReflectionTestable(assemblyName))
				.ToArray();

		public void TestIntegrationAssembliesContainNoCode()
		{
			List<string> errors = new List<string>();
			foreach (string assemblyName in AssembliesExceptNUnitFlexCelAndGenerated)
			{
				if (IsIntegrationAssembly(assemblyName) && !IsAssemblyInWhiteList(assemblyName))
				{
					Assembly assembly = Assembly.Load(assemblyName);
					foreach (Type type in assembly.GetTypes())
					{
						string typeNameWithoutGenericSuffix;
						if (type.IsGenericType && type.Name.IndexOf('`') >= 0)
						{
							typeNameWithoutGenericSuffix = type.Name.Substring(0, type.Name.IndexOf('`'));
						}
						else
						{
							typeNameWithoutGenericSuffix = type.Name;
						}

						if (type.IsClass &&
							!typeof(Delegate).IsAssignableFrom(type) &&
							!typeof(Exception).IsAssignableFrom(type) &&
							!typeof(EventArgs).IsAssignableFrom(type) &&
							!typeof(Attribute).IsAssignableFrom(type) &&
							!typeof(CodeDescriptionPairList).IsAssignableFrom(type) &&
							!typeof(TestCase).IsAssignableFrom(type) &&
							!(type.DeclaringType != null && typeof(CodeDescriptionPairList).IsAssignableFrom(type.DeclaringType)) &&
							!IsTypeStatic(type) &&
							!IsCodeContractType(type) &&
							type.Name != "CommonAssemblyInfo" &&
							!typeNameWithoutGenericSuffix.EndsWith("Test") &&
							!type.Name.StartsWith("Dummy") &&
							!type.Name.StartsWith("Mock") &&
							type.Name != "KeyGroup" &&
							type.Name != "TestResourceStringData" &&
							!type.FullName.EndsWith("Properties.Resources") &&

							// For when an otherwise accepted class uses an anonymous method accessing local fields
							//(the compiler generates a new type).
							Attribute.GetCustomAttribute(type, typeof(CompilerGeneratedAttribute)) == null &&
							!IsTypeInWhiteList(type))
						{
							errors.Add("Only interfaces / structures permitted in integration assembly " + assemblyName + " (" + type.FullName + ")");
						}
					}
				}
			}
			AssertEquals("\r\n" + string.Join("\r\n", errors.ToArray()), 0, errors.Count);
		}

		bool IsTypeInWhiteList(Type type)
		{
			return
				// Warehouse integration classes need to be moved (DO NOT ADD TO THIS LIST)
				type.Name == "AdditionalReference" ||
				type.Name == "BondedWarehouseLineProblemProvider" ||
				type.Name == "BondedWarehouseLink" ||
				type.Name == "BondedWarehouseLinkCreator" ||
				type.Name == "BondedWarehouseTransactionLineCollection" ||
				type.Name == "NotificationCollection" ||
				type.Name == "WarehouseTransactionLineCollection" ||

				// Accounting integration classes need to be moved (DO NOT ADD TO THIS LIST)
				type.Name == "AutoPostingNotification" ||
				type.Name == "AutoRatingProxy" ||
				type.Name == "CustomsCharge" ||
				type.Name == "ConsolBatchPostingDirectorCreator" ||
				type.Name == "JobBatchPostingDirectorCreator" ||
				type.Name == "CommonUtils" ||
				type.Name == "CustomsDisbursementChargePosterCreator" ||
				type.Name == "JobProfitLossCollection" ||
				type.Name == "ProfitLossCollectionBase" ||
				type.Name == "ProfitLossDetail" ||
				type.Name == "ProfitLossSummaryCollectionBase" ||
				type.Name == "ProfitLossSummaryDetail" ||
				type.Name == "ProfitLossDetailView" ||
				type.Name == "ProfitLossDetailCollectionBaseView" ||
				type.Name == "ProfitLossSummaryDetailView" ||
				type.Name == "ProfitLossSummaryCollectionBaseView" ||
				type.FullName == "Enterprise.Accounting.Integration.ProfitLossDetailView+Schema" ||
				type.FullName == "Enterprise.Accounting.Integration.ProfitLossSummaryDetailView+Schema" ||
				type.Name == "SupportExRateSourceAttribute" ||
				type.Name == "MoneyType" ||
				type.Name == "ServiceLevelRatingInformation" ||
				type.Name == "PackageInformation" ||
				type.Name == "AutoRatingReplacementCriteria" ||
				type.Name == "AutoRatingStatusInfo" ||
				type.Name == "EntryInfo" ||
				type.Name == "EntryInfoCollection" ||
				type.Name == "InvoiceInfo" ||
				type.Name == "InvoiceInfoCollection" ||
				type.Name == "ChargeCodeGroupCollection" ||
				type.Name == "ProductAttributesMeasure" ||
				type.Name == "ProductWithAttributes" ||
				type.Name == "LocationMeasure" ||
				type.Name == "MeasureInfo" ||
				type.Name == "JobValues" ||
				type.Name == "ContainerInfo" ||
				type.Name == "MeasureInfoWithDimensions" ||
				type.Name == "Point" ||
				type.Name == "MeasureInfoCollection" ||
				type.Name == "TimeInfo" ||
				type.Name == "TestCurrencyConverter" ||
				type.Name == "MoneyTypeTest" ||
				type.FullName.Contains("MeasureInfoWithDimensions") ||
				type.Name == "CommonUtilsTests" ||
				type.Name == "AutoRatingProxyBase" ||
				type.Name == "ICustomsChargesComparer" ||
				type.FullName.Contains("BaseBulkJobProfitPrintingModuleTest") ||

				// Warehouse integration classes need to be moved (DO NOT ADD TO THIS LIST)
				type.Name == "BondIDs" ||
				type.FullName.Contains("TestIWarehouseTransactionLine") ||
				type.FullName.Contains("SortLinesByQuantity") ||
				type.FullName.Contains("TestIBondedWarehouseTransactionLine") ||

				// Freight integration classes need to be moved
				type.FullName.Contains("ShipmentStatusList") ||

				// ArchiveManager: these are tiny classes and attributes to share, and better as classes than structs.
				type.FullName.Contains("ArchiveDocumentDescriptor") ||
				type.FullName.Contains("ArchiveSystemDescriptorProviderAttribute") ||
				type.FullName.Contains("ReferenceKeyType") ||
				type.FullName.Contains("ArchiveReferenceKey") ||
				type.FullName.Contains("ArchiveImageDescriptor") ||

				// DistanceCalculation: these are tiny classes and attributes to share, and better as classes than structs.
				type.FullName.Contains("DistanceCalculationAddress") ||
				type.FullName.Contains("DistanceCalculationConfiguration") ||
				type.FullName.Contains("DistanceCalculationResult") ||

				// Rating.Integration: these are tiny classes and attributes to share, and better as classes than structs.
				type.FullName.Contains("CalculationLog") ||
				type.FullName.Contains("CalculationStep") ||
				type.FullName.Contains("SerializationHelper") ||
				type.FullName.Contains("QuantityUnit") ||
				// helper class to make the AutoRatingProxyBase easier to maintain
				type.FullName.Equals("Enterprise.Accounting.Integration.AutoRatingProxyBase+Prop`1") ||

				// UniversalDataBuss.Integration: these are tiny classes with properties only on them, better as classes than structs.
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.TypeWithDescription") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.KeyValuePair") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.WorkflowInfo") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.EnterpriseServerAndCompanyID") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.UniversalObjectFactory") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.UniversalObjectFactory+RowFactoryExposingBusinessObjectFactory") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.UniversalObjectFactory+NotificationHandler") ||
				type.FullName.Equals("Enterprise.UniversalDataBuss.Integration.UniversalXmlSchema") ||

				// MasterFiles.Integration: these are tiny classes with properties only on them, better as classes than structs.
				type.FullName.Equals("Enterprise.MasterFiles.Integration.PropertyValue") ||

				// ComplianceRisk.Integration: these are tiny classes with properties only on them, better as classes than structs.
				type.FullName.Equals("Enterprise.ComplianceRisk.Integration.ComplianceAssessmentPointPairInfo") ||
				type.FullName.Equals("Enterprise.ComplianceRisk.Integration.ComplianceCheckRequestPointPair") ||
				type.FullName.Equals("Enterprise.ComplianceRisk.Integration.ComplianceCheckRequestPointPairLocation") ||

				// Messaging.Integration: couldn't find better place for billing classes.
				type.FullName.Equals("Enterprise.Billing.Integration.SnapshotAggregation") ||
				type.FullName.Equals("Enterprise.Billing.Integration.SourceInfo") ||
				type.FullName.Equals("Enterprise.Billing.Integration.FactorySnapshot") ||
				type.FullName.Equals("Enterprise.Billing.Integration.ObjectSnapshot") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingDataSource") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingInterfaceName") ||
				type.FullName.Equals("Enterprise.Billing.Integration.Test.FactoryBillingExtensionsTestCase") ||
				type.FullName.Equals("Enterprise.URLHandler.UrlAuthenticationMutex+DisposableAction") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingDataEncryptor") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingManager") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingManager+XmlTextWriter") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingManager+ReferenceFieldLengthRestriction") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingTransaction") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingTransactionWrapper") ||
				type.FullName.Equals("Enterprise.Billing.Integration.BillingTransaction+BillingTransactionEqualityComparer") ||
				type.FullName.Equals("Enterprise.Billing.Integration.OldBillingTransactions.NoVersion.BillingTransaction") ||
				type.FullName.Equals("Enterprise.Billing.Integration.OldBillingTransactions.OneTwo.BillingTransaction") ||
				type.FullName.Equals("Enterprise.Billing.Integration.UsageTransaction") ||

				// ContractManagement: Need constant strings that are used as search filter strings.
				type.FullName.Contains("CarrierContractFilterConstants") ||
				type.FullName.Contains("AllocationRouteFilterConstants") ||
				type.FullName.Contains("NamedAccountFilterConstants") ||

				// eManifest.Integration: need common place for unique per factory semaphore used in eManifest and TransportBookings
				type.FullName.Equals("Enterprise.eManifest.Integration.CreatingSupplierBookingLineBizOsPreventer+CreatingSupplierBookingLineBizOsSemaphore") ||

				// ServiceManager: Integration projects contain implementations of abstractions
				type.FullName.StartsWith("ServiceManager.Integration") ||
				type.FullName.StartsWith("Enterprise.ServiceManager.Shared") ||

				// DocumentVisualizer; event data classes
				type.FullName.Contains("DocumentVisualizer") ||

				// DocumentScanning.Integration: these are tiny classes and attributes to share, and better as classes than structs.
				type.FullName.Equals("Enterprise.DocumentScanning.Integration.EDocImageData") ||
				type.FullName.Contains("Enterprise.DocumentScanning.Integration.ShipamaxParseResult") ||
				type.FullName.Contains("Enterprise.DocumentScanning.Integration.ShipamaxEDocsChange") ||

				// Dash.Integration: these are tiny classes and attributes to share, and better as classes than structs.
				type.FullName.Equals("Enterprise.Dash.Integration.DashEDocsDetails");
		}

		bool IsAssemblyInWhiteList(string assemblyName)
		{
			// ServiceManager: Integration projects contain implementations of abstractions
			return assemblyName.StartsWith("ServiceManager.Integration") && !assemblyName.Equals("ServiceManager.Integration.Abstractions");
		}

		bool IsIntegrationAssembly(string assemblyName)
		{
			return (assemblyName.Contains(".Integration.") || assemblyName.EndsWith(".Integration"))
				&& !assemblyName.EndsWith(".Test")
				&& !assemblyName.EndsWith(".Testing");
		}

		static bool IsTypeStatic(Type type)
		{
			return type.IsAbstract && type.IsSealed;
		}

		static bool IsCodeContractType(Type type)
		{
			// We can't check for the ContractClassForAttribute since Type.GetCustomAttributes won't find it
			return type.IsAbstract && type.Name.IndexOf("Contract", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public void TestNoPublicWeaklyTypedCollections()
		{
			SubClassRetriever retriever = new SubClassRetriever(AssembliesExceptFlexCel.Except(MicrosoftWindowsCEFormsAssemblies).ToArray(), typeof(object));
			foreach (Type type in retriever.Retrieve())
			{
				foreach (string error in GetPublicWeaklyTypedCollections(type))
				{
					AddError(error);
				}
			}

			ReportError("The following members have collections of type Object. Consider using a more specific type. Use CargoWise.Common.SuppressWeaklyTypedCollectionMessageAttribute to exclude the member from this test.");
		}

		public void TestSetterDoesNotExistOnTypedIndexersOnBusinessObjectCollections()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObjectCollection));
			retriever.IncludeClientDlls = false;
			foreach (Type type in retriever.Retrieve())
			{
				foreach (PropertyInfo prop in type.GetProperties())
				{
					if (prop.Name == "Item" && prop.CanWrite)
					{
						AddError(type.FullName);
					}
				}
			}

			ReportError("The following classes implement setters on the typed indexer");
		}

		public void TestOverriddenAddNewExistsOnBusinessObjectCollections()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObjectCollection));
			retriever.IncludeClientDlls = false;
			retriever.ExcludedTypesAndTheirDescendants = new Type[] { typeof(DocumentWrapperCollection) };
			retriever.ExcludedAttributes = new[] { typeof(ExcludeFromOverriddenAddNewTest) };
			foreach (Type type in retriever.Retrieve())
			{
				Type itemType = GetMostExplicitImplementationForType(type, "Item");
				if (itemType != null && !itemType.IsAbstract)
				{
					Type addNewType = GetMostExplicitImplementationForMethod(type, "AddNew");
					if (addNewType != itemType)
					{
						AddError(type.FullName + ' ' + addNewType.Name + ' ' + itemType.Name);
					}
				}
			}
			ReportError("These collections do not have matched AddNew and typed indexers. They should both return the same type.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1081:DoNotUseSubClassOfTypeofBusinessObjectCollection", Justification = "Testing")]
		public void TestSetterDoesNotExistOnCollectionsHostedByBusinessObject()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
			retriever.IncludeClientDlls = false;
			foreach (Type type in retriever.Retrieve())
			{
				foreach (PropertyInfo info in type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
				{
					if (info.PropertyType.IsSubclassOf(typeof(BusinessObjectCollection)) || info.PropertyType == typeof(BusinessObjectCollection))
					{
						bool isExcluded = (info.GetCustomAttributes(typeof(BusinessObjectTestExclude), false).Length > 0);

						if (!isExcluded && info.CanWrite)
						{
							AddError(type.FullName + " " + info.Name);
						}
					}
				}
			}

			ReportError("These properties implement setters on collections, which is generally not advised.");
		}

		public void TestDetectStaticBusinessObjectsCollectionsAndFactories()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies.Except(MicrosoftWindowsCEFormsAssemblies).ToArray(), typeof(object));
			retriever.IncludeClientDlls = false;
			foreach (Type type in retriever.Retrieve())
			{
				foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					Type fieldType = fieldInfo.FieldType;
					if (IsDisallowedType(fieldInfo, fieldType))
					{
						AddError(type.FullName + ": " + fieldType.Name + " " + fieldInfo.Name);
					}
				}

				foreach (PropertyInfo info in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (IsDisallowedType(info, info.PropertyType))
					{
						AddError(type.FullName + ": " + info.PropertyType.Name + " " + info.Name);
					}
				}
			}

			ReportError("There should be no requirement to keep static factories, business objects or collections.");
		}

		public void TestCustomsSolutionsDoNotExposeBaseLevelProperties()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
			foreach (Type type in retriever.Retrieve())
			{
				if (NamespaceIsCustomsCountrySpecific(type.Namespace))
				{
					if (!type.IsAbstract)
					{
						foreach (FieldInfo fieldInfo in type.GetFields())
						{
							if (IsSuitableTypeForCustomsSolutionsDoNotExposeBaseLevelProperties(fieldInfo))
							{
								AddError(type.FullName + ": " + fieldInfo.Name);
							}
						}
						foreach (PropertyInfo propertyInfo in type.GetProperties())
						{
							if (IsSuitableTypeForCustomsSolutionsDoNotExposeBaseLevelProperties(propertyInfo))
							{
								AddError(type.FullName + ": " + propertyInfo.Name);
							}
						}
					}
				}
			}
			ReportError("All properties and fields exposed in specific country dll's should reintroduce each property/field as a concrete country specific version.");
		}

		bool IsSuitableTypeForCustomsSolutionsDoNotExposeBaseLevelProperties(MemberInfo info)
		{
			var type = info is PropertyInfo propertyInfo ? propertyInfo.PropertyType : info is FieldInfo fieldInfo ? fieldInfo.FieldType : null;
			return type != null &&
				!PropertiesWithBaseLevelTypeToIgnore.Contains(info.DeclaringType.FullName + "." + info.Name) &&
				NamespaceIsCustomsCountrySpecific(info.DeclaringType.Namespace) &&
				type.Namespace == "Enterprise.Customs.Business" &&
				typeof(BusinessObject).IsAssignableFrom(type);
		}

		bool NamespaceIsCustomsCountrySpecific(string @namespace)
		{
			return @namespace != null && @namespace.StartsWith("Enterprise.Customs") && @namespace.EndsWith(".Business") && @namespace != "Enterprise.Customs.Business";
		}

		// This is used for those business objects do not have country level types.
		HashSet<string> PropertiesWithBaseLevelTypeToIgnore
		{
			get
			{
				if (propertiesWithBaseLevelTypeToIgnore == null)
				{
					propertiesWithBaseLevelTypeToIgnore = new HashSet<string>();
					propertiesWithBaseLevelTypeToIgnore.Add("Enterprise.Customs.EU.Business.CusAuthorizationUsage.RelatedAuthorisationHeader");
					propertiesWithBaseLevelTypeToIgnore.Add("Enterprise.Customs.EU.Business.CusAuthorizationUsage.RelatedAuthorisationHeaderIgnoringReferenceNumber");
				}

				return propertiesWithBaseLevelTypeToIgnore;
			}
		}

		HashSet<string> propertiesWithBaseLevelTypeToIgnore;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1081:DoNotUseSubClassOfTypeofBusinessObjectCollection", Justification = "Testing")]
		bool IsDisallowedType(MemberInfo member, Type type)
		{
			bool isExcluded = (member.GetCustomAttributes(typeof(TestExcludeDetectStaticBusinessObjectsCollectionsAndFactoriesAttribute), false).Length == 0)
				|| (member.GetCustomAttributes(typeof(TestExcludeDetectStaticControlsAttribute), false).Length == 0);

			bool isDisallowedType = type.IsSubclassOf(typeof(BusinessObjectCollection))
				|| type == typeof(BusinessObjectCollection)
				|| type.IsSubclassOf(typeof(BusinessObject))
				|| type == typeof(BusinessObjectFactory)
				|| type.IsSubclassOf(typeof(Control))
				|| type == typeof(Control);

			return isDisallowedType && !isExcluded;
		}

		//public void TestTestClassesAreNotNested()
		//{
		//  int nested = 0;
		//  int nonNested = 0;
		//  SubClassRetriever Retriever = new SubClassRetriever(Assemblies, typeof(TestCase));
		//  Retriever.IncludeNestedClasses = true;
		//  Retriever.IncludePrivateNestedClasses = true;
		//  Retriever.IncludeTestClasses = true;
		//  foreach (Type type in Retriever.Retrieve())
		//  {
		//    if (type.IsNested)
		//    {
		//      AddError(type.FullName);
		//      nested++;
		//    }
		//    else
		//    {
		//      nonNested++;
		//    }
		//  }
		//  ReportError("There are " + nested.ToString() + " nested classes, and " + nonNested.ToString() + " non-nested classes. The test classes listed are nested. It is now black letter law that this must not happen.");
		//}

		public virtual void TestZWinFormHasTypedConstructor()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(ZForm));
			retriever.IncludeClientDlls = false;
			retriever.IncludeAbstractClasses = false;
			retriever.ExcludedAttributes = new[] { typeof(TestExcludeZWinFormHasTypedConstructorAttribute) };
			foreach (Type type in retriever.Retrieve())
			{
				using (OverrideClientAssembly(type.Assembly))
				{
					foreach (ConstructorInfo constructorInfo in type.GetConstructors())
					{
						foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
						{
							if (parameterInfo.ParameterType == typeof(IBusiness)
								|| parameterInfo.ParameterType == typeof(BusinessObject)
								|| parameterInfo.ParameterType == typeof(BusinessObjectCollection))
							{
								AddError(type.FullName);
								break;
							}
						}
					}
				}
			}

			ReportError("These forms have a bad constructor.  To improve readability and accuracy of the code change the constructor to take the correct BusinessObject or BusinessObjectCollection DESCENDANT, not the generic BusinessObject, IBusiness or BusinessObjectCollection. e.g. Make your form take a JobShipment or OrgHeader, not a plain vanilla BusinessObject");
		}

		public void TestControllersUsingSameIDAllDescendFromCommonBase()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(ZController));
			retriever.IncludeClientDlls = false;
			retriever.IncludeAbstractClasses = false;
			Hashtable iDs = new Hashtable();
			var errors = new ZStringBuilder();
			foreach (Type type in retriever.Retrieve())
			{
				using (OverrideClientAssembly(type.Assembly))
				{
					if (type.GetProperty("ID").DeclaringType == type)   // Has declared ID
					{
						ZController controller = (ZController)Activator.CreateInstance(type);
						if (iDs.Contains(controller.ID))
						{
							Type initialType = (Type)iDs[controller.ID];
							errors.Append(type.FullName + " & " + initialType.FullName + NewLineForHtml);
						}
						else
						{
							iDs.Add(controller.ID, type);
						}
					}
				}
			}

			if (errors.Length == 0)
			{
				Assert(true);
			}
			else
			{
				Fail("Duplicate definition of Controller ID" + NewLineForHtml + errors.ToStringWithDelimiterBetweenAppends(NewLineForHtml));
			}
		}

		public void TestModulesUsingSameIDAllDescendFromCommonBase()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(ZModule));
			retriever.IncludeClientDlls = false;
			retriever.IncludeAbstractClasses = false;
			Hashtable iDs = new Hashtable();
			var errors = new ZStringBuilder();
			foreach (Type type in retriever.Retrieve())
			{
				using (OverrideClientAssembly(type.Assembly))
				{
					if (type.GetProperty("ID").DeclaringType == type)   // Has declared ID in this class, not a subclass
					{
						using (ZModule module = GetNewModuleForType(type))
						{
							if (iDs.Contains(module.ID))
							{
								Type initialType = (Type)iDs[module.ID];
								errors.Append(type.FullName + " & " + initialType.FullName + NewLineForHtml);
							}
							else
							{
								iDs.Add(module.ID, type);
							}
						}
					}
				}
			}

			if (errors.Length == 0)
			{
				Assert(true);
			}
			else
			{
				Fail("Duplicate definition of Module ID (have you inherited a module " +
					"and expressly re-defined the child class's module ID to be the same " +
					"as its parent's ID? Remove the override.)" + NewLineForHtml + NewLineForHtml + errors.ToStringWithDelimiterBetweenAppends(NewLineForHtml));
			}
		}

		ZModule GetNewModuleForType(Type type)
		{
			if (typeof(ZArchitecture.Web.Modules.ZFilterGridModule).IsAssignableFrom(type))
			{
				return (ZModule)Activator.CreateInstance(type, new object[] { new BusinessObjectFactory(), new ZArchitecture.Web.GUI.WebControls.ZPage() });
			}
			else if (typeof(ZArchitecture.Web.Modules.ZWebModule).IsAssignableFrom(type))
			{
				return (ZModule)Activator.CreateInstance(type, new object[] { new BusinessObjectFactory() });
			}
			else
			{
				return (ZModule)Activator.CreateInstance(type);
			}
		}

		public void TestDocWrapperNewMethodsDoNotThrowException()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(DocumentWrapper));
			retriever.IncludeClientDlls = true;

			foreach (Type docWrapperType in retriever.Retrieve())
			{
				using (OverrideClientAssembly(docWrapperType.Assembly))
				{
					MethodInfo registerThisSubTypeOverride = docWrapperType.GetMethod("RegisterThisSubTypeOverride", BindingFlags.Public | BindingFlags.Static);
					if (registerThisSubTypeOverride != null)
					{
						registerThisSubTypeOverride.Invoke(null, null);

						var overridableNewDelegateField = docWrapperType.GetField("OverridableNewDelegate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
						AssertNotNull("Couldn't find OverridableNewDelegate field", overridableNewDelegateField);
						var overridableNewDelegate = overridableNewDelegateField.GetValue(null);
						var overriddenNewDelgate = (Delegate)overridableNewDelegate.GetType().GetProperty("Value").GetValue(overridableNewDelegate);

						try
						{
							overriddenNewDelgate.DynamicInvoke(new object[overriddenNewDelgate.Method.GetParameters().Length]);
						}
						catch (TargetInvocationException ex)
						{
							if (ex.InnerException is StackOverflowException)
							{
								AddError("Stack overflow in " + docWrapperType.FullName);
							}
							else
							{
								AddError("Exception " + ex.Message + " was thrown in " + docWrapperType.FullName);
							}
						}
						catch (TargetParameterCountException)
						{
							AddError("Parameter count mismatch when invoking delegate from " + docWrapperType.FullName);
						}
					}
				}
			}

			ReportError("Client override delegate caused problems in the following classes:");
		}

		// TODO: WI00801582 - Remove Serialization Reflection Tests
		static HashSet<string> GetTestSerializableClassesBaseline()
		{
			// Assemblies that are currently using the old serialization mechanism. (SYSLIB0050 and SYSLIB0051)
			const string textFileResource = "Enterprise.ReflectionTest.Properties.TestSerializableClassesBaseline.txt";
			var baseline = new HashSet<string>(StringComparer.Ordinal);
			using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(textFileResource);
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					baseline.Add(line);
				}
			}
			return baseline;
		}

		// TODO: WI00801582 - Remove Serialization Reflection Tests
		public void TestClassesInheritedFromSerializableClassesMustAlsoBeSerializable()
		{
			var baseline = GetTestSerializableClassesBaseline();
			foreach (Assembly assembly in AssemblyRetriever.LoadAsssembliesFromSimpleNames(Assemblies))
			{
				if (!baseline.Contains(assembly.GetName().Name.ToUpper()))
				{
					continue;
				}

				foreach (Type type in assembly.GetTypes())
				{
					if (
						type != typeof(Assertion) &&
						type.GetCustomAttributes(typeof(Core.NonSerializedClassAttribute), false).Length == 0)
					{
						Type baseType = type.BaseType;
						if (baseType != null && IsTypeSerializableByValueAndIsExceptionOrNotSystemOrAutoGenerated(baseType, type.Namespace) && !IsTypeSerializable(type))
						{
							AddError(type.FullName + " (base class " + baseType.FullName + ")");
						}
					}
				}
			}
			ReportError("The following classes have base classes that are [Serializable]. The decendent must also be marked as [Serializable] and meet the serialization requirements.");
		}

		// TODO: WI00801582 - Remove Serialization Reflection Tests
		public void TestSerializableClassesMeetSerializableRequirements()
		{
			var baseline = GetTestSerializableClassesBaseline();
			foreach (Assembly assembly in AssemblyRetriever.LoadAsssembliesFromSimpleNames(Assemblies))
			{
				if (!baseline.Contains(assembly.GetName().Name.ToUpper()))
				{
					continue;
				}

				foreach (Type type in assembly.GetTypes())
				{
					if (IsTypeSerializableByValueAndIsExceptionOrNotSystemOrAutoGenerated(type, ""))
					{
						EnsureTypeMeetsSerializableByValueRequirements(type);
					}
				}
			}
			ReportError("The following classes are marked as [Serializable] but don't meet the serialization requirements.");
		}

		void EnsureTypeMeetsSerializableByValueRequirements(Type type)
		{
			if (!type.IsValueType &&
				!type.IsSubclassOf(typeof(Delegate)) &&
				!type.IsSubclassOf(typeof(MarshalByRefObject)))
			{
				bool implementsISerializable = typeof(ISerializable).IsAssignableFrom(type);
				if ((implementsISerializable || HasStreamingContextConstructor(type.BaseType)) && !HasStreamingContextConstructor(type))
				{
					AddError(type.FullName + ": must have a constructor with signature: protected " + type.Name + "(SerializationInfo info, StreamingContext context) to honor the base class");
				}
				if (!implementsISerializable)
				{
					foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
					{
						if (!field.FieldType.IsInterface &&
														!field.FieldType.IsGenericParameter &&
							!IsTypeSerializable(field.FieldType) &&
							!field.FieldType.IsSubclassOf(typeof(MarshalByRefObject)) &&
							!field.IsDefined(typeof(NonSerializedAttribute), false))
						{
							AddError(type.FullName + ": field '" + field.Name + "' is not serializable");
						}
					}
				}
			}
		}

		bool HasStreamingContextConstructor(Type type)
		{
			return type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null) != null;
		}

		bool IsTypeSerializableByValueAndIsExceptionOrNotSystemOrAutoGenerated(Type type, string childNamespace)
		{
			return
				IsTypeSerializable(type) &&
				!type.IsSubclassOf(typeof(MarshalByRefObject)) &&
				type.Namespace != null &&
				type.GetCustomAttributes(typeof(GeneratedCodeAttribute), false).Length == 0 &&
				type.GetCustomAttributes(typeof(AutoGeneratedSourceCodeAttribute), false).Length == 0 &&
				(!type.Namespace.StartsWith("System") || typeof(Exception).IsAssignableFrom(type))
				&& !(type.FullName?.EndsWith("__ContractsRuntime+ContractException") ?? false)  // auto generated exception
				&& !Excluded(type, childNamespace);
		}

		bool IsTypeSerializable(Type type)
		{
#if NETFRAMEWORK
			return type.IsSerializable;
#elif NET
			return type.IsAssignableTo(typeof(ISerializable));
#else
#error Unexpected target platform
#endif
		}

		bool Excluded(Type type, string namespac)
		{
			return (!string.IsNullOrEmpty(namespac) && (
						namespac.StartsWith("Enterprise.Warehouse.RF.Core", StringComparison.OrdinalIgnoreCase) ||
						namespac.StartsWith("Enterprise.Warehouse.RF.Business.Testing.Exceptions", StringComparison.OrdinalIgnoreCase) ||
						namespac.StartsWith("Fluid.Drawing", StringComparison.OrdinalIgnoreCase)))
				//FlexCel exceptions use ISafeSerializationData and are tested for proper serialization in c:\dev\Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\_FlexCelTest.cs, TestFlexCelExceptionsCanBeSerializedAndDeserialized
				|| type.Namespace.StartsWith("FlexCel", StringComparison.OrdinalIgnoreCase) && type.IsSubclassOf(typeof(Exception));
		}

#endregion

		#region Naming Rules

		static bool IsAssemblyExcludedFromDuplicatesCheck(Assembly assembly)
		{
			string assemblyName = assembly.GetName().Name;

			if
			(
				   assemblyName.EndsWith(".XmlSerializers")
				|| assemblyName.Contains("Warehouse.RF")
				|| assemblyName.Contains("Enterprise.LocalTransport.Mobile")
				|| assemblyName.Contains("Mobile.Shared")
				|| assemblyName.Contains("MobileLoader")
				|| assemblyName.Contains("Enterprise.Edifact.D99B")
				|| assemblyName.StartsWith("XmlDiffPatch")
			)
			{
				return true;
			}

			string fileName = Path.GetFileName(assembly.Location);

			if (fileName.Equals("TestRunnerAnyCpu.exe", StringComparison.OrdinalIgnoreCase)
				// we temporarily exclude this CargoWise.Start.Unmerged.exe until we remove CargoWiseOne.Start.Unmerged.exe in WI00837307
				|| fileName.Equals("CargoWise.Start.Unmerged.exe", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (fileName.Equals("CargoWise.exe") || fileName.Equals("CargoWiseOne.exe") || fileName.Equals("CargoWiseOneAnyCpuexe"))
			{
				// These stub/placeholder exec are using the same Program.cs (and thus same namespace).
				return true;
			}

			return false;
		}

		static bool TypeIsAllowedToBeDuplicatedAcrossAssemblies(Type type)
		{
			var typeName = type.Name;
			var typeFullName = type.FullName;

			return
				   typeName == "CommonAssemblyInfo"
				|| typeName == "Contract"
				|| typeName == "PureAttribute"
				|| typeName == "ContractClassAttribute"
				|| typeName == "ContractInvariantMethodAttribute"
				|| typeName == "ContractClassForAttribute"
				|| typeName == "ContractAbbreviatorAttribute"
				|| typeName == "ContractVerificationAttribute"
				|| typeName == "SuppressClassNamesAreUniqueAcrossAssembliesMessageAttribute"
				|| typeName == "GeneratedInternalTypeHelper"
				|| typeFullName == "System.Runtime.CompilerServices.IsExternalInit" // used by C# 9.0 record types. Declared in IsExternalInit.cs
				|| typeName.StartsWith("<>")

				|| Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute))

				|| type.GetCustomAttributes(typeof(SuppressClassNamesAreUniqueAcrossAssembliesMessageAttribute), true).Length > 0
				|| type.GetCustomAttributes(typeof(NativeCppClassAttribute), true).Length > 0;
		}

		public void TestClassNamesAreUniqueAcrossAssembliesForEachNamespace()
		{
			Hashtable fullyQualifiedClassNameHashtable = new Hashtable();
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(object))
			{
				IncludeTestClasses = true
			};
			Type[] allTypes = retriever.Retrieve();
			var typesByAssemblyLive = allTypes.GroupBy(item => item.Assembly);

			foreach (IGrouping<Assembly, Type> types in typesByAssemblyLive)
			{
				if (IsAssemblyExcludedFromDuplicatesCheck(types.Key))
				{
					continue;
				}

				foreach (Type type in types)
				{
					if (!TypeIsAllowedToBeDuplicatedAcrossAssemblies(type))
					{
						if (!type.IsNestedFamORAssem && !type.IsNestedPrivate && !type.IsNestedPublic)
						{
							string key = type.Namespace;
							Type declaringType = type.DeclaringType;

							while (declaringType != null)
							{
								key += "." + declaringType.Name;
								declaringType = declaringType.DeclaringType;
							}

							key += "." + type.Name;
							var targetFrameworkAttribute = (TargetFrameworkAttribute)type.Assembly.GetCustomAttribute(typeof(TargetFrameworkAttribute));
							key += targetFrameworkAttribute?.FrameworkName; // Valid to have a Similar class with the same name in a different Framework. ie ExceptionExtensions(4.0) and ExceptionExtensions(4.5)

							if (fullyQualifiedClassNameHashtable.Contains(key))
							{
								object firstAssembly = fullyQualifiedClassNameHashtable[key];
								if (firstAssembly != null)
								{
									AddError(key + " found in assembly " + firstAssembly.ToString());
									fullyQualifiedClassNameHashtable[key] = null;
								}
								AddError(key + " found in assembly " + type.Assembly.ToString());
							}
							else
							{
								fullyQualifiedClassNameHashtable.Add(key, type.Assembly);
							}
						}
					}
				}
			}

			ReportError("These class names are duplicated across assemblies.");
		}

		[ExpectNoExceptions]
		public void TestSchemaConstantsHaveSameNameAsValue()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
			retriever.IncludeClientDlls = false;
			foreach (Type type in retriever.Retrieve())
			{
				Type schemaType = type.GetNestedType("Schema", BindingFlags.Public);

				if (schemaType != null)
				{
					foreach (FieldInfo field in schemaType.GetFields())
					{
						object fieldValue = field.GetValue(null);

						if (fieldValue is string stringValue)
						{
							if (field.Name != stringValue)
							{
								AddError(type.FullName + ".Schema." + field.Name + " != '" + stringValue + "'");
							}
						}
					}
				}
			}
		}

		//TODO: Implement when consensus agreed
		//		[ExpectNoExceptions]
		//		public void TestProtectedFieldsRepresentingBusinessObjectsStartWithLowercaseF()
		//		{
		//			SubClassRetriever Retriever = new SubClassRetriever(Assemblies, typeof(object));
		//			foreach (Type type in Retriever.Retrieve())
		//			{
		//				if (type.IsSubclassOf(typeof(BusinessObject)) || type.IsSubclassOf(typeof(BusinessObjectCollection)))
		//				{
		//					foreach (FieldInfo Field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
		//					{
		//
		//						if (Field.IsFamily)
		//						{
		//							if (Field.FieldType.IsSubclassOf(typeof(BusinessObject)))
		//							{
		//								if (Field.Name[0] != 'f' && Field.DeclaringType == type)
		//								{
		//									ZString PublicFieldName = new ZString(Field.Name).Substring(1);
		//									AddError(type.FullName + "." + Field.Name + " is a protected field that does not start with an 'f'");
		//								}
		//							}
		//						}
		//					}
		//				}
		//			}
		//			ReportError(
		//				@"The following fields are protected without being prefixed with an f.
		//They should be prefixed to better identify that they are 'hot' values, and a property should be used that checks the IsDeleted flag.");
		//		}
		//
		//		[ExpectNoExceptions]
		//		public void TestPublicReadOnlyBusinessObjectFieldsDoNotExist()
		//		{
		//			SubClassRetriever Retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
		//			foreach (Type type in Retriever.Retrieve())
		//			{
		//				if (type.IsSubclassOf(typeof(BusinessObject)) || type.IsSubclassOf(typeof(BusinessObjectCollection)))
		//				{
		//					foreach (FieldInfo Field in type.GetFields())
		//					{
		//						if (Field.IsInitOnly && Field.FieldType.IsSubclassOf(typeof(BusinessObject)))
		//						{
		//							if (Field.DeclaringType == type)
		//							{
		//								AddError(type.FullName + "." + Field.Name);
		//							}
		//						}
		//					}
		//				}
		//			}
		//			ReportError(
		//				@"The following properties are public readonly.
		//This means that if the object they represent is deleted, the property will still return a business object.
		//It is better to wrap the field with a property that checks the IsDeleted flag.");
		//		}

		#endregion

		#region TestEnsureAllClassesThatRequireASpecificTestCaseHasTest

		enum TypeKind
		{
			None,
			BaseTestForTypesWithAttribute,
			BaseTestForDerivedOrImplementingTypes,
			ConcreteTest
		}

		/// <summary>
		/// This ensures that when a base class is subclassed, its test case is also subclassed.
		///
		/// Example:
		///
		/// class BusinessBase { ... }
		///
		/// [TestsSubclassesOf(typeof(BusinessBase))]
		/// class BusinessBaseTest : TestCase
		/// {
		///
		/// }
		///
		/// Now, when you subclass BusinessBase, you must also subclass the test.
		///
		/// Example:
		///
		/// class ConcreteBusiness : BusinessBase { ... }
		///
		/// [TestedType(typeof(ConcreteBusiness))]
		/// class ConcreteBusinessTest : BusinessBaseTestCase
		/// {
		///
		/// }
		///
		/// Type ITestsInstancesOf.TestsInstancesOfType
		/// is replaced by TestedTypeAttribute(Type)
		///
		/// this allows to check for required tests without instantiating them
		/// </summary>
		public void TestAllClassesRequiringASpecificTestCaseHaveIt()
		{
			Assert("Precondition for performace: " + nameof(InReflectionTest), InReflectionTest);

			AssembliesContext ctx = AssembliesContext.Instance;
			(
				Type baseTest,
				(
					Type type,
					Type[] tests
				)[]
					derivedTestsForType
			)[] results;

#pragma warning disable SA1509 // Opening braces should not be preceded by blank line - not in this case
			{
				Type testsClassesWithAttributesAttributeType = ctx.GetReflectionType(typeof(TestsClassesWithAttributesAttribute));
				Type generatedAttributeType = ctx.GetReflectionType(typeof(CompilerGeneratedAttribute));
				Type generatedCodeAttributeType = ctx.GetReflectionType(typeof(GeneratedCodeAttribute));
				Type generatedByResourcesAttributeType = ctx.GetReflectionType(typeof(AutoGeneratedSourceCodeAttribute));
				Type testsSubclassesOfAttributeType = ctx.GetReflectionType(typeof(TestsSubclassesOfAttribute));
				Type testedTypeAttributeType = ctx.GetReflectionType(typeof(TestedTypeAttribute));
				Type testCaseType = ctx.GetReflectionType(typeof(TestCase));
				Type testClassAttributeType = ctx.GetReflectionType(typeof(TestClassAttribute));
				Assembly[] buildOutputAssemblies = ctx.GetBuildOutputAssembliesLive().ToArray();
				Type[] allTypes = buildOutputAssemblies
					.AsParallel()
					.SelectMany(item => item.GetTypes())
					.Where
					(
						item => !item.GetCustomAttributesData().Any
						(
							attr => attr.AttributeType == generatedAttributeType ||
								attr.AttributeType == generatedCodeAttributeType ||
								attr.AttributeType == generatedByResourcesAttributeType
						)
					)
					.ToArray();
				Dictionary<Assembly, HashSet<Assembly>> referencesByAssembly = ctx.GetAllReferencesByAssembly(buildOutputAssemblies);
				HashSet<Assembly> clientAssemblies = new HashSet<Assembly>
				(
					buildOutputAssemblies
						.AsParallel()
						.Where(item => REX_ClientDll.IsMatch(item.FullName))
						.AsSequential()
				);

				//THROUBLESHOUTING AID 1 - uncomment to list all TestCase derived classes without TestedType attribute by assembly in text file
				/*
								{
#pragma warning disable CW1054 // Do Not Use Hard Coded Paths - this is THROUBLESHOUTING AID normally commented
									using (StreamWriter wr = File.CreateText(@"C:\git\wtg\to-ann.txt"))
#pragma warning restore CW1054
									{
										foreach
										(
											var grp in allTypes.Where
											(
												item => !item.IsAbstract && testCaseType.IsAssignableFrom(item) &&
													IsReqiredToHaveTestedType(item) &&
													!item.GetCustomAttributesData().Any(attr => attr.AttributeType == testedTypeAttributeType)
											)
											.OrderBy(item => item.FullName)
											.GroupBy(item => item.Assembly.GetName().Name)
											.OrderByDescending(item => item.Count())
										)
										{
											wr.WriteLine(grp.Key);

											foreach (Type type in grp)
											{
												wr.Write('\t');
												wr.WriteLine(type.FullName);
											}
										}

										wr.Flush();
									}

									bool IsReqiredToHaveTestedType(Type type)
									{
										while (type.BaseType != null)
										{
											if
											(
												type.BaseType.GetCustomAttributesData().Any
												(
													item => item.AttributeType == testsSubclassesOfAttributeType || item.AttributeType == testsClassesWithAttributesAttributeType
												)
											)
											{
									return true;
											}

											type = type.BaseType;
										}

										return false;
									}
								}
				*/

				ILookup<Type, (Type type, CustomAttributeData attr)> baseTestsForTypesWithAttributeByAttributeType = null;
				ILookup<Type, (Type type, CustomAttributeData attr)> baseTestsForDerivedOrImplementingTypesByBaseType = null;
				ILookup<Type, (Type type, CustomAttributeData attr)> concreteTestsByTestedType = null;
				Type[] restOfTypes = null;

				{
					var typesByKind = allTypes.AsParallel()
						.SelectMany(GetTestingAttributesData)
						.ToLookup(item => item.kind, item => (item.type, item.attr));

					Parallel.Invoke
					(
						() => baseTestsForTypesWithAttributeByAttributeType = typesByKind[TypeKind.BaseTestForTypesWithAttribute]
							.ToLookup(item => (Type)item.attr.ConstructorArguments[0].Value),
						() => baseTestsForDerivedOrImplementingTypesByBaseType = typesByKind[TypeKind.BaseTestForDerivedOrImplementingTypes]
							.ToLookup(item => (Type)item.attr.ConstructorArguments[0].Value),
						() => concreteTestsByTestedType = typesByKind[TypeKind.ConcreteTest]
							.ToLookup(item => GetTestedType((Type)item.attr.ConstructorArguments[0].Value)),
						() => restOfTypes = typesByKind[TypeKind.None].Select(item => item.type).ToArray()
					);

					Type GetTestedType(Type type)
					{
						if (IsTestType(type) && type.Name == type.BaseType?.Name + "ForTest")
						{
							type = type.BaseType;
						}

						return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
					}
				}

				ILookup<Type, Type> derivedOrImplementingTypesByBaseType =
					//select base tests only
					baseTestsForTypesWithAttributeByAttributeType.SelectMany(item => item.Select(v => v.type))
						.Union
						(
							//select base tests and base types passed to TestsSubclassesOf
							baseTestsForDerivedOrImplementingTypesByBaseType.SelectMany(EnumerateBaseTypes)
						)
						.GroupBy(item => item.Assembly)
						//cross join assemblies
						.Join
						(
							allTypes.GroupBy(item => item.Assembly),
							item => 0,
							item => 0,
							(baseTypes, types) => (baseTypes, types)
						)
						.AsParallel()
						.Where
						(
							item => item.types.Key == item.baseTypes.Key ||
								referencesByAssembly[item.types.Key].Contains(item.baseTypes.Key)
						)
						.SelectMany
						(
							item => item.baseTypes.Join
							(
								item.types, baseType => 0, type => 0, (baseType, type) => (baseType, type)
							)
						)
						.Where(item => IsDerivedOrImplementing(item.baseType, item.type))
						.ToLookup(item => item.baseType, item => item.type);

				results = baseTestsForDerivedOrImplementingTypesByBaseType
					.SelectMany
					(
						item => item.Join
						(
							derivedOrImplementingTypesByBaseType[item.Key], baseTest => 0, type => 0, (baseTest, type) => (baseTest, type)
						)
					)
					.Concat
					(
						restOfTypes.AsParallel().Select
						(
							item =>
							(
								baseTests: item.GetCustomAttributesData()
									.SelectMany(attr => baseTestsForTypesWithAttributeByAttributeType[attr.AttributeType])
									.Distinct(),
								type: item
							)
						)
						.Where(item => item.baseTests.Any())
						.SelectMany
						(
							item => item.baseTests.Select(baseTest => (baseTest, item.type))
						)
						.AsSequential()
					)
					.GroupBy(item => item.baseTest, item => item.type)
					.AsParallel()
					.Select(items => FilterTypesRequiringTestDerivedFromBaseTest(items.Key.type, items.Key.attr, items))
					.Select
					(
						item =>
						(
							item.baseTest,
							derivedTestsForType: item.expectedToHaveDerivedTestTypes
								.Select
								(
									type =>
									(
										type,
										tests: concreteTestsByTestedType[type]
											.Where(test => item.derivedTests.Contains(test.type))
											.Select(test => test.type)
											.ToArray()
									)
								)
								.ToArray()
						)
					)
					.ToArray();

				bool IsTestType(Type type)
				{
					if (type == null)
					{
						throw new ArgumentNullException(nameof(type));
					}

					if (REX_TestClass.IsMatch(type.FullName))
					{
						return true;
					}

					if (type.GetCustomAttributesData().Any(item => item.AttributeType == testClassAttributeType))
					{
						return true;
					}

					while (type != null)
					{
						if (testCaseType.IsAssignableFrom(type))
						{
							return true;
						}

						type = type.DeclaringType;
					}

					return false;
				}

				IEnumerable<Type> EnumerateBaseTypes(IGrouping<Type, (Type type, CustomAttributeData attr)> grp)
				{
					yield return grp.Key; //baseType

					foreach (var v in grp)
					{
						yield return v.type; //baseTest

						if (v.attr.ConstructorArguments.Count < 3)
						{
							continue;
						}

						foreach
						(
							Type excludeTypeAndItsDescendands in
								((ReadOnlyCollection<CustomAttributeTypedArgument>)v.attr.ConstructorArguments[2].Value)
									.Where(item => item.Value != null)
									.Select
									(
										item => item.Value is Type ? (Type)item.Value : ctx.GetReflectionType((string)item.Value)
									)
									.Where(item => item != null)
						)
						{
							yield return excludeTypeAndItsDescendands;
						}
					}
				}

				(
					Type baseTest,
					HashSet<Type> derivedTests,
					IEnumerable<Type> expectedToHaveDerivedTestTypes
				)
				FilterTypesRequiringTestDerivedFromBaseTest(Type baseTest, CustomAttributeData attr, IEnumerable<Type> types)
				{
					types = types.Where
					(
						item => !item.IsInterface && !IsTestType(item) //GenericTypeDefinitions are expected to have tests
					);

					bool? includeAbstractClasses = (bool?)attr.NamedArguments
						.SingleOrDefault(item => item.MemberName == "IncludeAbstractClasses").TypedValue.Value;

					if (includeAbstractClasses != true)
					{
						types = types.Where(item => !item.IsAbstract);
					}

					bool? excludeClientDlls = (bool?)attr.NamedArguments
						.SingleOrDefault(item => item.MemberName == "ExcludeClientDlls").TypedValue.Value;

					if (excludeClientDlls == true)
					{
						types = types.Where(item => !clientAssemblies.Contains(item.Assembly));
					}

					ReadOnlyCollection<CustomAttributeTypedArgument> restrictedToAssemblies =
						(ReadOnlyCollection<CustomAttributeTypedArgument>)attr.NamedArguments
							.SingleOrDefault(item => item.MemberName == "RestrictedToAssemblies").TypedValue.Value;

					if (restrictedToAssemblies != null && restrictedToAssemblies.Count > 0)
					{
						HashSet<string> names = new HashSet<string>
						(
							restrictedToAssemblies.Select(item => (string)item.Value),
							StringComparer.OrdinalIgnoreCase
						);

						types = types.Where(item => names.Contains(item.Assembly.GetName().Name));
					}

					bool? excludePrivate = (bool?)attr.NamedArguments
						.SingleOrDefault(item => item.MemberName == "ExcludePrivate").TypedValue.Value;

					if (excludePrivate == true)
					{
						types = types.Where(item => item.IsVisible);
					}

					if (attr.AttributeType == testsSubclassesOfAttributeType)
					{
						bool? requireTestOnlyInFirstSubLevel = (bool?)attr.NamedArguments
							.SingleOrDefault(item => item.MemberName == "RequireTestOnlyInFirstSubLevel").TypedValue.Value;

						if (requireTestOnlyInFirstSubLevel == true)
						{
							Type baseType = (Type)attr.ConstructorArguments[0].Value;

							types = types.Where
							(
								item => item.BaseType == baseType ||
								baseType.IsGenericTypeDefinition && item.IsGenericType && item.GetGenericTypeDefinition() == baseType ||
								baseType.IsGenericTypeDefinition && item.BaseType.IsGenericType && item.BaseType.GetGenericTypeDefinition() == baseType ||
								baseType.IsInterface && item.FindInterfaces((t, o) => true, null)
									.Except(item.BaseType.FindInterfaces((t, o) => true, null))
									.Any(type => type == baseType || baseType.IsGenericTypeDefinition && type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
							);
						}
					}

					HashSet<Type> derivedTests = new HashSet<Type>(derivedOrImplementingTypesByBaseType[baseTest]);

					if (attr.ConstructorArguments.Count < 2)
					{
						return (baseTest, derivedTests, types);
					}

					HashSet<Type> excludedFromTestAttributeTypes;
					Type excludedFromTestAttributeType = attr.ConstructorArguments[1].Value as Type;

					if (excludedFromTestAttributeType != null)
					{
						excludedFromTestAttributeTypes = new HashSet<Type>(Enumerable.Repeat(excludedFromTestAttributeType, 1));
					}
					else if (attr.ConstructorArguments[1].Value == null)
					{
						excludedFromTestAttributeTypes = new HashSet<Type>();
					}
					else
					{
						excludedFromTestAttributeTypes = new HashSet<Type>
						(
							((ReadOnlyCollection<CustomAttributeTypedArgument>)attr.ConstructorArguments[1].Value)
								.Where(item => item.Value != null)
								.Select(item => (Type)item.Value)
						);
					}

					types = types.Where
					(
						type => !type.GetCustomAttributesData()
							.Any(item => excludedFromTestAttributeTypes.Contains(item.AttributeType))
					);

					if (attr.ConstructorArguments.Count < 3)
					{
						return (baseTest, derivedTests, types);
					}

					HashSet<Type> excludedTypesAndTheirDescendants = new HashSet<Type>
					(
						((ReadOnlyCollection<CustomAttributeTypedArgument>)attr.ConstructorArguments[2].Value)
							.Where(item => item.Value != null)
							.Select
							(
								item => item.Value is Type ? (Type)item.Value : ctx.GetReflectionType((string)item.Value)
							)
							.Where(item => item != null)
							//find their descendants
							.SelectMany
							(
								item => Enumerable.Repeat(item, 1).Concat(derivedOrImplementingTypesByBaseType[item])
							)
					);

					types = types.Where
					(
						item => !excludedTypesAndTheirDescendants.Contains(item)
					);

					return (baseTest, derivedTests, types);
				}

				//one type may have more than one attribute
				IEnumerable<(TypeKind kind, Type type, CustomAttributeData attr)> GetTestingAttributesData(Type type)
				{
					foreach (CustomAttributeData item in type.GetCustomAttributesData())
					{
						if (item.AttributeType == testsClassesWithAttributesAttributeType)
						{
							yield return (TypeKind.BaseTestForTypesWithAttribute, type, item);
						}
						else if (item.AttributeType == testsSubclassesOfAttributeType)
						{
							yield return (TypeKind.BaseTestForDerivedOrImplementingTypes, type, item);
						}
						else if (item.AttributeType == testedTypeAttributeType)
						{
							yield return (TypeKind.ConcreteTest, type, item);
						}
						else
						{
							yield return (TypeKind.None, type, null);
						}
					}
				}
			}
#pragma warning restore SA1509 // Opening braces should not be preceded by blank line

			//THROUBLESHOUTING AID 2 - uncomment to list not tested types (usually missed TestedType attribute on test) by base test in xml file
			/*
						XmlDocument doc = new XmlDocument();

						doc.LoadXml("<xml/>");

						XmlElement root = doc.DocumentElement;
			*/
			Dictionary<Type, HashSet<Type>> baseline = GetTestAllClassesRequiringASpecificTestCaseHaveItBaseline(ctx);

			foreach (var r in results)
			{
				if (!baseline.TryGetValue(r.baseTest, out HashSet<Type> excluded))
				{
					excluded = null;
				}

				foreach (var v in r.derivedTestsForType.Where(item => item.tests.Length < 1))
				{
					if (excluded == null || !excluded.Contains(v.type))
					{
						AddError("Type '" + v.type.Assembly.GetName().Name + "," + v.type.FullName + "' requires a test case of type " + r.baseTest.FullName);
					}
				}

				//THROUBLESHOUTING AID 2
				/*
								XmlElement elmBaseTest = doc.CreateElement("base-test");
								bool hasMissingTests = false;

								elmBaseTest.SetAttribute("type", r.baseTest.FullName);
								elmBaseTest.SetAttribute("assembly", r.baseTest.Assembly.GetName().Name);

								foreach (var v in r.derivedTestsForType)
								{
									XmlElement elmTypeExpectedToHaveDerivedTests = doc.CreateElement("tested-type");
									bool hasTests = false;

									elmTypeExpectedToHaveDerivedTests.SetAttribute("type", v.type.FullName);
									elmTypeExpectedToHaveDerivedTests.SetAttribute("assembly", v.type.Assembly.GetName().Name);

									foreach (Type test in v.tests)
									{
										XmlElement elmTest = doc.CreateElement("test");

										elmTest.SetAttribute("type", test.FullName);
										elmTest.SetAttribute("assembly", test.Assembly.GetName().Name);

										elmTypeExpectedToHaveDerivedTests.AppendChild(elmTest);
										hasTests = true;
									}

									if (!hasTests)
									{
										elmBaseTest.AppendChild(elmTypeExpectedToHaveDerivedTests);
										hasMissingTests = true;
									}
								}

								if (hasMissingTests)
								{
									root.AppendChild(elmBaseTest);
								}
				*/
			}

			//THROUBLESHOUTING AID 2
			/*
#pragma warning disable CW1054 // Do Not Use Hard Coded Paths - this is THROUBLESHOUTING AID normally commented
						doc.Save(@"C:\git\wtg\anew-01.xml");
#pragma warning restore CW1054
			*/

			ReportError("Found Classes with missing tests - You need to add test cases inheriting from the base classes listed below along with all the other test classes for the containing solution. The base test class tests in CW1 make sure that that the fundamental precepts of the design of these types of object, or objects with these attributes have been followed.");
		}

		static Dictionary<Type, HashSet<Type>> GetTestAllClassesRequiringASpecificTestCaseHaveItBaseline(AssembliesContext ctx)
		{
			XPathNavigator nav;

			using (Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.ReflectionTest.Properties.TestAllClassesRequiringASpecificTestCaseHaveIt-baseline.xml"))
			{
				nav = (new XPathDocument(strm)).CreateNavigator();
			}
			nav.MoveToFirstChild();

			return nav.SelectChildren(XPathNodeType.Element).Cast<XPathNavigator>()
				.ToDictionary
				(
					item => ctx.GetReflectionType
						(
							$"{item.GetAttribute("type", string.Empty)}, {item.GetAttribute("assembly", string.Empty)}"
						),
					item => new HashSet<Type>
					(
						item.SelectChildren(XPathNodeType.Element).Cast<XPathNavigator>()
							.Select
							(
								child => ctx.GetReflectionType
									(
										$"{child.GetAttribute("type", string.Empty)}, {child.GetAttribute("assembly", string.Empty)}"
									)
							)
					)
				);
		}

		static bool IsDerivedOrImplementing(Type baseType, Type type)
		{
			if (baseType == type)
			{
				return false;
			}

			if (baseType.IsGenericTypeDefinition) //does not handle implemented generic interfaces
			{
				Type currentType = type;

				while (currentType != null)
				{
					if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == baseType)
					{
						return true;
					}

					currentType = currentType.BaseType;
				}
			}

			return baseType.IsAssignableFrom(type);
		}
		#endregion

		#region Assemblies To Test

		public static string[] Assemblies;

		#endregion

		#region Tests For Tests

		public void TestGetPublicWeaklyTypedCollections()
		{
			string[] result = GetPublicWeaklyTypedCollections(typeof(TestHelperClass1));
			Assert("No public weakly typed collections", result.Length == 0);
			result = GetPublicWeaklyTypedCollections(typeof(TestHelperClass2));
			Assert("No public weakly typed collections", result.Length == 0);
			result = GetPublicWeaklyTypedCollections(typeof(TestHelperClass3));
			Assert("Public weakly typed collection", result.Length == 1);
			result = GetPublicWeaklyTypedCollections(typeof(ReflectionTestHelper));
			Assert("Public weakly typed collection", result.Length == 1);
		}

		public void TestAllTestedEnterpriseAssemblies()
		{
			AssertEquals("Should contain Enterprise.DataTransfer", true, ((IList)Assemblies).Contains("Enterprise.DataTransfer"));
			AssertEquals("Should contain ZArchitecture", true, ((IList)Assemblies).Contains("Enterprise.ZArchitecture.Business"));
			AssertEquals("Should have at least 30 assemblies returned", Assemblies.Length > 30);
		}

		#region TestHelpers

		protected class TestHelperClass1
		{
			public ArrayList GetArrayList()
			{
				return new ArrayList();
			}
		}

		public class TestHelperClass2
		{
			protected ArrayList GetArrayList()
			{
				return new ArrayList();
			}
		}

		public class TestHelperClass3
		{
			public ArrayList GetArrayList()
			{
				return new ArrayList();
			}
		}

		public ArrayList TestArrayList = new ArrayList();

		#endregion

		#endregion

		#region TestUnitPropertiesReferencedInMeasureUnitAttributes

		public void TestUnitPropertiesReferencedInMeasureUnitAttributes()
		{
			var assembliesToTest = Assemblies.OrderBy(x => x).ToArray();
			var retriever = new SubClassRetriever(assembliesToTest, typeof(BusinessObject));

			var properties = new HashSet<string>();

			foreach (var type in retriever.Retrieve().Where(x => !x.Name.StartsWith("Dummy") && !IsAutoGeneratedSourceCodeAttributeReferenced(x)))
			{
				foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => PropertyInfoRequiresMeasureUnitAttribute(type, x)))
				{
					properties.Add(type.FullName + " " + info.Name);
				}
			}

			foreach (var property in properties.OrderBy(x => x))
			{
				AddError(property);
			}

			ReportError("The following properties may require the MeasureUnitAttribute. Please investigate and either add the attribute appropriately or suppress in Enterprise.ReflectionTest.ReflectionTestHelper.PropertiesWith[out]PrefixToIgnore:");
		}

		bool PropertyInfoRequiresMeasureUnitAttribute(Type type, PropertyInfo info)
		{
			return !IsReadOnly(info)
				&& info.PropertyType == typeof(ZString)
				&& (info.Name.Contains("Unit") || info.Name.Contains("UQ"))
				&& !PropertiesWithPrefixToIgnore.Contains(info.Name)
				&& !PropertiesWithoutPrefixToIgnore.Contains(type.FullName + " " + info.Name)
				&& HasListAttribute(info)
				&& !IsMeasureUnitAttributeReferenced(type, info);
		}

		bool IsAutoGeneratedSourceCodeAttributeReferenced(Type type)
		{
			return type.GetCustomAttributes<AutoGeneratedSourceCodeAttribute>(true).Any();
		}

		bool IsMeasureUnitAttributeReferenced(Type type, PropertyInfo info)
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(x => HasMeasureUnitAttribute(info, x));
		}

		bool HasMeasureUnitAttribute(PropertyInfo unitInfo, PropertyInfo propertyInfo)
		{
			var attribute = propertyInfo.GetCustomAttribute<MeasureUnitAttribute>(true);
			return attribute != null && attribute.UnitProperty == unitInfo.Name;
		}

		bool HasListAttribute(PropertyInfo info)
		{
			return info.GetCustomAttributes<ListAttribute>(true).Any();
		}

		bool IsReadOnly(PropertyInfo info)
		{
			if (info.GetSetMethod() == null)
			{
				return true;
			}

			var attribute = info.GetCustomAttribute<ReadOnlyAttribute>(false);
			return attribute != null && attribute.IsReadOnly;
		}

		// if it has a table prefix it's safe to assume it's only applicable to types that map to that table
		HashSet<string> PropertiesWithPrefixToIgnore
		{
			get
			{
				if (propertiesWithPrefixToIgnore == null)
				{
					propertiesWithPrefixToIgnore = new HashSet<string>();
					propertiesWithPrefixToIgnore.Add("JC_AirVentFlowRateUnit"); // MeasureUnit does not include volume per unit of time
					propertiesWithPrefixToIgnore.Add("JC_GrossWeightUQ");
					propertiesWithPrefixToIgnore.Add("D2_ProductUnitOfQty");
					propertiesWithPrefixToIgnore.Add("JO_InnerPacksUQ");
					propertiesWithPrefixToIgnore.Add("JO_OuterPacksUQ");
					propertiesWithPrefixToIgnore.Add("CW_NumberOfPackagesUQ");
					propertiesWithPrefixToIgnore.Add("TI_FrequencyUnit");
					propertiesWithPrefixToIgnore.Add("ABL_ManifestUQ");
					propertiesWithPrefixToIgnore.Add("B0_ManifestUQ");
					propertiesWithPrefixToIgnore.Add("BW_VolumeUQ");
					propertiesWithPrefixToIgnore.Add("BW_WeightUQ");
					propertiesWithPrefixToIgnore.Add("BX_QuantityUQ");
					propertiesWithPrefixToIgnore.Add("BY_ManifestUnitCode");
					propertiesWithPrefixToIgnore.Add("C1_UnitOfMeasure");
					propertiesWithPrefixToIgnore.Add("C5_PackagesUnits");
					propertiesWithPrefixToIgnore.Add("CD_DDTCUnit");
					propertiesWithPrefixToIgnore.Add("CD_RX_NK9802ValuePerUnitCurr");
					propertiesWithPrefixToIgnore.Add("CD_RX_NKPerUnitCostCurr");
					propertiesWithPrefixToIgnore.Add("CL_FlatAmountUQ");
					propertiesWithPrefixToIgnore.Add("G0_UQ2");
					propertiesWithPrefixToIgnore.Add("ICB_ManifestUQ");
					propertiesWithPrefixToIgnore.Add("JI_CustomsUnitQty");
					propertiesWithPrefixToIgnore.Add("JI_SupplementaryUQ");
					propertiesWithPrefixToIgnore.Add("JI_InvoiceUQ");
					propertiesWithPrefixToIgnore.Add("JI_MAF_MeasurementUQ");
					propertiesWithPrefixToIgnore.Add("JI_SupplementaryUQ");
					propertiesWithPrefixToIgnore.Add("JO_OuterPackUnitOfDimension");
					propertiesWithPrefixToIgnore.Add("NZ_PackageUQ");
					propertiesWithPrefixToIgnore.Add("JPB_ManifestUQ");
					propertiesWithPrefixToIgnore.Add("OP_OrderMultipleUnit");
					propertiesWithPrefixToIgnore.Add("OP_StockKeepingUnit");
					propertiesWithPrefixToIgnore.Add("US_RX_NK98InvCurrPerUnitCurr");
					propertiesWithPrefixToIgnore.Add("US_RX_NKPerUnitCostCurr");
					propertiesWithPrefixToIgnore.Add("WeightUnitForBinding");
				}

				return propertiesWithPrefixToIgnore;
			}
		}

		HashSet<string> propertiesWithPrefixToIgnore;

		// if it doesn't have a table prefix the property name could exist on multiple types that are completely unrelated
		HashSet<string> PropertiesWithoutPrefixToIgnore
		{
			get
			{
				if (propertiesWithoutPrefixToIgnore == null)
				{
					propertiesWithoutPrefixToIgnore = new HashSet<string>();
					propertiesWithoutPrefixToIgnore.Add("Enterprise.ZArchitecture.Business.UnitConversion FromUnit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.ZArchitecture.Business.UnitConversion ToUnit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Freight.QuotedBookings.Business.QuotedBooking FrequencyUnit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.ClientRateEntry Unit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.QuoteEntry Unit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.RateEntry Unit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.CustomConversionFactor VolumeUnit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.CustomConversionFactor WeightUnit");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.RateLineItem UnitMultipleString");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Rating.Business.RelatedRateLineItem UnitMultipleString");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.AU.Business.EXDOC.QuarantineExDocLine QL_AqisCustomsWeightUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine Packages1UQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.NZ.Business.MAFeBACCa.MAFMessagingBO ZX_MeasurementUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_DimUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ1");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ2");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ3");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ4");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ5");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ACEFDA US_UQ6");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_InnerAmountUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_InnerPackageUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_OuterPackageUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_PackagesUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_QtyPerPackageUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.AMSLine US_TotalQuantityUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.ConstituentElement US_PGAUnitOfMeasure");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.Pesticide US_UQ1");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.Pesticide US_UQ2");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.USAIILineAddInfo US_InvUQDisp");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.Business.Vehicle US_EnginePowerUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.Customs.US.ISF.Business.CusISFHeader BF_EstimatedQuantityUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.eTail.Business.HVLVBookingHeader HVH_GrossVolumeUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.eTail.Business.HVLVBookingHeader HVH_GrossWeightUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.eTail.Business.HVLVConsignment HVC_VolumeUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.eTail.Business.HVLVConsignment HVC_WeightUQ");
					propertiesWithoutPrefixToIgnore.Add("Enterprise.eTail.Business.HVLVItemLine HVS_WeightUnit");
				}

				return propertiesWithoutPrefixToIgnore;
			}
		}

		HashSet<string> propertiesWithoutPrefixToIgnore;

		#endregion

		#region TestIComplianceItemRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM

		public void TestIComplianceItemRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM()
		{
			var retriever = new SubClassRetriever(Assemblies, Type.GetType("Enterprise.ComplianceRisk.Integration.IComplianceItemRiskStatusProvider, Enterprise.ComplianceRisk.Integration"));
			retriever.IncludeAbstractClasses = false;
			retriever.IncludeAutoGeneratedCode = false;
			var retrievedTypes = retriever.Retrieve();

			var approvedList = ApprovedListNotAllowedToBeModifiedUnlessConfirmedWithMDM.Select(x => Type.GetType(x.TypeUri));
			var notApprovedList = new List<Type>();

			foreach (var retrievedType in retrievedTypes)
			{
				if (!approvedList.Any(x => retrievedType == x || retrievedType.IsSubclassOf(x)))
				{
					notApprovedList = notApprovedList.Where(x => !x.IsSubclassOf(retrievedType)).ToList();
					if (notApprovedList.All(x => !retrievedType.IsSubclassOf(x)))
					{
						notApprovedList.Add(retrievedType);
					}
				}
			}

			var testType = Type.GetType("Enterprise.ComplianceRisk.Business.Test.ComplianceRiskBusinessObjectTestCase, Enterprise.ComplianceRisk.Business.Test");
			var approvedTestList = ApprovedListNotAllowedToBeModifiedUnlessConfirmedWithMDM.Select(x => Type.GetType(x.TestTypeUri));

			foreach (var approvedTest in approvedTestList)
			{
				if (!testType.IsAssignableFrom(approvedTest))
				{
					notApprovedList.Add(approvedTest);
				}
			}

			foreach (var notApprovedType in notApprovedList)
			{
				AddError(notApprovedType.FullName);
			}

			ReportError("If you want a class to implement IComplianceItemRiskStatusProvider, please ensure that 1) you have discussed relevant information with MDM and 2) implement a test inherit from ComplianceRiskBusinessObjectTestCase at the same time please.");
		}

		readonly (string TypeUri, string TestTypeUri)[] ApprovedListNotAllowedToBeModifiedUnlessConfirmedWithMDM = new[]
		{
			("Enterprise.Freight.Forwarding.Business.ForwardingConsol, Enterprise.Freight.Forwarding.Business", "Enterprise.Freight.Forwarding.Business.Testing.ForwardingConsolComplianceRiskTest, Enterprise.Freight.Forwarding.Business.Test"),
			("Enterprise.Freight.Forwarding.Business.ForwardingShipment, Enterprise.Freight.Forwarding.Business", "Enterprise.Freight.Forwarding.Business.Testing.ForwardingShipmentComplianceRiskTest, Enterprise.Freight.Forwarding.Business.Test"),
			("Enterprise.Freight.QuotedBookings.Business.QuotedBooking, Enterprise.Freight.QuotedBookings.Business", "Enterprise.Freight.QuotedBookings.Business.Test.QuoteBookingComplianceRiskTest, Enterprise.Freight.QuotedBookings.Business.Test"),
			("Enterprise.Customs.Business.BaseJobDeclaration, Enterprise.Customs.Business","Enterprise.Customs.Business.Testing.BaseJobDeclarationComplianceRiskTest, Enterprise.Customs.Business.Test"),
			("Enterprise.Freight.Agency.Business.AgencyShipment, Enterprise.Freight.Agency.Business", "Enterprise.Freight.Agency.Business.Testing.AgencyShipmentComplianceRiskTest, Enterprise.Freight.Agency.Business.Test")
		};

		#endregion TestIComplianceItemRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM

		#region TestIAutoRatingImplementersHaveAutoratingAuditLogNote

		public void TestIAutoRatingImplementersHaveAutoratingAuditLogNote()
		{
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, Type.GetType("Enterprise.MasterFiles.Business.IAutoRating, Enterprise.MasterFiles.Business"));
			retriever.IncludeAbstractClasses = false;
			retriever.IncludeAutoGeneratedCode = false;
			retriever.IncludeClientDlls = true;
			retriever.IncludeNestedClasses = true;
			retriever.IncludePrivateNestedClasses = true;
			retriever.IncludeTestClasses = false;
			Type[] retrievedTypes = retriever.Retrieve();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			foreach (Type retrievedType in retrievedTypes)
			{
				using (OverrideClientAssembly(retrievedType.Assembly))
				{
					if (typeof(IStmNoteParent).IsAssignableFrom(retrievedType) &&
						!retrievedType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
					{
						BusinessObject bizo = factory.New(retrievedType);

						if (!((IList)((IStmNoteParent)bizo).NoteTypes).Contains(PredefinedNoteTypes.Instance.AutoRatingAuditLog))
						{
							if (!IsExcludedFromIAutoRatingHasAutoratingAuditLogNote(retrievedType))
							{
								AddError(retrievedType.FullName);
							}
						}
					}
				}
			}

			ReportError("The following classes implement IAutoRating but do not have the AutoRatingAuditLog in their NoteTypesCore implementation.");
		}

		bool IsExcludedFromIAutoRatingHasAutoratingAuditLogNote(Type typeofIAutoRating)
		{
			return typeofIAutoRating.GetCustomAttributes(typeof(AutoRatingAuditLogExclude), true).Length > 0;
		}

		#endregion

		#region TestIImportExportImplementedImplicitly

		public void TestIImportExportImplementedImplicitly()
		{
			var errors = new List<Type>();

			var iImportExportType = typeof(MasterFiles.Business.IImportExport);
			SubClassRetriever retriever = new SubClassRetriever(Assemblies, iImportExportType);
			retriever.IncludeAbstractClasses = false;
			retriever.IncludeAutoGeneratedCode = false;
			retriever.IncludeClientDlls = true;
			retriever.IncludeNestedClasses = true;
			retriever.IncludePrivateNestedClasses = true;
			retriever.IncludeTestClasses = false;

			foreach (Type type in retriever.Retrieve())
			{
				if (!type.GetInterfaceMap(iImportExportType).TargetMethods.Any(m => m.IsPublic && m.Name == "get_JobDirection"))
				{
					errors.Add(type);
				}
			}

			var errorMessage = "\r\nIImportExport’s JobDirection should be visible for Documents, MenuFilters etc. Please make an implementation implicit (public) for the following types\r\n"
									+ string.Join("\r\n", errors.Select(o => o.FullName).ToArray());

			AssertEquals(errorMessage, 0, errors.Count);
		}

		#endregion

		#region Get Output Assemblies From Project File

		public static string[] GetOutputAssemblies(string projectFilePath)
		{
			var projectXml = LoadXmlWithoutNamespace(projectFilePath);

			var assemblyName = GetAssemblyName(projectXml, projectFilePath);

			var targets = new List<string>();
			var targetFramework = projectXml.SelectNodes("/Project/PropertyGroup/TargetFramework")[0]?.InnerText;
			if (targetFramework != null)
			{
				targets.Add(targetFramework);
			}
			else
			{
				var targetFrameworks = projectXml.SelectNodes("/Project/PropertyGroup/TargetFrameworks")[0]?.InnerText;
				if (targetFrameworks != null)
				{
					targets.AddRange(targetFrameworks.Split(';'));
				}
			}

			// non-SDK .NET Framework projects
			if (targets.Count == 0 && projectXml.SelectNodes("/Project/PropertyGroup/TargetFrameworkVersion")[0] != null)
			{
				targets.Add("net4");
			}

			var outputAssemblies = targets.SelectMany(target =>
			{
				if (target.StartsWith("net4", StringComparison.OrdinalIgnoreCase))
				{
					return [assemblyName];
				}

				if (target.Equals($"$({NetCoreTargetFrameworkPropertyName})", StringComparison.OrdinalIgnoreCase)
					|| target.Equals($"$({NetCoreWindowsTargetFrameworkPropertyName})", StringComparison.OrdinalIgnoreCase)
					|| target.Equals($"{InternalCommonAssemblyInfo.CWNetCoreSubfolder}", StringComparison.OrdinalIgnoreCase)
					|| target.Equals($"{InternalCommonAssemblyInfo.CWNetCoreSubfolder}-windows", StringComparison.OrdinalIgnoreCase))
				{
					return GetAssemblyNamesForNetCorePath(InternalCommonAssemblyInfo.CWNetCoreSubfolder, assemblyName);
				}
				return [assemblyName];
			}).Where(name => name != null).ToArray();

			return outputAssemblies;
		}

		static string[] GetAssemblyNamesForNetCorePath(string targetFramework, string assemblyName)
		{
			if (assemblyName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			{
				// .NET Core projects have both .exe and .dll files
				// see: https://learn.microsoft.com/en-us/dotnet/core/deploying/#publish-framework-dependent
				var dllAssemblyName = assemblyName.Substring(0, assemblyName.Length - 4) + ".dll";
				return
				[
					$@"{targetFramework}\{assemblyName}",
					$@"{targetFramework}\{dllAssemblyName}"
				];
			}
			else
			{
				return
				[
					$@"{targetFramework}\{assemblyName}"
				];
			}
		}

		static string GetAssemblyName(XmlDocument projectXml, string projectFilePath)
		{
			var assemblyName = projectXml.DocumentElement.SelectSingleNode("PropertyGroup/AssemblyName")?.InnerText
				?? Path.GetFileNameWithoutExtension(projectFilePath);

			var isSdkStyleProject = projectXml.DocumentElement.HasAttribute("Sdk");
			var outputType = projectXml.DocumentElement.SelectSingleNode("PropertyGroup/OutputType")?.InnerText.ToLowerInvariant()
				?? (isSdkStyleProject ? "library" : null);

			var extension = outputType switch
			{
				"exe" or "winexe" => "exe",
				"library" or "database" => "dll",
				"module" => "netmodule",
				"package" => "msi",
				_ => throw new InvalidOperationException("Unknown output type: " + outputType)
			};

			return assemblyName + "." + extension;
		}

		public static XmlDocument LoadXmlWithoutNamespace(string xmlFilePath)
		{
			var propsXml = new XmlDocument();
			propsXml.Load(xmlFilePath);
			propsXml.DocumentElement.SetAttribute("xmlns", "");
			propsXml.LoadXml(propsXml.OuterXml);
			return propsXml;
		}

		#endregion

		#region TestDebugOnlyDllsHaveDeployToClientsFalse

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDebugOnlyDllsHaveDeployToClientsFalse()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var deployedToClients = new HashSet<string>(buildXml.GetAllAssembliesToBuild(deployedToClientsOnly: true), StringComparer.OrdinalIgnoreCase);
			var missingReleaseBuildConfiguration = new ConcurrentHashSet<string>();

			Parallel.ForEach(buildXml.GetAllSolutionFileNames(), solutionFileName =>
			{
				var solutionPath = Path.Combine(BaseSourcePath, solutionFileName);
				var solutionFile = new SolutionFile(solutionPath);
				var debugOnly = solutionFile.GetProjectsBuiltInConfiguration("DEBUG").Select(p => p.ProjectPath).Except(solutionFile.GetProjectsBuiltInConfiguration("RELEASE").Select(p => p.ProjectPath));
				foreach (var debugOnlyItem in debugOnly)
				{
					var assemblies = GetOutputAssemblies(Path.Combine(Path.GetDirectoryName(solutionPath), debugOnlyItem));
					foreach (var assembly in assemblies)
					{
						if (deployedToClients.Contains(assembly))
						{
							missingReleaseBuildConfiguration.TryAdd(assembly);
						}
					}
				}
			});
			if (missingReleaseBuildConfiguration.Any())
			{
				CombineAssertions("The following assemblies are not set to build for RELEASE in their solution config. If this is correct, configure them to not be deployed to clients in build.xml by adding the DeployToClients=\"false\" attribute to the corresponding <Bin> elements of each assembly.", () =>
				{
					foreach (var assembly in missingReleaseBuildConfiguration)
					{
						HtmlFail(HtmlFormatBadValue(assembly));
					}
				});
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Submodules

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSolutionFileNamesUsesRootBuildXml()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			AssertGreaterThanOrEqualTo("Should be loading solutions from the root build.xml file and not this submodule", buildXml.GetAllSolutionFileNames().Length, 100);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetAllSolutionFileNamesIncludesSubmodules()
		{
			var buildXml = BuildXml.CreateFromSourceCodeDirectory();
			var allSolutionFileNames = buildXml.GetAllSolutionFileNames();
			var introspectionSolutions = allSolutionFileNames.Where(s => s.IndexOf("Introspection", StringComparison.OrdinalIgnoreCase) > -1).ToArray();
			AssertEquals(2, introspectionSolutions.Length);
			AssertSequencesEqual([@"Common\Testing\Introspection\ReflectionTest\ReflectionTest.sln", @"Common\Testing\Introspection\SourceCodeTests\SourceCodeTests.sln"], introspectionSolutions);
		}

		#endregion

		#region TestAllSchemaBoundResourceStringsAreValid

		public void TestAllSchemaBoundResourceStringsAreValid()
		{
			StringBuilder errors = new StringBuilder();
			var typeTable = new Dictionary<string, List<Type>>();
			var retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
			foreach (Type type in retriever.Retrieve())
			{
				string tableName = SchemaBoundResourceStrings.GetTableName(type);
				if (!string.IsNullOrEmpty(tableName))
				{
					List<Type> typeList;
					typeTable.TryGetValue(tableName, out typeList);
					if (typeList == null)
					{
						typeList = new List<Type>();
						typeTable.Add(tableName, typeList);
					}
					typeList.Add(type);
				}
			}

			foreach (var entry in typeTable)
			{
				ushort asmid;
				var manifestData = SchemaBoundResourceStrings.GetManifestData(entry.Key, entry.Value[0], out asmid);
				if (manifestData != null)
				{
					foreach (var resourceString in manifestData)
					{
						string[] parts = resourceString.Key.Split('|');
						if (parts.Length >= 3)
						{
							errors.AppendLine(string.Format("Too many pipes (|) in schema bound resource string key '{0}' defined in {1}ResourceStrings.xml." + resourceString.Key, entry.Key));
						}
						if (parts[0] != entry.Key)
						{
							errors.AppendLine(string.Format("Resource string with key '{0}' defined in {1}ResourceStrings.xml does not correctly start with table name '{1}'.", resourceString.Key, entry.Key));
						}
						try
						{
							if (parts.Length == 2 && entry.Value.Find(type => type.GetProperty(parts[1]) != null) == null)
							{
								errors.AppendLine(string.Format("Resource string with key '{0}' defined in {1}ResourceStrings.xml does not match a valid property of any derived type of {2}.", resourceString.Key, entry.Key, entry.Value[0].Name));
							}
						}
						catch (AmbiguousMatchException)
						{ }
					}
				}
				else
				{
					for (int i = 1; i < entry.Value.Count; i++)
					{
						if (SchemaBoundResourceStrings.GetManifestData(entry.Key, entry.Value[i], out asmid) != null)
						{
							errors.AppendFormat("Manifest retrieved for type {0} but not {1} linked to the same table\n", entry.Value[i].ToString(), entry.Value[0].ToString());
						}
					}
				}
			}

			Assert("\n" + errors.ToString(), errors.Length == 0);
		}

		#endregion

		#region TestCustomViewSchemasValidation
		public void TestCustomViewSchemasValidation()
		{
			var num = 0;
			var comparisonErrors = new StringBuilder();
			var retriever = new SubClassRetriever(Assemblies, typeof(BusinessObject));
			foreach (Type type in retriever.Retrieve())
			{
				var viewSample = new ViewSample(type);
				if (viewSample.IsView && viewSample.IsCustoms)
				{
					var enumerator = viewSample.ColumnEnumerator;
					while (enumerator.MoveNext())
					{
						var column = enumerator.Current;
						foreach (var tableName in viewSample.TableNames)
						{
							var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
							if (tableSchema != null)
							{
								var tableColumns = tableSchema.All.ToArray();
								foreach (var tableColumn in tableColumns)
								{
									if (column.Name.Equals(tableColumn.Name, StringComparison.OrdinalIgnoreCase))
									{
										var tempString = viewSample.ViewName + "-" + tableName + "-" + column.Name;
										if (!ColumnMismatchAcceptableList.Contains(tempString))
										{
											if (!(column.TypeInfo.Equals(tableColumn.TypeInfo)))
											{
												++num;
												comparisonErrors.AppendLine("(" + num + ") Different TypeInfo or Case : '" + column.Name + "' in \n VIEW: " + viewSample.ViewName + " - " + column.Name + " - " + column.TypeInfo + "\n and \n TABLE: " + tableName + " - " + tableColumn.Name + " - " + tableColumn.TypeInfo + "\n\n");
											}
											else if (!column.Name.Equals(tableColumn.Name, StringComparison.Ordinal))
											{
												++num;
												comparisonErrors.AppendLine("(" + num + ") Different Case : '" + column.Name + "' in \n VIEW: " + viewSample.ViewName + " - " + column.Name + " - " + column.TypeInfo + "\n and \n TABLE: " + tableName + " - " + tableColumn.Name + " - " + tableColumn.TypeInfo + "\n\n");
											}
										}
									}
								}
							}
						}
					}
				}
			}
			Assert("\n" + comparisonErrors.ToString(), comparisonErrors.Length == 0);
			AssertEquals("No error!", num, 0);
		}
		#endregion

		#region class ViewSample
		public class ViewSample
		{
			public ViewSample()
			{
				TypeValue = null;
				ViewName = "";
				NameSpace = "";
				TableNames = null;
				IsView = false;
				SchemaValue = null;
				IsCustoms = false;
				ColumnEnumerator = null;
			}
			public ViewSample(Type typeValue)
			{
				TypeValue = null;
				ViewName = "";
				NameSpace = "";
				TableNames = null;
				IsView = false;
				SchemaValue = null;
				IsCustoms = false;
				ColumnEnumerator = null;
				if (!typeValue.Name.IsNullOrEmpty())
				{
					NameSpace = typeValue.Namespace;
					IsCustoms = NameSpace.StartsWith("Enterprise.Customs", StringComparison.OrdinalIgnoreCase);
					if (IsCustoms)
					{
						var full_name = "[dbo].[" + typeValue.Name + "]";
						var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(@full_name, N'V'));";
						using (var cmd = Db.Connection.Command(sql))
						{
							cmd.AddParameter("@full_name", System.Data.SqlDbType.NVarChar, 128, full_name);
							var tempDefinition = cmd.ExecuteScalar();
							if ((tempDefinition != null) && (tempDefinition != DBNull.Value))
							{
								IsView = ((string)tempDefinition).Contains("CREATE VIEW", StringComparison.OrdinalIgnoreCase);
								if (IsView)
								{
									TypeValue = typeValue;
									ViewName = typeValue.Name;
									IsView = true;
									SchemaValue = BusinessObjectFactory.GetTableSchemaFromType(typeValue);
									if (SchemaValue != null && SchemaValue.All != null)
									{
										ColumnEnumerator = SchemaValue.All.GetEnumerator();
										TableNames = new List<string>();
										var sql1 = @"
SELECT DISTINCT referenced_entity_name
FROM sys.sql_expression_dependencies AS sed
INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id
WHERE referencing_id = OBJECT_ID(@tempView_name)
AND referencing_class_desc = 'OBJECT_OR_COLUMN'
AND referenced_class_desc = 'OBJECT_OR_COLUMN'
AND o.type_desc = 'VIEW'";
										using (var cmd1 = Db.Connection.Command(sql1))
										{
											cmd1.AddParameter("@tempView_name", System.Data.SqlDbType.NVarChar, 128, ViewName);
											using (var reader = cmd1.ExecuteReader())
											{
												while (reader.Read())
												{
													TableNames.Add(reader["referenced_entity_name"].ToString());
												}
											}
										}
										return;
									}
								}
							}
						}
					}
				}
			}
			public Type TypeValue { get; private set; }
			public string ViewName { get; private set; }
			public string NameSpace { get; private set; }
			public List<string> TableNames { get; private set; }
			public bool IsView { get; private set; }
			public ITableSchema SchemaValue { get; private set; }
			public bool IsCustoms { get; private set; }
			public IEnumerator<SchemaColumn> ColumnEnumerator { get; private set; }
		}
		#endregion

		#region ColumnMismatchAcceptableList
		// Format: "View Name-Table Name-Column Name"
		public readonly string[] ColumnMismatchAcceptableList =
		{
			"ZZRefCusCodeListAttributeCombined-RefDatabase_RefCusCodeListAttribute-ZZE_Value",
			"ZZRefCusCodeListAttributeCombined-RefDatabase_RefCusCodeListAttribute-ZZE_ZXE_NKName",
			"ZZRefCusCodeListCombined-RefDatabase_RefCusCodeList-ZZD_Description",
			"ZZRefCusMapCombined-ZZRefCusMap-ZZM_CW1OrCommercialValue",
			"ZZRefCusMapCombined-ZZRefCusMap-ZZM_CW1orCommercialValue",
			"USCCarrierAndFIRMS-RefDbEntUS_USCFIRMS-US_Address",
			"USCCarrierAndFIRMS-RefDbEntUS_USCFIRMS-US_IsActive",
			"USCCarrierAndFIRMS-RefDbEntUS_USCFIRMS-US_Name"
		};
		#endregion

		#region Implementation

		delegate void TearDownAction(TestCase testCase);

		//TODO:investigate and either delete or raise with analyzers
#pragma warning disable IDE0051 // Remove unused private members - unsure if they are called from the framework
		List<TearDownAction> SetupAssemblyTestSetupAttributes(TestCase testCase)
		{
			var listOfTearDownActions = new List<TearDownAction>();
			var type = testCase.GetType();
			foreach (TestSetupAttribute setupAttribute in type.Assembly.GetCustomAttributes(typeof(TestSetupAttribute), false))
			{
				listOfTearDownActions.Add(setupAttribute.TearDown);
				setupAttribute.SetUp(testCase);
			}

			return listOfTearDownActions;
		}

		void TearDownAssemblyTestSetupAttributes(TestCase testCase, List<TearDownAction> listOfTearDownActions)
		{
			foreach (TearDownAction tearDownAction in listOfTearDownActions)
			{
				tearDownAction(testCase);
			}
		}
#pragma warning restore IDE0051

		Type GetMostExplicitImplementationForMethod(Type type, string methodName)
		{
			bool foundType = false;
			Type mostExplicitType = typeof(object);

			while (type != typeof(object))
			{
				foreach (MethodInfo itemInfo in type.GetMethods())
				{
					if (itemInfo.Name == methodName)
					{
						Type currentType = itemInfo.ReturnType;

						if (currentType.IsSubclassOf(mostExplicitType))
						{
							mostExplicitType = currentType;
							foundType = true;
						}
					}
				}

				type = type.BaseType;
			}

			return foundType ? mostExplicitType : null;
		}

		Type GetMostExplicitImplementationForType(Type type, string propertyName)
		{
			bool foundType = false;
			Type mostExplicitType = typeof(object);

			while (type != typeof(object))
			{
				foreach (PropertyInfo itemInfo in type.GetProperties())
				{
					if (IsMatchingProperty(itemInfo, propertyName))
					{
						Type currentType = itemInfo.PropertyType;

						if (currentType.IsSubclassOf(mostExplicitType))
						{
							mostExplicitType = currentType;
							foundType = true;
						}
					}
				}

				type = type.BaseType;
			}

			return foundType ? mostExplicitType : null;
		}

		bool IsMatchingProperty(PropertyInfo itemInfo, string propertyName)
		{
			return itemInfo.Name != "Item" && itemInfo.Name == propertyName ||
				itemInfo.Name == "Item" && new List<ParameterInfo>(itemInfo.GetIndexParameters()).Exists(x => x.ParameterType == typeof(int) || x.ParameterType == typeof(ZInt));
		}

		ZStringBuilder errors;

		IDisposable OverrideClientAssembly(Assembly newAssembly)
		{
			return ClientHookLoader.Instance.OverrideClientAssemblyForTestIfNeeded(newAssembly)
				?? DisposableAction.NoAction;
		}

		protected override void SetUp()
		{
			base.SetUp();
			errors = new ZStringBuilder();
			Assemblies = AssembliesUnderTest.AllAssemblies;
		}

		internal void SetUpHelper()
		{
			SetUp();
		}

		internal void TearDownHelper()
		{
			TearDown();
		}

		internal void SetUpForRunWithAppDomain()
		{
			Db.Connection.BeginTransaction();
			TestingState.Setup();
			((ITransactionalTestInternals)this).SetInTransactionedTestCase();
			((ITransactionalTestInternals)this).SetInReflectionTestTemporary();

			SetUpHelper();
		}

		internal void TearDownForRunWithAppDomain()
		{
			try
			{
				TearDownHelper();
			}
			finally
			{
				TestingState.TearDown();
				Db.Connection.RollbackTransaction();
			}
		}

		protected void AddError(string errorText)
		{
			errors.Append(errorText + NewLineForHtml);
		}

		protected void ReportError(string errorHelpText)
		{
			ErrorReporter.Clear();
			if (errors.Length > 0)
			{
				throw new Exception(errorHelpText + NewLineForHtml + NewLineForHtml + errors.ToStringWithDelimiterBetweenAppends(NewLineForHtml));
			}
			else
			{
				Assert("This test passed", true);
			}
		}

		//TODO:investigate and either delete or raise with analyzers
#pragma warning disable IDE0051 // Remove unused private members - unsure if they are called from the framework
		void SetUpTest(TestCase testCase)
		{
			testCase.GetType().GetMethod("SetUp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(testCase, null);
		}

		void TearDownTestCase(TestCase testCase)
		{
			testCase.GetType().GetMethod("TearDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(testCase, null);
			while (Db.Connection.IsInTransaction)
			{
				Db.Connection.RollbackTransaction();
			}
			Db.Connection.BeginTransaction();
		}
#pragma warning restore IDE0051

		protected string[] GetPublicWeaklyTypedCollections(Type type)
		{
			List<string> result = new List<string>();
			if (type.DeclaringType == null || type.IsNestedPublic)
			{
				foreach (MemberInfo member in type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
				{
					Type returnType = null;
					if (member is PropertyInfo)
					{
						returnType = (member as PropertyInfo).PropertyType;
					}
					else if (member is FieldInfo)
					{
						returnType = (member as FieldInfo).FieldType;
					}
					else if (member is MethodInfo)
					{
						if (!(member as MethodInfo).IsSpecialName) // Don't care about property getters, they're picked up by the PropertyInfo
						{
							returnType = (member as MethodInfo).ReturnType;
						}
					}

					if (member.DeclaringType == type)
					{
						if (returnType == typeof(ArrayList) || returnType == typeof(IList) || returnType == typeof(Hashtable) || returnType == typeof(IDictionary))
						{
							if (!IsExcluded(member, typeof(SuppressWeaklyTypedCollectionMessageAttribute)) &&
								!IsExcluded(member.DeclaringType, typeof(SuppressWeaklyTypedCollectionMessageAttribute)) &&
								!IsSubclassedPropertyInfo(member))
							{
								result.Add(string.Format("{2} {0}.{1} [{3}]", type.FullName, member.Name, returnType.Name, type.Assembly.GetName().Name));
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		bool IsSubclassedPropertyInfo(MemberInfo memberInfo)
		{
			bool result = false;
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				MethodInfo method1 = propertyInfo.GetGetMethod();
				if (method1 != null)
				{
					MethodInfo method2 = propertyInfo.GetGetMethod().GetBaseDefinition();
					result = method2 != null && method1.DeclaringType != method2.DeclaringType;
				}
			}
			return result;
		}

		bool IsExcluded(MemberInfo memberInfo, Type excludeAttribute)
		{
			bool result = false;
			foreach (Attribute attribute in memberInfo.GetCustomAttributes(false))
			{
				if (attribute.GetType().Name == excludeAttribute.Name)
				{
					result = true;
					break;
				}
			}

			if (memberInfo.DeclaringType != null && !memberInfo.DeclaringType.Assembly.GetReferencedAssemblies().Any(name => name.FullName.Contains("CargoWise.Common")))
			{
				// The member comes from an assembly which doesn't have a reference to CargoWise.Common
				result = true;
			}

			return result;
		}

		ZString NewLineForHtml
		{
			get { return " <br/>" + System.Environment.NewLine; }
		}

		#endregion

		public static bool IsNotReflectionTestable(string assembly)
		{
			if (AssemblyChecker.IsNotTargetPrefix(assembly))
			{
				return true;
			}

			// This method can be passed either a file name or an assembly name.
			var assemblyName = Path.GetExtension(assembly).Equals(".dll", StringComparison.OrdinalIgnoreCase)
				? Path.GetFileNameWithoutExtension(assembly)
				: assembly;

			return string.Equals(assemblyName, "Enterprise.DbUpgrader.Resource.Test", StringComparison.OrdinalIgnoreCase);
		}

		static string GetTypeReadableName(Type type)
		{
			string typeName;

			if (type.IsGenericType)
			{
				var name = type.FullName;
				var tickIndex = name.IndexOf('`');
				if (tickIndex != -1)
				{
					name = name.Substring(0, tickIndex);
				}

				typeName = name + "<" + string.Join(", ", type.GenericTypeArguments.Select(GetTypeReadableName)) + ">";
			}
			else
			{
				typeName = type.FullName;
			}

			return typeName.Replace('+', '.');
		}
	}
}
