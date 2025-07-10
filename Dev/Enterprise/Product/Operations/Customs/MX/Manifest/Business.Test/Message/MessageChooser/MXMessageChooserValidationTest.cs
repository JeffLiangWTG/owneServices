using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.MX.Manifest.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class MXMessageChooserValidationTest : TestCaseWithFactory
	{
		public void TestValidateReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			var chooser = new MXMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);
			ValidationTestHelper.AssertInvalidCodeMessageError(chooser.ReasonInfo, "@#", SEAReasonCodes.Codes.R01);

			chooser.Reason = "01";
			chooser.Validation.ValidateAll();

			AssertNoMessageErrors(chooser.ReasonInfo);
		}
	}
}
