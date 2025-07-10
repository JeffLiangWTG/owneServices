using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsReferenceDataObjectReader : Customs.DataTransfer.Universal.CustomsReferenceDataObjectReader
	{
		public CustomsReferenceDataObjectReader(UniversalCustoms.CustomsReference customsReferenceDataObject, IXmlImportLogger logger, ZGuid parentPK, ZString parentTableCode, UniversalObjectFactory factory)
			: base(customsReferenceDataObject, logger, parentPK, parentTableCode, factory)
		{
		}

		public override IColumnIndexer ReadIntoDataRowCusReference()
		{
			IColumnIndexer row = null;
			var upperCustomsReferenceType = dataObject.Type.GetCodeAsUpperCase();
			var upperCustomsReferenceCode = dataObject.SubType.GetCodeAsUpperCase();
			var customsReferenceReference = dataObject.Reference;
			if (!upperCustomsReferenceType.IsEmpty && !upperCustomsReferenceCode.IsEmpty && !string.IsNullOrWhiteSpace(customsReferenceReference))
			{
				var cusReferenceType = new CusReferenceTypeDecider().GetTypeForLoad(upperCustomsReferenceType, parentTableCode, parentPK, factory.BOFactory);
				row = CreateNewColumnIndexer(CusReferenceSchema.PK, cusReferenceType);
				if (row != null)
				{
					var ownerAddress = dataObject.Owner;
					SetValue(row, CusReferenceSchema.CFR_ParentID, parentPK);
					SetValue(row, CusReferenceSchema.CFR_ParentTableCode, parentTableCode);
					SetValue(row, CusReferenceSchema.CFR_Type, upperCustomsReferenceType);
					SetValue(row, CusReferenceSchema.CFR_Code, upperCustomsReferenceCode);
					SetValue(row, CusReferenceSchema.CFR_OA_Owner, ownerAddress != null ? new OrganisationDataObjectReader(ownerAddress, logger, factory).GetMatched()?.PK : null);
					SetValue(row, CusReferenceSchema.CFR_Reference, customsReferenceReference);
				}
			}
			return row;
		}
	}
}
