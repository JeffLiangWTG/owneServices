using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseMessageLine
	{
		public BaseMessageLine(ICusEntryLine entryLine, SegmentGroup30 group30)
		{
			this.EntryLine = entryLine;
			this.Group30 = group30;
		}

		public abstract void Populate(int lineNumber, string lineActionCode);

		#region Group 30

		protected internal SegmentGroup30 Group30
		{
			get { return fGroup30; }
			set { fGroup30 = value; }
		}
		SegmentGroup30 fGroup30;

		#endregion

		#region CST Segment

		protected internal virtual void PopulateCST(string lineActionCode)
		{
			CSTForGroup30 = Group30.CST.InstantiateAChildAndAddItToChildrenCollection();

			CSTForGroup30.GoodsItemNumber = EntryLine.CL_LineNumber.ToString();
			CSTForGroup30.CustomsIdentityCodes1.CustomsCodeIdentification = lineActionCode;
			CSTForGroup30.CustomsIdentityCodes1.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		#region Group 30 - CST

		protected CSTSegment fCSTForGroup30;
		protected CSTSegment CSTForGroup30
		{
			get { return fCSTForGroup30; }
			set { fCSTForGroup30 = value; }
		}

		#endregion

		#endregion

		#region FTX Segment

		protected internal virtual void PopulateFTX()
		{
			PopulateFTX(EntryLine.Description);
		}

		protected internal void PopulateFTX(ZString freeTextValue1)
		{
			if (!freeTextValue1.IsEmpty)
			{
				FTXSegment fTX = Group30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GoodsDescription;
				fTX.TextLiteral.FreeTextValue1 = freeTextValue1.SubstringSafe(0, 512);
			}
		}

		#endregion

		#region MEA Segment

		protected internal virtual void PopulateMEA()
		{
			if (EntryLine.CustomsQuantity > 0 && !EntryLine.CustomsUnitQty.IsEmpty)
			{
				ZString uQ = GetUnitOfQuantity(EntryLine.CustomsUnitQty);
				PopulateMEA(MeasurementAttributeCodeList.LineItemMeasurement, uQ, EntryLine.CustomsQuantity, 5);
			}

			PopulateSecondUnitOfQuantity();
		}

		protected void PopulateSecondUnitOfQuantity()
		{
			ZDecimal secondQuantity = EntryLine.SecondCustomsQuantity;
			if (secondQuantity > 0 && !EntryLine.SecondCustomsUnitQty.IsEmpty)
			{
				ZString uQ = GetUnitOfQuantity(EntryLine.SecondCustomsUnitQty);
				PopulateMEA(MeasurementAttributeCodeList.LineItemMeasurement, uQ, secondQuantity, 5);
			}
		}

		protected internal void PopulateMEA(MeasurementAttributeCodeList measurementAttributeCode, ZString measurementUnitCode, ZDecimal measurementValue, ZInt noOfDecimals)
		{
			if (measurementValue > 0)
			{
				MEASegment mEA = Group30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				mEA.MeasurementAttributeCode = measurementAttributeCode;
				mEA.ValueRange.MeasurementUnitCode = measurementUnitCode;
				mEA.ValueRange.MeasurementValue = measurementValue.Round(noOfDecimals).ToString(noOfDecimals);
			}
		}

		#endregion

		#region ToDo: Remove this once the Tariff and UQ are correct

		protected ZString GetUnitOfQuantity(ZString uQ)
		{
			ZString result = uQ;

			if (result == "M2")
			{
				result = "SM";
			}
			else if (result == "M3")
			{
				result = "CU";
			}

			return result;
		}

		#endregion

		#region Segment Group 33

		protected internal abstract void PopulateGroup33();

		protected void PopulateMOA(SegmentGroup33 group33, Money monetaryAmount, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			if (monetaryAmount != null && (!monetaryAmount.Amount.IsEmpty || qualifier == MonetaryAmountTypeCodeQualifierList.InvoiceItemAmount))
			{
				MOASegment mOA = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = qualifier;

				ZDecimal amount = monetaryAmount.Amount;

				mOA.MonetaryAmount.MonetaryAmountValue = amount.ToString(2);
				if (monetaryAmount.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = monetaryAmount.Currency.Code;
				}
				else if (qualifier == MonetaryAmountTypeCodeQualifierList.InvoiceItemAmount && EntryLine.Header != null
					&& EntryLine.Header.InvoiceTotal != null && EntryLine.Header.InvoiceTotal.Currency != null)
				{
					mOA.MonetaryAmount.CurrencyIdentificationCode = EntryLine.Header.InvoiceTotal.Currency.Code;
				}
			}
		}

		#region Group 33

		protected SegmentGroup33 fGroup33;
		protected SegmentGroup33 Group33
		{
			get { return fGroup33; }
			set { fGroup33 = value; }
		}

		#endregion

		#endregion

		#region Segment Group 35

		protected internal virtual void PopulateGroup35()
		{
			Group35 = Group30.Group35.InstantiateAChildAndAddItToChildrenCollection();

			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.CustomsTariffNumber, EntryLine.TariffNumber);
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.CommodityNumber, EntryLine.StatCodeForCMR);
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.DispensationReference, EntryLine.GSTE);
			PopulateRFF(Group35, ReferenceFunctionCodeQualifierList.DefermentApprovalNumber, EntryLine.WETE);
			PopulateImportPermitNumber(Group35);
		}

		void PopulateImportPermitNumber(SegmentGroup35 group35)
		{
			foreach (var importPermitNumber in EntryLine.ImportPermitNumbers)
			{
				PopulateRFF(group35, ReferenceFunctionCodeQualifierList.ImportPermitNumber, importPermitNumber);
			}
		}

		#region Populate RFF

		protected void PopulateRFF(SegmentGroup35 group35, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier)
		{
			PopulateRFF(group35, referenceFunctionCodeQualifier, referenceIdentifier, null);
		}

		protected void PopulateRFF(SegmentGroup35 group35, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier, ZString referenceVersionIdentifier)
		{
			if (!referenceIdentifier.IsEmpty)
			{
				RFFSegment rFF = group35.RFF.InstantiateAChildAndAddItToChildrenCollection();
				rFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
				rFF.Reference.ReferenceIdentifier = referenceIdentifier;
				rFF.Reference.ReferenceVersionIdentifier = referenceVersionIdentifier;
			}
		}

		#endregion

		#region Group 35

		protected SegmentGroup35 fGroup35;
		protected internal SegmentGroup35 Group35
		{
			get { return fGroup35; }
			set { fGroup35 = value; }
		}

		#endregion

		#endregion

		#region Segment Group 40

		protected internal virtual void PopulateGroup40()
		{
			//Wine Equalisation Tax Quote Indicator
			PopulateGISInGroup40(EntryLine.WETQ, "WET");
		}

		protected void PopulateGISInGroup40(string value, string processingIndicatorDescriptionCode)
		{
			if (value == "Y")
			{
				GISSegment gIS = Group40.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(processingIndicatorDescriptionCode);
				gIS.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsIndicator;
				gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		#region Group 40

		SegmentGroup40 Group40
		{
			get
			{
				if (fGroup40 == null)
				{
					fGroup40 = Group30.Group40.InstantiateAChildAndAddItToChildrenCollection();
				}

				return fGroup40;
			}
		}
		SegmentGroup40 fGroup40;

		#endregion

		#endregion

		protected readonly ICusEntryLine EntryLine;
	}
}
