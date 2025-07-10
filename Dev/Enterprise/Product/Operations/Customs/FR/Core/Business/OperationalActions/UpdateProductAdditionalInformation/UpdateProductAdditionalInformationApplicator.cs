using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class UpdateProductAdditionalInformationApplicator : OperationalActionMethodApplicator
	{
		public UpdateProductAdditionalInformationApplicator(BusinessObjectFactory factory, IApplicatorValidationSupport updateProductAdditionalInformationSupport)
			: base((NoResString)"Update Product Additional Information", factory)
		{
		}

		public ProductPendingUpdateDataObjectCollection ProductPendingUpdateDataForBinding => productPendingUpdateDataObjectCollection ??= new ProductPendingUpdateDataObjectCollection(Factory);
		ProductPendingUpdateDataObjectCollection productPendingUpdateDataObjectCollection;

		protected override void BuildCore(ZGuid[] selectItemPKs)
		{
			foreach (var selectItemPK in selectItemPKs)
			{
				var product = Factory.Load<OrgSupplierPart>(selectItemPK);
				var productPendingUpdateDataObject = ProductPendingUpdateDataForBinding.AddNew();
				productPendingUpdateDataObject.Description = product.OP_Desc;
				productPendingUpdateDataObject.ProductPk = product.PK;
			}
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
		}
	}
}
