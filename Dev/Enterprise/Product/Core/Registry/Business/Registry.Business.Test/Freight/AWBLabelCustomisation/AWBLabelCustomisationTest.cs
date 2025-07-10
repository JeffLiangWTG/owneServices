using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AWBLabelCustomisation))]
	sealed class AWBLabelCustomisationTest : RegistryBusinessObjectTemplateTestCase<AWBLabelCustomisation>
	{
		#region Implementation

		protected override AWBLabelCustomisation GetBusinessObjectToClone()
		{
			return new AWBLabelCustomisation();
		}

		protected override AWBLabelCustomisation GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion

		public void TestGetDefault()
		{
			AWBLabelCustomisation awbLabel = AWBLabelCustomisation.GetDefault();

			AssertEquals("Defaul design", AWBLabelCustomDesignList.Codes.Default, awbLabel.CustomDesign);

			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.HousebillNumber, awbLabel.OptionalInformation1);
			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount, awbLabel.OptionalInformation2);
			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.ConsolOrigin, awbLabel.OptionalInformation3);
			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.Blank, awbLabel.OptionalInformation4);
			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.Blank, awbLabel.OptionalInformation5);
			AssertEquals("Defaul optional information fields", AWBLabelOptionalInformationList.Codes.Blank, awbLabel.OptionalInformation6);

			AssertEquals("Defaul optional information fields", "HAWB NO", awbLabel.OptionalDescription1);
			AssertEquals("Defaul optional information fields", "PIECE COUNT", awbLabel.OptionalDescription2);
			AssertEquals("Defaul optional information fields", "ORIGIN", awbLabel.OptionalDescription3);
			AssertEquals("Defaul optional information fields", "", awbLabel.OptionalDescription4);
			AssertEquals("Defaul optional information fields", "", awbLabel.OptionalDescription5);
			AssertEquals("Defaul optional information fields", "", awbLabel.OptionalDescription6);

			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeConsolDestination);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeConsolOrigin);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeConsolPieceCount);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeConsolWeight);
			AssertEquals("Defaul optional information fields", true, awbLabel.BarcodeHousebillNumber);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeShipmentHandlingInformation);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeShipmentPieceCount);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeShipmentPieceNumber);
			AssertEquals("Defaul optional information fields", false, awbLabel.BarcodeShipmentWeight);
		}

		public void TestValidation()
		{
			AWBLabelCustomisation awbLabel = AWBLabelCustomisation.GetDefault();

			AssertEquals("Precondition - no validation errors", false, awbLabel.CustomDesignInfo.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation1Info.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation2Info.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation3Info.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation4Info.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation5Info.HasErrors());
			AssertEquals("Precondition - no validation errors", false, awbLabel.OptionalInformation6Info.HasErrors());

			awbLabel.CustomDesign = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.CustomDesignInfo.HasErrors());

			awbLabel.OptionalInformation1 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation1Info.HasErrors());

			awbLabel.OptionalInformation2 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation2Info.HasErrors());

			awbLabel.OptionalInformation3 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation3Info.HasErrors());

			awbLabel.OptionalInformation4 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation4Info.HasErrors());

			awbLabel.OptionalInformation5 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation5Info.HasErrors());

			awbLabel.OptionalInformation6 = "crap";
			AssertEquals("Validation error on invalid value entered", true, awbLabel.OptionalInformation6Info.HasErrors());
		}

		public void TestDescriptions()
		{
			AWBLabelCustomisation awbLabel = AWBLabelCustomisation.GetDefault();

			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.ConsolOrigin;
			AssertEquals("Description is set and is in UPPERCASE", AWBLabelOptionalInformationList.Codes.ConsolOrigin.ToUpper(), awbLabel.OptionalDescription1);

			awbLabel.OptionalInformation1 = AWBLabelOptionalInformationList.Codes.ConsolDestination;
			AssertEquals("Description is set and is in UPPERCASE", AWBLabelOptionalInformationList.Codes.ConsolDestination.ToUpper(), awbLabel.OptionalDescription1);

			awbLabel.OptionalDescription1 = "some text";
			AssertEquals("Description can be overriden", "some text", awbLabel.OptionalDescription1);
		}
	}
}
