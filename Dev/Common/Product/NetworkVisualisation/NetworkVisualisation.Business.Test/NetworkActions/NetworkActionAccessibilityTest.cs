using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkActionAccessibilityTest : TestCase
	{
		public void TestConstructionWithIsAllowedAndExplanation()
		{
			var entity = new Entity();
			var applicable = new NetworkActionAccessibility(true, entity, () => "Some explanation");
			AssertAllowed(applicable);

			var nonApplicable = new NetworkActionAccessibility(false, entity, () => "Some explanation");
			AssertNotAllowedWithSingleReason(entity, "Some explanation", nonApplicable);
		}

		public void TestConstructionWithIsAllowedAndReason()
		{
			var entity = new Entity();
			var expectedReason = new NetworkActionDenialReason(entity, "Some explanation");

			var applicable = new NetworkActionAccessibility(true, () => expectedReason);
			AssertAllowed(applicable);

			var nonApplicable = new NetworkActionAccessibility(false, () => expectedReason);
			AssertNotAllowedWithSingleReason(expectedReason, nonApplicable);
		}

		public void TestConstructionWithIsAllowedAndManyReasons()
		{
			var expectedReasons = new INetworkActionDenialReason[] { new NetworkActionDenialReason(new Entity(), "Some explanation 1"), new NetworkActionDenialReason(new Entity(), "Some explanation 2") };

			var applicable = new NetworkActionAccessibility(true, () => expectedReasons);
			AssertAllowed(applicable);

			var nonApplicable = new NetworkActionAccessibility(false, () => expectedReasons);
			AssertNotAllowedWithExactReasons(expectedReasons, nonApplicable);
		}

		public void TestConstructionWithIsAllowedAndManyReasons_ShouldThrowIfNotAllowedWithoutReason()
		{
			AssertExceptionThrown<ArgumentException>("Should provide reasons of denial.", () => new NetworkActionAccessibility(false, denialReasonsProvider: null));
			AssertExceptionThrown<ArgumentException>("Should provide reasons of denial.", () => new NetworkActionAccessibility(false, denialReasonsProvider: () => null));
			AssertExceptionThrown<ArgumentException>("Should provide reasons of denial.", () => new NetworkActionAccessibility(false, denialReasonsProvider: () => Array.Empty<INetworkActionDenialReason>()));
			AssertExceptionThrown<ArgumentException>("Should provide reasons of denial.", () => new NetworkActionAccessibility(false, denialReasonsProvider: () => new INetworkActionDenialReason[] { null }));
			AssertExceptionThrown<ArgumentException>("Should provide reasons of denial.", () => new NetworkActionAccessibility(false, denialReasonsProvider: () => new INetworkActionDenialReason[] { null, null }));
		}

		public void TestConstructionWithIsAllowedAndManyReasons_ShouldIgnoreNulls()
		{
			var realReason1 = new NetworkActionDenialReason(new Entity(), "Some explanation 1");
			var realReason2 = new NetworkActionDenialReason(new Entity(), "Some explanation 2");
			var expectedReasons = new INetworkActionDenialReason[] {
				realReason1,
				null,
				null,
				realReason2
			};
			var nonApplicable = new NetworkActionAccessibility(false, () => expectedReasons);
			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 }, nonApplicable);
		}

		public void TestConstructionWithSingleReason()
		{
			var applicable = new NetworkActionAccessibility(denialReason: null);
			AssertAllowed(applicable);

			var expectedReason = new NetworkActionDenialReason(new Entity(), "Some explanation");
			var nonApplicable = new NetworkActionAccessibility(expectedReason);
			AssertNotAllowedWithSingleReason(expectedReason, nonApplicable);
		}

		public void TestConstructionWithJustReasons()
		{
			var applicable = new NetworkActionAccessibility(Array.Empty<INetworkActionDenialReason>());
			AssertAllowed(applicable);

			var expectedReasons = new INetworkActionDenialReason[] { new NetworkActionDenialReason(new Entity(), "Some explanation 1"), new NetworkActionDenialReason(new Entity(), "Some explanation 2") };
			var nonApplicable = new NetworkActionAccessibility(expectedReasons);
			AssertNotAllowedWithExactReasons(expectedReasons, nonApplicable);
		}

		public void TestConstructionWithJustReasons_ShouldIgnoreNulls()
		{
			var applicable = new NetworkActionAccessibility(new INetworkActionDenialReason[] { null, null });
			AssertAllowed(applicable);

			var realReason1 = new NetworkActionDenialReason(new Entity(), "Some explanation 1");
			var realReason2 = new NetworkActionDenialReason(new Entity(), "Some explanation 2");
			var reasonsWithNulls = new INetworkActionDenialReason[] {
					realReason1,
					null,
					null,
					realReason2
				};
			var nonApplicable = new NetworkActionAccessibility(reasonsWithNulls);
			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 }, nonApplicable);
		}

		public void TestUnion()
		{
			var realReason1 = new NetworkActionDenialReason(new Entity(), "Some explanation 1");
			var realReason2 = new NetworkActionDenialReason(new Entity(), "Some explanation 2");
			var realReason3 = new NetworkActionDenialReason(new Entity(), "Some explanation 3");
			var realReason4 = new NetworkActionDenialReason(new Entity(), "Some explanation 4");

			var reasonsToCombine1 = new INetworkActionDenialReason[] {
				realReason1,
				null,
				null,
				realReason2
			};

			var reasonsToCombine2 = new INetworkActionDenialReason[] {
				realReason3,
				null,
				realReason4,
				null
			};

			AssertAllowed(NetworkActionAccessibility.Allowed
				.Union(NetworkActionAccessibility.Allowed));

			AssertAllowed(NetworkActionAccessibility.Allowed
				.Union(new NetworkActionAccessibility(new INetworkActionDenialReason[] { null, null })));

			AssertNotAllowedWithSingleReason("Some explanation",
				NetworkActionAccessibility.Allowed
				.Union(new NetworkActionAccessibility(new NetworkActionDenialReason(new Entity(), "Some explanation"))));

			AssertNotAllowedWithSingleReason("Some explanation",
				new NetworkActionAccessibility(new NetworkActionDenialReason(new Entity(), "Some explanation"))
				.Union(NetworkActionAccessibility.Allowed));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 },
				NetworkActionAccessibility.Allowed
				.Union(new NetworkActionAccessibility(reasonsToCombine1)));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason3, realReason4 },
				new NetworkActionAccessibility(reasonsToCombine2)
				.Union(NetworkActionAccessibility.Allowed));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2, realReason3, realReason4 },
				new NetworkActionAccessibility(reasonsToCombine1)
				.Union(new NetworkActionAccessibility(reasonsToCombine2)));
		}

		public void TestUnionIfAllowed()
		{
			var realReason1 = new NetworkActionDenialReason(new Entity(), "Some explanation 1");
			var realReason2 = new NetworkActionDenialReason(new Entity(), "Some explanation 2");
			var realReason3 = new NetworkActionDenialReason(new Entity(), "Some explanation 3");
			var realReason4 = new NetworkActionDenialReason(new Entity(), "Some explanation 4");

			var reasonsToCombine1 = new INetworkActionDenialReason[] {
				realReason1,
				null,
				null,
				realReason2
			};

			var reasonsToCombine2 = new INetworkActionDenialReason[] {
				realReason3,
				null,
				realReason4,
				null
			};

			AssertAllowed(NetworkActionAccessibility.Allowed
				.UnionIfAllowed(() => NetworkActionAccessibility.Allowed));

			AssertAllowed(NetworkActionAccessibility.Allowed
				.UnionIfAllowed(() => new NetworkActionAccessibility(new INetworkActionDenialReason[] { null, null })));

			AssertNotAllowedWithSingleReason("Some explanation",
				NetworkActionAccessibility.Allowed
				.UnionIfAllowed(() => new NetworkActionAccessibility(new NetworkActionDenialReason(new Entity(), "Some explanation"))));

			AssertNotAllowedWithSingleReason("Some explanation",
				new NetworkActionAccessibility(new NetworkActionDenialReason(new Entity(), "Some explanation"))
				.UnionIfAllowed(() => NetworkActionAccessibility.Allowed));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 },
				NetworkActionAccessibility.Allowed
				.UnionIfAllowed(() => new NetworkActionAccessibility(reasonsToCombine1)));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason3, realReason4 },
				new NetworkActionAccessibility(reasonsToCombine2)
				.UnionIfAllowed(() => NetworkActionAccessibility.Allowed));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 },
				new NetworkActionAccessibility(reasonsToCombine1)
				.UnionIfAllowed(() => new NetworkActionAccessibility(reasonsToCombine2)));
		}

		public void TestJoin()
		{
			AssertAllowed(NetworkActionAccessibility.Join(null));
			AssertAllowed(NetworkActionAccessibility.Join(Array.Empty<INetworkActionAccessibility>()));
			AssertAllowed(NetworkActionAccessibility.Join(new INetworkActionAccessibility[] { NetworkActionAccessibility.Allowed }));
			AssertAllowed(NetworkActionAccessibility.Join(new INetworkActionAccessibility[] { NetworkActionAccessibility.Allowed, NetworkActionAccessibility.Allowed }));

			var realReason1 = new NetworkActionDenialReason(new Entity(), "Some explanation 1");
			var realReason2 = new NetworkActionDenialReason(new Entity(), "Some explanation 2");
			var realReason3 = new NetworkActionDenialReason(new Entity(), "Some explanation 3");
			var realReason4 = new NetworkActionDenialReason(new Entity(), "Some explanation 4");

			var result1 = new NetworkActionAccessibility(new INetworkActionDenialReason[] {
				realReason1,
				null,
				null,
				realReason2
			});

			var result2 = new NetworkActionAccessibility(new INetworkActionDenialReason[] {
				realReason3,
				null,
				realReason4,
				null
			});

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2 }, NetworkActionAccessibility.Join(new INetworkActionAccessibility[] { NetworkActionAccessibility.Allowed, result1 }));
			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason3, realReason4 }, NetworkActionAccessibility.Join(new INetworkActionAccessibility[] { result2, NetworkActionAccessibility.Allowed }));

			AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] { realReason1, realReason2, realReason3, realReason4 }, NetworkActionAccessibility.Join(new INetworkActionAccessibility[] { NetworkActionAccessibility.Allowed, result1, result2 }));
		}

		public void TestToString_ForMultipleReasons()
		{
			var entity1 = new Entity()
			{
				Name = "Entity1"
			};
			var entity2 = new Entity()
			{
				Name = "Entity2"
			};
			var accessibility = new NetworkActionAccessibility(new NetworkActionDenialReason[] {
					new NetworkActionDenialReason(entity1, "This can't be done"),
					new NetworkActionDenialReason(entity2, "Because it's against the law"),
				});

			AssertEquals(@"Entity1: This can't be done
Entity2: Because it's against the law", accessibility.ToString());
		}

		public void TestToString_WhenAllowed()
		{
			AssertEquals("", NetworkActionAccessibility.Allowed.ToString());
		}

		public void TestToString_ForDeletedEntities()
		{
			var entity = new Entity()
			{
				Name = "EntityToDelete",
				IsDeleted = true
			};
			var accessibility = new NetworkActionAccessibility(entity, "Cannot execute for deleted entity");

			AssertEquals("<deleted entity>: Cannot execute for deleted entity", accessibility.ToString());
		}

		#region Assertions

		public static void AssertAllowed(INetworkActionAccessibility result)
		{
			AssertAllowed(message: null, result);
		}

		public static void AssertAllowed(string message, INetworkActionAccessibility result)
		{
			message = GetAdornedMessage(message);
			AssertEquals($@"{message}Should be allowed but was denied with the following reasons:
				{ConvertReasonsToString(result.DenialReasons)}", true, result.IsAllowed);
			AssertNotNull($"{message}Reasons should not be null (should be an empty collection)", result.DenialReasons);
			Assert($@"{message}Should ignore reasons if there are any but the following reasons are found:
				{ConvertReasonsToString(result.DenialReasons)}", !result.DenialReasons.Any());
		}

		public static void AssertNotAllowedWithSingleReason(INetworkActionDenialReason expectedReason, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message: null, expectedReason, result);
		}

		public static void AssertNotAllowedWithSingleReason(string message, INetworkActionDenialReason expectedReason, INetworkActionAccessibility result)
		{
			message = GetAdornedMessage(message);
			AssertEquals($"{message}Should not be allowed", false, result.IsAllowed);
			AssertEquals($"{message}Should be a single reason", 1, result.DenialReasons.Count());
			AssertEquals($@"{message}Should have the expected reason but the following reasons are found:
				{ConvertReasonsToString(result.DenialReasons)}", expectedReason, result.DenialReasons.Single());
		}

		public static void AssertNotAllowedWithSingleReason(string expectedExplanation, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message: null, expectedExplanation, result);
		}

		public static void AssertNotAllowedWithSingleReason(string message, string expectedExplanation, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message, expectedEntity: null, expectedExplanation, result);
		}

		public static void AssertNotAllowedWithSingleReason(INetworkEntity expectedEntity, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message: null, expectedEntity, result);
		}

		public static void AssertNotAllowedWithSingleReason(string message, INetworkEntity expectedEntity, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message, expectedEntity, expectedExplanation: null, result);
		}

		public static void AssertNotAllowedWithSingleReason(INetworkEntity expectedEntity, string expectedExplanation, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithSingleReason(message: null, expectedEntity, expectedExplanation, result);
		}

		public static void AssertNotAllowedWithSingleReason(string message, INetworkEntity expectedEntity, string expectedExplanation, INetworkActionAccessibility result)
		{
			message = GetAdornedMessage(message);
			AssertEquals($"{message}Should not be allowed with the following explanation: {expectedExplanation}", false, result.IsAllowed);
			AssertEquals($@"{message}Should be a single reason
				{expectedExplanation}
				but the following reasons are found:
				{ConvertReasonsToString(result.DenialReasons)}", 1, result.DenialReasons.Count());

			var reason = result.DenialReasons.Single();

			if (expectedEntity != null)
			{
				AssertEquals($"{message}Should mention the expected entity in the denial reason", expectedEntity, reason.Entity);
			}

			if (expectedExplanation != null)
			{
				AssertEquals($"{message}Should contain the expected explanation in the denial reason", expectedExplanation, reason.Explanation);
			}
		}

		public static void AssertNotAllowedWithExactReasons(IEnumerable<INetworkActionDenialReason> expectedReasons, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message: null, expectedReasons, result);
		}

		public static void AssertNotAllowedWithExactReasons(string message, IEnumerable<INetworkActionDenialReason> expectedReasons, INetworkActionAccessibility result)
		{
			message = GetAdornedMessage(message);
			AssertEquals($"{message}Should not be allowed", false, result.IsAllowed);
			AssertContainsExactElementsInAnyOrder($"{message}Should have expected reasons", expectedReasons, result.DenialReasons);
		}

		public static void AssertNotAllowedWithExactReasons(IEnumerable<string> expectedExplanations, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message: null, expectedExplanations, result);
		}

		public static void AssertNotAllowedWithExactReasons(string message, IEnumerable<string> expectedExplanations, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message, expectedEntities: null, expectedExplanations, result);
		}

		public static void AssertNotAllowedWithExactReasons(IEnumerable<INetworkEntity> expectedEntities, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message: null, expectedEntities, result);
		}

		public static void AssertNotAllowedWithExactReasons(string message, IEnumerable<INetworkEntity> expectedEntities, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message, expectedEntities, expectedExplanations: null, result);
		}

		public static void AssertNotAllowedWithExactReasons(IEnumerable<INetworkEntity> expectedEntities, IEnumerable<string> expectedExplanations, INetworkActionAccessibility result)
		{
			AssertNotAllowedWithExactReasons(message: null, expectedEntities, expectedExplanations, result);
		}

		public static void AssertNotAllowedWithExactReasons(string message, IEnumerable<INetworkEntity> expectedEntities, IEnumerable<string> expectedExplanations, INetworkActionAccessibility result)
		{
			message = GetAdornedMessage(message);
			AssertEquals($"{message}Should not be allowed", false, result.IsAllowed);

			if (expectedExplanations != null)
			{
				AssertContainsExactElementsInAnyOrder($"{message}Should have expected reasons", expectedExplanations, result.DenialReasons.Select(r => r.Explanation));
			}

			if (expectedEntities != null)
			{
				AssertContainsExactElementsInAnyOrder($"{message}Should have expected reasons", expectedEntities, result.DenialReasons.Select(r => r.Entity));
			}
		}

		static string GetAdornedMessage(string message) => string.IsNullOrEmpty(message) ? string.Empty : message + ": ";

		static string ConvertReasonsToString(IEnumerable<INetworkActionDenialReason> reasons)
		{
			return string.Join(Environment.NewLine, reasons);
		}

		#endregion
	}
}
