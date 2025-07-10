using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

class SupportingInformationControlTest : TestCaseWithFactory
{
	public void TestGuaranteesUserControlType()
	{
		using (var form = new ZForm())
		using (var control = new SupportingInformationControlForTest())
		{
			form.Controls.Add(control);
			form.Show();
			var guarantees = control.Controls.Find("GuaranteesUserControl", searchAllChildren: true)[0] as ZDynamicControlCreationUserControl;
			AssertEquals(typeof(ESGuaranteesUserControl), guarantees.UserControlType);
		}
	}

	public void TestPreviousDocumentsUserControlType()
	{
		using (var control = new SupportingInformationControlForTest())
		{
			AssertEquals(typeof(PreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestAdditionalDocumentsUserControl()
	{
		void AssertAdditionalDocumentTypeAndText(string version, string messageForType, string messageType, JobDeclaration dec, Type expectedType, string expectedCaption)
		{
			using (var control = new SupportingInformationControlForTest())
			{
				var additionalDocumentsTab = (ZTabPage)control.Controls.Find("AdditionalDocumentsTabPage", searchAllChildren: true)[0];
				dec.JE_MessageType = messageType;
				control.SetDataBinding(dec, string.Empty);
				AssertEquals($"When {version}, GetAdditionalDocumentsUserControlType is {messageForType} when message is {messageType} type.", expectedType, control.GetAdditionalDocumentsUserControlType_Exposed());
				AssertEquals($"When {version}, AdditionalDocumentsTabPage has {messageForType} caption when message is {messageType} type.", expectedCaption, additionalDocumentsTab.Text);
			}
		}

		CombineAssertions(() =>
		{
			var jobDec = Factory.New<JobDeclaration>();

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertAdditionalDocumentTypeAndText("H1", "base type", MessageTypeList.Codes.Import, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), "Additional Documents");

				AssertAdditionalDocumentTypeAndText("H1", "new grid type", MessageTypeList.Codes.Export, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), "Additional Documents");

				AssertAdditionalDocumentTypeAndText("H1", "base type", MessageTypeList.Codes.MiscellaneousCustoms, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), "Additional Documents");
			}
		});
	}

	public void TestAdditionalInfoUserControl()
	{
		void AssertAdditionalInfoTypeAndText(string version, string messageForType, string messageType, JobDeclaration dec, Type expectedType, string expectedCaption)
		{
			using (var control = new SupportingInformationControlForTest())
			{
				var additionalInfoTab = (ZTabPage)control.Controls.Find("AdditionalInfoTabPage", searchAllChildren: true)[0];
				dec.JE_MessageType = messageType;
				control.SetDataBinding(dec, string.Empty);
				AssertEquals($"When {version}, GetAdditionalInfosUserControlType is {messageForType} when message is {messageType} type.", expectedType, control.GetAdditionalInfosUserControlType_Exposed());
				AssertEquals($"When {version}, AdditionalInfoTabPage has {messageForType} caption when message is {messageType} type.", expectedCaption, additionalInfoTab.Text);
			}
		}

		CombineAssertions(() =>
		{
			var jobDec = Factory.New<JobDeclaration>();

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				AssertAdditionalInfoTypeAndText("AES", "base type", MessageTypeList.Codes.Import, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControl), "[44] Additional Info");

				AssertAdditionalInfoTypeAndText("AES", "new grid type", MessageTypeList.Codes.Export, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid), "Additional Documents");

				AssertAdditionalInfoTypeAndText("AES", "base type", MessageTypeList.Codes.MiscellaneousCustoms, jobDec, typeof(EU.GUI.PlugIn.AdditionalInfosUserControl), "[44] Additional Info");
			}
		});
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		using (var control = new SupportingInformationControlForTest())
		{
			control.SetDataBinding(declaration, string.Empty);
			AssertEquals("Export type", typeof(ExportSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import type", typeof(ImportSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
		public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
		public Type GetAdditionalDocumentsUserControlType_Exposed() => base.GetAdditionalDocumentsUserControlType();
	}
}
