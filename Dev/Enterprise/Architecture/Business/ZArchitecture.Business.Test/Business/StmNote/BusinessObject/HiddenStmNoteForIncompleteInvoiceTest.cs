using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(HiddenStmNoteNotAutoLogged))]
	sealed class HiddenStmNoteForIncompleteInvoiceTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return hiddenNote;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var hiddenStmNote = base.GetNewBusinessObjectForDeleteTest(factory) as HiddenStmNoteNotAutoLogged;
			hiddenStmNote.ST_Table = "GlbStaff";

			return hiddenStmNote;
		}

		protected override void SetUp()
		{
			base.SetUp();
			hiddenNote = Factory.New<HiddenStmNoteNotAutoLogged>();
			DummyBizOWithRelatedNotes bizO = Factory.New<DummyBizOWithRelatedNotes>();
			hiddenNote.ST_ParentID = bizO.PK;
			hiddenNote.ST_Table = bizO.TableName;
			bizO.Notes.Add(hiddenNote);
		}

		HiddenStmNoteNotAutoLogged hiddenNote;

		#endregion
	}
}
