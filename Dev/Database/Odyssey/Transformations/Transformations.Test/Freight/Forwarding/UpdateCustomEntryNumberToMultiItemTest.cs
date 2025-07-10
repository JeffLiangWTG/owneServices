using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(UpdateCustomEntryNumberToMultiItem))]
	public class UpdateCustomEntryNumberToMultiItemTest : DataTransformationTestCase
	{
		readonly string CusEntryNumTableName = CusEntryNumSchema.Constants.TableName;

		readonly Guid DocsAndCartagePK1 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK2 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK3 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK4 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK5 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK6 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK7 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK8 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK9 = Guid.NewGuid();
		readonly Guid DocsAndCartagePK10 = Guid.NewGuid();

		readonly Guid ShipmentPK1 = Guid.NewGuid();
		readonly Guid ShipmentPK2 = Guid.NewGuid();
		readonly Guid ShipmentPK3 = Guid.NewGuid();
		readonly Guid ShipmentPK4 = Guid.NewGuid();
		readonly Guid ShipmentPK5 = Guid.NewGuid();
		readonly Guid ShipmentPK6 = Guid.NewGuid();
		readonly Guid ShipmentPK7 = Guid.NewGuid();
		readonly Guid ShipmentPK8 = Guid.NewGuid();
		readonly Guid ShipmentPK9 = Guid.NewGuid();
		readonly Guid ShipmentPK10 = Guid.NewGuid();

		readonly Guid CusEntryPK1 = Guid.NewGuid();
		readonly Guid CusEntryPK2 = Guid.NewGuid();
		readonly Guid CusEntryPK3 = Guid.NewGuid();
		readonly Guid CusEntryPK4 = Guid.NewGuid();
		readonly Guid CusEntryPK5 = Guid.NewGuid();
		readonly Guid CusEntryPK6 = Guid.NewGuid();
		readonly Guid CusEntryPK7 = Guid.NewGuid();
		readonly Guid CusEntryPK8 = Guid.NewGuid();
		readonly Guid CusEntryPK9 = Guid.NewGuid();
		readonly Guid CusEntryPK10 = Guid.NewGuid();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update Custom Entry Number To Multi Item_1] ON [dbo].[JobDocsAndCartage] ([JP_ParentID]) WHERE ([JP_DeliveryCartageCompleted] IS NULL AND [JP_ParentTableCode]='JS') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void AssertTransformationResults()
		{
			AssertEachTransformationResult(ShipmentPK1, new[] { "" }, "Should not update when not a valid entry number");
			AssertEachTransformationResult(ShipmentPK2, new[] { "," }, "Should not update when not a valid entry number");
			AssertEachTransformationResult(ShipmentPK3, new[] { ",," }, "Should not update when not a valid entry number");
			AssertEachTransformationResult(ShipmentPK4, new[] { "111" }, "Should not update when only one entry number");
			AssertEachTransformationResult(ShipmentPK5, new[] { "111" }, "Should update when only one entry number and a comma behind");
			AssertEachTransformationResult(ShipmentPK6, new[] { "111" }, "Should update when only one entry number and a comma ahead");
			AssertEachTransformationResult(ShipmentPK7, new[] { "111,222" }, "Should not update when Category is not CUS");
			AssertEachTransformationResult(ShipmentPK8, new[] { "111,222" }, "Should not update when shipment has a docsAndCartage with actual delivery date");
			AssertEachTransformationResult(ShipmentPK9, new[] { "111", "222" }, "Should update for multiple entry numbers");
			AssertEachTransformationResult(ShipmentPK10, new[] { "111", "222", "333", "444", "555" }, "Should update for multiple entry numbers");
		}

		void AssertEachTransformationResult(Guid shipmentPK, string[] resultEntryNumber, string message)
		{
			AssertEquals(message, resultEntryNumber.Length, TestConnection.ExecuteScalar<int>($@"
	SELECT
		COUNT(*)
	FROM
		dbo.{CusEntryNumTableName}
	WHERE
		CE_EntryType = 'CMR'
		AND CE_RN_NKCountryCode = 'AU'
		AND CE_ParentID = '{shipmentPK}'
"));

			foreach (var entryNumber in resultEntryNumber)
			{
				AssertEquals(message, 1, TestConnection.ExecuteScalar<int>($@"
	SELECT
		COUNT(*)
	FROM
		dbo.{CusEntryNumTableName}
	WHERE
		CE_EntryNum = '{entryNumber}'
		AND CE_EntryType = 'CMR'
		AND CE_RN_NKCountryCode = 'AU'
		AND CE_ParentID = '{shipmentPK}'
"));
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateCustomEntryNumberToMultiItem();
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();

			helper.CreateShipment(ShipmentPK1, "S0000001", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK1, "JS", ShipmentPK1);
			helper.CreateCusEntryNum(CusEntryPK1, ShipmentPK1, "JobShipment", "", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK2, "S0000002", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK2, "JS", ShipmentPK2);
			helper.CreateCusEntryNum(CusEntryPK2, ShipmentPK2, "JobShipment", ",", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK3, "S0000003", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK3, "JS", ShipmentPK3);
			helper.CreateCusEntryNum(CusEntryPK3, ShipmentPK3, "JobShipment", ",,", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK4, "S0000004", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK4, "JS", ShipmentPK4);
			helper.CreateCusEntryNum(CusEntryPK4, ShipmentPK4, "JobShipment", "111", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK5, "S0000005", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK5, "JS", ShipmentPK5);
			helper.CreateCusEntryNum(CusEntryPK5, ShipmentPK5, "JobShipment", "111,", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK6, "S0000006", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK6, "JS", ShipmentPK6);
			helper.CreateCusEntryNum(CusEntryPK6, ShipmentPK6, "JobShipment", " ,111", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK7, "S0000007", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK7, "JS", ShipmentPK7);
			helper.CreateCusEntryNum(CusEntryPK7, ShipmentPK7, "JobShipment", "111,222", "CMR", "AU", "OTH");

			helper.CreateShipment(ShipmentPK8, "S0000008", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK8, "JS", ShipmentPK8, actualDeliveryTime: DateTime.Now);
			helper.CreateCusEntryNum(CusEntryPK8, ShipmentPK8, "JobShipment", "111,222", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK9, "S0000009", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK9, "JS", ShipmentPK9);
			helper.CreateCusEntryNum(CusEntryPK9, ShipmentPK9, "JobShipment", "111,222", "CMR", "AU", "CUS");

			helper.CreateShipment(ShipmentPK10, "S00000010", false);
			helper.CreateDocsAndCartage(DocsAndCartagePK10, "JS", ShipmentPK10);
			helper.CreateCusEntryNum(CusEntryPK10, ShipmentPK10, "JobShipment", "111,222,,333,444,,555", "CMR", "AU", "CUS");
		}
	}
}
