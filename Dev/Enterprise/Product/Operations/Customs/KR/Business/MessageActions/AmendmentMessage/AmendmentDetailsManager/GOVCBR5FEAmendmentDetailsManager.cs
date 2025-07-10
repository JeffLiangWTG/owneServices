using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5FEAmendmentDetailsManager : AmendmentDetailsManager<ImportEntryHeader, IImportEntryHeader>
	{
		public GOVCBR5FEAmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		public override ZString AmendmentType => DutyTaxCorrectionCode + DeclarationCorrectionCode;
		ZString DutyTaxCorrectionCode
		{
			get
			{
				if (dutyTaxCorrectionCode.IsEmpty)
				{
					dutyTaxCorrectionCode = DutyTaxCorrectionCodeList.Codes.X;
					if (HasDutyTaxRelatedChanges)
					{
						dutyTaxCorrectionCode = DutyTaxCorrectionCodeList.Codes.O;
						if (PaymentDate.IsValid)
						{
							if (DutyTaxDifferenceToPay > 0)
							{
								if (PaymentDate.AddMonths(MaxMonthsOfStage1Amendment) >= ZDateTime.Today)
								{
									dutyTaxCorrectionCode = DutyTaxCorrectionCodeList.Codes.A;
								}
								else
								{
									dutyTaxCorrectionCode = DutyTaxCorrectionCodeList.Codes.B;
								}
							}
							else if (DutyTaxDifferenceToPay < 0 && PaymentDate.AddYears(MaxYearOfStage2Amendment) >= ZDateTime.Today)
							{
								dutyTaxCorrectionCode = DutyTaxCorrectionCodeList.Codes.C;
							}
						}
					}
				}
				return dutyTaxCorrectionCode;
			}
		}
		ZString dutyTaxCorrectionCode;

		const int MaxMonthsOfStage1Amendment = 6;
		const int MaxYearOfStage2Amendment = 5;

		ZString DeclarationCorrectionCode
		{
			get
			{
				if (declarationCorrectionCode.IsEmpty)
				{
					declarationCorrectionCode = DeclarationCorrectionCodeList.Codes.X;
					if (HasDeclarationChanges)
					{
						declarationCorrectionCode = DeclarationCorrectionCodeList.Codes.D;
					}
				}
				return declarationCorrectionCode;
			}
		}
		ZString declarationCorrectionCode;

		bool HasDutyTaxRelatedChanges => HasChangesOfTypes(new DataItemIDAttribute.ChangeType[] { DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal, DataItemIDAttribute.ChangeType.DutyTaxRelated });
		bool HasDeclarationChanges => HasChangesOfTypes(new DataItemIDAttribute.ChangeType[] { DataItemIDAttribute.ChangeType.DutyTaxRelatedAndNormal, DataItemIDAttribute.ChangeType.Normal });

		bool HasChangesOfTypes(IEnumerable<DataItemIDAttribute.ChangeType> changeTypes) => AmendedItems.Any(x => changeTypes.Contains(x.ChangeType));

		public ZDateTime PaymentDate
		{
			get
			{
				if (!paymentDate.HasValue)
				{
					paymentDate = ZDateTime.Empty;
					var statement929 = Entry.Statement929;
					if (statement929 != null)
					{
						paymentDate = statement929.B2_PaymentAuthorizationDate;
					}
				}
				return paymentDate.Value;
			}
		}
		ZDateTime? paymentDate;

		ZDecimal DutyTaxDifferenceToPay
		{
			get
			{
				if (!dutyTaxDifferenceToPay.HasValue)
				{
					var beforeTotalDutyTaxAmount = ZDecimal.Zero;
					var snapShot = Entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
					if (snapShot != null)
					{
						using (var textReader = snapShot.GetCES_SnapshotXmlReader())
						{
							var snapShotData = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryHeader>(textReader);
							beforeTotalDutyTaxAmount = snapShotData.TotalPayableAmount;
						}
					}
					dutyTaxDifferenceToPay = Entry.TotalAmountPayable - beforeTotalDutyTaxAmount;
				}
				return dutyTaxDifferenceToPay.Value;
			}
		}
		ZDecimal? dutyTaxDifferenceToPay;

		protected override CodeDescriptionPairList GetDataItemIDList(BusinessObjectFactory factory) => factory.GetCachedValue<ImportAmendmentDataItemIDList>();

		protected override ImportEntryHeader GetCurrentDataProvider() => new ImportEntryHeaderCreator().Create(Entry);

		protected override IEnumerable<AmendedItem> GetAmendedItems()
		{
			var result = new List<AmendedItem>();
			IEnumerable<AmendedItem> amendedItems = base.GetAmendedItems();
			foreach (AmendedItem item in amendedItems)
			{
				item.AmendType = TransformAmendType(item);
				if (item.AmendType != EntityAmendType.NoChange)
				{
					item.DataItemID = TransformDataItemID(item);
					var bondedFactoryUsedCode = Entry.EntryInstruction?.CEI_BondedFactoryUseCode ?? ZString.Empty;
					item.BeforeValue = TransformDescription(bondedFactoryUsedCode, item, item.BeforeValue);
					item.AfterValue = TransformDescription(bondedFactoryUsedCode, item, item.AfterValue);

					result.Add(item);
				}
			}
			return result;
		}

		protected override IEnumerable<string> GetMandatoryItems()
		{
			return Array.Empty<string>();
		}

		static EntityAmendType TransformAmendType(AmendedItem item)
		{
			var result = item.AmendType;

			if (ImportAmendmentDataItemIDList.IsHeaderDataItem(item.DataItemID) && (item.AmendType == EntityAmendType.Delete || item.AmendType == EntityAmendType.Add))
			{
				result = EntityAmendType.Update;
			}
			return result;
		}

		static ZString TransformDataItemID(AmendedItem item)
		{
			var result = item.DataItemID;

			if (item.AmendType == EntityAmendType.Delete)
			{
				switch (item.EntityType)
				{
					case nameof(IImportEntryLine):
						result = ImportAmendmentDataItemIDList.Codes.B111;
						break;
					case nameof(IImportInvoiceLine):
						result = ImportAmendmentDataItemIDList.Codes.C109;
						break;
					case nameof(IImportGAApprovalDocument):
						result = ImportAmendmentDataItemIDList.Codes.D109;
						break;
					case nameof(IImportNonGADetail):
						result = ImportAmendmentDataItemIDList.Codes.E106;
						break;
					case nameof(IImportPreviousExpDecLine):
						result = ImportAmendmentDataItemIDList.Codes.G106;
						break;
					case nameof(IImportContainer):
						result = ImportAmendmentDataItemIDList.Codes.H103;
						break;
					case nameof(IImportImmediateDelivery):
						result = ImportAmendmentDataItemIDList.Codes.I103;
						break;
					case nameof(IImportOnlineOrder):
						result = ImportAmendmentDataItemIDList.Codes.J103;
						break;
				}
			}
			return result;
		}

		static ZString TransformDescription(ZString bondedFactoryUseCode, AmendedItem amendedItem, ZString value)
		{
			var result = value;

			if (ImportAmendmentDataItemIDList.IsDateTimeField(amendedItem.DataItemID))
			{
				if (!value.IsEmpty)
				{
					var dataFormatType = DateFormatType.Date;
					if (amendedItem.DataItemID == ImportAmendmentDataItemIDList.Codes.A704 && bondedFactoryUseCode == BondedFactoryUseCodeList.Codes.B)
					{
						dataFormatType = DateFormatType.DateTime;
					}

					if (DateTime.TryParseExact(value, ZDateTime.LongTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
					{
						result = dt.ToString(dataFormatType);
					}
					else
					{
						result = ZString.Empty;
					}
				}
			}

			return result;
		}
	}
}
