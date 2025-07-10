using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class MissingSupportingDocument
{
	public MissingSupportingDocument(RefCusConditionValue conditionValue, JobComInvoiceLine invoiceLine)
	{
		this.conditionValue = Argument.NotNull(conditionValue, nameof(conditionValue));
		Argument.NotNull(conditionValue.Condition, nameof(conditionValue));
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}
	readonly RefCusConditionValue conditionValue;
	readonly JobComInvoiceLine invoiceLine;

	public ZBool ShouldImport { get; set; }

	public ZString Code => conditionValue.ZX3_Value;

	public ZString Description => description ?? (description = RefCusCodeListCodeDescriptionPairList.GetDescriptionFromCode(Code));
	string description;

	public ZString CodeAndDescription => Code + (Description.IsEmpty ? "" : FormattableString.Invariant($" - {Description}"));

	CodeDescriptionPairList RefCusCodeListCodeDescriptionPairList => refCusCodeListCodeDescriptionPairList ?? (refCusCodeListCodeDescriptionPairList = GetRefCusCodeListCodeDescriptionPairList());
	CodeDescriptionPairList refCusCodeListCodeDescriptionPairList;

	CodeDescriptionPairList GetRefCusCodeListCodeDescriptionPairList()
	{
		var codeType = GetRefCusCodeListCodeType();
		var refCusCodeListCodeDescriptionPairList = codeType.IsEmpty ? new CodeDescriptionPairList() : RefCusCodeListTypes.GetCachedList(invoiceLine.Factory, Core.Constants.CountryCodes.Italy, codeType, ZDateTime.Today);
		return refCusCodeListCodeDescriptionPairList;
	}

	ZString GetRefCusCodeListCodeType()
	{
		var declaration = invoiceLine.Declaration;

		string refCusCodeListCodeType = ZString.Empty;
		if (declaration?.IsImport ?? ZBool.False)
		{
			refCusCodeListCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		}
		else if (declaration?.IsExport ?? ZBool.False)
		{
			refCusCodeListCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		}
		return refCusCodeListCodeType;
	}
}
