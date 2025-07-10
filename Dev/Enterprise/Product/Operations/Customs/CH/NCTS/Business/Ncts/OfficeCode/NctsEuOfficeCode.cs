using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CH.NCTS.Business;

[SystemDefinedValues]
public class NctsEuOfficeCode : EU.NCTS.Business.NctsEuOfficeCode, Integration.Customs.CH.INctsEuOfficeCode
{
	public new class Schema : EuOfficeCode.Schema
	{
		public const string EstimatedNumberOfDays = nameof(NctsEuOfficeCode.EstimatedNumberOfDays);
	}

	public NctsEuOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusCodeDataValidation GetNewPhase5Validation() => MovementHeader != null ? new NctsEuOfficeCodeDepartureValidation(this) : new NctsEuOfficeCodeValidation(this);

	protected override CusCodeDataValidation GetNewPhase4Validation() => new NctsEuOfficeCodeValidation(this);

	public new NctsDepartureMovementHeader MovementHeader => base.Parent as NctsDepartureMovementHeader;

	[ReadOnlyMember(nameof(IsFixedOrder))]
	public override ZShort CY_Order
	{
		get => base.CY_Order;
		set
		{
			var oldValue = CY_Order;
			base.CY_Order = value;
			if (!IsCopying && oldValue != CY_Order && !IsFixedOrder)
			{
				if (MovementHeader != null)
				{
					MovementHeader.RecalculateCustomsOfficeSequenceWhenRenumbered(this, oldValue, CY_Code);
				}
				else if (ArrivalMovementHeader != null)
				{
					ArrivalMovementHeader.RecalculateCustomsOfficeSequenceWhenRenumbered(this, oldValue, CY_Code);
				}
			}
		}
	}

	public override ZString CY_Code
	{
		get => base.CY_Code;
		set
		{
			var oldValue = CY_Code;
			base.CY_Code = value;
			if (!IsCopying && oldValue != CY_Code)
			{
				ResetEstimatedNumberOfDaysIfNotApplicable();
				SetDefaultValueForSequence();
			}
		}
	}

	public override ZString CY_Data
	{
		get => base.CY_Data;
		set
		{
			var oldValue = CY_Data;
			base.CY_Data = value;
			if (CY_Data != oldValue)
			{
				Header?.MovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	[MaxLength(2)]
	[ReadOnlyMember(nameof(IsEstimatedNumberOfDaysReadOnly))]
	[ResourceStringData("Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode|EstimatedNumberOfDays", Caption = "Estimated Days", ShortCaption = "Est. Days")]
	public ZString EstimatedNumberOfDays
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.EstimatedNumberOfDays);
		set
		{
			CheckMaximumLength(EstimatedNumberOfDaysInfo, value);
			var oldValue = EstimatedNumberOfDays;
			this.SetSystemDefinedValue(Schema.EstimatedNumberOfDays, value);
			if (!IsValidationSuspended)
			{
				if (Validation is NctsEuOfficeCodeDepartureValidation departureValidation)
				{
					departureValidation.ValidateEstimatedNumberOfDays();
				}
				else if (Validation is NctsEuOfficeCodeValidation validation)
				{
					validation.ValidateEstimatedNumberOfDays();
				}
			}
			EstimatedNumberOfDaysInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo EstimatedNumberOfDaysInfo => GetZPropertyInfo(nameof(EstimatedNumberOfDays));

	public bool IsEstimatedNumberOfDaysReadOnly => CY_Code != OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;

	void ResetEstimatedNumberOfDaysIfNotApplicable()
	{
		if (IsEstimatedNumberOfDaysReadOnly)
		{
			EstimatedNumberOfDays = ZString.Empty;
		}
	}

	void SetDefaultValueForSequence()
	{
		if (HasDefaultValue)
		{
			CY_Order = 1;
		}
	}

	bool IsFixedOrder => !editableCustomsOfficesCodes.ToList().Contains(CY_Code);
	bool HasDefaultValue => defaultedCustomsOfficesCodes.ToList().Contains(CY_Code);

	readonly string[] defaultedCustomsOfficesCodes = new string[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination };

	readonly string[] editableCustomsOfficesCodes = new string[] { OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit };
}
