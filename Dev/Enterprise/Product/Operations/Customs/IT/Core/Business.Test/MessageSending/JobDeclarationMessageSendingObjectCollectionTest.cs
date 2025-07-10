using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingObjectCollection))]
sealed class JobDeclarationMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		Assert(!GetCollectionToTest().AllowNew);
	}

	protected override JobDeclarationMessageSendingObjectCollection GetCollectionToTest()
	{
		return new JobDeclarationMessageSendingObjectCollection(Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new JobDeclarationMessageSendingObjectWithEmptySubTypeForTest(header, new JobDeclarationMessageSendingObjectParent(declaration));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		header = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader header;

	class JobDeclarationMessageSendingObjectWithEmptySubTypeForTest : JobDeclarationMessageSendingObject
	{
		public JobDeclarationMessageSendingObjectWithEmptySubTypeForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
		{
		}

		protected override ZString GetMessageSubType() => ZString.Empty;
	}
}
