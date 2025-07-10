using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.Customs.EU.GUI.SADH;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(SADHEntryForm))]
	public class SADHEntryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SADHEntryForm(new FormDataManagerForTesting(Factory.New<JobDeclaration>()));
		}

		public void TestTariffFindBox()
		{
			var manager = new FormDataManagerForTesting(Factory.New<JobDeclaration>());
			manager.FormData.D1_MessageType = "EXP";
			using (var form = new SADHEntryForm(manager))
			{
				form.Show();
				var tariffFindBox = (Universal.GUI.TariffFindBox)form.Controls.Find("Section33TariffFindBox", searchAllChildren: true)[0];
				AssertEquals("Section33TariffFindBox.GetTariffType()", Universal.Constants.TariffTypes.Export, tariffFindBox.GetTariffType());
				manager.FormData.D1_MessageType = "IMP";
				AssertEquals("Section33TariffFindBox.GetTariffType()", Universal.Constants.TariffTypes.Import, tariffFindBox.GetTariffType());
				AssertType<Universal.GUI.TariffFindBox>(tariffFindBox);
			}
		}

		class FormDataManagerForTesting : SADHFormDataManager
		{
			public FormDataManagerForTesting(JobDeclaration declaration)
				: base(declaration)
			{
			}

			public new void WriteData()
			{
				WriteDataWasCalled = true;
			}
			public bool WriteDataWasCalled;
		}
	}
}
