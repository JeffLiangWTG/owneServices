using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSupportingDocument : EU.NCTS.Business.NctsSupportingDocument, ISupportingDocument, ICusSupportingInfoWithYearOfIssue
{
	public NctsSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Schema : EU.NCTS.Business.NctsSupportingDocument.Schema
	{
		public const int CSI_QuantityDecimalPlaces = 5;
		public const string CSI_YearOfIssue = "CSI_YearOfIssue";
		public const int CSI_YearOfIssueMaxLength = 4;
	}

	protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsSupportingDocumentPhase4Lookups(this);

	protected override CusSupportingInfoLookups GetNewPhase5Lookups() => new NctsSupportingDocumentPhase5Lookups(this);

	protected override CusSupportingInfoValidation GetNewPhase5Validation() => IsPhase5Departure ? new NctsSupportingDocumentPhase5DepartureValidation(this) : new NctsSupportingDocumentValidation(this);

	protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsSupportingDocumentValidation(this);

	#region CSI_Quantity

	[DecimalPlaces(Schema.CSI_QuantityDecimalPlaces)]
	public override ZDecimal CSI_Quantity { get => base.CSI_Quantity; set => base.CSI_Quantity = value; }

	#endregion

	#region CSI_YearOfIssue

	[MaxLength(Schema.CSI_YearOfIssueMaxLength)]
	[ResourceStringData("Enterprise.Customs.IT.NCTS.Business.NctsSupportingDocument|CSI_YearOfIssue", Caption = "Year of Issue", ShortCaption = "Year")]
	[BusinessObjectTestExclude]
	public ZString CSI_YearOfIssue
	{
		get => CSI_DateOfIssue.ToYearDateString();
		set => this.SetYearOfIssue(value);
	}

	public ZPropertyInfo CSI_YearOfIssueInfo => GetWrappedZPropertyInfo(Schema.CSI_YearOfIssue, (x) => CSI_DateOfIssueInfo);

	#endregion

	#region CSI_Code

	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (!IsCopying && !IsValidationSuspended && oldValue != CSI_Code)
			{
				Validation.ValidateCSI_RN_NKCountryCode();
				Validation.ValidateCSI_DateOfIssue();
				Validation.ValidateCSI_Quantity();
				Validation.ValidateCSI_UnitOfQuantity();
			}
		}
	}

	#endregion

	#region CSI_DateOfIssue

	[BusinessObjectTestExclude]
	public override ZDateTime CSI_DateOfIssue
	{
		get => base.CSI_DateOfIssue;
		set => base.CSI_DateOfIssue = value.BackwardDateToFirstDayOfYear();
	}

	#endregion

	protected override ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider()
	{
		if (IsPhase5Departure)
		{
			return new NctsSupportingDocumentNcts5DepartureReadOnlyConditionsProvider();
		}

		return base.GetNewReadOnlyProvider();
	}

	protected override ZZRefCusCodeListCombined GetRefCusCodeCore()
	{
		if (IsPhase5Departure)
		{
			return GetRefCusCodeForPhase5();
		}

		return base.GetRefCusCodeCore();
	}

	ZZRefCusCodeListCombined GetRefCusCodeForPhase5() => Lookups.TypeCodeList
		.Cast<ZZRefCusCodeListCombined>()
		.SingleOrDefault(c => c.ZZD_Code == CSI_Code);

	#region ISupportingDocument members

	ZString ISupportingDocument.Type => CSI_Code;

	ZString ISupportingDocument.CountryOfIssue => CSI_RN_NKCountryCode;

	ZString ISupportingDocument.YearOfIssue => CSI_DateOfIssue.ToYearDateString();

	ZString ISupportingDocument.ReferenceNumber => CSI_ReferenceNumber;

	ZDecimal ISupportingDocument.Quantity => CSI_Quantity;

	ZString ISupportingDocument.UnitOfQuantity => CSI_UnitOfQuantity;

	ZString ISupportingDocument.Status => CSI_Status;

	#endregion
}
