using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsPreviousDocumentPhase4Lookups : EU.NCTS.Business.NctsPreviousDocumentPhase4Lookups
	{
		public NctsPreviousDocumentPhase4Lookups(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		public override ICollection ReferenceList
		{
			get
			{
				var dataValue = ZString.Empty;
				var header = Parent.Parent?.MoveHeader?.Header;
				if (header != null)
				{
					var customsOffices = header.CustomsOffices;
					dataValue = customsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)?.CY_Data ?? ZString.Empty;
				}

				return Factory.GetCachedValue("FRSupportingDocumentLookups.ReferenceNumberList.Ncts." + dataValue, () =>
				{
					var referenceNumberList = new CusTempStorage.CusTempStorageRegHeaderCollection(Factory);
					referenceNumberList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.Status, "Property", (ZString)TempStorageDeclarationStatusList.Codes.Open, false));
					referenceNumberList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Declaration.PreviousDocumentLookups.CusTempStorageRegHeaderCollectionFilterConstants.CustomsOffice, "Property", dataValue, true));
					return referenceNumberList;
				});
			}
		}
	}
}
