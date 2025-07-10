using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class UniversalDataObjectReaderHelper : Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper
	{
		public UniversalDataObjectReaderHelper(UniversalObjectFactory factory, ZString sourceCountryCode)
			: base(factory, Core.Constants.CountryCodes.Canada, sourceCountryCode)
		{
		}

		protected override IEnumerable<KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>> GetAdditionalAddInfoGroupCollectionSupportForCore(ZString parentTableCode, ZString type, IXmlImportLogger logger)
		{
			if (parentTableCode == CusAddInfoSchema.Constants.Prefix && type == CusAddInfoTypeAttribute.Codes.CACCN)
			{
				yield return new KeyValuePair<ZString, IAdditionalAddInfoGroupCollectionDataObjectReader>(Constants.AddInfoKeys.CargoControlNumber.Type, new AdditionalAddInfoGroupCollectionDataObjectReaderForCargoControlNumber(logger, this));
			}
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderCore(IAddInfoManager addInfoManager, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, SchemaStringColumn column)
		{
			AddInfoDataObjectReader result = null;
			var type = addInfoManager.GetType();
			var universalDataObjectReaderHelper = helper as UniversalDataObjectReaderHelper;

			if (type == typeof(DFOPGAHeader))
			{
				result = new AddInfoDataObjectReaderForDFO(logger, universalDataObjectReaderHelper);
			}
			else if (type == typeof(ECCCPGAHeader))
			{
				result = new AddInfoDataObjectReaderForECCC(logger, universalDataObjectReaderHelper);
			}
			else if (type == typeof(HCPGAHeader))
			{
				result = new AddInfoDataObjectReaderForHC(logger, universalDataObjectReaderHelper);
			}
			else
			{
				result = base.GetNewAddInfoDataObjectReaderCore(addInfoManager, logger, helper, column);
			}

			return result;
		}

		protected override void DeleteOtherTableDataViaAddInfoGroupTypeCore(ZString addInfoGroupType, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
			if (addInfoGroupType.EqualsIgnoringCase(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType))
			{
				var query = new ZQuery(CusCALPCOSchema.CLP_ParentID, parentPK);
				query.AddToFilter(CusCALPCOSchema.CLP_ParentTableCode, parentTableCode);
				query.FetchOnlyFromLocalCache = isParentInDatabase.HasValue && !isParentInDatabase.Value;
				var lpcos = Factory.Load<CusCALPCO>(query);
				lpcos.DeleteAll(true, getAdditionalChildren: AdditionalChildrenHelper.GetAdditionalChildrenIfSupported);
			}
		}

		protected override IAddInfoGroupToOtherTableDataObjectReader GetAddInfoGroupToOtherTableDataObjectReaderCore(ZString parentTableCode, ZString addInfoGroupType, UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup addInfoGroup, IXmlImportLogger logger)
		{
			IAddInfoGroupToOtherTableDataObjectReader result = null;
			if ((parentTableCode == JobDeclarationSchema.Constants.Prefix || parentTableCode == CusAddInfoSchema.Constants.Prefix) && addInfoGroupType == Constants.AddInfoKeys.CusCALPCO.CusAddInfoType)
			{
				result = new LPCOAddInfoDataObjectReader(addInfoGroup, logger, this);
			}
			return result;
		}

		protected override IEnumerable<ZString> GetMatchingKeysInSettingOrderCore(IColumnIndexer row)
		{
			if (row is DutyAndTax dutyAndTax)
			{
				var order = new List<ZString>();
				CADutyAndTaxAddInfoSchema.All.ForEach(x =>
				{
					if (x.Name != CADutyAndTaxAddInfoSchema.C1_ExemptCode.Name)
					{
						order.Add(ColumnValueSetter.GetKey(dutyAndTax.PK, x));
					}
				});
				order.Add(ColumnValueSetter.GetKey(dutyAndTax.PK, CADutyAndTaxAddInfoSchema.C1_ExemptCode));
				return order;
			}

			return base.GetMatchingKeysInSettingOrderCore(row);
		}
	}
}
