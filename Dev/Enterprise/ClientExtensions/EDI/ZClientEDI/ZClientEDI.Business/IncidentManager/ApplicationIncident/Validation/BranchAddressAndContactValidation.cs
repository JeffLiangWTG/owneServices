using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class BranchAddressAndContactValidation : AutoIncidentMainValidation
	{
		public BranchAddressAndContactValidation(AutoIncidentMain parent)
			: base(parent)
		{
		}

		protected override void CheckIM_OH_Client()
		{
			base.CheckIM_OH_Client();
			ValidateIM_OA_BranchAddress();
		}

		protected override void CheckIM_OA_BranchAddress()
		{
			base.CheckIM_OA_BranchAddress();

			MandatoryValidation.CheckEntered(Parent.IM_OA_BranchAddressInfo);
		}

		protected override void CheckIM_OC_Contact()
		{
			base.CheckIM_OC_Contact();
			MandatoryValidation.CheckEntered(Parent.IM_OC_ContactInfo);
		}
	}
}

