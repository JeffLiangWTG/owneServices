using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Registry;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class CusEntryLine : AutoCusEntryLine, ICusAddInfoTypeSupporter, ICusSupportingInfoTypeSupporter
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : AutoCusEntryLine.Schema
		{
			public const string SequenceNumber = "SequenceNumber";
		}

		protected override ZDecimal GetGSTVATAmountCore()
		{
			return Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalGSTAmount) + Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount);
		}

		protected override ZDecimal GetGSTVATDeferredCore()
		{
			return ZDecimal.Zero;
		}

		public ZString[] Permits
		{
			get
			{
				var result = new List<ZString>();
				if (Declaration != null)
				{
					foreach (DeclarationExportPermit permit in Declaration.Permits)
					{
						if (!result.Contains(permit.CY_Data))
						{
							result.Add(permit.CY_Data);
						}
					}
				}
				foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
				{
					foreach (InvoiceLineExportPermit permit in invoiceLine.Permits)
					{
						if (!result.Contains(permit.CY_Data))
						{
							result.Add(permit.CY_Data);
						}
					}
				}
				return result.ToArray();
			}
		}

		public ZString SequenceNumber
		{
			get
			{
				if (sequenceNumber == null)
				{
					sequenceNumber = new CachedProperty<ZString>(Factory, () => $"[{CL_GoodsShipmentSequence},{CL_CommoditySequence}]");
				}
				return sequenceNumber.Value;
			}
		}
		CachedProperty<ZString> sequenceNumber;

		protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.DoMergeInvoiceLine(baseInvoiceLine);
			if (CL_DutyPercent.IsEmpty)
			{
				CL_DutyPercent = ((JobComInvoiceLine)baseInvoiceLine).DutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.CustomsDuty);
			}
		}

		protected override bool CanBeLinkedUpByPivot
		{
			get { return Header.IsB3CorCAD; }
		}

		#region CA_B2SubHeader

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		public override ZInt CA_B2SubHeader
		{
			get { return base.CA_B2SubHeader; }
			set { base.CA_B2SubHeader = value; }
		}

		#endregion

		public bool DutyFeeChangedSinceLastResponse
		{
			get
			{
				if (dutyFeeChangedSinceLastResponseCached == null)
				{
					dutyFeeChangedSinceLastResponseCached = new CachedProperty<bool>(Factory, () =>
						DutyDiscrepancyAmount != ZDecimal.Zero
							|| GSTDiscrepancyAmount != ZDecimal.Zero
							|| ExciseTaxDiscrepancyAmount != ZDecimal.Zero
							|| SIMADiscrepancyAmount != ZDecimal.Zero
						);
				}
				return dutyFeeChangedSinceLastResponseCached.Value;
			}
		}
		CachedProperty<bool> dutyFeeChangedSinceLastResponseCached;

		public ZDecimal DutyDiscrepancyAmount => GetDiscrepancyAmount([EntryChargeTypeList.Codes.TotalDutyAmount], [CADDutyTaxFeeTypeCodes.Codes.CUD]);

		public ZDecimal GSTDiscrepancyAmount => GetDiscrepancyAmount([EntryChargeTypeList.Codes.TotalGSTAmount, EntryChargeTypeList.Codes.TotalGSTDirectAmount], [CADDutyTaxFeeTypeCodes.Codes.GST]);

		public ZDecimal ExciseTaxDiscrepancyAmount => GetDiscrepancyAmount([EntryChargeTypeList.Codes.TotalExciseTaxAmount], [CADDutyTaxFeeTypeCodes.Codes.FET]);

		public ZDecimal SIMADiscrepancyAmount => GetDiscrepancyAmount([EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, EntryChargeTypeList.Codes.TotalSIMAAmount], [CADDutyTaxFeeTypeCodes.Codes.ADD, CADDutyTaxFeeTypeCodes.Codes.CVD, CADDutyTaxFeeTypeCodes.Codes.SUR]);

		ZDecimal GetDiscrepancyAmount(ZString[] cw1ChargeTypes, ZString[] cusChargeTypes)
		{
			var cw1Value = ZDecimal.Zero;
			var cusValue = ZDecimal.Zero;
			Array.ForEach(cw1ChargeTypes, x => cw1Value += Fees.GetAmount(x));
			Array.ForEach(cusChargeTypes, x => cusValue += ConfirmedFees.GetAmount(x));
			return cusValue - cw1Value;
		}

		public bool AlwaysReturnTrue
		{
			get { return true; }
		}

		readonly string descriptionSourceFromInvoiceLine = ResString.GetMultilingualString("6DDF1035-BDC4-47BC-82E6-B4890C44CB82", "Invoice Line Goods Description");
		readonly string descriptionSourceFromTraiff = ResString.GetMultilingualString("96E8A71F-AB37-49B8-A056-1D89B90EC0DC", "Tariff Description");
		protected override ZString DescriptionInternal
		{
			get
			{
				var calculator = GetDescriptionCalculator();
				calculator.Declaration = Declaration;

				bool isNOP = Declaration.CA_MergeBy == B3MergeByList.Codes.NotMergeUsingProductNumberInDescription;
				bool descNeedFromHighestValueLine = Declaration.CA_MergeBy == B3MergeByList.Codes.ClassificationTariff
				|| Declaration.CA_MergeBy == B3MergeByList.Codes.ClassificationLookup
				|| Declaration.CA_MergeBy == B3MergeByList.Codes.ProductNumber
				|| Declaration.CA_MergeBy == B3MergeByList.Codes.ProductNumberUsingProductNumberInDescription
				|| Declaration.CA_MergeBy == B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices;

				var descriptionInternal = ZString.Empty;
				if (descNeedFromHighestValueLine)
				{
					calculator.DescriptionOverrideDelegate = Tuple.Create<ZStringReturner, ZString>(() => HighestValueLine.JI_Description, descriptionSourceFromInvoiceLine);
					calculator.Part = HighestValueLine.Part;
					calculator.Class = HighestValueLine.Classification;
					calculator.TariffDescriptionDelegate = Tuple.Create<ZStringReturner, ZString>(() => HighestValueLine.TariffDescription, descriptionSourceFromTraiff);
					calculator.CalculateDescription();
					descriptionInternal = calculator.Description;
				}
				else if (isNOP)
				{
					calculator.DescriptionOverrideDelegate = FirstLine.Part == null ? Tuple.Create<ZStringReturner, ZString>(GetFallbackInvoiceLineDescription, descriptionSourceFromInvoiceLine) : null;
					calculator.Part = FirstLine.Part;
					calculator.Class = FirstLine.Classification;
					calculator.TariffDescriptionDelegate = Tuple.Create<ZStringReturner, ZString>(() => FirstLine.TariffDescription, descriptionSourceFromTraiff);
					calculator.CalculateDescription();
					descriptionInternal = calculator.Description;
				}
				else
				{
					descriptionInternal = base.DescriptionInternal;
				}

				return descriptionInternal;
			}
		}

		protected BaseJobComInvoiceLine HighestValueLine
		{
			get { return GetHighestValueLine(); }
		}

		protected BaseJobComInvoiceLine GetHighestValueLine()
		{
			BaseJobComInvoiceLine result;
			if (InvoiceLines.Count == 0)
			{
				result = (BaseJobComInvoiceLine)Factory.GetNull(BaseJobComInvoiceLine.TypeDecider.GetTypeForCountryCode(CountryCode));
			}
			else
			{
				InvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LinePrice, System.ComponentModel.ListSortDirection.Descending);
				result = InvoiceLines[0];
			}
			return result;
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CADutyAndTax, typeof(DutyAndTax));
			return result;
		}

		#endregion

		#region AmendmentDetails

		public CADCorrectionMessageSendingActionCollection AmendmentDetails
		{
			get
			{
				if (fAmendmentDetails == null)
				{
					fAmendmentDetails = new CADCorrectionMessageSendingActionCollection(this);
					fAmendmentDetails.SuspendValidation();
					fAmendmentDetails.Load();
				}
				return fAmendmentDetails;
			}
		}
		CADCorrectionMessageSendingActionCollection fAmendmentDetails;

		public void RefreshAmendmentDetails()
		{
			if (AmendmentDetails != null)
			{
				AmendmentDetails.Reload(true);
			}
		}

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>()
			{
				{ Common.CA.CusSupportingInfoTypeList.Codes.CadCorrectionMessageSendingAction, typeof(CADCorrectionMessageSendingAction) },
			};
		}

		#endregion
	}
}
