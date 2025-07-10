using System.Collections.Generic;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageLayout))]
	sealed class UCC6TemporaryStorageLayoutTest : LayoutsAbstractTest
	{
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
				yield return (EUInstance.CountryCodeFindBox, ControlWidthClass.Medium);
				yield return (IEInstance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.DeclarationDateDateEdit, ControlWidthClass.Medium);
				yield return (EUInstance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.TransportTypeDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.ArrivalTransportMeansCodeTextBox, ControlWidthClass.Medium);
				yield return (EUInstance.DeclarantAddressControl, ControlWidthClass.Long);
				yield return (EUInstance.RepresentativeAddressControl, ControlWidthClass.Long);
				yield return (EUInstance.SupervisingCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.PresentationCustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (IEInstance.CustomsOfficeofLodgementCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.GoodsPresentationDateEdit, ControlWidthClass.Medium);
				yield return (EUInstance.EstimatedDateOfArrivalDateEdit, ControlWidthClass.Medium);
				yield return (EUInstance.PersonPresentingTheGoodsAddressControl, ControlWidthClass.Long);
				yield return (EUInstance.PlaceOfUnloadingCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.PlaceOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (EUInstance.AuthorizationTypeDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.AuthorizationOwnerGuidFindBox, ControlWidthClass.Long);
				yield return (EUInstance.AuthorizationNumberCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.HasHouseConsignmentCheckBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EUInstance.LRNTextBox, ControlWidthClass.Long);
				yield return (EUInstance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (EUInstance.CustomsStatusDateEdit, ControlWidthClass.Medium);
				yield return (EUInstance.MRNTextBox, ControlWidthClass.Long);
				yield return (EUInstance.CusAgentCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.DocumentsTabControl, ControlWidthClass.Auto);
			}
		}

		TemporaryStorageUserControlBag IEInstance => TemporaryStorageUserControlBag.Instance;

		EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag EUInstance => EU.TemporaryStorage.GUI.TemporaryStorageUserControlBag.Instance;

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<EU.Business.CusTempStorage.TemporaryStorageHeader>();
	}
}
