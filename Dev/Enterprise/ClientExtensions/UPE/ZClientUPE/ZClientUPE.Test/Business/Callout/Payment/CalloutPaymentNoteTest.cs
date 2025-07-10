using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(CalloutPaymentNote))]
	internal class CalloutPaymentNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSubclassOfUPEStmNote()
		{
			Assert("CalloutPaymentNote should be a subclass of UPEStmNote", typeof(UPEStmNote).IsAssignableFrom(Note.GetType()));
		}

		public void TestDefaultValues()
		{
			Assert("Should be a custom description", Note.ST_IsCustomDescription);
			AssertEquals("Payment Details", Note.ST_Description);
		}

		public void TestReadOnly()
		{
			Assert("Should be read-only", Note.ReadOnly);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		CalloutPaymentNote Note
		{
			get
			{
				if (fNote == null)
				{
					fNote = Factory.New<CalloutPaymentNote>();
				}

				return fNote;
			}
		}

		CalloutPaymentNote fNote;
		protected override BusinessObject GetNewBusinessObject()
		{
			Customs.AU.Declaration.Business.CusHAWB hawb = Factory.New<Customs.AU.Declaration.Business.CusHAWB>();
			CalloutPaymentNote note = Factory.New<CalloutPaymentNote>();
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
