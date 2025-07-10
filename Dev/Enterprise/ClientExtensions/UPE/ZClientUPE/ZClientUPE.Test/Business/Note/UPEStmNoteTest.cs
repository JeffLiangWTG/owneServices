using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEStmNote))]
	public class UPEStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEStmNote>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestSetDefaultValues()
		{
			UPEStmNote uPEStmNote = Factory.New<UPEStmNote>();
			AssertEquals(nameof(StmNoteVisibility.INT), uPEStmNote.ST_NoteType);
		}

		public void TestNoteIsReadonly_ForCusHAWB()
		{
			CusHAWB hAWB = Factory.NewWithValidTestData<CusHAWB>();
			TestNoteIsReadonly(hAWB, true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
		}

		public void TestNoteIsReadonly_ForCusMAWB()
		{
			CusMAWB mAWB = Factory.NewWithValidTestData<CusMAWB>();
			TestNoteIsReadonly(mAWB, true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
		}

		public void TestNoteIsReadonly_ForJobDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			TestNoteIsReadonly(declaration, true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
		}

		public void TestNoteIsEditable_ForDeliveryInstructions()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			TestNoteIsReadonly(declaration, false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
		}

		public void TestNoteIsEditable_ForOrgHeader()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			TestNoteIsReadonly(organisation, false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
		}

		public void TestNoteIsReadonly(BusinessObject parent, bool expectReadOnly, string description)
		{
			UPEStmNote note = (UPEStmNote)parent.GetNotes().AddNew();
			note.ST_NoteDataAsText = "TEST";
			note.ST_IsCustomDescription = false;
			note.ST_Description = description;
			AssertEquals("Newly created note not expected to be read only", false, note.ReadOnly);
			Factory.Save();
			BusinessObjectFactory cleanFactoryToForceOnLoad = new BusinessObjectFactory();
			note = cleanFactoryToForceOnLoad.Load<UPEStmNote>(note.PK);
			AssertEquals("Expected the note on " + parent.GetType().Name + " to be " + (expectReadOnly ? " read only" : " not read only"), expectReadOnly, note.ReadOnly);
		}

		#region Base Test Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			CusHAWB hawb = Factory.NewWithValidTestData<CusHAWB>();
			UPEStmNote note = Factory.New<UPEStmNote>();
			note.ST_ParentID = hawb.PK;
			note.ST_Table = hawb.TableName;
			hawb.Notes.Add(note);
			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			StmNote note = (StmNote)base.GetNewBusinessObjectForDeleteTest(factory);
			note.ST_Table = "CusHAWB";
			return note;
		}
		#endregion
	}
}
