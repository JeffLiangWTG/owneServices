using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionDocumentPivot))]
	internal sealed class OperationalActionDocumentPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAction()
		{
			AssertNull("Action", Pivot.Action);
			OperationalAction action = Factory.New<OperationalAction>();
			Pivot.SF_SU_Inward = action.PK;
			AssertEquals("Action", action, pivot.Action);
		}

		public void TestDocument()
		{
			AssertNull("Document", Pivot.Document);
			DocumentCommand document = Factory.New<DocumentCommand>();
			Pivot.SF_SU_Outward = document.PK;
			AssertEquals("Document", document, pivot.Document);
		}

		public void TestEditingMode()
		{
			OperationalActionMenuEditableHelperTest.TestEditingMode(Pivot);
		}

		[ExpectNoExceptions]
		public void TestEditingModeOnDeletedBusinessObject()
		{
			Pivot.Delete();
			AssertEquals(false, Pivot.ReadOnly);
		}

		public void TestLookups()
		{
			AssertEquals("Lookups.GetType()", typeof(OperationalActionDocumentPivotLookups), Pivot.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals("Validation.GetType()", typeof(OperationalActionDocumentPivotValidation), Pivot.Validation.GetType());
		}

		#region Implementation
		OperationalActionDocumentPivot Pivot
		{
			get
			{
				return pivot ?? (pivot = Factory.New<OperationalActionDocumentPivot>());
			}
		}

		OperationalActionDocumentPivot pivot;
		#endregion
	}
}
