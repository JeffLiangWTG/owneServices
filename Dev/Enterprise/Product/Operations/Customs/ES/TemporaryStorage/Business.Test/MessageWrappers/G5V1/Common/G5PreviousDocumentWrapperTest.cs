using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5PreviousDocumentWrapperTest : WrapperHelperTest<G5PreviousDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "doc"), () => GetWrapper("", null));
		}

		public void TestPreviousTSD()
		{
			CombineAssertions(() =>
			{
				var previousTSD = wrapper.PreviousTSD;
				AssertNotNull("Expected filled PreviousTSD", previousTSD);
				AssertSame("Cached PreviousTSD", wrapper.PreviousTSD, previousTSD);

				wrapper = GetWrapper("FR009999", doc);
				AssertNull("Expected empty PreviousTSD when customsOffice doesn't start with ES", wrapper.PreviousTSD);
			});
		}

		public void TestPreviousGeneric()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected empty PreviousGeneric when customsOffice starts with ES", wrapper.PreviousGeneric);

				wrapper = GetWrapper("FR009999", doc);
				var previousGeneric = wrapper.PreviousGeneric;
				AssertNotNull("Expected filled PreviousGeneric", previousGeneric);
				AssertSame("Cached PreviousGeneric", wrapper.PreviousGeneric, previousGeneric);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			doc = Factory.New<TemporaryStoragePreviousDocument>();
			wrapper = GetWrapper("ES009999", doc);
		}

		TemporaryStoragePreviousDocument doc;
		G5PreviousDocumentWrapper wrapper;

		G5PreviousDocumentWrapper GetWrapper(ZString customsOffice, TemporaryStoragePreviousDocument doc) => new G5PreviousDocumentWrapper(customsOffice, doc);

		protected override G5PreviousDocumentWrapper GetProvider() => wrapper;
	}
}
