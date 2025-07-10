using System;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupporter.DocumentSupporterHelper;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	class DocumentSupporterBaseTest : TestCaseWithFactory
	{
		public virtual void TestNullParentBusinessObjectNotAllowed()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate { GetNewDocSupporter(null); });
		}

		public void TestIsDataContextSupportedForDocumentWrapperDataContext()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.UnitTest)));
			AssertEquals(false, DocSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Declaration)));
		}

		public virtual void TestIsDataContextSupportedForBusinessObjectDataContext()
		{
			AssertEquals(true, DocSupporter.IsDataContextSupported(new DataContextValue(".DummyEnterpriseBusinessObject")));
			// Exception thrown because it's too long a name. Can only store 35 chars.
			AssertExceptionThrown(typeof(DataContextIsInvalidException), delegate { DocSupporter.IsDataContextSupported(new DataContextValue(".Business.Testing.DummyEnterpriseBusinessObject")); });
			AssertEquals(false, DocSupporter.IsDataContextSupported(new DataContextValue(".JobDeclaration")));
		}

		public void TestFilterForSupportedDataContexts()
		{
			ZQuery filter = DocSupporter.FilterForSupportedDataContexts;
			AssertEquals(ExpectedFilterForSupportedDataContexts, filter.LiteralTextADO);
		}

		public void TestGetFilterValue()
		{
			var filters = Enum.GetValues(typeof(DocumentFilters)).Cast<DocumentFilters>();
			foreach (var filter in filters)
			{
				if (filter == DocumentFilters.CTY)
				{
					IGlbCompany currentCompany = Factory.Load<IGlbCompany>(Env.CurrentCompany.PK);
					using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
					{
						AssertEquals("Country filter should return CA", Core.Constants.CountryCodes.Canada, DocSupporter.GetFilterValue(DocumentFilters.CTY));
					}

					using (currentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
					{
						AssertEquals("Country filter should return AU", Core.Constants.CountryCodes.Australia, DocSupporter.GetFilterValue(DocumentFilters.CTY));
					}
				}
				else
				{
					AssertNull(DocSupporter.GetFilterValue(filter));
				}
			}
		}

		protected virtual string ExpectedFilterForSupportedDataContexts
		{
			get { return "(SO_DataContext in ('.DummyEnterpriseBusinessObject', 'UnitTest'))"; }
		}

		public virtual void TestGetBODocDataProvidersForDocumentWrapperDataContext()
		{
			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.UnitTest), null);
			AssertNotNull("GetBODocDataProviders for DataContext.UnitTest", results);
			AssertEquals("GetBODocDataProviders Count for DataContext.UnitTest", 1, results.Length);
			AssertEquals("GetBODocDataProviders for DataContext.UnitTest", typeof(DocumentWrapperForTesting), results[0].GetType());

			results = DocSupporter.GetBODocDataProviders(new DataContextValueForTesting(DataContext.Declaration), null);
			AssertNull("GetBODocDataProviders for DataContext.Declaration", results);
		}

		public virtual void TestGetBODocDataProvidersForBusinessObjectDataContext()
		{
			IBODocDataProvider[] results = DocSupporter.GetBODocDataProviders(new DataContextValue(".DummyEnterpriseBusinessObject"), null);
			AssertNotNull("GetBODocDataProviders for .DummyEnterpriseBusinessObject should not be null.", results);
			AssertEquals("GetBODocDataProviders Count for .DummyEnterpriseBusinessObject", 1, results.Length);
			AssertEquals("GetBODocDataProviders for .DummyEnterpriseBusinessObject", typeof(DummyEnterpriseBusinessObject), BODocDataProvider.GetBusinessObject(results[0]).GetType());

			results = DocSupporter.GetBODocDataProviders(new DataContextValue(".JobDeclaration"), null);
			AssertNull("GetBODocDataProviders for .JobDeclaration should be null.", results);
		}

		public virtual void TestWantsBusinessObjectOfTypeUseTheRightType()
		{
			var dummyBO = Factory.New<DummyBusinessObjectModule>();
			var docSupporter = GetNewDocSupporter(dummyBO);

			IBODocDataProvider[] results = docSupporter.GetBODocDataProviders(new DataContextValue(".DummyEnterpriseBusinessObject"), null);
			AssertNotNull("GetBODocDataProviders for .DummyEnterpriseBusinessObject should not be null.", results);
		}

		class DummyBusinessObjectModule : DummyEnterpriseBusinessObject, ITopLevelBusinessEntityForDocSup
		{
			public DummyBusinessObjectModule(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public Type TopLevelBusinessEntity => typeof(DummyEnterpriseBusinessObject);
		}		

		public virtual void TestCommaSeparatedListOfSupportedDataContexts()
		{
			AssertEquals("UnitTest, .DummyEnterpriseBusinessObject", DocSupporter.CommaSeparatedListOfSupportedDataContexts);
		}

		public virtual void TestListOfSupportedDataContexts()
		{
			CodeDescriptionPairList list = DocSupporter.ListOfSupportedDataContexts;
			AssertEquals("list.Count", 2, list.Count);
			AssertEquals("list[0].Code", "UnitTest", list[0].Code);
			AssertEquals("list[1].Code", ".DummyEnterpriseBusinessObject", list[1].Code);
		}

		public virtual void TestShowReasonForNotPrinting()
		{
			AssertEquals(true, DocSupporter.ShowReasonForNotPrinting(DataContext.None, null));
		}

		public virtual void TestCustomWaremarkText()
		{
			AssertNull(DocSupporter.CustomWatermarkText);
		}

		public virtual void TestCustomWaremarkTextFromCommand()
		{
			IDocumentCommand docCommand = null;
			IBODocDataProvider docDataProvider = null;
			AssertNull(DocSupporter.GetCustomWatermarkText(docCommand, docDataProvider));
		}

		public void TestGetPDFEncryptionPasswordIfNeeded()
		{
			AssertNullOrEmpty(DocSupporter.GetEncryptedPDFPassword(null));
		}

		#region Implementation

		protected DummyEnterpriseBusinessObject BizObject
		{
			get
			{
				if (fBizObject == null)
				{
					fBizObject = GetNewBusinessObject();
				}
				return fBizObject;
			}
		}
		DummyEnterpriseBusinessObject fBizObject;

		protected virtual DummyEnterpriseBusinessObject GetNewBusinessObject()
		{
			return Factory.New<DummyEnterpriseBusinessObject>();
		}

		protected DocumentSupporterForTesting DocSupporter
		{
			get
			{
				if (fDocSupporter == null)
				{
					fDocSupporter = GetNewDocSupporter(BizObject);
				}
				return fDocSupporter;
			}
		}
		DocumentSupporterForTesting fDocSupporter;

		protected virtual DocumentSupporterForTesting GetNewDocSupporter(DummyEnterpriseBusinessObject parentBusinessObject)
		{
			return new DocumentSupporterForTesting(parentBusinessObject);
		}

		internal class DocumentSupporterForTesting : DocumentSupporter
		{
			public DocumentSupporterForTesting(DummyEnterpriseBusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
				DummyBO = parentBusinessObject;
			}
			protected readonly DummyEnterpriseBusinessObject DummyBO;

			public override BusinessContext BusinessContext
			{
				get { return CargoWise.Definitions.BusinessContext.Test; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				switch (dataContext)
				{
					case DataContext.UnitTest:
						return new DocumentWrapper[] { new DocumentWrapperForTesting(DummyBO) };
				}
				return null;
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				return new DataContext[] { DataContext.UnitTest };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return null; }
			}
		}

		protected class DocumentWrapperForTesting : DocumentWrapper
		{
			public DocumentWrapperForTesting(DummyBusinessObject parentBusinessObject)
				: base(parentBusinessObject, parentBusinessObject.Factory)
			{
				DummyBO = parentBusinessObject;
			}
			readonly DummyBusinessObject DummyBO;

			public ZString Text
			{
				get { return DummyBO.Z0_VarCharMax; }
			}
		}

		#endregion
	}
}
