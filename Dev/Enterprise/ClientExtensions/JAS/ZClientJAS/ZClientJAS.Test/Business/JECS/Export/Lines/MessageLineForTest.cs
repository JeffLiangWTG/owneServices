using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class MessageLineForTest : MessageLine
	{
		public MessageLineForTest() : this("TESTLINE", "LineContent1", "AnotherContent2", "AnotherValue")
		{
		}

		public MessageLineForTest(ZString testLineType, ZString field1Value, ZString field2Value, ZString field3Value)
		{
			this.TestLineType = testLineType;
			this.Field1Value = field1Value;
			this.Field2Value = field2Value;
			this.Field3Value = field3Value;
			this.TestFieldCount = JXCConstants.TestFieldCount;
		}

		public ZString TestLineType;
		public ZString Field1Value;
		public ZString Field2Value;
		public ZString Field3Value;
		public int TestFieldCount;
		protected override ZString LineType
		{
			get
			{
				return TestLineType;
			}
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.TestFieldPositions.Field1, Field1Value);
			if (FieldCount > JXCConstants.TestFieldPositions.Field2)
			{
				dataRow.SetField(JXCConstants.TestFieldPositions.Field2, Field2Value);
			}

			if (FieldCount > JXCConstants.TestFieldPositions.Field3)
			{
				dataRow.SetField(JXCConstants.TestFieldPositions.Field3, Field3Value);
			}
		}

		protected override int FieldCount
		{
			get
			{
				return TestFieldCount;
			}
		}
	}
}
