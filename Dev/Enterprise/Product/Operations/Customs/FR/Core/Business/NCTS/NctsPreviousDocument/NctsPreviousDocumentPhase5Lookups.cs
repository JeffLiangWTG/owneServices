using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using static Enterprise.Customs.FR.Business.Declaration.PreviousDocumentLookups;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPreviousDocumentPhase5Lookups : EU.NCTS.Business.NctsPreviousDocumentPhase5Lookups
	{
		public NctsPreviousDocumentPhase5Lookups(EU.NCTS.Business.NctsPreviousDocument parent) : base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		public override ICollection ReferenceList
		{
			get
			{
				var previousDoc = Parent;
				if (previousDoc.CSI_Code == UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage)
				{
					var result = new CusTempStorageRegHeaderCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.Status, "Property", (ZString)TempStorageDeclarationStatusList.Codes.Open, false));

					var departureCustomsOffice = Parent.Parent?.Header?.MovementHeader.DepartureCustomsOfficeCode;
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, "Property", departureCustomsOffice));

					var reference = previousDoc.CSI_ReferenceNumber;
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.TsdNumber, "Property", reference));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.PreviousReferenceNumber, "Property", ZString.Empty));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusTempStorageRegHeaderCollectionFilterConstants.PackingType, "Property", ZString.Empty));

					return result;
				}
				else
				{
					return base.ReferenceList;
				}
			}
		}
	}
}
