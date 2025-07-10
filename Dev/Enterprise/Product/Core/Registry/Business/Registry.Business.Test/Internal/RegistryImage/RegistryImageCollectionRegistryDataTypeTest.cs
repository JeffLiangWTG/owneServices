using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryImageCollectionRegistryItem.RegistryImageCollectionRegistryDataType))]
	class RegistryImageCollectionRegistryDataTypeTest : FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase
	{
		#region Implementation

		protected override IRegistryDataType GetNewDataType()
		{
			return new RegistryImageCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System).DataType;
		}

		protected override string ExpectedEditorName
		{
			get { return "RegistryImageCollectionRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			RegistryImage lhsImage = (RegistryImage)lhs;
			RegistryImage rhsImage = (RegistryImage)rhs;

			AssertEquals("imagePK", lhsImage.ImagePkForTest, rhsImage.ImagePkForTest);
			AssertEquals("fallbackKeyInDb", lhsImage.FallbackKeyInDbForTest, rhsImage.FallbackKeyInDbForTest);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			RegistryImageCollection collection = new RegistryImageCollection();

			RegistryImage image1 = collection.AddNew();
			RegistryImage image2 = collection.AddNew();

			image1.Code = "123";
			image2.Code = "abc";

			image1.Description = (NoResString)"123 desc";
			image2.Description = (NoResString)"abc desc";

			image1.Image = new Bitmap(1, 1);
			image2.Image = new Bitmap(2, 2);

			image1.ImagePkForTest = new ZGuid("6309F992-2285-44a8-8EA4-258D0991683D");
			image2.ImagePkForTest = new ZGuid("31375EC3-2F50-4e1c-8DB6-3BA79068E9B4");

			image1.FallbackKeyInDbForTest = "x";
			image2.FallbackKeyInDbForTest = "x";

			collection.SetFallbackKey("x");

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,32,0,120,0,109,0,108,0,110,0,115,
				0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,
				0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,
				0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,82,0,101,0,103,0,105,
				0,115,0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,
				0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,49,0,50,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,
				0,116,0,105,0,111,0,110,0,62,0,49,0,50,0,51,0,32,0,100,0,101,0,115,0,99,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,109,0,97,0,103,0,101,
				0,80,0,75,0,62,0,54,0,51,0,48,0,57,0,102,0,57,0,57,0,50,0,45,0,50,0,50,0,56,0,53,0,45,0,52,0,52,0,97,0,56,0,45,0,56,0,101,0,97,0,52,0,45,0,50,0,53,0,56,0,100,0,48,0,57,
				0,57,0,49,0,54,0,56,0,51,0,100,0,60,0,47,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,60,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,120,0,60,0,47,0,70,
				0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,60,0,47,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,82,0,101,0,103,0,105,0,115,
				0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,67,0,111,0,100,0,101,0,77,0,97,0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,51,0,60,0,47,0,67,0,111,0,100,0,101,0,77,0,97,
				0,120,0,76,0,101,0,110,0,103,0,116,0,104,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,97,0,98,0,99,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,
				0,105,0,111,0,110,0,62,0,97,0,98,0,99,0,32,0,100,0,101,0,115,0,99,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,73,0,109,0,97,0,103,0,101,0,80,
				0,75,0,62,0,51,0,49,0,51,0,55,0,53,0,101,0,99,0,51,0,45,0,50,0,102,0,53,0,48,0,45,0,52,0,101,0,49,0,99,0,45,0,56,0,100,0,98,0,54,0,45,0,51,0,98,0,97,0,55,0,57,0,48,0,54,
				0,56,0,101,0,57,0,98,0,52,0,60,0,47,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,60,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,120,0,60,0,47,0,70,0,97,
				0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,60,0,47,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,
				0,79,0,102,0,82,0,101,0,103,0,105,0,115,0,116,0,114,0,121,0,73,0,109,0,97,0,103,0,101,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
