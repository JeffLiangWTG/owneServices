using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class CLMessageChooserTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			var chooser = GetChooser();
			AssertType<CLMessageChooserValidation>(chooser.Validation);
		}

		public void TestReason()
		{
			var chooser = GetChooser();
			AssertEquals(255, chooser.ReasonInfo.MaxLength);
		}

		CLMessageChooser GetChooser()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			return new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);
		}
	}

	[TestedType(typeof(CLMessageChooser))]
	class CLMessageChooserBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			return new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);
		}
	}
}
