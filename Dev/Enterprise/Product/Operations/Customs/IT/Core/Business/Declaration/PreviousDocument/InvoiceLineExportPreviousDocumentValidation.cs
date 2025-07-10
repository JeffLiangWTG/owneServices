using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class InvoiceLineExportPreviousDocumentValidation : PreviousDocumentValidation
{
	public InvoiceLineExportPreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
		this.invoiceLine = Argument.NotNull(parent.Parent as JobComInvoiceLine, nameof(JobComInvoiceLine));
	}
	readonly JobComInvoiceLine invoiceLine;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateNumberOfPreviousDocumentsAllowed();
	}

	protected override void CheckCSI_PackType()
	{
		base.CheckCSI_PackType();

		if (IsUcc6Export)
		{
			ValidateIfPackageTypeIsRequired();
		}
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		if (IsUcc6Export)
		{
			ValidateIfUnitOfQuantityIsRequired();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
			return;
		}

		base.CheckCSI_UnitOfQuantity();
	}

	protected override void CheckCSI_ItemNumber()
	{
		base.CheckCSI_ItemNumber();
		if (IsUcc6Export)
		{
			ValidateIfItemNumberIsRequired();
		}
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		var parent = Parent;

		if (IsUcc6Export
			&& parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod
			&& !parent.CSI_ReferenceNumber.IsEmpty
			&& parent.CSI_ReferenceNumber.Length > 35)
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.ReferenceNumberCanHaveOnly35Characters);
		}
	}

	protected override IPreviousDocumentReferenceNumberValidator GetReferenceNumberValidator()
	{
		var parent = Parent;
		return IsUcc6Export
			? new Ucc6ExportPreviousDocumentReferenceNumberValidator(parent.Factory, parent)
			: base.GetReferenceNumberValidator();
	}

	protected override void CheckCSI_Procedure()
	{
		if (IsUcc6Export)
		{
			return;
		}

		base.CheckCSI_Procedure();
	}

	protected override void CheckCSI_SubType()
	{
		if (IsUcc6Export)
		{
			return;
		}

		base.CheckCSI_SubType();
	}

	protected override void CheckCSI_UnitOfQuantity3()
	{
		if (IsUcc6Export)
		{
			return;
		}

		base.CheckCSI_UnitOfQuantity3();
	}

	internal void ValidateNumberOfPreviousDocumentsAllowed()
	{
		var parent = Parent;
		var isMergeDone = parent.Declaration?.IsMergeDone ?? false;
		var entryLine = invoiceLine.CusEntryLine;

		if (!IsUcc6Export || !isMergeDone || entryLine == null)
		{
			return;
		}

		var isInTransitionPeriod = parent.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod;
		var maxAllowedDocuments = isInTransitionPeriod ? 9 : 99;
		var messageError = isInTransitionPeriod
			? ValidationCaptions.InvoiceLinePreviousDocuments.Max9DocumentsAreAllowedInTransitionPeriod
			: ValidationCaptions.InvoiceLinePreviousDocuments.Max99DocumentsAreAllowed;

		if (entryLine.PreviousDocuments.IsCountMoreThan(maxAllowedDocuments))
		{
			parent.AddRowMessageError(messageError);
		}
	}

	#region Implementation

	void ValidateIfItemNumberIsRequired()
	{
		var parent = Parent;
		if (parent.CSI_Code.IsEmpty || !parent.CSI_ItemNumber.IsEmpty)
		{
			return;
		}

		var codesMandatingItemNumber = new ZString[]
		{
			UniversalReferenceConstants.SupportingDocumentTypes.C651,
			UniversalReferenceConstants.SupportingDocumentTypes.C658
		};

		if (parent.CSI_Code.In(codesMandatingItemNumber))
		{
			parent.CSI_ItemNumberInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.ItemNumberIsRequiredForC651OrC658Codes);
		}
	}

	void ValidateIfUnitOfQuantityIsRequired()
	{
		var parent = Parent;
		var unitOfQuantityInfo = parent.CSI_UnitOfQuantityInfo;
		var isUnitOfQuantityPresent = !parent.CSI_UnitOfQuantity.IsEmpty;
		var isQuantityPresent = !parent.CSI_Quantity.IsEmpty;

		if (!isQuantityPresent && isUnitOfQuantityPresent)
		{
			unitOfQuantityInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.UnitOfMeasureRequiredOnlyIfQuantityIsProvided);
		}

		if (!isUnitOfQuantityPresent && isQuantityPresent)
		{
			unitOfQuantityInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.UnitOfQuantityIsRequireForNonZeroQuantity);
		}
	}

	void ValidateIfPackageTypeIsRequired()
	{
		var parent = Parent;
		var packageTypeInfo = parent.CSI_PackTypeInfo;
		var isPackageTypePresent = !parent.CSI_PackType.IsEmpty;
		var isPackageQtyProvided = !parent.CSI_PackQty.IsEmpty;

		if (!isPackageTypePresent && isPackageQtyProvided)
		{
			packageTypeInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.PackageTypeRequiredWhenNumberOfItemsArePresent);
		}

		if (!isPackageQtyProvided && isPackageTypePresent && !DoesPackageHaveBulkAttribute(parent.CSI_PackType))
		{
			packageTypeInfo.AddMessageError(ValidationCaptions.InvoiceLinePreviousDocuments.PackageTypeRequiredForNonBulkPkgTypeAndForNonZeroQuantity);
		}
	}

	bool DoesPackageHaveBulkAttribute(ZString packageType)
	{
		var packageUnitsWithBulkAttribute = GetBulkPackageUnitTypeList();
		return packageUnitsWithBulkAttribute.ContainsCode(packageType);
	}

	CodeDescriptionPairList GetBulkPackageUnitTypeList()
	{
		return Universal.RefCusCodeListTypes.GetCachedListMatchAnyAttributes(Parent.Factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			EU.Business.UniversalReferenceConstants.UNPackTypeStartDate,
			attributeNameValuePairs: new[] { new KeyValuePair<ZString, ZString>(Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, ZString.Empty) }.ToArray());
	}

	bool IsUcc6Export => Parent?.Declaration?.IsUCC6AndIsExport ?? false;

	#endregion
}
