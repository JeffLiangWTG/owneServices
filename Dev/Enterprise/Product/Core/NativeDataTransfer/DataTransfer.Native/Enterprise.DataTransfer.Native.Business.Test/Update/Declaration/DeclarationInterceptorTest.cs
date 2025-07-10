using System.Linq;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.DataTransfer.Native.Business.XmlConstants;

namespace Enterprise.DataTransfer.Native.Business.Update.Declaration
{
	sealed class DeclarationInterceptorTest : TransactionedTestCase
	{
		public void TestDataModelPopulatesFromDeclarationCompany()
		{
			var sessionServices = new AncillaryImportServices();
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var targetCompany = context.ObjectFactory.New<GlbCompany>();
			targetCompany.GC_RN_NKCountryCode = "Z1";

			var headerDataMock = new Mock<HeaderData>();
			var headerData = headerDataMock.Object;
			headerData.TargetCompanyPK = targetCompany.PK.ToGuid();

			var setting = new DeclarationInterceptorSetting(headerData) { Context = context };
			var interceptor = new DeclarationInterceptor(setting, sessionServices) { Function = e => { } };

			var invoiceLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine"), sessionServices);
			var invoiceHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader"), sessionServices);
			invoiceHeader.ChildrenCollection.Add(invoiceLine);

			var entryLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader.CusEntryLine"), sessionServices);
			var entryHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader"), sessionServices);
			entryHeader.ChildrenCollection.Add(entryLine);

			var declaration = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration"), sessionServices);
			declaration.ChildrenCollection.Add(invoiceHeader);
			declaration.ChildrenCollection.Add(entryHeader);

			var glbCompany = context.ObjectFactory.New<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "ZZ";

			var company = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.GlbCompany"), sessionServices)
			{
				InternalPK = glbCompany.PK.ToGuid()
			};
			declaration.ParentCollection.Add(company);

			var declarationSet = new EntitySet(EntitySetNames.Declaration) { Root = declaration };

			AssertEquals(false, declaration.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceLine.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryLine.HasProperty(PropertyNames.DataModel));

			interceptor.Invoke(declarationSet);

			AssertEquals("ZZ", declaration[PropertyNames.DataModel]);
			AssertEquals("ZZ", invoiceHeader[PropertyNames.DataModel]);
			AssertEquals("ZZ", invoiceLine[PropertyNames.DataModel]);
			AssertEquals("ZZ", entryHeader[PropertyNames.DataModel]);
			AssertEquals("ZZ", entryLine[PropertyNames.DataModel]);
		}

		public void TestDataModelPopulatesFromTargetCompany()
		{
			var sessionServices = new AncillaryImportServices();
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var glbCompany = context.ObjectFactory.New<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = "ZZ";

			var headerDataMock = new Mock<HeaderData>();
			var headerData = headerDataMock.Object;
			headerData.TargetCompanyPK = glbCompany.PK.ToGuid();

			var setting = new DeclarationInterceptorSetting(headerData) { Context = context };
			var interceptor = new DeclarationInterceptor(setting, sessionServices) { Function = e => { } };

			var invoiceLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine"), sessionServices);
			var invoiceHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader"), sessionServices);
			invoiceHeader.ChildrenCollection.Add(invoiceLine);

			var entryLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader.CusEntryLine"), sessionServices);
			var entryHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader"), sessionServices);
			entryHeader.ChildrenCollection.Add(entryLine);

			var declaration = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration"), sessionServices);
			declaration.ChildrenCollection.Add(invoiceHeader);
			declaration.ChildrenCollection.Add(entryHeader);

			var declarationSet = new EntitySet(EntitySetNames.Declaration) { Root = declaration };

			AssertEquals(false, declaration.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceLine.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryLine.HasProperty(PropertyNames.DataModel));

			interceptor.Invoke(declarationSet);

			AssertEquals("ZZ", declaration[PropertyNames.DataModel]);
			AssertEquals("ZZ", invoiceHeader[PropertyNames.DataModel]);
			AssertEquals("ZZ", invoiceLine[PropertyNames.DataModel]);
			AssertEquals("ZZ", entryHeader[PropertyNames.DataModel]);
			AssertEquals("ZZ", entryLine[PropertyNames.DataModel]);
		}

		public void TestDataModelPopulatesFromEnvCompany()
		{
			var headerDataMock = new Mock<HeaderData>();
			var headerData = headerDataMock.Object;

			var sessionServices = new AncillaryImportServices();
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new DeclarationInterceptorSetting(headerData) { Context = context };
			var interceptor = new DeclarationInterceptor(setting, sessionServices) { Function = e => { } };

			var invoiceLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader.JobComInvoiceLine"), sessionServices);
			var invoiceHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.JobComInvoiceHeader"), sessionServices);
			invoiceHeader.ChildrenCollection.Add(invoiceLine);

			var entryLine = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader.CusEntryLine"), sessionServices);
			var entryHeader = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration.CusEntryHeader"), sessionServices);
			entryHeader.ChildrenCollection.Add(entryLine);

			var declaration = new Entity(TestUtil.FindEntityDefinition(EntitySetNames.Declaration, "JobDeclaration"), sessionServices);
			declaration.ChildrenCollection.Add(invoiceHeader);
			declaration.ChildrenCollection.Add(entryHeader);

			var declarationSet = new EntitySet(EntitySetNames.Declaration) { Root = declaration };

			AssertEquals(false, declaration.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, invoiceLine.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryHeader.HasProperty(PropertyNames.DataModel));
			AssertEquals(false, entryLine.HasProperty(PropertyNames.DataModel));

			interceptor.Invoke(declarationSet);

			AssertEquals("AU", declaration[PropertyNames.DataModel]);
			AssertEquals("AU", invoiceHeader[PropertyNames.DataModel]);
			AssertEquals("AU", invoiceLine[PropertyNames.DataModel]);
			AssertEquals("AU", entryHeader[PropertyNames.DataModel]);
			AssertEquals("AU", entryLine[PropertyNames.DataModel]);
		}

		public void TestEnableList()
		{
			var setting = new DeclarationInterceptorSetting(null);
			AssertContainsExactElementsInAnyOrder(new[] { EntitySetNames.Declaration }, setting.EnableList);
		}

		public void TestDisableList()
		{
			var setting = new DeclarationInterceptorSetting(null);
			AssertEquals(0, setting.DisableList.Count());
		}
	}
}
