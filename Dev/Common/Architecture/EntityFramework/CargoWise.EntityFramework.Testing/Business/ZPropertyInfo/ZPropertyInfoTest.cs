using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoTest : TestCaseWithDummyForValidationTesting
	{
		public void TestINotificationType()
		{
			ZPropertyInfo info = Dummy.Z0_DescriptionInfo;
			AssertEquals(null, info.GetHighestSeverityNotificationType());

			info.AddWarning("warn");
			AssertEquals(NotificationType.Warning, info.GetHighestSeverityNotificationType());

			info.AddMessageError("messerr");
			AssertEquals(NotificationType.MessageError, info.GetHighestSeverityNotificationType());

			info.AddError("err");
			AssertEquals(NotificationType.Error, info.GetHighestSeverityNotificationType());
		}

		public void TestSuspendOnValueChanged()
		{
			var info = Dummy.Z0_DescriptionInfo;

			var calledOnValueChanged = false;
			info.ValueChanged += (s, e) => calledOnValueChanged = true;

			using (info.SuspendOnValueChanged())
			{
				info.Value = new ZString("some value");
			}

			AssertEquals("OnValueChanged wasn't called due to suspension", false, calledOnValueChanged);

			info.Value = new ZString("other value");
			AssertEquals("OnValueChanged was called after suspension was removed", true, calledOnValueChanged);
		}

		#region TestPersistentValue

		class DummyWithBits : DummyBusinessObject
		{
			public DummyWithBits(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_Description
			{
				get { return "moo"; }
				set { base.Z0_Description = value; }
			}
		}

		public void TestPersistentValue()
		{
			DummyWithBits dummyWithBits = Factory.New<DummyWithBits>();

			dummyWithBits.Z0_Description = "row value";
			AssertEquals("moo", dummyWithBits.Z0_DescriptionInfo.Value);
			AssertEquals("row value", dummyWithBits.Z0_DescriptionInfo.PersistentValue);

			AssertEquals(dummyWithBits.Z0_CalculatedInfo.Value, dummyWithBits.Z0_CalculatedInfo.PersistentValue);
		}

		public void TestPersistentValueWithZBool()
		{
			DummyWithBits dummyWithBits = Factory.New<DummyWithBits>();
			AssertEquals("Z0_BoolInfo.PersistentValue should be a ZBool (the row datatype will be string).", true, dummyWithBits.Z0_BoolInfo.PersistentValue is ZBool);
		}

		#endregion

		#region Test HasSetter

		class ChildOfDummy : DummyBusinessObject
		{
			public ChildOfDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IMayHaveASetter

			[ReadOnly(false)]
			public virtual ZString IMayHaveASetter
			{
				get { return "Foo!"; }
			}

			public ZPropertyInfo IMayHaveASetterInfo
			{
				get { return GetZPropertyInfo(nameof(IMayHaveASetter)); }
			}

			#endregion
		}

		class GrandChildOfDummy : ChildOfDummy
		{
			public GrandChildOfDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_Description
			{
				get { return base.Z0_Description; }
				set
				{
					// can't call base.base!
				}
			}

			public new ZString IMayHaveASetter
			{
				set
				{
				}
			}
		}

		public void TestHasSetter()
		{
			AssertEquals(true, Dummy.Z0_DescriptionInfo.HasSetter);

			ChildOfDummy childOfDummy = Factory.New<ChildOfDummy>();
			AssertEquals(true, childOfDummy.Z0_DescriptionInfo.HasSetter);
			AssertEquals(false, childOfDummy.IMayHaveASetterInfo.HasSetter);

			GrandChildOfDummy grandChildOfDummy = Factory.New<GrandChildOfDummy>();
			AssertEquals(true, grandChildOfDummy.Z0_DescriptionInfo.HasSetter);
			AssertEquals(true, grandChildOfDummy.IMayHaveASetterInfo.HasSetter);
		}

		#endregion

		public void TestValueChanged_UnhookRemovesFromStorage()
		{
			Dummy.Z0_AnotherNumber = 0;
			Dummy.Z0_AnotherNumberInfo.ValueChanged += new EventHandler(OnValueChanged);
			Dummy.Z0_AnotherNumberInfo.ValueChanged += new EventHandler(OnValueChangedAgain);
			AssertEquals(1, Factory.PropertyInfoStorage.ValueChangedDictionary.Count);

			Dummy.Z0_AnotherNumberInfo.ValueChanged -= new EventHandler(OnValueChanged);
			Dummy.Z0_AnotherNumber = 10;
			Assert(!fValueChangedFired);
			Assert(fValueChangedFiredAgain);
			fValueChangedFired = false;
			fValueChangedFiredAgain = false;
			AssertEquals(1, Factory.PropertyInfoStorage.ValueChangedDictionary.Count);

			Dummy.Z0_AnotherNumberInfo.ValueChanged -= new EventHandler(OnValueChangedAgain);
			Dummy.Z0_AnotherNumber = 11;
			Assert(!fValueChangedFired);
			Assert(!fValueChangedFiredAgain);
			AssertEquals(0, Factory.PropertyInfoStorage.ValueChangedDictionary.Count);
		}

		public void TestRefreshBindingProvidesOldValueAndInfo()
		{
			Dummy.Z0_AnotherNumber = 3;
			Dummy.Z0_AnotherNumberInfo.ValueChanged += new EventHandler(Z0_AnotherNumberInfo_ValueChanged);
			Dummy.Z0_AnotherNumber = 6;
		}

		void Z0_AnotherNumberInfo_ValueChanged(object sender, EventArgs e)
		{
			ValueChangedEventArgs ve = (ValueChangedEventArgs)e;
			AssertEquals(3, ve.OldValue);
			AssertEquals(Dummy.Z0_AnotherNumberInfo, ve.Info);
		}

		public void TestIsPersistent()
		{
			Assert("Field in DB", Dummy.Z0_AnotherDateInfo.IsPersistent);
			Assert("Not field in DB", !Dummy.Z0_CalculatedInfo.IsPersistent);
		}

		public void TestClearValue()
		{
			Info.Value = new ZString("Hello");
			Info.ClearValue();
			AssertEquals("Value cleared", ZString.Empty, Info.Value);
		}

		public void TestIsBusinessObjectValidationSuspended()
		{
			DummyBusinessObject relatedDummy = Factory.New<DummyBusinessObject>();
			Dummy.Z0_Guid = relatedDummy.PK;

			using (Dummy.GetValidationSuspender())
			using (relatedDummy.GetValidationSuspender())
			{
				AssertEquals("IsBusinessObjectValidationSuspended", true, Dummy.ZPropertyInfoHash[DummyBizoSchema.Z0_Code.Name].IsBusinessObjectValidationSuspended());
				AssertEquals("IsBusinessObjectValidationSuspended on wrapped", true, Dummy.ZPropertyInfoHash["RelatedDummy+" + DummyBizoSchema.Z0_Code.Name].IsBusinessObjectValidationSuspended());
			}

			AssertEquals("not IsBusinessObjectValidationSuspended", false, Dummy.ZPropertyInfoHash[DummyBizoSchema.Z0_Code.Name].IsBusinessObjectValidationSuspended());
			AssertEquals("not IsBusinessObjectValidationSuspended on wrapped", false, Dummy.ZPropertyInfoHash["RelatedDummy+" + DummyBizoSchema.Z0_Code.Name].IsBusinessObjectValidationSuspended());
		}

		public void TestClearErrors()
		{
			Info.AddError("Bad");
			Info.AddMessageError("Bad");
			Info.AddWarning("Bad");

			AssertEquals(true, Info.HasNotifications());

			Info.ClearAllNotifications();
			AssertEquals(false, Info.HasNotifications());
		}

		public void TestReplaceNotification()
		{
			Info.AddError("Bad");
			Info.AddMessageError("Bad");
			Info.AddWarning("Bad");

			AssertEquals(true, Info.HasNotifications());
			AssertEquals(3, Info.Notifications.Count());
			AssertEquals(true, Info.HasWarning("Bad"));
			AssertEquals(false, Info.HasError("Replaced"));

			var warning = Info.Notifications.GetWarnings().GetFirst();
			Info.ReplaceNotification(warning, NotificationType.Error, "Replaced");
			AssertEquals(true, Info.HasNotifications());
			AssertEquals(3, Info.Notifications.Count());
			AssertEquals(false, Info.HasWarning("Bad"));
			AssertEquals(true, Info.HasError("Replaced"));
		}

		public void TestAddError()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddError("Something bad!");
			AssertEquals(true, Info.HasErrors());
			AssertEquals(false, Info.HasWarnings());
			AssertEquals(false, Info.HasMessageErrors());
			AssertEquals(true, Info.HasNotifications());
		}

		public void TestAddWarning()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddWarning("Something bad!");
			AssertEquals(false, Info.HasErrors());
			AssertEquals(true, Info.HasWarnings());
			AssertEquals(false, Info.HasMessageErrors());
			AssertEquals(true, Info.HasNotifications());
		}

		public void TestAddMessageError()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddMessageError("Something bad!");
			AssertEquals(false, Info.HasErrors());
			AssertEquals(false, Info.HasWarnings());
			AssertEquals(true, Info.HasMessageErrors());
			AssertEquals(true, Info.HasNotifications());
		}

		public void TestErrorsProperty()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddError("error");
			AssertEquals(Info.GetErrors().GetFirstMessage(), "error");

			Info.ClearAllNotifications();
			AssertEquals(0, Info.GetErrors().Count());
		}

		public void TestWarningsProperty()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddWarning("warnings");
			AssertEquals(Info.GetWarnings().GetFirstMessage(), "warnings");

			Info.ClearAllNotifications();
			AssertEquals(0, Info.GetWarnings().Count());
		}

		public void TestMessageErrorsProperty()
		{
			AssertEquals(false, Info.HasNotifications());

			Info.AddMessageError("messageerror");
			AssertEquals(Info.GetMessageErrors().GetFirstMessage(), "messageerror");

			Info.ClearAllNotifications();
			AssertEquals(0, Info.GetMessageErrors().Count());
		}

		public void TestAddNotificationError()
		{
			Assert(!Info.HasNotifications());
			Info.AddNotification(NotificationType.Error, "test");
			Assert(Info.HasErrors());
			AssertEquals("test", Info.GetErrors().GetFirstMessage());
			Assert(!Info.HasWarnings());
			Assert(!Info.HasMessageErrors());
		}

		public void TestAddNotificationMessageError()
		{
			Assert(!Info.HasNotifications());
			Info.AddNotification(NotificationType.MessageError, "test");
			Assert(Info.HasMessageErrors());
			AssertEquals("test", Info.GetMessageErrors().GetFirstMessage());
			Assert(!Info.HasWarnings());
			Assert(!Info.HasErrors());
		}

		public void TestAddNotificationWarning()
		{
			Assert(!Info.HasNotifications());
			Info.AddNotification(NotificationType.Warning, "test");
			Assert(Info.HasWarnings());
			AssertEquals("test", Info.GetWarnings().GetFirstMessage());
			Assert(!Info.HasErrors());
			Assert(!Info.HasMessageErrors());
		}

		public void TestAddNotificationWithInvalidParamaeters()
		{
			try
			{
				Assert(!Info.HasNotifications());
				Info.AddNotification(NotificationType.Warning, "   ");
				AssertEquals("Cannot pass null/empty notification message for property 'Z0_Description'. Additional info:'Description-BizObj:DummyBizo'", ErrorReporter.LastMessageReported);
				Info.AddNotification(NotificationType.Error, string.Empty);
				AssertEquals("Cannot pass null/empty notification message for property 'Z0_Description'. Additional info:'Description-BizObj:DummyBizo'", ErrorReporter.LastMessageReported);
				Info.AddNotification(NotificationType.Information, null);
				AssertEquals("Cannot pass null/empty notification message for property 'Z0_Description'. Additional info:'Description-BizObj:DummyBizo'", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestUsesPropertyNotificationType()
		{
			DummyWithDependentsBusinessObject dummy = Factory.New<DummyWithDependentsBusinessObject>();
			DummyDependantBusinessObject dependent = dummy.Dependents.AddNew();
			dependent.ZD1_Code = "error";
			AssertEquals(
				"PropertyNotification notification type preserved when traversing through registered editable children",
				true, dummy.NotificationsIncludingChildren.GetFirst() is PropertyNotification);
		}

		public void TestPropertyType()
		{
			DummyBusinessObject bO = Factory.New<DummyBusinessObject>();
			AssertEquals(typeof(ZDateTimeOffset), bO.Z0_DateTimeOffsetInfo.PropertyType);
			AssertEquals(typeof(ZDateTime), bO.Z0_AnotherDateInfo.PropertyType);
			AssertEquals(typeof(ZDate), bO.Z0_DateOnlyInfo.PropertyType);
			AssertEquals(typeof(ZString), bO.Z0_CodeInfo.PropertyType);
			AssertEquals(typeof(ZGuid), bO.Z0_GuidInfo.PropertyType);
			AssertEquals(typeof(ZGeography), bO.Z0_GeographyInfo.PropertyType);
		}

		public void TestAddAllNotificationsFromError()
		{
			const string TestError = "Test Error";
			DummyBusinessObject bO1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bO2 = Factory.New<DummyBusinessObject>();

			using (bO1.SuspendValidationTesting())
			using (bO2.SuspendValidationTesting())
			{
				bO1.Z0_CodeInfo.ClearAllNotifications();
				Assert("PreCondition 1 : No notifications", !bO1.Z0_CodeInfo.HasNotifications());
				Assert("PreCondition 2 : No notifications", !bO2.Z0_CodeInfo.HasNotifications());
				bO1.Z0_CodeInfo.AddError(TestError);
				AssertEquals("PreCondition 3 : 1 notifications", 1, bO1.Z0_CodeInfo.GetErrors().Count());
				bO2.Z0_CodeInfo.AddAllNotificationsFrom(bO1.Z0_CodeInfo);
				AssertEquals("Error Count", 1, bO2.Z0_CodeInfo.GetErrors().Count());
				AssertEquals("BO2.HasErrors", true, bO2.HasErrors());
				AssertEquals("Copied text", TestError, bO2.Z0_CodeInfo.GetErrors().GetFirstMessage());
			}
		}

		public void TestAddAllNotificationsFromMessageError()
		{
			const string TestError = "Test Error";
			DummyBusinessObject bO1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bO2 = Factory.New<DummyBusinessObject>();
			using (bO1.SuspendValidationTesting())
			using (bO2.SuspendValidationTesting())
			{
				bO1.Z0_CodeInfo.ClearAllNotifications();
				Assert("PreCondition 1 : No notifications", !bO1.Z0_CodeInfo.HasNotifications());
				Assert("PreCondition 2 : No notifications", !bO2.Z0_CodeInfo.HasNotifications());
				bO1.Z0_CodeInfo.AddMessageError(TestError);
				AssertEquals("PreCondition 3 : 1 notifications", 1, bO1.Z0_CodeInfo.GetMessageErrors().Count());
				bO2.Z0_CodeInfo.AddAllNotificationsFrom(bO1.Z0_CodeInfo);
				AssertEquals("MessageError Count", 1, bO2.Z0_CodeInfo.GetMessageErrors().Count());
				AssertEquals("BO2.HasMessageErrors", true, bO2.HasMessageErrors());
				AssertEquals("Copied text", TestError, bO2.Z0_CodeInfo.GetMessageErrors().GetFirstMessage());
			}
		}

		public void TestAddAllNotificationsFromWarning()
		{
			const string TestError = "Test Error";
			DummyBusinessObject bO1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bO2 = Factory.New<DummyBusinessObject>();

			using (bO1.SuspendValidationTesting())
			using (bO2.SuspendValidationTesting())
			{
				bO1.Z0_CodeInfo.ClearAllNotifications();
				Assert("PreCondition 1 : No notifications", !bO1.Z0_CodeInfo.HasNotifications());
				Assert("PreCondition 2 : No notifications", !bO2.Z0_CodeInfo.HasNotifications());
				bO1.Z0_CodeInfo.AddWarning(TestError);
				AssertEquals("PreCondition 3 : 1 notifications", 1, bO1.Z0_CodeInfo.GetWarnings().GetUniqueMessageList().Length);
				bO2.Z0_CodeInfo.AddAllNotificationsFrom(bO1.Z0_CodeInfo);
				AssertEquals("Notifications Warnings Count", 1, bO2.Z0_CodeInfo.GetWarnings().GetUniqueMessageList().Length);
				AssertEquals("BizO.HasWarnings", true, bO2.HasWarnings());
				AssertEquals("Copied text", TestError, bO2.Z0_CodeInfo.GetWarnings().GetFirstMessage());
			}
		}

		public void TestValueChangedEvent()
		{
			Dummy.Z0_Number = 1;

			Dummy.Z0_NumberInfo.ValueChanged += new EventHandler(OnValueChanged);
			Dummy.Z0_Number = 2;
			AssertEquals("ValueChanged fired", true, fValueChangedFired);
			Dummy.Z0_NumberInfo.ValueChanged -= new EventHandler(OnValueChanged);
			fValueChangedFired = false;
			Dummy.Z0_Number = 3;
			AssertEquals("ValueChanged fired", false, fValueChangedFired);
		}

		public void TestConcurrencyMergedEvent()
		{
			NotificationHandler.Instance = new ZGUINotificationHandler();
			Dummy.Z0_Number = 1;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var dummyFactory1 = factory1.Load<DummyBusinessObject>(Dummy.PK);
			var dummyFactory2 = factory2.Load<DummyBusinessObject>(Dummy.PK);

			dummyFactory2.Z0_NumberInfo.ConcurrencyMerged += OnConcurrencyMerged;

			dummyFactory1.Z0_Number = 2;
			dummyFactory2.Z0_Number = 3;

			factory1.Save();

			try
			{
				factory2.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				Assert(true);
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				AssertEquals("ConcurrencyMerged fired", true, fConcurrencyMergedFired);
			}
		}

		public void TestOnElementChangedCalledWhenNotificationsChange()
		{
			bool listChangedFired = false;
			((IBindingList)Dummy).ListChanged += (sender, args) => listChangedFired = true;
			AssertEquals(false, listChangedFired);

			Dummy.Z0_CodeInfo.AddError("Message");
			AssertEquals("OnElementChanged fired when adding a notification so the gui refreshes", true, listChangedFired);
			listChangedFired = false;

			Dummy.Z0_CodeInfo.ClearAllNotifications();
			AssertEquals("OnElementChanged fired when clearing all notifications so the gui refreshes", true, listChangedFired);
		}

		public void TestAdditionalValidationEvents()
		{
			Dummy.Z0_Number = 1;
			Dummy.Z0_NumberInfo.AdditionalValidation += new RunValidationInvoker(Z0_NumberInfo_AdditionalValidation);   // Single Hook
			AssertEquals(0, additionalValidationFired);
			Dummy.Z0_Number = 2;
			AssertEquals(1, additionalValidationFired);
			Dummy.Z0_NumberInfo.AdditionalValidation += new RunValidationInvoker(Z0_NumberInfo_AdditionalValidation);   // Double hook and fire
			Dummy.Z0_Number = 3;
			AssertEquals(3, additionalValidationFired);
			Dummy.Z0_NumberInfo.AdditionalValidation -= new RunValidationInvoker(Z0_NumberInfo_AdditionalValidation);   // Remove one
			Dummy.Z0_NumberInfo.AdditionalValidation -= new RunValidationInvoker(Z0_NumberInfo_AdditionalValidation);   // and the other
			Dummy.Z0_Number = 4;
			AssertEquals(3, additionalValidationFired);                                                                 // No extra event fired
		}
		int additionalValidationFired;

		void Z0_NumberInfo_AdditionalValidation()
		{
			additionalValidationFired++;
		}

		bool fValueChangedFired;
		void OnValueChanged(object sender, EventArgs e)
		{
			fValueChangedFired = true;
		}

		bool fValueChangedFiredAgain;
		void OnValueChangedAgain(object sender, EventArgs e)
		{
			fValueChangedFiredAgain = true;
		}

		bool fConcurrencyMergedFired;

		void OnConcurrencyMerged(object sender, EventArgs e)
		{
			fConcurrencyMergedFired = true;
		}

		public void TestOriginalValueAndHasChanges()
		{
			Dummy.Z0_Bool = true;
			Dummy.Z0_Date = new ZDateTime(1982, 10, 6);
			Dummy.Z0_DateTimeOffset = new ZDateTimeOffset(1982, 10, 6, 1, 2, 3, TimeSpan.FromHours(11));
			Dummy.Z0_Geography = new ZGeography("-121 48");

			AssertEquals("Current value", ZBool.True, Dummy.Z0_Bool);
			AssertEquals("Original value", ZBool.True, Dummy.Z0_BoolInfo.OriginalValue);
			Assert("No Changes", !Dummy.Z0_BoolInfo.HasChanges);
			Factory.Save();

			AssertEquals("Current value", ZBool.True, Dummy.Z0_Bool);
			AssertEquals("Original value", ZBool.True, Dummy.Z0_BoolInfo.OriginalValue);
			Assert("No Changes", !Dummy.Z0_BoolInfo.HasChanges);

			Dummy.Z0_Bool = false;
			AssertEquals("Current value", ZBool.False, Dummy.Z0_Bool);
			AssertEquals("Original value", ZBool.True, Dummy.Z0_BoolInfo.OriginalValue);
			Assert("Changes", Dummy.Z0_BoolInfo.HasChanges);

			Dummy.Z0_Date = new ZDateTime(1982, 3, 1);
			AssertEquals("Current value", new ZDateTime(1982, 3, 1), Dummy.Z0_Date);
			AssertEquals("Original value", new ZDateTime(1982, 10, 6), Dummy.Z0_DateInfo.OriginalValue);
			Assert("Changes", Dummy.Z0_DateInfo.HasChanges);

			Dummy.Z0_DateTimeOffset = new ZDateTimeOffset(1982, 3, 1, 1, 2, 3, TimeSpan.FromHours(11));
			AssertEquals("Current value", new ZDateTimeOffset(1982, 3, 1, 1, 2, 3, TimeSpan.FromHours(11)), Dummy.Z0_DateTimeOffset);
			AssertEquals("Original value", new ZDateTimeOffset(1982, 10, 6, 1, 2, 3, TimeSpan.FromHours(11)), Dummy.Z0_DateTimeOffsetInfo.OriginalValue);
			Assert("Changes", Dummy.Z0_DateTimeOffsetInfo.HasChanges);

			Dummy.Z0_Geography = new ZGeography("-122 47");
			AssertEquals("Current value", new ZGeography("-122 47"), Dummy.Z0_Geography);
			AssertEquals("Original value", new ZGeography("-121 48"), Dummy.Z0_GeographyInfo.OriginalValue);
			Assert("Changes", Dummy.Z0_GeographyInfo.HasChanges);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject newDummy = newFactory.Load<DummyBusinessObject>(Dummy.PK);

			AssertEquals("Current value", ZBool.False, newDummy.Z0_Bool);
			AssertEquals("Original value", ZBool.False, newDummy.Z0_BoolInfo.OriginalValue);

			newDummy.Z0_Bool = true;
			AssertEquals("Current value", ZBool.True, newDummy.Z0_Bool);
			AssertEquals("Original value", ZBool.False, newDummy.Z0_BoolInfo.OriginalValue);

			newDummy.Z0_Date = new ZDateTime(1984, 3, 2);
			AssertEquals("Current value", new ZDateTime(1984, 3, 2), newDummy.Z0_Date);
			AssertEquals("Original value", new ZDateTime(1982, 3, 1), newDummy.Z0_DateInfo.OriginalValue);

			newDummy.Z0_DateTimeOffset = new ZDateTimeOffset(1984, 3, 2, 1, 2, 3, TimeSpan.FromHours(11));
			AssertEquals("Current value", new ZDateTimeOffset(1984, 3, 2, 1, 2, 3, TimeSpan.FromHours(11)), newDummy.Z0_DateTimeOffset);
			AssertEquals("Original value", new ZDateTimeOffset(1982, 3, 1, 1, 2, 3, TimeSpan.FromHours(11)), newDummy.Z0_DateTimeOffsetInfo.OriginalValue);

			newDummy.Z0_Geography = new ZGeography("123 41");
			AssertEquals("Current value", new ZGeography("123 41"), newDummy.Z0_Geography);
			AssertEquals("Original value", new ZGeography("-122 47"), newDummy.Z0_GeographyInfo.OriginalValue);
		}

		public void TestOriginalValueWithNonPersistentBusinessObject()
		{
			DummyNonPersistent dummyNonPersistent = new DummyNonPersistent();
			dummyNonPersistent.Property = "New Value";
			AssertEquals("Value should be set for the test", "New Value", dummyNonPersistent.PropertyInfo.Value);
			AssertEquals("For non-persistent business object the OriginalValue should = Value", "New Value", dummyNonPersistent.PropertyInfo.OriginalValue);
		}

		public void TestOriginalValueWithNonPersistentProperty()
		{
			Dummy.NonPersistentProperty = "New Value";
			AssertEquals("Value should be set for the test", "New Value", Dummy.NonPersistentPropertyInfo.Value);
			AssertEquals("For a non-persistent property the OriginalValue should = Value", "New Value", Dummy.NonPersistentPropertyInfo.OriginalValue);
		}

		public void TestOriginalValueIsNull()
		{
			MyPropertyInfo info = new MyPropertyInfo(Dummy, "Z0_Date", null);
			try
			{
				IZType date = info.OriginalValue;
				Assert("No exception expected", true);
			}
			catch
			{
				Fail("No exception expected");
			}
		}

		public void TestOriginalValue_Provider()
		{
			var bizO = new DummyWithOriginalValueProvider();
			bizO.Property = "DEF";
			AssertEquals("ABC", bizO.PropertyInfo.OriginalValue);
			AssertEquals("DEF", bizO.PropertyInfo.Value);
		}

		class DummyWithOriginalValueProvider : DummyNonPersistent, IOriginalValueProvider
		{
			public IZType GetOriginalValue(ZPropertyInfo info)
			{
				switch (info.Name)
				{
					case "Property":
						return new ZString("ABC");
				}
				return info.Value;
			}
		}

		class MyPropertyInfo : ZPropertyInfo
		{
			public MyPropertyInfo(BusinessObject obj, string name, PropertyDescriptor descr)
				: base(obj, name, descr)
			{ }

			protected override object GetBizOOriginalValue()
			{
				return null;
			}
		}

		class DummyNonPersistent : NonPersistentBusinessObject
		{
			public ZString Property
			{
				get { return fProperty; }
				set { fProperty = value; }
			}
			ZString fProperty;

			public ZPropertyInfo PropertyInfo
			{
				get { return GetZPropertyInfo(nameof(Property)); }
			}
		}

		public void TestGetFriendlyColumnName()
		{
			var testPropertyInfo = new ZDummyPropertyInfo(Dummy, "Z0_Description");
			AssertEquals("Fred Flintstone", testPropertyInfo.GetFriendlyColumnNameExposed("FF_FredFlintstone"));
			AssertEquals("Parent ID", testPropertyInfo.GetFriendlyColumnNameExposed("JS_ParentID"));
			AssertEquals("Parent ID Link", testPropertyInfo.GetFriendlyColumnNameExposed("JS_ParentIDLink"));
			AssertEquals("INCO Term", testPropertyInfo.GetFriendlyColumnNameExposed("JS_INCOTerm"));
			AssertEquals("JS_RS_JS", testPropertyInfo.GetFriendlyColumnNameExposed("JS_RS_JS"));
			AssertEquals("JS_RS", testPropertyInfo.GetFriendlyColumnNameExposed("JS_RS"));
			AssertEquals("Event", testPropertyInfo.GetFriendlyColumnNameExposed("JS_RS_NKEvent"));
			AssertEquals("Inbond 7512 Number", testPropertyInfo.GetFriendlyColumnNameExposed("Inbond7512Number"));
			AssertEquals("Section 321A With FDA", testPropertyInfo.GetFriendlyColumnNameExposed("Section321AWithFDA"));
			AssertEquals("A3BX21 Code", testPropertyInfo.GetFriendlyColumnNameExposed("A3BX21Code"));
			AssertEquals("Section A32", testPropertyInfo.GetFriendlyColumnNameExposed("SectionA32"));
			AssertEquals("C4 Codes", testPropertyInfo.GetFriendlyColumnNameExposed("C4Codes"));
		}

		public void TestHumanReadableNameReturnsDescription()
		{
			string columnName = "Z0_Description";
			ZDummyPropertyInfo testPropertyInfo = new ZDummyPropertyInfo(Dummy, columnName);
			AssertEquals("PreCondition: Column should have a user description", true, testPropertyInfo.HasUserDescription);
			AssertEquals(testPropertyInfo.Description, testPropertyInfo.HumanReadableName);
		}

		public void TestHumanReadableNameReturnsFriendlyName()
		{
			string columnName = "Z0_Money";
			ZDummyPropertyInfo testPropertyInfo = new ZDummyPropertyInfo(Dummy, columnName);
			AssertEquals("PreCondition: Column should not have a user description", false, testPropertyInfo.HasUserDescription);
			AssertEquals(testPropertyInfo.GetFriendlyColumnNameExposed(columnName), testPropertyInfo.HumanReadableName);
		}

		public void TestIsDbColumnSmallDateTime()
		{
			Assert("The property is not mapped to a smalldatetime column so should not be saying it is, and yet...", !Dummy.Z0_DateInfo.IsDbColumnSmallDateTime);
			Assert("The property is mapped to a smalldatetime column so should be saying so, and yet...", Dummy.Z0_SmallDateTimeInfo.IsDbColumnSmallDateTime);

			var nonPersistendDummy = new DummyNonPersistent();
			Assert("It's a nonpersistent business object, so there's no database mapping therefore shouldn't be saying that it's mapped to a smalldatetime column, and yet...", !nonPersistendDummy.PropertyInfo.IsDbColumnSmallDateTime);
		}

		[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
		class DummyWithReadOnlySecurity : DummyBusinessObject
		{
			public DummyWithReadOnlySecurity(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetDescriptionReadOnly(bool shouldBeReadOnly)
			{
				Z0_Description_ReadOnly = shouldBeReadOnly;
			}

			public bool DescriptionShouldBeReadOnlyForSecurity;

			#region IReadOnlySecurity Members

			protected bool GetReadOnlySecurity(PropertyDescriptor property)
			{
				return property.Name == "Z0_Description" && DescriptionShouldBeReadOnlyForSecurity ||
					MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			}

			#endregion
		}

		public void TestReadOnlyBasedOnSecurityAndOverride()
		{
			DummyWithReadOnlySecurity dummy = Factory.New<DummyWithReadOnlySecurity>();
			dummy.SetDescriptionReadOnly(false);
			AssertEquals("Info is not readonly", false, dummy.Z0_DescriptionInfo.ReadOnly);

			dummy.SetDescriptionReadOnly(true);
			AssertEquals("Info is readonly by property setter", true, dummy.Z0_DescriptionInfo.ReadOnly);

			dummy.DescriptionShouldBeReadOnlyForSecurity = true;
			AssertEquals("Info is readonly by security", true, dummy.Z0_DescriptionInfo.ReadOnly);

			dummy.SetDescriptionReadOnly(false);
			AssertEquals("Info is still readonly by security", true, dummy.Z0_DescriptionInfo.ReadOnly);
		}

		public void TestReadOnlyBasedOnSecurity()
		{
			DummyWithReadOnlySecurity dummyWithReadOnlySecurity = Factory.New<DummyWithReadOnlySecurity>();
			dummyWithReadOnlySecurity.DescriptionShouldBeReadOnlyForSecurity = true;
			Assert("Z0_Description is reaonly", dummyWithReadOnlySecurity.Z0_DescriptionInfo.ReadOnly);
			Assert("Z0_Code is NOT reaonly", !dummyWithReadOnlySecurity.Z0_CodeInfo.ReadOnly);

			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			Assert("Z0_Description is NOT reaonly", !dummy.Z0_DescriptionInfo.ReadOnly);
			Assert("Z0_Code is NOT reaonly", !dummy.Z0_CodeInfo.ReadOnly);
		}

		public void TestHumanReadableNameGetAndSet()
		{
			AssertEquals("HumanReadabelName before setting", "Another Number", Dummy.Z0_AnotherNumberInfo.HumanReadableName);

			Dummy.Z0_AnotherNumberInfo.HumanReadableName = "this is a test";
			AssertEquals("HumanReadabelName before setting", "this is a test", Dummy.Z0_AnotherNumberInfo.HumanReadableName);

			Dummy.Z0_AnotherNumberInfo.HumanReadableName = ZString.Empty;
			AssertEquals("HumanReadabelName before setting", "Another Number", Dummy.Z0_AnotherNumberInfo.HumanReadableName);
		}

		public void TestPropertyInfoDescriptionForMultipleResourceStringData()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectSupportMultipleResourceStringData1>();
			AssertEquals("[21] Description", dummy.Z0_DescriptionInfo.Description);

			dummy.IsECCCompliant = true;
			AssertEquals("[5/34] Description", dummy.Z0_DescriptionInfo.Description);

			dummy.MultipleKeysToUseForTesting = new[] { "HELLO|WORLD" };
			AssertEquals("[54] Test", dummy.Z0_DescriptionInfo.Description);
		}

		public void TestPropertyInfoDescriptionForMultipleResourceStringDataNonMultipleKeyIsUsed()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectSupportMultipleResourceStringData2>();
			AssertEquals("[21] Description", dummy.Z0_DescriptionInfo.Description);

			dummy.IsECCCompliant = true;
			AssertEquals("[5/34] Description", dummy.Z0_DescriptionInfo.Description);

			dummy.MultipleKeysToUseForTesting = new[] { "HELLO|WORLD" };
			AssertEquals("Should pickup the override blank multiple key attribute", "[36] Test Description", dummy.Z0_DescriptionInfo.Description);
		}

		public void TestPropertyInfoDescriptionForMultipleResourceStringData_ExceptionThrown()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectSupportMultipleResourceStringData3>();
			AssertEquals("MultipleKeysToUseThrowsException, so fall back to default", "[54] Test", dummy.Z0_DescriptionInfo.Description);

			dummy.IsECCCompliant = true;
			AssertEquals("MultipleKeysToUseThrowsException, so fall back to default", "[54] Test", dummy.Z0_DescriptionInfo.Description);

			dummy.MultipleKeysToUseForTesting = new[] { "DummyBusinessObject|SAD" };
			AssertEquals("override still works", "[21] Description", dummy.Z0_DescriptionInfo.Description);
		}

		public void TestNonWesternEuropeanCharactersAddInfoFieldAttribute()
		{
			var bizObj = Factory.New<DummyBusinessObjectWithNAddInfoField>();

			var string1Info = bizObj.TC_String1Info;
			var string2Info = bizObj.TC_String2Info;

			AssertNotNull("string1Info should be a ZPropertyInfoString", string1Info as ZPropertyInfoString);
			AssertEquals("string1Info is not a NAddInfoField", false, string1Info.IsNAddInfoField());
			AssertEquals("string1Info is not a NAddInfoField", false, (string1Info as ZPropertyInfoString).IsNAddInfoField());
			AssertEquals("WrappedString1Info is not a NAddInfoField", false, bizObj.WrappedString1Info.IsNAddInfoField());

			AssertNotNull("string2Info should be a ZPropertyInfoString", string2Info as ZPropertyInfoString);
			AssertEquals("string2Info is a NAddInfoField", true, string2Info.IsNAddInfoField());
			AssertEquals("string2Info is a NAddInfoField", true, (string2Info as ZPropertyInfoString).IsNAddInfoField());
			AssertEquals("WrappedString2Info is a NAddInfoField", true, bizObj.WrappedString2Info.IsNAddInfoField());
		}

		class DummyBusinessObjectWithNAddInfoField : DummyBusinessObject
		{
			public DummyBusinessObjectWithNAddInfoField(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString TC_String1 { get; set; }
			public ZPropertyInfo TC_String1Info => GetZPropertyInfo(nameof(TC_String1));
			public ZWrappedPropertyInfo WrappedString1Info => GetWrappedZPropertyInfo(nameof(TC_String1), x => TC_String1Info);

			[IsNAddInfoField]
			public ZString TC_String2 { get; set; }
			public ZPropertyInfo TC_String2Info => GetZPropertyInfo(nameof(TC_String2));
			public ZWrappedPropertyInfo WrappedString2Info => GetWrappedZPropertyInfo(nameof(TC_String2), x => TC_String2Info);
		}

		#region Implementation

		ZPropertyInfo Info;

		protected override void SetUp()
		{
			base.SetUp();
			Info = Dummy.Z0_DescriptionInfo;
		}

		public class ZDummyPropertyInfo : ZPropertyInfo
		{
			public ZDummyPropertyInfo(BusinessObject bizO, string name)
				: base(bizO, name, null)
			{
			}

			public string GetFriendlyColumnNameExposed(string columnName)
			{
				return base.GetFriendlyColumnName(columnName);
			}
		}

		#endregion
	}
}
