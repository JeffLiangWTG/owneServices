using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	public abstract class OverrideLevelTestCase<T> : TestCaseWithFactory where T : IOverrideLevel
	{
		protected abstract void SetAtValue(IRegistryItem item, object value);
		protected abstract IEnumerable<IOverrideLevel> ExpectedChildren(T level);
		protected abstract IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel { get; }
		protected abstract IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel { get; }

		protected static IEnumerable<RegistryStorageFlags> AllStorageFlags
		{
			get { return Enum.GetValues(typeof(RegistryStorageFlags)).Cast<RegistryStorageFlags>(); }
		}

		protected static IRegistryItem RegistryItemWithStorage(RegistryStorageFlags storage)
		{
			return new IntRegistryItem(storage.ToString(), (NoResString)"", (NoResString)"", (NoResString)"", storage);
		}

		protected virtual void SetValueForFallback(IRegistryItem item, object value)
		{
			//System is a fallback to most nodes...
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		protected virtual T GetNewLevel()
		{
			return Activator.CreateInstance<T>();
		}

		public virtual void TestGetValueAtLevel()
		{
			var item = new StringRegistryItem("BLA", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
			var level = GetNewLevel();
			SetAtValue(item, "Alt");

			AssertEquals("Alt", level.GetValueOf(item));
		}

		public virtual void TestGetNearestFallback()
		{
			var item = new StringRegistryItem("BLA", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
			var level = GetNewLevel();

			SetValueForFallback(item, "Alt");
			AssertEquals("Alt", level.GetValueOf(item));
		}

		public virtual void TestCanSetValueOf()
		{
			var level = GetNewLevel();

			var itemsThatShouldButDont = ExampleItemsThatApplyAtThisLevel
				.Append(new StringRegistryItem("BLA", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue")) // RegistryStorageFlags.All should always be true
				.Where(item => !level.CanSetValueOf(item))
				.Select(item => item.Storage)
				.ToList();

			var itemsThatShouldntButDo = ExampleItemsThatDoNotAtThisLevel
				.Where(item => level.CanSetValueOf(item))
				.Select(item => item.Storage)
				.ToList();

			CombineAssertions(() =>
			{
				Assert("Items that SHOULD apply but DONT: " + string.Join(", ", itemsThatShouldButDont), !itemsThatShouldButDont.Any());
				Assert("Items that SHOULD NOT apply but do: " + string.Join(", ", itemsThatShouldntButDo), !itemsThatShouldntButDo.Any());
			});
		}

		public void TestDescriptionIsNotNull()
		{
			Assert("Description should not be null", !string.IsNullOrWhiteSpace(GetNewLevel().Description));
		}

		public virtual void TestSetValue()
		{
			var item = new StringRegistryItem("BLA", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
			var level = GetNewLevel();

			level.SetValueOf(item, "My new value");
			AssertEquals("My new value", level.GetValueOf(item));
		}

		public void TestHasChildren()
		{
			var level = GetNewLevel();
			var expectedChildren = ExpectedChildren(level).ToList();
			var actualChildren = level.Children.Where(c => !(c is DepartmentOverrideLevel)).ToList();

			var comparer = new ComparerForTest();
			var foundNotExpected = actualChildren.Except(expectedChildren, comparer).ToList();
			var expectedNotFound = expectedChildren.Except(actualChildren, comparer).ToList();

			var failMessage = new Lazy<StringBuilder>();
			if (foundNotExpected.Any())
			{
				failMessage.Value.AppendLine("The following were found but not expected:");
				foundNotExpected.ForEach(overrideLevel => failMessage.Value.AppendLine(Format(overrideLevel)));
			}

			if (expectedNotFound.Any())
			{
				failMessage.Value.AppendLine("The following were expected but not found:");
				expectedNotFound.ForEach(overrideLevel => failMessage.Value.AppendLine(Format(overrideLevel)));
			}

			if (failMessage.IsValueCreated)
			{
				Fail(failMessage.Value.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		static string Format(IOverrideLevel level)
		{
			return string.Format("[{0}: '{1}']", level.GetType().Name, level.Description);
		}

		class ComparerForTest : IEqualityComparer<IOverrideLevel>
		{
			public bool Equals(IOverrideLevel x, IOverrideLevel y)
			{
				return x.GetType() == y.GetType() && x.Description == y.Description;
			}

			public int GetHashCode(IOverrideLevel obj)
			{
				return obj.GetType().GetHashCode() ^ obj.Description.GetHashCode();
			}
		}

		protected static ICompany CreateCompanyWithBranches(string companyName, params string[] branchNames)
		{
			return CreateCompanyWithBranches(companyName, Array.Empty<string>(), branchNames);
		}

		protected static ICompany CreateCompanyWithBranches(string companyName, string[] inactiveBranchNames, string[] activeBranchNames)
		{
			var company = new Mock<ICompany>();
			company.Setup(c => c.HumanReadableNameForRegistry).Returns(companyName);
			company.Setup(c => c.PK).Returns(Guid.NewGuid());

			var inactiveBranches = new List<IBranch>(inactiveBranchNames.Select(name => CreateBranch(company.Object, name)));
			var activeBranches = new List<IBranch>(activeBranchNames.Select(name => CreateBranch(company.Object, name)));

			company.Setup(c => c.Branches).Returns(activeBranches.Concat(inactiveBranches));
			company.Setup(c => c.ActiveBranches).Returns(activeBranches);

			return company.Object;
		}

		static IBranch CreateBranch(ICompany company, string name)
		{
			var branch = new Mock<IBranch>();
			branch.Setup(b => b.HumanReadableNameForRegistry).Returns(name);
			branch.Setup(b => b.PK).Returns(Guid.NewGuid());
			branch.Setup(b => b.Company).Returns(company);
			branch.Setup(b => b.CompanyPK).Returns(branch.Object.Company.PK);

			return branch.Object;
		}

		public virtual void TestGetPkOfEntry()
		{
			const string registyItemKey = "TESTGETPKOFENTRY";
			const string newValue = "DIFFERENT_VALUE";

			var level = GetNewLevel();
			var itemThatGetsSet = new StringRegistryItem(registyItemKey, (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");

			if (level.CanSetValueOf(itemThatGetsSet))
			{
				level.SetValueOf(itemThatGetsSet, newValue);

				var pkOfItem = level.GetPkOfEntry(itemThatGetsSet); // If you are expecting ZGuid.Empty, than you should override this test
				using (var command = Db.Connection.Command("SELECT SD_Name, SD_BinaryValue from dbo.StmData where SD_PK=@pk"))
				{
					command.AddParameterBasedOnDbColumn("@pk", pkOfItem, StmDataSchema.PK);
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						AssertEquals("The Pk we get back should be for the item we saved", reader[StmDataSchema.SD_Name.Name], registyItemKey);
						AssertArrayEqualsByElements("The Pk we get back should be for the item we saved", (byte[])reader[StmDataSchema.SD_BinaryValue.Name], new StringRegistryDataType().Serialise(newValue));
					}
				}
			}
			else
			{
				Fail("Cant set the registry item to test for GetPkOfEntry. Please override this method and write a custom test.");
			}
		}

		public void TestGetPkOfEntry_ShouldBeEmptyWhenUnset()
		{
			var item = new StringRegistryItem("MAH_KEY", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
			AssertEquals("The item was never set so there should not be anything to return", Guid.Empty, GetNewLevel().GetPkOfEntry(item));
		}
	}
}
