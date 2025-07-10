using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using EUTemporaryStorageUserControlBag = Enterprise.Customs.EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStorageLayout))]
sealed class UCC6TemporaryStorageLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EUTemporaryStorageUserControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.TransportTypeDropEdit, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
			yield return (EUTemporaryStorageUserControlBag.Instance.DeclarantAddressControl, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.RepresentativeAddressControl, ControlWidthClass.Long);
			yield return (TemporaryStorageUserControlBag.Instance.RepresentativeQualificationDropEdit, ControlWidthClass.Long);
			yield return (TemporaryStorageUserControlBag.Instance.AccountNameDropEdit, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EUTemporaryStorageUserControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
			yield return (EUTemporaryStorageUserControlBag.Instance.CustomsStatusDateEdit, ControlWidthClass.Medium);
		}
	}
}
