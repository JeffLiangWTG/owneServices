using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Billing.Testing
{
	[TestedType(typeof(AccBillingHeader))]
	public class AccBillingHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AccBillingHeader>();
			FillTheBizO(header);
			return header;
		}

		public void TestAccBillingHeaderInternalReferenceNumberIsNotCustomisable()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			using (testObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, testObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			{
				var header1 = GetNewBusinessObject() as AccBillingHeader;
				Factory.Save();
				AssertEquals("There should not be any prefix", "00001000", header1.ABH_InternalReferenceNumber);

				var header2 = GetNewBusinessObject() as AccBillingHeader;
				Factory.Save();
				AssertEquals("There should not be any prefix", "00001001", header2.ABH_InternalReferenceNumber);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = base.GetNewBusinessObjectForDeleteTest(factory) as AccBillingHeader;
			FillTheBizO(header);
			if (header.BillingItems.Any())
			{
				var line = header.BillingItems[0];
				line.ABI_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				line.ABI_ParentId = new ZGuid();
				line.ABI_ParentReferenceNumber = TestObjectCreator.GetRandomString(20);
			}
			return header;
		}

		void FillTheBizO(AccBillingHeader header)
		{
			header.ABH_BillingCode = "GSH";
			header.ABH_BillingCounter = 1;
			header.ABH_EventTimeUtc = ZDateTime.UtcNow;
			header.ABH_EventType = "PST";
			header.ABH_InternalReferenceNumber = TestObjectCreator.GetRandomString(8);
			header.ABH_ParentId = new ZGuid();
			header.ABH_ParentReferenceNumber = TestObjectCreator.GetRandomString(20);
			header.ABH_ParentTableCode = JobConsolSchema.Constants.Prefix;
		}

		public void TestDefaultValues()
		{
			var header = Factory.New<AccBillingHeader>();
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, header.ABH_GC_Company);
			AssertEquals("Event User", GlbStaff.CurrentUser.GS_Code, header.ABH_GS_NKEventUser);
		}

		public void TestABH_ParentReferenceNumberMaxLength()
		{
			AssertEquals(JobConsolSchema.JK_UniqueConsignRef.MaxLength, AccBillingHeaderSchema.ABH_ParentReferenceNumber.MaxLength);
		}
	}
}
