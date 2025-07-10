using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI.Certificates;
using Enterprise.Customs.EU.GUI.Certificates.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(GlbStaffForm_ITCredentialsUserControl))]
sealed class GlbStaffForm_ITCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
{
	public void TestNodesGridColumns()
	{
		using (var form = GetFormToBash())
		{
			var nodesGrid = (ZGrid)form.Controls.Find("ITAccUserGrid", true).SingleOrDefault();
			AssertNotNull("Grid should not be null", nodesGrid);
			AssertEquals("Columns Count", 2, nodesGrid.ColumnStyles.Count);
			CombineAssertions("Column Styles", () =>
			{
				AssertColumn(nodesGrid, GlbExternalPassword.Schema.GP_UserID, expectedVisible: true, expectedReadOnly: false);
				AssertColumn(nodesGrid, GlbBrokerExternalPassword.Schema.AccountNumber, expectedVisible: true, expectedReadOnly: true);
			});
		}
	}

	public void TestToggleCertificateLoaderUserControlStatus()
	{
		using (var form = GetFormToBash())
		{
			form.Show();

			var staffWrapper = (GlbStaffWrapper)((ZForm)form).CurrentDataItem;
			var certificateLoaderUserControl = (DigitalCertificateControl_p12)form.Controls.Find("CertificateLoaderUserControl", true).SingleOrDefault();
			staffWrapper.PasswordCollection.RemoveAndDeleteAll();
			Assert(!certificateLoaderUserControl.Enabled);

			staffWrapper.PasswordCollection.AddNew();
			Assert(certificateLoaderUserControl.Enabled);
		}
	}

	public void TestCertificatePasswordTextBox()
	{
		using (var form = GetFormToBash())
		{
			form.Show();

			var certificatePasswordTextBox = form.FindSingle<ZTextBox>("CertificatePasswordTextBox");
			AssertEquals("CharacterCasing", CharacterCasing.Normal, certificatePasswordTextBox.CharacterCasing);

			var staffWrapper = (GlbStaffWrapper)((ZForm)form).CurrentDataItem;
			var glbPassword = staffWrapper.PasswordCollection.AddNew();
			glbPassword.CurrentDecryptedCertificatePassphrase = "UPPERCASE PWD";
			AssertEquals("When Password is entered uppercase, Text", "UPPERCASE PWD", certificatePasswordTextBox.Text);

			glbPassword.CurrentDecryptedCertificatePassphrase = "lowercase pwd";
			AssertEquals("When Password is entered lowercase, Text", "lowercase pwd", certificatePasswordTextBox.Text);

			glbPassword.CurrentDecryptedCertificatePassphrase = "mIxEdCaSe PwD";
			AssertEquals("When Password is entered mixed case, Text", "mIxEdCaSe PwD", certificatePasswordTextBox.Text);
		}
	}

	public void TestXadesCertificateChipsetDropEdit()
	{
		using (var form = GetFormToBash())
		{
			form.Show();

			CombineAssertions(() =>
			{
				var xadesCertificateChipsetDropEdit = form.FindSingle<ZDropEdit>("XadesCertificateChipsetDropEdit");
				AssertEquals("BindTo", "CryptokiCertificateCollection.GP_Name", xadesCertificateChipsetDropEdit.BindTo);
				AssertEquals("ShowInDropDown", ShowInDropDownList.OnlyShowCode, xadesCertificateChipsetDropEdit.ShowInDropDown);
				AssertEquals("ShowDescriptionBox", false, xadesCertificateChipsetDropEdit.ShowDescriptionBox);
				AssertEquals("ShouldResizeByMaxLength", false, xadesCertificateChipsetDropEdit.ShouldResizeByMaxLength);
			});
		}
	}

