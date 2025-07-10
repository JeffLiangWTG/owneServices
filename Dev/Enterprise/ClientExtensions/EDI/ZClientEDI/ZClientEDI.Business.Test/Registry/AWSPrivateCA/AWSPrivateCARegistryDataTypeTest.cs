using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business
{
	[TestedType(typeof(AWSPrivateCARegistryDataType))]
	class AWSPrivateCARegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AWSPrivateCARegistryDataType>
	{
		protected override AWSPrivateCARegistryDataType GetNewDataType()
		{
			return new AWSPrivateCARegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "AWSPrivateCARegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var cAArnCollection = new AWSPrivateCACollection();
			cAArnCollection.Add(new AWSPrivateCA()
			{
				IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust,
				Arn = "pc:ca:arn",
				IsEnabled = ZBool.True,
				AccessKey = "Test1",
				SecretKey = "Test1"
			});

			var cAArnCollection1 = new AWSPrivateCACollection();
			cAArnCollection1.Add(new AWSPrivateCA()
			{
				IssuingCA = CARootCodeDescriptionList.Codes.Adaptor,
				Arn = "pc:ca:arn1",
				IsEnabled = ZBool.True,
				AccessKey = "Test2",
				SecretKey = "Test2"
			});

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(cAArnCollection,DataType.Serialise(cAArnCollection)),
				new ValidSampleAndBinaryValueInDB(cAArnCollection1,DataType.Serialise(cAArnCollection1))
			};
		}
	}
}
