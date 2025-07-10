using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryLineVINData))]
	class EntryLineVINDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_LineNumber = 2;
			var qualification = Factory.New<CIQProductQualification>();
			qualification.CSI_Code = "408";
			qualification.CSI_ReferenceNumber = "001";
			qualification.CSI_LineNo = 1;
			qualification.CSI_Quantity = 1;
			qualification.CSI_UnitOfQuantity = "010";
			var vinData = Factory.New<VINData>();
			vinData.XC_VIN = "VIN123";
			vinData.XC_ChassisNo = "ChassisNo123";
			vinData.XC_EngineNo = "EngineNo123";
			vinData.XC_ModelEN = "Model";
			vinData.XC_ProductNameCN = "品名";
			vinData.XC_ProductNameEN = "Product Name";
			vinData.XC_QGP = "质保期";
			var entryLineProductQualification = new EntryLineProductQualification(entryLine, new[] { qualification }, 3);
			var entryLineVIN = new EntryLineVINData(entryLineProductQualification, vinData);
			AssertEquals("EntryLineNo", (ZShort)2, entryLineVIN.EntryLineNo);
			AssertEquals("ProductQualificationSequence", (ZInt)3, entryLineVIN.ProductQualificationSequence);
			AssertEquals("VIN", "VIN123", entryLineVIN.VIN);
			AssertEquals("ChassisNo", "ChassisNo123", entryLineVIN.ChassisNo);
			AssertEquals("EngineNo", "EngineNo123", entryLineVIN.EngineNo);
			AssertEquals("ModelEN", "Model", entryLineVIN.ModelEN);
			AssertEquals("ProductNameCN", "品名", entryLineVIN.ProductNameCN);
			AssertEquals("ProductNameEN", "Product Name", entryLineVIN.ProductNameEN);
			AssertEquals("QGP", "质保期", entryLineVIN.QGP);
		}

		protected override BusinessObject GetNewBusinessObject() => new EntryLineVINData(new EntryLineProductQualification(Factory.New<CusEntryLine>(), new[] { Factory.New<CIQProductQualification>() }, 1), Factory.New<VINData>());
	}
}
