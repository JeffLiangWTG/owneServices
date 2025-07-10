using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	sealed class HostedServiceBusinessObjectBindingsProviderTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAllBindingsAreSupportedByIndexes()
		{
			// Arrange
			var nudgingSchemaResolver = new NudgingSchemaResolver(new EnterpriseSchemaResolver());
			var predicateFactory = new PredicateFactory(nudgingSchemaResolver);
			var bindings = new HostedServiceBusinessObjectBindingsProvider().BusinessObjectBindings;

			var baselineExcludedQueueNames = new[]
			{
				"Document Delivery - EDocsProcessed = '0'",
				"Print Jobs Scheduling",
				"Print Jobs Scheduling with Signed DOS"
			};

			CombineAssertions(() =>
			{
				foreach (var binding in bindings
							.Where(binding => !string.IsNullOrEmpty(binding.QueueName)
											&& !binding.Table.Contains("Queue"))
							.OrderBy(binding => binding.Table)
							.ThenBy(binding => binding.QueueName))
				{
					var predicates = predicateFactory.GeneratePredicates(binding.Predicates, binding.Table);
					var predicatesCondition = string.Join(" AND ", predicates.Select(predicate => predicate.ParameterizedSqlCondition));

					// check every index. If one is compatible with FORCESEEK, all good.
					// if one is able to be used and is still a scan, all good?

					var indexes = new List<(string IndexName, bool HasFilter)>();
					Db.Connection.ExecuteReader($"select name, has_filter from sys.indexes where object_id = object_id('{binding.Table}', 'U')",
						datarecord =>
						{
							indexes.Add((IndexName: datarecord.GetString(0), HasFilter: datarecord.GetBoolean(1)));
						});

					// Act
					var result = indexes
						.Select(index =>
						{
							var forceSeekConditionally = index.HasFilter ? "" : ", FORCESEEK";
							var statement = $"SELECT COUNT(*) from {binding.Table} WITH (INDEX({index.IndexName}){forceSeekConditionally}) WHERE {predicatesCondition}";
							return statement;
						})
						.Any(statement =>
						{
							try
							{
								_ = Db.Connection.ExecuteScalar<int>(statement, cmd =>
									predicates.ForEach(predicate =>
										predicate.Parameters.ForEach(param =>
											cmd.AddParameter(param.ParameterName, param.Type, param.Size, param.Value))));
								return true;
							}
							catch (SqlException ex) when (ex.Errors[0].Number == 8622) // Query process could not produce a query plan
							{
								return false;
							}
						});

					// Assert
					if (!result && !baselineExcludedQueueNames.Contains(binding.QueueName))
					{
						Fail($"Failed to find an optimized index on '{binding.Table}' for queue '{binding.QueueName}' and query of 'SELECT COUNT(*) FROM {binding.Table} WHERE {predicatesCondition}'. Make sure that your nudge predicates or queue provider is able to use an index - tweak the predicates or request a new index.");
					}
				}
			});
		}

		public void TestAllClientAssembliesHaveProperBinding()
		{
			// Arrange
			var clients = Enum.GetValues(typeof(Clients))
				.Cast<Clients>()
				.ToArray();

			foreach (var client in clients)
			{
				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(client))
				{
					var nudgingSchemaResolver = new NudgingSchemaResolver(new EnterpriseSchemaResolver());
					var predicateFactory = new PredicateFactory(nudgingSchemaResolver);
					var bindings = new HostedServiceBusinessObjectBindingsProvider()
						.BusinessObjectBindings;

					// Act
					var result = bindings
						.Select(binding => TryGeneratePredicate(binding, predicateFactory))
						.Where(tuple => !tuple.resolved)
						.Select(tuple =>
							$"Service task [{tuple.binding.ServiceTaskCode}] queue name [{tuple.binding.QueueName}] binding error: {tuple.message}");

					// Assert
					AssertContainsExactElementsInAnyOrder($"Client [{client}] has binding errors",
						Enumerable.Empty<string>(), result);
				}
			}

			(bool resolved, IHostedServiceBusinessObjectBinding binding, string message) TryGeneratePredicate(
				IHostedServiceBusinessObjectBinding binding, IPredicateFactory predicateFactory)
			{
				try
				{
					_ = predicateFactory.GeneratePredicates(binding.Predicates, binding.Table)
						?? throw new ApplicationException("Generated predicate is null");
					return (true, binding, null);
				}
				catch (Exception exception)
				{
					return (false, binding, exception.Message);
				}
			}
		}

		public void TestBusinessObjectBindingsAlwaysGetsTheSameObject()
		{
			// Arrange
			var hostedServiceBusinessObjectBindingsProvider = HostedServiceBusinessObjectBindingsProvider.Instance;
			var businessObjectBindingsEnumerable1 = hostedServiceBusinessObjectBindingsProvider.BusinessObjectBindings;

			// Act
			var businessObjectBindingsEnumerable2 = hostedServiceBusinessObjectBindingsProvider.BusinessObjectBindings;

			// Assert
			Assert(ReferenceEquals(businessObjectBindingsEnumerable1, businessObjectBindingsEnumerable2));
		}

		public void TestBusinessObjectBindingsIsCollection()
		{
			// Arrange
			var hostedServiceBusinessObjectBindingsProvider = HostedServiceBusinessObjectBindingsProvider.Instance;

			// Act
			var businessObjectBindingsEnumerable = hostedServiceBusinessObjectBindingsProvider.BusinessObjectBindings;

			// Assert
			Assert(businessObjectBindingsEnumerable is ICollection<IHostedServiceBusinessObjectBinding>);
		}

		public void TestInstanceReturnsSingleton()
		{
			// Arrange
			var hostedServiceBusinessObjectBindingsProvider1 = HostedServiceBusinessObjectBindingsProvider.Instance;

			// Act
			var hostedServiceBusinessObjectBindingsProvider2 = HostedServiceBusinessObjectBindingsProvider.Instance;

			// Assert
			Assert(ReferenceEquals(hostedServiceBusinessObjectBindingsProvider1, hostedServiceBusinessObjectBindingsProvider2));
		}

		public void TestGetBindings()
		{
			var subProvider1Mock = Mock.Of<IHostedServiceBusinessObjectBindingsSubProvider>(
				s => s.BusinessObjectBindings == new[] { Mock.Of<IHostedServiceBusinessObjectBinding>() });
			var subProvider2Mock = Mock.Of<IHostedServiceBusinessObjectBindingsSubProvider>(
				s => s.BusinessObjectBindings == new[] { Mock.Of<IHostedServiceBusinessObjectBinding>(), Mock.Of<IHostedServiceBusinessObjectBinding>() });

			using (ObjectFactory.Substitute("HostedServiceBusinessObjectBindingsSubProviders", new IHostedServiceBusinessObjectBindingsSubProvider[] { subProvider1Mock, subProvider2Mock }))
			{
				var hostedServiceBusinessObjectBindingProvider = HostedServiceBusinessObjectBindingsProvider.Instance;
				var hostedServiceBusinessObjectBindingsProvider = hostedServiceBusinessObjectBindingProvider;

				AssertNotNull(hostedServiceBusinessObjectBindingsProvider);

				var bindings = hostedServiceBusinessObjectBindingsProvider.BusinessObjectBindings.ToArray();
				AssertEquals("Bindings Count", 3, bindings.Length);
			}
		}
	}
}
