using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntryLineCollection))]
	sealed class CusReconEntryLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusReconEntryLineCollection>
	{
		protected override CusReconEntryLineCollection GetCollectionToTest()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			return new CusReconEntryLineCollection(reconDeclaration);
		}

		public void TestAddNewAndDelete()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			AssertEquals(0, reconDeclaration.CusReconEntries.Count);
			AssertEquals(0, reconDeclaration.CusReconEntryLines.Count);

			Assert(!reconDeclaration.IsRefundTypeContractRevocation);
			var entryLine1 = reconDeclaration.CusReconEntryLines.AddNew();
			AssertEquals(1, reconDeclaration.CusReconEntries.Count);
			AssertEquals(1, reconDeclaration.CusReconEntryLines.Count);
			AssertNull(reconDeclaration.CusReconEntryLines[0].ContractRevocation);
			AssertEquals(entryLine1.CRL_CRE, reconDeclaration.CusReconEntries[0].PK);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			Assert(reconDeclaration.IsRefundTypeContractRevocation);
			var entryLine2 = reconDeclaration.CusReconEntryLines.AddNew();
			AssertEquals(2, reconDeclaration.CusReconEntries.Count);
			AssertEquals(2, reconDeclaration.CusReconEntryLines.Count);
			AssertNotNull(reconDeclaration.CusReconEntryLines[0].ContractRevocation);
			AssertNotNull(reconDeclaration.CusReconEntryLines[1].ContractRevocation);
			AssertEquals(entryLine2.CRL_CRE, reconDeclaration.CusReconEntries[1].PK);

			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.E;
			Assert(!reconDeclaration.IsRefundTypeContractRevocation);
			AssertNull(reconDeclaration.CusReconEntryLines[0].ContractRevocation);
			AssertNull(reconDeclaration.CusReconEntryLines[1].ContractRevocation);

			reconDeclaration.CusReconEntryLines.Delete(entryLine2);
			AssertEquals(1, reconDeclaration.CusReconEntries.Count);
			AssertEquals(1, reconDeclaration.CusReconEntryLines.Count);
			AssertEquals(entryLine1.CRL_CRE, reconDeclaration.CusReconEntries[0].PK);
		}

		public void TestContractRevocation()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(factory, "BUS", "KR1", "TestCompany");
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();

			var reconDeclaration = factory.NewWithValidTestData<CusReconDeclaration>();
			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			reconDeclaration.CusReconEntryLines.AddNew();
			var reconEntry = reconDeclaration.CusReconEntries[0];

			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var reconEntryLine = reconDeclaration.CusReconEntryLines[0];
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			AssertEquals(1, reconEntryLine.ContractRevocations.Count);	
			factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var savedReconDeclaration = anotherFactory.Load<CusReconDeclaration>(reconDeclaration.PK);
			AssertEquals(1, savedReconDeclaration.CusReconEntryLines[0].ContractRevocations.Count);
		}
	}
}
