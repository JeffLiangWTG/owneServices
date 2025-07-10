using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUseRegistryDataType))]
	sealed class SupplyChainSecurityOrganisationToUseRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SupplyChainSecurityOrganisationToUseRegistryDataType>
	{
		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override string ExpectedEditorName
		{
			get { return "SupplyChainSecurityOrganisationToUseRegistryItemEditor"; }
		}

		protected override SupplyChainSecurityOrganisationToUseRegistryDataType GetNewDataType()
		{
			return new SupplyChainSecurityOrganisationToUseRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			SupplyChainSecurityOrganisationToUseCollection collection = new SupplyChainSecurityOrganisationToUseCollection();
			SupplyChainSecurityOrganisationToUse element = collection.AddNew();
			element.Code = "ABC";
			element.Description = (NoResString)"XYZ";
			element.ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;

			byte[] byteArray = new byte[]
				{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,83,0,117,0,112,0,112,0,108,0,121,0,67,0,104,0,97,0,105,0,110,0,83,0,101,0,99,0,117,0,114,0,105,0,116,0,121,
				0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,84,0,111,0,85,0,115,0,101,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,
				0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,
				0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,
				0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,83,0,117,0,112,0,112,0,108,0,121,0,67,0,104,0,97,0,105,0,110,0,83,0,101,0,99,
				0,117,0,114,0,105,0,116,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,84,0,111,0,85,0,115,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,
				0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,65,0,66,0,67,0,60,0,47,
				0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,88,0,89,0,90,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,
				0,110,0,62,0,60,0,86,0,97,0,108,0,105,0,100,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,87,0,65,0,82,0,78,0,60,0,47,0,86,0,97,0,108,0,105,0,100,0,97,0,116,0,105,0,111,
				0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,47,0,83,0,117,0,112,0,112,0,108,0,121,0,67,0,104,0,97,0,105,0,110,0,83,0,101,0,99,0,117,0,114,0,105,0,116,0,121,0,79,0,114,0,103,0,97,0,110,0,105,
				0,115,0,97,0,116,0,105,0,111,0,110,0,84,0,111,0,85,0,115,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,83,0,117,0,112,0,112,0,108,0,121,0,67,0,104,0,97,0,105,0,110,0,83,
				0,101,0,99,0,117,0,114,0,105,0,116,0,121,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,84,0,111,0,85,0,115,0,101,0,62,0
				};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArray)
			};
		}
	}
}
