using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Stubs;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class MapperTest : TestCase
	{
		public void TestAllDataTransformationsAreMapped()
		{
			var mappedTypes = GetMapper().AllTransformations
					.Select(x => x.GetType())
					.ToList();
			var transformationTypes = GetEndClassTransformationTypes<DataTransformation>(typeof(Mapper).Assembly)
					.ToList();

			var unmappedTypes = transformationTypes.Except(mappedTypes)
					.Except(ExcludedDataTransformationTypes)
					.ToList();
			var failureMessage = Invariant($@"The following {nameof(DataTransformation)} types are not mapped:
-------------------------------------------------------
{string.Join(System.Environment.NewLine, unmappedTypes)}
-------------------------------------------------------
>> If you have marked your type(s) as CodeAlive, please add to the {nameof(ExcludedDataTransformationTypes)} list below to temporarily exclude them.
");

			AssertNotEquals("No mapped transforms were found", 0, mappedTypes);
			AssertNotEquals("No transformation types were found", 0, transformationTypes.Count);
			AssertEquals(failureMessage, 0, unmappedTypes.Count);
		}

		public void TestExcludedDataTransformationTypesAreFromTheList()
		{
			var mappedTypes = GetMapper().AllTransformations
					.Select(x => x.GetType())
					.ToList();
			var mappedTypesNotRemovedFromExcludedList = mappedTypes.Intersect(ExcludedDataTransformationTypes).ToList();
			var cleanupMessage = Invariant($@"The following types from exclusions have already been mapped, please remove them from the {nameof(ExcludedDataTransformationTypes)} list:
-------------------------------------------------------
{string.Join(System.Environment.NewLine, mappedTypesNotRemovedFromExcludedList)}
");

			AssertEquals(cleanupMessage, 0, mappedTypesNotRemovedFromExcludedList.Count);
		}

		public void TestTransforms_ShouldBeUnique()
		{
			CombineAssertions("The same transform shouldn't be mapped twice. It's probably a mistake.", () =>
			{
				foreach (var group in GetMapper().AllTransformations.GroupBy(t => t.GetType()))
				{
					if (group.Count() > 1)
					{
						Fail(string.Format("The transformation type [{0}] has been mapped multiple times.", group.Key));
					}
				}
			});

			Assert(true);
		}

		public void TestNoTransformationMappedVersionIsGreaterThanCurrentTransformVersion()
		{
			var mapper = new Mapper(new DummyUpgradeManager());

			var aheadMappedTransformations = GetTransformationsMappedToVersionGreaterThanCurrentTransformVersion(mapper.AllTransformations);

			if (aheadMappedTransformations.Length > 0)
			{
				StringBuilder failMessage = new StringBuilder(
					string.Format("The following transformations are mapped to a version greater than the current ({1}):\r\n",
					SchemaVersion.Application.ToString()));

				foreach (var aheadMappedTransformation in aheadMappedTransformations)
				{
					failMessage.Append(string.Format("{0} - {1}\r\n",
						aheadMappedTransformation.Version.ToString().PadRight(9),
						aheadMappedTransformation.GetType().FullName));
				}

				failMessage.Append("\r\n\r\n");
				failMessage.Append("  ** ** ** ** ** ** ** ** ** **    A T T E N T I O N    ** ** ** ** ** ** ** ** ** **\r\n\r\n");
				failMessage.Append("  This test is a build-stopper and it MUST NOT be fixed by changing the mapper.\r\n\r\n");
				failMessage.Append("  The right way to fix it is by BUMPING the transform version.\r\n\r\n");
				failMessage.Append("\r\n\r\n");

				Fail(failMessage.ToString());
			}

			AssertEquals("There should be NO transformations mapped ahead", 0, aheadMappedTransformations.Length);
		}

		DataTransformation[] GetTransformationsMappedToVersionGreaterThanCurrentTransformVersion(DataTransformation[] allTransformationsFromMapper)
		{
			ArrayList resultList = new ArrayList();

			for (int i = 0; i < allTransformationsFromMapper.Length; i++)
			{
				if (allTransformationsFromMapper[i].Version.CompareTo(TransformationVersion.ApplicationNumber) > 0)
				{
					resultList.Add(allTransformationsFromMapper[i]);
				}
			}

			DataTransformation[] result = new DataTransformation[resultList.Count];
			resultList.CopyTo(result);

			return result;
		}

		public void TestExcludePreUpgradeTransformationTypesAreRemovedFromTheList()
		{
			var mappedTypes = GetMapper().AllTransformations
					.Select(x => x.GetType())
					.ToList();

			var mappedTypesNotRemovedFromExcludedList = mappedTypes.Intersect(ExcludedDataTransformationTypes).ToList();
			var cleanupMessage = Invariant($@"The following types from exclusions have already been mapped, please remove them from the {nameof(ExcludedDataTransformationTypes)} list:
-------------------------------------------------------
{string.Join(System.Environment.NewLine, mappedTypesNotRemovedFromExcludedList)}
");

			AssertEquals(cleanupMessage, 0, mappedTypesNotRemovedFromExcludedList.Count);
		}

		public void TestGetOrderedTransformations_TransformationVersionsAreInAscendingOrder()
		{
			var mapper = GetMapper();
			var orderedTransformations = mapper.GetOrderedTransformations();
			var sortedTransformations = orderedTransformations.OrderBy(t => t.Version);

			var expected = string.Join(System.Environment.NewLine, orderedTransformations.Cast<DataTransformation>().Select(t => t.UserDescription));
			var actual = string.Join(System.Environment.NewLine, sortedTransformations.Select(t => t.UserDescription));
			AssertMultilineASCIIEquals("Transformations should be sorted by Major and then Minor version.", expected, actual);
		}

		public void TestGetOrderedTransformations_TransformationWithSameVersionAreRunInDeterministicOrder()
		{
			var mapper = GetMapper();
			var orderedTransformations = mapper.GetOrderedTransformations();

			CombineAssertions("Order of transformations with same version number should not change from the reverse order they are in the mapper.", () =>
			{
				for (int i = 1; i < orderedTransformations.Length; i++)
				{
					if (orderedTransformations[i - 1].Version.Major == orderedTransformations[i].Version.Major &&
						orderedTransformations[i - 1].Version.Minor == orderedTransformations[i].Version.Minor &&
						Array.IndexOf(mapper.AllTransformations, orderedTransformations[i - 1]) < Array.IndexOf(mapper.AllTransformations, orderedTransformations[i]))
					{
						Fail($"The transformation type [{orderedTransformations[i]}] is being run out of order.");
					}
				}
			});

			Assert(true);
		}

		static readonly Version BaseLineTransformationVersion = new Version(7459, 0);

		/// <summary>
		/// Put a type to this exclusion list as a placeholder if it is planned for the type to be mapped later;
		/// remove the type from the exclusion list at the same time when the type is mapped for production code.
		/// </summary>
		static readonly IEnumerable<Type> ExcludedDataTransformationTypes = new []
		{
			typeof(RenameJK_RH_NKRatingCommodityCodeToJK_RH_NKConsolCommodity),
			typeof(InsertStmALogFromDTUReturnHistory),
			typeof(UpdateReceiveTransportationUnitTransportProviderIsKnown),
		};

		static Mapper GetMapper()
		{
			return new Mapper(new DummyUpgradeManager());
		}

		public void TestGetEndClassTransformationTypes()
		{
			// Arrange
			// Act
			// Assert
			AssertCollectionContains(
				typeof(TestDataTransformation),
				GetEndClassTransformationTypes<DataTransformation>(typeof(TestDataTransformation).Assembly));
		}

		static IEnumerable<Type> GetEndClassTransformationTypes<T>(Assembly assembly) where T : DataTransformation
		{
			return assembly.GetTypes()
				.Where(type => !type.IsInterface)
				.Where(type => !type.IsAbstract)
				.Where(type => typeof(DataTransformation).IsAssignableFrom(type))
				.Where(type => type.FullName.StartsWith("Enterprise.DbUpgrader.Transformation", StringComparison.OrdinalIgnoreCase))
				.Where(type => !type.FullName.EndsWith("Test", StringComparison.OrdinalIgnoreCase))
				.Where(type => type.FullName.IndexOf(".Test.", StringComparison.OrdinalIgnoreCase) < 0)
				.Where(type => type.FullName.IndexOf("Testing", StringComparison.OrdinalIgnoreCase) < 0);
		}
	}
}

namespace Enterprise.DbUpgrader.Transformation.DataModification.Stubs
{
	public class TestDataTransformation : DataTransformation
	{
		public override string UserDescription => "test";
	}
}
