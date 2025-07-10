using System.Linq;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(ReexportH7SendMessageWrapper))]
	public class ReexportH7SendMessageWrapperTest : H7CommonSendMessageWrapperBaseTest<ReexportH7SendMessageWrapper>
	{ 
		public void TestOperationCode()
		{
			operationCode = "3";

			AssertEquals("3", Provider.OperationCode);
		}

		public void TestDeclarant_WhenHasId_ShouldPopulateIdAndName()
		{
			var companyName = "My company";
			var eori = "ES123456789000";
			PrepareTestData(cusCodeType: "EOR", cusCodeRegNo: eori, companyName);

			CombineAssertions(() =>
			{
				AssertEquals("Declarant.Id", eori, Provider.Declarant.Id);
				AssertEquals("Declarant.Name", companyName, Provider.Declarant.Name);
			});
		}

		public void TestDeclarant_WhenHasNoId_ShouldPopulateOnlyName()
		{
			var companyName = "My company";
			PrepareTestData(cusCodeType: "PAS", cusCodeRegNo: "P000", companyName);

			CombineAssertions(() =>
			{
				AssertEquals("Declarant.Id", string.Empty, Provider.Declarant.Id);
				AssertEquals("Declarant.Name", companyName, Provider.Declarant.Name);
			});
		}

		public void TestDeclarationMRNCodes()
		{
			var mrn = "MRN0002";
			PrepareTestData(mrn: mrn);

			AssertEquals(mrn, Provider.DeclarationMRNCodes.Single());
		}

		public void TestDeclarationMRNCodes_WhenMRNIsEmpty_ShouldReturnEmptyList()
		{
			PrepareTestData(mrn: string.Empty);

			AssertEquals(0, Provider.DeclarationMRNCodes.Count);
		}

		void PrepareTestData(string cusCodeType = "", string cusCodeRegNo = "", string companyName = "Company", string mrn = "MRN0001")
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = companyName;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var cusCode = orgAddress.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = cusCodeRegNo;
			cusCode.OK_CodeType = cusCodeType;

			var broker = Factory.NewWithValidTestData<GlbStaff>();

			header.AMA_OA_Declarant = orgAddress.PK;
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			bill.MovementReferenceNumber = mrn;
		}

		protected override void SetUp()
		{
			base.SetUp();

			operationCode = "0";
		}

		protected override ReexportH7SendMessageWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate)
		{
			return new ReexportH7SendMessageWrapper(bill, certificate, operationCode);
		}

		string operationCode;
	}
}
