using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgCusCodeValidation : OrgCusCodeValidation
	{
		public UPEOrgCusCodeValidation(AutoOrgCusCode parent)
			: base(parent)
		{
		}

		protected override void CheckOK_CodeType()
		{
			base.CheckOK_CodeType();
			ValidateUANIsUnique();
		}

		void ValidateUANIsUnique()
		{
			if (Parent.OK_CodeType == UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber)
			{
				ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, Parent.OK_CodeType);
				filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Parent.OK_CustomsRegNo);
				filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Parent.OK_RN_NKCodeCountry);
				BusinessObject[] cusCodes = Parent.Factory.Load(typeof(OrgCusCode), filter);
				if (cusCodes.Length > 1)
				{
					Parent.OK_CodeTypeInfo.AddError("This Registration No is used in another organisation.");
				}
			}
		}
	}
}
