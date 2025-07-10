using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyRegexCollection))]
	internal class ExceptionKeyRegexCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ExceptionKeyRegexCollection>
	{
		public void TestGetExceptionKeyRegex()
		{
			var collection = new ExceptionKeyRegexCollection();
			collection.Add(new ExceptionKeyRegex() { Regex = @"regex1", Description = "Test Desc 1" });
			collection.Add(new ExceptionKeyRegex() { Regex = @"regex2", Description = "Test Desc 2" });

			AssertEquals("2 regexes", 2, collection.Count);
			AssertEquals("Test Desc 1", collection.Get("regex1").Description);
			AssertEquals("Test Desc 2", collection.Get("regex2").Description);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ExceptionKeyRegexCollection GetCollectionToTest()
		{
			return new ExceptionKeyRegexCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExceptionKeyRegex();
		}

		#endregion
	}
}
