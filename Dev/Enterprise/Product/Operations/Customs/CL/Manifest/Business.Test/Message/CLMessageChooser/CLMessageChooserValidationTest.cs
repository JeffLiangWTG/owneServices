using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class CLMessageChooserValidationTest : TestCaseWithFactory
	{
		public void TestValidateReason()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Cancellation);

			chooser.Validation.ValidateAll();

			AssertHasMessageErrorContaining(chooser.ReasonInfo, MandatoryValidation.YouHaveNotEntered);

			chooser.Reason = "Reason";
			chooser.Validation.ValidateAll();

			AssertNoMessageErrors(chooser.ReasonInfo);
		}

		public void TestValidateSeaAmendReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			chooser.AmendReason = "21";
			AssertHasMessageErrorContaining(chooser.AmendReasonInfo, ListValidation.InvalidCodeMessageError);

			chooser.AmendReason = CargoWise.Types.ZString.Empty;
			AssertHasMessageErrorContaining(chooser.AmendReasonInfo, MandatoryValidation.YouHaveNotEntered);

			chooser.AmendReason = "01";
			AssertNoMessageErrors(chooser.AmendReasonInfo);
		}

		public void TestValidateAmendType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			chooser.AmendType = "L";
			AssertHasMessageErrorContaining(chooser.AmendTypeInfo, ListValidation.InvalidCodeMessageError);

			chooser.AmendType = CargoWise.Types.ZString.Empty;
			AssertHasMessageErrorContaining(chooser.AmendTypeInfo, MandatoryValidation.YouHaveNotEntered);

			chooser.AmendType = "M";
			AssertNoMessageErrors(chooser.AmendTypeInfo);
		}

		public void TestValidateAirAmendReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();
			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			chooser.AmendReason = "21";
			AssertHasMessageErrorContaining(chooser.AmendReasonInfo, ListValidation.InvalidCodeMessageError);

			chooser.AmendReason = CargoWise.Types.ZString.Empty;
			AssertHasMessageErrorContaining(chooser.AmendReasonInfo, MandatoryValidation.YouHaveNotEntered);

			chooser.AmendReason = "CONT";
			AssertNoMessageErrors(chooser.AmendReasonInfo);
		}
	}
}
