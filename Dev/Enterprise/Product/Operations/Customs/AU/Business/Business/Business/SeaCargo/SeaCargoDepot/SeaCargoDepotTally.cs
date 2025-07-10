using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotTally : SeaCargoDepotBusinessObject
	{
		protected SeaCargoDepotTally(CFSContainer container)
			: base(container)
		{
			fContainer = container;
		}

		public static SeaCargoDepotTally Load(CFSContainer container)
		{
			return (SeaCargoDepotTally)Load(typeof(SeaCargoDepotTally), container);
		}

		#region Properties

		public override CFSLoadListConsol ParentConsol
		{
			get
			{
				CFSLoadListConsol result = null;
				if (Container != null)
				{
					result = Container.Consol;
				}
				return result;
			}
		}

		public CFSContainer Container
		{
			get
			{
				return fContainer;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public CommonShipment[] Shipments
		{
			get
			{
				if (fShipments == null)
				{
					ZQuery shipmentPackInContainerFilter = new ZQuery();
					foreach (PackLine packLine in Container.PackLines)
					{
						shipmentPackInContainerFilter.AddToFilter(new ZQuery(JobShipmentSchema.PK, packLine.JL_JS), JoinCondition.Or);
					}
					shipmentPackInContainerFilter.OrderBy = CommonShipment.Schema.JS_HouseBill;
					BusinessObject[] shipmentsPackInContainer = Container.Factory.Load(typeof(PackUnpackShipment), shipmentPackInContainerFilter);
					fShipments = (CommonShipment[])shipmentsPackInContainer;
				}
				return fShipments;
			}
		}

		public CusSCADepotContainerCollection ReportedContainers
		{
			get
			{
				if (fReportedContainers == null)
				{
					fReportedContainers = new CusSCADepotContainerCollection(Container, Factory);
					fReportedContainers.Load();
				}
				return fReportedContainers;
			}
		}

		#endregion

		#region Implementation

		CommonShipment[] fShipments;
		readonly CFSContainer fContainer;
		CusSCADepotContainerCollection fReportedContainers;

		#endregion

	}
}
