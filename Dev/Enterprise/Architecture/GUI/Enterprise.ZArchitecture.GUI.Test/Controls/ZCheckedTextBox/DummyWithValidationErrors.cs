using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class DummyWithValidationErrors : DummyBusinessObject
	{
		public DummyWithValidationErrors(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public virtual ZString PropertyWithErrors
		{
			get { return null; }
			set { }
		}
		public virtual ZPropertyInfo PropertyWithErrorsInfo { get { return GetZPropertyInfo(nameof(PropertyWithErrors)); } }

		public new DummyFailingValidation Validation
		{
			get { return (DummyFailingValidation)base.Validation; }
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
		Type fValidationType = typeof(DummyFailingValidation);
	}
}
