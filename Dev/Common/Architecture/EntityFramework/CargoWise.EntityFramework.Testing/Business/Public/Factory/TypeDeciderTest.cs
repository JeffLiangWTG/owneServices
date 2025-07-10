using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			Type type = TypeDecider.GetTypeForBinding(typeof(DummyConcreteBusinessObjectWithTypeDecider));
			AssertEquals("GetTypeForBinding", typeof(DummyBaseBusinessObject), type);
		}

		public void TestGetTypeForBinding_ClientSpecific()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				Type type = TypeDecider.GetTypeForBinding(typeof(DummyBaseBusinessObject));
				AssertEquals("GetTypeForBinding ClientSpecific", typeof(MockClientTypeForBindingDecidedBusinessObject), type);
			}
		}

		#region Classes for Cascading Test

		abstract class AbstractDummyBizO : DummyBaseBusinessObject
		{
			protected AbstractDummyBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region TypeDecider class

			[WTG.StaticAnalysis.Annotation.Immutable]
			public class AbstractDummyBizOTypeDecider : TypeDecider
			{
				public override Type GetTypeForNew()
				{
					return typeof(SecondAbstractDummyBizO);
				}

				public override Type GetTypeForBinding()
				{
					return null;
				}

				public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
				{
					return GetTypeForNew();
				}
			}

			#endregion

			public readonly static new TypeDecider TypeDecider = new AbstractDummyBizOTypeDecider();
		}

		abstract class SecondAbstractDummyBizO : DummyBaseBusinessObject
		{
			protected SecondAbstractDummyBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region TypeDecider class
			public class SecondAbstractDummyBizOTypeDecider : TypeDecider
			{
				public override Type GetTypeForNew()
				{
					return typeof(DummyBusinessObject);
				}

				public override Type GetTypeForBinding()
				{
					return null;
				}

				public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
				{
					return GetTypeForNew();
				}
			}

			#endregion

			public readonly static new TypeDecider TypeDecider = new SecondAbstractDummyBizOTypeDecider();
		}

		#endregion

		public void TestTypeDeciderCascades()
		{
			AssertEquals(typeof(DummyBusinessObject), Factory.New(typeof(AbstractDummyBizO)).GetType());
		}

		public void TestNoInfiniteLoopInTypeDecider()
		{
			var dummyBase = Factory.New<DummyBaseBusinessObject>();
			dummyBase.Z0_Description = "ABSTRACT";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			AssertExceptionThrown<NoConcreteTypeException>(() => factory2.Load<DummyAbstractBusinessObjectWithTypeDecider2>(dummyBase.PK));
		}

		public abstract class DummyAbstractBusinessObjectWithTypeDecider2 : DummyAbstractBusinessObjectWithTypeDecider
		{
			public readonly static new DummyAbstractTypeDecider TypeDecider = new DummyAbstractTypeDecider();

			public DummyAbstractBusinessObjectWithTypeDecider2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestLoadWithConcreteTypeDecider()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "notBase";

			DummyBaseBusinessObject dummyBase = Factory.New<DummyBaseBusinessObject>();
			dummyBase.Z0_Description = "BASE";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			BusinessObject loadedDummy = factory2.Load(typeof(DummyConcreteBusinessObjectWithTypeDecider), dummy.PK);
			BusinessObject loadedDummyBase = factory2.Load(typeof(DummyConcreteBusinessObjectWithTypeDecider), dummyBase.PK);

			AssertEquals(loadedDummy.GetType(), typeof(DummyBusinessObject));
			AssertEquals(loadedDummyBase.GetType(), typeof(DummyBaseBusinessObject));
		}

		public void TestNewWithConcreteTypeDecider()
		{
			DummyConcreteBusinessObjectWithTypeDecider.TypeDecider.MakeBase = false;
			BusinessObject dummy = Factory.New(typeof(DummyConcreteBusinessObjectWithTypeDecider));

			DummyConcreteBusinessObjectWithTypeDecider.TypeDecider.MakeBase = true;
			BusinessObject dummyBase = Factory.New(typeof(DummyConcreteBusinessObjectWithTypeDecider));

			AssertEquals(dummy.GetType(), typeof(DummyBusinessObject));
			AssertEquals(dummyBase.GetType(), typeof(DummyBaseBusinessObject));
		}

		[ExpectException(typeof(NoConcreteTypeException))]
		public void TestAbstractTypeDeciderThrowsExceptionOnNew()
		{
			Factory.New(typeof(DummyAbstractBusinessObjectWithTypeDecider));
		}

		public void TestAbstractTypeDeciderOnLoad()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObject loaded = factory2.Load(typeof(DummyAbstractBusinessObjectWithTypeDecider), dummy.PK);

			AssertEquals(loaded.GetType(), typeof(DummyBusinessObject));
		}

		public void TestLoadTypeDeciderOnClientOverride()
		{
			using (ObjectFactory.Get<IClientHookLoader>().OverrideClientHookForTest(new MockClientHookForTypeDeciderTest()))
			{
				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				Factory.Save();
				AssertEquals("Client specified business sub-class", typeof(MockClientTypeDecidedBusinessObject), dummy.GetType());

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				BusinessObject loaded = factory2.Load(typeof(DummyBusinessObject), dummy.PK);

				AssertEquals("Client specified loaded business sub-class", loaded.GetType(), typeof(MockClientLoadedTypeDecidedBusinessObject));
			}
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var typeDecider = new DummyDependantBusinessObject.DummyDependantTypeDecider();
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			var countrySetup = typeDeciderContextMock.Setup(k => k.Country);

			countrySetup.Returns("XX");
			var dummyBusinessObject = typeDecider.GetTypeForNew(typeDeciderContextMock.Object);
			AssertEquals(typeof(DummyDependantBusinessObject.DummyDependantBusinessObjectXXCountry), dummyBusinessObject);

			countrySetup.Returns("YY");
			var dummyBaseBusinessObject = typeDecider.GetTypeForNew(typeDeciderContextMock.Object);
			AssertEquals(typeof(DummyDependantBusinessObject), dummyBaseBusinessObject);
		}

		#region Test Type Substitution

		public void TestTypeSubstitution()
		{
			try
			{
				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				AssertEquals(typeof(DummyBusinessObject), dummy.GetType());
				Factory.Save();

				TypeDecider.AddSubstitution(typeof(DummyBusinessObject), typeof(DummyBizo1));

				AssertEquals(typeof(DummyBizo1), Factory.New<DummyBusinessObject>().GetType());
				AssertEquals(typeof(DummyBizo1), (new BusinessObjectFactory()).Load<DummyBusinessObject>(dummy.PK).GetType());

				AssertEquals(typeof(DummyBizo1), Factory.New<DummyBizo1>().GetType());
				AssertEquals(typeof(DummyBizo2), Factory.New<DummyBizo2>().GetType());

				TypeDecider.RemoveSubstitution(typeof(DummyBizo1));

				AssertEquals(typeof(DummyBizo1), Factory.New<DummyBusinessObject>().GetType());
				AssertEquals(typeof(DummyBizo1), (new BusinessObjectFactory()).Load<DummyBusinessObject>(dummy.PK).GetType());

				TypeDecider.RemoveSubstitution(typeof(DummyBusinessObject));

				AssertEquals(typeof(DummyBusinessObject), Factory.New<DummyBusinessObject>().GetType());
				AssertEquals(typeof(DummyBusinessObject), (new BusinessObjectFactory()).Load<DummyBusinessObject>(dummy.PK).GetType());
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(DummyBusinessObject));
			}
		}

		class DummyBizo1 : DummyBusinessObject
		{
			public DummyBizo1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyBizo2 : DummyBusinessObject
		{
			public DummyBizo2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion

		#region Mock Objects for Client Specific Type Deciding

		public class MockClientTypeDecidedBusinessObject : DummyBusinessObject
		{
			public MockClientTypeDecidedBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class MockClientLoadedTypeDecidedBusinessObject : DummyBusinessObject
		{
			public MockClientLoadedTypeDecidedBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public class MockClientTypeForBindingDecidedBusinessObject : DummyBusinessObject
		{
			public MockClientTypeForBindingDecidedBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class MockClientHookForTypeDeciderTest : IClientHook
		{
			public class MyTypeDecider : TypeDecider
			{
				public override Type GetTypeForNew()
				{
					return typeof(MockClientTypeDecidedBusinessObject);
				}

				public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
				{
					return typeof(MockClientLoadedTypeDecidedBusinessObject);
				}

				public override Type GetTypeForBinding()
				{
					return typeof(MockClientTypeForBindingDecidedBusinessObject);
				}
			}

			public ITypeDeciderDictionary ClientTypeDeciders
			{
				get
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(DummyBusinessObject), new MyTypeDecider());
					result.Add(typeof(DummyBaseBusinessObject), new MyTypeDecider());
					return new TypeDeciderDictionary(result);
				}
			}

			#region IClientHook Members

			bool IClientHook.IsInitialised
			{
				get { return IsInitialised; }
			}
			bool IsInitialised;

			void IClientHook.Initialise()
			{
				IsInitialised = true;
			}

			void IClientHook.Initialise(bool loggedIn)
			{
				IsInitialised = true;
			}

			void IClientHook.Uninitialise()
			{
				IsInitialised = false;
			}

			object IClientHook.GetTableSchema(string tableName)
			{
				return null;
			}

			bool IClientHook.HasCompanySpecificOverrides { get { return false; } }

			#endregion

			public string UniqueId
			{
				get { return string.Empty; }
			}

			public bool IsUpgrading { get; set; }
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
		}

		#endregion
	}
}
