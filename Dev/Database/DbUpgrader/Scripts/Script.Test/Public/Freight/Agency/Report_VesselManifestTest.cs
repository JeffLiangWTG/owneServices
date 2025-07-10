using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_VesselManifest))]
	internal class Report_VesselManifestTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from Report_VesselManifest(null, null, null, null, null, null, null, null, null, null, null, null)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}

		public void TestGoodsDescription_DisplaysPG_IfPackingGroupNotEmpty()
		{
			var time = DateTime.UtcNow;

			var principalPK = Helper.InsertOrgHeader("Org1", "Organisation1");

			var voyagePK = helper.InsertJobVoyage("Voyage1", "Vessel1");
			var originPK = helper.InsertJobVoyOrigin("AUBNE", voyagePK);
			var destinationPK = helper.InsertJobVoyDestination("NLAMS", voyagePK, time);
			var sailingPK = helper.InsertJobSailing(originPK, destinationPK, time);

			var shipmentPK = helper.InsertShipment("V00000001", false, time, "SEA", "FCL", "AUBNE", "NLAMS", time, time.AddDays(1), 10, sailingPK, principalPK, "CNF", true);
			var packLinePK = Helper.InsertPackLine(shipmentPK, "Washing Machines", 2);
			var dataItemPK = Helper.InsertUNDGDataItem(packLinePK, new decimal(-2.0), "3");
			Helper.InsertUNDGSubstance(dataItemPK, "9898", "a", "IMO", "3", "II", new decimal(-2.0), "subs1");

			const string sql = "select GoodsDescription from Report_VesselManifest(null, null, null, @vessel, @voyage, null, @principalPk, null, null, null, null, null)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@principalPk", SqlDbType.UniqueIdentifier, principalPK);
				command.AddParameter("@vessel", SqlDbType.VarChar, "Vessel1");
				command.AddParameter("@voyage", SqlDbType.VarChar, "Voyage1");

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("Data is being returned", true, reader.Read());
					Assert("Packing group is not preceded by 'PKG'", !reader.GetValue(0).ToString().Contains("PKG"));
					Assert("Packing group is preceded by 'PG'", reader.GetValue(0).ToString().Contains("PG"));
					AssertEquals("Only one line of data should be returned", false, reader.Read());
					reader.Close();
				}
			}
		}

		public void TestGoodsDescription_DoesNotDisplayPG_IfPackingGroupEmpty()
		{
			var time = DateTime.Today;

			var principalPK = Helper.InsertOrgHeader("Org1", "Organisation1");

			var voyagePK = helper.InsertJobVoyage("Voyage1", "Vessel1");
			var originPK = helper.InsertJobVoyOrigin("AUBNE", voyagePK);
			var destinationPK = helper.InsertJobVoyDestination("NLAMS", voyagePK, time);
			var sailingPK = helper.InsertJobSailing(originPK, destinationPK, time);

			var shipmentPK = helper.InsertShipment("V00000001", false, time, "SEA", "FCL", "AUBNE", "NLAMS", time, time.AddDays(1), 10, sailingPK, principalPK, "CNF", true);
			var packLinePK = Helper.InsertPackLine(shipmentPK, "Washing Machines", 2);
			var dataItemPK = Helper.InsertUNDGDataItem(packLinePK, new decimal(-2.0), "3");
			Helper.InsertUNDGSubstance(dataItemPK, "9898", "a", "IMO", "3", "", new decimal(-2.0), "subs1");

			const string sql = "select GoodsDescription from Report_VesselManifest(null, null, null, @vessel, @voyage, null, @principalPk, null, null, null, null, null)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@principalPk", SqlDbType.UniqueIdentifier, principalPK);
				command.AddParameter("@vessel", SqlDbType.VarChar, "Vessel1");
				command.AddParameter("@voyage", SqlDbType.VarChar, "Voyage1");

				using (var reader = command.ExecuteReader())
				{
					AssertEquals("Data is being returned", true, reader.Read());
					Assert("As there is no packing group, there should be no 'pg' in the dangerous goods description", !reader.GetValue(0).ToString().Contains("PG"));
					AssertEquals("Only one line of data should be returned", false, reader.Read());
					reader.Close();
				}
			}
		}

		public void TestSTRING_AGGNoExceptionWithLargeNumberOfContainersAndPacklines()
		{
			var refContainerPK = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 {RefContainerSchema.Constants.PK} FROM dbo.{RefContainerSchema.Constants.TableName}");

			var time = DateTime.Today;

			var principalPK = Helper.InsertOrgHeader("Org1", "Organisation1");
			var voyagePK = helper.InsertJobVoyage("Voyage1", "Vessel1");
			var originPK = helper.InsertJobVoyOrigin("AUBNE", voyagePK);
			var destinationPK = helper.InsertJobVoyDestination("NLAMS", voyagePK, time);
			var sailingPK = helper.InsertJobSailing(originPK, destinationPK, time);
			var shipmentPK = InsertShipment("V00000001", false, time, "SEA", "FCL", "AUBNE", "NLAMS", time, time.AddDays(1), 10, sailingPK, principalPK, "CNF", true);
			var containerPK = InsertJobContainer(shipmentPK, refContainerPK, "Container0", "REL");

			for (var i = 1; i < 500; i++)
			{
				InsertJobContainer(shipmentPK, refContainerPK, "CONNUMBER" + i, "REL");
			}

			for (var i = 1; i < 600; i++)
			{
				InsertJobContainerPackPivot(containerPK, helper.InsertPackLine(shipmentPK, "This is PackLine Description" + i, i));
			}

			const string sql = "SELECT COUNT(*) FROM Report_VesselManifest(null, null, null, @vessel, @voyage, null, @principalPk, null, null, null, null, null)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@principalPk", SqlDbType.UniqueIdentifier, principalPK);
				command.AddParameter("@vessel", SqlDbType.VarChar, "Vessel1");
				command.AddParameter("@voyage", SqlDbType.VarChar, "Voyage1");

				AssertNoExceptionThrown(() => command.ExecuteScalar());
			}
		}

		public void TestTheLengthOfPrepaidRevenue_ShouldBe_Infinite()
		{
			var time = DateTime.Today;

			var principalPK = Helper.InsertOrgHeader("Org1", "Organisation1");
			var voyagePK = helper.InsertJobVoyage("Voyage1", "Vessel1");
			var originPK = helper.InsertJobVoyOrigin("AUBNE", voyagePK);
			var destinationPK = helper.InsertJobVoyDestination("NLAMS", voyagePK, time);
			var sailingPK = helper.InsertJobSailing(originPK, destinationPK, time);
			var shipmentPK = InsertShipment("V00000001", false, time, "SEA", "FCL", "AUBNE", "NLAMS", time, time.AddDays(1), 10, sailingPK, principalPK, "CNF", true);

			var org1 = helper.InsertOrgHeader("ZZC", "Org1");
			var company1 = helper.InsertCompany("ABC", "ABC Compay", "CNY", "CN", true, true);
			var branch1 = helper.InsertBranch("ZZB", company1);
			var department1 = helper.InsertDepartment("ZZD");

			var chargeCodePk = helper.InsertChargeCode(company1, "CC1");
			var jobPk = helper.InsertJob("S001", company1, branch1, department1, "JS", shipmentPK, "WRK", new DateTime(2012, 07, 25));

			for (int i = 0; i < 200; i++)
			{
				InsertJobCharge(Guid.NewGuid(), jobPk, branch1, company1, department1, chargeCodePk, org1, 100M, org1, 100M, "TST", Guid.Empty, Guid.Empty, "AAA", "LPP");
			}

			InsertRefCurrency(Guid.NewGuid(), "AAA");

			const string sql1 = "select ChargeCode,PrepaidRevenue,Currency,CollectRevenue from Report_VesselManifest(@CurrentCompany, null, null, @vessel, @voyage, null, @principalPk, null, null, null, null, 'ALL')";

			using (var command = Db.Connection.Command(sql1))
			{
				command.AddParameter("@CurrentCompany", SqlDbType.UniqueIdentifier, company1);
				command.AddParameter("@principalPk", SqlDbType.UniqueIdentifier, principalPK);
				command.AddParameter("@vessel", SqlDbType.VarChar, "Vessel1");
				command.AddParameter("@voyage", SqlDbType.VarChar, "Voyage1");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var prepaidRevenue = (string)reader["PrepaidRevenue"];
						AssertGreaterThan("The length of PrepaidRevenue should be greater than 500.", prepaidRevenue.Length, 500);
						break;
					}
				}
			}
		}

		Guid InsertShipment(string uniqueConsignRef, bool isForwardRegistered, DateTime createdTime, string transportMode, string packingMode, string origin, string destination, DateTime departure, DateTime arrival, decimal chargeable, Guid? sailingPk, Guid? principalPk, string shipmentStatus, bool isShipping)
		{
			var shipmentPK = Guid.NewGuid();
			helper.Insert(JobShipmentSchema.Constants.TableName, new
			{
				JS_PK = shipmentPK,
				JS_UniqueConsignRef = uniqueConsignRef,
				JS_IsForwardRegistered = isForwardRegistered,
				JS_SystemCreateTimeUtc = createdTime,
				JS_TransportMode = transportMode,
				JS_PackingMode = packingMode,
				JS_RL_NKOrigin = origin,
				JS_RL_NKDestination = destination,
				JS_E_DEP = departure,
				JS_E_ARV = arrival,
				JS_ActualChargeable = chargeable,
				JS_JX = sailingPk,
				JS_OH_DeliveryAgent = principalPk,
				JS_ShipmentStatus = shipmentStatus,
				JS_IsShipping = isShipping,
				JS_ActualWeight = 10,
				JS_UnitOfWeight = "KG",
			});
			return shipmentPK;
		}

		void InsertJobCharge(Guid jobChargePK, Guid jobPK, Guid branchPK, Guid companyPK, Guid departmentPK, Guid chargeCodePK,
			Guid jR_OH_SellAccount, decimal jR_LocalSellAmt, Guid jR_OH_CostAccount, decimal jR_LocalCostAmt, string chargeType,
			Guid jR_AL_APLine, Guid jR_AL_ARLine, string currency, string invoiceType)
		{
			helper.Insert(JobChargeSchema.Constants.TableName, new
			{
				JR_PK = jobChargePK,
				JR_JH = jobPK,
				JR_GB = branchPK,
				JR_GC = companyPK,
				JR_GE = departmentPK,
				JR_AC = chargeCodePK,
				JR_OH_SellAccount = jR_OH_SellAccount,
				JR_LocalSellAmt = jR_LocalSellAmt,
				JR_OH_CostAccount = jR_OH_CostAccount,
				JR_LocalCostAmt = jR_LocalCostAmt,
				JR_ChargeType = chargeType,
				JR_AL_APLine = jR_AL_APLine != Guid.Empty ? jR_AL_APLine : (object)DBNull.Value,
				JR_AL_ARLine = jR_AL_ARLine != Guid.Empty ? jR_AL_ARLine : (object)DBNull.Value,
				JR_RX_NKSellCurrency = currency,
				JR_OSSellAmt = 100,
				JR_InvoiceType = invoiceType,
			});
		}

		Guid InsertJobContainer(Guid shipmentPK, Guid refContainerPK, string containerNum, string purpose)
		{
			var containerPK = Guid.NewGuid();

			helper.Insert(JobContainerSchema.Constants.TableName, new
			{
				JC_PK = containerPK,
				JC_JS_FCLBookingOnlyLink = shipmentPK,
				JC_RC = refContainerPK,
				JC_ContainerNum = containerNum,
				JC_Purpose = purpose
			});

			return containerPK;
		}

		void InsertJobContainerPackPivot(Guid containerPK, Guid packLinePK)
		{
			helper.Insert(JobContainerPackPivotSchema.Constants.TableName, new
			{
				J6_PK = Guid.NewGuid(),
				J6_JC = containerPK,
				J6_JL = packLinePK
			});
		}

		protected void InsertRefCurrency(Guid refCurrency, string rxCode)
		{
			helper.Insert(RefCurrencySchema.Constants.TableName, new
			{
				RX_PK = refCurrency,
				RX_Code = rxCode,
				RX_ISOSubUnitRatio = 1
			});
		}

		protected override bool RequiresSchemaBinding => false;

		#region Implementation

		TestDbHelper Helper
		{
			get { return helper ?? (helper = new TestDbHelper(TestConnection)); }
		}
		TestDbHelper helper;
		#endregion
	}
}
