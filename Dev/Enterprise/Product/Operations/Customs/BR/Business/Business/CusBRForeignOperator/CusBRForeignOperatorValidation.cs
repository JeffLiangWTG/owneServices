using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class CusBRForeignOperatorValidation : AutoCusBRForeignOperatorValidation
	{
		public CusBRForeignOperatorValidation(AutoCusBRForeignOperator parent) : base(parent)
		{
		}

		protected override void CheckBFR_OH_Owner()
		{
			base.CheckBFR_OH_Owner();

			var owner = Parent.Owner;
			if (owner != null)
			{
				var catalogRootCnpj = owner.GetRootCNPJ();
				var ownerInfo = Parent.BFR_OH_OwnerInfo;
				if (catalogRootCnpj.IsEmpty)
				{
					ownerInfo.AddMessageError(Res.GetString("A2E1A5E7-BA98-473E-8732-86A4F7DB3BF1", "Please enter a Root CNPJ in this organization to continue."));
				}

				if (!owner.OH_IsConsignee && !owner.OH_IsConsignor)
				{
					ownerInfo.AddMessageError(Res.GetString("D8A1D38B-B5BC-40E2-8577-882ECFD5BD9C", "An Owner should be designated as either a Consignor or a Consignee."));
				}
			}
		}

		protected override void CheckBFR_OH_ForeignOperator()
		{
			base.CheckBFR_OH_ForeignOperator();
			var foreignOperator = Parent.ForeignOperator;
			var foreignOperatorInfo = Parent.BFR_OH_ForeignOperatorInfo;
			MandatoryValidation.CheckEntered(foreignOperatorInfo);
			if (foreignOperator != null && foreignOperator.CountryCode == Core.Constants.CountryCodes.Brazil)
			{
				foreignOperatorInfo.AddMessageError(Res.GetString("8A782678-B9A0-4936-9FE1-0C3FE1FF34B6", "A Foreign Operator should not have its Country set to BR."));
			}
		}
	}
}
