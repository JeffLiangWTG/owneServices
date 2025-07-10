using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(DeleteEmptyCusEntryNumbers))]
	public class DeleteEmptyCusEntryNumbersTest : DataTransformationTestCase
	{
		readonly Guid ShipmentPK_1 = Guid.NewGuid();
		readonly Guid ShipmentPK_2 = Guid.NewGuid();

		readonly List<KeyValuePair<string, Guid>> CusEntryNumPKs = new List<KeyValuePair<string, Guid>>()
		{
			new KeyValuePair<string, Guid>("CUSAU_Empty", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_Empty", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("INSAU_Empty", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSAU_IssueDate", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSAU_ExpiryDate", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSAU_Default", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_Default", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumYes", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumNo1", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumNo2", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumNo3", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumNo4", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NumNo5", Guid.NewGuid()),
			new KeyValuePair<string, Guid>("CUSBE_NoType", Guid.NewGuid())
		};

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteEmptyCusEntryNumbers();
		}

		protected override void PrepareTestData()
		{
			Helper.CreateShipment(ShipmentPK_1, "S0000001", false);
			CreateCusEntryNum("CUSAU_Empty", ShipmentPK_1, "JobShipment", string.Empty, string.Empty, "AU", "CUS");
			CreateCusEntryNum("CUSBE_Empty", ShipmentPK_1, "JobShipment", string.Empty, string.Empty, "BE", "CUS");
			CreateCusEntryNum("INSAU_Empty", ShipmentPK_1, "JobShipment", string.Empty, string.Empty, "AU", "INS");
			CreateCusEntryNum("CUSAU_IssueDate", ShipmentPK_1, "JobShipment", string.Empty, string.Empty, "AU", "CUS", issueDate: new DateTime(2023, 3, 3));
			CreateCusEntryNum("CUSAU_ExpiryDate", ShipmentPK_1, "JobShipment", string.Empty, string.Empty, "AU", "CUS", expiryDate: new DateTime(2024, 4, 4));
			CreateCusEntryNum("CUSAU_Default", ShipmentPK_1, "JobShipment", string.Empty, "CAN", "AU", "CUS");
			CreateCusEntryNum("CUSBE_Default", ShipmentPK_1, "JobShipment", string.Empty, "PMT", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NumYes", ShipmentPK_1, "JobShipment", "696969", "IMP", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NumNo1", ShipmentPK_1, "JobShipment", string.Empty, "IMP", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NumNo2", ShipmentPK_1, "JobShipment", string.Empty, "IMP", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NoType", ShipmentPK_1, "JobShipment", "121212", string.Empty, "AU", "CUS");

			Helper.CreateShipment(ShipmentPK_2, "S0000002", false);
			CreateCusEntryNum("CUSBE_NumNo3", ShipmentPK_2, "JobShipment", string.Empty, "IMP", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NumNo4", ShipmentPK_2, "JobShipment", string.Empty, "IMP", "AU", "CUS");
			CreateCusEntryNum("CUSBE_NumNo5", ShipmentPK_2, "JobShipment", string.Empty, "IMP", "AU", "CUS");
		}

		protected override void AssertTransformationResults()
		{
			string[] GetCusEntryNumsInDb(Guid shipmentPK)
			{
				var sqlText = $"SELECT CE_PK, CE_Category, CE_EntryType, CE_EntryNum, CE_IssueDate, CE_ExpiryDate FROM dbo.CusEntryNum WHERE CE_ParentId = '{shipmentPK}';";
				return DataUtils.GetDataTableFromQuery(TestConnection, sqlText)
					.Rows
					.Cast<DataRow>()
					.Select(r => $"{GetCusEntryNumKeyFromPK((Guid)r[CusEntryNumSchema.Constants.PK])}")
					.ToArray();
			}

			var cusEntryNumsShipment1 = GetCusEntryNumsInDb(ShipmentPK_1);
			AssertContainsExactElementsInAnyOrder($"{nameof(ShipmentPK_1)}: CusEntryNum where all fields are empty has been deleted",
				new[]
				{
					"INSAU_Empty",
					"CUSAU_Default",
					"CUSBE_Default",
					"CUSBE_NumYes",
					"CUSBE_NoType"
				}, cusEntryNumsShipment1);

			var cusEntryNumsShipment2 = GetCusEntryNumsInDb(ShipmentPK_2);
			AssertEquals($"{nameof(ShipmentPK_2)}: CusEntryNum count mismatch, expecting only 1.", 1, cusEntryNumsShipment2.Length);
			AssertCollectionContains($"{nameof(ShipmentPK_2)}: The existing CusEntryNum should be one of the expected ones.", cusEntryNumsShipment2.Single(), new[] { "CUSBE_NumNo3", "CUSBE_NumNo4", "CUSBE_NumNo5" });
		}

		void CreateCusEntryNum(string key, Guid shipmentPK, string parentTable, string entryNum, string entryType, string countryCode, string category, DateTime? issueDate = null, DateTime? expiryDate = null)
		{
			var guid = CusEntryNumPKs.First(kv => kv.Key == key).Value;
			Helper.CreateCusEntryNum(guid, shipmentPK, parentTable, entryNum, entryType, countryCode, category, issueDate, expiryDate);
		}

		public string GetCusEntryNumKeyFromPK(Guid pk) => CusEntryNumPKs.FirstOrDefault(kv => kv.Value == pk).Key ?? pk.ToString();

		TransformationTestDataCreator Helper => helper ??= new TransformationTestDataCreator();
		TransformationTestDataCreator helper;
	}
}
