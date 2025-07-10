using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	public class ComplianceDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsAddedCorrectly()
		{
			AssertColumnAddedCorrectly("LocalAmount");
			AssertColumnAddedCorrectly("LocalTaxAmount");
			AssertColumnAddedCorrectly("LocalTotalAmount");
		}

		void AssertColumnAddedCorrectly(string columnName)
		{
			var header = new TestObjectCreator(Factory).CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Test", "ABC") as ARComplianceDocumentHeader;

			using (var form = new ComplianceDocumentForm(header))
			using (var userControl = new ComplianceDocumentUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var expectedColumn = columnName;
				var columns = userControl.ComplianceDocumentLineGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
				Assert("New column should be visibled.", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
			}
		}

		public void TestCustomRelatedCheckBoxCaption()
		{
			using (var control = new ComplianceDocumentUserControl())
			{
				Func<string, Control> getControl = (controlName) =>
				{
					var controls = control.Controls.Find(controlName, true);
					AssertEquals(1, controls.Length);
					return controls[0];
				};
				Assert(getControl("ADH_CustomRelated") is ZCheckBox);
				AssertEquals("caption should be Via Customs", "Via Customs", ((ZCheckBox)getControl("ADH_CustomRelated")).CaptionResourceString.Caption);
			}
		}

		public void TestSpecialVoidingPanel()
		{
			using (var control = new ComplianceDocumentUserControl())
			{
				Func<string, Control> getControl = (controlName) =>
				{
					var controls = control.Controls.Find(controlName, true);
					AssertEquals(1, controls.Length);
					return controls[0];
				};
				Assert(getControl("IsSpecialVoiding") is ZCheckBox);
				Assert(getControl("ADH_VoidingReason") is ZTextBox);
				Assert(getControl("ADH_ApprovalNumber") is ZTextBox);
			}
		}

		public void TestSetSpecialVoidingPanel()
		{
			using (var control = new ComplianceDocumentUserControl())
			{
				var specialVoidingPanel = control.Controls.Find("specialVoidingPanel", true)[0] as ZPanel;
				AssertNotNull(specialVoidingPanel);

				var mockIComplianceDocumentVoidingProvider = new Mock<IComplianceDocumentVoidingProvider>();
				mockIComplianceDocumentVoidingProvider.Setup(x => x.IsAllowedSpecialVoid(It.IsAny<AccComplianceDocumentHeader>())).Returns(false);

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentVoidingProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentVoidingProvider.Object);

				var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
				{
					control.SetSpecialVoidingPanel();
					AssertEquals(false, specialVoidingPanel.Visible);
				}

				mockIComplianceDocumentVoidingProvider.Setup(x => x.IsAllowedSpecialVoid(It.IsAny<AccComplianceDocumentHeader>())).Returns(true);

				using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
				{
					control.SetSpecialVoidingPanel();
					AssertEquals(true, specialVoidingPanel.Visible);
				}
			}
		}
	}
}
