using CargoWise.EntityFramework.Testing;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public abstract class ConstraintTest<T> : TestCaseWithFactory where T : IFilterConstraint, new()
	{
		public void TestName()
		{
			AssertEquals("Name", ExpectedName, Constraint.Name);
		}

		public void TestSingularValueName()
		{
			AssertEquals("SingularValueName", ExpectedSingularValueName, Constraint.SingularValueName);
		}

		public void TestPluralValueName()
		{
			AssertEquals("PluralValueName", ExpectedPluralValueName, Constraint.PluralValueName);
		}

		public void TestDescription()
		{
			AssertNotNullOrEmpty(nameof(Constraint.Description), Constraint.Description);
		}

		public void TestIsRegistered()
		{
			AssertType(typeof(T), EnvironmentFilterProvider.Instance.GetConstraint(ExpectedName));
		}

		public abstract void TestGetValue();

		public virtual void TestGetDefaultStringValue()
		{
			AssertEquals("Override this method and implement correct testing of GetDefaultStringValue()", expected: true, ExpectGetValueToReturnGetDefaultStringValue);
		}
		
		#region Implementation

		protected abstract string ExpectedName { get; }

		protected abstract string ExpectedSingularValueName { get; }

		protected abstract string ExpectedPluralValueName { get; }

		protected abstract bool ExpectGetValueToReturnGetDefaultStringValue { get; }

		protected IFilterConstraint Constraint => constraint ??= ExpectGetValueToReturnGetDefaultStringValue
			? new FilterConstraintWrapperForGetDefaultStringValue(new T())
			: new T();

		IFilterConstraint constraint;

		#endregion
	}
}
