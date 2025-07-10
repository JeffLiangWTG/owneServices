using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZWrappedPropertyInfoTest : TestCaseWithDummy
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor()
		{
			new ZWrappedPropertyInfo("test", Dummy, null);
		}

		public void TestAdditionalValidation()
		{
			var dummy = Factory.New<DummyWithProperty>();
			dummy.PropertyInfo.AdditionalValidation += new RunValidationInvoker(PropertyInfo_AdditionalValidation);
			PropertyInfo_AdditionalValidationWasCalled = false;
			dummy.Property = "Hello";
			AssertEquals(true, PropertyInfo_AdditionalValidationWasCalled);
		}

		public void TestHasHumanReadableName()
		{
			var wrappedPropertyInfo = new ZWrappedPropertyInfo("test", Dummy, x => null);
			AssertNoExceptionThrown(() => _ = wrappedPropertyInfo.HasHumanReadableName);
		}

		[ExpectNoExceptions]
		public void TestAddAllNotificationsFromThrowsNoExceptionWhenSourceIsInnerInfo()
		{
			var dummy = Factory.New<DummyWithProperty>();
			AssertEquals("Source and InnerInfo", ((ZWrappedPropertyInfo)dummy.PropertyInfo).InnerInfo, dummy.Info.PropertyInfo);

			dummy.Info.RaisePropertyError = true;
			dummy.Info.Validation.ValidateProperty();
			AssertHasErrors(dummy.Info.PropertyInfo);
			AssertHasErrors(dummy.PropertyInfo);

			dummy.PropertyInfo.AddAllNotificationsFrom(dummy.Info.PropertyInfo);
		}

		public void TestIsLoadedFlagKeptPerBizObj()
		{
			Factory.New<DummyWithProperty>(); // populate wrapped property name cache
			var dummy = Factory.New<DummyWithProperty>();
			var info = ((ZWrappedPropertyInfo)dummy.PropertyInfo);
			AssertEquals("Precondition: WrappedPropertyInfo is not loaded", false, info.IsLoaded);
			AssertNotNull("Load InnerInfo", info.InnerInfo);
			AssertEquals("IsLoaded is True because innerInfo is not null", true, info.IsLoaded);
			AssertEquals("IsLoaded is True because Flags are kept on BizObj", true, ((ZWrappedPropertyInfo)dummy.PropertyInfo).IsLoaded);

			var newInfo = new ZWrappedPropertyInfo(dummy.PropertyName, dummy, x => dummy.Info.PropertyInfo);
			AssertNotNull("New Info", newInfo);
			AssertEquals("IsLoaded is True because Flags are kept on BizObj", true, newInfo.IsLoaded);
		}

		bool PropertyInfo_AdditionalValidationWasCalled;
		void PropertyInfo_AdditionalValidation()
		{
			PropertyInfo_AdditionalValidationWasCalled = true;
		}

		class DummyWithoutProperty : DummyBusinessObject
		{
			public DummyWithoutProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZPropertyInfo PropertyInfo
			{
				get { return GetWrappedZPropertyInfo(PropertyName, x => Info.PropertyInfo); }
			}

			public virtual string PropertyName
			{
				get { return "Info+Property"; }
			}

			public AddInfo Info
			{
				get
				{
					if (fInfo == null)
					{
						fInfo = new AddInfo();
						RegisterListChangedCalledRefreshBinding(fInfo);
					}
					return fInfo;
				}
			}
			AddInfo fInfo;

			public class AddInfo : NonPersistentBusinessObject
			{
				public ZString Property
				{
					get { return fProperty; }
					set
					{
						fProperty = value;
						if (!IsValidationSuspended)
						{
							new AddInfoValidation(this).ValidateProperty();
						}
					}
				}
				ZString fProperty;

				public ZPropertyInfo PropertyInfo
				{
					get { return GetZPropertyInfo(nameof(Property)); }
				}

				public bool RaisePropertyError;

				public AddInfoValidation Validation
				{
					get { return new AddInfoValidation(this); }
				}
			}

			public class AddInfoValidation : ZValidation
			{
				public AddInfoValidation(AddInfo addInfo)
					: base(addInfo)
				{
					this.addInfo = addInfo;
				}
				readonly AddInfo addInfo;

				public AddInfo Parent
				{
					get { return addInfo; }
				}

				public override void ValidateAll()
				{
					throw new NotImplementedException();
				}

				public override Type AutoValidationType
				{
					get { return typeof(AddInfo); }
				}

				public void ValidateProperty()
				{
					ValidateCalculatedProperty(addInfo.PropertyInfo);
				}

				protected virtual void CheckProperty()
				{
					if (Parent.RaisePropertyError)
					{
						Parent.PropertyInfo.AddError("Error");
					}
				}
			}
		}

		class DummyWithProperty : DummyWithoutProperty
		{
			public DummyWithProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override string PropertyName
			{
				get { return "Property"; }
			}

			public ZString Property
			{
				get { return Info.Property + "EXTRA"; }
				set
				{
					Info.Property = value;
					DidSomeOtherThing = true;
				}
			}

			public bool DidSomeOtherThing;

			public new DummyWithPropertyValidation Validation
			{
				get { return new DummyWithPropertyValidation(this); }
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyWithPropertyValidation(this);
			}
		}

		class DummyWithPropertyValidation : DummyBizoValidation
		{
			public DummyWithPropertyValidation(DummyWithProperty bizO)
				: base(bizO)
			{
			}

			public new DummyWithProperty Parent
			{
				get { return base.Parent as DummyWithProperty; }
			}

			public void ValidateProperty()
			{
				ValidateCalculatedProperty(Parent.PropertyInfo);
			}

			protected virtual void CheckProperty()
			{
			}
		}

		public void TestValueCallsThroughToPropertyOnTheBusinessObject()
		{
			var dummy = Factory.New<DummyWithProperty>();
			dummy.PropertyInfo.Value = new ZString("ABC");
			Assert("Called setter on BO", dummy.DidSomeOtherThing);
			AssertEquals("Called getter on BO", "ABCEXTRA", dummy.PropertyInfo.Value);
		}

		public void TestValueCallsThroughToInnerInfoValueWhenThereIsNoMatchingPropertyOnTheBusinessObject()
		{
			var dummy = Factory.New<DummyWithoutProperty>();
			dummy.PropertyInfo.Value = new ZString("ABC");
			AssertEquals("Get/Set to inner info", "ABC", dummy.PropertyInfo.Value);
		}

		public void TestHumanReadableName()
		{
			var dummy = Factory.New<DummyWithoutProperty>();
			AssertEquals("No human readable name provided", "Property", dummy.PropertyInfo.HumanReadableName);

			dummy.Info.PropertyInfo.HumanReadableName = "Bob";
			AssertEquals("If we havnt been given a human readable name ourselves then get it from the inner property.", "Bob", dummy.PropertyInfo.HumanReadableName);

			dummy.PropertyInfo.HumanReadableName = "Fread";
			AssertEquals("But if we have been given our own human readable then use it instead.", "Fread", dummy.PropertyInfo.HumanReadableName);
		}

		public void TestConstructorNotPublic()
		{
			var ctors = typeof(ZWrappedPropertyInfo).GetConstructors();

			foreach (var ctor in ctors)
			{
				if (ctor.IsPublic)
				{
					Fail("There should be no public constructor - should always use GetWrappedZPropertyInfo() - calling a public constructor has unexpected results.");
				}
			}
			Assert("all OK - no public constructors found", true);
		}

		public void TestZWrappedPropertyInfoWithNullInnerInfo()
		{
			var dummy = Factory.New<DummyWithoutProperty>();
			var info = new ZWrappedPropertyInfo("RelatedDummy+Z0_AnotherDate", dummy, x => null);

			AssertNotNull(info);
			AssertNull("InnerInfo", info.InnerInfo);
			AssertEquals("Null InnerInfo.HasChanges should be false", false, info.HasChanges);

			AssertEquals("IsBusinessObjectValidationSuspended()", false, info.IsBusinessObjectValidationSuspended());
			// Notifications should not blow up with the null InnerInfo
			AssertNotNull("Notifications", info.Notifications);
			AssertNull("Value", info.Value);

			info.Value = ZDateTime.Now;

			AssertNull("Assigned value is null", info.Value);
		}

		public void TestRecursiveWarning()
		{
			var dummy = Factory.New<DummyWithRecursiveZWrapperPropInfo>();

			dummy.Property1Info.ClearAllNotifications();
			Assert("This is no longer an error - recursive ZWrappedPropertyInfo is OK", String.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		class DummyWithRecursiveZWrapperPropInfo : DummyBusinessObject
		{
			public DummyWithRecursiveZWrapperPropInfo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Property1 { get; set; }

			public ZPropertyInfo Property1Info
			{
				get { return GetWrappedZPropertyInfo(nameof(Property1), x => Property2Info); }
			}

			public ZString Property2 { get; set; }

			public ZPropertyInfo Property2Info
			{
				get { return GetWrappedZPropertyInfo(nameof(Property2), x => Property3Info); }
			}

			public ZString Property3 { get; set; }

			public ZPropertyInfo Property3Info => GetZPropertyInfo(nameof(Property3));
		}
	}
}
