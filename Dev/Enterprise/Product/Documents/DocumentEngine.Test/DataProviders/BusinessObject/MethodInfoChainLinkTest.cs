using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class MethodInfoChainLinkTest : TestCaseWithFactory
	{
		public void TestIndexerForDocumentWrapperCollection()
		{
			var infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("Desks").GetGetMethod(), "Last");
			object actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals(RichardsDesk, actualResult);
		}

		public void TestCallingFormatWithParameters()
		{
			var infoLink = new MethodInfoChainLink(typeof(IBODocDataProviderCollection).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{EmployeeName}"), new ZString("Colon"), ZString.Empty, ZString.Empty, ZInt.Zero });
			object actualResult = infoLink.ReflectOutObject(MainOffice.Desks, MainOffice);
			AssertEquals("Ben : Brett : Richard", actualResult);
		}

		public void TestCallingFormatForIBusinessObjectCollectionWithParameters()
		{
			var infoLink = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper).GetMethod("Format", new Type[] { typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZString), typeof(ZInt) }), new object[] { new ZString("{Z0_Description}"), new ZString("Colon"), ZString.Empty, ZString.Empty, ZInt.Zero });
			object actualResult = infoLink.ReflectOutObject(MainOffice.Dummies, MainOffice);
			AssertEquals("Bob : Jack", actualResult);
		}

		public void TestCollectionWithNoIndexerGetsFirstRowIfExists()
		{
			var mainBO = new BusinessObjectForTesting("Main");

			var infoLink = new MethodInfoChainLink(typeof(BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod());
			object actualResult = infoLink.ReflectOutObject(mainBO.CollectionWithNoFactoryOnlyConstructor, mainBO);
			AssertEquals("Child 0", actualResult.ToString());

			mainBO.CollectionWithNoFactoryOnlyConstructor.RemoveAndDeleteAll();
			actualResult = infoLink.ReflectOutObject(mainBO.CollectionWithNoFactoryOnlyConstructor, mainBO);
			AssertEquals(null, actualResult);
		}

		public void TestGetCurrentRowFromIndexerOnDocumentWrapperCollection()
		{
			var infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("Desks").GetGetMethod(), "2");
			object actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals(BrettsDesk, actualResult);

			infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("Desks").GetGetMethod(), "4");
			actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals(null, actualResult);
		}

		public void TestGetCurrentRowFromIndexerOnBusinessObjectCollection()
		{
			var mainBO = new BusinessObjectForTesting("Main");

			var infoLink = new MethodInfoChainLink(typeof(BusinessObjectForTesting).GetProperty("CollectionWithNoFactoryOnlyConstructor").GetGetMethod(), "2");
			object actualResult = infoLink.ReflectOutObject(mainBO, mainBO);
			AssertEquals("Child 1", actualResult.ToString());

			infoLink = new MethodInfoChainLink(typeof(BusinessObjectForTesting).GetProperty("CollectionWithNoFactoryOnlyConstructor").GetGetMethod(), "4");
			actualResult = infoLink.ReflectOutObject(mainBO, mainBO);
			AssertEquals(null, actualResult);
		}

		public void TestGetCurrentRowFromIndexerOnDocumentWrapperCollectionDoesntThrowExceptionOnZero()
		{
			var infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("Desks").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals(null, actualResult);
		}

		public void TestGetCurrentRowFromIndexerOnBusinessObjectCollectionDoesntThrowExceptionOnZero()
		{
			var mainBO = new BusinessObjectForTesting("Main");

			var infoLink = new MethodInfoChainLink(typeof(BusinessObjectForTesting).GetProperty("CollectionWithNoFactoryOnlyConstructor").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(mainBO, mainBO);
			AssertEquals(null, actualResult);
		}

		public void TestSystemCreateUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "KNC";

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_SystemCreateUser = staff.GS_Code;

			Factory.Save();

			var infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("SystemCreateUser").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(staff, actualResult);

			infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("Z0_SystemCreateUser").GetGetMethod(), "0");
			actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(staff.GS_Code, actualResult);
		}

		public void TestDifferentSystemCreateUserReturnTypesWhenEmpty()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("List_SystemCreateUser").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(null, actualResult);
		}

		public void TestSystemCreateUserDefaultsToEnvCurrentUserWhenEmpty()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("SystemCreateUser").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(Env.CurrentUser, actualResult);
		}

		public void TestZ0_SystemCreateUserDefaultsToEnvCurrentUserWhenEmpty()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("Z0_SystemCreateUser").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(Env.CurrentUser.Initials, actualResult);
		}

		[TestDate(2018, 7, 3)]
		public void TestSystemCreateTimeUtcDefaultsNowWhenEmpty()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var infoLink = new MethodInfoChainLink(typeof(DummyWithWorkflow).GetProperty("Z0_SystemCreateTimeUtc").GetGetMethod(), "0");
			object actualResult = infoLink.ReflectOutObject(dummy, dummy);
			AssertEquals(ZDateTime.Now, actualResult);
		}

		public void TestBasicInvoke()
		{
			var infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("OfficeName").GetGetMethod());
			object actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals("Main Office", actualResult);
		}

		public void TestReflectOutObject_InvokeReturnNull_CauseCollectionNullReference_Issue00849372()
		{
			var infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("NullDesks").GetGetMethod(), "2");
			AssertNoExceptionThrown("Expect not throw NullReference Exception.", () => infoLink.ReflectOutObject(MainOffice, MainOffice));
			AssertEquals(null, infoLink.ReflectOutObject(MainOffice, MainOffice));

			infoLink = new MethodInfoChainLink(typeof(OfficeWrapper).GetProperty("Desks").GetGetMethod(), "4");
			var actualResult = infoLink.ReflectOutObject(MainOffice, MainOffice);
			AssertEquals(null, actualResult);
		}

		public void TestReflectOutObject_ThrowDetailedTargetInvocationException()
		{
			var testClassForThrowingTargetInvocationException = new TestClassForThrowingTargetInvocationException();
			var methodInfo = testClassForThrowingTargetInvocationException.GetType().GetMethod("ThrowingMethod");
			var infoLink = new MethodInfoChainLink(methodInfo);
			var exMsg = string.Format(CultureInfo.InvariantCulture,
					"Error reflecting property [{0}] from parent type [{1}] : {2}", methodInfo.Name, methodInfo.ReflectedType.FullName,
					new NotImplementedException().Message);
			AssertExceptionThrown(typeof(InvalidOperationException), exMsg, () => infoLink.ReflectOutObject(testClassForThrowingTargetInvocationException, testClassForThrowingTargetInvocationException));

			var methodInfo2 = testClassForThrowingTargetInvocationException.GetType().GetMethod("ThrowingMethodAccessException");
			var infoLink2 = new MethodInfoChainLink(methodInfo2);
			var exMsg2 = string.Format(CultureInfo.InvariantCulture,
					"Error reflecting property [{0}] from parent type [{1}] : {2}", methodInfo2.Name, methodInfo2.ReflectedType.FullName,
					new NotImplementedException("info").Message);
			AssertExceptionThrown(typeof(InvalidOperationException), exMsg2, () => infoLink2.ReflectOutObject(testClassForThrowingTargetInvocationException, testClassForThrowingTargetInvocationException));
		}

		class TestClassForThrowingTargetInvocationException
		{
			public void ThrowingMethod()
			{
				throw new NotImplementedException();
			}

			public void ThrowingMethodAccessException()
			{
				throw new MethodAccessException("info");
			}
		}

		public void TestReflectOutObject_HandlesDocEnginExceptions()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var first = collection.AddNew();
			var second = collection.AddNew();
			first.Z0_Number = 1;
			second.Z0_Number = 2;

			var helper = new BODocDataProviderCollectionHelper(collection);

			var infoChainLinkWithBadMacro = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper)
				.GetMethod("Find", new[] { typeof(ZString) }), new object[] { new ZString("{Z0_Number} = 1") });

			AssertExceptionThrown<BODocDataProviderCollectionFindException>(
				"When an exception is thrown we want to check if it's the type of exception that's reported only to the user so ReportErrorManager knows how to deal with it",
				() => infoChainLinkWithBadMacro.ReflectOutObject(helper, helper));

			var infoChainLinkWithGoodMacro = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper)
				.GetMethod("Find", new[] { typeof(ZString) }), new object[] { new ZString("{Z0_Number} == 1") });
			var resultObject = infoChainLinkWithGoodMacro.ReflectOutObject(helper, helper);

			AssertEquals("Now that the macro has been corrected, expected to find bizObj", first, resultObject);

			infoChainLinkWithGoodMacro = new MethodInfoChainLink(typeof(BODocDataProviderCollectionHelper)
				.GetMethod("Find", new[] { typeof(ZString) }), new object[] { new ZString("{Z0_Number} == 2") });
			resultObject = infoChainLinkWithGoodMacro.ReflectOutObject(helper, helper);

			AssertEquals(second, resultObject);
		}

		public void TestReflectOutObjectCore_NullReferenceException()
		{
			var methodInfo = typeof(BODocDataProviderCollectionHelper).GetMethod("Format");
			var infoLink = new MethodInfoChainLink(methodInfo);

			const string exceptionMessage = "Error reflecting property [Format] from parent type [Enterprise.DocumentEngine.BODocDataProviderCollectionHelper] : ";
			AssertExceptionThrown<InvalidOperationException>("Should throw an InvalidOperationException", exceptionMessage, () => infoLink.ReflectOutObject(null, null));
		}

		public void TestMethodInfoChainLinkConstructor_NullReferenceException()
		{
			var methodInfo = typeof(DummyWrapperwithStaticProperty).GetMethod("get_DummyStaticProperty");

			AssertNoExceptionThrown("Expect not throw NullReference Exception.", () => new MethodInfoChainLink(methodInfo));
		}
		#region Implementation

		#region MainOffice

		OfficeWrapper MainOffice
		{
			get
			{
				if (mainOffice == null)
				{
					mainOffice = new OfficeWrapper(Factory, "Main Office");
					mainOffice.Desks.Add(BensDesk);
					mainOffice.Desks.Add(BrettsDesk);
					mainOffice.Desks.Add(RichardsDesk);
					mainOffice.Dummies.Add(DummyBob);
					mainOffice.Dummies.Add(DummyJack);
				}
				return mainOffice;
			}
		}
		OfficeWrapper mainOffice;

		#endregion

		#region BensDesk

		DeskWrapper BensDesk
		{
			get { return bensDesk ?? (bensDesk = new DeskWrapper(Factory, "Ben")); }
		}
		DeskWrapper bensDesk;

		#endregion

		#region BrettsDesk

		DeskWrapper BrettsDesk
		{
			get { return brettsDesk ?? (brettsDesk = new DeskWrapper(Factory, "Brett")); }
		}
		DeskWrapper brettsDesk;

		#endregion

		#region RichardsDesk

		DeskWrapper RichardsDesk
		{
			get { return richardsDesk ?? (richardsDesk = new DeskWrapper(Factory, "Richard")); }
		}
		DeskWrapper richardsDesk;

		#endregion

		DummyBusinessObject DummyBob
		{
			get
			{
				if (dummyBob == null)
				{
					dummyBob = Factory.New<DummyBusinessObject>();
					dummyBob.Z0_Description = "Bob";
				}
				return dummyBob;
			}
		}
		DummyBusinessObject dummyBob;

		DummyBusinessObject DummyJack
		{
			get
			{
				if (dummyJack == null)
				{
					dummyJack = Factory.New<DummyBusinessObject>();
					dummyJack.Z0_Description = "Jack";
				}
				return dummyJack;
			}
		}
		DummyBusinessObject dummyJack;

		class OfficeWrapper : DocumentWrapper
		{
			public OfficeWrapper(BusinessObjectFactory factory, ZString officeName)
				: base(null, factory)
			{
				this.officeName = officeName;
			}

			public DeskWrapperCollection Desks
			{
				get { return desks ?? (desks = new DeskWrapperCollection(Factory)); }
			}
			DeskWrapperCollection desks;

			public DeskWrapperCollection NullDesks
			{
				get
				{
					nullDesk = null;
					return nullDesk;
				}
			}
			DeskWrapperCollection nullDesk;

			public DummyBusinessObjectCollection Dummies
			{
				get { return dummies ?? (dummies = new DummyBusinessObjectCollection(Factory)); }
			}
			DummyBusinessObjectCollection dummies;

			public ZString OfficeName
			{
				get { return officeName; }
			}
			readonly ZString officeName;
		}

		[DefaultField("EmployeeName")]
		class DeskWrapper : DocumentWrapper
		{
			public DeskWrapper(BusinessObjectFactory factory, ZString employeeName)
				: base(null, factory)
			{
				this.employeeName = employeeName;
			}

			public ZString EmployeeName
			{
				get { return employeeName; }
			}
			readonly ZString employeeName;
		}

		class DeskWrapperCollection : DocumentWrapperCollection<DeskWrapper>
		{
			public DeskWrapperCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion
	}
}
