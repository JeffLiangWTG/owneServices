using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CHGOFFCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CHGOFFCusTempStorageLine Parent => (CHGOFFCusTempStorageLine)base.Parent;

		protected override void CheckTSL_LineNo()
		{
			base.CheckTSL_LineNo();
			if (Parent.IsREGDeclaration)
			{
				CheckLineNoIsUnique();
			}
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_OwnerReferenceType();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
			}
		}

		protected override void CheckTSL_CustodianIdentifierMandatory()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifierMandatory();
			}
		}

		protected override void CheckTSL_CustodianIdentifierBranchNo()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifierBranchNo();
			}
		}
	}
}
