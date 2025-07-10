using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitConsignmentValidation : EU.ExitControl.Business.CusExitConsignmentValidation
	{
		public CusExitConsignmentValidation(CusExitConsignment parent)
			: base(parent)
		{
		}

		protected override void CheckCXC_MovementReference()
		{
			base.CheckCXC_MovementReference();

			var parent = Parent;
			var mrnError = MRNFormatValidator.CheckMRNFormat(parent.CXC_MovementReference, parent.Factory, ZString.Empty);
			if (!mrnError.IsEmpty)
			{
				parent.CXC_MovementReferenceInfo.AddMessageError(mrnError);
			}

			if (parent.CXC_MovementReference.Length > 18)
			{
				parent.CXC_MovementReferenceInfo.AddMessageError(Res.GetString("b786b962-ffff-4b72-af00-7449460cb0c5", "The MRN must not exceed a length of 18 characters."));
			}
		}

		protected override void CheckCXC_MovementReference_Mandatory()
		{
			if (Parent.CXC_LocalReference.IsEmpty)
			{
				base.CheckCXC_MovementReference_Mandatory();
			}
		}

		new CusExitConsignment Parent => (CusExitConsignment)base.Parent;
	}
}
