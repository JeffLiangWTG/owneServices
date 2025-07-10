using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionDocumentPivotCollection))]
	internal sealed class OperationalActionDocumentPivotCollectionTest : MenuEditableBusinessObjectCollectionTestCase<OperationalActionDocumentPivotCollection>
	{
		public void TestAllowNewAndAllow()
		{
			Action.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			Action.SU_IsSystemDefined = false;
			AssertEquals("Should allow new as not system defined", true, Action.DocumentPivots.AllowNew);
			AssertEquals("Should allow remove as not system defined", true, Action.DocumentPivots.AllowRemove);
			Action.SU_IsSystemDefined = true;
			AssertEquals("Should not allow new as now system defined", false, Action.DocumentPivots.AllowNew);
			AssertEquals("Should not allow remove as now system defined", false, Action.DocumentPivots.AllowRemove);
			Action.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Should allow new as editing of system defined actions is now allowed", true, Action.DocumentPivots.AllowNew);
			AssertEquals("Should allow remove as editing of system defined actions is now allowed", true, Action.DocumentPivots.AllowRemove);
		}

		public void TestActionSupportableForChildren()
		{
			AssertEquals("AddNew().SF_SU_Inward", Action.PK, Collection.AddNew().SF_SU_Inward);
		}

		public void TestAdditionalFilter()
		{
			OperationalAction anotherAction = Factory.NewWithValidTestData<OperationalAction>();
			OperationalActionDocumentPivot pivot1 = Factory.New<OperationalActionDocumentPivot>();
			OperationalActionDocumentPivot pivot2 = Factory.New<OperationalActionDocumentPivot>();
			pivot1.SF_SU_Inward = Action.PK;
			pivot1.SF_SU_Outward = Action.PK;
			pivot2.SF_SU_Inward = anotherAction.PK;
			pivot2.SF_SU_Outward = anotherAction.PK;
			Factory.Save();
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			OperationalActionDocumentPivotCollection documentPivots = new OperationalActionDocumentPivotCollection(anotherFactory.Load<OperationalAction>(Action.PK));
			documentPivots.Load();
			AssertEquals("Contains(pivot1.PK)", true, documentPivots.Contains(pivot1.PK));
			AssertEquals("Contains(pivot2.PK)", false, documentPivots.Contains(pivot2.PK));
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OperationalActionDocumentPivotCollection(Action);
		}

		OperationalAction Action
		{
			get
			{
				return action ?? (action = Factory.New<OperationalAction>());
			}
		}

		OperationalAction action;
		#endregion
	}
}
