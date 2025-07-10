using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AE;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.AE.Business;

public class JobDeclaration : TypeSafeJobDeclaration, IStatusNeedsRecalculationProvider, Integration.Customs.AE.IJobDeclaration
{
	public static new JobDeclaration New(BusinessObjectFactory factory)
	{
		return factory.New<JobDeclaration>();
	}

	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region Schema
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
	public new class Schema : TypeSafeJobDeclaration.Schema
	{
		public const string JE_Calc_CustomsCode = "JE_Calc_CustomsCode";
		public const string JE_Calc_LegacyCode = "JE_Calc_LegacyCode";
		public const string JE_Calc_ImporterBankAccount = "JE_Calc_ImporterBankAccount";
		public const string TypeOfGoods = "TypeOfGoods";
	}
	#endregion

	public const string CustomsCodeFreeZoneSuffix = "FZ";

	#region Boolean properties

	protected override bool IsHouseBillMandatory
	{
		get { return true; }
	}

	protected override bool IsCustomsHeaderAmendmentATotalReplacement
	{
		get { return false; }
	}

	protected override bool IsCustomsLineAmendmentATotalReplacement
	{
		get { return false; }
	}

	protected override bool SupportJE_PaymentMethodUsageCore
	{
		get { return true; }
	}

	public bool IsTranshipment
	{
		get { return JE_MessageType == AEJobMessageTypeList.Codes.Transfer; }
	}

	public bool IsTransit
	{
		get { return JE_MessageType == AEJobMessageTypeList.Codes.Transit; }
	}

	public override ZBool AreMultipleEntryInstructionsAllowed => false;

	public bool IsApplicationCodeDubai
	{
		get { return JE_ApplicationCode == AEDeclarationApplicationCodeList.Codes.Dubai; }
	}

	#endregion

