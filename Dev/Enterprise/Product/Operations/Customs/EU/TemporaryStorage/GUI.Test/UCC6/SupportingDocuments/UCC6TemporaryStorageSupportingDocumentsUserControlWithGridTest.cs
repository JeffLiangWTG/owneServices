using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStorageSupportingDocumentsUserControlWithGridTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid())
			{
				var billsGrid = (ZGrid)control.Controls.Find("SupportingDocumentsGrid", true).First();
				var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("SupportingDocumentsGrid Column Names",
				new[] { "CSI_Code",
						"CSI_ReferenceNumber",
				}, columns.Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestCaptionCustomized()
		{
			var testData = new UCC6TemporaryStorageBillTestData();
			using (var form = new ZForm(testData.GetBindingParent(Factory)))
			using (var control = new UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid())
			{
				new ControlRebinder().Rebind(control, "Bills", testData.BindingString);
				form.Controls.Add(control);
				form.Show();

				var groupBox = control.FindSingle<ZGroupBox>("SupportingDocumentsGroupBox");
				AssertEquals("GroupBox Caption should be customized for UCC6TemporaryStorageBill Supporting Documents control", "Supporting Documents", groupBox.CaptionResourceString.Caption);
			}
		}

		abstract class TestData
		{
			public abstract string BindingString { get; }

			public abstract BusinessObject GetBindingParent(BusinessObjectFactory factory);
		}

		class UCC6TemporaryStorageBillTestData : TestData
		{
			public override string BindingString => "Bills";

			public override BusinessObject GetBindingParent(BusinessObjectFactory factory) => factory.NewWithValidTestData<TemporaryStorageHeader>();
		}
	}
}
