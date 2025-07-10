using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(AgencyBookedActualContainerMap))]
	internal class AgencyBookedActualContainerMapTest : DbCreateScriptTest
	{
		public void TestSampleCall1()
		{
			Guid sailing = NewSailing("Vessel1", "Voyage1", "AUBNE", "NLAMS");

			Guid shipment1 = NewShipment(sailing, "V00000001");
			Guid container1b1 = NewBookedContainer(shipment1, "20GP", "TEST4100013", false);
			Guid container1b2 = NewBookedContainer(shipment1, "20GP", "TEST4100029", false);
			Guid container1a1 = NewActualContainer(shipment1, "20GP", "TEST4100029", false);
			Guid container1a2 = NewActualContainer(shipment1, "20GP", "TEST4100034", false);

			Guid shipment2 = NewShipment(sailing, "V00000002");
			Guid container2b1 = NewBookedContainer(shipment2, "20GP", "TEST4100034", false);
			Guid container2a1 = NewActualContainer(shipment2, "20GP", "TEST4100013", false);

			Dictionary<Guid, string> labels = new Dictionary<Guid, string>();
			labels.Add(container1b1, "container1b1");
			labels.Add(container1b2, "container1b1");
			labels.Add(container1a1, "container1a1");
			labels.Add(container1a2, "container1a2");
			labels.Add(container2b1, "container2b1");
			labels.Add(container2a1, "container2a1");

			Converter<Guid, string> renderer = delegate(Guid pk)
			{
				string result;
				if (!labels.TryGetValue(pk, out result))
				{
					result = pk.ToString();
				}
				return result;
			};

			Dictionary<Guid, List<Guid>> map = GetMappings("Vessel1", "Voyage1");

			AssertEquals(4, map.Count);
			AssertContainsExactElementsInAnyOrder("null", renderer, new Guid[] { container1a2, container2a1 }, map[Guid.Empty]);
			AssertContainsExactElementsInAnyOrder("container1b1", renderer, new Guid[] { Guid.Empty }, map[container1b1]);
			AssertContainsExactElementsInAnyOrder("conatiner1b2", renderer, new Guid[] { container1a1 }, map[container1b2]);
			AssertContainsExactElementsInAnyOrder("container2b1", renderer, new Guid[] { Guid.Empty }, map[container2b1]);
		}

		public void TestSampleCall2()
		{
			Guid sailing = NewSailing("Vessel2", "Voyage2", "AUBNE", "NLAMS");

			Guid shipment1 = NewShipment(sailing, "V00000001");
			Guid container1b1 = NewBookedContainer(shipment1, "20GP", 2, false);
			Guid container1b2 = NewBookedContainer(shipment1, "20RE", 2, false);

			Guid container1a1 = NewActualContainer(shipment1, "20GP", "TEST4100013", false);
			Guid container1a2 = NewActualContainer(shipment1, "20GP", "TEST4100029", false);
			Guid container1a3 = NewActualContainer(shipment1, "20GP", "TEST4100034", false);
			Guid container1a4 = NewActualContainer(shipment1, "20RE", "TEST4100050", false);
			Guid container1a5 = NewActualContainer(shipment1, "20RE", "TEST4100071", true);
			Guid container1a6 = NewActualContainer(shipment1, "20RE", "", true);

			Guid shipment2 = NewShipment(sailing, "V00000002");
			Guid container2b1 = NewBookedContainer(shipment2, "20RE", 1, false);

			Dictionary<Guid, string> labels = new Dictionary<Guid, string>();
			labels.Add(container1b1, "container1b1");
			labels.Add(container1b2, "container1b1");
			labels.Add(container1a1, "container1a1");
			labels.Add(container1a2, "container1a2");
			labels.Add(container1a3, "container1a3");
			labels.Add(container1a4, "container1a4");
			labels.Add(container1a5, "container1a5");
			labels.Add(container1a6, "container1a6");
			labels.Add(container2b1, "container2b1");

			Converter<Guid, string> renderer = delegate(Guid pk)
			{
				string result;
				if (!labels.TryGetValue(pk, out result))
				{
					result = pk.ToString();
				}
				return result;
			};

			Dictionary<Guid, List<Guid>> map = GetMappings("Vessel2", "Voyage2");

			AssertEquals(4, map.Count);
			AssertContainsExactElementsInAnyOrder("null", renderer, new Guid[] { container1a3, container1a5, container1a6 }, map[Guid.Empty]);
			AssertContainsExactElementsInAnyOrder("container1b1", renderer, new Guid[] { container1a1, container1a2 }, map[container1b1]);
			AssertContainsExactElementsInAnyOrder("container1b2", renderer, new Guid[] { Guid.Empty, container1a4 }, map[container1b2]);
			AssertContainsExactElementsInAnyOrder("container2b1", renderer, new Guid[] { Guid.Empty }, map[container2b1]);
		}

		#region Implementation

		static Guid NewSailing(string vessel, string voyage, string load, string discharge)
		{
			Guid pk = Guid.NewGuid();

			const string sql =
				"declare @VoyagePK uniqueidentifier " +
				"declare @OriginPK uniqueidentifier " +
				"declare @DestinationPK uniqueidentifier " +

				"set @VoyagePK = newid() " +
				"set @OriginPK = newid() " +
				"set @DestinationPK = newid() " +

				"insert into dbo.JobVoyage(JV_PK, JV_RV_NKVessel, JV_VoyageFlight) " +
				"values(@VoyagePK, @Vessel, @Voyage) " +

				"insert into dbo.JobVoyOrigin(JA_PK, JA_JV, JA_RL_NKPortOfLoading) " +
				"values(@OriginPK, @VoyagePK, @Load) " +

				"insert into dbo.JobVoyDestination(JB_PK, JB_JV, JB_RL_NKPortOfDischarge) " +
				"values(@DestinationPK, @VoyagePK, @Discharge) " +

				"insert into dbo.JobSailing(JX_PK, JX_JA, JX_JB) " +
				"values(@PK, @OriginPK, @DestinationPK) " +
				"";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Vessel", SqlDbType.VarChar, JobVoyageSchema.JV_RV_NKVessel.MaxLength, vessel);
				command.AddParameter("@Voyage", SqlDbType.VarChar, JobVoyageSchema.JV_VoyageFlight.MaxLength, voyage);
				command.AddParameter("@Load", SqlDbType.VarChar, JobVoyOriginSchema.JA_RL_NKPortOfLoading.MaxLength, load);
				command.AddParameter("@Discharge", SqlDbType.VarChar, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge.MaxLength, discharge);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		static Guid NewShipment(Guid sailingpk, string shipmentNo)
		{
			Guid pk = Guid.NewGuid();

			const string sql =
				"insert into dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsShipping, JS_PackingMode, JS_JX) " +
				"values(@PK, @ShipmentNo, 1, @JS_PackingMode, @sailingpk)" +
				"";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ShipmentNo", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, shipmentNo);
				command.AddParameter("@JS_PackingMode", SqlDbType.VarChar, JobShipmentSchema.JS_PackingMode.MaxLength, "FCL");
				command.AddParameter("@sailingpk", SqlDbType.UniqueIdentifier, sailingpk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		static Guid NewBookedContainer(Guid shipment, string containerType, string containerNum, bool isShipperOwned)
		{
			return NewContainerCore(shipment, "BKD", containerType, containerNum, 1, isShipperOwned);
		}

		static Guid NewBookedContainer(Guid shipment, string containerType, int containerCount, bool isShipperOwned)
		{
			return NewContainerCore(shipment, "BKD", containerType, "", containerCount, isShipperOwned);
		}

		static Guid NewActualContainer(Guid shipment, string containerType, string containerNum, bool isShipperOwned)
		{
			return NewContainerCore(shipment, "REL", containerType, containerNum, 1, isShipperOwned);
		}

		static Guid NewContainerCore(Guid shipment, string purpose, string containerType, string containerNum, int containerCount, bool isShipperOwned)
		{
			Guid pk = Guid.NewGuid();

			const string sql =
				"declare @RC uniqueidentifier " +
				"select @RC = RC_PK from dbo.RefContainer where RC_Code = @ContainerType " +
				"" +
				"insert into dbo.JobContainer(JC_PK, JC_JS_FCLBookingOnlyLink, JC_Purpose, JC_RC, JC_ContainerNum, JC_ContainerCount, JC_IsShipperOwned) " +
				"values(@PK, @JS, @Purpose, @RC, @ContainerNum, @ContainerCount, @IsShipperOwned)" +
				"";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@JS", SqlDbType.UniqueIdentifier, shipment);
				command.AddParameter("@Purpose", SqlDbType.VarChar, JobContainerSchema.JC_Purpose.MaxLength, purpose);
				command.AddParameter("@ContainerType", SqlDbType.VarChar, RefContainerSchema.RC_Code.MaxLength, containerType);
				command.AddParameter("@ContainerNum", SqlDbType.VarChar, JobContainerSchema.JC_ContainerNum.MaxLength, containerNum);
				command.AddParameter("@ContainerCount", SqlDbType.Int, containerCount);
				command.AddParameter("@IsShipperOwned", SqlDbType.Bit, isShipperOwned);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		Dictionary<Guid, List<Guid>> GetMappings(string vessel, string voyage)
		{
			const string sql = "select * from dbo.AgencyBookedActualContainerMap(@vessel, @voyage, 'N', null, null, null, null) order by JC_BookedContainer, JC_RealContainer";
			const string JC_BookedContainer = "JC_BookedContainer";
			const string JC_RealContainer = "JC_RealContainer";

			Dictionary<Guid, List<Guid>> result = new Dictionary<Guid, List<Guid>>();

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@vessel", SqlDbType.VarChar, 35, vessel);
				command.AddParameter("@voyage", SqlDbType.VarChar, 35, voyage);

				using (var reader = command.ExecuteReader())
				{
					Guid? lastBookedPK = null;
					List<Guid> actual = new List<Guid>();

					while (reader.Read())
					{
						Guid bookedPK = GetGuid(reader, JC_BookedContainer);

						if (!lastBookedPK.HasValue)
						{
							lastBookedPK = bookedPK;
						}
						else if (lastBookedPK != bookedPK)
						{
							result.Add(lastBookedPK.Value, actual);
							actual = new List<Guid>();
							lastBookedPK = bookedPK;
						}

						actual.Add(GetGuid(reader, JC_RealContainer));
					}

					if (lastBookedPK.HasValue)
					{
						result.Add(lastBookedPK.Value, actual);
					}
				}
			}

			return result;
		}

		Guid GetGuid(IDataReader reader, string name)
		{
			object value = reader[name];
			return value == DBNull.Value ? Guid.Empty : (Guid)value;
		}
		#endregion
	}
}

