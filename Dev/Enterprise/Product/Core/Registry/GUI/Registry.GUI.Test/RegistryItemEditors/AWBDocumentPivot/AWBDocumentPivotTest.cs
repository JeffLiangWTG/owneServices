using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AWBDocumentPivot))]
	sealed class AWBDocumentPivotTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizO = new AWBDocumentPivot(Factory);
			return bizO;
		}

		public void TestPivot()
		{
			var factory = new BusinessObjectFactory();
			var pivot = new AWBDocumentPivot(factory);
			pivot.Printed = true;
			pivot.Title = "Title";
			pivot.Name = "Name";

			AssertEquals("Name", "Name", pivot.Name);
			AssertEquals("Title", "Title", pivot.Title);
			AssertEquals("Printed", ZBool.True, pivot.Printed);
		}

		#region ICanDelete

		public void TestCanDelete()
		{
			var bizO = new AWBDocumentPivot(Factory);
			AssertEquals("Should not be able to delete", false, bizO.CanDelete);
		}

		public void TestReasonForNotAbleToDelete()
		{
			var bizO = new AWBDocumentPivot(Factory);
			AssertEquals("Users should use the 'Printed?' column to determine if it's printed", "Use the 'Printed?' column to enable/disable printing of this Document", bizO.ReasonForNotAbleToDelete);
		}

		#endregion
	}
}
