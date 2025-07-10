using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageDecValidation : CusTempStorageDecValidation
	{
		public CHGTSTCusTempStorageDecValidation(CHGTSTCusTempStorageDec parent) : base(parent)
		{
		}

		protected override void CheckSTH_OwnerReferenceNumber()
		{
			base.CheckSTH_OwnerReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.STH_OwnerReferenceNumberInfo);
		}

		public void ValidateNewCustodianBranch()
		{
			ValidateCalculatedProperty(Parent.NewCustodianBranchInfo);
		}

		protected void CheckNewCustodianBranch()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.NewCustodianBranchInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNewCustodianBranch();
		}

		protected new CHGTSTCusTempStorageDec Parent => (CHGTSTCusTempStorageDec)base.Parent;
	}
}
