using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	abstract class StmSystemDefinedFieldBaseTestCase : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("S1_DisplayEditRule", "ING", Field.S1_DisplayEditRule);
			AssertEquals("S1_Validation", "NOV", Field.S1_Validation);
		}

		public void TestIsGrid()
		{
			AssertEquals("IsGrid", false, Field.IsGrid);
			Field.S1_Type = "GRD";
			AssertEquals("IsGrid", true, Field.IsGrid);
		}

		public void TestValidationType()
		{
			AssertEquals("Validation.GetType()", ExpectedValidationType, Field.Validation.GetType());
		}

		public void TestParentCollection()
		{
			AssertNull("ParentCollection", Field.ParentCollection);

			var mockCollection = new Mock<BusinessObjectCollection>(new object[] { Factory }) { CallBase = true };
			BusinessObjectCollection collection = mockCollection.Object;

			collection.Add(Field);
			mockCollection.Setup(m => m.GetTypeOfElementsFromPK(It.IsAny<ZGuid>())).Returns(typeof(DummyBusinessObject));
			AssertNull("ParentCollection", Field.ParentCollection);

			mockCollection.Setup(m => m.GetTypeOfElementsFromPK(It.IsAny<ZGuid>())).Returns(ExpectedBusinessObjectType);
			AssertEquals("ParentCollection", collection, Field.ParentCollection);
		}

		public void TestRangeValueDecimalPlacesIsSetOnLoad()
		{
			Field.S1_Type = "DEC";
			Field.S1_Precision = 9.3m;
			Factory.Save();
			StmSystemDefinedField loadedField = Factory.Load<StmSystemDefinedField>(Field.PK);
			AssertEquals("RangeValueDecimalPlaces", 3, loadedField.RangeValueDecimalPlaces);
		}

		#region Setting Properties

		public void TestSettingS1_Precision()
		{
			AssertEquals("RangeValueDecimalPlaces", 0, Field.RangeValueDecimalPlaces);

			Field.S1_Type = "DEC";
			Field.S1_Precision = 9m;
			AssertEquals("RangeValueDecimalPlaces", 0, Field.RangeValueDecimalPlaces);

			Field.S1_Precision = 9.1m;
			AssertEquals("RangeValueDecimalPlaces", 1, Field.RangeValueDecimalPlaces);

			Field.S1_Precision = 9.3m;
			AssertEquals("RangeValueDecimalPlaces", 3, Field.RangeValueDecimalPlaces);

			Field.S1_Precision = 9.4m;
			AssertEquals("RangeValueDecimalPlaces", 0, Field.RangeValueDecimalPlaces);

			Field.S1_Type = "INT";
			Field.S1_Precision = 9.3m;
			AssertEquals("RangeValueDecimalPlaces", 0, Field.RangeValueDecimalPlaces);
		}

		public void TestSettingS1_Type()
		{
			Field.S1_Precision = 2.0m;
			Field.S1_LowerValue = 1;
			Field.S1_UpperValue = 2;

			Field.S1_Type = "DEC";
			AssertEquals("S1_Precision", 0m, Field.S1_Precision);
			AssertEquals("S1_LowerValue", 1m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 2m, Field.S1_UpperValue);

			Field.S1_Type = "INT";
			AssertEquals("S1_Precision", 0m, Field.S1_Precision);
			AssertEquals("S1_LowerValue", 1m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 2m, Field.S1_UpperValue);

			Field.S1_Type = "TXT";
			AssertEquals("S1_Precision", 0m, Field.S1_Precision);
			AssertEquals("S1_LowerValue", 1m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 2m, Field.S1_UpperValue);

			Field.S1_Type = "DAT";
			AssertEquals("S1_Precision", 0m, Field.S1_Precision);
			AssertEquals("S1_LowerValue", 0m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 0m, Field.S1_UpperValue);
		}

		public void TestSettingS1_Validation()
		{
			Field.S1_LowerValue = 1m;
			Field.S1_UpperValue = 2m;

			Field.S1_Validation = "RVL";
			AssertEquals("S1_LowerValue", 1m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 2m, Field.S1_UpperValue);

			Field.S1_Validation = "VAL";
			AssertEquals("S1_LowerValue", 0m, Field.S1_LowerValue);
			AssertEquals("S1_UpperValue", 0m, Field.S1_UpperValue);
		}

		#endregion

		#region ZPropertyInfo ReadOnly States

		public void TestS1_PrecisionReadOnly()
		{
			AssertEquals("S1_PrecisionInfo.ReadOnly", true, Field.S1_PrecisionInfo.ReadOnly);

			Field.S1_Type = "DEC";
			AssertEquals("S1_PrecisionInfo.ReadOnly", false, Field.S1_PrecisionInfo.ReadOnly);

			Field.S1_Type = "TXT";
			AssertEquals("S1_PrecisionInfo.ReadOnly", true, Field.S1_PrecisionInfo.ReadOnly);
		}

		public void TestS1_LowerValueReadOnly()
		{
			AssertEquals("S1_LowerValueInfo.ReadOnly", true, Field.S1_LowerValueInfo.ReadOnly);

			Field.S1_Type = "DAT";
			Field.S1_Validation = "RVL";
			AssertEquals("S1_LowerValueInfo.ReadOnly", true, Field.S1_LowerValueInfo.ReadOnly);

			Field.S1_Type = "TXT";
			AssertEquals("S1_LowerValueInfo.ReadOnly", false, Field.S1_LowerValueInfo.ReadOnly);

			Field.S1_Validation = "VAL";
			AssertEquals("S1_LowerValueInfo.ReadOnly", true, Field.S1_LowerValueInfo.ReadOnly);
		}

		public void TestS1_UpperValueReadOnly()
		{
			AssertEquals("S1_UpperValueInfo.ReadOnly", true, Field.S1_UpperValueInfo.ReadOnly);

			Field.S1_Type = "DAT";
			Field.S1_Validation = "RVL";
			AssertEquals("S1_UpperValueInfo.ReadOnly", true, Field.S1_UpperValueInfo.ReadOnly);

			Field.S1_Type = "TXT";
			AssertEquals("S1_UpperValueInfo.ReadOnly", false, Field.S1_UpperValueInfo.ReadOnly);

			Field.S1_Validation = "VAL";
			AssertEquals("S1_UpperValueInfo.ReadOnly", true, Field.S1_UpperValueInfo.ReadOnly);
		}

		#endregion

		#region Implementation

		protected StmSystemDefinedFieldBase Field
		{
			get
			{
				if (fField == null)
				{
					fField = (StmSystemDefinedFieldBase)Factory.New(ExpectedBusinessObjectType);
				}
				return fField;
			}
		}

		protected abstract Type ExpectedValidationType { get; }
		StmSystemDefinedFieldBase fField;

		#endregion
	}
}
