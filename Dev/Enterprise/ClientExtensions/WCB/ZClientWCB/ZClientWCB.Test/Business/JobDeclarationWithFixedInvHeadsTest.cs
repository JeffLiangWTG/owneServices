using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	[TestedType(typeof(JobDeclarationWithFixedInvHeads))]
	internal class JobDeclarationWithFixedInvHeadsTest : BaseJobDeclarationAbstractTest
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.AUJobDeclaration);
		public void TestCreateNewInvoiceHeaderCollection()
		{
			var declaration = Factory.New<JobDeclarationWithFixedInvHeads>();
			AssertEquals("Type is InvHeadFixedCollection", typeof(InvHeadFixedCollection), declaration.FixedInvoices.GetType());
		}

		public void TestDeclarationType()
		{
			var anotherFactory = new BusinessObjectFactory();
			var collection = new Enterprise.Customs.Business.BaseJobDeclarationCollection(anotherFactory);
			collection.IsManagedForDataRefresh = true;
			collection.Load();
			var declaration = Factory.NewWithValidTestData<JobDeclarationWithFixedInvHeads>();
			AssertNoExceptionThrown(() =>
			{
				BusinessObjectFactory.SaveTogether(Factory, anotherFactory);
			});
		}
	}
}
