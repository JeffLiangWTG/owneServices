namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Reflection;
	using System.Threading.Tasks;
	using CargoWise.Application;
	using CargoWise.Bi.ConfigLoader;
	using CargoWise.Common;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.MDM.Subscribers;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.GlowInterop;
	using Moq;
	using NUnit.Framework;

	public class SubscriberLoaderTest : TransactionedTestCase
	{
		public void TestCannotSubscribeToTablesThatAreNotAudited()
		{
			var loader = new SubscriberLoader();
			var subscribers = loader.EnumerateAllSubscriberTypes();

			CombineAssertions("The following columns are subscribed to but not audited:\r\n", () =>
			{
				foreach (var sub2 in subscribers)
				{
					var sub = sub2 as ActualDataChangesAuditSubscriber;
					if (sub != null && sub.SpecificColumns != null)
					{
						AssertSubscriberColumnsAreAudited(sub);
					}
				}
			});
		}

		void AssertSubscriberColumnsAreAudited(ActualDataChangesAuditSubscriber sub)
		{
			foreach (SchemaColumn col in sub.SpecificColumns)
			{
				string schema = col.TableSchema.SqlSchemaName;
				string table = col.TableName;
				string column = col.Name;
				Assert(
					$"{schema}.{table} - {column}",
					BiAutomationConfigLoader.ColumnInAuditTableExists(schema, table, column)
				);
			}
		}

		public void TestEnumerateAllSubscriberTypes_IndividualSubscribersValid()
		{
			var loader = new SubscriberLoader();
			var subscribers = loader.EnumerateAllSubscriberTypes();

			AssertEquals("Subscribers should be loaded",
				true, subscribers.Any());
			AssertEquals("All subscribers should implement IAuditSubscriber",
				false, subscribers.Any(s => !typeof(IAuditSubscriber).IsAssignableFrom(s.GetType())));
			AssertEquals("Should be that all Subscriber Code.Lenth == 3",
				false, subscribers.Any(s => s.Code == null || s.Code.Trim().Length != 3));
			AssertEquals("All subscribers should have a description",
				false, subscribers.Any(s => s.Description == null));
			AssertEquals("Subscriber codes should be unique",
				true, subscribers.Select(s => s.Code).Distinct().Count() == subscribers.Select(s => s.Code).Count());

			AssertEquals("All subscribers should implement one of the valid IAuditSubscriber subtype interfaces",
				false,
				subscribers.Any(s =>
						!typeof(ActualDataChangesAuditSubscriber).IsInstanceOfType(s)
						&& !typeof(ChangedTableListOnlyAuditSubscriber).IsInstanceOfType(s)
						&& !typeof(TableValuePairSubscriber).IsInstanceOfType(s)
));
		}

		public void TestEnumerateAllSubscriberTypes_IActualDataChangesAuditSubscriber()
		{
			var loader = new SubscriberLoader();
			var actualDataChangesAuditSubscriber = loader.EnumerateAllSubscriberTypes().Where(
					s =>
						typeof(ActualDataChangesAuditSubscriber).IsInstanceOfType(s)
				).Cast<ActualDataChangesAuditSubscriber>();

			AssertEquals("There should be no subscribers that aren't subscribed to a table",
				false,
				actualDataChangesAuditSubscriber.Any(s =>
						s.Table == null
));
		}

		public void TestEnumerateAllSubscriberTypes_IChangedTableListOnlyAuditSubscriber()
		{
			var loader = new SubscriberLoader();
			var changedTableListOnlyAuditSubscriber = loader.EnumerateAllSubscriberTypes().Where(
						s =>
							typeof(ChangedTableListOnlyAuditSubscriber).IsInstanceOfType(s)
					).Cast<ChangedTableListOnlyAuditSubscriber>();

			var glowResponse = new HttpResponseMessage();
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			using (glowResponse.Content = new StringContent(@"{ value: [""JobShipment""] }"))
			{
				var glowClientMock = new Mock<IGlowServiceClient>();
				glowClientMock.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(glowResponse));
				var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
				clientFactoryMock.Setup(f => f.Create(It.Is<Uri>(x => x == new Uri("https://localhost/Glow/")))).Returns(glowClientMock.Object);
				ObjectFactory.Substitute(clientFactoryMock.Object);

				AssertEquals("There should be no subscribers that aren't subscribed to a table",
					false,
					changedTableListOnlyAuditSubscriber.Any(s =>
							s.SubscribedTables == null
							|| !s.SubscribedTables.Any()
							|| s.SubscribedTables.Any(t => t == null)));
			}
		}

		public void TestEnumerateAllSubscriberTypes_AreNotOfTypePatternMatching()
		{
			var allSubscribers = new SubscriberLoader().EnumerateAllSubscriberTypes();
			var patternMatchingType = typeof(PatternMatchingSubscriber<>);

			CombineAssertions("The following Borderwise subscribers should not be in EnumerateSubscribers:\r\n", () =>
			{
				foreach (IAuditSubscriber subscriber in allSubscribers)
				{
					Assert($"{subscriber.ToString()} is of type PatternMatching", !patternMatchingType.IsInstanceOfType(subscriber));
				}
			});
		}

		public void TestEnumerateAllSubscriberTypes_EnumeratesAllSubscribers()
		{
			var subscriberLoaderForTest = new SubscriberLoader();
			var allSubscribers = subscriberLoaderForTest.EnumerateAllSubscriberTypes();

			var query = from t in AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.SubscriberCode)).GetTypes()
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.BorderWiseSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.PaveSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.GlowSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.MdmSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.HvlvSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.AccountingSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.DopSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.TelematicsSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.TransportBookingSubscriberCode)).GetTypes())
							.Union(AssemblyLoader.LoadAssembly(subscriberLoaderForTest.GetSubscriberNamespacePrefix(SubscriberLoader.CoreSubscriberCode)).GetTypes())
						where
								t.IsClass
							&& !t.IsAbstract
							&& typeof(IAuditSubscriber).IsAssignableFrom(t)
							&& !string.IsNullOrWhiteSpace(t.Namespace)
							&& (
								t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.SubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.BorderWiseSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.PaveSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.GlowSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.MdmSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.HvlvSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.AccountingSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.DopSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.TelematicsSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.TransportBookingSubscriberCode), StringComparison.OrdinalIgnoreCase)
								|| t.Namespace.StartsWith(subscriberLoaderForTest.GetSubscriberNamespace(SubscriberLoader.CoreSubscriberCode), StringComparison.OrdinalIgnoreCase)
								)
							select t;

			AssertGreaterThan("Precondition: result of EnumerateAllSubscriberTypes is not empty", allSubscribers.ToArray().Length, 0);
			AssertGreaterThan("Precondition: expected value, created by code in the unit test, is not empty", query.AsEnumerable().ToArray().Length, 0);

			var queryCodes = new List<string>();
			var allSubscriberCodes = new List<string>();

			foreach (var subscriberType in query)
			{
				var subscriber = (IAuditSubscriber)Activator.CreateInstance(subscriberType);
				queryCodes.Add(subscriber.Code);
			}

			foreach (var subscriber in allSubscribers)
			{
				allSubscriberCodes.Add(subscriber.Code);
			}

			AssertContainsExactElementsInAnyOrder("Should enumerate all subscribers", queryCodes, allSubscriberCodes);
		}

		public void TestEnumerateAllSubscriberTypes_GetsSubscribersFromAllSubscriberCodes()
		{
			var subscriberCodes = new[] {
				SubscriberLoader.SubscriberCode,
				SubscriberLoader.BorderWiseSubscriberCode,
				SubscriberLoader.PaveSubscriberCode,
				SubscriberLoader.GlowSubscriberCode,
				SubscriberLoader.MdmSubscriberCode,
				SubscriberLoader.HvlvSubscriberCode,
				SubscriberLoader.AccountingSubscriberCode,
				SubscriberLoader.DopSubscriberCode,
				SubscriberLoader.TelematicsSubscriberCode,
				SubscriberLoader.TransportBookingSubscriberCode,
			};

			var subscriberLoaderForTest = new SubscriberLoader();
			var allSubscribers = subscriberLoaderForTest.EnumerateAllSubscriberTypes();

			CombineAssertions("All subscriber codes should have subscribers", () =>
			{
				foreach (var subscriberCode in subscriberCodes)
				{
					var subscriberNameSpace = subscriberLoaderForTest.GetSubscriberNamespace(subscriberCode);
					var subscribersForSubscriberCode = allSubscribers.Where(x => x.GetType().Namespace.StartsWith(subscriberNameSpace, StringComparison.OrdinalIgnoreCase));
					AssertGreaterThan($"Subscriber code {subscriberCode} should have subscribers", subscribersForSubscriberCode.Count(), 0);
				}
			});
		}

		public void TestSubscriberCodesAreUnique()
		{
			var allSubscribers = new SubscriberLoader().EnumerateAllSubscriberTypes();

			var duplicateCodeSubscribers =
				from s in allSubscribers
				join dup in (
					from s in allSubscribers
					group s by s.Code into grp
					where grp.Count() > 1
					select new { Code = grp.Key })
				on s.Code equals dup.Code
				orderby s.Code, s.GetType().Name
				select "    [" + (s.Code ?? "") + "] " + s.GetType().Name;

			Assert(
				"The following subscribers have duplicated codes:\r\n\r\n" + string.Join("\r\n", duplicateCodeSubscribers) + "\r\n",
				!duplicateCodeSubscribers.Any());
		}

		public void TestSubscriberLoaderFailure()
		{
			var fakeAssemblyName = SubscriberLoader.ZClientEdiAssemblyName;
			var mockAssemblyLoader = new Mock<IAssemblyLoader>();

			var expectedType = GetType();
			var expectedInnerException = new Exception("mock inner message");
			mockAssemblyLoader
				.Setup(x => x.LoadAssembly(It.IsAny<AssemblyName>()))
				.Throws(new ReflectionTypeLoadException(new Type[] { expectedType }, new Exception[] { expectedInnerException }));

			var originalAssemblyLoader = AssemblyLoader.Instance;
			try
			{
				AssemblyLoader.Instance = mockAssemblyLoader.Object;

				var loader = new SubscriberLoader();

				var subscribers = loader.EnumerateSubscribersOfType(fakeAssemblyName, fakeAssemblyName);

				AssertEquals("No subscriber should be loaded for invalid assembly.", 0, subscribers.Count());
			}
			finally
			{
				AssemblyLoader.Instance = originalAssemblyLoader;
			}
		}

		public void TestSubscriberCodeList()
		{
			var expectedSubscriberCodes = from p in typeof(SubscriberLoader).GetFields(BindingFlags.Public | BindingFlags.Static)
					 where p.FieldType == typeof(string) && p.Name.EndsWith("SubscriberCode")
					 select p.GetValue(null) as string;

			AssertContainsExactElementsInAnyOrder("Subscriber codes should match.", expectedSubscriberCodes, new SubscriberLoader().SubscriberCodes);
		}
	}
}
