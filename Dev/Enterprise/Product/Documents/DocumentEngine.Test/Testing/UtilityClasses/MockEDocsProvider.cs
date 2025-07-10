using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	class MockEDocsProvider : DummyEnterpriseBusinessObject, IEDocsProvider
	{
		BusinessContext businessContext;
		MockEDocsProviderDocumentSupporter documentSupporter;
		BusinessObjectFactory documentSupporterFactory;
		List<MenuItemIdentifier> eDocsConsumers;
		List<IEDocsProvider> eDocsProviders;
		Mock<DocManagerInfo> mockDocManagerInfo;

		public MockEDocsProvider(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessContext BusinessContext
		{
			get { return businessContext; }
			set { businessContext = value; }
		}

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (mockDocManagerInfo == null)
				{
					mockDocManagerInfo = new Mock<DocManagerInfo>(this, "XYZ") { CallBase = true };
					mockDocManagerInfo.Protected().Setup<Array>("GetEDocsProvidersCore").Returns((eDocsProviders == null) ? Array.Empty<IEDocsProvider>() : eDocsProviders.ToArray());
				}
				return mockDocManagerInfo.Object;
			}
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new MockEDocsProviderDocumentSupporter(this)); }
		}

		public BusinessObjectFactory DocumentSupporterFactory
		{
			get { return documentSupporterFactory ?? Factory; }
			set { documentSupporterFactory = value; }
		}

		public void AddEDocsConsumer(MenuItemIdentifier consumer)
		{
			if (eDocsConsumers == null)
			{
				eDocsConsumers = new List<MenuItemIdentifier>();
			}
			eDocsConsumers.Add(consumer);
		}

		public void AddEDocsProvider(IEDocsProvider provider)
		{
			if (eDocsProviders == null)
			{
				eDocsProviders = new List<IEDocsProvider>();
			}
			eDocsProviders.Add(provider);
			if (mockDocManagerInfo != null)
			{
				mockDocManagerInfo.Reset();
				mockDocManagerInfo.Protected().Setup<Array>("GetEDocsProvidersCore").Returns(eDocsProviders.ToArray());
			}
		}

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			EDocsProviderSupporter result = new EDocsProviderSupporter(this);
			if (eDocsConsumers != null)
			{
				foreach (MenuItemIdentifier consumer in eDocsConsumers)
				{
					result.AddConsumer(consumer);
				}
			}
			return result;
		}

		public static MockEDocsProvider New(BusinessObjectFactory factory, BusinessContext businessContext)
		{
			MockEDocsProvider result = factory.New<MockEDocsProvider>();
			result.BusinessContext = businessContext;
			return result;
		}
	}

	#region MockEDocsProviderDocumentSupporter

	class MockEDocsProviderDocumentSupporter : DocumentSupporter
	{
		public MockEDocsProviderDocumentSupporter(MockEDocsProvider parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessObject.BusinessContext; }
		}

		protected new MockEDocsProvider BusinessObject
		{
			get { return (MockEDocsProvider)base.BusinessObject; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { throw new NotImplementedException(); }
		}

		public override BusinessObjectFactory Factory
		{
			get { return BusinessObject.DocumentSupporterFactory; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			return new IDocumentSupportable[] { BusinessObject };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { null };
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
