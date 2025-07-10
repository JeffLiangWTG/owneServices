using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(BusinessObjectNotificationsViewerForm))]
	class BusinessObjectNotificationsViewerFormTest : ZFormBasherTest
	{
		public void TestNotificationsGridColumnNames()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			var supporter = new BusinessObjectNotificationsViewerSupporter(provider);
			using (var form = new BusinessObjectNotificationsViewerForm(supporter))
			{
				form.Show();

				var notificationsGrid = (ZGrid)form.Controls.Find("NotificationsGrid", true)[0];
				AssertEquals(3, notificationsGrid.Columns.Count);
				AssertNotNull("HumanReadableColumn1 column is added", notificationsGrid.GetColumnStyle("HumanReadableColumn1"));
				AssertNotNull("NotificationType column is added", notificationsGrid.GetColumnStyle("NotificationType"));
				AssertNotNull("NotificationMessage column is added", notificationsGrid.GetColumnStyle("NotificationMessage"));
				AssertNull("HumanReadableColumn2 column is NOT added", notificationsGrid.GetColumnStyle("HumanReadableColumn2"));
			}
		}

		public void TestProgressFormShown()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			var supporter = new BusinessObjectNotificationsViewerSupporter(provider);
			var dummyBizObj1 = Factory.New<DummyBusinessObject>();
			dummyBizObj1.Z0_Code = "Code1";

			supporter.BusinessObjectCollection.Add(dummyBizObj1);

			using (dummyBizObj1.SuspendValidationTesting())
			{
				dummyBizObj1.Z0_CodeInfo.AddWarning("Test Warning");
			}

			using (var form = new BusinessObjectNotificationsViewerForm(supporter))
			{
				form.Show();

				var openedForms = ZApplication.GetOpenForms();
				var progressForm = openedForms.FirstOrDefault(x => x is ProgressForm);
				AssertNotNull("ProgressForm shown", progressForm);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var provider = new BusinessObjectCollectionNotificationsViewerProvider(Factory);
			var supporter = new BusinessObjectNotificationsViewerSupporter(provider);
			return new BusinessObjectNotificationsViewerForm(supporter);
		}
	}
}
