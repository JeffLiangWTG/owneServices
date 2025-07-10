using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderMessageStatus : ICustomsMessageStatusProvider, Integration.Customs.AU.ICusSeaManOBLHeaderMessageStatus
	{
		#region ICusSeaManOBLHeaderMessageStatusProvider Members

		public bool ShouldShow(BillOfLading billOfLading)
		{
			return true;
		}

		public string GetCustomsStatus(BillOfLading billOfLading)
		{
			ZString result = String.Empty;
			CusSeaManTranHead manifest = null;
			if (billOfLading.Sailing != null)
			{
				manifest = GetHeader(billOfLading);
			}

			if (manifest != null)
			{
				foreach (CusSeaManOBLHeader cusSeaManOBLHeader in manifest.OceanBills)
				{
					if (cusSeaManOBLHeader.BO_OceanBill == billOfLading.JS_HouseBill)
					{
						result = cusSeaManOBLHeader.ShipmentStatus.Description;
					}
				}
			}

			return result;
		}

		public string GetMessageStatus(BillOfLading billOfLading)
		{
			ZString result = String.Empty;

			CusSeaManTranHead manifest = null;
			if (billOfLading.Sailing != null)
			{
				manifest = GetHeader(billOfLading);
			}

			if (manifest != null)
			{
				foreach (CusSeaManOBLHeader cusSeaManOBLHeader in manifest.OceanBills)
				{
					if (cusSeaManOBLHeader.BO_OceanBill == billOfLading.JS_HouseBill)
					{
						result = cusSeaManOBLHeader.CargoReportStatus.Description;
					}
				}

				if (result.IsEmpty)
				{
					foreach (CusSeaManArrivalPort arrivalPort in manifest.Arrivals)
					{
						if (arrivalPort.ArrivalPort.Code == billOfLading.JS_NKDischargePort)
						{
							if (arrivalPort.Messages.Count > 0)
							{
								result = "Message Sent";
							}
							else
							{
								result = "Not Sent";
							}
						}
					}
				}
			}

			return result;
		}

		public string GetUserFriendlyStatusMessage(BillOfLading billOfLading)
		{
			String result = String.Empty;
			CusSeaManTranHead manifest = null;
			if (billOfLading.Sailing != null)
			{
				manifest = GetHeader(billOfLading);
			}

			if (manifest != null)
			{
				foreach (CusSeaManOBLHeader cusSeaManOBLHeader in manifest.OceanBills)
				{
					if (cusSeaManOBLHeader.BO_OceanBill == billOfLading.JS_HouseBill)
					{
						result = ((ICalculatedCusStatusCalculator)cusSeaManOBLHeader.ShipmentCalculator).UserFriendlyStatusText;
					}
				}
			}
			return result;
		}

		CusSeaManTranHead GetHeader(BillOfLading billOfLading)
		{
			List<ZString> emptyContainers = new List<ZString>();

			AgencyShipmentContainerDependentCollection containers = billOfLading.IsBillOfLadingStage ? billOfLading.RealContainers : billOfLading.BookedContainers;
			foreach (BillOfLadingContainer container in containers)
			{
				if (container.JC_IsEmptyContainer || billOfLading.IsDomestic())
				{
					emptyContainers.Add(container.JC_ContainerNum);
				}
			}

			ZDBOnlySubQuery oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLHeader), CusSeaManOBLHeaderSchema.BO_BT);
			oceanBillSubQuery.AddToFilter(CusSeaManOBLHeaderSchema.BO_OceanBill, billOfLading.JS_HouseBill);
			oceanBillSubQuery.AddToFilter(CusSeaManOBLHeaderSchema.BO_RL_NKDischargePort, billOfLading.JS_NKDischargePort);

			ZDBOnlyQuery oceanBillOrCargoListQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
			oceanBillOrCargoListQuery.AddSubQuery(oceanBillSubQuery, JoinCondition.And);
			if (emptyContainers.Count > 0)
			{
				ZDBOnlySubQuery detailSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLDetail), CusSeaManOBLDetailSchema.BD_BO);
				detailSubQuery.AddToFilter(CusSeaManOBLDetailSchema.BD_ContainerNumber, emptyContainers);
				ZDBOnlySubQuery cargoLineSubQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLHeader), CusSeaManOBLHeaderSchema.BO_BT);
				cargoLineSubQuery.AddSubQuery(detailSubQuery, JoinCondition.And);
				cargoLineSubQuery.AddToFilter(CusSeaManOBLHeaderSchema.BO_RL_NKDischargePort, billOfLading.JS_NKDischargePort);
				oceanBillOrCargoListQuery.AddSubQuery(cargoLineSubQuery, JoinCondition.Or);
			}

			ZQuery tranHeadQuery = new ZQuery();
			tranHeadQuery.AddToFilter(oceanBillOrCargoListQuery);
			tranHeadQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, billOfLading.Sailing.JX_JV_VoyageFlight);
			tranHeadQuery.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, billOfLading.Sailing.JX_JV_NKVessel);

			CusSeaManTranHead manifest = billOfLading.Factory.LoadTop1<CusSeaManTranHead>(tranHeadQuery);

			return manifest;
		}

		#endregion
	}
}
