using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationControlBag))]
	sealed class CommonDeclarationControlBagTest
	: ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(CommonDeclarationControlBag.ExportCodeTextBox),
					nameof(CommonDeclarationControlBag.ExportNameTextBox),
					nameof(CommonDeclarationControlBag.DeclarantCodeTextBox),
					nameof(CommonDeclarationControlBag.CarrierCodeCodeFindBox),
					nameof(CommonDeclarationControlBag.VesselCodeFindBox),
					nameof(CommonDeclarationControlBag.VesselNameCodeFindBox),
					nameof(CommonDeclarationControlBag.DeclarationReferenceTextBox),
					nameof(CommonDeclarationControlBag.FinalDestinationCodeFindBox),
					nameof(CommonDeclarationControlBag.ExportControlNumberUserControl),
					nameof(CommonDeclarationControlBag.VoyageFlightNoBoundTextBox),
					nameof(CommonDeclarationControlBag.DateOfArrivalBoundDateEdit),
					nameof(CommonDeclarationControlBag.PortOfLoadingCodeFindBox),
					nameof(CommonDeclarationControlBag.ExportDateBoundDateEdit),
					nameof(CommonDeclarationControlBag.PortOfDischargeCodeFindBox),
					nameof(CommonDeclarationControlBag.ReceiptModeDropEdit),
					nameof(CommonDeclarationControlBag.DeliveryModeDropEdit),
					nameof(CommonDeclarationControlBag.BookingNumberTextBox),
					nameof(CommonDeclarationControlBag.AllEntryInsSeparatorUserControl),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonDeclarationControlBag.Instance;
	}
}
