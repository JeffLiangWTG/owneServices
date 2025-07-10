using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ContainerDetailUserControl))]
sealed class ContainerDetailUserControlTest : TestCaseWithFactory
{
	public void TestSealTypeField()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		using var userControl = new ContainerUserControl();
		userControl.JobDeclaration = declaration;

		CombineAssertions(() =>
		{
			AssertNotNull(userControl.containersUserControl1);

			var controls = userControl.containersUserControl1.Controls;
			var field = controls.Find("SealTypeDropEdit", searchAllChildren: true).Single();
			Assert("Export - SealType visible", field.Visible);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Assert("Import - SealType not visible", !field.Visible);
		});
	}
}
