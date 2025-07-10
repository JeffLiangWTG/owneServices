using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNSWClientSettingRegistryItemEditor))]
	class CNSWClientSettingRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CNSWClientSettingRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CNSWClientSettingRegistryItemUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CNSWClientSettingRegistryItemUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CNSWClientSettingRegistryItem("", null, null, null, RegistryStorageFlags.System, CreateNewValue());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { CreateNewValue() };
		}

		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (var control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 436, 247);
				AssertEquals("should have the correct width", 436, control.Width);
				AssertEquals("should have the correct height", 390, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, control.Anchor);
			}
		}

		static CNSWClientSetting CreateNewValue()
		{
			return new CNSWClientSetting
			{
				MachineName = "Machine Name",
				SendFolder = @"D:\Folders\SendFolder",
				ReceiveFolder = @"D:\Folders\ReceiveFolder",
				ArchiveFolder = @"D:\Folders\ArchiveFolder",
				ErrorResponseFolder = @"D:\Folders\ErrorResponseFolder",
				AcdaSendFolder = @"D:\Folders\AcdaSendFolder",
				AcdaReceiveFolder = @"D:\Folders\AcdaReceiveFolder",
				AcdaArchiveFolder = @"D:\Folders\AcdaArchiveFolder",
				AcdaErrorResponseFolder = @"D:\Folders\AcdaErrorResponseFolder",
				RunningIntervalInSeconds = 60
			};
		}

		public void TestSystemCreateTimeShouldBeChangeAfterUpdate()
		{
			var branch = CreateBranch("CNC", "CNB");
			Factory.Save();

			var setting = new CNSWClientSetting(new FallbackLevel(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory)
			{
				MachineName = "MACHINE1123",
				SendFolder = @"D:\DEC OUT",
				ReceiveFolder = @"D:\DEC IN",
				ErrorResponseFolder = @"D:\DEC FAIL",
				ArchiveFolder = @"D:\DEC ARCHIVE",
				AcdaSendFolder = @"D:\ACD OUT",
				AcdaReceiveFolder = @"D:\ACD IN",
				AcdaErrorResponseFolder = @"D:\ACD FAIL",
				AcdaArchiveFolder = @"D:\ACD ARCHIVE",
				RunningIntervalInSeconds = 15
			};

			var regItem = new RegistryItemTagForTest(CNCustomsDataRegistry.Instance.CNSWClientSetting)
			{
				CompanyPKForTest = GlbCompany.CurrentCompany.PK.ToGuid(),
				BranchPKForTest = Guid.Empty,
				DepartmentPKForTest = Guid.Empty,
				NewValue = setting,
				IsChanged = true,
				HasValue = true
			};

			regItem.SaveAllValues();

			var interchangeQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub") { OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC" };
			var interchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);

			var systemCreateTimeUtc = interchange.EI_SystemCreateTimeUtc;
			interchange.Delete();

			setting.MachineName = "Machine";

			regItem.NewValue = setting;
			regItem.IsChanged = true;
			regItem.HasValue = true;

			regItem.SaveAllValues();

			interchange = Factory.LoadTop1<EDIInterchange>(interchangeQuery);
			AssertNotEquals("EI_SystemCreateTimeUtc", systemCreateTimeUtc, interchange.EI_SystemCreateTimeUtc);
		}

		GlbBranch CreateBranch(string companyCode, string branchCode)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = "CN";
			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			return branch;
		}
	}
}
