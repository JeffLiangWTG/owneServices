using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class SummaryDeclarationsCommonSendMessageWrapperBaseOnlyTest<T> : WrapperHelperTest<T>
				where T : SummaryDeclarationsCommonSendMessageWrapper
	{
		public void TestSenderID()
		{
			AssertEquals("Expected filled SenderId", Certificate.CertificateID, wrapper.SenderId);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		T wrapper;

		protected abstract T GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData);

		protected override T GetProvider() => wrapper;
	}
}
