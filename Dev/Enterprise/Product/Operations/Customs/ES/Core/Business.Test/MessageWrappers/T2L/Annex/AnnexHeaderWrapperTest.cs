using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AnnexHeaderWrapperTest : WrapperHelperTest<AnnexHeaderWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AnnexHeaderWrapper(null, false));
		}

		public void TestT2LReferenceNumber()
		{
			entryHeader.MovementReferenceNumber = HeaderData.MovementReferenceNumber;
			AssertEquals("Expected filled T2LReferenceNumber", HeaderData.MovementReferenceNumber, wrapper.T2LReferenceNumber);
		}

		public void TestNullDeclarant()
		{
			declaration.Declarant.OA_OH = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declarant.OA_OH = orgHeader.PK;
			var declarant = wrapper.Declarant;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Declarant", declarant);
				AssertSame("Cached Declarant", wrapper.Declarant, declarant);
			});
		}

		public void TestFinalAnnexIndicator()
		{
			CombineAssertions(() =>
			{
				var wrap1 = new AnnexHeaderWrapper(entryHeader, false);
				AssertEquals("Expected false FinalAnnexIndicator", false, wrap1.FinalAnnexIndicator);

				var wrap2 = new AnnexHeaderWrapper(entryHeader, true);
				AssertEquals("Expected true FinalAnnexIndicator", true, wrap2.FinalAnnexIndicator);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			wrapper = new AnnexHeaderWrapper(entryHeader, false);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		AnnexHeaderWrapper wrapper;

		protected override AnnexHeaderWrapper GetProvider() => wrapper;
	}
}
