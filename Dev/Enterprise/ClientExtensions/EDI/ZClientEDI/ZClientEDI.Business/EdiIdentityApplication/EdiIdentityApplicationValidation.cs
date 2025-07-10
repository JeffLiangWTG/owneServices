//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityApplicationValidation
//
//    This class should be used for overriding validation in AutoEdiIdentityApplicationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.IdentityApplication.Business
{
	using CargoWise.EntityFramework;
	using Res = ZClientEDI.Business.Res;

	public class EdiIdentityApplicationValidation : AutoEdiIdentityApplicationValidation
	{
		public EdiIdentityApplicationValidation(AutoEdiIdentityApplication parent) : base(parent)
		{
		}

		public new EdiIdentityApplication Parent => (EdiIdentityApplication)base.Parent;

		protected override void CheckIDA_ApplicationName()
		{
			base.CheckIDA_ApplicationName();
			MandatoryValidation.CheckEntered(Parent.IDA_ApplicationNameInfo);
		}

		protected override void CheckIDA_ApplicationType()
		{
			base.CheckIDA_ApplicationType();
			ListValidation.ErrorIfInvalidCode(Parent.IDA_ApplicationTypeInfo, Parent.Lookups.DatabaseTypesList);
		}

		protected override void CheckIDA_Product()
		{
			base.CheckIDA_Product();
			ListValidation.ErrorIfInvalidCode(Parent.IDA_ProductInfo, Parent.Lookups.ProductTypeList);
			if (Parent.IDA_LD.IsEmpty && ProductTypes.IsEnterpriseFamily(Parent.IDA_Product))
			{
				Parent.IDA_ProductInfo.AddError(Res.GetString("C3003BA1-01D7-4653-A84F-FEDB1000567D", "You must have an active license when selecting a CargoWise product."));
			}
		}

		protected override void CheckIDA_IDA_ParentApplication()
		{
			base.CheckIDA_IDA_ParentApplication();

			if (Parent.IsCustomerApplication)
			{
				if (Parent.IDA_OH_ParentOrg.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.IDA_IDA_ParentApplicationInfo);
				}

				if (Parent.ParentApplication != null && Parent.ParentApplication.IDA_LD.IsEmpty)
				{
					Parent.IDA_IDA_ParentApplicationInfo.AddError(Res.GetString("482C164F-0F67-497C-9F41-5C1976928CD8", "Please enter a CW1 Application."));
				}
			}
		}

		protected override void CheckIDA_OH_ParentOrg()
		{
			base.CheckIDA_OH_ParentOrg();

			if (Parent.IsCustomerApplication && Parent.IDA_IDA_ParentApplication.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.IDA_OH_ParentOrgInfo);
			}
		}
	}
}
