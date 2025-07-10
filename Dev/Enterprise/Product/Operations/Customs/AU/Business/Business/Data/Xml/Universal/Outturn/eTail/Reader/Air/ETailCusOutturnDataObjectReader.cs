using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ETailCusOutturnDataObjectReader : ShipmentDataObjectReader<CusOutturn>
	{
		public ETailCusOutturnDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusUnderbond underbond) : base(dataObject, logger, factory)
		{
			this.underbond = Argument.NotNull(underbond, nameof(underbond));
			cusMAWB = Argument.NotNull(underbond.MAWB, "underbond.MAWB");
			var wayBillType = dataObject.WayBillType;
			if (wayBillType != null && wayBillType.Code.GetValueOrDefault() == WayBillTypeList.Codes.House)
			{
				hawbNumber = dataObject.WayBillNumber.GetValueOrDefault();
			}
			else
			{
				hawbNumber = dataObject.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key.GetValueOrDefault() ?? ZString.Empty;
			}
			cusHAWB = cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(hb => hb.CS_HAWB == hawbNumber);
		}

		protected override void PopulateBusinessObject(CusOutturn targetBO)
		{
			var outturnRow = GetColumnIndexer(targetBO);
			var totalPieces = dataObject.TotalNoOfPieces;
			var outturnedPieces = 0;
			var isDamaged = false;
			var isPillaged = false;
			var goodsDesc = dataObject.GoodsDescription.GetValueOrDefault();

			foreach (var line in dataObject.PackingLineCollection)
			{
				if (!isDamaged && line.OutturnDamagedQty.GetValueOrDefault() > 0)
				{
					isDamaged = true;
				}
				if (!isPillaged && line.OutturnPillagedQty.GetValueOrDefault() > 0)
				{
					isPillaged = true;
				}
				outturnedPieces += line.OutturnQty.GetValueOrDefault();
			}
			SetOutturnValues(outturnRow, totalPieces, outturnedPieces, isDamaged, isPillaged, goodsDesc);
		}

		void SetOutturnValues(IColumnIndexer outturnRow, ZInt? totalPieces, ZInt outturnedPieces, bool isDamaged, bool isPillaged, ZString goodsDesc)
		{
			SetValue(outturnRow, CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			SetValue(outturnRow, CusOutturnSchema.C5_OuterPacks, totalPieces);
			// C5_PackagesOutturned needs to be set up before C5_OutturnResultType or the result will be updated automatically, which would be false in case of SurplusConsignment
			SetValue(outturnRow, CusOutturnSchema.C5_PackagesOutturned, outturnedPieces);
			SetValue(outturnRow, CusOutturnSchema.C5_DamageIndicator, isDamaged);
			SetValue(outturnRow, CusOutturnSchema.C5_PillageIndicator, isPillaged);
			SetValue(outturnRow, CusOutturnSchema.C5_GoodsDescription, goodsDesc);

			cusMAWB.LoadChildEditableObjects();
			if (cusHAWB != null)
			{
				if (totalPieces == outturnedPieces)
				{
					SetValue(outturnRow, CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.NilDiscrepancy);
				}
				else if (outturnedPieces < totalPieces)
				{
					SetValue(outturnRow, CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.ShortLanded);
				}
				else if (outturnedPieces > totalPieces && totalPieces > 0)
				{
					SetValue(outturnRow, CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusPackages);
				}
			}
			else
			{
				cusHAWB = cusMAWB.ChildBills.AddNew();
				cusHAWB.CS_HAWB = hawbNumber;
				cusHAWB.CS_GoodsDescription = goodsDesc;
				cusHAWB.CS_GoodsValue = dataObject.GoodsValue ?? 0;
				cusHAWB.CS_Weight = dataObject.TotalWeight ?? 0;
				cusHAWB.CS_WeightUQ = dataObject.TotalWeightUnit.Code ?? ZString.Empty;

				SetValue(outturnRow, CusOutturnSchema.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusConsignment);
			}
			SetValue(outturnRow, CusOutturnSchema.C5_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			SetValue(outturnRow, CusOutturnSchema.C5_ParentID, cusHAWB.PK);
		}

		protected override CusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (cusHAWB == null)
			{
				return null;
			}
			var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			query.AddToFilter(CusOutturnSchema.C5_ParentID, cusHAWB.PK);
			query.FetchOnlyFromLocalCache = !underbond.IsInDatabase;
			var result = factory.LoadTop1<CusOutturn>(query);

			return result;
		}

		protected override IMatchingBusinessEntityFinder<CusOutturn> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for Outturn. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		public override DataContextType DataContextType => DataContextType.Outturn;

		readonly CusUnderbond underbond;
		readonly CusMAWB cusMAWB;
		readonly ZString hawbNumber;
		CusHAWB cusHAWB;
	}
}
