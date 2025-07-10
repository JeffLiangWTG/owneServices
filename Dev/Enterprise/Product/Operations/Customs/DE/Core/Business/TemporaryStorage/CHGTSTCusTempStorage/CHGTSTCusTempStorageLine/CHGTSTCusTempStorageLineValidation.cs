using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CHGTSTCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CHGTSTCusTempStorageLine Parent => (CHGTSTCusTempStorageLine)base.Parent;

		protected override void CheckTSL_LineNo()
		{
			base.CheckTSL_LineNo();
			CheckLineNoIsUnique();
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_OwnerReferenceType();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
			}
		}

		protected override void CheckTSL_CustodianIdentifier()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifier();
			}
		}

		protected override void CheckTSL_CustodianIdentifierBranchNo()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifierBranchNo();
			}
		}

		protected override void CheckTSL_LocationOfGoodsListValidation()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TSL_LocationOfGoodsInfo);
		}
	}
}
