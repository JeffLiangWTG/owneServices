using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ContainerUserControl))]
sealed class ContainerUserControlTest : TestCaseWithFactory
{
	public void TestSealTypeColumn()
	{
		using var userControl = new ContainerUserControl();
		userControl.JobDeclaration = declaration;
		var controlGrid = (ZModuleButtonGrid)userControl.Controls.Find("CusContainersBoundGrid", searchAllChildren: true).Single();
		SealTypeColumnAvailabilityCheck("CO_SealType");
		SealTypeColumnAvailabilityCheck("CO_SealDeviceID");
		SealTypeColumnAvailabilityCheck("CO_MovementDocumentType");
		SealTypeColumnAvailabilityCheck("CO_MovementDocumentNum");

		void SealTypeColumnAvailabilityCheck(string columnName)
		{
			var columnStyle = controlGrid.InnerGrid.GetColumnStyle(columnName);
			CombineAssertions($"Availability Check: {columnStyle.ColumnName}", () =>
			{
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				Assert("Export", !columnStyle.IsUnavailable);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				Assert("Import", columnStyle.IsUnavailable);
			});
		}
	}

	public void TestCusContainersBoundGridColumnOrder()
	{
		using var userControl = new ContainerUserControl();
		userControl.JobDeclaration = declaration;
		var controlGrid = (ZModuleButtonGrid)userControl.Controls.Find("CusContainersBoundGrid", searchAllChildren: true).Single();
		var columnStylesOrder = controlGrid.InnerGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).Take(6).ToArray();
		string[] expectedColumnOrder =
		{
			CusContainer.Schema.CO_ContainerNumber,
			CusContainer.Schema.CO_Seal,
			CusContainer.Schema.CO_SealType,
			CusContainer.Schema.CO_SealDeviceID,
			CusContainer.Schema.CO_MovementDocumentType,
			CusContainer.Schema.CO_MovementDocumentNum
		};
		AssertArrayEqualsByElements(expectedColumnOrder, columnStylesOrder);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
	}

	JobDeclaration declaration;
}
