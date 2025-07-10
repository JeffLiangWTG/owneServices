using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business
{
	class ImportMergeStrategy : EntryCreationStrategy
	{
		public ImportMergeStrategy(JobDeclaration declaration, string entryType)
			: base(declaration, entryType)
		{
		}

		#region GetKeyForHeader/Line

		#region GetKeyForHeader

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			return new MergeKey();
		}

		#endregion

		#region GetKeyForLine

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			switch (CH_MessageTypeToNewEntryHeader)
			{
				case MessageTypeList.Codes.EDIRelease:
					return base.GetKeyForLine(baseInvoiceLine);
				case MessageTypeList.Codes.B3CUSDEC:
				case MessageTypeList.Codes.CommercialAccountingDeclaration:
					return GetKeyForLineForB3Entry((JobComInvoiceLine)baseInvoiceLine);
				default:
					throw GetUnsupportedEntryException();
			}
		}

		MergeKey GetKeyForLineForB3Entry(JobComInvoiceLine line)
		{
			MergeKey result;
			var declaration = line.Declaration;
			if (declaration.IsConsolidatedLVS)
			{
				result = GetKeyForHeader(line) + new LowValueShipmentsMergeKey(line, declaration.JE_MessageSubType);
			}
			else
			{
				result = base.GetKeyForLine(line) + new B3MergeKey(line, declaration.CA_MergeBy);
			}
			return result;
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override ZString GetMergeBy()
		{
			switch (CH_MessageTypeToNewEntryHeader)
			{
				case MessageTypeList.Codes.EDIRelease:
					return Declaration.JE_MergeBy;
				case MessageTypeList.Codes.B3CUSDEC:
				case MessageTypeList.Codes.CommercialAccountingDeclaration:
					return Declaration.CA_MergeBy;
				default:
					throw GetUnsupportedEntryException();
			}
		}

		#region B3MergeKey

		//NOTE: Make sure that all fields in B3 and LVS merge key (except duties) are included to 
		//JobComInvoiceHeader.ApportionmentDirtyChangedEventHandler and JobComInvoiceLine.OnMarkApportionmentDirty
		class B3MergeKey : MergeKey
		{
			public B3MergeKey(JobComInvoiceLine line, ZString mergeBy)
			{
				if (line.IsDutiesAndTaxesOverriden || line.CA_CalculationMethod == CalculationMethods.Codes.RepairsRemission || line.CA_CalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission)
				{
					Add(line.PK);
				}
				else
				{
					if (line.InvoiceHeader is JobComInvoiceHeader invoice)
					{
						if (mergeBy == B3MergeByList.Codes.ClassificationOrTariffOverMultipleInvoices)
						{
							Add(invoice.SupplierDocumentaryAddress.HumanReadableName);
							Add(invoice.SupplierDocumentaryAddress.AddressAsASingleLine);
							Add(invoice.CA_USPortOfExit);
							Add(invoice.JZ_ValuationDateOverride);
							Add(invoice.JZ_RX_NKInvoice_Currency);
							Add(invoice.CA_TimeLimit);
							Add(invoice.CA_TimeLimitCode);
							var effectivePlaceOfExp = invoice.CA_TradeZone.IsEmpty ? line.EffectiveCountryAndStateOfExport : invoice.CA_TradeZone;
							Add(effectivePlaceOfExp);
						}
						else
						{
							Add(invoice.PK);
						}
						if (invoice.JobDeclaration is JobDeclaration parentDeclaration && parentDeclaration.IsIM2)
						{
							Add(line.CA_PreviousB3LineNo);
						}
					}

					Add(line.JI_Tariff);
					Add(line.CA_99TariffCode);
					Add(line.EffectiveValueForDutyCode);
					Add(line.CA_AuthorityNumber);
					Add(line.CA_TRSNumber);
					Add(line.CA_CalculationMethod);

					Add(line.JI_ParentID);
					Add(line.EffectiveCountryAndStateOfOrigin);
					Add(line.EffectiveTreatmentCode);
					Add(line.JI_CustomsUnitQty);
					Add(line.JI_CustomsSecondUnitQty);
					Add(line.JI_CustomsThirdUnitQty);

					var sima = line.DutyAndTaxManager.SIMADuties.FirstOrDefault();
					if (sima != null)
					{
						Add(sima.C1_ExemptCode);
					}

					foreach (var tax in line.DutiesAndTaxes.Where(a => a.IsTax).OrderBy(a => a, new DutyAndTaxComparer()))
					{
						Add(tax.C1_Code);
						Add(tax.C1_ExemptCode);
					}
				}
			}
		}

		#endregion

		#region LowValueShipmentsMergeKey

		//NOTE: Make sure that all fields in B3 and LVS merge key (except duties) are included to 
		//JobComInvoiceHeader.ApportionmentDirtyChangedEventHandler and JobComInvoiceLine.OnMarkApportionmentDirty
		class LowValueShipmentsMergeKey : MergeKey
		{
			public LowValueShipmentsMergeKey(JobComInvoiceLine line, ZString lvsType)
			{
				if (line.InvoiceHeader is JobComInvoiceHeader invoice)
				{
					Add(invoice.CA_TimeLimit);
					Add(invoice.CA_TimeLimitCode);
					Add(invoice.JZ_RX_NKInvoice_Currency);
				}

				Add(line.EffectiveTreatmentCode);

				if (TariffTreatmentCodes.IsCountryOfOriginAndExportRequiredForLVS(line.EffectiveTreatmentCode))
				{
					Add(line.EffectiveCountryAndStateOfOrigin);
					Add(line.EffectiveCountryAndStateOfExport);
				}

				if (lvsType == LowValueShipmentsTypes.Codes.ConsolidationByImporter)
				{
					if (line.IsDutiesAndTaxesOverriden)
					{
						Add(line.PK);
					}
					else
					{
						var isOicLine = !line.CA_AuthorityNumber.IsEmpty;
						var isRemissionLine = line.IsRemissionLine;
						if (isOicLine)
						{
							Add(line.CA_AuthorityNumber);
							Add(line.JI_CustomsUnitQty);
							Add(line.JI_CustomsSecondUnitQty);
							Add(line.JI_CustomsThirdUnitQty);
							Add(line.JI_Tariff);
							Add(line.CA_99TariffCode);
						}

						if (isRemissionLine)
						{
							Add(line.CA_CalculationMethod);
							Add(line.JI_ParentID);
						}

						foreach (var tax in line.DutiesAndTaxes.Where(a => a.IsTax).OrderBy(a => a, new DutyAndTaxComparer()))
						{
							Add(tax.C1_Code);
							Add(tax.C1_ExemptCode);
						}
					}
				}

				if (line.DutyAndTaxManager.SIMADuties.Any())
				{
					Add(line.PK);
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region GetExistingEntry/Line

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return from Customs.Business.CusEntryHeader entry in Declaration.ActiveEntryHeaders where entry.CH_MessageType == CH_MessageTypeToNewEntryHeader select entry;
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			CusEntryLine result = null;
			switch (CH_MessageTypeToNewEntryHeader)
			{
				case MessageTypeList.Codes.EDIRelease:
					result = (CusEntryLine)invoiceLine.CusEntryLine;
					break;
				case MessageTypeList.Codes.B3CUSDEC:
				case MessageTypeList.Codes.CommercialAccountingDeclaration:
					if (((JobComInvoiceLine)invoiceLine).Declaration.IsLVX)
					{
						result = (CusEntryLine)invoiceLine.CusEntryLine;
					}
					else if (invoiceLine.AdditionalEntryLineLinks.Count > 0)
					{
						result = (CusEntryLine)invoiceLine.AdditionalEntryLineLinks[0].EntryLine;
					}
					break;
			}
			return result;
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_BGMReference = Declaration.TransactionNumber;

			var legacyMessageType = entryHeader.CH_MessageType.ToString() switch
			{
				MessageTypeList.Codes.B3CUSDEC => MessageTypeList.Codes.CommercialAccountingDeclaration,
				MessageTypeList.Codes.CommercialAccountingDeclaration => MessageTypeList.Codes.B3CUSDEC,
				_ => null
			};

			if (legacyMessageType != null)
			{
				var legacyEntry = Declaration.GetEntryHeaderFor(legacyMessageType);
				if (legacyEntry != null && !legacyEntry.IsDeleted)
				{
					legacyEntry.IsActive = false;
				}
			}
		}

		#endregion

		#region LinkInvoiceLineEntryLineAndReturnPivotIfUsed

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			switch (CH_MessageTypeToNewEntryHeader)
			{
				case MessageTypeList.Codes.EDIRelease:
					return base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine);
				case MessageTypeList.Codes.B3CUSDEC:
				case MessageTypeList.Codes.CommercialAccountingDeclaration:
					return ((JobComInvoiceLine)baseInvoiceLine).Declaration.IsLVX ?
						base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine) : baseInvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
				default:
					throw GetUnsupportedEntryException();
			}
		}

		InvalidOperationException GetUnsupportedEntryException()
		{
			return new InvalidOperationException(string.Format("Unsupported import entry type: '{0}'", CH_MessageTypeToNewEntryHeader));
		}

		#endregion
	}
}
