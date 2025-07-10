using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader)
			: base(declaration, entryHeaderMessageTypeToNewEntryHeader)
		{
			comparers = new Dictionary<Type, ColumnComparer>();
			IsUCC6 = declaration.IsUCC6;
			IsExport = declaration.IsExport;
			IsUCC6AndIsExport = IsUCC6 && IsExport;
			IsImport = declaration.IsImport;
			IsUCC6AndIsImport = IsUCC6 && IsImport;
			includeJI_RN_NKCountryOfExportInLineMergeKey = declaration.Configuration.EntryLineConfiguration.MergeJI_RN_NKCountryOfExport(declaration);
		}

		public EntryCreationStrategy(JobDeclaration declaration)
			: this(declaration, declaration.JE_ApplicationCode.IsEmpty ? declaration.JE_MessageType : declaration.JE_ApplicationCode)
		{
		}

		protected bool IsUCC6 { get; }
		protected bool IsExport { get; }
		protected bool IsImport { get; }
		readonly bool includeJI_RN_NKCountryOfExportInLineMergeKey;

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			ProcessInvoiceHeaderAdditionalInfosForHeaderCore(result, invoiceLine);

			if (!IsUCC6)
			{
				var line = (JobComInvoiceLine)invoiceLine;
				var header = line.InvoiceHeader;
				result.Add(header.RelatedIndicator);
				result.Add(header.ZG_RelatedIndicator2);
				result.Add(header.ZG_RelatedIndicator3);
				result.Add(header.ZG_RelatedIndicator4);
			}

			result.Add(invoiceLine.JI_CEI);
			return result;
		}

		protected virtual void ProcessInvoiceHeaderAdditionalInfosForHeaderCore(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			var keys = GetAdditionalInfoKeys();
			if (keys.Length > 0)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

				// Each additional info can either be header only, header or line, or line only.
				// header only addinfos must be added to the header key to create separate entries
				List<BusinessObject> headerAdditionalInfos = new List<BusinessObject>();
				foreach (AdditionalInfo ai in line.InvoiceHeader.AdditionalInfos)
				{
					if (ai.IsHeaderOnly)
					{
						headerAdditionalInfos.Add(ai);
					}
				}
				AddResults(mergeKey, headerAdditionalInfos, keys);
			}
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine invoiceLine)
		{
			MergeKey result = base.GetKeyForLine(invoiceLine);
			JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;
			// Listed in screen order, working from first tab onwards
			result.Add(line.JI_CountryOfOrigin);
			result.Add(line.JI_Procedure);
			result.Add(line.JI_PrimaryPreference);
			result.Add(GetCodesAsString(line.SupplementaryCodes.Select(x => x.CY_Code)));
			if (ShouldProcessNationalCodesForLineMergeKey)
			{
				result.Add(GetCodesAsString(line.NationalCodes.Select(x => x.CY_Code)));
			}
			result.Add(line.JI_AdditionalSupplements);
			result.Add(line.JI_ValuationCode);
			result.Add(line.ZG_ValueAdjustmentCode);
			result.Add(line.JI_ValuationMarkup);
			result.Add(GetUnitQuantityKey(line, line.JI_CustomsSecondUnitQty));
			result.Add(line.JI_ConcessionOrder);
			var invoice = line.InvoiceHeader;
			AddLineLevelOrganisationMergeKeys(result, invoice, line);
			result.Add(invoice.ZG_TransportChargesMethodOfPayment);
			result.Add(line.ZG_CountryOfDestination);
			result.Add(line.ZG_CountryOfSupply);
			if (ShouldProcessRelatedIndicatorsForLineMergeKey)
			{
				result.Add(line.RelatedIndicator);
				result.Add(line.ZG_RelatedIndicator2);
				result.Add(line.ZG_RelatedIndicator3);
				result.Add(line.ZG_RelatedIndicator4);
			}
			result.Add(line.AdditionalProcedureCodesAsString);
			result.Add(GetUnitQuantityKey(line, line.JI_CustomsThirdUnitQty));
			result.Add(line.ZG_CommercialReference);
			result.Add(line.ZG_TransNature);
			result.Add(GetUnitQuantityKey(line, line.JI_CustomsFourthUnitQty));
			result.Add(GetUnitQuantityKey(line, line.JI_CustomsFifthUnitQty));
			if (ShouldProcessCountryOfDispatchAndRegionOfDestination)
			{
				result.Add(line.ZG_RegionOfDestination);
				result.Add(line.ZG_CountryOfDispatch);
			}
			if (includeJI_RN_NKCountryOfExportInLineMergeKey)
			{
				result.Add(line.JI_RN_NKCountryOfExport);
			}

			ProcessAdditionalInfosForLineMergeKey(result, invoiceLine);
			AddResults(result, line.Taxes, GetTaxKeys());

			ProcessSupportingDocsForLineMergeKey(result, invoiceLine);
			if (ShouldProcessCusAuthorizationUsagesForLineMergeKey)
			{
				AddResults(result, line.CusAuthorizationUsages, GetCusAuthorizationUsageKeys());
			}
			if (ShouldProcessCusSupplyChainActorReferencesForLineMergeKey)
			{
				AddResults(result, line.CusSupplyChainActorReferences, GetCusSupplyChainActorReferenceKeys());
			}
			if (ShouldProcessFiscalReferencesForLineMergeKey)
			{
				AddResults(result, line.FiscalReferences, GetFiscalReferenceKeys());
			}
			result.Add(line.ZG_CusNumber);

			if (ForbidMergingInvoiceLinesWithRatioBasedTariff(line))
			{
				result.Add(line.PK);
			}

			return result;
		}

		protected virtual void AddLineLevelOrganisationMergeKeys(MergeKey result, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine)
		{
			result.Add(invoice.JZ_OH_Buyer);     // Ensures one line has only one buyer
			result.Add(invoice.JZ_OH_Supplier);  //  --"-- supplier
			if (ShouldProcessOrganisationsForLineMergeKey)
			{
				result.Add(invoiceLine.ExporterAddress?.Header.PK ?? ZGuid.Empty);
				result.Add(invoiceLine.ConsigneeAddress?.Header.PK ?? ZGuid.Empty);
				result.Add(invoiceLine.BuyerDocAddress.OrganisationPK);
				result.Add(invoiceLine.SellerDocAddress.OrganisationPK);
			}
		}

		protected virtual bool ShouldProcessCountryOfDispatchAndRegionOfDestination => IsUCC6AndIsImport;
		protected virtual bool ShouldProcessNationalCodesForLineMergeKey => false;
		protected virtual bool ShouldProcessCusAuthorizationUsagesForLineMergeKey => IsUCC6;
		protected virtual bool ShouldProcessCusSupplyChainActorReferencesForLineMergeKey => IsUCC6;
		protected virtual bool ShouldProcessFiscalReferencesForLineMergeKey => IsUCC6;
		bool ShouldProcessOrganisationsForLineMergeKey => IsUCC6;
		bool ShouldProcessRelatedIndicatorsForLineMergeKey => !IsUCC6AndIsExport;

		ZString GetCodesAsString(IEnumerable<ZString> codes)
		{
			return ZString.Join("_", codes.Where(x => !x.IsEmpty).OrderBy(x => x).ToArray());
		}

		protected virtual void ProcessAdditionalInfosForLineMergeKey(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			var keys = GetAdditionalInfoKeys();
			if (keys.Length > 0)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

				// Each additional info can either be header only, header or line, or line only.
				// header or line are all currently sent at the line level so get added together for the merge key
				List<BusinessObject> lineAdditionalInfos = new List<BusinessObject>();
				foreach (AdditionalInfo ai in line.InvoiceHeader.AdditionalInfos)
				{
					if (ai.IsLine)
					{
						lineAdditionalInfos.Add(ai);
					}
				}
				lineAdditionalInfos.AddRange(line.AdditionalInfos);
				AddResults(mergeKey, lineAdditionalInfos, keys);
			}
		}

		void ProcessSupportingDocsForLineMergeKey(MergeKey mergeKey, BaseJobComInvoiceLine invoiceLine)
		{
			var keys = GetSupportingDocumentKeys();
			if (keys.Length > 0)
			{
				JobComInvoiceLine line = (JobComInvoiceLine)invoiceLine;

				var lineSupportingDocs = new List<BusinessObject>();
				foreach (SupportingDocument sd in line.InvoiceHeader.SupportingDocuments)
				{
					if (sd.IsLineOnly && DocumentForMerge(sd))
					{
						lineSupportingDocs.Add(sd);
					}
				}
				lineSupportingDocs.AddRange(line.SupportingDocuments.Cast<SupportingDocument>().Where(x => DocumentForMerge(x)));

				AddResults(mergeKey, lineSupportingDocs, keys);
			}
		}

		protected virtual bool DocumentForMerge(SupportingDocument doc)
		{
			return true;
		}

		public string[] GetAdditionalInfoKeys() => additionalInfoKeys ?? (additionalInfoKeys = GetAdditionalInfoKeysCore());
		string[] additionalInfoKeys;

		protected virtual string[] GetAdditionalInfoKeysCore()
		{
			return new string[]
			{
				AdditionalInfo.Schema.CSI_Description,
				AdditionalInfo.Schema.CSI_Code
			};
		}

		public string[] GetTaxKeys() => taxKeys ?? (taxKeys = GetTaxKeysCore());
		string[] taxKeys;

		protected virtual string[] GetTaxKeysCore()
		{
			return new string[]
			{
				JobComInvoiceLineTax.Schema.JLT_MethodOfPayment,
				JobComInvoiceLineTax.Schema.JLT_RateOverrideReasonCode,
				JobComInvoiceLineTax.Schema.JLT_MethodOfCalculation,  // covers RateDuty and Suspension
				JobComInvoiceLineTax.Schema.JLT_Type,
			};
		}

		public string[] GetCusAuthorizationUsageKeys() => cusAuthorizationUsageKeys ?? (cusAuthorizationUsageKeys = GetCusAuthorizationUsageKeysCore());
		string[] cusAuthorizationUsageKeys;

		protected virtual string[] GetCusAuthorizationUsageKeysCore()
		{
			return new string[]
			{
				CusAuthorizationUsage.Schema.AGC_Code,
				CusAuthorizationUsage.Schema.AGC_Number,
				CusAuthorizationUsage.Schema.AGC_OH_Owner
			};
		}

		public string[] GetCusSupplyChainActorReferenceKeys() => cusSupplyChainActorReferenceKeysKeys ?? (cusSupplyChainActorReferenceKeysKeys = GetCusSupplyChainActorReferenceKeysCore());
		string[] cusSupplyChainActorReferenceKeysKeys;

		protected virtual string[] GetCusSupplyChainActorReferenceKeysCore()
		{
			return new string[]
			{
				CusSupplyChainActorReference.Schema.CFR_Code,
				CusSupplyChainActorReference.Schema.CFR_Reference,
				CusSupplyChainActorReference.Schema.CFR_OA_Owner
			};
		}

		public string[] GetFiscalReferenceKeys() => fiscalReferenceKeys ?? (fiscalReferenceKeys = GetFiscalReferenceKeysCore());
		string[] fiscalReferenceKeys;

		string[] GetFiscalReferenceKeysCore()
		{
			return new string[]
			{
				CusFiscalReference.Schema.CFR_Code,
				CusFiscalReference.Schema.CFR_Reference,
			};
		}

		public string[] GetPreviousDocumentHeaderKeys() => previousDocumentHeaderKeys ?? (previousDocumentHeaderKeys = GetPreviousDocumentHeaderKeysCore());
		string[] previousDocumentHeaderKeys;

		protected virtual string[] GetPreviousDocumentHeaderKeysCore() => GetPreviousDocumentKeys();

		public string[] GetPreviousDocumentKeys() => previousDocumentKeys ?? (previousDocumentKeys = GetPreviousDocumentKeysCore());
		string[] previousDocumentKeys;

		protected virtual string[] GetPreviousDocumentKeysCore()
		{
			return new string[]
			{
				  PreviousDocument.Schema.CSI_SubType,
				  PreviousDocument.Schema.CSI_Description,
				  PreviousDocument.Schema.CSI_ReferenceNumber,
				  PreviousDocument.Schema.CSI_Code
			};
		}

		public string[] GetSupportingDocumentKeys() => supportingDocumentKeys ?? (supportingDocumentKeys = GetSupportingDocumentKeysCore());
		string[] supportingDocumentKeys;

		protected virtual string[] GetSupportingDocumentKeysCore()
		{
			return new string[]
			{
				SupportingDocument.Schema.CSI_DateOfIssue,
				SupportingDocument.Schema.CSI_SubType,
				SupportingDocument.Schema.CSI_Description,
				SupportingDocument.Schema.CSI_ReferenceNumber,
				SupportingDocument.Schema.CSI_Code,
				SupportingDocument.Schema.CSI_Status,
				SupportingDocument.Schema.CSI_UnitOfQuantity,
				SupportingDocument.Schema.CSI_UnitOfQuantity2,
				SupportingDocument.Schema.CSI_UnitOfQuantity3,
				SupportingDocument.Schema.CSI_RX_NKCurrency,
				SupportingDocument.Schema.CSI_RN_NKCountryCode,
				SupportingDocument.Schema.CSI_ReferenceNumber2,
				SupportingDocument.Schema.CSI_DateOfExpiry,
				SupportingDocument.Schema.CSI_Procedure,
				SupportingDocument.Schema.CSI_Tariff
			};
		}

		protected void AddResults(MergeKey mergeKey, IEnumerable<BusinessObject> inners, params string[] fieldNames)
		{
			if (fieldNames.Length > 0 && inners.Any())
			{
				ColumnComparer comparer;
				var innerType = inners.First().GetType();
				if (!comparers.TryGetValue(innerType, out comparer))
				{
					comparer = new ColumnComparer(fieldNames);
					comparers[innerType] = comparer;
				}
				foreach (BusinessObject bizO in inners.OrderBy((x) => x, comparer))
				{
					foreach (string fieldName in fieldNames)
					{
						mergeKey.Add((IZType)bizO[fieldName]);
					}
				}
			}
		}

		readonly Dictionary<Type, ColumnComparer> comparers;

		protected override bool CanCreateEntryLineCore(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.CanCreateEntryLineCore(entryHeader, invoiceLine);
			var euEntryHeader = (CusEntryHeader)entryHeader;
			var euInvoiceline = (JobComInvoiceLine)invoiceLine;
			if (euEntryHeader.LockNumberOfEntryLines)
			{
				result = false;
				euInvoiceline.Validation.AddNotAllowCreateNewEntryLineError(euEntryHeader);
			}
			else
			{
				euInvoiceline.ShouldKeepNotAllowCreateNewEntryLineError = false;
			}
			return result;
		}

		protected IZType GetUnitQuantityKey(BaseJobComInvoiceLine invoiceLine, ZString unitQuantity)
		{
			var isNoMerge = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Declaration.Factory, Declaration.GetDefaultDataGroupingCode(),
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, invoiceLine.EffectiveAssessmentDate, unitQuantity, RefCusCodeListAttributeTypes.Codes.NoMerge).Any();
			return isNoMerge ? ZGuid.NewZGuid() : unitQuantity;
		}

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.LineIsValidForMerge(baseInvoiceLine);
			if (result && baseInvoiceLine.EntryInstruction?.EntryHeader is CusEntryHeader entryHeader)
			{
				if (entryHeader.LockNumberOfEntryLines && baseInvoiceLine.JI_CL.IsEmpty)
				{
					result = false;
					((JobComInvoiceLine)baseInvoiceLine).Validation.AddNotAllowCreateNewEntryLineError(entryHeader);
				}
			}
			return result;
		}

		protected bool IsUCC6AndIsExport { get; }
		protected bool IsUCC6AndIsImport { get; }

		protected bool ForbidMergingInvoiceLinesWithRatioBasedTariff(JobComInvoiceLine invoiceLine)
		{
			return EUCustomsDataRegistry.Instance.ForbidMergingInvoiceLinesWithRatioBasedTariff.Value
				&& invoiceLine.UniversalTariff != null
				&& invoiceLine.UniversalTariff.Conditions.Any(x => x.ConditionClass == Enterprise.Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class);
		}
	}

	class ColumnComparer : IComparer<BusinessObject>
	{
		public ColumnComparer(string[] columns)
		{
			this.columns = columns;
		}

		readonly string[] columns;

		int IComparer<BusinessObject>.Compare(BusinessObject x, BusinessObject y)
		{
			int result = 0;
			foreach (string fieldName in columns)
			{
				IZType propertyX = (IZType)x[fieldName];
				IZType propertyY = (IZType)y[fieldName];
				result = propertyX.CompareTo(propertyY);
				if (result != 0)
				{
					break;
				}
			}
			return result;
		}
	}
}
