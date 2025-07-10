using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(PrimaryOrgSelectorForm))]
	public class PrimaryOrgSelectorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PrimaryOrgSelectorForm(new PrimaryOrgSelectorCollection(Factory));
		}

		public void TestContinueButton_Click()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			PrimaryOrgSelectorCollection collection = new PrimaryOrgSelectorCollection(Factory);
			PrimaryOrgSelector primaryOrg1 = new PrimaryOrgSelector(Factory, creator.AALSHI);
			PrimaryOrgSelector primaryOrg2 = new PrimaryOrgSelector(Factory, creator.ABIGAS);
			collection.Add(primaryOrg1);
			collection.Add(primaryOrg2);

			Factory.Save();

			using (PrimaryOrgSelectorForm form = new PrimaryOrgSelectorForm(collection))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ContinueButton.PerformClick();
				AssertEquals(ZGuid.Empty, form.SelectedOrgPK);
				AssertEquals("Please select primary organization", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OrganisationsGrid.SelectAllElements();
				form.ContinueButton.PerformClick();
				AssertEquals(ZGuid.Empty, form.SelectedOrgPK);
				AssertEquals("Please select only one primary organization", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OrganisationsGrid.UnSelectAll();
				form.OrganisationsGrid.Select(0);
				form.ContinueButton.PerformClick();
				AssertEquals(creator.AALSHI.PK, form.SelectedOrgPK);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
