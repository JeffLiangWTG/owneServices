using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ILDatabaseValidationHelperTest : TestCaseWithFactory
	{
		public void TestGetMandatoryFieldsInZZ()
		{
			Assert("GetMandatoryFieldsInZZ must be false", !databaseValidationHelper.GetMandatoryFieldsInZZExposed);
		}

		public void TestGetMandatoryFields()
		{
			var mandatoryFields = databaseValidationHelper.GetMandatoryFieldsExposed();
			AssertNotNull("Mandatory fields must not be null", mandatoryFields);
			Assert("Mandatory fields must contain 'Seal'", mandatoryFields.ContainsKey("Seal"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			databaseValidationHelper = new ILDatabaseValidationHelperForTest(header);
		}

		ILDatabaseValidationHelperForTest databaseValidationHelper;
	}

	class ILDatabaseValidationHelperForTest : ILDatabaseValidationHelper
	{
		public ILDatabaseValidationHelperForTest(AsycudaManifestHeader header) : base(header)
		{
		}

		internal bool GetMandatoryFieldsInZZExposed => GetMandatoryFieldsInZZ;

		internal Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsExposed() => GetMandatoryFieldsCore();
	}
}
