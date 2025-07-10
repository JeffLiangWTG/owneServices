using System;
using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyWithValidation : DummyBusinessObject
	{
		public DummyWithValidation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Calculated Property

		public virtual ZString Z0_CalculatedPropertyInfo
		{
			get { return fZ0_CalculatedPropertyInfo; }
			set
			{
				fZ0_CalculatedPropertyInfo = value;
				Validation.ValidateZ0_CalculatedProperty();
			}
		}

		public ZPropertyInfoString Z0_CalculatedPropertyInfoInfo
		{
			get { return (ZPropertyInfoString)GetZPropertyInfo(nameof(Z0_CalculatedPropertyInfo)); }
		}

		ZString fZ0_CalculatedPropertyInfo;

		#endregion

		#region Another Calculated Property

		public virtual ZString Z0_AnotherCalculatedProperty
		{
			get { return fZ0_AnotherCalculatedProperty; }
			set
			{
				fZ0_AnotherCalculatedProperty = value;
				Validation.ValidateZ0_AnotherCalculatedProperty();
			}
		}

		public ZPropertyInfoString Z0_AnotherCalculatedPropertyInfo
		{
			get { return (ZPropertyInfoString)GetZPropertyInfo(nameof(Z0_AnotherCalculatedProperty)); }
		}

		ZString fZ0_AnotherCalculatedProperty;

		#endregion

		#region Validation

		public bool CheckZ0_DecimalFired;
		public bool CheckZ0_DecimalIsValidZDecimalFired;
		public bool CheckZ0_DecimalPiggyBackedFired;
		public bool CheckZ0_DecimalIsValidZDecimalPiggyBackedFired;
		public bool CheckZ0_DecimalDomainFired;
		public bool CheckZ0_DecimalIsValidZDecimalDomainFired;
		public bool CheckZ0_DecimalMultiLevelPiggyBackedFired;
		public bool CheckZ0_DecimalIsValidZDecimalMultiLevelPiggyBackedFired;
		public bool CheckZ0_DecimalDomainWithPiggybackedFired;
		public bool CheckZ0_DecimalIsValidZDecimalDomainWithPiggybackedFired;
		public bool CheckZ0_DecimalDomainWithMultiLevelPiggybackedFired;
		public bool CheckZ0_DecimalIsValidZDecimalDomainWithMultiLevelPiggybackedFired;

		public bool CheckZ0_CalculatedPropertyInfoFired;
		public bool CheckZ0_CalculatedPropertyInfoPiggybackedFired;
		public bool CheckZ0_CalculatedPropertyInfoMultiLevelPiggybackedFired;
		public bool CheckZ0_CalculatedPropertyInfoDomainFired;
		public bool CheckZ0_CalculatedPropertyInfoDomainWithPiggybackedFired;
		public bool CheckZ0_CalculatedPropertyInfoDomainWithMultiLevelPiggybackedFired;

		public bool CheckZ0_AnotherCalculatedPropertyFired;

		public new DummyZValidation Validation
		{
			get { return (DummyZValidation)base.Validation; }
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			return (DummyBizoValidation)Activator.CreateInstance(ValidationType, new object[] { this });
		}

		public Type ValidationType
		{
			get { return fValidationType; }
			set { fValidationType = value; }
		}
		Type fValidationType = typeof(DummyZValidation);

		#endregion
	}
}
