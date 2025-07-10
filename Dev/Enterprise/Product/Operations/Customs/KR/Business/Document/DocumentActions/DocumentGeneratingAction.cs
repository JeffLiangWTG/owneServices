using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class DocumentGeneratingAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		class Schema
		{
			public const string ToBeDelivered = "ToBeDelivered";
		}

		public DocumentGeneratingAction(CusEntryHeader entry, ZString entryNum, string fullMenuName)
		{
			this.Entry = entry;
			this.entryNum = entryNum;
			this.fullMenuName = fullMenuName;
			this.entryNumStatus = GetEntryNumStatus();
			this.statusList = GetStatusList();
			individualStatements = entry.IndividualStatements;
			this.IncludingCurrentDifference = HasNotAmendment;
		}

		readonly public CusEntryHeader Entry;
		readonly ZString entryNum;
		readonly string fullMenuName;
		readonly ZString entryNumStatus;
		readonly IStatusList statusList;
		readonly CusStatementHeader[] individualStatements;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		public readonly string VATDefermentConflictErrorMessage = Res.GetString("F5DEBBB5-43DC-4E1C-B9A3-916BDAAD0CD0", "System cannot produce this document because Payer's VAT Deferral is not confirmed. Please press F3 in the Payer Organization field and set the VAT Deferral configuration on the Organization form > Details > Config > South Korea.");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		public readonly string MissingPayerErrorMessage = Res.GetString("9031F9A3-2188-4754-BA7F-C41BC6E316B9", "There is no payer entered for this declaration. Please enter a payer. Their VAT Deferral is used for printing this document.");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		public readonly string MultipleInvoicesMessage = Res.GetString("F06A2ED6-7041-4530-8909-1DC1B83457B6", "Multiple Invoices");

		public ZString StatusDescription => statusList?.GetDescriptionFromCode(entryNumStatus) ?? string.Empty;
		public ZString EntryNumber => entryNum;

		public ZBool ToBeDelivered
		{
			get => toBeDelivered;
			set
			{
				SetNonPersistentPropertyValue(ToBeDeliveredInfo, ref toBeDelivered, value);
				ValidateToBeDelivered();
			}
		}
		ZBool toBeDelivered;

		public ZPropertyInfo ToBeDeliveredInfo => GetZPropertyInfo(Schema.ToBeDelivered);
		[ResourceStringData("D35B7C2D-2DCE-4558-ACFC-ACDBCC1C318C", Caption = "Payment Invoice number")]
		public ZString PaymentInvoiceNumber
		{
			get
			{
				var result = ZString.Empty;
				if (individualStatements != null)
				{
					if (individualStatements.Length == 1)
					{
						result = individualStatements[0].B2_StatementNumber;
					}
					else if (individualStatements.Length > 1)
					{
						result = MultipleInvoicesMessage;
					}
				}
				return result;
			}
		}
		[ResourceStringData("608547D2-103D-4CAA-B17B-C4F8AE31D83D", Caption = "Received Date")]
		public ZDateTime ReceivedDate
		{
			get
			{
				var result = ZDateTime.Empty;
				if (individualStatements != null)
				{
					if (individualStatements.Length == 1)
					{
						result = individualStatements[0].B2_ProcessDate;
					}
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(HasNotAmendment))]
		public ZBool IncludingCurrentDifference
		{
			get => includingCurrentDifference;
			set
			{
				var oldValue = includingCurrentDifference;
				if (oldValue != value)
				{
					includingCurrentDifference = value;
					IncludingCurrentDifferenceInfo.RefreshBinding();
				}
			}
		}
		ZBool includingCurrentDifference;
		public ZPropertyInfo IncludingCurrentDifferenceInfo => GetZPropertyInfo(nameof(IncludingCurrentDifference));
		bool HasNotAmendment => !Entry.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5AS && x.EM_MessageSubType != _5ASAmendmentType.Codes.Extension);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateToBeDelivered();
		}

		public void ValidateToBeDelivered()
		{
			ToBeDeliveredInfo.ClearAllNotifications();
			if (ToBeDelivered)
			{
				if (!GetEntryTypeNeedsStatus().IsEmpty)
				{
					ValidateEntryStatus();
				}
				if (GetStatementRelevant())
				{
					ValidateVATDeferment();
				}
			}
		}
		void ValidateVATDeferment()
		{
			var payer = OrgHeaderWrapper.New(Entry.Declaration.DutyPayer);
			if (payer != null)
			{
				if (payer.ZO_VATDeferment == VATDefermentCodeList.Codes.Conflict || payer.ZO_VATDeferment.IsEmpty)
				{
					ToBeDeliveredInfo.AddError(VATDefermentConflictErrorMessage);
				}
			}
			else
			{
				ToBeDeliveredInfo.AddError(MissingPayerErrorMessage);
			}
		}

		void ValidateEntryStatus()
		{
			var warningMessage = GetWarningMessageBasedOnStatus();
			if (!string.IsNullOrEmpty(warningMessage))
			{
				ToBeDeliveredInfo.AddWarning(warningMessage);
			}
		}

		public string GetWarningMessageBasedOnStatus()
		{
			var isToBeWarned = statusList?.ShouldUsersBeWarnedPriorToPrintingDocument(entryNumStatus) ?? false;
			return isToBeWarned ? statusList?.GetDocumentPrintingWarningMessage() : string.Empty;
		}

		bool GetStatementRelevant()
		{
			ZBool result = ZBool.False;
			switch (fullMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges:
					result = ZBool.True;
					break;

				default:
					break;
			}
			return result;
		}

		string GetEntryNumStatus()
		{
			var entryType = GetEntryTypeNeedsStatus();
			var result = string.Empty;
			if (!string.IsNullOrEmpty(entryType))
			{
				if (entryType == KRJobMessageTypeList.Codes.Import || entryType == KRJobMessageTypeList.Codes.Export || entryType == KRJobMessageTypeList.Codes.LocalExport)
				{
					switch (fullMenuName)
					{
						case JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport:
						case JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport:
						case JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo:
						case JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods:
						case JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport:
						case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate:
						case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English:
						case JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration:
						case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration:
						case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
						case JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration:
							result = Entry.CH_Status;
							break;
						default:
							result = Entry.CH_EntryStatus;
							break;
					}
				}
				else
				{
					CusEntryNumber entryNumber = null;
					if (entryType == ElectronicDocumentTypeList.Codes._5FN)
					{
						if (Entry.HasAny5FNRejection)
						{
							result = CustomsMessageStatusTypeList.Codes.OriginalRejected;
						}
						else
						{
							entryNumber = Entry.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == entryType).OrderBy(x => x.CE_SystemCreateTimeUtc).LastOrDefault();
						}
					}
					else
					{
						entryNumber = Entry.EntryNumbers.Cast<CusEntryNumber>().SingleOrDefault(x => x.CE_EntryType == entryType);
					}

					if (entryNumber != null)
					{
						result = entryNumber.CE_EntryStatus;
					}
				}
			}
			return result;
		}

		ZString GetEntryTypeNeedsStatus()
		{
			var entryType = string.Empty;
			switch (fullMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease:
					entryType = ElectronicDocumentTypeList.Codes._5BD;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines:
					entryType = ElectronicDocumentTypeList.Codes._5BA;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration:
					entryType = ElectronicDocumentTypeList.Codes._5TM;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate:
					entryType = ElectronicDocumentTypeList.Codes._5FN;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods:
					entryType = KRJobMessageTypeList.Codes.Import;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate:
					entryType = ElectronicDocumentTypeList.Codes._D72;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.RefundRequest:
					entryType = ElectronicDocumentTypeList.Codes._5UL;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo:
				case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate:
				case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English:
				case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration:
					entryType = KRJobMessageTypeList.Codes.Export;
					break;

				case JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration:
					entryType = KRJobMessageTypeList.Codes.LocalExport;
					break;

				default:
					break;
			}
			return entryType;
		}

		IStatusList GetStatusList()
		{
			IStatusList result = null;

			switch (fullMenuName)
			{
				case JobDeclarationDocumentSupporter.MenuNames.AgreedRateForAllLines:
				case JobDeclarationDocumentSupporter.MenuNames.GoldVATDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.ApplyingTaxExemptionOrSpecificUseDutyRate:
				case JobDeclarationDocumentSupporter.MenuNames.ExportGoodsInspectionResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.InspectionPlanAndResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo:
				case JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods:
				case JobDeclarationDocumentSupporter.MenuNames.LocalExportGoodsInspectionResultReport:
				case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate:
				case JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English:
				case JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration:
				case JobDeclarationDocumentSupporter.MenuNames.GoodsRemovalPriorToCustomsRelease:
				case JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate:
				case JobDeclarationDocumentSupporter.MenuNames.RefundRequest:
				case JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration:
					result = Entry.Factory.GetCachedValue<CustomsMessageStatusTypeList>();
					break;

				case JobDeclarationDocumentSupporter.MenuNames.CancellationOfImportDeclaration:
					result = Entry.Factory.GetCachedValue<CustomsEntryStatusExtendedTypeList>();
					break;

				default:
					break;
			}
			return result;
		}
	}
}
