using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class MXMessageChooserTest : TestCaseWithFactory
	{
		public void TestReason()
		{
			var chooser = GetChooser();
			AssertEquals(2, chooser.ReasonInfo.MaxLength);
		}

		public void TestLookups()
		{
			var chooser = GetChooser();
			AssertType<MXMessageChooserLookups>(chooser.Lookups);
		}

		public void TestValidation()
		{
			var chooser = GetChooser();
			AssertType<MXMessageChooserValidation>(chooser.Validation);
		}

		MXMessageChooser GetChooser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			return new MXMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);
		}
	}

	[TestedType(typeof(MXMessageChooser))]
	class MXMessageChooserBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			return new MXMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);
		}
	}
}
