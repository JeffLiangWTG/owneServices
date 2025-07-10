using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaScanForOutturnHeldShipmentManager : ScanForOutturnHeldShipmentManager
	{
		public SeaScanForOutturnHeldShipmentManager(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public override OutturnLineCollection GetNewOutturnLineCollection()
		{
			return new SeaOutturnLineCollection(Factory);
		}

		protected override OutturnLine GetNewOutturnLine()
		{
			return new SeaOutturnLine();
		}

		protected override IEnumerable<IScanHouseBillProvider> GetHouseBills()
		{
			var houseBillQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));

			var pivotQuery = new ZDBOnlySubQuery(typeof(CusSCAPivot), CusSCAPivotSchema.CV_CA);
			pivotQuery.AddToFilter(CusSCAPivotSchema.CV_IsHeldAtOutturn, true);

			if (!OutturningPremiseID.IsEmpty)
			{
				var outturnPremiseIDQueryString = @" CV_CN IN (
	SELECT C4_ParentID FROM dbo.CusUnderbond
		LEFT JOIN dbo.OrgAddress ON OA_PK = C4_OA_DestinationAddress
		LEFT JOIN dbo.OrgCusCode branchCode ON branchCode.OK_OA_PremisesAddress = OA_PK AND branchCode.OK_CodeType = 'CCP' AND branchCode.OK_RN_NKCodeCountry = 'AU'
		LEFT JOIN dbo.OrgCusCode orgCode ON orgCode.OK_OH = OA_OH AND orgCode.OK_CodeType = 'CCP' AND orgCode.OK_RN_NKCodeCountry = 'AU'
	WHERE C4_MovementReason = 'DCL' AND (C4_DestinationPremiseID = @premiseID OR branchCode.OK_CustomsRegNo = @premiseID 
		OR ( branchCode.OK_CustomsRegNo IS NULL AND orgCode.OK_CustomsRegNo = @premiseID ))) ";

				pivotQuery.AddFilterAndZSQLParameterCollection(outturnPremiseIDQueryString, new ZSqlParameterCollection(
					ZSqlParameter.New("@premiseID", OutturningPremiseID, CusUnderbondSchema.C4_DestinationPremiseID)));
			}

			houseBillQuery.AddSubQuery(pivotQuery, JoinCondition.And);

			var oceanBillQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
			oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);

			houseBillQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanBillQuery, JoinCondition.And);

			return Factory.Load<CusSCAHouse>(houseBillQuery);
		}

		protected override IScanHouseBillProvider GetMatchingOutturnedHouseBill(ZString houseBillNo)
		{
			var houseBillQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
			houseBillQuery.AddToFilter(CusSCAHouseSchema.CA_HouseBill, houseBillNo);
			var oceanBillQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAOceanBillSchema.PK);
			oceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
			houseBillQuery.AddSubQuery(CusSCAHouseSchema.CA_CB, oceanBillQuery, JoinCondition.And);
			var houseBills = Factory.Load<CusSCAHouse>(houseBillQuery);
			var result = houseBills.Length > 0 ? Factory.Load<CusSCAHouse>(houseBillQuery).Select(x =>
			{
				var manifest = x.GetManifestInformation(null);
				var outturned = manifest != null && manifest.CargoReceivedAtDepotLogs.Count > 0 ? manifest.CargoReceivedAtDepotLogs[0].SL_EventTime : ZDateTime.MinSmallDateTimeValue;
				return Tuple.Create(outturned, x);
			}).Aggregate((x1, x2) => x1.Item1 > x2.Item1 ? x1 : x2) : null;
			return result != null && result.Item1.IsValid ? result.Item2 : null;
		}

		protected override bool IsConditionalClearToBeConsideredAsClear
		{
			get { return AUCustomsDataRegistry.Instance.SeaCargoOutturnScanningCONDCLEARReleaseStatus.Value == Core.Constants.AUCustoms.CondClearReleaseStatus.Clear; }
		}
	}
}
