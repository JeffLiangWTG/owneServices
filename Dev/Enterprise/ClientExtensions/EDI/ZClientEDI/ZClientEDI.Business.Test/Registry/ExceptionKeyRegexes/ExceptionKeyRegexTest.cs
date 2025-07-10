using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ExceptionKeyRegex))]
	internal class ExceptionKeyRegexTest : RegistryBusinessObjectTemplateTestCase<ExceptionKeyRegex>
	{
		public void TestValidation_RegexIsNotEmpty()
		{
			var regex = new ExceptionKeyRegex();
			regex.ValidateRegex();
			Assert(regex.HasErrors);
			AssertHasError(regex.RegexInfo, "Should have Regular Expression.");

			regex = new ExceptionKeyRegex() { Regex = "TestRegex", Description = "TestDesc" };
			regex.ValidateRegex();
			AssertNoErrors(regex.RegexInfo);
		}

		public void TestValdation_RegexInvalid()
		{
			var collection = new ExceptionKeyRegexCollection(null, null);
			var regex = collection.AddNew();
			regex.Regex = @"Test(";
			Assert(regex.HasErrors);
			AssertHasError(regex.RegexInfo, "Invalid Regular Expression.");

			regex.Regex = @"Test\(";
			AssertNoErrors(regex.RegexInfo);
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

		protected override ExceptionKeyRegex GetBusinessObjectToClone()
		{
			return new ExceptionKeyRegex(
				new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override ExceptionKeyRegex GetBusinessObjectToSerialise()
		{
			return new ExceptionKeyRegex() { Regex = "TestRegex", Description = "TestDesc" };
		}

		#endregion
	}
}
