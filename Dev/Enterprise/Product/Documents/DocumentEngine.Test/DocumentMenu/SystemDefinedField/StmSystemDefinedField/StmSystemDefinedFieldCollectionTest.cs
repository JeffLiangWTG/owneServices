using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldCollection))]
	sealed class StmSystemDefinedFieldCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Load with Business Context and Country

		public void TestLoadWithBusinessContextAndRefCountry()
		{
			TestLoadWithBusinessContextAndCountry();
		}

		public void TestLoadWithBusinessContextAndCountryPK()
		{
			TestLoadWithBusinessContextAndCountry();
		}

		void TestLoadWithBusinessContextAndCountry()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedFieldSchema.Constants.TableName);

			BusinessContext shipmentContext = BusinessContext.Shipment;
			BusinessContext consolContext = BusinessContext.Consol;

			StmSystemDefinedField shipmentField = CreateField(shipmentContext, "ShipmentField");
			StmSystemDefinedField shipmentAUField = CreateField(shipmentContext, "ShipmentAUField");
			StmSystemDefinedField shipmentSGField = CreateField(shipmentContext, "ShipmentSGField");
			StmSystemDefinedField shipmentAUSuppressedField = CreateField(shipmentContext, "ShipmentAUSuppressedField");
			StmSystemDefinedField shipmentAUAndSGField = CreateField(shipmentContext, "ShipmentAUAndSGField");
			StmSystemDefinedField shipmentAUAndSGSuppressedField = CreateField(shipmentContext, "ShipmentAUAndSGSuppressedField");
			StmSystemDefinedField shipmentAUGridField = CreateField(shipmentContext, "ShipmentAUGridField");

			StmSystemDefinedField consolField = CreateField(consolContext, "ConsolField");
			StmSystemDefinedField consolAUField = CreateField(consolContext, "ConsolAUField");
			StmSystemDefinedField consolSGField = CreateField(consolContext, "ConsolSGField");
			StmSystemDefinedField consolAUSuppressedField = CreateField(consolContext, "ConsolAUSuppressedField");
			StmSystemDefinedField consolAUAndSGField = CreateField(consolContext, "ConsolAUAndSGField");
			StmSystemDefinedField consolAUAndSGSuppressedField = CreateField(consolContext, "ConsolAUAndSGSuppressedField");
			StmSystemDefinedField consolAUGridField = CreateField(consolContext, "ConsolAUGridField");

			AddFieldCountry(shipmentAUField, "AU", false);
			AddFieldCountry(shipmentSGField, "SG", false);
			AddFieldCountry(shipmentAUSuppressedField, "AU", true);
			AddFieldCountry(shipmentAUAndSGField, "AU", false);
			AddFieldCountry(shipmentAUAndSGField, "SG", false);
			AddFieldCountry(shipmentAUAndSGSuppressedField, "AU", true);
			AddFieldCountry(shipmentAUAndSGSuppressedField, "SG", true);
			AddFieldCountry(shipmentAUGridField, "AU", false);

			AddFieldCountry(consolAUField, "AU", false);
			AddFieldCountry(consolSGField, "SG", false);
			AddFieldCountry(consolAUSuppressedField, "AU", true);
			AddFieldCountry(consolAUAndSGField, "AU", false);
			AddFieldCountry(consolAUAndSGField, "SG", false);
			AddFieldCountry(consolAUAndSGSuppressedField, "AU", true);
			AddFieldCountry(consolAUAndSGSuppressedField, "SG", true);
			AddFieldCountry(consolAUGridField, "AU", false);

			shipmentAUGridField.S1_Type = "GRD";
			consolAUGridField.S1_Type = "GRD";
			shipmentAUGridField.FieldColumns.AddNew();
			consolAUGridField.FieldColumns.AddNew();

			Factory.Save();

			StmSystemDefinedFieldCollection collection = new StmSystemDefinedFieldCollection(new BusinessObjectFactory());

			LoadCollection(collection, shipmentContext, "AU");
			AssertEquals("Count", 4, collection.Count);
			AssertEquals("Contains(ShipmentField)", true, collection.Contains(shipmentField));
			AssertEquals("Contains(ShipmentAUField)", true, collection.Contains(shipmentAUField));
			AssertEquals("Contains(ShipmentAUAndSGField)", true, collection.Contains(shipmentAUAndSGField));
			AssertEquals("Contains(ShipmentAUGridField)", true, collection.Contains(shipmentAUGridField));

			LoadCollection(collection, shipmentContext, "SG");
			AssertEquals("Count", 4, collection.Count);
			AssertEquals("Contains(ShipmentField)", true, collection.Contains(shipmentField));
			AssertEquals("Contains(ShipmentSGField)", true, collection.Contains(shipmentSGField));
			AssertEquals("Contains(ShipmentAUSuppressedField)", true, collection.Contains(shipmentAUSuppressedField));
			AssertEquals("Contains(ShipmentAUAndSGField)", true, collection.Contains(shipmentAUAndSGField));

			LoadCollection(collection, shipmentContext, "NZ");
			AssertEquals("Count", 3, collection.Count);
			AssertEquals("Contains(ShipmentField)", true, collection.Contains(shipmentField));
			AssertEquals("Contains(ShipmentAUSuppressedField)", true, collection.Contains(shipmentAUSuppressedField));
			AssertEquals("Contains(ShipmentAUAndSGSuppressedField)", true, collection.Contains(shipmentAUAndSGSuppressedField));

			LoadCollection(collection, consolContext, "AU");
			AssertEquals("Count", 4, collection.Count);
			AssertEquals("Contains(ConsolField)", true, collection.Contains(consolField));
			AssertEquals("Contains(ConsolAUField)", true, collection.Contains(consolAUField));
			AssertEquals("Contains(ConsolAUAndSGField)", true, collection.Contains(consolAUAndSGField));
			AssertEquals("Contains(ConsolAUGridField)", true, collection.Contains(consolAUGridField));

			LoadCollection(collection, consolContext, "SG");
			AssertEquals("Count", 4, collection.Count);
			AssertEquals("Contains(ConsolField)", true, collection.Contains(consolField));
			AssertEquals("Contains(ConsolSGField)", true, collection.Contains(consolSGField));
			AssertEquals("Contains(ConsolAUSuppressedField)", true, collection.Contains(consolAUSuppressedField));
			AssertEquals("Contains(ConsolAUAndSGField)", true, collection.Contains(consolAUAndSGField));

			LoadCollection(collection, consolContext, "NZ");
			AssertEquals("Count", 3, collection.Count);
			AssertEquals("Contains(ConsolField)", true, collection.Contains(consolField));
			AssertEquals("Contains(ConsolAUSuppressedField)", true, collection.Contains(consolAUSuppressedField));
			AssertEquals("Contains(ConsolAUAndSGSuppressedField)", true, collection.Contains(consolAUAndSGSuppressedField));
		}

		StmSystemDefinedField CreateField(BusinessContext businessContext, string name)
		{
			StmSystemDefinedField result = Factory.New<StmSystemDefinedField>();
			result.S1_BusinessContext = businessContext.ToString();
			result.S1_Name = name;
			return result;
		}

		void AddFieldCountry(StmSystemDefinedField field, ZString countryCode, bool isSuppressed)
		{
			StmSystemDefinedFieldCountry fieldCountry = field.FieldCountries.AddNew();
			fieldCountry.S1_RN_NKCntrySpecific = countryCode;
			fieldCountry.S1_IsSuppressed = isSuppressed;
		}

		void LoadCollection(StmSystemDefinedFieldCollection collection, BusinessContext businessContext, ZString countryCode)
		{
			collection.Load(businessContext, countryCode);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmSystemDefinedFieldCollection(Factory);
		}

		#endregion
	}
}
