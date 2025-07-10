using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public NctsHeaderCollectionFetchStrategy(IBusinessObjectCollection collection) : base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			var factory = Collection.Factory;

			var requiresJobShipmentFetchHints = false;
			var requiresContainerFetchHints = false;
			var requiresJobDocAddressFetchHints = false;
			var requiresDepartureOfficeFetchHints = false;
			var requiresGoodsItemsFetchHints = false;
			var requiresPacksFetchHints = false;
			var requiresCusGoodsLocationFetchHints = false;

			var headers = businessObjects.Cast<NctsHeader>();

			foreach (var column in columns)
			{
				switch (column.ColumnName.Split('+')[0])
				{
					case nameof(NctsHeader.Consignee):
					case nameof(NctsHeader.Consignor):
					case nameof(NctsHeader.Principal):
						requiresJobDocAddressFetchHints = true;
						break;
					case nameof(NctsHeader.CusGoodsLocation):
						requiresCusGoodsLocationFetchHints = true;
						break;
				}

				switch (column.ColumnName)
				{
					case nameof(NctsHeader.JobReferenceNumber):
						requiresJobShipmentFetchHints = true;
						break;
					case nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.IsContainerised):
						requiresContainerFetchHints = true;
						break;
					case nameof(NctsHeader.TotalNumberOfItems):
						requiresGoodsItemsFetchHints = true;
						break;
					case nameof(NctsHeader.TotalNumberOfPackages):
						requiresPacksFetchHints = true;
						requiresGoodsItemsFetchHints = true;
						break;
					case nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.DestinationCustomsOfficeCodeForDepartureForModuleGrid):
					case nameof(NctsHeader.MovementHeader) + "+" + nameof(NctsDepartureMovementHeader.DepartureCustomsOfficeCodeForModuleGrid):
						requiresDepartureOfficeFetchHints = true;
						break;
				}
			}

			AddFetchHintsOnHeaders();
			AddFetchHintsOnMovementHeaders();
			if (requiresGoodsItemsFetchHints || requiresPacksFetchHints)
			{
				AddFetchHintsForGoodsItems();
			}

			void AddFetchHintsOnHeaders()
			{
				foreach (var header in headers)
				{
					factory.AddFetchHint(CusInBondMoveHeaderSchema.BM_BH, header.PK);

					if (header.IsPhase5Departure)
					{
						factory.AddFetchHint(CusInBondBillSchema.B0_BH, header.PK);
					}

					if (requiresJobShipmentFetchHints && header.BH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
					{
						factory.AddFetchHint(JobShipmentSchema.PK, header.BH_ParentID);
					}

					if (requiresContainerFetchHints && header.IsPhase5Departure)
					{
						factory.AddFetchHint(CusInBondContainerSchema.BC_ParentID, header.PK);
					}

					if (requiresJobDocAddressFetchHints)
					{
						factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, header.PK);
					}
				}
			}

			void AddFetchHintsOnMovementHeaders()
			{
				foreach (var header in headers)
				{
					if (header.IsPhase5Departure && header.MovementHeader is NctsDepartureMovementHeader movementHeader)
					{
						if (requiresDepartureOfficeFetchHints)
						{
							AddFetchHintOffice(movementHeader);
						}

						if (requiresGoodsItemsFetchHints)
						{
							factory.AddFetchHint(CusInBondMoveDetailSchema.B9_BM, movementHeader.PK);
						}

						if (requiresCusGoodsLocationFetchHints)
						{
							var query = new ZQuery(CusGoodsLocationSchema.CGL_ParentID, movementHeader.PK)
								.AddToFilter(CusGoodsLocationSchema.CGL_LocationUse, CusGoodsLocationUseList.Codes.Departure);
							factory.AddFetchHint(CusGoodsLocationSchema.Instance, query);

							movementHeader.AddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore();
						}
					}

					if (header.IsPhase5Arrival && header.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
					{
						if (requiresCusGoodsLocationFetchHints)
						{
							var query = new ZQuery(CusGoodsLocationSchema.CGL_ParentID, arrivalMovementHeader.PK)
								.AddToFilter(CusGoodsLocationSchema.CGL_LocationUse, CusGoodsLocationUseList.Codes.Arrival);

							factory.AddFetchHint(CusGoodsLocationSchema.Instance, query);

							arrivalMovementHeader.AddCusGoodsLocationAdditionalIdentifierDescriptionFetchHintsForViewCore();
						}
					}
				}
			}

			void AddFetchHintsForGoodsItems()
			{
				foreach (var bill in headers.Where(x => x.IsPhase5Departure).SelectMany(x => x.Bills))
				{
					factory.AddFetchHint(CusInBondCargoDescSchema.BY_ParentID, bill.PK);
				}

				if (requiresPacksFetchHints)
				{
					foreach (var goodsItem in headers.Where(x => x.IsPhase5Departure).SelectMany(x => x.Bills.SelectMany(b => b.GoodsItems)))
					{
						factory.AddFetchHint(CusInvPackSchema.B5_ParentID, goodsItem.PK);
					}
				}
			}

			void AddFetchHintOffice(NctsCommonMovementHeader movementHeader)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, movementHeader.PK)
					.AddToFilter(CusCodeDataSchema.CY_Type, EU.Business.CusCodeDataTypeList.Codes.OfficeCode);

				factory.AddFetchHint(CusCodeDataSchema.Instance, query);
			}
		}
	}
}
