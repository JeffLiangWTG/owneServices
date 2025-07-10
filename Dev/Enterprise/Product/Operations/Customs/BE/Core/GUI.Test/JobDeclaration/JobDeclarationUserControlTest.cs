using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
{
	public void TestGroupBoxLocationsAndSizeUCC6Export()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		using (var userControl = new JobDeclarationUserControl())
		{
			var customsOfficesUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>(c => c.Name == "CustomsOfficesUserControl");
			userControl.JobDeclaration = declaration;
			CombineAssertions(() =>
			{
				AssertEquals("Is UCC6", true, declaration.Configuration.IsUCC6(declaration));
				AssertEquals("EXP - TransportDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 263, true), userControl.TransportDetailsGroupBox.Size);
				AssertEquals("EXP - ShipmentDetailsGroupBox Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 317, true), userControl.ShipmentDetailsGroupBox.Location);
				AssertEquals("EXP - ShipmentDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 405, true), userControl.ShipmentDetailsGroupBox.Size);
				AssertEquals("EXP - CustomsOfficesUserControl Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 728, true), customsOfficesUserControl.Location);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("IMP - TransportDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 263, true), userControl.TransportDetailsGroupBox.Size);
				AssertEquals("IMP - ShipmentDetailsGroupBox Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 317, true), userControl.ShipmentDetailsGroupBox.Location);
				AssertEquals("IMP - ShipmentDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 405, true), userControl.ShipmentDetailsGroupBox.Size);
				AssertEquals("IMP - CustomsOfficesUserControl Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 728, true), customsOfficesUserControl.Location);

				EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false);
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("Is Non UCC6", false, declaration.Configuration.IsUCC6(declaration));
				AssertEquals("Non UCC 6 - TransportDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 263, true), userControl.TransportDetailsGroupBox.Size);
				AssertEquals("Non UCC 6 - ShipmentDetailsGroupBox Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 317, true), userControl.ShipmentDetailsGroupBox.Location);
				AssertEquals("Non UCC 6 - ShipmentDetailsGroupBox Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 405, true), userControl.ShipmentDetailsGroupBox.Size);
				AssertEquals("Non UCC 6 - CustomsOfficesUserControl Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 728, true), customsOfficesUserControl.Location);
			});
		}
	}

	public void TestImportDocAddressCaption()
	{
		using (var userControl = new JobDeclarationUserControl())
		{
			AssertEquals("[UCC 3/15] Importer", userControl.ImporterDocAddress.CaptionResourceString.Caption);
		}
	}

	public void TestSupplierDocAddressCaption()
	{
		using (var userControl = new JobDeclarationUserControl())
		{
			AssertEquals("[UCC 3/7] Supplier", userControl.SupplierDocAddress.CaptionResourceString.Caption);
		}
	}
}
