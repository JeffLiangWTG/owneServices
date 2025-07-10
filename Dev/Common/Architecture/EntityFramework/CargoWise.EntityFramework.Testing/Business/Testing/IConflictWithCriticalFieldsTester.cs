using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace CargoWise.EntityFramework.Business.Testing
{
	[TestsSubclassesOf(
			typeof(IConflictWithCriticalFields),
			RequireTestOnlyInFirstSubLevel = true,
			IncludeAbstractClasses = true)]
	public abstract class IConflictWithCriticalFieldsTester : TestCaseWithFactory
	{
		#region Implementation

		protected Type GetExpectedObjectType() => TestedTypeHelper.GetTestedType(GetType());

		protected abstract IConflictWithCriticalFields GetNewBusinessObject();

		#endregion

		#region Test

		public void TestThrowNotSupportedExceptionForNonBusinessObject()
		{
			var obj = GetNewBusinessObject();

			if (obj != null)
			{
				if (obj is BusinessObject)
				{
					AssertNoExceptionThrown("Should not throw any exceptions", () => obj.SetConflictWithCriticalFieldsBusinessContext());
				}
				else
				{
					AssertExceptionThrown<NotSupportedException>("SetConflictWithCriticalFieldsBusinessContext will never be called for non BusinessObject", () => obj.SetConflictWithCriticalFieldsBusinessContext());
				}
			}

			Assert(true);
		}

		#endregion
	}
}
