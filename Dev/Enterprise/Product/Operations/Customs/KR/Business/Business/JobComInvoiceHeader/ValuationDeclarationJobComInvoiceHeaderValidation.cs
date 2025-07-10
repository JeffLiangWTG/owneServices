using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ValuationDeclarationJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateValuationQuestion5A();
			ValidateValuationQuestion5B();
			ValidateValuationQuestion5C();
			ValidateValuationQuestion5D();
			ValidateValuationQuestion5EA();
			ValidateValuationQuestion5EB();
			ValidateValuationQuestion6A();
			ValidateValuationQuestion6B();
			ValidateValuationQuestion7A_5SM();
			ValidateValuationQuestion7B_5SM();
			ValidateValuationQuestion8A();
			ValidateValuationQuestion8B();
			ValidateValuationQuestion8C();
			ValidateValuationQuestion8D();
			ValidateValuationQuestion9A();
			ValidateValuationQuestion9B();
			ValidateValuationQuestion10A();
			ValidateValuationQuestion10B();
			ValidateValuationQuestion10C();
			ValidateValuationQuestion10D();
			ValidateValuationQuestion11A();
			ValidateValuationQuestion11B();
			ValidateValuationQuestion11C();
			ValidateValuationQuestion11D();
		}

		public void ValidateValuationQuestion5A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5AInfo);
		}

		protected void CheckValuationQuestion5A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion5AInfo);
			}
		}

		public void ValidateValuationQuestion5B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5BInfo);
		}

		protected void CheckValuationQuestion5B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne && Parent.ValuationQuestion5A == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion5BInfo);
			}
		}

		public void ValidateValuationQuestion5C()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5CInfo);
		}

		protected void CheckValuationQuestion5C()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne && Parent.ValuationQuestion5A == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion5CInfo);
			}
		}

		public void ValidateValuationQuestion5D()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5DInfo);
		}

		protected void CheckValuationQuestion5D()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne && Parent.ValuationQuestion5A == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion5DInfo);
			}
		}

		public void ValidateValuationQuestion5EA()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5EAInfo);
		}

		protected void CheckValuationQuestion5EA()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne && Parent.ValuationQuestion5A == YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion5EAInfo);
			}
		}

		public void ValidateValuationQuestion5EB()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion5EBInfo);
		}

		protected void CheckValuationQuestion5EB()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne && Parent.ValuationQuestion5A == YesNoList.Codes.Yes && Parent.ValuationQuestion5EA == PricingCodeList.Codes._99)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ValuationQuestion5EBInfo);
			}
		}

		public void ValidateValuationQuestion6A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion6AInfo);
		}

		protected void CheckValuationQuestion6A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion6AInfo);
			}
		}

		public void ValidateValuationQuestion6B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion6BInfo);
		}

		protected void CheckValuationQuestion6B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion6BInfo);
			}
		}

		public void ValidateValuationQuestion7A_5SM()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion7A_5SMInfo);
		}

		protected void CheckValuationQuestion7A_5SM()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7A_5SMInfo);
			}
		}

		public void ValidateValuationQuestion7B_5SM()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion7B_5SMInfo);
		}

		protected void CheckValuationQuestion7B_5SM()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion7B_5SMInfo);
			}
		}

		public void ValidateValuationQuestion8A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion8AInfo);
		}

		protected void CheckValuationQuestion8A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion8AInfo);
			}
		}

		public void ValidateValuationQuestion8B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion8BInfo);
		}

		protected void CheckValuationQuestion8B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion8BInfo);
			}
		}

		public void ValidateValuationQuestion8C()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion8CInfo);
		}

		protected void CheckValuationQuestion8C()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion8CInfo);
			}
		}

		public void ValidateValuationQuestion8D()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion8DInfo);
		}

		protected void CheckValuationQuestion8D()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion8DInfo);
			}
		}

		public void ValidateValuationQuestion9A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion9AInfo);
		}

		protected void CheckValuationQuestion9A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion9AInfo);
			}
		}

		public void ValidateValuationQuestion9B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion9BInfo);
		}

		protected void CheckValuationQuestion9B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion9BInfo);
			}
		}

		public void ValidateValuationQuestion10A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion10AInfo);
		}

		protected void CheckValuationQuestion10A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion10AInfo);
			}
		}

		public void ValidateValuationQuestion10B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion10BInfo);
		}

		protected void CheckValuationQuestion10B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion10BInfo);
			}
		}

		public void ValidateValuationQuestion10C()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion10CInfo);
		}

		protected void CheckValuationQuestion10C()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion10CInfo);
			}
		}

		public void ValidateValuationQuestion10D()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion10DInfo);
		}

		protected void CheckValuationQuestion10D()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion10DInfo);
			}
		}

		public void ValidateValuationQuestion11A()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion11AInfo);
		}

		protected void CheckValuationQuestion11A()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion11AInfo);
			}
		}

		public void ValidateValuationQuestion11B()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion11BInfo);
		}

		protected void CheckValuationQuestion11B()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion11BInfo);
			}
		}

		public void ValidateValuationQuestion11C()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion11CInfo);
		}

		protected void CheckValuationQuestion11C()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion11CInfo);
			}
		}

		public void ValidateValuationQuestion11D()
		{
			ValidateCalculatedProperty(Parent.ValuationQuestion11DInfo);
		}

		protected void CheckValuationQuestion11D()
		{
			if (Parent.JZ_ValuationCode == ValuationCodeList.Codes.MethodOne)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ValuationQuestion11DInfo);
			}
		}
	}
}
