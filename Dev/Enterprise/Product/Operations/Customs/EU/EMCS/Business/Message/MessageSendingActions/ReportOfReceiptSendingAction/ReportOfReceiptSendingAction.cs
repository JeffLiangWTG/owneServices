using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReportOfReceiptSendingAction : EMCSMessageSendingAction, IObsoleteValidation, IReportOfReceipt
	{
		public ReportOfReceiptSendingAction(EMCSJobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
			JobDeclaration = jobDeclaration;
		}

		public readonly EMCSJobDeclaration JobDeclaration;

		public static class Schema
		{
			public const string ArrivalDate = "ArrivalDate";
			public const string ReceiptResult = "ReceiptResult";
			public const string ComplementaryInformation = "ComplementaryInformation";

			public const int ComplementaryInformationMaxLength = 350;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("EE9E8AD5-7A6B-4586-8471-818666059BB1", "Report of Receipt");

		[ResourceStringData("045B9B4F-B05C-497C-BC19-60743B68D04B", Caption = "Arrival Date")]
		public ZDateTime ArrivalDate
		{
			get => arrivalDate;
			set
			{
				if (value != arrivalDate)
				{
					SetNonPersistentPropertyValue(ArrivalDateInfo, ref arrivalDate, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateArrivalDate();
					}
				}
			}
		}
		ZDateTime arrivalDate;
		public ZPropertyInfo ArrivalDateInfo => GetZPropertyInfo(Schema.ArrivalDate);

		[List(nameof(Lookups) + "." + nameof(ReportOfReceiptSendingActionLookups.ReceiptResultList))]
		[ResourceStringData("415F0A90-3A64-432A-9ADB-B9AB83EFDD00", Caption = "Receipt Result")]
		public ZString ReceiptResult
		{
			get => receiptResult;
			set
			{
				if (value != receiptResult)
				{
					SetNonPersistentPropertyValue(ReceiptResultInfo, ref receiptResult, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateReceiptResult();
					}
				}
			}
		}
		ZString receiptResult;
		public ZPropertyInfo ReceiptResultInfo => GetZPropertyInfo(Schema.ReceiptResult);

		[MaxLength(Schema.ComplementaryInformationMaxLength)]
		[ResourceStringData("EB88296F-4285-4D27-973D-EE33803EFC14", Caption = "Complementary Information")]
		public ZString ComplementaryInformation
		{
			get => complementaryInformation;
			set
			{
				if (value != complementaryInformation)
				{
					SetNonPersistentPropertyValue(ComplementaryInformationInfo, ref complementaryInformation, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateComplementaryInformation();
					}
				}
			}
		}
		ZString complementaryInformation;
		public ZPropertyInfo ComplementaryInformationInfo => GetZPropertyInfo(Schema.ComplementaryInformation);

		public ReportOfReceiptSendingActionLookups Lookups => lookups ?? (lookups = GetNewLookups());
		ReportOfReceiptSendingActionLookups lookups;
		public ReportOfReceiptSendingActionLookups GetNewLookups() => new ReportOfReceiptSendingActionLookups(this);

		#region Validation

		public ReportOfReceiptSendingActionValidation Validation => new ReportOfReceiptSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
