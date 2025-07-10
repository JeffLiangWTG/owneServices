using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ReportOrderRegistryDataType))]
	class ReportOrderRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ReportOrderRegistryDataType>
	{
		#region Implementation

		protected override ReportOrderRegistryDataType GetNewDataType()
		{
			return new ReportOrderRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ReportOrderRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ReportOrderCollection collection = new ReportOrderCollection();

			ReportOrder reportOrder = collection.AddNew();
			reportOrder.DoNotPerformListValidationOnGLAccountSecondReportStartsFrom = true;
			reportOrder.Language = reportOrder.LanguageList[0].Code;
			reportOrder.AccountsOrderBeginsWith = reportOrder.AccountOrderTypeList[0].Code;
			reportOrder.GLAccountSecondReportStartsFrom = new ZGuid("c48b4691-8c26-4da8-a0ab-33488e4a2172");

			byte[] byteArrayValue = new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0,
				111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0,
				111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49,
				0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0,
				82, 0, 101, 0, 112, 0, 111, 0, 114, 0, 116, 0, 79, 0, 114, 0, 100, 0, 101, 0, 114, 0, 32,
				0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 100, 0, 61, 0, 34, 0,
				104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0,
				51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0,
				76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 34, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110,
				0, 115, 0, 58, 0, 120, 0, 115, 0, 105, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47,
				0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0,
				50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0,
				97, 0, 45, 0, 105, 0, 110, 0, 115, 0, 116, 0, 97, 0, 110, 0, 99, 0, 101, 0, 34, 0, 62, 0, 60,
				0, 82, 0, 101, 0, 112, 0, 111, 0, 114, 0, 116, 0, 79, 0, 114, 0, 100, 0, 101, 0, 114, 0, 62,
				0, 60, 0, 76, 0, 97, 0, 110, 0, 103, 0, 117, 0, 97, 0, 103, 0, 101, 0, 62, 0, 69, 0, 78, 0, 60,
				0, 47, 0, 76, 0, 97, 0, 110, 0, 103, 0, 117, 0, 97, 0, 103, 0, 101, 0, 62, 0, 60, 0, 67, 0, 111,
				0, 117, 0, 110, 0, 116, 0, 114, 0, 121, 0, 67, 0, 111, 0, 100, 0, 101, 0, 32, 0, 47, 0, 62, 0, 60, 0, 65,
				0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 115, 0, 79, 0, 114, 0, 100, 0, 101, 0, 114, 0, 66, 0, 101,
				0, 103, 0, 105, 0, 110, 0, 115, 0, 87, 0, 105, 0, 116, 0, 104, 0, 62, 0, 66, 0, 97, 0, 108, 0, 97, 0,
				110, 0, 99, 0, 101, 0, 83, 0, 104, 0, 101, 0, 101, 0, 116, 0, 60, 0, 47, 0, 65, 0, 99, 0, 99, 0,
				111, 0, 117, 0, 110, 0, 116, 0, 115, 0, 79, 0, 114, 0, 100, 0, 101, 0, 114, 0, 66, 0, 101, 0, 103,
				0, 105, 0, 110, 0, 115, 0, 87, 0, 105, 0, 116, 0, 104, 0, 62, 0, 60, 0, 71, 0, 76, 0, 65, 0, 99, 0, 99, 0,
				111, 0, 117, 0, 110, 0, 116, 0, 83, 0, 101, 0, 99, 0, 111, 0, 110, 0, 100, 0, 82, 0, 101, 0, 112, 0, 111,
				0, 114, 0, 116, 0, 83, 0, 116, 0, 97, 0, 114, 0, 116, 0, 115, 0, 70, 0, 114, 0, 111, 0, 109, 0, 62, 0, 99, 0, 52,
				0, 56, 0, 98, 0, 52, 0, 54, 0, 57, 0, 49, 0, 45, 0, 56, 0, 99, 0, 50, 0, 54, 0, 45, 0, 52, 0, 100, 0, 97, 0, 56, 0,
				45, 0, 97, 0, 48, 0, 97, 0, 98, 0, 45, 0, 51, 0, 51, 0, 52, 0, 56, 0, 56, 0, 101, 0, 52, 0, 97, 0, 50, 0, 49, 0, 55,
				0, 50, 0, 60, 0, 47, 0, 71, 0, 76, 0, 65, 0, 99, 0, 99, 0, 111, 0, 117, 0, 110, 0, 116, 0, 83, 0, 101, 0, 99, 0,
				111, 0, 110, 0, 100, 0, 82, 0, 101, 0, 112, 0, 111, 0, 114, 0, 116, 0, 83, 0, 116, 0, 97, 0, 114, 0, 116, 0, 115, 0,
				70, 0, 114, 0, 111, 0, 109, 0, 62, 0, 60, 0, 47, 0, 82, 0, 101, 0, 112, 0, 111, 0, 114, 0, 116, 0, 79, 0, 114, 0,
				100, 0, 101, 0, 114, 0, 62, 0, 60, 0, 47, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0, 82, 0, 101, 0,
				112, 0, 111, 0, 114, 0, 116, 0, 79, 0, 114, 0, 100, 0, 101, 0, 114, 0, 62, 0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
