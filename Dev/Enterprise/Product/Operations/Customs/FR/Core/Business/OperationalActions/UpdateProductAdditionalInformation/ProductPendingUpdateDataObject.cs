using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class ProductPendingUpdateDataObject : AutoProductPendingUpdateDataObject
	{
		public ProductPendingUpdateDataObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Description

		[ReadOnlyMember(nameof(Description_ReadOnly))]
		public override ZString Description
		{
			get { return base.Description; }
			set	{ base.Description = value; }
		}

		ZBool Description_ReadOnly => true;

		void SetDescriptionByProductPk(ZGuid productPk)
		{
			var product = Factory.Load<OrgSupplierPart>(productPk);
			Description = product?.OP_Desc ?? ZString.Empty;
		}

		#endregion

		#region ProductPk

		[List(nameof(Lookups) + "." + nameof(ProductPendingUpdateDataObjectLookups.Products))]
		public override ZGuid ProductPk
		{
			get { return base.ProductPk; }
			set
			{
				base.ProductPk = value;
				SetDescriptionByProductPk(ProductPk);
			}
		}

		#endregion

		#region CustomsType

		[List(nameof(Lookups) + "." + nameof(ProductPendingUpdateDataObjectLookups.CustomsTypes))]
		public override ZString CustomsType
		{
			get { return base.CustomsType; }
			set { base.CustomsType = value; }
		}

		#endregion

		#region OrganizationPk

		[List(nameof(Lookups) + "." + nameof(ProductPendingUpdateDataObjectLookups.RelatedOrgs))]
		public override ZGuid OrganizationPk
		{
			get { return base.OrganizationPk; }
			set { base.OrganizationPk = value; }
		}

		#endregion

		#region Lookups

		public ProductPendingUpdateDataObjectLookups Lookups
		{
			get { return lookups ?? (lookups = new ProductPendingUpdateDataObjectLookups(this)); }
		}

		ProductPendingUpdateDataObjectLookups lookups;

		#endregion
	}
}
