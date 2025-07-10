using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnDataObjectReader : ShipmentDataObjectReader<CusOutturn>
	{
		public CusOutturnDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusUnderbond underbond) : base(dataObject, logger, factory)
		{
			this.underbond = Argument.NotNull(underbond, nameof(underbond));
			this.cusMAWB = Argument.NotNull(underbond.MAWB, "underbond.MAWB");

			var wayBillType = dataObject.WayBillType?.Code.GetValueOrDefault() ?? ZString.Empty;
			hawbNumber = wayBillType == WayBillTypeList.Codes.House ? dataObject.WayBillNumber.GetValueOrDefault() : ZString.Empty;
			cusHAWB = hawbNumber.IsEmpty ? null : cusMAWB.ChildBills.Cast<CusHAWB>().FirstOrDefault(hb => hb.CS_HAWB == hawbNumber);
		}

		protected override void PopulateBusinessObject(CusOutturn targetBO)
		{
			var outturnRow = GetColumnIndexer(targetBO);
			SetValue(outturnRow, CusOutturnSchema.C5_C4_Underbond, underbond.PK);
			SetValue(outturnRow, CusOutturnSchema.C5_OuterPacks, dataObject.OuterPacks.GetValueOrDefault());
			SetValue(outturnRow, CusOutturnSchema.C5_OuterPackUnits, dataObject.OuterPacksPackageType.GetNullableCodeAsUpperCase() ?? ZString.Empty);
			// C5_PackagesOutturned needs to be set up before C5_OutturnResultType or the result will be updated automatically
			SetValue(outturnRow, CusOutturnSchema.C5_PackagesOutturned, dataObject.TotalNoOfPacks);
			SetValue(outturnRow, CusOutturnSchema.C5_OutturnResultType, dataObject.ShipmentType.GetNullableCodeAsUpperCase() ?? ZString.Empty);

			FillAddInfos(outturnRow);
			FillNotes(outturnRow);

			cusMAWB.LoadChildEditableObjects();
			if (cusHAWB == null)
			{
				cusHAWB = cusMAWB.ChildBills.AddNew();
				cusHAWB.CS_HAWB = hawbNumber;
				cusHAWB.CS_GoodsDescription = targetBO.C5_GoodsDescription;
				cusHAWB.CS_GoodsValue = dataObject.GoodsValue ?? 0;
				cusHAWB.CS_Weight = dataObject.TotalWeight ?? 0;
				cusHAWB.CS_WeightUQ = dataObject.TotalWeightUnit?.Code ?? ZString.Empty;
			}
			SetValue(outturnRow, CusOutturnSchema.C5_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			SetValue(outturnRow, CusOutturnSchema.C5_ParentID, cusHAWB.PK);
		}

		void FillAddInfos(IColumnIndexer outturnRow)
		{
			var addInfos = dataObject.AddInfoCollection;
			if (addInfos?.Any() ?? false)
			{
				FillAddInfosCore(outturnRow, addInfos, CusOutturnSchema.C5_DamageIndicator, Outturn.Constants.AddInfoType.IsDamage);
				FillAddInfosCore(outturnRow, addInfos, CusOutturnSchema.C5_PillageIndicator, Outturn.Constants.AddInfoType.IsPillage);
			}
		}

		void FillAddInfosCore(IColumnIndexer outturnRow, IEnumerable<AddInfo> addInfos, SchemaBoolColumn column, ZString key)
		{
			var addInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
			if (addInfo != null)
			{
				SetValue(outturnRow, column, addInfo.Value.GetValueOrDefault() == YesNoList.Codes.Yes ? ZBool.True : ZBool.False);
			}
		}

		void FillNotes(IColumnIndexer outturnRow)
		{
			var notes = dataObject.NoteCollection;
			if (notes?.Any() ?? false)
			{
				FillNotes(outturnRow, notes, CusOutturnSchema.C5_GoodsDescription, Outturn.Constants.Note.Descriptions.GoodsDescription);
			}
		}

		void FillNotes(IColumnIndexer outturnRow, IEnumerable<Note> notes, SchemaStringColumn column, ZString description)
		{
			var note = notes.FirstOrDefault(x => x.Description.GetValueOrDefault() == description);
			if (note != null)
			{
				SetValue(outturnRow, column, note.NoteText.GetValueOrDefault());
			}
		}

		protected override CusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusOutturn result = null;
			if (cusHAWB != null)
			{
				var query = new ZQuery(CusOutturnSchema.C5_C4_Underbond, underbond.PK);
				query.FetchOnlyFromLocalCache = !underbond.IsInDatabase;
				query.AddToFilter(CusOutturnSchema.C5_ParentID, cusHAWB.PK);
				result = factory.LoadTop1<CusOutturn>(query);
			}

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
