using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DE.Business.BondedWarehousingHelper.Constants;
using static Enterprise.Customs.EU.Business.BondedWarehousingHelper.Constants;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business
{
	public static class CusEntryInstructionInwardRelatedFieldChangeChecker
	{
		public static bool BondedWhsFieldsChangedAndShouldUpdate(this CusEntryInstruction entryInstruction)
		{
			var shouldSendUpdate = false;
			if (GetCanUpdateWarehouseBooking(entryInstruction))
			{
				var factory = entryInstruction.Factory;
				var declaration = entryInstruction.JobDeclaration;
				var docketLines = GetDocketLines(declaration, factory);
				var declarationChecked = false;
				var checkedInvoiceHeaders = new List<ZGuid>();
				foreach (var docketLine in docketLines)
				{
					var bwhAttribute = GetBwhAttributeFromDocketLine(docketLine, factory);

					if (!declarationChecked)
					{
						shouldSendUpdate = DeclarationFieldsChanged(declaration, bwhAttribute);
						declarationChecked = true;
					}

					var invoiceLine = entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().SingleOrDefault(x => x.JI_MatchingKey == bwhAttribute.WB_MatchingKey);

					if (invoiceLine is not null)
					{
						var invoiceHeader = invoiceLine.InvoiceHeader;
						if (!checkedInvoiceHeaders.Contains(invoiceHeader.PK) && !shouldSendUpdate)
						{
							shouldSendUpdate = InvoiceHeaderFieldsChanged(invoiceHeader, bwhAttribute, factory);
							checkedInvoiceHeaders.Add(invoiceHeader.PK);
							if (!shouldSendUpdate)
							{
								shouldSendUpdate = InvoiceHeaderSupportingDocumentsChanged(invoiceHeader, bwhAttribute, factory);
							}
						}

						if (!shouldSendUpdate)
						{
							shouldSendUpdate = InvoiceLineFieldsChanged(invoiceLine, docketLine, bwhAttribute);
						}

						if (!shouldSendUpdate)
						{
							shouldSendUpdate = InvoiceLineSupportingDocumentsChanged(invoiceLine, bwhAttribute, factory);
						}

						if (!shouldSendUpdate)
						{
							shouldSendUpdate = InvoiceChargesChanged(invoiceLine, bwhAttribute, factory);
						}

						if (shouldSendUpdate)
						{
							break;
						}
					}
					else
					{
						shouldSendUpdate = true;
					}
				}
			}

			return shouldSendUpdate;
		}

		static bool GetCanUpdateWarehouseBooking(CusEntryInstruction entryInstruction)
		{
			var entryHeader = entryInstruction.EntryHeader;
			return DeclarationTypeForWarehouseUpdate.Contains(entryInstruction.CEI_Style) && entryHeader != null &&
					!entryHeader.CH_WarehouseTransactionStatus.IsEmpty && entryHeader.IsIntoWarehouseWarehousing;
		}

		static readonly ImmutableHashSet<string> DeclarationTypeForWarehouseUpdate = new HashSet<string>(new[] { ImportDeclarationTypeList.Codes.VZL, ImportDeclarationTypeList.Codes.AZL }).ToImmutableHashSet();

		static bool DeclarationFieldsChanged(JobDeclaration declaration, IWhsBondedWarehouseAttribute bwhAttribute)
		{
			var provider = new DeclarationGroupingDefinitionProvider(bwhAttribute, declaration.Factory);

			var declarationChanged = declaration.SupplierDocumentaryAddress.E2_OA_Address != provider.SupplierAddressPK ||
								declaration.ImporterDocumentaryAddress.E2_OA_Address != provider.ImporterAddressPK ||
								declaration.JE_RL_NKPortOfLoading != provider.PortOfLoading ||
								declaration.JE_RL_NKPortOfFirstArrival != provider.FirstEUArrival ||
								declaration.JE_TransportMode != provider.Transport ||
								declaration.JE_OA_ConsigneeAddress != provider.Buyer ||
								declaration.JE_OA_SellerAddress != provider.Seller;
			return declarationChanged;
		}

		static bool InvoiceHeaderFieldsChanged(JobComInvoiceHeader invoiceHeader, IWhsBondedWarehouseAttribute bwhAttribute, BusinessObjectFactory factory)
		{
			var provider = new InvoiceHeaderGroupingDefinitionProvider(bwhAttribute, factory);

			var invoiceHeaderChanged = invoiceHeader.JZ_InvoiceNumber != provider.InvoiceNumber ||
								invoiceHeader.JZ_InvoiceDate != provider.InvoiceDate ||
								invoiceHeader.JZ_RX_NKInvoice_Currency != provider.LinePriceCurrency ||
								invoiceHeader.JZ_IncoTerm != provider.IncotermCode ||
								invoiceHeader.JZ_IncoTermPlace != provider.IncotermPlace ||
								invoiceHeader.JZ_ValuationCode != provider.TransNature;

			return invoiceHeaderChanged;
		}

		static bool InvoiceLineFieldsChanged(JobComInvoiceLine invoiceLine, IWhsDocketLine docketLine, IWhsBondedWarehouseAttribute bwhAttribute)
		{
			var provider = new InvoiceLineAddInfoDefinitionProvider(bwhAttribute);

			var invoiceLineChanged = invoiceLine.JI_Tariff != bwhAttribute.WB_Tariff ||
								invoiceLine.JI_CountryOfOrigin != bwhAttribute.WB_RN_NKCountryOfOrigin ||
								invoiceLine.ZG_CountryOfSupply != provider.CountryOfSupply ||
								invoiceLine.JI_PrimaryPreference != bwhAttribute.WB_PrimaryPreference ||
								invoiceLine.JI_LinePrice != provider.LinePrice ||
								invoiceLine.JI_NetPrice != provider.LineNetPrice ||
								(invoiceLine.Part?.PK ?? ZGuid.Empty) != docketLine.WE_OP ||
								(invoiceLine.JI_NetWeight.IsEmpty ? invoiceLine.JI_CustomsQuantity : invoiceLine.JI_NetWeight) != bwhAttribute.WB_CustomsQty ||
								(invoiceLine.JI_NetWeight.IsEmpty ? invoiceLine.JI_CustomsUnitQty : invoiceLine.JI_NetWeightUQ) != bwhAttribute.WB_CustomsUnitOfQty ||
								invoiceLine.JI_CustomsSecondQuantity != bwhAttribute.WB_CustomsSecondQuantity ||
								invoiceLine.JI_CustomsSecondUnitQty != bwhAttribute.WB_CustomsSecondUnitQty ||
								invoiceLine.JI_CustomsThirdQuantity != bwhAttribute.WB_CustomsThirdQuantity ||
								invoiceLine.JI_CustomsThirdUnitQty != bwhAttribute.WB_CustomsThirdUnitQty ||
								invoiceLine.JI_BondedWhsQuantity != docketLine.WE_TransactionQuantity ||
								invoiceLine.JI_BondedWhsUnitQty != docketLine.WE_F3_NKPackType ||
								invoiceLine.JI_CustomsValue != bwhAttribute.WB_ValueForDuty;

			return invoiceLineChanged;
		}

		static bool InvoiceHeaderSupportingDocumentsChanged(JobComInvoiceHeader invoiceHeader, IWhsBondedWarehouseAttribute bwhAttribute, BusinessObjectFactory factory)
		{
			var addInfoQuery = new ZDBOnlyQuery(typeof(CusAddInfo));
			addInfoQuery.AddToFilter(CusAddInfoSchema.B7_ParentID, bwhAttribute.PK);
			addInfoQuery.AddToFilter(CusAddInfoSchema.B7_Type, InvoiceSupportingDocumentAddInfoTypes.InvoiceHeader);
			var addInfoEntries = factory.Load<WarehouseCustomsAddInfo>(addInfoQuery);

			var supportingDocuments = invoiceHeader.SupportingDocuments.Cast<SupportingDocument>().ToList();

			var headerSupportingDocumentsChanged = addInfoEntries.Length != supportingDocuments.Count;
			if (!headerSupportingDocumentsChanged)
			{
				foreach (var addInfo in addInfoEntries)
				{
					var provider = new InvoiceSupportingDocumentAddInfoDefinitionProvider(addInfo);
					var matchedSupportingDocument = supportingDocuments.FirstOrDefault(sd =>
						sd.CSI_Code == provider.Type &&
						sd.CSI_ReferenceNumber == provider.Reference &&
						sd.CSI_DateOfIssue == provider.DateOfIssue
					);

					if (matchedSupportingDocument != null)
					{
						supportingDocuments.Remove(matchedSupportingDocument);
					}
					else
					{
						headerSupportingDocumentsChanged = true;
						break;
					}
				}
			}

			return headerSupportingDocumentsChanged;
		}

		static bool InvoiceLineSupportingDocumentsChanged(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute bwhAttribute, BusinessObjectFactory factory)
		{
			var addInfoQuery = new ZDBOnlyQuery(typeof(WarehouseCustomsAddInfo));
			addInfoQuery.AddToFilter(CusAddInfoSchema.B7_ParentID, bwhAttribute.PK);
			addInfoQuery.AddToFilter(CusAddInfoSchema.B7_Type, InvoiceSupportingDocumentAddInfoTypes.InvoiceLine);
			var addInfoEntries = factory.Load<WarehouseCustomsAddInfo>(addInfoQuery);

			var supportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().ToList();

			var lineSupportingDocumentsChanged = addInfoEntries.Length != supportingDocuments.Count;

			if (!lineSupportingDocumentsChanged)
			{
				foreach (var addInfo in addInfoEntries)
				{
					var provider = new InvoiceSupportingDocumentAddInfoDefinitionProvider(addInfo);
					var matchedSupportingDocument = supportingDocuments.FirstOrDefault(sd =>
						sd.CSI_Code == provider.Type &&
						sd.CSI_ReferenceNumber == provider.Reference &&
						sd.CSI_DateOfIssue == provider.DateOfIssue &&
						sd.CSI_Status == provider.Available &&
						sd.CSI_Quantity == provider.Quantity &&
						sd.CSI_UnitOfQuantity == provider.UnitOfMeasure
					);

					if (matchedSupportingDocument != null)
					{
						supportingDocuments.Remove(matchedSupportingDocument);
					}
					else
					{
						lineSupportingDocumentsChanged = true;
						break;
					}
				}
			}
			return lineSupportingDocumentsChanged;
		}

		static bool InvoiceChargesChanged(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute bwhAttribute, BusinessObjectFactory factory)
		{
			var chargesAddInfoQuery = new ZDBOnlyQuery(typeof(CusAddInfo));
			chargesAddInfoQuery.AddToFilter(CusAddInfoSchema.B7_ParentID, bwhAttribute.PK);
			chargesAddInfoQuery.AddToFilter(CusAddInfoSchema.B7_Type, CommercialChargeAddInfo.Type);
			var chargeAddInfoEntries = factory.Load<WarehouseCustomsAddInfo>(chargesAddInfoQuery);

			var charges = invoiceLine.Charges.Cast<BaseJobComInvHeaderCharge>().ToList();
			charges.AddRange(invoiceLine.ApportionedCharges.Cast<BaseJobComInvHeaderCharge>().ToList());

			var chargesChanged = chargeAddInfoEntries.Length != charges.Count;
			if (!chargesChanged)
			{
				foreach (var chargeAddInfo in chargeAddInfoEntries)
				{
					var provider = new InvoiceLineChargeDefinitionProvider(chargeAddInfo);

					var matchedCharge = charges.FirstOrDefault(c => c.J7_Amount == provider.Amount &&
																	c.J7_ChargeType == provider.ChargeType &&
																	c.J7_RX_NKCurrency == provider.Currency &&
																	c.J7_IsDutiable == provider.IsDutiable &&
																	c.J7_IsGSTApplicable == provider.IsGSTApplicable &&
																	c.J7_IsIncludedInITOT == provider.IsIncludedInITOT &&
																	c.J7_IsStatisticalValueApplicable == provider.IsStatisticalValueApplicable);

					if (matchedCharge != null)
					{
						charges.Remove(matchedCharge);
					}
					else
					{
						chargesChanged = true;
						break;
					}
				}
			}
			return chargesChanged;
		}

		static IWhsDocketLine[] GetDocketLines(JobDeclaration declaration, BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.Equal, declaration.JE_UCR);
			query.OrderBy = WhsDocketSchema.WD_ExternalReferenceSplit.Name + OrderByClause.Descending;
			var docket = factory.LoadTop1<IWhsDocket>(query);

			var docketLines = Array.Empty<IWhsDocketLine>();
			if (docket != null)
			{
				var docketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK);
				docketLines = factory.Load<IWhsDocketLine>(docketLineQuery);
			}

			return docketLines;
		}

		static IWhsBondedWarehouseAttribute GetBwhAttributeFromDocketLine(IWhsDocketLine docketLine, BusinessObjectFactory factory)
		{
			var bwhAttributeQuery = new ZDBOnlyQuery(typeof(IWhsBondedWarehouseAttribute));
			bwhAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLine.PK);
			var bwhAttribute = factory.LoadTop1<IWhsBondedWarehouseAttribute>(bwhAttributeQuery);
			return bwhAttribute;
		}
	}
}
