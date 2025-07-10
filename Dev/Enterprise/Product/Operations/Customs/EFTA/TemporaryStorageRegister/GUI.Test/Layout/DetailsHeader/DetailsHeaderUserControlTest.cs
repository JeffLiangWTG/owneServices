using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(DetailsHeaderUserControl))]
sealed class DetailsHeaderUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var userControl = new DetailsHeaderUserControl();
		_ = userControl.AssertContainsControl<ZDropEdit>(nameof(userControl.StatusDropEdit),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_Status)));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.PreviousReferenceNumberTextBox),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_PreviousReference)));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.ATBNumberTextBox),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_Reference)));
		_ = userControl.AssertContainsControl<ZDateEdit>(nameof(userControl.ArrvialDateEdit),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_ArrivalDate)));
		_ = userControl.AssertContainsControl<ZDateEdit>(nameof(userControl.PresentationDateEdit),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_PresentationDate)));
		_ = userControl.AssertContainsControl<ZDropEdit>(nameof(userControl.PreviousReferenceTypeDropEdit),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_PreviousReferenceType)));
		_ = userControl.AssertContainsControl<ZTextBox>(nameof(userControl.CustomerReferenceTextBox),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_InternalReference)));
		_ = userControl.AssertContainsControl<ZCodeFindBox>(nameof(userControl.CustomsOfficeCodeFindBox),
		x => x
		.WithBindTo(nameof(CusTempStorageRegHeader.SRH_CustomsOffice)));
	});
}
