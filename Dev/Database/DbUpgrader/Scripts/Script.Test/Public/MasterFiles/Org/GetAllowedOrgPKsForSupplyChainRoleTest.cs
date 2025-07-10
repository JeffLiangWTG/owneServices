using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(GetAllowedOrgPKsForSupplyChainRole))]
	class GetAllowedOrgPKsForSupplyChainRoleTest : DbCreateScriptTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			var commandText =
@"UPDATE dbo.OrgHeader SET OH_IsShippingProvider = 0;
UPDATE dbo.OrgHeader SET OH_IsMiscFreightServices = 0;";
			Db.Connection.ExecuteNonQuery(commandText);
		}

		public void TestManagementGrouping()
		{
			var parentPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, childPK)}
{InsertOrgRelatedParty(childPK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(childPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(childPK, result);
			}
		}

		public void TestSupplierBuyerLink()
		{
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(buyerPK, supplierPK)}
INSERT dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Buyer, OL_OH_Supplier) VALUES (newid(), '{buyerPK}', '{supplierPK}')");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(buyerPK);
			AssertResult(supplierPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(buyerPK, result);
				AssertCollectionContains(supplierPK, result);
			}
		}

		public void TestShipperBrokerRelation()
		{
			var parentPK = Guid.NewGuid();
			var relatedPartyPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, relatedPartyPK)}
{InsertOrgRelatedParty(parentPK, relatedPartyPK, "SHB")}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(relatedPartyPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(relatedPartyPK, result);
			}
		}

		public void TestClientControllerRelation()
		{
			var parentPK = Guid.NewGuid();
			var relatedPartyPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, relatedPartyPK)}
{InsertOrgRelatedParty(parentPK, relatedPartyPK, "CCB")}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(relatedPartyPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(relatedPartyPK, result);
			}
		}

		public void TestInvoiceFreightJobsToRelation()
		{
			var parentPK = Guid.NewGuid();
			var relatedPartyPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, relatedPartyPK)}
{InsertOrgRelatedParty(parentPK, relatedPartyPK, "IFT")}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(relatedPartyPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(relatedPartyPK, result);
			}
		}

		public void TestShipperBrokerSupplierBuyerLink()
		{
			var orgPK = Guid.NewGuid();
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(orgPK, buyerPK, supplierPK)}
INSERT dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Buyer, OL_OH_Supplier) VALUES (newid(), '{buyerPK}', '{supplierPK}');
{InsertOrgRelatedParty(supplierPK, orgPK, "SHB")}");
			Db.Connection.ExecuteNonQuery(commandText);

			var result = GetResult(orgPK);
			AssertEquals(3, result.Count);
			AssertCollectionContains(orgPK, result);
			AssertCollectionContains(buyerPK, result);
			AssertCollectionContains(supplierPK, result);
		}

		public void TestClientControllerSupplierBuyerLink()
		{
			var orgPK = Guid.NewGuid();
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(orgPK, buyerPK, supplierPK)}
INSERT dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Buyer, OL_OH_Supplier) VALUES (newid(), '{buyerPK}', '{supplierPK}');
{InsertOrgRelatedParty(supplierPK, orgPK, "CCB")}");
			Db.Connection.ExecuteNonQuery(commandText);

			var result = GetResult(orgPK);
			AssertEquals(3, result.Count);
			AssertCollectionContains(orgPK, result);
			AssertCollectionContains(buyerPK, result);
			AssertCollectionContains(supplierPK, result);
		}

		public void TestClientControllerSupplierBuyerLink_RelatedToBuyer()
		{
			var orgPK = Guid.NewGuid();
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(orgPK, buyerPK, supplierPK)}
INSERT dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Buyer, OL_OH_Supplier) VALUES (newid(), '{buyerPK}', '{supplierPK}');
{InsertOrgRelatedParty(buyerPK, orgPK, "CCB")}");
			Db.Connection.ExecuteNonQuery(commandText);

			var result = GetResult(orgPK);
			AssertEquals(3, result.Count);
			AssertCollectionContains(orgPK, result);
			AssertCollectionContains(buyerPK, result);
			AssertCollectionContains(supplierPK, result);
		}

		#region OrderManager Relationships

		public void TestOrderManagerRelationships_BothPartiesOnBuyer()
		{
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();
			var manufacturerPK = Guid.NewGuid();
			var customerPK = Guid.NewGuid();
			var dummyOrgPK = Guid.NewGuid();

			var commandText =
$@"{InsertOrgHeaders(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK)}
{InsertBuyerSupplierRelationship(buyerPK, supplierPK)}
{InsertOrgRelatedParty(buyerPK, manufacturerPK, "MAN")}
{InsertOrgRelatedParty(buyerPK, customerPK, "CCB")}";

			Db.Connection.ExecuteNonQuery(commandText);

			AssertOrderManagerRelationships(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK);
		}

		public void TestOrderManagerRelationships_BothPartiesOnSupplier()
		{
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();
			var manufacturerPK = Guid.NewGuid();
			var customerPK = Guid.NewGuid();
			var dummyOrgPK = Guid.NewGuid();

			var commandText =
$@"{InsertOrgHeaders(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK)}
{InsertBuyerSupplierRelationship(buyerPK, supplierPK)}
{InsertOrgRelatedParty(supplierPK, manufacturerPK, "MAN")}
{InsertOrgRelatedParty(supplierPK, customerPK, "CCB")}";

			Db.Connection.ExecuteNonQuery(commandText);

			AssertOrderManagerRelationships(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK);
		}

		public void TestOrderManagerRelationships_ManOnBuyer_CcbOnSupplier()
		{
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();
			var manufacturerPK = Guid.NewGuid();
			var customerPK = Guid.NewGuid();
			var dummyOrgPK = Guid.NewGuid();

			var commandText =
$@"{InsertOrgHeaders(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK)}
{InsertBuyerSupplierRelationship(buyerPK, supplierPK)}
{InsertOrgRelatedParty(buyerPK, manufacturerPK, "MAN")}
{InsertOrgRelatedParty(supplierPK, customerPK, "CCB")}";

			Db.Connection.ExecuteNonQuery(commandText);

			AssertOrderManagerRelationships(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK);
		}

		public void TestOrderManagerRelationships_CcbOnBuyer_ManOnSupplier()
		{
			var buyerPK = Guid.NewGuid();
			var supplierPK = Guid.NewGuid();
			var manufacturerPK = Guid.NewGuid();
			var customerPK = Guid.NewGuid();
			var dummyOrgPK = Guid.NewGuid();

			var commandText =
$@"{InsertOrgHeaders(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK)}
{InsertBuyerSupplierRelationship(buyerPK, supplierPK)}
{InsertOrgRelatedParty(supplierPK, manufacturerPK, "MAN")}
{InsertOrgRelatedParty(buyerPK, customerPK, "CCB")}";

			Db.Connection.ExecuteNonQuery(commandText);

			AssertOrderManagerRelationships(buyerPK, supplierPK, manufacturerPK, customerPK, dummyOrgPK);
		}

		void AssertOrderManagerRelationships(Guid buyerPK, Guid supplierPK, Guid manufacturerPK, Guid customerPK, Guid dummyOrgPK)
		{
			var result = GetResult(buyerPK);
			AssertCollectionContains("Buyer>Self", buyerPK, result);
			AssertCollectionContains("Buyer>Supplier", supplierPK, result);
			AssertCollectionContains("Buyer>Manufacturer", manufacturerPK, result);
			AssertCollectionContains("Buyer>ControllingCustomer", customerPK, result);
			AssertCollectionNotContains("Buyer>Dummy", dummyOrgPK, result);

			result = GetResult(supplierPK);
			AssertCollectionContains("Supplier>Buyer", buyerPK, result);
			AssertCollectionContains("Supplier>Self", supplierPK, result);
			AssertCollectionContains("Supplier>Manufacturer", manufacturerPK, result);
			AssertCollectionContains("Supplier>ControllingCustomer", customerPK, result);
			AssertCollectionNotContains("Supplier>Dummy", dummyOrgPK, result);

			result = GetResult(manufacturerPK);
			AssertCollectionContains("Manufacturer>Buyer", manufacturerPK, result);
			AssertCollectionContains("Manufacturer>Supplier", supplierPK, result);
			AssertCollectionContains("Manufacturer>Self", manufacturerPK, result);
			AssertCollectionContains("Manufacturer>ControllingCustomer", customerPK, result);
			AssertCollectionNotContains("Manufacturer>Dummy", dummyOrgPK, result);

			result = GetResult(customerPK);
			AssertCollectionContains("Customer>Self", customerPK, result);
			AssertCollectionContains("Customer>Supplier", supplierPK, result);
			AssertCollectionContains("Customer>Manufacturer", manufacturerPK, result);
			AssertCollectionContains("Customer>ControllingCustomer", customerPK, result);
			AssertCollectionNotContains("Customer>Dummy", dummyOrgPK, result);
		}

		#endregion

		static string InsertBuyerSupplierRelationship(Guid buyerPK, Guid supplierPK)
		{
			return FormattableString.Invariant($"INSERT dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Buyer, OL_OH_Supplier) VALUES (newid(), '{buyerPK}', '{supplierPK}');");
		}

		static string InsertOrgHeaders(params Guid[] pks)
		{
			var lines = pks.Select(pk => FormattableString.Invariant($"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES('{pk}', '{pk.ToString("n").Substring(0, OrgHeaderSchema.OH_Code.MaxLength)}');"));
			return string.Join(System.Environment.NewLine, lines);
		}

		static string InsertOrgRelatedParty(Guid parentPK, Guid relatedPK, string partyType = "MNG")
		{
			return FormattableString.Invariant($"INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_OH_Parent, PR_OH_RelatedParty, PR_PartyType) VALUES (newid(), '{parentPK}', '{relatedPK}', '{partyType}');");
		}

		IReadOnlyCollection<Guid> GetResult(Guid currentOrgPK)
		{
			return DataUtils.GetListOfValuesFromQuery(TestConnection, FormattableString.Invariant($"SELECT OrgPK FROM GetAllowedOrgPKsForSupplyChainRole('{currentOrgPK}')")).Select(Guid.Parse).ToList();
		}
	}
}

