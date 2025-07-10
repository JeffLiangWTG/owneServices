using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageUserControlBag))]
	sealed class TemporaryStorageUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TemporaryStorageUserControlBag.CountryCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.DeclarationDateDateEdit);
				yield return nameof(TemporaryStorageUserControlBag.MessageTypeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.IsENSReuseCheckBox);
				yield return nameof(TemporaryStorageUserControlBag.TransportModeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.TransportTypeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.ArrivalTransportMeansCodeTextBox);
				yield return nameof(TemporaryStorageUserControlBag.DeclarantAddressControl);
				yield return nameof(TemporaryStorageUserControlBag.RepresentativeAddressControl);
				yield return nameof(TemporaryStorageUserControlBag.SupervisingCustomsOfficeCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.PresentationCustomsOfficeCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.GoodsPresentationDateEdit);
				yield return nameof(TemporaryStorageUserControlBag.EstimatedDateOfArrivalDateEdit);
				yield return nameof(TemporaryStorageUserControlBag.PersonPresentingTheGoodsAddressControl);
				yield return nameof(TemporaryStorageUserControlBag.CarrierAddressControl);
				yield return nameof(TemporaryStorageUserControlBag.PlaceOfUnloadingCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.PlaceOfLoadingCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.LocationOfGoodsUserControl);
				yield return nameof(TemporaryStorageUserControlBag.AuthorizationOwnerGuidFindBox);
				yield return nameof(TemporaryStorageUserControlBag.AuthorizationNumberCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.AuthorizationTypeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.HasHouseConsignmentCheckBox);
				yield return nameof(TemporaryStorageUserControlBag.HasNoMasterBillCheckBox);

				yield return nameof(TemporaryStorageUserControlBag.LRNTextBox);
				yield return nameof(TemporaryStorageUserControlBag.MessageStatusDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.CustomsStatusDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.CustomsStatusDateEdit);
				yield return nameof(TemporaryStorageUserControlBag.MRNTextBox);
				yield return nameof(TemporaryStorageUserControlBag.CRNTextBox);
				yield return nameof(TemporaryStorageUserControlBag.FRNTextBox);
				yield return nameof(TemporaryStorageUserControlBag.CusAgentCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.DocumentsTabControl);
				yield return nameof(TemporaryStorageUserControlBag.GuaranteeGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TemporaryStorageUserControlBag.Instance;
	}
}
