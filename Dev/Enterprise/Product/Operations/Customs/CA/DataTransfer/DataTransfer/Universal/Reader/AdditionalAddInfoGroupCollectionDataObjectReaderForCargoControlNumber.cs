using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class AdditionalAddInfoGroupCollectionDataObjectReaderForCargoControlNumber : DataObjectReader, IAdditionalAddInfoGroupCollectionDataObjectReader
	{
		public AdditionalAddInfoGroupCollectionDataObjectReaderForCargoControlNumber(IXmlImportLogger logger, UniversalDataObjectReaderHelper helper)
			: base(logger)
		{
			this.helper = Argument.NotNull(helper, "Helper");
		}
		readonly UniversalDataObjectReaderHelper helper;

		public void Process(IEnumerable<AddInfoGroup> addInfoGroupCollection, ZGuid parentPK, ZString parentTableCode, bool? isParentInDatabase)
		{
			var cargoControlNumber = parentTableCode == CusAddInfoSchema.Constants.Prefix ? helper.Load<CargoControlNumber>(parentPK) : null;
			var ccnRow = GetColumnIndexer(cargoControlNumber);
			var declaration = cargoControlNumber == null ? null : helper.Load<JobDeclaration>(ccnRow, CusAddInfoSchema.B7_ParentID);
			if (declaration != null)
			{
				var declarationIsInDatabase = declaration.IsInDatabase;
				foreach (var addInfoGroup in addInfoGroupCollection)
				{
					ZString? ccnInfoNumber = addInfoGroup.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.CCNumber, logger);
					ZString? billType = addInfoGroup.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillType, logger);
					ZString? billNumber = addInfoGroup.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillNumber, logger);

					if (ccnInfoNumber.HasValue && ccnInfoNumber.GetValueOrDefault() == cargoControlNumber.CY_CargoControlNumber)
					{
						if (billNumber.HasValue && !billNumber.GetValueOrDefault().IsEmpty || billType.HasValue && !billType.GetValueOrDefault().IsEmpty)
						{
							var billQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declaration.PK);
							billQuery.FetchOnlyFromLocalCache = !declarationIsInDatabase;
							var bills = helper.Load<Bill>(billQuery);
							var bill = bills.FirstOrDefault(x => x.CU_BillType == billType.GetValueOrDefault() && x.CU_BillNum == billNumber.GetValueOrDefault());
							if (bill != null)
							{
								cargoControlNumber.CA_CU_CCNInfoBill = bill.PK;
							}
						}
					}
				}
			}
		}
	}
}
