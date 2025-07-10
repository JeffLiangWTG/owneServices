using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonCountryValidation : Customs.Business.CusPersonCountryValidation
	{
		public CusPersonCountryValidation(CusPersonCountry parent)
			: base(parent)
		{ }

		public new CusPersonCountry Parent
		{
			get { return (CusPersonCountry)base.Parent; }
		}

		protected override void CheckCPC_RN_NKCountry()
		{
			base.CheckCPC_RN_NKCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CPC_RN_NKCountryInfo);
			ValidateForConstraint(Parent.CPC_RN_NKCountryInfo);
		}

		protected override void CheckCPC_Type()
		{
			base.CheckCPC_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CPC_TypeInfo);
			ValidateForConstraint(Parent.CPC_TypeInfo);
		}

		protected override void CheckCPC_CPN_Person()
		{
			base.CheckCPC_CPN_Person();
			ValidateForConstraint(Parent.CPC_CPN_PersonInfo);
		}

		protected override void CheckCPC_Value()
		{
			base.CheckCPC_Value();
			var targetInfo = Parent.CPC_ValueInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		void ValidateForConstraint(ZPropertyInfo zpi)
		{
			// Person & country & type is unique
			var parentPerson = Parent.ParentPerson;
			if (parentPerson != null && !parentPerson.IsDeleted && !parentPerson.IsDeleting)
			{
				foreach (CusPersonCountry cpc in parentPerson.Countries.Where(c => !c.IsDeleted && c.PK != Parent.PK))
				{
					if (cpc.CPC_RN_NKCountry == Parent.CPC_RN_NKCountry && cpc.CPC_Type == Parent.CPC_Type)
					{
						zpi.AddError("Country and Type must be unique per Person");
						break;
					}
				}
			}
		}
	}
}
