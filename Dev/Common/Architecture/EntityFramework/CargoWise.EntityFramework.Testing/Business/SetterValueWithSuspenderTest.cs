using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SetterValueWithSuspenderTest : TestCaseWithFactory
	{
		public void TestSetValueOnlySuspendCorrentBizObj()
		{
			var bizObj1 = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj1.SupportedFieldsForTesting = new[] { "Z0_Number" };
			bizObj1.Z0_Number = 123;
			var bizObj2 = Factory.New<DummySetterSuspenderSupporterClass>();
			using (var suspender = new SetterValueWithSuspender(bizObj1, new[] { "Z0_Description", "Z0_Number", "Z0_VarCharMax" }))
			{
				suspender.SetValue(bizObj1, "Z0_Description", new ZString("Hello"));
				AssertEquals("Defaulted from Z0_Description", 9000, bizObj1.Z0_Number);
				suspender.SetValue(bizObj1, "Z0_Number", new ZInt(100));
				AssertEquals("Set by Z0_Number", 100, bizObj1.Z0_Number);
				suspender.SetValue(bizObj1, "Z0_Number", new ZInt(150));
				AssertEquals("Set by Z0_Number", 150, bizObj1.Z0_Number);
				suspender.SetValue(bizObj1, "Z0_VarCharMax", new ZString("Hi"));
				AssertEquals("Should keep what's set", 150, bizObj1.Z0_Number);

				suspender.SetValue(bizObj2, "Z0_VarCharMax", new ZString("Hi"));
				AssertEquals(9999, bizObj2.Z0_Number);
				suspender.SetValue(bizObj2, "Z0_Number", new ZInt(200));
				AssertEquals(200, bizObj2.Z0_Number);
				suspender.SetValue(bizObj2, "Z0_Description", new ZString("Hello"));
				AssertEquals(9000, bizObj2.Z0_Number);
			}
		}

		public void TestSetValueOnlyWhenItIsDifferent()
		{
			var bizObj = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj.SupportedFieldsForTesting = new[] { "Z0_Number" };
			bizObj.TimesSettersCalled = 0; // start afresh
			bizObj.Z0_Number = 123;
			AssertEquals(1, bizObj.TimesSettersCalled);

			using (var suspender = new SetterValueWithSuspender(bizObj, new[] { "Z0_Number" }))
			{
				suspender.SetValue(bizObj, "Z0_Number", 200);
				AssertEquals("Value changed", 2, bizObj.TimesSettersCalled);

				suspender.SetValue(bizObj, "Z0_Number", 200);
				AssertEquals("The suspender will not try to update a value if it's already that value.", 2, bizObj.TimesSettersCalled);

				bizObj.Z0_Number = 300;
				AssertEquals(3, bizObj.TimesSettersCalled);

				bizObj.Z0_Number = 300;
				AssertEquals("Accessing the value directly causes the setter to be called. ", 4, bizObj.TimesSettersCalled);
			}
			bizObj.Z0_Number = 300;
			AssertEquals("Accessing the value directly causes the setter to be called. ", 5, bizObj.TimesSettersCalled);
		}

		public void TestSetValue()
		{
			var bizObj = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj.SupportedFieldsForTesting = new[] { "Z0_Number" };
			bizObj.Z0_Number = 123;
			using (var suspender = new SetterValueWithSuspender(bizObj, new[] { "Z0_Description", "Z0_Number", "Z0_VarCharMax" }))
			{
				suspender.SetValue(bizObj, "Z0_Description", new ZString("Hello"));
				AssertEquals("Defaulted from Z0_Description", 9000, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(100));
				AssertEquals("Set by Z0_Number", 100, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(150));
				AssertEquals("Set by Z0_Number", 150, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_VarCharMax", new ZString("Hi"));
				AssertEquals("Should keep what's set", 150, bizObj.Z0_Number);
				bizObj.Z0_Number = 200;
				AssertEquals("Should keep what was previously set", 150, bizObj.Z0_Number);
			}
			bizObj.Z0_Number = 200;
			AssertEquals("Should have resume setting", 200, bizObj.Z0_Number);

			bizObj = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj.Z0_Number = 123;
			using (var suspender = new SetterValueWithSuspender(bizObj, new[] { "Z0_Description", "Z0_Number", "Z0_VarCharMax" }))
			{
				suspender.SetValue(bizObj, "Z0_Description", new ZString("Hello"));
				AssertEquals("Defaulted from Z0_Description", 9000, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(100));
				AssertEquals("Set by Z0_Number", 100, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(150));
				AssertEquals("Set by Z0_Number", 150, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_VarCharMax", new ZString("Hi"));
				AssertEquals("Should be defaulted from Z0_VarCharMax", 9999, bizObj.Z0_Number);
				bizObj.Z0_Number = 200;
				AssertEquals("Should allow set", 200, bizObj.Z0_Number);
			}
			bizObj.Z0_Number = 300;
			AssertEquals("Should allow set", 300, bizObj.Z0_Number);

			bizObj = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj.Z0_Number = 123;
			using (bizObj.SetterSuspender.SuspendSetting("Z0_Number"))
			using (var suspender = new SetterValueWithSuspender(bizObj, new[] { "Z0_Description", "Z0_Number", "Z0_VarCharMax" }))
			{
				suspender.SetValue(bizObj, "Z0_Description", new ZString("Hello"));
				AssertEquals("Should be original value", 123, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(100));
				AssertEquals("Should be original value", 123, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_Number", new ZInt(150));
				AssertEquals("Should be original value", 123, bizObj.Z0_Number);
				suspender.SetValue(bizObj, "Z0_VarCharMax", new ZString("Hi"));
				AssertEquals("Should be original value", 123, bizObj.Z0_Number);
				bizObj.Z0_Number = 200;
				AssertEquals("Should be original value", 123, bizObj.Z0_Number);
			}
			bizObj.Z0_Number = 300;
			AssertEquals("Should allow set", 300, bizObj.Z0_Number);

			bizObj = Factory.New<DummySetterSuspenderSupporterClass>();
			bizObj.SupportedFieldsForTesting = new[] { "Z0_Number" };
			bizObj.Z0_Number = 123;
			using (bizObj.SetterSuspender.SuspendSetting("Z0_Number"))
			{
				using (var suspender = new SetterValueWithSuspender(bizObj, new[] { "Z0_Description", "Z0_Number", "Z0_VarCharMax" }))
				{
					suspender.SetValue(bizObj, "Z0_Description", new ZString("Hello"));
					AssertEquals("Should be original value", 123, bizObj.Z0_Number);
					suspender.SetValue(bizObj, "Z0_Number", new ZInt(100));
					AssertEquals("Set by Z0_Number override", 100, bizObj.Z0_Number);
					suspender.SetValue(bizObj, "Z0_Number", new ZInt(150));
					AssertEquals("Set by Z0_Number override", 150, bizObj.Z0_Number);
					suspender.SetValue(bizObj, "Z0_VarCharMax", new ZString("Hi"));
					AssertEquals("Should keep what's set", 150, bizObj.Z0_Number);
					bizObj.Z0_Number = 200;
					AssertEquals("Should keep what was previously set", 150, bizObj.Z0_Number);
				}
				bizObj.Z0_Number = 200;
				AssertEquals("Should keep what was previously set", 150, bizObj.Z0_Number);
			}
			bizObj.Z0_Number = 300;
			AssertEquals("Should allow set", 300, bizObj.Z0_Number);
		}

		#region ISetterSuspenderSupporter

		class DummySetterSuspenderSupporterClass : DummyChildBusinessObject, ISetterSuspenderSupporter
		{
			public DummySetterSuspenderSupporterClass(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IEnumerable<string> ISetterSuspenderSupporter.SupportedFields => SupportedFieldsForTesting ?? Enumerable.Empty<string>();
			public string[] SupportedFieldsForTesting;

			SetterSuspender fsuspender;
			public SetterSuspender SetterSuspender => fsuspender ?? (fsuspender = new SetterSuspender());

			public override ZInt Z0_Number
			{
				get => base.Z0_Number;
				set
				{
					TimesSettersCalled++;
					if (value != base.Z0_Number && !SetterSuspender.IsSetterSuspended(nameof(Z0_Number)))
					{
						base.Z0_Number = value;
					}
				}
			}

			public override ZString Z0_VarCharMax
			{
				get => base.Z0_VarCharMax;
				set
				{
					TimesSettersCalled++;
					base.Z0_VarCharMax = value;
					Z0_Number = 9999;
				}
			}

			public override ZString Z0_Description
			{
				get => base.Z0_Description;
				set
				{
					TimesSettersCalled++;
					Z0_Number = 9000;
					base.Z0_Description = value;
				}
			}

			public int TimesSettersCalled { get; set; }
		}

		#endregion
	}
}
