using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeParsingDiagnosticsValidation : ZValidation
	{
		public BarcodeParsingDiagnosticsValidation(BarcodeParsingDiagnostics barcodeParsingDiagnostic)
			: base(barcodeParsingDiagnostic)
		{
		}

		#region ValidateBuyerPK

		public void ValidateBuyerPK() => ValidateCalculatedProperty(Parent.BuyerPKInfo);

		#endregion

		#region ValidateSupplierPK

		public void ValidateSupplierPK() => ValidateCalculatedProperty(Parent.SupplierPKInfo);

		protected void CheckBuyerPK() => TypeValidation.CheckValidGuid(Parent.BuyerPKInfo);

		protected void CheckSupplierPK() => TypeValidation.CheckValidGuid(Parent.SupplierPKInfo);

		#endregion

		#region ValidateRelatedEntityPK

		public void ValidateRelatedEntityPK() => ValidateCalculatedProperty(Parent.RelatedEntityPKInfo);

		protected void CheckRelatedEntityPK() => TypeValidation.CheckValidGuid(Parent.RelatedEntityPKInfo);

		#endregion

		#region ValidateTargetField

		public void ValidateTargetField()
		{
			if (Parent.DiagnosticsType == DiagnosticsTypes.Codes.Validation)
			{
				ValidateCalculatedProperty(Parent.TargetFieldInfo);
			}
		}

		protected void CheckTargetField()
		{
			MandatoryValidation.CheckEntered(Parent.TargetFieldInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TargetFieldInfo);
		}

		#endregion

		#region ValidateDiagnosticsType

		public void ValidateDiagnosticsType() => ValidateCalculatedProperty(Parent.DiagnosticsTypeInfo);

		protected void CheckDiagnosticsType()
		{
			MandatoryValidation.CheckEntered(Parent.DiagnosticsTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DiagnosticsTypeInfo);
		}

		#endregion

		#region ValidateModuleCode

		public void ValidateModuleCode() => ValidateCalculatedProperty(Parent.ModuleCodeInfo);

		protected void CheckModuleCode()
		{
			MandatoryValidation.CheckEntered(Parent.ModuleCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ModuleCodeInfo);
			if (!Parent.ModuleCodeInfo.HasErrors() && Parent.DiagnosticsType == DiagnosticsTypes.Codes.Validation && Parent.ModuleCode != BarcodeModuleTypes.Codes.Warehouse)
			{
				Parent.ModuleCodeInfo.AddError(Res.GetString("DBED68EB-F513-4CE2-9E5E-AB7E46CE1C14", "This module does not support validation rules."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateBuyerPK();
			ValidateSupplierPK();
			ValidateRelatedEntityPK();
			ValidateTargetField();
			ValidateModuleCode();
			ValidateDiagnosticsType();
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType => typeof(BarcodeParsingDiagnosticsValidation);

		#endregion

		#region Diagnostics

		BarcodeParsingDiagnostics Parent => (BarcodeParsingDiagnostics)ParentFilter;

		#endregion
	}
}
