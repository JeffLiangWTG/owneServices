using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyWithPiggyBackedAdditionalCalculatedPropertiesValidation : DummyWithValidation
	{
		public DummyWithPiggyBackedAdditionalCalculatedPropertiesValidation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString Z0_CalculatedPropertyWithPiggyBackedValidation
		{
			get { return fZ0_CalculatedPropertyWithPiggyBackedValidation; }
			set
			{
				fZ0_CalculatedPropertyWithPiggyBackedValidation = value;
				PiggyBackedValidation.ValidateZ0_CalculatedPropertyWithPiggyBackedValidation();
			}
		}
		ZString fZ0_CalculatedPropertyWithPiggyBackedValidation;

		public ZPropertyInfo Z0_CalculatedPropertyWithPiggyBackedValidationInfo
		{
			get { return GetZPropertyInfo(nameof(Z0_CalculatedPropertyWithPiggyBackedValidation)); }
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			DummyBizoValidation result = base.GetNewValidation();
			result.Add(PiggyBackedValidation);
			return result;
		}

		DummyZValidationWithPiggyBackedAdditionalCalculatedProperties PiggyBackedValidation
		{
			get
			{
				if (fPiggyBackedValidation == null)
				{
					fPiggyBackedValidation = new DummyZValidationWithPiggyBackedAdditionalCalculatedProperties(this);
				}
				return fPiggyBackedValidation;
			}
		}
		DummyZValidationWithPiggyBackedAdditionalCalculatedProperties fPiggyBackedValidation;
	}
}
