using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(DummyRegistryBusinessObject))]
	class TestRigRegistryOptionsValidationTest : ValidationTest
	{
		public void TestProduct()
		{
			options.Validation.ValidateProduct();
			AssertHasError(options.ProductInfo, "Please enter a Product.");

			options.Product = "1AA";
			AssertNoErrors(options.ProductInfo);

			options.Product = "2AA";
			AssertHasError(options.ProductInfo, "Enter a valid Product.");
		}

		public void TestProductArea()
		{
			options.Validation.ValidateProductArea();
			AssertHasError(options.ProductAreaInfo, "Please enter a Product Area.");

			options.ProductArea = "2AA";
			AssertNoErrors(options.ProductAreaInfo);

			options.ProductArea = "3AA";
			AssertHasError(options.ProductAreaInfo, "Enter a valid Product Area.");
		}

		public void TestModule()
		{
			options.Validation.ValidateModule();
			AssertNoErrors(options.ModuleInfo);

			options.Module = "3AA";
			AssertNoErrors(options.ModuleInfo);

			options.Module = "4AA";
			AssertHasError(options.ModuleInfo, "Enter a valid Module.");
		}

		public void TestChangeType()
		{
			options.Validation.ValidateChangeType();
			AssertNoErrors(options.ChangeTypeInfo);

			options.ChangeType = "4AA";
			AssertNoErrors(options.ChangeTypeInfo);

			options.ChangeType = "2AA";
			AssertHasError(options.ChangeTypeInfo, "Enter a valid Change Type.");
		}

		public void TestBackupFile()
		{
			options.Validation.ValidateBackupFile();
			AssertHasError(options.BackupFileInfo, "Please enter a Backup File.");

			options.BackupFile = "Invalid";
			AssertHasError(options.BackupFileInfo, @"Please enter a properly formatted file path to a .bak file (e.g. \\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\TeamFolder\backup.bak).");

			options.BackupFile = @"C:\dev\something.bak";
			AssertHasError(options.BackupFileInfo, @"Please enter a properly formatted file path to a .bak file (e.g. \\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\TeamFolder\backup.bak).");

			options.BackupFile = @"\\server\dev\something";
			AssertHasError(options.BackupFileInfo, @"Please enter a properly formatted file path to a .bak file (e.g. \\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\TeamFolder\backup.bak).");

			options.BackupFile = @"\\something.bak";
			AssertHasError(options.BackupFileInfo, @"Please enter a properly formatted file path to a .bak file (e.g. \\sydsp-ssql-1.sand.wtg.zone\SQL_Backups\TeamFolder\backup.bak).");

			options.BackupFile = @"\\server\something.bak";
			AssertNoErrors(options.BackupFileInfo);
		}

		public void TestDuplicateIdentifiers_ShouldHaveValidationErrors()
		{
			var header = new TestRigRegistryHeader(NewFallbackLevel(), Factory);
			var option1 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "1AA", ProductArea = "2AA", Module = "3AA", ChangeType = "4AA", BackupFile = @"\\lion.ted\cruz.bak" };
			var option2 = new TestRigRegistryOptions(NewFallbackLevel(), Factory) { Product = "1AA", ProductArea = "2AA", Module = "3AA", ChangeType = "4AX", BackupFile = @"\\crooked\hillary.bak" };

			header.OptionsCollection.Add(option1);
			header.OptionsCollection.Add(option2);
			header.RunPreSaveValidation();

			AssertNoErrors(option1.ProductInfo);
			AssertNoErrors(option1.ProductAreaInfo);
			AssertNoErrors(option1.ModuleInfo);
			AssertNoErrors(option1.ChangeTypeInfo);
			AssertNoErrors(option2.ProductInfo);
			AssertNoErrors(option2.ProductAreaInfo);
			AssertNoErrors(option2.ModuleInfo);
			AssertNoErrors(option2.ChangeTypeInfo);

			option2.ChangeType = "4AA";
			AssertExceptionThrown<RegistryValidationException>(() => header.RunPreSaveValidation());

			const string expectedError = "The Product/Product Area/Module/Change Type combination has already been specified. Each combination can only be specified once.";
			AssertHasError(option1.ProductInfo, expectedError);
			AssertHasError(option2.ProductInfo, expectedError);
			AssertHasError(option1.ProductAreaInfo, expectedError);
			AssertHasError(option2.ProductAreaInfo, expectedError);
			AssertHasError(option1.ModuleInfo, expectedError);
			AssertHasError(option2.ModuleInfo, expectedError);
			AssertHasError(option1.ChangeTypeInfo, expectedError);
			AssertHasError(option2.ChangeTypeInfo, expectedError);

			option1.ChangeType = "4AX";
			header.RunPreSaveValidation();

			AssertNoErrors(option1.ProductInfo);
			AssertNoErrors(option1.ProductAreaInfo);
			AssertNoErrors(option1.ModuleInfo);
			AssertNoErrors(option1.ChangeTypeInfo);
			AssertNoErrors(option2.ProductInfo);
			AssertNoErrors(option2.ProductAreaInfo);
			AssertNoErrors(option2.ModuleInfo);
			AssertNoErrors(option2.ChangeTypeInfo);

			option2.ChangeType = "4AX";
			option1.Module = string.Empty;
			option2.Module = string.Empty;
			AssertExceptionThrown<RegistryValidationException>(() => header.RunPreSaveValidation());

			AssertHasError(option1.ProductInfo, expectedError);
			AssertHasError(option2.ProductInfo, expectedError);
			AssertHasError(option1.ProductAreaInfo, expectedError);
			AssertHasError(option2.ProductAreaInfo, expectedError);
			AssertHasError(option1.ModuleInfo, expectedError);
			AssertHasError(option2.ModuleInfo, expectedError);
			AssertHasError(option1.ChangeTypeInfo, expectedError);
			AssertHasError(option2.ChangeTypeInfo, expectedError);
		}

		TestRigRegistryOptions options;

		protected override void SetUp()
		{
			base.SetUp();

			TestRigRegistryTestHelper.AddWorkItemTypes();
			options = new TestRigRegistryOptions(NewFallbackLevel(), Factory);
		}
	}
}
