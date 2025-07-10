using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ENettRegistrationRegistryDataType))]
	class ENettRegistrationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ENettRegistrationRegistryDataType>
	{
		#region Implementation

		protected override ENettRegistrationRegistryDataType GetNewDataType()
		{
			return new ENettRegistrationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ENettRegistrationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			EnettRegistrationCode item = new EnettRegistrationCode() { AuthenticationCode = "authcode", RegistrationCode = "regcode", OrganisationPK = new ZGuid("A232D857-8C3E-4FDB-BA32-12EDED63EA22") };

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,69,0,110,0,101,0,116,0,116,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,82,0,101,
0,103,0,105,0,115,0,116,0,114,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,114,0,101,0,103,0,99,0,111,0,100,0,101,0,60,0,47,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,97,0,116,
0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,65,0,117,0,116,0,104,0,101,0,110,0,116,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,97,0,117,0,116,0,104,0,99,
0,111,0,100,0,101,0,60,0,47,0,65,0,117,0,116,0,104,0,101,0,110,0,116,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,
0,116,0,105,0,111,0,110,0,80,0,75,0,62,0,97,0,50,0,51,0,50,0,100,0,56,0,53,0,55,0,45,0,56,0,99,0,51,0,101,0,45,0,52,0,102,0,100,0,98,0,45,0,98,0,97,0,51,0,50,0,45,0,49,0,50,
0,101,0,100,0,101,0,100,0,54,0,51,0,101,0,97,0,50,0,50,0,60,0,47,0,79,0,114,0,103,0,97,0,110,0,105,0,115,0,97,0,116,0,105,0,111,0,110,0,80,0,75,0,62,0,60,0,47,0,69,0,110,0,101,0,116,
0,116,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,97,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(item, byteArrayValue)
			};
		}

		#endregion
	}
}
