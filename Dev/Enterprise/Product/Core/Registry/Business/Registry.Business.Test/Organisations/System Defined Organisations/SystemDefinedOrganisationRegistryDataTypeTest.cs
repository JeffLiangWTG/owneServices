using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinedOrganisationRegistryDataType))]
	sealed class SystemDefinedOrganisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SystemDefinedOrganisationRegistryDataType>
	{
		#region Implementation

		protected override SystemDefinedOrganisationRegistryDataType GetNewDataType()
		{
			return new SystemDefinedOrganisationRegistryDataType(typeof(UnmatchedOrganisation));
		}

		protected override string ExpectedEditorName
		{
			get { return "SystemDefinedOrganisationRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);
			AssertEquals("LastSavedOrganisation", ((SystemDefinedOrganisation)lhs).LastSavedOrganisation, ((SystemDefinedOrganisation)rhs).LastSavedOrganisation);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			SystemDefinedOrganisation unmatchedOrg = new UnmatchedOrganisation();

			unmatchedOrg.IsEnabled = true;
			unmatchedOrg.SetOrganisation(new ZGuid("C3F842EF-3BE5-448C-BED3-0017B232C624")); // This is the JAYSCH OrgHeader.
			unmatchedOrg.SetLastSavedOrganisation(unmatchedOrg.Organisation);

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,
				0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,
				0,63,0,62,0,60,0,85,0,110,0,109,0,97,0,116,0,99,0,104,0,101,0,100,0,79,0,114,0,103,0,97,0,110,0,105,0,115,
				0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,89,0,60,0,
				47,0,73,0,115,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,
				97,0,116,0,105,0,111,0,110,0,62,0,99,0,51,0,102,0,56,0,52,0,50,0,101,0,102,0,45,0,51,0,98,0,101,0,53,0,45,
				0,52,0,52,0,56,0,99,0,45,0,98,0,101,0,100,0,51,0,45,0,48,0,48,0,49,0,55,0,98,0,50,0,51,0,50,0,99,0,54,0,
				50,0,52,0,60,0,47,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,
				85,0,110,0,109,0,97,0,116,0,99,0,104,0,101,0,100,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,
				0,111,0,110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(unmatchedOrg, byteArrayValue)
			};
		}

		protected override void TearDown()
		{
			base.TearDown();
			SystemDefinedOrganisation.ClearCache();
		}

		#endregion
	}
}
