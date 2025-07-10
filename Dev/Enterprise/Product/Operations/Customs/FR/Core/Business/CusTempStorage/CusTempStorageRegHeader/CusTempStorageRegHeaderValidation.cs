using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderValidation : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderValidation
	{
		public CusTempStorageRegHeaderValidation(AutoCusTempStorageRegHeader parent) : base(parent)
		{
		}

		protected new CusTempStorageRegHeader Parent => (CusTempStorageRegHeader)base.Parent;

		protected override void CheckSRH_Status()
		{
			base.CheckSRH_Status();
			if (!Parent.IsDeclarationClosed && Parent.ShouldBeClosed)
			{
				Parent.SRH_StatusInfo.AddError(Res.GetString("cbf75548-97fd-4651-a2dc-1f5634ead7ff", "Temp. register header status can't be open if the sum of packages in all transactions equals zero."));
			}
		}
	}
}
