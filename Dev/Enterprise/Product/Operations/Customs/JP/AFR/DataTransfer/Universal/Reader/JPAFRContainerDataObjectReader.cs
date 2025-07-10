using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRContainerDataObjectReader : BaseContainerDataObjectReader<JPAFRContainer>
	{
		public JPAFRContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, JPAFRBills bill)
			: base(containerDataObject, logger, factory)
		{
			this.bill = Argument.NotNull(bill, "bill");
			var billRow = GetColumnIndexer(bill);
			this.billPK = billRow.GetValue(JPAFRBillsSchema.PK);
		}
		readonly JPAFRBills bill;
		readonly ZGuid billPK;

		protected override JPAFRContainer GetExistingBusinessObject()
		{
			var query = new ZQuery(JPAFRContainerSchema.JPC_JPB_Bill, billPK);
			query.AddToFilter(JPAFRContainerSchema.JPC_ContainerNum, dataObject.ContainerNumber.GetValueOrDefault());
			query.FetchOnlyFromLocalCache = !bill.IsInDatabase;
			return factory.LoadTop1<JPAFRContainer>(query);
		}

		protected override void PopulateBusinessObject(JPAFRContainer containerBO)
		{
			var containerRow = GetColumnIndexer(containerBO);
			SetValue(containerRow, JPAFRContainerSchema.JPC_JPB_Bill, billPK);
			SetValue(containerRow, JPAFRContainerSchema.JPC_ContainerNum, dataObject.ContainerNumber);
			SetValue(containerRow, JPAFRContainerSchema.JPC_Seal1, dataObject.Seal);
			SetValue(containerRow, JPAFRContainerSchema.JPC_Seal2, dataObject.SecondSeal);
			SetValue(containerRow, JPAFRContainerSchema.JPC_IsEmpty, dataObject.IsEmptyContainer);
			SetValue(containerRow, JPAFRContainerSchema.JPC_RC_ContainerType, dataObject.ContainerType);
			SetValue(containerRow, JPAFRContainerSchema.JPC_OwnershipCode, dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Container.ContainerOwnershipCode, logger));
		}
	}
}
