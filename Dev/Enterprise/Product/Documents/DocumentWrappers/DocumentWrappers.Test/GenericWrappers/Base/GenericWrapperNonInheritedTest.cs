using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestedType(typeof(ConcreteWrapper))]
	sealed class GenericWrapperNonInheritedTest : GenericWrapperTest
	{
		public void TestWrappedObjectPKComesFromWrappedBO()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			ConcreteWrapper dummyWrapper = new ConcreteWrapper(dummyBO, Factory);
			AssertEquals("dummyWrapper.WrappedObjectPK", dummyBO.PK, dummyWrapper.WrappedObjectPK);

			ConcreteWrapper nullWrapper = new ConcreteWrapper(null, Factory);
			AssertEquals("nullWrapper.WrappedObjectPK.IsEmpty", false, nullWrapper.WrappedObjectPK.IsEmpty);
		}

		public void TestRegistry()
		{
			RegistryWrapper wrapper = new ConcreteWrapper(Factory).Registry;
			AssertNotNull("ConcreteWrapper.Registry should never be null", wrapper);
			AssertEquals(Factory, wrapper.Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			ConcreteWrapper wrapperEmpty = new ConcreteWrapper(Factory);
			AssertEquals("wrapperEmpty.ToString()", "GoodMayyyyte", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Strength", "GoodMayyyyte", wrapperEmpty.Strength);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Concrete                                     (Default Field: Strength)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Strength                                String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new ConcreteWrapper(null, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ConcreteWrapper(Factory);
		}

		[DefaultField("Strength")]
		class ConcreteWrapper : GenericWrapper
		{
			public ConcreteWrapper(DummyBusinessObject wrappedBO, BusinessObjectFactory factory)
				: base(wrappedBO, factory)
			{
			}

			public ConcreteWrapper(BusinessObjectFactory factory)
				: base(null, factory)
			{
			}

			public ZString Strength
			{
				get { return "GoodMayyyyte"; }
			}
		}
	}
}
