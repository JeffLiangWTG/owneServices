using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeParsingDiagnosticsValidationTest : BusinessObjectValidationTestCase
	{
		#region TestGuidColumnValidations

		public void TestGuidColumnValidations()
		{
			AssertGuidValidation<OrgHeader>("BuyerPK", "BuyerPKInfo");
			AssertGuidValidation<OrgHeader>("SupplierPK", "SupplierPKInfo");
			AssertGuidValidation<OrgSupplierPart>("RelatedEntityPK", "RelatedEntityPKInfo");
		}

		void AssertGuidValidation<T>(string propertyName, string propertyInfo) where T : BusinessObject
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var bizO = Factory.New<T>();
			AssertNoErrors((ZPropertyInfo)diagnostic[propertyInfo]);

			diagnostic[propertyName] = bizO.PK;
			AssertNoErrors((ZPropertyInfo)diagnostic[propertyInfo]);

			diagnostic[propertyName] = ZGuid.Invalid;
			AssertHasErrors((ZPropertyInfo)diagnostic[propertyInfo]);
		}

		#endregion

		#region TestModuleCode

		public void TestModuleCode()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.ModuleCode = "";
			AssertHasErrors(diagnostic.ModuleCodeInfo);

			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			AssertNoErrors(diagnostic.ModuleCodeInfo);

			diagnostic.ModuleCode = "XXX";
			AssertHasErrors(diagnostic.ModuleCodeInfo);
		}

		public void TestModuleCode_WhenDiagnosticsTypeIsValidationAndModuleIsWarehouse()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;

			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
			AssertNoErrors(diagnostic.ModuleCodeInfo);
		}

		public void TestModuleCode_WhenDiagnosticsTypeIsValidation_ModuleIsNotWarehouse()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
			AssertNoErrors(diagnostic.ModuleCodeInfo);

			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.ETail;
			AssertHasErrors("This module does not support validation rules.", diagnostic.ModuleCodeInfo);
		}

		public void TestModuleCode_ModuleIsNotWarehouse_DiagnosticsTypeIsValidation()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Parsing;
			diagnostic.ModuleCode = BarcodeModuleTypes.Codes.ETail;
			AssertNoErrors(diagnostic.ModuleCodeInfo);

			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
			AssertHasErrors("This module does not support validation rules.", diagnostic.ModuleCodeInfo);
		}

		#endregion

		#region TestDiagnosticsType

		public void TestDiagnosticsType_ValidValue()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Parsing;
			AssertNoErrors(diagnostic.DiagnosticsTypeInfo);
		}

		public void TestDiagnosticsType_InvalidValue()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = "";
			AssertHasErrors(diagnostic.DiagnosticsTypeInfo);

			diagnostic.DiagnosticsType = "XXX";
			AssertHasErrors(diagnostic.DiagnosticsTypeInfo);
		}

		#endregion

		#region TestTargetField

		public void TestTargetField_ValidValue()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;

			diagnostic.TargetField = nameof(WarehouseTargetField.PRC);
			AssertNoErrors(diagnostic.DiagnosticsTypeInfo);
		}

		public void TestTargetField_InvalidValue()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;

			diagnostic.TargetField = "";
			AssertHasErrors(diagnostic.TargetFieldInfo);

			diagnostic.TargetField = "XXX";
			AssertHasErrors(diagnostic.TargetFieldInfo);
		}

		#endregion
	}
}
