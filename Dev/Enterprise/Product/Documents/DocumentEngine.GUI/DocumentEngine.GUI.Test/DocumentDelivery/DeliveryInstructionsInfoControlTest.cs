using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery.Testing
{
	sealed class DeliveryInstructionsInfoControlTest : TestCaseWithFactory
	{
		#region Tests

		public void TestRecipientsGrid()
		{
			DocumentPack pack = new DocumentPack();
			DeliveryInstructions instruction = pack.DeliveryInstructions;
			instruction.Recipients.RemoveAll();

			DocDeliveryContact contact1 = new DocDeliveryContact(Factory);
			contact1.Name = "TEST NAME1";
			instruction.Recipients.Add(contact1);

			DocDeliveryContact contact2 = new DocDeliveryContact(Factory);
			contact2.Name = "TEST NAME2";
			instruction.Recipients.Add(contact2);

			DocDeliveryContact contact3 = new DocDeliveryContact(Factory);
			contact3.Name = "TEST NAME3";
			instruction.Recipients.Add(contact3);

			PrintTask task = new PrintTask();
			task.Add(pack);

			using (TestForm form = new TestForm(task.TaskSettings))
			{
				form.Show();
				form.Control.SetDataBinding(task.TaskSettings, "");
				ZGrid recipientsGrid = form.Control.RecipientsGrid;
				AssertEquals(3, recipientsGrid.List.Count);
				AssertCollectionContains(contact1, recipientsGrid.List);
				AssertCollectionContains(contact2, recipientsGrid.List);
				AssertCollectionContains(contact3, recipientsGrid.List);

				AssertEquals(7, recipientsGrid.Columns.Count);
				Assert(recipientsGrid.Columns.Contains("OrgHeaderPK"));
				Assert(recipientsGrid.Columns.Contains("CompanyName"));
				Assert(recipientsGrid.Columns.Contains("Salutation"));
				Assert(recipientsGrid.Columns.Contains("Name"));
				Assert(recipientsGrid.Columns.Contains("DeliveryMethodDescription"));
				Assert(recipientsGrid.Columns.Contains("AttachmentType"));
				Assert(recipientsGrid.Columns.Contains("DeliveryAddress"));
			}
		}

		#endregion

		#region Implementation

		internal class TestForm : ZForm
		{
			public TestForm(PrintTaskSettings entity)
				: base(entity)
			{
				Control = new DeliveryInstructionsInfoControl();
				this.Controls.Add(Control);
			}

			public DeliveryInstructionsInfoControl Control;
		}

		#endregion
	}
}
