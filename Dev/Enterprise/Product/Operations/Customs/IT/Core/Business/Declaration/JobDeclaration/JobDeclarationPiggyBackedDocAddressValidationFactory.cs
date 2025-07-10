using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationPiggyBackedDocAddressValidationFactory
{
	public JobDeclarationPiggyBackedDocAddressValidationFactory(JobDeclaration declaration, Func<JobDocAddress, ZValidation> fallbackPiggyBackedDocAddressValidationFunc)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
		this.fallbackPiggyBackedDocAddressValidationFunc = fallbackPiggyBackedDocAddressValidationFunc;
	}

	readonly JobDeclaration declaration;
	readonly Func<JobDocAddress, ZValidation> fallbackPiggyBackedDocAddressValidationFunc;

	public ZValidation GetNewPiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		Argument.NotNull(addressToValidate, nameof(addressToValidate));
		switch (addressToValidate.E2_AddressType)
		{
			case DocAddressTypes.Codes.ImporterDocumentaryAddress:
				return GetImporterAddressValidation(addressToValidate);

			case DocAddressTypes.Codes.SupplierDocumentaryAddress:
				return GetSupplierAddressValidation(addressToValidate);

			case DocAddressTypes.Codes.Exporter:
				return GetExporterAddressValidation(addressToValidate);
		}

		return GetFallbackPiggyBackedDocAddressValidation(addressToValidate);
	}

	#region Implementation

	ZValidation GetSupplierAddressValidation(JobDocAddress addressToValidate)
	{
		if (IsUcc6Export)
		{
			return new Ucc6ExportSupplierJobDocAddressValidation(addressToValidate, declaration);
		}

		var isMandatory = IsImportAndAnyEntryInstructionIsNonWarehouseProcedure() || IsExportAndAnyEntryInstructionIsNotBuyersConsol();
		return new TraderJobDocAddressValidation(addressToValidate, declaration.JE_OA_SupplierAddressInfo.HumanReadableName, declaration, isMandatory);
	}

	ZValidation GetImporterAddressValidation(JobDocAddress addressToValidate)
	{
		var importerName = declaration.JE_OA_ImporterAddressInfo.HumanReadableName;

		if (IsImport)
		{
			return new RequiringEoriUcc6TraderJobDocAddressValidation(addressToValidate, importerName, declaration);
		}

		if (IsExport)
		{
			if (IsUCC6)
			{
				return new Ucc6ExportImporterJobDocAddressValidation(addressToValidate, declaration);
			}
			return new TraderJobDocAddressValidation(addressToValidate, importerName, declaration);
		}

		return null;
	}

	ZValidation GetExporterAddressValidation(JobDocAddress addressToValidate)
	{
		if (IsUcc6Export)
		{
			return new RequiringEoriUcc6TraderJobDocAddressValidation(addressToValidate, declaration.JE_OH_ExporterInfo.HumanReadableName, declaration, HasInstructionStyleEmptyOrNotC2);
		}
		return GetFallbackPiggyBackedDocAddressValidation(addressToValidate);
	}

	ZValidation GetFallbackPiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		if (fallbackPiggyBackedDocAddressValidationFunc != null)
		{
			return fallbackPiggyBackedDocAddressValidationFunc(addressToValidate);
		}
		return null;
	}

	bool IsImportAndAnyEntryInstructionIsNonWarehouseProcedure() => IsImport && EntryInstructions.AnyEntryInstructionIsNonWarehouseProcedure();

	bool IsExportAndAnyEntryInstructionIsNotBuyersConsol()
	{
		return IsExport && EntryInstructions
			.Any(x => !x.IsBuyersConsol);
	}

	IEnumerable<CusEntryInstruction> EntryInstructions => declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>();

	ZBool IsImport => declaration.IsImport;
	ZBool IsExport => declaration.IsExport;
	ZBool IsUCC6 => declaration.IsUCC6;
	ZBool IsUcc6Export => declaration.IsUCC6AndIsExport;

	bool HasInstructionStyleEmptyOrNotC2 => EntryInstructions
		.Any(x => x.CEI_Style.IsEmpty || x.CEI_Style != ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2);

	#endregion
}
