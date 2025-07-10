using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyZZBizoTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestPerformance()
		{
			var sw = Stopwatch.StartNew();
			var l = new List<DummyZZBizo>(30000);
			for (var i = 0; i < 30000; i++)
			{
				l.Add(Factory.New<DummyZZBizo>());
			}
			var new30000 = sw.ElapsedMilliseconds;
			sw.Restart();
			for (var i = 0; i < 30000; i++)
			{
				l[i].Z0_AddInfoDecimal073 = i;
			}
			var set30000 = sw.ElapsedMilliseconds;
			sw.Restart();
			Factory.Save();
			var save30000 = sw.ElapsedMilliseconds;
			sw.Restart();
			var f2 = new BusinessObjectFactory();
			f2.Load<DummyZZBizo>(new ZQuery());
			var load30000 = sw.ElapsedMilliseconds;
			Fail($@"new : {new30000}\r\nset : {set30000}\r\nSave : {save30000}\r\nLoad : {load30000}");
		}

		public void TestValidation()
		{
			var bizO = Factory.New<DummyZZBizo>();
			AssertType<DummyZZBizoValidation>(bizO.Validation);
		}

		public void TestPropertySetsHasChanges()
		{
			var bizO = Factory.New<DummyZZBizo>();
			Assert(!bizO.HasChanges);
			bizO.Z0_AddInfoDecimal073 = 123m;
			Assert(!bizO.Z0_AddInfoDecimal073Info.HasChanges);
			Assert(bizO.HasChanges);

			Factory.Save();
			Assert(!bizO.Z0_AddInfoDecimal073Info.HasChanges);
			Assert(!bizO.HasChanges);

			bizO.Z0_AddInfoDecimal073 = 456m;
			Assert(bizO.Z0_AddInfoDecimal073Info.HasChanges);
			Assert(bizO.HasChanges);

			Factory.Save();
			Assert(!bizO.Z0_AddInfoDecimal073Info.HasChanges);
			Assert(!bizO.HasChanges);
		}

		public void TestPropertyRoundtrip()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDecimal073 = 123m;
			Factory.Save();
			var f2 = new BusinessObjectFactory();
			var bizoInNewFactory = f2.Load<DummyZZBizo>(bizO.PK);
			AssertEquals(123m, bizoInNewFactory.Z0_AddInfoDecimal073);
		}

		public void TestValueChanged()
		{
			var changed = false;
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDecimal073Info.ValueChanged += (sender, eventargs) =>
			{
				changed = true;
			};
			Assert(!changed);
			bizO.Z0_AddInfoDecimal073 = 123m;
			Assert(changed);
		}

		public void TestDataRefresh()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDecimal073 = 123m;
			Factory.Save();
			var f2 = new BusinessObjectFactory();
			var bizoInNewFactory = f2.Load<DummyZZBizo>(bizO.PK);
			bizoInNewFactory.Z0_AddInfoDecimal073 = 456m;
			f2.Save();
			AssertEquals(456m, bizO.Z0_AddInfoDecimal073);
		}

		public void TestConcurrencyResolution_Default_Date() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoDate, ZDate.BrettsBirthday, ZDate.Today, ZDate.Today.AddDays(1));
		public void TestConcurrencyResolution_Default_DateTime() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoDateTime, ZDateTime.BrettsBirthday, ZDateTime.Today, ZDateTime.Today.AddDays(1));
		public void TestConcurrencyResolution_Default_Decimal() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoDecimal073, new ZDecimal(123m), new ZDecimal(456m), new ZDecimal(789m));
		public void TestConcurrencyResolution_Default_Guid() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoGuid, ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());
		public void TestConcurrencyResolution_Default_Short() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoInt16, new ZShort(123), new ZShort(456), new ZShort(789));
		public void TestConcurrencyResolution_Default_Int() => AssertConcurrencyResolution_Default(AutoZZDummyBizo.Schema.Z0_AddInfoInt32, new ZInt(123), new ZInt(456), new ZInt(789));

		void AssertConcurrencyResolution_Default<TZType>(string propertyName, TZType originalValue, TZType otherValue, TZType newValue)
			where TZType : IZType
		{
			using var handlerDisposable = NotificationHandler.SetHandler(new ZGUINotificationHandler());

			var propertyNameWithoutPrefix = propertyName.Substring(3);
			var newValueString = newValue.GetStringRepresentation();
			var originalValueString = originalValue.GetStringRepresentation();
			var otherValueString = otherValue.GetStringRepresentation();

			Factory.RefreshEnabled = false;
			var bizO = Factory.New<DummyZZBizo>();
			bizO[propertyName] = originalValue;
			Factory.Save();
			var f2 = new BusinessObjectFactory();
			f2.RefreshEnabled = false;
			var bizoInNewFactory = f2.Load<DummyZZBizo>(bizO.PK);
			bizoInNewFactory[propertyName] = otherValue;
			f2.Save();

			AssertEquals("Precondition: Z0_AddInfo is updated correctly.", $"{propertyNameWithoutPrefix}={originalValueString}", bizO.Z0_AddInfo);
			AssertEquals("Precondition: Z0_AddInfo is updated correctly.", $"{propertyNameWithoutPrefix}={otherValueString}", bizoInNewFactory.Z0_AddInfo);

			bizO[propertyName] = newValue;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				AssertEquals("Precondition: Since Save failed, Z0_AddInfo will contain the new local value.", $"{propertyNameWithoutPrefix}={newValueString}", bizO.Z0_AddInfo);
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("Precondition: With ConcurrencyPolicy.Default, Z0_AddInfo will be merged.", $"{propertyNameWithoutPrefix}={otherValueString}", bizO.Z0_AddInfo);
			}

			AssertHasWarningContaining("Notifications should be added against actual property info.",
				bizO.GetZPropertyInfo(propertyName),
				$@"Another user () has changed this field.
Yours: '{newValueString}', Theirs: '{otherValueString}'");
			AssertEquals("Value should be merged with ConcurrencyPolicy.Default", otherValue, bizO[propertyName]);
		}

		public void TestConcurrencyResolution_OnlyAddWarningsToChangedProperties()
		{
			using var handlerDisposable = NotificationHandler.SetHandler(new ZGUINotificationHandler());

			Factory.RefreshEnabled = false;
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoBool = ZBool.True;
			bizO.Z0_AddInfoDate = ZDate.BrettsBirthday;
			bizO.Z0_AddInfoDateTime = ZDateTime.BrettsBirthday;
			bizO.Z0_AddInfoDecimal073 = 123m;
			bizO.Z0_AddInfoGuid = ZGuid.BrettsGuid;
			bizO.Z0_AddInfoInt16 = 123;
			bizO.Z0_AddInfoInt32 = 123;

			Factory.Save();
			var f2 = new BusinessObjectFactory();
			f2.RefreshEnabled = false;
			var bizoInNewFactory = f2.Load<DummyZZBizo>(bizO.PK);
			bizoInNewFactory.Z0_AddInfoDecimal073 = 456m;
			f2.Save();

			bizO.Z0_AddInfoDecimal073 = 789m;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			CombineAssertions(() =>
			{
				var message = "Notifications should be added against actual property info.";
				var warning = "Another user () has changed this field.";
				AssertHasWarningContaining(message, bizO.Z0_AddInfoDecimal073Info, warning);
				message = "Notifications should not be added against AddInfo properties that did not change.";
				AssertNoWarningContaining(message, bizO.Z0_AddInfoBoolInfo, warning);
				AssertNoWarningContaining(message, bizO.Z0_AddInfoDateInfo, warning);
				AssertNoWarningContaining(message, bizO.Z0_AddInfoDateTimeInfo, warning);
				AssertNoWarningContaining(message, bizO.Z0_AddInfoGuidInfo, warning);
				AssertNoWarningContaining(message, bizO.Z0_AddInfoInt16Info, warning);
				AssertNoWarningContaining(message, bizO.Z0_AddInfoInt32Info, warning);
			});
		}

		public void TestStarSerialization()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "abc*def";
			bizO.Z0_AddInfoString3 = "ghi";
			((INeedRow)bizO).Row[DummyZZBizo.Schema.Z0_AddInfo] = new ZString("ghi");
			Factory.Save();
			AssertEquals("AddInfoString3=ghi*AddInfoString35=abc¤def", bizO.Z0_AddInfo);

			var bizOInNewFactory = new BusinessObjectFactory().Load<DummyZZBizo>(bizO.PK);
			AssertEquals("abc*def", bizOInNewFactory.Z0_AddInfoString35);
		}

		public void TestBooleanToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoBool = ZBool.True;
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoBool=Y", bizO.Z0_AddInfo);
		}

		public void TestDateToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDate = new ZDate(2004, 9, 18);
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoDate=2004-09-18", bizO.Z0_AddInfo);
		}

		public void TestDateTimeToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDateTime = new ZDateTime(2004, 9, 18, 12, 30, 31);
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoDateTime=" + bizO.Z0_AddInfoDateTime.GetStringRepresentation(), bizO.Z0_AddInfo);
		}

		public void TestDecimalToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoDecimal122 = 123456.12345m;
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoDecimal122=123456.12345", bizO.Z0_AddInfo);

			bizO.Z0_AddInfoDecimal122 = 100000.00000m;
			bizO.OnFactorySavingInternal();
			AssertEquals("Trailing zeros should be truncated", "AddInfoDecimal122=100000", bizO.Z0_AddInfo);
		}

		public void TestIntToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoInt32 = 123456;
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoInt32=123456", bizO.Z0_AddInfo);
		}

		public void TestShortToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoInt16 = 12345;
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoInt16=12345", bizO.Z0_AddInfo);
		}

		public void TestStringToString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "123456";
			bizO.Z0_NAddInfoString35 = "一二三四五六";
			bizO.OnFactorySavingInternal();
			AssertEquals("ToString()", "AddInfoString35=123456", bizO.Z0_AddInfo);
			AssertEquals("ToString()", "NAddInfoString35=一二三四五六", bizO.Z0_NAddInfo);
		}

		public void TestLoadBooleanFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoBool=Y";
			AssertEquals("Value", ZBool.True, bizO.Z0_AddInfoBool);
		}

		public void TestLoadDateFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoDate=2012-01-02";
			AssertEquals("Value", new ZDate(2012, 1, 2), bizO.Z0_AddInfoDate);

			bizO.Z0_AddInfo = "AddInfoDate=2012-01-02 00:00:01";
			AssertEquals("Value", new ZDate(2012, 1, 2), bizO.Z0_AddInfoDate);

			bizO.Z0_AddInfo = "AddInfoDate=2012-01-99";
			AssertEquals("Value", ZDate.Invalid, bizO.Z0_AddInfoDate);
		}

		public void TestLoadDateTimeFromString()
		{
			var testDateTime = ZDateTime.Now;
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoDateTime=" + testDateTime.SqlFormat;
			AssertEquals("Value", testDateTime, bizO.Z0_AddInfoDateTime);

			bizO.Z0_AddInfo = "AddInfoDateTime=2012-01-02 00:00:01";
			AssertEquals("Value", new ZDateTime(2012, 1, 2, 0, 0, 1), bizO.Z0_AddInfoDateTime);

			bizO.Z0_AddInfo = "AddInfoDateTime=2012-01-02 AB:AB:01";
			AssertEquals("Value", ZDateTime.Invalid, bizO.Z0_AddInfoDateTime);
		}

		public void TestLoadDecimalFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoDecimal122=123.12345";
			AssertEquals("Value", 123.12345m, bizO.Z0_AddInfoDecimal122);
		}

		public void TestLoadIntFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoInt32=123";
			AssertEquals("Value", 123, bizO.Z0_AddInfoInt32);
		}

		public void TestLoadShortFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoInt16=123";
			AssertEquals("Value", (short)123, bizO.Z0_AddInfoInt16);
		}

		public void TestLoadStringFromString()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoString35=123456";
			bizO.Z0_NAddInfo = "NAddInfoString35=一二三四五六";
			AssertEquals("AddInfoString35 Value", "123456", bizO.Z0_AddInfoString35);
			AssertEquals("NAddInfoString35 Value", "一二三四五六", bizO.Z0_NAddInfoString35);
		}

		[DeveloperOnlyTest]
		public void TestOnLoaded_Performance()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfo = "AddInfoString35=Hello World";
			bizO.Z0_AddInfo = "NAddInfoString35=你好，世界";
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 1000000; i++)
			{
				bizO.OnLoadedInternal();
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 2000);
		}

		[DeveloperOnlyTest]
		public void TestOnFactorySaving_Performance()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "Hello World";
			bizO.Z0_NAddInfoString35 = "你好，世界";
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 1000000; i++)
			{
				bizO.OnFactorySavingInternal();
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 7000);
		}

		public void TestValidateCharacterSet()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "123456";
			bizO.Z0_NAddInfoString35 = "一二三四五六";
			bizO.Validation.ValidateZ0_AddInfoString35();
			bizO.Validation.ValidateZ0_NAddInfoString35();
			AssertEquals("Z0_AddInfoString35 has no errors.", false, bizO.Z0_AddInfoString35Info.HasNotifications());
			AssertEquals("Z0_NAddInfoString35 has no errors.", false, bizO.Z0_NAddInfoString35Info.HasNotifications());
			bizO.Z0_AddInfoString35 = "UZ\u73a8";
			bizO.Z0_NAddInfoString35 = "UZ\u73a8";
			bizO.Validation.ValidateZ0_AddInfoString35();
			bizO.Validation.ValidateZ0_NAddInfoString35();
			AssertEquals("Z0_AddInfoString35 has non-english character error?", true, bizO.Z0_AddInfoString35Info.HasError("Add Info String 35 only accepts Western European languages characters."));
			AssertEquals("Z0_NAddInfoString35 has no errors.", false, bizO.Z0_NAddInfoString35Info.HasNotifications());
		}

		public void TestOriginalValue()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "123456";
			bizO.Z0_NAddInfoString35 = "一二三四五六";
			Factory.Save();

			var bizOInNewFactory = new BusinessObjectFactory().Load<DummyZZBizo>(bizO.PK);
			bizOInNewFactory.Z0_AddInfoString35 = "654321";
			bizOInNewFactory.Z0_NAddInfoString35 = "六五四三二一";
			AssertEquals("123456", bizOInNewFactory.Z0_AddInfoString35Info.OriginalValue);
			AssertEquals("一二三四五六", bizOInNewFactory.Z0_NAddInfoString35Info.OriginalValue);
		}

		public void TestIConcurrencyExceptionDecorator()
		{
			var bizO = Factory.New<DummyZZBizo>();
			Assert(bizO is IConcurrencyExceptionDecorator);

			var record = new Mock<IPropertyRecord>();
			record.Setup(m => m.ColumnName).Returns(AutoDummyBizo.Schema.Z0_AddInfo);
			record.Setup(m => m.OriginalValue).Returns("AddInfoString3=ABC");
			record.Setup(m => m.CurrentValue).Returns("AddInfoString3=DEF");
			record.Setup(m => m.DatabaseValue).Returns("AddInfoString3=GHI");
			record.Setup(m => m.DisplayName).Returns(bizO.Z0_AddInfoInfo.HumanReadableName);

			var stringBuilder = new StringBuilder();
			bizO.AppendDecoratedDisplayName(stringBuilder, record.Object);

			var expected = @"	Add Info
		Add Info String 3
";
			AssertEquals(expected, stringBuilder.ToString());
		}

		[ExpectNoExceptions]
		public void TestIConcurrencyExceptionDecorator_DuplicateAddInfo()
		{
			var stringBuilder = new StringBuilder();
			var bizO = Factory.New<DummyZZBizo>();
			var record = new Mock<IPropertyRecord>();
			record.Setup(m => m.ColumnName).Returns(AutoDummyBizo.Schema.Z0_AddInfo);
			record.Setup(m => m.OriginalValue).Returns("AddInfoString3=ABC*AddInfoString3=ABC");
			record.Setup(m => m.CurrentValue).Returns("AddInfoString3=DEF*AddInfoString3=DEF");
			record.Setup(m => m.DatabaseValue).Returns("AddInfoString3=GHI*AddInfoString3=GHI");
			record.Setup(m => m.DisplayName).Returns(bizO.Z0_AddInfoInfo.HumanReadableName);

			bizO.AppendDecoratedDisplayName(stringBuilder, record.Object);
		}

		[ExpectNoExceptions]
		public void TestIConcurrencyExceptionDecorator_InvalidAddInfo()
		{
			var stringBuilder = new StringBuilder();
			var bizO = Factory.New<DummyZZBizo>();
			var record = new Mock<IPropertyRecord>();
			record.Setup(m => m.ColumnName).Returns(AutoDummyBizo.Schema.Z0_AddInfo);
			record.Setup(m => m.OriginalValue).Returns("String3=ABC");
			record.Setup(m => m.CurrentValue).Returns("String3=DEF");
			record.Setup(m => m.DatabaseValue).Returns("String3=GHI");
			record.Setup(m => m.DisplayName).Returns(bizO.Z0_AddInfoInfo.HumanReadableName);

			bizO.AppendDecoratedDisplayName(stringBuilder, record.Object);
		}

		public void TestCopyPersistentValuesFrom_WithSetters()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "value1";
			bizO.Z0_NAddInfoString35 = "价值1";
			var bizO2Moq = Factory.NewMoq<DummyZZBizo>();
			var bizO2 = bizO2Moq.Object;
			bizO2.CopyPersistentValuesFrom(bizO);
			bizO2Moq.Verify(x => x.Z0_AddInfoString35, Times.AtLeastOnce());
			bizO2Moq.Verify(x => x.Z0_NAddInfoString35, Times.AtLeastOnce());
			AssertEquals(bizO.Z0_AddInfoString35, bizO2.Z0_AddInfoString35);
			AssertEquals(bizO.Z0_NAddInfoString35, bizO2.Z0_NAddInfoString35);
		}

		public void TestCopyPersistentValuesFrom_WithoutSetters()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "value1";
			bizO.Z0_NAddInfoString35 = "价值1";
			var bizO2Moq = Factory.NewMoq<DummyZZBizo>();
			var bizO2 = bizO2Moq.Object;
			bizO2.CopyPersistentValuesFrom(bizO, new BusinessObjectCloneArgs([], true));
			bizO2Moq.Verify(x => x.Z0_AddInfoString35, Times.Never());
			bizO2Moq.Verify(x => x.Z0_NAddInfoString35, Times.Never());
			AssertEquals(bizO.Z0_AddInfoString35, bizO2.Z0_AddInfoString35);
			AssertEquals(bizO.Z0_NAddInfoString35, bizO2.Z0_NAddInfoString35);
		}

		public void TestClone()
		{
			var bizO = Factory.New<DummyZZBizo>();
			bizO.Z0_AddInfoString35 = "value1";
			bizO.Z0_NAddInfoString35 = "价值1";
			var bizO2 = bizO.Clone() as DummyZZBizo;
			AssertEquals(bizO.Z0_AddInfoString35, bizO2.Z0_AddInfoString35);
			AssertEquals(bizO.Z0_NAddInfoString35, bizO2.Z0_NAddInfoString35);
		}
	}
}
