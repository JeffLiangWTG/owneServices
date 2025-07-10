using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CertificateNumberCollection))]
	sealed class CertificateNumberCollectionTest : ActiveBusinessObjectCollectionTestCase<CertificateNumberCollection>
	{
		public void TestDefaultsForNewElement()
		{
			var certificateNumbers = GetCollectionToTest();
			var newElement = certificateNumbers.AddNew();
			AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Australia, newElement.CE_RN_NKCountryCode);
			AssertEquals("CE_ParentTable", QuarantineExDocHeader.Schema.TableName, newElement.CE_ParentTable);
			AssertEquals("CE_EntryType", CusEntryNumberTypes.Australia.QuarantineCertificateNumber, newElement.CE_EntryType);
			AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, newElement.CE_Category);
			AssertEquals("CE_IssueDate", ZDateTime.Empty, newElement.CE_IssueDate);
			AssertEquals("CE_EntryNum", ZString.Empty, newElement.CE_EntryNum);
		}

		protected override CertificateNumberCollection GetCollectionToTest()
		{
			var helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var quarantineHeader = helper.Header1.QuarantineExDocHeader;
			return new CertificateNumberCollection(quarantineHeader);
		}
	}
}
