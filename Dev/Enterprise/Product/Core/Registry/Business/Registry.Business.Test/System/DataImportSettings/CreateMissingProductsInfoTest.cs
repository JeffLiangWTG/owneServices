using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreateMissingProductsInfo))]
	sealed class CreateMissingProductsInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("FallbackRule", DefaultRalationshipCodes.Owner, CreateMissingProductsInfo.DefaultRelationship);
		}

		public void TestDefaultRelationship()
		{
			CreateMissingProductsInfo.IsOverrideToYes = true;
			CreateMissingProductsInfo.DefaultRelationship = DefaultRalationshipCodes.Both;
			CreateMissingProductsInfo.RunPreSaveValidation();
			AssertHasMessageError(CreateMissingProductsInfo.DefaultRelationshipInfo, "BTH – Both Owner and Supplier is no longer valid, please select either 'OWN – Owner' or 'SUP – Supplier'.");

			CreateMissingProductsInfo.IsOverrideToYes = false;
			CreateMissingProductsInfo.DefaultRelationship = DefaultRalationshipCodes.Owner;
			CreateMissingProductsInfo.RunPreSaveValidation();
			AssertNoMessageErrors(CreateMissingProductsInfo.DefaultRelationshipInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CreateMissingProductsInfo();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new CreateMissingProductsInfo();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		CreateMissingProductsInfo CreateMissingProductsInfo
		{
			get { return createMissingProductsInfo ?? (createMissingProductsInfo = new CreateMissingProductsInfo()); }
		}
		CreateMissingProductsInfo createMissingProductsInfo;

		#endregion
	}
}
