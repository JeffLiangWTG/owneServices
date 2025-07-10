using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HouseBillOfLadingTermsAndConditionsCollectionRegistryItem.HouseBillOfLadingTermsAndConditionsCollectionRegistryDataType))]
	sealed class HouseBillOfLadingTermsAndConditionsCollectionRegistryDataTypeTest : RegistryImageCollectionRegistryDataTypeTest
	{
		public void TestFallBackMergeValuesCore()
		{
			TestCaseHelper.ClearTable(StmDataSchema.Constants.TableName);

			HouseBillOfLadingTermsAndConditionsCollectionRegistryItem registryItem = new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem("", null, null, null, RegistryStorageFlags.All);

			HouseBillOfLadingTermsAndConditionsCollection systemCollection = new HouseBillOfLadingTermsAndConditionsCollection();

			HouseBillOfLadingTermsAndConditions systemElement1 = systemCollection.AddNew();
			HouseBillOfLadingTermsAndConditions systemElement2 = systemCollection.AddNew();
			HouseBillOfLadingTermsAndConditions systemElement3 = systemCollection.AddNew();
			HouseBillOfLadingTermsAndConditions systemElement4 = systemCollection.AddNew();
			HouseBillOfLadingTermsAndConditions systemElement5 = systemCollection.AddNew();

			systemElement1.Code = "AAA";
			systemElement1.Description = (NoResString)"AAA1";
			systemElement1.DeliveryMode = nameof(PrintCopyType.ALL);
			systemElement1.Image = new Bitmap(10, 10);

			systemElement2.Code = "AAA";
			systemElement2.Description = (NoResString)"AAA2";
			systemElement2.DeliveryMode = nameof(PrintCopyType.EML);
			systemElement2.Image = new Bitmap(10, 10);

			systemElement3.Code = "AAA";
			systemElement3.Description = (NoResString)"AAA3";
			systemElement3.DeliveryMode = nameof(PrintCopyType.FAX);
			systemElement3.Image = new Bitmap(10, 10);

			systemElement4.Code = "BBB";
			systemElement4.Description = (NoResString)"BBB1";
			systemElement4.DeliveryMode = nameof(PrintCopyType.ALL);
			systemElement4.Image = new Bitmap(10, 10);

			systemElement5.Code = "CCC";
			systemElement5.Description = (NoResString)"CCC1";
			systemElement5.DeliveryMode = nameof(PrintCopyType.ALL);
			systemElement5.Image = new Bitmap(10, 10);

			HouseBillOfLadingTermsAndConditionsCollection companyCollection = new HouseBillOfLadingTermsAndConditionsCollection();

			HouseBillOfLadingTermsAndConditions companyElement1 = companyCollection.AddNew();
			HouseBillOfLadingTermsAndConditions companyElement2 = companyCollection.AddNew();
			HouseBillOfLadingTermsAndConditions companyElement3 = companyCollection.AddNew();

			companyElement1.Code = "AAA";
			companyElement1.Description = (NoResString)"AAA1New";
			companyElement1.DeliveryMode = nameof(PrintCopyType.ALL);
			companyElement1.Image = new Bitmap(20, 20);

			companyElement2.Code = "AAA";
			companyElement2.Description = (NoResString)"AAA2New";
			companyElement2.DeliveryMode = nameof(PrintCopyType.PRN);
			companyElement2.Image = new Bitmap(20, 20);

			companyElement3.Code = "DDD";
			companyElement3.Description = (NoResString)"DDD1";
			companyElement3.DeliveryMode = nameof(PrintCopyType.ALL);
			companyElement3.Image = new Bitmap(20, 20);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemCollection);
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, companyCollection);

			HouseBillOfLadingTermsAndConditionsCollection obtainedCollection = registryItem.Value;
			AssertEquals("RegistryItem Count", 5, obtainedCollection.Count);

			HouseBillOfLadingTermsAndConditions obtainedElement1 = null;
			HouseBillOfLadingTermsAndConditions obtainedElement2 = null;
			HouseBillOfLadingTermsAndConditions obtainedElement3 = null;
			HouseBillOfLadingTermsAndConditions obtainedElement4 = null;
			HouseBillOfLadingTermsAndConditions obtainedElement5 = null;

			foreach (HouseBillOfLadingTermsAndConditions element in obtainedCollection)
			{
				if (element.Description == "BBB1")
				{
					obtainedElement1 = element;
				}

				if (element.Description == "CCC1")
				{
					obtainedElement2 = element;
				}

				if (element.Description == "AAA1New")
				{
					obtainedElement3 = element;
				}

				if (element.Description == "AAA2New")
				{
					obtainedElement4 = element;
				}

				if (element.Description == "DDD1")
				{
					obtainedElement5 = element;
				}
			}

			Assert("No ObtainedElements should be null", obtainedElement1 != null);
			Assert("No ObtainedElements should be null", obtainedElement2 != null);
			Assert("No ObtainedElements should be null", obtainedElement3 != null);
			Assert("No ObtainedElements should be null", obtainedElement4 != null);
			Assert("No ObtainedElements should be null", obtainedElement5 != null);

			AssertEquals("Code", "BBB", obtainedElement1.Code);
			AssertEquals("Description", "BBB1", obtainedElement1.Description);
			AssertEquals("Delivery Mode", nameof(PrintCopyType.ALL), obtainedElement1.DeliveryMode);
			Assert("Image should be equal", Utilities.IsImageEqual(new Bitmap(10, 10), obtainedElement1.Image));

			AssertEquals("Code", "CCC", obtainedElement2.Code);
			AssertEquals("Description", "CCC1", obtainedElement2.Description);
			AssertEquals("Delivery Mode", nameof(PrintCopyType.ALL), obtainedElement2.DeliveryMode);
			Assert("Image should be equal", Utilities.IsImageEqual(new Bitmap(10, 10), obtainedElement2.Image));

			AssertEquals("Code", "AAA", obtainedElement3.Code);
			AssertEquals("Description", "AAA1New", obtainedElement3.Description);
			AssertEquals("Delivery Mode", nameof(PrintCopyType.ALL), obtainedElement3.DeliveryMode);
			Assert("Image should be equal", Utilities.IsImageEqual(new Bitmap(20, 20), obtainedElement3.Image));

			AssertEquals("Code", "AAA", obtainedElement4.Code);
			AssertEquals("Description", "AAA2New", obtainedElement4.Description);
			AssertEquals("Delivery Mode", nameof(PrintCopyType.PRN), obtainedElement4.DeliveryMode);
			Assert("Image should be equal", Utilities.IsImageEqual(new Bitmap(20, 20), obtainedElement4.Image));

			AssertEquals("Code", "DDD", obtainedElement5.Code);
			AssertEquals("Description", "DDD1", obtainedElement5.Description);
			AssertEquals("Delivery Mode", nameof(PrintCopyType.ALL), obtainedElement5.DeliveryMode);
			Assert("Image should be equal", Utilities.IsImageEqual(new Bitmap(20, 20), obtainedElement5.Image));
		}

		#region Implementation

		protected override IRegistryDataType GetNewDataType()
		{
			return new HouseBillOfLadingTermsAndConditionsCollectionRegistryItem("", null, null, null, RegistryStorageFlags.All).DataType;
		}

		protected override string ExpectedEditorName
		{
			get { return "HouseBillOfLadingTermsAndConditionsCollectionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			HouseBillOfLadingTermsAndConditionsCollection collection = new HouseBillOfLadingTermsAndConditionsCollection();

			HouseBillOfLadingTermsAndConditions element1 = collection.AddNew();
			HouseBillOfLadingTermsAndConditions element2 = collection.AddNew();

			element1.Code = "aaa";
			element1.Description = (NoResString)"aaaa";
			element1.DeliveryMode = "ALL";
			element1.Image = new Bitmap(10, 10);
			element1.ImagePkForTest = new ZGuid("6309F992-2285-44a8-8EA4-258D0991683D");
			element1.FallbackKeyInDbForTest = "x";

			element2.Code = "bbb";
			element2.Description = (NoResString)"bbbb";
			element2.DeliveryMode = "ALL";
			element2.Image = new Bitmap(20, 20);
			element2.ImagePkForTest = new ZGuid("31375EC3-2F50-4e1c-8DB6-3BA79068E9B4");
			element2.FallbackKeyInDbForTest = "x";

			collection.SetFallbackKey("x");

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,
				0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,
				79,0,102,0,72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,
				0,115,0,65,0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,116,0,105,0,111,0,110,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,
				0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,
				0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,
				119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,
				72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,0,115,0,65,
				0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,116,0,105,0,111,0,110,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,97,0,97,0,97,0,60,
				0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,97,0,97,0,97,0,97,0,
				60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,
				0,77,0,111,0,100,0,101,0,62,0,65,0,76,0,76,0,60,0,47,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,77,0,111,0,100,0,101,0,62,
				0,60,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,54,0,51,0,48,0,57,0,102,0,57,0,57,0,50,0,45,0,50,0,50,0,56,0,53,0,45,0,52,0,52,
				0,97,0,56,0,45,0,56,0,101,0,97,0,52,0,45,0,50,0,53,0,56,0,100,0,48,0,57,0,57,0,49,0,54,0,56,0,51,0,100,0,60,0,47,0,73,0,109,0,97,
				0,103,0,101,0,80,0,75,0,62,0,60,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,120,0,60,0,47,0,70,0,97,0,108,
				0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,60,0,47,0,72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,
				97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,0,115,0,65,0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,116,0,105,0,111,0,110,0,
				115,0,62,0,60,0,72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,
				0,109,0,115,0,65,0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,116,0,105,0,111,0,110,0,115,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,
				98,0,98,0,98,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,
				98,0,98,0,98,0,98,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,68,0,101,0,108,0,105,0,
				118,0,101,0,114,0,121,0,77,0,111,0,100,0,101,0,62,0,65,0,76,0,76,0,60,0,47,0,68,0,101,0,108,0,105,0,118,0,101,0,114,0,121,0,77,0,
				111,0,100,0,101,0,62,0,60,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,51,0,49,0,51,0,55,0,53,0,101,0,99,0,51,0,45,0,50,0,102,0,
				53,0,48,0,45,0,52,0,101,0,49,0,99,0,45,0,56,0,100,0,98,0,54,0,45,0,51,0,98,0,97,0,55,0,57,0,48,0,54,0,56,0,101,0,57,0,98,0,52,0,
				60,0,47,0,73,0,109,0,97,0,103,0,101,0,80,0,75,0,62,0,60,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,120,0,
				60,0,47,0,70,0,97,0,108,0,108,0,98,0,97,0,99,0,107,0,75,0,101,0,121,0,62,0,60,0,47,0,72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,
				0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,0,115,0,65,0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,
				116,0,105,0,111,0,110,0,115,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,72,0,111,0,117,0,115,0,101,0,66,0,105,0,108,
				0,108,0,79,0,102,0,76,0,97,0,100,0,105,0,110,0,103,0,84,0,101,0,114,0,109,0,115,0,65,0,110,0,100,0,67,0,111,0,110,0,100,0,105,0,
				116,0,105,0,111,0,110,0,115,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
