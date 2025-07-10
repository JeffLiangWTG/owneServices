using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	public class CusGuaranteeHeaderTest : CusGuaranteeHeaderAbstractTest
	{
		public void TestCorrectTypeDeciding()
		{
			var header = GetNewBusinessObject(Factory);
			Factory.Save();
			AssertType<CusGuaranteeHeader>("Using EU.Business.CusGuaranteeHeader", new BusinessObjectFactory().Load<EU.Business.CusGuaranteeHeader>(header.PK));
			AssertType<CusGuaranteeHeader>("Using BaseCusGuaranteeHeader", new BusinessObjectFactory().Load<Customs.Business.BaseCusGuaranteeHeader>(header.PK));
			AssertType<CusGuaranteeHeader>("Using SharedCusPermitHeader", new BusinessObjectFactory().Load<Customs.Business.SharedCusPermitHeader>(header.PK));
			AssertType<CusGuaranteeHeader>("Using CommonCusPermitHeader", new BusinessObjectFactory().Load<Customs.Business.CommonCusPermitHeader>(header.PK));
		}

		public void TestIMessageAttacheeMembers()
		{
			var header = GetNewBusinessObject(Factory);
			IMessageAttachee messageAttachee = header;
			CombineAssertions(() =>
			{
				AssertEquals("PK", header.PK, messageAttachee.PK);
				AssertEquals("TableName", header.TableName, messageAttachee.TableName);
				AssertEquals("TablePrefix", header.TablePrefix, messageAttachee.TablePrefix);
				AssertEquals("Branch", GlbBranch.CurrentBranch, messageAttachee.Branch);
				AssertNull("CustomsAgent", messageAttachee.CustomsAgent);
				AssertSame("RelatedJob", header, messageAttachee.RelatedJob);
				AssertSame("Factory", Factory, messageAttachee.Factory);
				AssertEquals("LogicalStatus", ZString.Empty, messageAttachee.LogicalStatus);
				AssertEquals("EntryStatus", ZString.Empty, messageAttachee.EntryStatus);
				AssertEquals("MovementReferenceNumber", ZString.Empty, messageAttachee.MovementReferenceNumber);
				header.Messages.AddNew();
				header.Messages.AddNew();
				AssertContainsExactElementsInAnyOrder("Messages", header.Messages, messageAttachee.Messages);
			});
		}

		public void TestIRelatedJobMembers()
		{
			var header = GetNewBusinessObject(Factory);
			IRelatedJob relatedJob = header;
			CombineAssertions(() =>
			{
				AssertEquals("JobNumber", header.HumanReadableName, relatedJob.JobNumber);
				AssertEquals("JobStatus", ZString.Empty, relatedJob.JobStatus);
				AssertEquals("JobDescription", ZString.Empty, relatedJob.JobDescription);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		public static CusGuaranteeHeader GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var permit = factory.NewWithValidTestData<CusGuaranteeHeader>();
			return permit;
		}

		public void TestSupportsMessages()
		{
			var header = GetNewBusinessObject(Factory);
			Assert(header.SupportsMessages);
		}
	}
}
