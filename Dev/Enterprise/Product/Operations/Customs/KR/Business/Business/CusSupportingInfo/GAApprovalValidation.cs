using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class GAApprovalValidation : CusSupportingInfoValidation
	{
		public GAApprovalValidation(GAApproval parent)
			: base(parent)
		{
		}

		new GAApproval Parent => (GAApproval)base.Parent;

		protected override void CheckCSI_Procedure()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ProcedureInfo);
			foreach (var gaApproval in Parent.Parent.GAApprovalDataCollection)
			{
				if (gaApproval.PK != Parent.PK && gaApproval.CSI_Procedure == Parent.CSI_Procedure)
				{
					Parent.CSI_ProcedureInfo.AddMessageError(Res.GetString("2D1BCFA2-A4B9-4526-A6C2-3C57A098AA12", "You cannot enter a 'Regulation Category' that already exists."));
					break;
				}
			}
		}

		protected override void CheckCSI_SubType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
		}
	}
}
