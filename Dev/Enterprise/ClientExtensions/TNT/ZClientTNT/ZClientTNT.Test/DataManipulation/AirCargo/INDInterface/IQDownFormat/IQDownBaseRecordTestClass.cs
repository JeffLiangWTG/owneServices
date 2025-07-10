using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
	public class IQDownBaseRecordTestClass : IQDownBaseRecord
	{
		public IQDownBaseRecordTestClass(ZString rawData) : base(rawData, 3)
		{
		}

#region Schema
		public static class Schema
		{
			public static readonly FixedWidthFlatFileFieldProperty TestString = new FixedWidthFlatFileFieldProperty(0, 0, 10); // 1 10 Test String
			public static readonly FixedWidthFlatFileFieldProperty TestDecimal = new FixedWidthFlatFileFieldProperty(1, 10, 5); // 11 15 Test Date
			public static readonly FixedWidthFlatFileFieldProperty TestInt = new FixedWidthFlatFileFieldProperty(2, 15, 5); // 16 20 Test Date
		}

#endregion
#region Field Property
		public ZString TestString
		{
			get
			{
				return this[Schema.TestString.Name];
			}
		}

		public ZDecimal TestDecimal
		{
			get
			{
				return GetFieldAsZDecimal(Schema.TestDecimal.Name);
			}
		}

		public ZInt TestInt
		{
			get
			{
				return GetFieldAsZInt(Schema.TestInt.Name);
			}
		}

		public override ZString RecordDelimiter
		{
			get
			{
				return fRecordDelimiter;
			}
		}

		string fRecordDelimiter = ".";
		public override ZString HumanReadable
		{
			get
			{
				return new ZString("Test HumanReadable");
			}
		}

#endregion
		protected override void FillFromRawData(ZString rawData)
		{
			SetProperty(Schema.TestString, rawData);
			SetProperty(Schema.TestDecimal, rawData);
			SetProperty(Schema.TestInt, rawData);
		}

		public class Test : IQDownBaseRecordTest
		{
			public override void TestFieldProperty()
			{
				IQDownBaseRecordTestClass record = new IQDownBaseRecordTestClass(DataString);
				AssertEquals(new ZString("teststr   "), record.TestString);
				AssertEquals(new ZDecimal(23.3), record.TestDecimal);
				AssertEquals(new ZInt(54), record.TestInt);
			}

			public override void TestHumanReadable()
			{
				IQDownBaseRecordTestClass record = new IQDownBaseRecordTestClass(DataString);
				AssertEquals("HumanReadable", new ZString("Test HumanReadable"), record.HumanReadable);
			}

			public void TestIsValid()
			{
				IQDownBaseRecordTestClass record = new IQDownBaseRecordTestClass(DataString);
				AssertEquals("PreCondition: RecordDelimiter should contain '.'", ".", record.RecordDelimiter);
				NotificationBuffer buffer = new NotificationBuffer();
				AssertEquals("It should be a valid record - Buffer:" + System.Environment.NewLine + buffer.AsString, true, record.IsValid(buffer));
				AssertEquals("Buffer should have no error - Buffer:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
				record.fRecordDelimiter = "ad";
				AssertEquals("It should be an invalid record - Buffer:" + System.Environment.NewLine + buffer.AsString, false, record.IsValid(buffer));
				AssertEquals("Buffer should have errors - Buffer:" + System.Environment.NewLine + buffer.AsString, true, buffer.HasErrors);
				AssertEquals("Buffer should have errors - Buffer:" + System.Environment.NewLine + buffer.AsString, true, buffer.ContainsNotificationType(TNTErrorType.InvalidRecordDelimiter));
				string errorMessage = " Error was encouter on " + record.HumanReadable;
				AssertEquals("Buffer should have error message '" + errorMessage + "' - Buffer:" + System.Environment.NewLine + buffer.AsString, true, buffer.AsString.IndexOf(errorMessage) >= 0);
			}

			protected override IQDownBaseRecord GetRecord(ZString rawData)
			{
				return new IQDownBaseRecordTestClass(rawData);
			}

			protected override ZString DataString
			{ //	  1234567890ddMMyyyy1234512345
				get
				{
					return "teststr    23.3 54";
				}
			}

			protected override Type ExpectedRecordType
			{
				get
				{
					return typeof(IQDownBaseRecordTestClass);
				}
			}

			protected override int ExpectedFieldCount
			{
				get
				{
					return 3;
				}
			}
		}
	}
}
