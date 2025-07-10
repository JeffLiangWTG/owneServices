
using Enterprise.Customs.EU.Business;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Segments;
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CUSCARGeneratorFRI : CUSCARGeneratorBase
	{
		public CUSCARGeneratorFRI(ICcsukCusAwb awb, ErrorCollector ec)
			: base(awb, ec)
		{
			MaybeAddRedErrorForTesting(awb);
		}

		protected override string AssociationAssignedCode
		{
			get { return "109502"; }
		}

		protected override string BgmDocumentName
		{
			get { return "FRI"; }
		}

		protected override string BgmDocumentNameHuman
		{
			get { return "Insert Freight Record"; }
		}

		protected override void MakeCommunityHandlingCodes()
		{
			MakeCommunityHandlingCodesShared(iCuscar, result.GIS);
		}

		internal static void MakeCommunityHandlingCodesShared(ICuscar iCuscar, SegmentMessageSection<GISSegment> gisSegmentSection)
		{
			int index = 0;
			foreach (var handlingCode in iCuscar.CommunityHandlingCodes)
			{
				index++;
				var gisSpecialHandling131 = gisSegmentSection.InstantiateAChildAndAddItToChildrenCollection();
				gisSpecialHandling131.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(handlingCode.KeepAlphanumericCharacters().ToUpper());  // Shipment description code
				gisSpecialHandling131.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.GetFromString("131");
				if (index > 33)
				{
					break; // only 33 repetitions allowed
				}
			}
		}

		void MaybeAddRedErrorForTesting(ICcsukCusAwb awb)
		{
#if DEBUG
			if (awb.AirportOfArrival == "XXX" && awb.ReferenceNumber.Contains("TSTCRASH"))
			{
				errorCollector.AddError("Error for testing");  // See AutoFrcTests.TestDoNotKeepTemporaryFriMessagesIfRedStopOccursDuringGeneration()
			}
#endif
		}

		protected override string GetCuscarMessageVersionTwoOrThree()
		{
			return allowVersion3CusCar ? "3" : "2";
		}
	}
}
