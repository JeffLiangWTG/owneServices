using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsHeaderCollectionFetchStrategyTest : TestCaseWithFactory
	{
		const string CusInBondMoveHeaderTableName = CusInBondMoveHeaderSchema.Constants.TableName;
		const string GoodsItemsTableName = CusInBondCargoDescSchema.Constants.TableName;
		const string CusInvPackTableName = CusInvPackSchema.Constants.TableName;
		const string NctsBillsTableName = CusInBondBillSchema.Constants.TableName;
		const string CusInBondMoveDetailTableName = CusInBondMoveDetailSchema.Constants.TableName;
		const string CusInBondContainerTableName = CusInBondContainerSchema.Constants.TableName;
		const string JobShipmentTableName = JobShipmentSchema.Constants.TableName;
		const string CusCodeDataTableName = CusCodeDataSchema.Constants.TableName;
		const string CusGoodsLocationTableName = CusGoodsLocationSchema.Constants.TableName;
		const string JobDocAddressTableName = JobDocAddressSchema.Constants.TableName;

		const string DepartureOfficeColumnName = nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.DepartureCustomsOfficeCodeForModuleGrid);
		const string DestinationOfficeColumnName = nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.DestinationCustomsOfficeCodeForDepartureForModuleGrid);
		const string IsContainerisedColumnName = nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.IsContainerised);

		public void TestFetchForViewActiveTableFetchHints_JobReference() => AssertFetchForViewActiveTableFetchHints(nameof(NctsHeader.JobReferenceNumber),
				(JobShipmentTableName, 2),
				(CusInBondMoveHeaderTableName, 0));

		public void TestFetchForViewActiveTableFetchHints_TotalItems() => AssertFetchForViewActiveTableFetchHints(nameof(NctsHeader.TotalNumberOfItems),
				(GoodsItemsTableName, 4),
				(CusInvPackTableName, 0));

		public void TestFetchForViewActiveTableFetchHints_TotalPackages() => AssertFetchForViewActiveTableFetchHints(nameof(NctsHeader.TotalNumberOfPackages),
				(CusInvPackTableName, 8),
				(GoodsItemsTableName, 0));

		public void TestFetchForViewActiveTableFetchHints_OfficeForDeparture() => AssertFetchForViewActiveTableFetchHints(DepartureOfficeColumnName,
				(CusCodeDataTableName, 2));

		public void TestFetchForViewActiveTableFetchHints_OfficeForDestination() => AssertFetchForViewActiveTableFetchHints(DestinationOfficeColumnName,
				(CusCodeDataTableName, 2));

		void AssertFetchForViewActiveTableFetchHints(string columnName, params (string table, int fetchHintCount)[] fetchHints)
		{
			CreatePhase5Declarations();

			var factory = new BusinessObjectFactory();
			var headerCollection = new NctsHeaderCollection(factory);
			headerCollection.Load();
			headerCollection.ForEach(x => x.ReadOnly = true);
			var fetchStrategy = headerCollection.FetchStrategy;
			fetchStrategy.FetchForView(headerCollection.ToArray(), [new TableColumn(string.Empty, columnName)]);
			CombineAssertions(() =>
			{
				foreach (var (table, fetchHintCount) in fetchHints)
				{
					AssertEquals($"{columnName} table {table} fetchHints", fetchHintCount, factory.ActiveFetchHintsForTable(table));
				}
			});
		}

		public void TestTableHitCounts_Items() => AssertTableHitCount(nameof(NctsHeader.TotalNumberOfItems),
				header => header.IsPhase5Departure ? header.Bills.SelectMany(x => x.GoodsItems).Count() : 0, 8,
				[GoodsItemsTableName, CusInBondMoveHeaderTableName, NctsBillsTableName, CusInBondMoveDetailTableName],
				[CusInvPackTableName]);

		public void TestTableHitCounts_Packages() => AssertTableHitCount(nameof(NctsHeader.TotalNumberOfPackages),
				header => header.IsPhase5Departure ? header.Bills.SelectMany(x => x.GoodsItems.SelectMany(g => g.Packages)).Count() : 0, 16,
				[CusInvPackTableName, GoodsItemsTableName, CusInBondMoveHeaderTableName, NctsBillsTableName, CusInBondMoveDetailTableName]);

		public void TestTableHitCounts_DepartureCustomsOffice() => AssertTableHitCount(DepartureOfficeColumnName,
				header => header.MovementHeader?.DepartureCustomsOffice == null ? 0 : 1, 2,
				[CusCodeDataTableName, CusInBondMoveHeaderTableName]);

		public void TestTableHitCounts_DestinationCustomsOffice() => AssertTableHitCount(DestinationOfficeColumnName,
				header => header.MovementHeader?.DestinationCustomsOffice == null ? 0 : 1, 2,

				[CusCodeDataTableName, CusInBondMoveHeaderTableName]);

		public void TestTableHitCounts_Consignee() => AssertTableHitCount(nameof(NctsHeader.Consignee),
				header => header.Consignee.IsEmpty ? 0 : 1, 2,
				[JobDocAddressTableName]);

		public void TestTableHitCounts_Consignor() => AssertTableHitCount(nameof(NctsHeader.Consignor),
				header => header.Consignor.IsEmpty ? 0 : 1, 2,
				[JobDocAddressTableName]);

		public void TestTableHitCounts_Principal() => AssertTableHitCount(nameof(NctsHeader.Principal),
				header => header.Principal.IsEmpty ? 0 : 1, 2,
				[JobDocAddressTableName]);

		public void TestTableHitCounts_IsContainerised() => AssertTableHitCount(IsContainerisedColumnName,
				header => header.MovementHeader?.IsContainerised ?? false ? 1 : 0, 2,
				[CusInBondContainerTableName]);

		public void TestTableHitCounts_GoodsLocation() => AssertTableHitCount(nameof(NctsHeader.CusGoodsLocation),
				header => header.CusGoodsLocation == null ? 0 : 1, 4,
				[CusGoodsLocationTableName]);
		public void TestTableHitCounts_JobReference() => AssertTableHitCount(nameof(NctsHeader.JobReferenceNumber),
				header => header.JobReferenceNumber.IsEmpty ? 0 : 1, 4,
				[JobShipmentTableName]);

		void AssertTableHitCount(string columnName, Func<NctsHeader, int> getCountFunc, int expectedCount, string[] tablesWith1Hit, string[] tablesWith0Hits = null)
		{
			CreatePhase5Declarations();

			var factory = new BusinessObjectFactory();
			var headerCollection = new NctsHeaderCollection(factory);
			headerCollection.Load();
			headerCollection.ForEach(x => x.ReadOnly = true);
			var fetchStrategy = headerCollection.FetchStrategy;
			fetchStrategy.FetchForView(headerCollection.ToArray(), [new TableColumn(string.Empty, columnName)]);
			var actualCount = 0;

			foreach (NctsHeader header in headerCollection.ToArray())
			{
				actualCount += getCountFunc(header);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Should match count", expectedCount, actualCount);

				foreach (var tableName in tablesWith1Hit)
				{
					AssertEquals($"{columnName} table {tableName} should have 1 hit", 1, factory.GetTableHitCount(tableName));
				}
				foreach (var tableName in tablesWith0Hits ?? [])
				{
					AssertEquals($"{columnName} table {tableName} should have 0 hits", 0, factory.GetTableHitCount(tableName));
				}
			});
		}

		void CreatePhase5Declarations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			for (var i = 1; i <= 4; i++)
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(i % 2 == 0 ? NctsMovementType.Codes.Departure : NctsMovementType.Codes.Arrival);
				if (i == 1 || i == 3)
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					header.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					header.BH_ParentID = shipment.PK;
				}

				if (header.MovementHeader is NctsDepartureMovementHeader departureHeader)
				{
					var des = departureHeader.CustomsOffices.AddNew();
					des.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
					des.CY_Data = "DESdata";
					var dep = departureHeader.CustomsOffices.AddNew();
					dep.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
					dep.CY_Data = "DEPdata";

					header.Consignee.OrganisationPK = org1.PK;
					header.Consignor.OrganisationPK = org2.PK;
					header.Principal.OrganisationPK = org3.PK;

					var cont1 = header.DepartureHeaderContainers.AddNew();
					cont1.BC_Mode = Core.Constants.ContainerModes.Containerised;
					var cont2 = header.DepartureHeaderContainers.AddNew();
					cont2.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
				}

				if (header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalHeader)
				{
					var des = arrivalHeader.CustomsOffices.AddNew();
					des.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
					des.CY_Data = "DSAdata";
				}

				var bills = new[] { header.Bills.AddNew(), header.Bills.AddNew() };

				bills.ForEach(bill =>
				{
					for (var j = 1; j <= 2; j++)
					{
						NctsCommonCargoDesc goodsItem;
						if (header.IsDepartureMovement)
						{
							goodsItem = bill.GoodsItems.AddNew();
						}
						else
						{
							goodsItem = bill.ArrivalGoodsItems.AddNew();
						}

						goodsItem.Packages.AddNew();
						goodsItem.Packages.AddNew();
					}
				});
			}

			Factory.Save();
		}
	}
}
