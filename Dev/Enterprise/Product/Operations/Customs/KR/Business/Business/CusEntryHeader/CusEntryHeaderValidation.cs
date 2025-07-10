using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryHeaderValidation : AutoKRCusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFreight();
			ValidateInsurance();
			ValidateTotalPackages();
			ValidateCustomsValue();
		}

		public void ValidateFreight()
		{
			ValidateCalculatedProperty(Parent.FreightInfo);
		}

		protected void CheckFreight()
		{
			if (!IncotermList.IsFreightExcluded(Parent.RandomHeader.JZ_IncoTerm))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.FreightInfo);
			}
			if (Parent.RandomHeader.JZ_IncoTerm == IncotermList.Codes.FreeOnBoard && Parent.Freight > 0)
			{
				Parent.FreightInfo.AddMessageError(Res.GetString("ED69A782-7DF9-4AF1-8CC3-45E5F0DF58C7", "If Incoterm is ‘FOB’ then, Freight must be zero."));
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.FreightInfo);
		}

		public void ValidateInsurance()
		{
			ValidateCalculatedProperty(Parent.InsuranceInfo);
		}

		protected void CheckInsurance()
		{
			if (!IncotermList.IsInsuranceExcluded(Parent.RandomHeader.JZ_IncoTerm))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.InsuranceInfo);
			}
			if (Parent.RandomHeader.JZ_IncoTerm == IncotermList.Codes.FreeOnBoard && Parent.Insurance > 0)
			{
				Parent.InsuranceInfo.AddMessageError(Res.GetString("D552C3E7-58E5-476D-9CF4-ABB20FB3C66E", "If Incoterm is ‘FOB’ then, Insurance must be zero."));
			}
			MandatoryValidation.MessageErrorIfIsNegative(Parent.InsuranceInfo);
		}

		public void ValidateTotalPackages()
		{
			ValidateCalculatedProperty(Parent.TotalPackagesInfo);
		}

		protected void CheckTotalPackages()
		{
			if (!PackageKindCodeList.IsBulk(Parent.PackagesUQ))
			{
				if (Parent.TotalPackages <= 0)
				{
					Parent.TotalPackagesInfo.AddMessageError(Res.GetString("47635D92-F06B-4166-9B40-A85C6863BA80", "If 'Pack Type' is not 'bulk', then 'Total Packages' needs to be bigger than 0."));
				}
			}
		}

		public void ValidateCustomsValue()
		{
			ValidateCalculatedProperty(Parent.CustomsValueInfo);
		}

		protected void CheckCustomsValue()
		{
			MandatoryValidation.MessageErrorIfIsNegative(Parent.CustomsValueInfo);
		}
	}
}
