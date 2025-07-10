using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU.ModelViews.EUJobDeclaration))]
	sealed class EUJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"EUJobDeclaration",
				"JobDeclaration",
				ExpectedColumns.ToArray()
			);
		}

		public static IEnumerable<TestDbViewHelper.DbColumn> ExpectedColumns => new []
		{
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_ActivateByOperator", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_AgreedPlaceCode", VarChar, 5),
			new TestDbViewHelper.DbColumn("JE_AuthorisationNumber", VarChar, 35),
			new TestDbViewHelper.DbColumn("JE_BorderTransportMeans", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_Box18TransportID", VarChar, 27),
			new TestDbViewHelper.DbColumn("JE_Box18TransportNationality", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_Box18TransportType", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_BypassCode", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_BypassReason", VarChar, 255),
			new TestDbViewHelper.DbColumn("JE_CTStatusID", VarChar, 5),
			new TestDbViewHelper.DbColumn("JE_DepartureMeansOfTransport", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_ExportUnionCountryCode", VarChar, 4),
			new TestDbViewHelper.DbColumn("JE_Gateway", VarChar, 5),
			new TestDbViewHelper.DbColumn("JE_InlandTransportType", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_IsHighValueOvrd", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_IsSecurityDeclaration", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_IsTrainingDeclaration", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_LCPDepart", DateTime, -1),
			new TestDbViewHelper.DbColumn("JE_LCPInspect", DateTime, -1),
			new TestDbViewHelper.DbColumn("JE_MethodOfPayment", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_RegionOfDestination", VarChar, 10),
			new TestDbViewHelper.DbColumn("JE_RouteFRequested", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_ShipmentType", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_SpecificCircumstanceIndicator", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_StyleOfEntrySOE", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_TypeOfSecurity", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_VATCANACode", VarChar, 4),
			new TestDbViewHelper.DbColumn("JE_VATDeferNumber", VarChar, 20),
			new TestDbViewHelper.DbColumn("JE_VATDeferType", VarChar, 3),
		};

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"EUJobDeclaration_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_Box18TransportID", "JE_Box18TransportID"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_Box18TransportNationality", "JE_Box18TransportNationality"),
				}
			);
		}
	}
}