	public override ZString JE_RL_NKPortOfArrival
	{
		get { return base.JE_RL_NKPortOfArrival; }
		set
		{
			bool hasChanged = JE_RL_NKPortOfArrival != value;
			base.JE_RL_NKPortOfArrival = value;
			if (hasChanged)
			{
				JE_RL_NKPortOfFirstArrival = value;
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.AE.Business.JobDeclaration|JE_TotalNoOfPiecesOverride", Caption = "Units")]
	public override ZInt JE_TotalNoOfPieces
	{
		get => base.JE_TotalNoOfPieces;
		set => base.JE_TotalNoOfPieces = value;
	}

	protected override ZBool IsReciprocalRatesCore
	{
		get { return IsReciprocalRatesConstant; }
	}

	internal static bool IsReciprocalRatesConstant
	{
		get { return true; }
	}

	protected override ZString LocalCurrencyCodeCore
	{
		get { return LocalCurrencyConstantCode; }
	}

	internal static ZString LocalCurrencyConstantCode
	{
		get { return Enterprise.Core.Constants.CurrencyCodes.UnitedArabEmirates; }
	}

	[List(nameof(Lookups) + "." + nameof(Lookups.PlaceOfDischargeList))]
	public override ZString JE_PlaceOfDischarge
	{
		get { return base.JE_PlaceOfDischarge; }
		set { base.JE_PlaceOfDischarge = value; }
	}

	[List(nameof(Lookups) + "." + nameof(Lookups.ExitPointList))]
	public override ZString JE_ExitPoint
	{
		get { return base.JE_ExitPoint; }
		set { base.JE_ExitPoint = value; }
	}

	[List(nameof(Lookups) + "." + nameof(Lookups.TypeOfGoodsList))]
	public override ZString JE_TypeOfGoods
	{
		get { return base.JE_TypeOfGoods; }
		set
		{
			bool hasChanged = base.JE_TypeOfGoods != value;
			base.JE_TypeOfGoods = value;
			if (hasChanged && !IsMarkingAsNeedingValidationSuspended && !IsCopying)
			{
				MarkAsNeedingValidation();
				Invoices.MarkAsNeedingValidation();
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(Lookups.ClearanceLocationList))]
	public override ZString JE_ClearanceLocation
	{
		get { return base.JE_ClearanceLocation; }
		set { base.JE_ClearanceLocation = value; }
	}

	public override ZGuid JE_OH_Importer
	{
		get { return base.JE_OH_Importer; }
		set
		{
			bool hasChanged = base.JE_OH_Importer != value;
			base.JE_OH_Importer = value;
			if (hasChanged)
			{
				Invoices.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid JE_OH_Supplier
	{
		get { return base.JE_OH_Supplier; }
		set
		{
			bool hasChanged = base.JE_OH_Supplier != value;
			base.JE_OH_Supplier = value;
			if (hasChanged)
			{
				Invoices.MarkAsNeedingValidation();
			}
		}
	}

	#region New Properties

	[MaxLength(4)]
	public ZString TypeOfGoods
	{
		get
		{
			ZString result = new TypeOfGoodsList().GetDescriptionFromCode(JE_TypeOfGoods);
			return result.Left(4);
		}
	}

	public ZPropertyInfo TypeOfGoodsInfo
	{
		get { return GetZPropertyInfo(Schema.TypeOfGoods); }
	}

	[MaxLength(20)]
	public ZString JE_Calc_ImporterBankAccount
	{
		get { return (Importer != null) ? Importer.MiscServ.OM_IMEFTBankAccount : ZString.Empty; }
	}

	public ZPropertyInfo JE_Calc_ImporterBankAccountInfo
	{
		get { return GetZPropertyInfo(Schema.JE_Calc_ImporterBankAccount); }
	}

	[MaxLength(10)]
	public ZString JE_Calc_LegacyCode
	{
		get { return (Consignee != null) ? Consignee.LegacyCode : ZString.Empty; }
	}

	public ZPropertyInfo JE_Calc_LegacyCodeInfo
	{
		get { return GetZPropertyInfo(Schema.JE_Calc_LegacyCode); }
	}

	[MaxLength(10)]
	public ZString JE_Calc_CustomsCode
	{
		get { return (Consignee != null) ? Consignee.LocalCustomsClientCode : ZString.Empty; }
	}

	public ZPropertyInfo JE_Calc_CustomsCodeInfo
	{
		get { return GetZPropertyInfo(Schema.JE_Calc_CustomsCode); }
	}

	#endregion

	#region Default Values

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		using (SuspendMarkingAsNeedingValidation())
		{
			JE_MessageType = JobMessageTypeList.Codes.Import;
			JE_MessageSubType = ZString.Empty;
			JE_TypeOfGoods = TypeOfGoodsList.Codes.LowValueBelowDeminimis;
			JE_PaymentMethod = PaymentByList.Codes.CouriersAccount;
			JE_ExitPoint = AEConstants.ExitPointAirportFreeZone;
			JE_ClearanceLocation = AEConstants.ClearanceLocationAirportFreeZone;
			JE_PlaceOfDischarge = AEConstants.PlaceOfDischargeAirportFreeZone;
		}
	}

	protected override ZString DefaultDataGroupingCore => IsInterface ? base.DefaultDataGroupingCore : JE_ApplicationCode;

	protected override ZString DefaultDataGroupingForTariffsCore => AEConstants.DefaultDataGroupingForTariffs;

	protected override ZString DefaultDataGroupingForCusProcedureCore => DefaultDataGroupingCore;

	#endregion

	#region IsHighValue
	public ZBool IsHighValue
	{
		get { return JE_TypeOfGoods == TypeOfGoodsList.Codes.HighValueAboveDeminimis; }
	}
	#endregion

	#region IDocumentSupport Members

	protected override DocumentSupporter CreateNewDocumentSupporter()
	{
		return new JobDeclarationDocumentSupporter(this);
	}

	#endregion

	#region IStatusNeedsRecalculationProvider Members
	public bool StatusNeedsRecalculation
	{
		get { return !IsDeleted && HasChanges; }
	}
	#endregion

	#region HouseBills

	[ChildEditable(true)]
	public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

	protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

	#endregion

	protected override bool SupportMultipleBuiltInTypes => true;

	protected override string SubmissionTypeBuiltinCode => AEDeclarationApplicationCodeList.Codes.Dubai;

	protected override bool IsBuiltinSubmissionType(string type)
	{
		return new AEDeclarationApplicationCodeList().ContainsCode(type);
	}
}
