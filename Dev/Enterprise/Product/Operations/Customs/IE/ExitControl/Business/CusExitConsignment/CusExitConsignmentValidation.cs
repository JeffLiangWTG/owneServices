using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentValidation : EU.ExitControl.Business.CusExitConsignmentValidation
	{
		public CusExitConsignmentValidation(CusExitConsignment parent)
			: base(parent)
		{
		}

		protected new CusExitConsignment Parent => (CusExitConsignment)base.Parent;

		protected override void CheckCXC_MovementReference()
		{
			var mrn = Parent.CXC_MovementReference;
			if (mrn.IsEmpty)
			{
				var info = Parent.CXC_MovementReferenceInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.Description));
			}
			else
			{
				var regex = new Regex("([0-3][0-9])[A-Z]{2}[A-Z0-9]{13}[0-9]");
				if (!regex.Match(mrn).Success)
				{
					var errorString = Res.GetString("D5E690D2-56DD-4FF2-A2B4-529E8417D9AD", "MRN is not in valid format");
					Parent.CXC_MovementReferenceInfo.AddMessageError(errorString);
				}
			}
		}
	}
}
