using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.DeclarationActivation.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CH.DeclarationActivation.Module.Testing;

[TestedType(typeof(DeclarationActivationFilterControl))]
sealed class DeclarationActivationFilterControlTest : TestCaseWithFactory
{
	public void TestGridColumns() => CombineAssertions(() =>
	{
		using var control = new DeclarationActivationFilterControl(null /*new DeclarationActivationCollection(Factory)*/, null /*DeclarationActivationFilterStripBusinessObject()*/);
		var index = 0;
		AssertColumn(nameof(DeclarationActivationHeader.CXH_JobReference), isVisible: true, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.ExporterCode), isVisible: true, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.ExporterName), isVisible: true, width: 120);
		AssertColumn(nameof(DeclarationActivationHeader.CER_Type), isVisible: true, width: 40);
		AssertColumn(nameof(DeclarationActivationHeader.TypeDescription), isVisible: true, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CXC_MovementReference), isVisible: true, width: 130);
		AssertColumn(nameof(DeclarationActivationHeader.CXH_OwnerReference), isVisible: true, width: 130);
		AssertColumn(nameof(DeclarationActivationHeader.NextProcedure), isVisible: false, width: 60);
		AssertColumn(nameof(DeclarationActivationHeader.NextProcedureDescription), isVisible: false, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CER_Location), isVisible: false, width: 90);
		AssertColumn(nameof(DeclarationActivationHeader.CER_OfficeOfExport), isVisible: false, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.EdecOriginalTraderUID), isVisible: false, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CER_TransportMode), isVisible: false, width: 40);
		AssertColumn(nameof(DeclarationActivationHeader.CER_TransportID), isVisible: false, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CER_RN_NKTransportNationality), isVisible: false, width: 40);
		AssertColumn(nameof(DeclarationActivationHeader.CXC_ReferenceNumber), isVisible: false, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CER_MessageStatus), isVisible: true, width: 40);
		AssertColumn(nameof(DeclarationActivationHeader.MessageStatusDescription), isVisible: true, width: 80);
		AssertColumn(nameof(DeclarationActivationHeader.CER_Status), isVisible: true, width: 40);
		AssertColumn(nameof(DeclarationActivationHeader.CustomsStatusDescription), isVisible: true, width: 120);

		void AssertColumn(string columnName, bool isVisible, int width)
		{
			var column = control.Grid.GetColumnStyle(columnName);
			AssertType<ZTextBoxColumnStyleInfo>($"{columnName}", column);
			AssertEquals($"{columnName} IsVisible", isVisible, column.IsVisible);
			AssertEquals($"{columnName} Width", width, column.Width);
			AssertEquals($"{columnName} Index", index++, control.Grid.ColumnStyles.IndexOf(column));
		}
	});
}