	public void TestXadesCertificateSerialNumberTextBox()
	{
		using (var form = GetFormToBash())
		{
			form.Show();

			CombineAssertions(() =>
			{
				var xadesCertificateSerialNumberTextBox = form.FindSingle<ZTextBox>("XadesCertificateSerialNumberTextBox");
				AssertEquals("BindTo", "CryptokiCertificateCollection.GP_CertificateSerialNumber", xadesCertificateSerialNumberTextBox.BindTo);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, xadesCertificateSerialNumberTextBox.CharacterCasing);
			});
		}
	}

	public void TestXadesCertificateCertificateAuthorityDropEdit()
	{
		using (var form = GetFormToBash())
		{
			form.Show();

			var xadesCertificateCertificateAuthorityDropEdit = form.FindSingle<ZDropEdit>("XadesCertificateCertificateAuthorityDropEdit");
			AssertEquals("BindTo", "CryptokiCertificateCollection.GP_CertificateAuthority", xadesCertificateCertificateAuthorityDropEdit.BindTo);
		}
	}

	public void TestXadesCertificateChooseButton()
	{
		using (var form = GetFormForTest<GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest>())
		{
			form.Show();

			var credentialsUserControl = (GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest)form.Controls["GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest"];
			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;

			var xadesCertificateChooseButton = form.FindSingle<ZButton>("XadesCertificateChooseButton");
			AssertEquals("XadesCertificateChooseButton Visible", true, xadesCertificateChooseButton.Visible);

			var addXadesCertificateButton = form.FindSingle<ZButton>("AddXadesCertificateButton");
			addXadesCertificateButton.PerformClick();
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
			cryptokiCertificate.GP_Name = "";
			xadesCertificateChooseButton.PerformClick();
			AssertEquals("When Chipset is empty, LastMessage", "You must specify chipset.", UnitTestUserNotification.Instance.LastMessage.Text);

			cryptokiCertificate.GP_Name = "BIT4ID";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			credentialsUserControl.AddDummyCertificateToList();
			xadesCertificateChooseButton.PerformClick();
			AssertEquals("Once the certificate is selected, GP_CertificateSerialNumber (and other fields) are mapped to the BizObj", "02b9572b9cad7250a906b3", cryptokiCertificate.GP_CertificateSerialNumber);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			credentialsUserControl.ResetCertificateList();
			xadesCertificateChooseButton.PerformClick();
			AssertEquals("Even though the user abort the certificate selection, previously mapped GP_CertificateSerialNumber (and other fields) are not changed", "02b9572b9cad7250a906b3", cryptokiCertificate.GP_CertificateSerialNumber);
		}
	}

	public void TestExceptionHandlerCryptokiCertificateProviderDecorator_IsUsed()
	{
		using (var form = GetFormForTest<GlbStaffForm_ITCredentialsUserControl>())
		{
			form.Show();

			var credentialsUserControl = (GlbStaffForm_ITCredentialsUserControl)form.Controls["GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest"];
			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;

			var xadesCertificateChooseButton = form.FindSingle<ZButton>("XadesCertificateChooseButton");
			AssertEquals("XadesCertificateChooseButton Visible", true, xadesCertificateChooseButton.Visible);

			var addXadesCertificateButton = form.FindSingle<ZButton>("AddXadesCertificateButton");
			addXadesCertificateButton.PerformClick();

			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
			cryptokiCertificate.GP_CertificateAuthority = "ARU";
			cryptokiCertificate.GP_Name = "BIT4ID";
			xadesCertificateChooseButton.PerformClick();

			AssertEquals(
				"When drivers are not installed",
				"The system cannot find the drivers for your Certificate (bit4xpki.dll). Please install the drivers following the instructions of the manufacturer.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestXadesCertificateInformationButton()
	{
		using (var form = GetFormForTest<GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest>())
		{
			form.Show();

			var credentialsUserControl = (GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest)form.Controls["GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest"];
			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;

			var xadesCertificateInformationButton = form.FindSingle<ZButton>("XadesCertificateInformationButton");
			AssertEquals("XadesCertificateInformationButton Visible", true, xadesCertificateInformationButton.Visible);

			var addXadesCertificateButton = form.FindSingle<ZButton>("AddXadesCertificateButton");
			addXadesCertificateButton.PerformClick();
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
			cryptokiCertificate.GP_Name = "";
			xadesCertificateInformationButton.PerformClick();
			AssertEquals("When Chipset is empty, LastMessage", "You must specify chipset.", UnitTestUserNotification.Instance.LastMessage.Text);

			cryptokiCertificate.GP_Name = "BIT4ID";
			cryptokiCertificate.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			credentialsUserControl.AddDummyCertificateToList();
			xadesCertificateInformationButton.PerformClick();
			AssertType<CertificateInformationForm>("When the certificate is correctly retrieved from the token, LastFormShownDialog", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestGetCertificateSelector()
	{
		using (var form = GetFormForTest<GlbStaffForm_ITCredentialsUserControlForExposedTest>())
		{
			form.Show();

			var credentialsUserControl = (GlbStaffForm_ITCredentialsUserControlForExposedTest)form.Controls["GlbStaffForm_ITCredentialsUserControlForExposedTest"];
			AssertType<ExceptionFriendlyCertificateSelector>("CertificateSelector Type", credentialsUserControl.GetCertificateSelectorExposed("BIT4ID"));
		}
	}

	public void TestAddXadesCertificateButton()
	{
		using (var form = (ZForm)GetFormToBash())
		{
			form.Show();

			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;
			var addXadesCertificateButton = form.FindSingle<ZButton>("AddXadesCertificateButton");
			var clearXadesCertificateButton = form.FindSingle<ZButton>("ClearXadesCertificateButton");

			AssertEquals("CryptokiCertificateCollection Count", 0, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("AddXadesCertificateButton Enabled", true, addXadesCertificateButton.Enabled);

			addXadesCertificateButton.PerformClick();
			AssertEquals("When AddXadesCertificateButton is clicked, HasChanges", true, staffWrapper.HasChanges);
			AssertEquals("CryptokiCertificateCollection Count", 1, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("AddXadesCertificateButton Enabled", false, addXadesCertificateButton.Enabled);

			clearXadesCertificateButton.PerformClick();
			AssertEquals("CryptokiCertificateCollection Count", 0, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("AddXadesCertificateButton Enabled", true, addXadesCertificateButton.Enabled);
		}
	}

	public void TestClearXadesCertificateButton()
	{
		using (var form = (ZForm)GetFormToBash())
		{
			form.Show();

			var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;
			var addXadesCertificateButton = form.FindSingle<ZButton>("AddXadesCertificateButton");
			var clearXadesCertificateButton = form.FindSingle<ZButton>("ClearXadesCertificateButton");

			AssertEquals("CryptokiCertificateCollection Count", 0, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("ClearXadesCertificateButton Enabled", false, clearXadesCertificateButton.Enabled);

			addXadesCertificateButton.PerformClick();
			AssertEquals("CryptokiCertificateCollection Count", 1, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("ClearXadesCertificateButton Enabled", true, clearXadesCertificateButton.Enabled);

			clearXadesCertificateButton.PerformClick();
			AssertEquals("CryptokiCertificateCollection Count", 0, staffWrapper.CryptokiCertificateCollection.Count);
			AssertEquals("ClearXadesCertificateButton Enabled", false, clearXadesCertificateButton.Enabled);
		}
	}

	public void TestAutomaticSignaturePanel()
	{
		using var form = (ZForm)GetFormToBash();
		form.Show();

		CombineAssertions(() =>
		{
			form.AssertContainsControl<ZGroupBox>("AutomaticSignatureGroupBox", x => x.WithCaption("Automatic Signature"));
			form.AssertContainsControl<GlbStaffAutomaticSignatureUserControl>("AutomaticSignatureUserControl", x => x.WithBindTo("."));
		});
	}

	public void TestAddAndClearAutomaticSignatureButtons()
	{
		using var form = (ZForm)GetFormToBash();
		form.Show();

		var staffWrapper = (GlbStaffWrapper)form.CurrentDataItem;
		var addAutomaticSignatureButton = form.FindSingle<ZButton>("AddAutomaticSignatureButton");
		var clearAutomaticSignatureButton = form.FindSingle<ZButton>("ClearAutomaticSignatureButton");

		CombineAssertions("Initial Scenario", () =>
		{
			AssertEquals("HasChanges", false, staffWrapper.HasChanges);
			AssertEquals("AutomaticSignaturePasswordCollection Count", 0, staffWrapper.AutomaticSignaturePasswordCollection.Count);
			AssertEquals("AddAutomaticSignatureButton Enabled", true, addAutomaticSignatureButton.Enabled);
			AssertEquals("ClearAutomaticSignatureButton Enabled", false, clearAutomaticSignatureButton.Enabled);
		});

		addAutomaticSignatureButton.PerformClick();
		CombineAssertions("When AddAutomaticSignatureButton is clicked", () =>
		{
			AssertEquals("HasChanges", true, staffWrapper.HasChanges);
			AssertEquals("AutomaticSignaturePasswordCollection Count", 1, staffWrapper.AutomaticSignaturePasswordCollection.Count);
			AssertEquals("AddAutomaticSignatureButton Enabled", false, addAutomaticSignatureButton.Enabled);
			AssertEquals("ClearAutomaticSignatureButton Enabled", true, clearAutomaticSignatureButton.Enabled);
		});

		staffWrapper.HasChanges = false;
		clearAutomaticSignatureButton.PerformClick();
		CombineAssertions("When ClearAutomaticSignatureButton is clicked", () =>
		{
			AssertEquals("HasChanges", true, staffWrapper.HasChanges);
			AssertEquals("AutomaticSignaturePasswordCollection Count", 0, staffWrapper.AutomaticSignaturePasswordCollection.Count);
			AssertEquals("AddAutomaticSignatureButton Enabled", true, addAutomaticSignatureButton.Enabled);
			AssertEquals("ClearAutomaticSignatureButton Enabled", false, clearAutomaticSignatureButton.Enabled);
		});
	}

	void AssertColumn(ZGrid parentGrid, ZString columnName, ZBool expectedVisible, ZBool expectedReadOnly)
	{
		var columnInfo = parentGrid.GetColumnStyle(columnName);
		AssertNotNull($"'{columnName}' not null", columnInfo);
		AssertEquals($"'{columnName}' IsVisible", expectedVisible, columnInfo.IsVisible);
		AssertEquals($"'{columnName}' IsReadOnly", expectedReadOnly, columnInfo.IsReadOnly);
	}

	ZForm GetFormForTest<TChildControl>() where TChildControl : GlbStaffForm_ITCredentialsUserControl, new()
	{
		var zChildForm = new ZChildForm();
		zChildForm.CaptionRenderingEnabled = true;
		zChildForm.Controls.Add(new TChildControl { Dock = DockStyle.Fill, Name = typeof(TChildControl).Name });
		zChildForm.SetDataBinding(GlbStaffWrapper.Get(glbStaff), "");
		return zChildForm;
	}
}

class GlbStaffForm_ITCredentialsUserControlForCertificateSelectionTest : GlbStaffForm_ITCredentialsUserControl
{
	public List<CryptokiCertificate> CertificateList { get; } = new List<CryptokiCertificate>();
	IDisposable cryptokiCertificateProviderSetter;

	public void AddDummyCertificateToList()
	{
		ResetCertificateList();
		CertificateList.Add(new CryptokiCertificate()
		{
			Owner = "Owner",
			Issuer = "Issuer",
			NotBefore = new ZDateTime(2022, 01, 01),
			NotAfter = new ZDateTime(2022, 12, 31),
			Thumbprint = "Thumbprint",
			SerialNumber = "02b9572b9cad7250a906b3",
			TokenManufacturerId = "TokenManufacturerId",
			TokenModel = "TokenModel",
			TokenChipset = "TokenChipset",
		});
		TemporarilySetCryptokiCertificateProvider();
	}

	public void ResetCertificateList() => CertificateList.Clear();

	protected override CertificateSelector GetCertificateSelector(string chipset) => new DummyCertificateSelector(chipset, CertificateList);

	void TemporarilySetCryptokiCertificateProvider()
	{
		DisposeCryptokiCertificateProviderSetter();
		var cryptokiCertificateProviderMock = new Mock<ICryptokiCertificateProvider>();
		cryptokiCertificateProviderMock.Setup(m => m.GetCertificateList(It.IsAny<string>())).Returns(CertificateList);
		cryptokiCertificateProviderMock.Setup(m => m.ReadCertificate(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(CertificateList[0]);

		var objectHandleMock = new Mock<ObjectHandle>();
		objectHandleMock.Setup(m => m.GetObject()).Returns(cryptokiCertificateProviderMock.Object);

		var cryptokiCertificateProvider = new KeyObjectHandleDictionaryObject
		{
			{ "Dummy", objectHandleMock.Object }
		};

		cryptokiCertificateProviderSetter = ObjectFactory.Substitute(nameof(ICryptokiCertificateProvider), cryptokiCertificateProvider);
	}

	void DisposeCryptokiCertificateProviderSetter()
	{
		cryptokiCertificateProviderSetter?.Dispose();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			DisposeCryptokiCertificateProviderSetter();
		}

		base.Dispose(disposing);
	}
}

class GlbStaffForm_ITCredentialsUserControlForExposedTest : GlbStaffForm_ITCredentialsUserControl
{
	public CertificateSelector GetCertificateSelectorExposed(string chipset) => GetCertificateSelector(chipset);
}
