using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(VW_RelatedOrganisations))]
	class VW_RelatedOrganisationsTest : DbCreateScriptTest
	{
		public void TestView()
		{
			var orgPks = Enumerable.Range(1, 10).Select(i => TestDataCreator.CreateOrganisation("~O" + i, "Org " + i)).ToArray();
			CreateOrgRelatedParty(orgPks[0], orgPks[1]);
			CreateOrgRelatedParty(orgPks[2], orgPks[0]);
			CreateOrgSupplierBuyerLink(orgPks[0], orgPks[3]);
			CreateOrgSupplierBuyerLink(orgPks[4], orgPks[0]);
			CreateOrgSupplierBuyerLink(orgPks[0], orgPks[1]);
			CreateOrgSupplierBuyerLink(orgPks[1], orgPks[2]);
			CreateOrgSupplierBuyerLink(orgPks[3], orgPks[4]);

			var results = new List<Tuple<Guid, Guid>>();
			var sql = @"select OrgPK, RelatedPK from dbo.VW_RelatedOrganisations where OrgPK = @orgPK";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPks[0]);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						results.Add(Tuple.Create(reader.GetGuid(0), reader.GetGuid(1)));
					}
				}
			}

			AssertEquals(5, results.Count);
			AssertEquals(true, results.Contains(Tuple.Create(orgPks[0], orgPks[0])));
			AssertEquals(true, results.Contains(Tuple.Create(orgPks[0], orgPks[1])));
			AssertEquals(true, results.Contains(Tuple.Create(orgPks[0], orgPks[2])));
			AssertEquals(true, results.Contains(Tuple.Create(orgPks[0], orgPks[3])));
			AssertEquals(true, results.Contains(Tuple.Create(orgPks[0], orgPks[4])));
		}

		void CreateOrgRelatedParty(Guid parent, Guid relatedParty)
		{
			var sql = @"INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_OH_Parent, PR_OH_RelatedParty) VALUES (NEWID(), @parent, @relatedParty)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@parent", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@relatedParty", SqlDbType.UniqueIdentifier, relatedParty);
				command.ExecuteNonQuery();
			}
		}

		void CreateOrgSupplierBuyerLink(Guid supplier, Guid buyer)
		{
			var sql = @"INSERT INTO dbo.OrgSupplierBuyerLink (OL_PK, OL_OH_Supplier, OL_OH_Buyer) VALUES (NEWID(), @supplier, @buyer)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@supplier", SqlDbType.UniqueIdentifier, supplier);
				command.AddParameter("@buyer", SqlDbType.UniqueIdentifier, buyer);
				command.ExecuteNonQuery();
			}
		}
	}
}

