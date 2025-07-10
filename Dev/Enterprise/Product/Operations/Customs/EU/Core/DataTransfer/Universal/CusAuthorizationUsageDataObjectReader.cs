using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CusAuthorizationUsageDataObjectReader : Customs.DataTransfer.Universal.CustomsReferenceDataObjectReader
	{
		public CusAuthorizationUsageDataObjectReader(UniversalCustoms.CustomsReference customsReferenceDataObject, IXmlImportLogger logger, ZGuid parentPK, ZString parentTableCode, UniversalObjectFactory factory)
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
				row = GetColumnIndexer(factory.New<CusAuthorizationUsage>());
				if (row != null)
				{
					var ownerAddress = dataObject.Owner;
					SetValue(row, CusAuthorizationUsageSchema.AGC_ParentID, parentPK);
					SetValue(row, CusAuthorizationUsageSchema.AGC_ParentTableCode, parentTableCode);
					SetValue(row, CusAuthorizationUsageSchema.AGC_Number, customsReferenceReference);
					SetValue(row, CusAuthorizationUsageSchema.AGC_Code, upperCustomsReferenceCode);
					var matchedOrganisation = ownerAddress != null ? new OrganisationDataObjectReader(ownerAddress, logger, factory).GetMatched(true)?.OA_OH ?? OrgHeader.UnmatchedOrganisationPK : OrgHeader.UnmatchedOrganisationPK;
					SetValue(row, CusAuthorizationUsageSchema.AGC_OH_Owner, matchedOrganisation);

					if (ownerAddress == null)
					{
						logger.Log(Integration.LogType.Information, Res.GetString("350E97B7-D8BE-4803-A63D-F895139414DD", "There are not any owner details for Authorization {0} - using UNMATCHED", dataObject.Reference));
					}
					else if (matchedOrganisation == OrgHeader.UnmatchedOrganisationPK)
					{
						logger.Log(Integration.LogType.Information, Res.GetString("AE79D2BF-7530-4D81-A344-885C382F86BA", "Organization {0} could not be matched for Authorization {1} - using UNMATCHED", ownerAddress.OrganizationCode, dataObject.Reference));
					}
				}
			}
			return row;
		}
	}
}
