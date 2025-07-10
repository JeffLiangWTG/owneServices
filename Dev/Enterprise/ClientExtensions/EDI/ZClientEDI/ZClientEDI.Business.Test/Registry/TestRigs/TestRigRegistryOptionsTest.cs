using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TestRigRegistryOptions))]
	public class TestRigRegistryOptionsTest : RegistryBusinessObjectTemplateTestCase<TestRigRegistryOptions>
	{
		public void TestMaxLength()
		{
			var options = new TestRigRegistryOptions();

			AssertNoExceptionThrown(() => options.Product = "AAA");
			AssertNoExceptionThrown(() => options.ProductArea = "AAA");
			AssertNoExceptionThrown(() => options.Module = "AAA");
			AssertNoExceptionThrown(() => options.ChangeType = "AAA");
			AssertNoExceptionThrown(() => options.BackupFile = new string('A', 260));
			AssertNoExceptionThrown(() => options.AdditionalOptions = new string('A', 1000));

			AssertExceptionThrown<MaxLengthExceededException>(() => options.Product = "AAAA");
			AssertExceptionThrown<MaxLengthExceededException>(() => options.ProductArea = "AAAA");
			AssertExceptionThrown<MaxLengthExceededException>(() => options.Module = "AAAA");
			AssertExceptionThrown<MaxLengthExceededException>(() => options.ChangeType = "AAAA");
			AssertExceptionThrown<MaxLengthExceededException>(() => options.BackupFile = new string('A', 261));
			AssertExceptionThrown<MaxLengthExceededException>(() => options.AdditionalOptions = new string('A', 1001));

			ErrorReporter.Clear();
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override TestRigRegistryOptions GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override TestRigRegistryOptions GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		TestRigRegistryOptions NewPopulatedBusinessObject()
		{
			return new TestRigRegistryOptions(NewFallbackLevel(), Factory);
		}
	}
}
