using System.Windows.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(GenerateAsycudaXMLForm))]
	class GenerateAsycudaXMLFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		public void TestNewAddedColumn()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				Assert(grid.Columns.Contains(AutoJobDeclarationMessageSendingObject.Schema.DeclarationType));
				Assert(grid.Columns.Contains(AutoJobDeclarationMessageSendingObject.Schema.LocalReferenceNumber));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			return new GenerateAsycudaXMLForm(new JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>(declaration));
		}
	}
}
