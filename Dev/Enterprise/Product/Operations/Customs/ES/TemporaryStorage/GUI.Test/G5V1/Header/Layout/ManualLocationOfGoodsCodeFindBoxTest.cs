using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(ManualLocationOfGoodsCodeFindBox))]
sealed class ManualLocationOfGoodsCodeFindBoxTest : TestCaseWithFactory
{
	public void TestFormattedDescription()
	{
		var expectedResult = CusGoodsLocationQualifierList.Codes.AuthorizationNumber + ";" + CusGoodsLocationTypeList.Codes.AuthorizedPlace + ";AAA";
		var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_CustomsLocation = "AAA";
		using (var form = new ZForm())
		using (var control = new ManualLocationOfGoodsCodeFindBox())
		{
			control.SetDataBinding(temporaryStorageHeader, "ManualDestinationCustomsOffice");
			form.Controls.Add(control);
			new ManualLocationOfGoodsPopupModuleDecisionProvider(control).SetFindBoxCodeDescription(premises);
			AssertEquals("The Description field returns the formatted description", expectedResult, (control as IFindBox).Description);
			AssertEquals("DescriptionBox returns the formatted description", expectedResult, control.DescriptionBox.Text);
		}
	}
}
