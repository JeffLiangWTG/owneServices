using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class BusinessObjectForTestingWithZStringValue : NonPersistentBusinessObject
	{
		public BusinessObjectForTestingWithZStringValue(string value)
		{
			fValue = value;
		}

		public ZString ActualValue
		{
			get { return fValue; }
		}

		readonly string fValue;
	}

	[TestExcludeBusinessObjectsAllHaveTestCases()]
	internal class BusinessObjectForTestingWith3ZTypedFields : NonPersistentBusinessObject
	{
		public BusinessObjectForTestingWith3ZTypedFields(ZInt field1, ZString field2, ZDateTime field3)
		{
			fField1 = field1;
			fField2 = field2;
			fFeild3 = field3;
		}

		public override string TableName
		{
			get
			{
				return "Test[NonPersistent]";
			}
		}

		ZInt fField1;
		ZString fField2;
		ZDateTime fFeild3;

		public ZInt Field1
		{
			get
			{
				return fField1;
			}
			set
			{
				fField1 = value;
			}
		}

		internal ZString Field2
		{
			get
			{
				return fField2;
			}
			set
			{
				fField2 = value;
			}
		}

		public ZDateTime Field3
		{
			get
			{
				return fFeild3;
			}
			set
			{
				fFeild3 = value;
			}
		}
	}

	[TestClass]
	internal class BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor : BusinessObjectCollection<BusinessObjectForTesting>
	{
		public BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor() : base(new BusinessObjectFactory()) { }
	}

	[TestClass]
	internal class DocumentWrapperCollectionForTestingNotAllowNew : DocumentWrapperCollection
	{
		public DocumentWrapperCollectionForTestingNotAllowNew()
			: base(new BusinessObjectFactory())
		{
		}

		public DocumentWrapperCollectionForTestingNotAllowNew(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("Not Support");
		}
	}

	[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
	internal class DummyBODocumentSupportable : DummyBusinessObject, IDocumentSupportable
	{
		public DummyBODocumentSupportable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new DummyBODocumentSupporter(this); }
		}
	}

	[TestExcludeBusinessObjectsAllHaveTestCasesAttribute]
	internal class DummyBODocumentSupporter : DocumentSupporter
	{
		public DummyBODocumentSupporter(DummyBODocumentSupportable parentBO)
			: base(parentBO)
		{
			this.parentBO = parentBO;
		}
		readonly DummyBODocumentSupportable parentBO;

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Test; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case DataContext.UnitTest:
					return new DocumentWrapper[] { new DummyBODocumentWrapper(parentBO) };
			}
			return null;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.UnitTest };
		}
	}

	internal class DummyBODocumentWrapper : DocumentWrapper
	{
		public DummyBODocumentWrapper(DummyBaseBusinessObject parentBO)
			: base(parentBO, parentBO.Factory)
		{
			this.parentBO = parentBO;
		}
		readonly DummyBaseBusinessObject parentBO;

		public ZString Description
		{
			get { return parentBO.Z0_Description; }
		}

		public ZString Text
		{
			get { return parentBO.Z0_VarCharMax; }
		}

		public DummyBODocumentWrapperCollection Collection
		{
			get { return collection ?? (collection = GetNewCollection()); }
		}

		DummyBODocumentWrapperCollection GetNewCollection()
		{
			DummyBusinessObject parentDummyBusinessObject = parentBO as DummyBusinessObject;
			if (parentDummyBusinessObject != null)
			{
				return new DummyBODocumentWrapperCollection(parentDummyBusinessObject.Collection);
			}
			return new DummyBODocumentWrapperCollection(Factory);
		}

		DummyBODocumentWrapperCollection collection;
	}

	[TestClass]
	internal class DummyBODocumentWrapperCollection : DocumentWrapperCollection<DummyBODocumentWrapper>
	{
		public DummyBODocumentWrapperCollection(BusinessObjectFactory factory) : base(factory) { }

		public DummyBODocumentWrapperCollection(DummyChildBusinessObjectCollection parentCollection) : base(parentCollection, parentCollection.Factory) { }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new DummyBODocumentWrapper((DummyBaseBusinessObject)objectToWrap);
		}
	}

	[TestClass]
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	internal class DummyWrapperwithStaticProperty : NonPersistentBusinessObject
	{
		public static int DummyStaticProperty
		{
			get { return 1; }
		}
	}
}
