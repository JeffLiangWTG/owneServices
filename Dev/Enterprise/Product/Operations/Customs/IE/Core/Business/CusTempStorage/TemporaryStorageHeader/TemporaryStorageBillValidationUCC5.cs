using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageBillValidationUCC5 : EU.Business.CusTempStorage.TemporaryStorageBillValidation
	{
		public TemporaryStorageBillValidationUCC5(AutoAsycudaBill parent) : base(parent)
		{
		}
		public new TemporaryStorageBill Parent => (TemporaryStorageBill)base.Parent;

		public override void ValidateAll()
		{
		}

		protected override void CheckABL_GrossWeight()
		{
		}

		protected override void CheckABL_GrossWeightUQ()
		{
		}

		protected override void CheckConsignorOrgPK()
		{
		}

		protected override void CheckABL_ShipperName()
		{
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
		}

		protected override void CheckABL_ShipperPostcode()
		{
		}

		protected override void CheckConsigneeOrgPK()
		{
		}

		protected override void CheckABL_ConsigneeName()
		{
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
		}

		protected override void CheckABL_ConsigneePostcode()
		{
		}

		protected override void CheckTypeOfBillDocument()
		{
		}

		protected override void CheckABL_BillNumber()
		{
		}

		protected override void CheckABL_BolType()
		{
		}

		protected override void CheckABL_ShipperRegNoType()
		{
		}

		protected override void CheckABL_ShipperState()
		{
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
		}

		protected override void CheckABL_ConsigneeState()
		{
		}

		protected override void CheckABL_NotifyPartyRegNoType()
		{
		}

		protected override void CheckABL_NotifyPartyState()
		{
		}
	}
}
