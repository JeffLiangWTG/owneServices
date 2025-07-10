using System;
using CargoWise.DbUpgrader.Scripts.Definitions.ProductionRules;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ProductionRules
{
	[TestedType(typeof(ProductionRulesFactTypeView))]
	class ProductionRulesFactTypeViewTest : DbCreateScriptTest
	{
		public void TestView_ProductionRulesFactTypeView()
		{
			var entries = new (string TypeCode, Guid TypeGuid)[]
			{
				("INV", new Guid("c09ebab5-9650-4ab0-80ac-8408b6d957db")),
				("PKG", new Guid("1ce2b2ad-c3ba-40aa-b4fa-ab4902af6ea1")),
				("ALO", new Guid("07fa3297-f0f6-4778-af57-7215617c39d7")),
				("ORD", new Guid("c7e774b7-0720-4bd9-ae02-6afd9211b2af")),
				("CCL", new Guid("3a362f6d-3d74-43a7-8590-c006126b0acd")),
				("SHP", new Guid("8bbf2364-6777-447e-914d-20ec6c5d6c33")),
				("CON", new Guid("6bfd837b-d776-493b-9c29-0e7ab1aef6c7")),
				("DEC", new Guid("a1c3d305-394e-4ef8-8d53-d7edbefbd142")),
				("WAR", new Guid("6eaade55-1c74-4191-975b-669fd9d6bd3f")),
				("WKI", new Guid("f5e202f2-2f6a-4bb6-a778-dd33d87aa9a9")),
				("TST", new Guid("8c8a259d-d49c-4a99-9dd6-de42a03e5bef")),
				("CYO", new Guid("128f31d1-955d-4e9f-a448-2a27de0630ab")),
				("CYU", new Guid("cfc8cffb-b1ef-46a3-acdd-4aeab528663a")),
				("CYT", new Guid("ea2cbbec-ce0f-4d65-ba31-b2d96a3d2b78")),
				("CGM", new Guid("74971fde-81ea-4796-b842-95b7820239dd")),
				("TCC", new Guid("c206512a-7b91-45ee-a1c2-c84c81b412a9")),
				("PTU", new Guid("7350bdea-1c97-4b1b-ab93-acb3f6100133")),
				("PTT", new Guid("073c0e08-89e1-4875-9170-c4c9e4af3ae0")),
				("PTP", new Guid("7f7f89f0-9e6e-474c-a342-7094553d3094")),
				("PTL", new Guid("f9db6305-2ef4-4ada-a161-7565840b9269")),
				("PTC",  new Guid("571c598d-d99d-4437-bc79-3151e691eaf8")),
				("PWT", new Guid("96a18125-a5bd-41d3-8cda-d561c8d36ba1")),
			};

			AssertEquals($"View should have {entries.Length} records.", entries.Length, TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.ProductionRulesFactTypeView"));
			AssertEquals($"View should have {entries.Length} unique PKs.", entries.Length, TestConnection.ExecuteScalar($"SELECT COUNT(DISTINCT PFV_PK) FROM dbo.ProductionRulesFactTypeView"));
			AssertEquals($"View should have {entries.Length} unique keys.", entries.Length, TestConnection.ExecuteScalar($"SELECT COUNT(DISTINCT PFV_FactTypeKey) FROM dbo.ProductionRulesFactTypeView"));

			foreach (var (typeCode, typeGuid) in entries)
			{
				TestViewCore(typeCode, typeGuid);
			}
		}

		void TestViewCore(string propertyKey, Guid correctParentID) =>
			AssertEquals($"ProductionRulesFactType of FactKey={propertyKey} should have the correct PFV_PK.", correctParentID, TestConnection.ExecuteScalar($"SELECT PFV_PK FROM dbo.ProductionRulesFactTypeView WHERE PFV_FactTypeKey = '{propertyKey}'"));
	}
}

