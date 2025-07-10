using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFEImportObjectParent : NonPersistentBusinessObject
	{
		public NFEImportObjectParent(JobDeclaration declaration) : base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public NFEImportObjectCollection NFEImportObjectCollection
		{
			get
			{
				if (nFEImportObjectCollection == null)
				{
					nFEImportObjectCollection = new NFEImportObjectCollection(Factory);
					RegisterEditableChildObject(nFEImportObjectCollection);
				}
				return nFEImportObjectCollection;
			}
		}

		NFEImportObjectCollection nFEImportObjectCollection;

		#region Add NFEImportObject

		public NFEImportObject ProcessStreamAndAddNew(Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				NFEImportObject newElement = null;
				var nfeProc = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<nfeProc>(reader.ReadToEnd());
				var infNFe = nfeProc?.NFe?.infNFe;
				var nfeId = infNFe?.Id;

				if (!string.IsNullOrEmpty(nfeId))
				{
					var nfeKey = nfeId.StartsWith("NFe") ? nfeId.Replace("NFe", "") : nfeId;

					if (!NFEImportObjectCollection.Cast<NFEImportObject>().Any(x => x.NfeKey == nfeKey))
					{
						newElement = NFEImportObjectCollection.AddNew();
						newElement.NfeKey = nfeKey;
						newElement.NfeSerie = infNFe.ide?.serie.ToString() ?? ZString.Empty;
						newElement.NfeNumber = infNFe.ide?.nNF.ToString() ?? ZString.Empty;
						newElement.NfeGrossWeight = infNFe.transp?.vol?.pesoB ?? ZDecimal.Zero;
						newElement.NfeNetWeight = infNFe.transp?.vol?.pesoL ?? ZDecimal.Zero;
						newElement.NfeInvoiceAmount = infNFe.total?.ICMSTot?.vProd ?? ZDecimal.Zero;
						newElement.NfeDate = infNFe.ide != null && DateTimeOffset.TryParse(infNFe.ide.dhEmi, out DateTimeOffset date) ? date.Date : ZDateTime.Empty;
						newElement.NfeTotalFreight = infNFe.total?.ICMSTot?.vFrete ?? ZDecimal.Zero;
						newElement.NfeTotalInsurance = infNFe.total?.ICMSTot?.vSeg ?? ZDecimal.Zero;
						newElement.NfeTotalOtherCharges = infNFe.total?.ICMSTot?.vOutro ?? ZDecimal.Zero;
						newElement.NfeTotalDiscount = infNFe.total?.ICMSTot?.vDesc ?? ZDecimal.Zero;
						newElement.Items.AddNewItems(infNFe.det);
						newElement.Declaration = Declaration;
						newElement.ObjectParent = this;
						newElement.SupplierID = infNFe.emit?.CNPJ;
					}
				}

				return newElement;
			}
		}

		#endregion

		#region ImportInvoices

		public readonly JobDeclaration Declaration;

		public Action<int, int, string> InvoiceImported;

		public void ImportInvoices()
		{
			Declaration.IsImportingData = true;

			try
			{
				var completedCount = 0;
				var totalCount = NFEImportObjectCollection.Count;
				ZGuid? newEntryInstructionPK = null;

				foreach (NFEImportObject nfe in NFEImportObjectCollection)
				{
					var message = string.Empty;
					JobComInvoiceHeader invoiceHeader = null;

					var invoiceNumber = nfe.NfeNumber.Left(JobComInvoiceHeader.Schema.JZ_InvoiceNumberMaxLength);

					if (nfe.InvoiceHeaderPK.IsValid)
					{
						invoiceHeader = nfe.InvoiceHeader;

						if (invoiceHeader != null)
						{
							if (string.IsNullOrEmpty(invoiceHeader.JZ_IncoTerm)
								|| string.IsNullOrEmpty(invoiceHeader.JZ_RX_NKInvoice_Currency))
							{
								message = Res.GetString("F6B6F516-7359-40B6-80B2-4C634AAD5220", "Inv. Header: {0} - NF-e {1} - Error - Incoterm and/or Currency were not entered.", invoiceHeader.JZ_InvoiceNumber, nfe.NfeNumber);
								invoiceHeader = null;
							}
							else
							{
								message = Res.GetString("ff615f21-bfe5-4c0a-9b05-59804c5d5519", "Inv. Header: {0} - NF-e {1} lines successfully imported into existing Inv. Header.", invoiceHeader.JZ_InvoiceNumber, nfe.NfeNumber);
							}
						}
					}
					else if (Declaration.Invoices.Any(x => x.JZ_InvoiceNumber == invoiceNumber))
					{
						message = Res.GetString("05bf166a-1523-40f2-ae84-dc5fbf843865", "Inv. Header: {0} - NF-e {1} - Error - Invoice with the same Invoice Number already exists.", invoiceNumber, nfe.NfeNumber);
					}
					else
					{
						invoiceHeader = Declaration.Invoices.AddNew();
						invoiceHeader.JZ_InvoiceNumber = invoiceNumber;
						invoiceHeader.JZ_IncoTerm = nfe.Incoterm.Left(invoiceHeader.JZ_IncoTermInfo.MaxLength);
						invoiceHeader.JZ_RX_NKInvoice_Currency = nfe.CurrencyCode.Left(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo.MaxLength);

						message = Res.GetString("75418ad7-aa95-431c-aab2-9054dfdd47ec", "Inv. Header: {0} - NF-e {1} - New invoice created, lines successfully imported into Inv. Header.", invoiceHeader.JZ_InvoiceNumber, nfe.NfeNumber);
					}

					if (invoiceHeader != null)
					{
						var exchangeRate = nfe.ExchangeRate;
						invoiceHeader.JZ_InvoiceDate = nfe.NfeDate;
						var supplierPK = GetSupplierFromCNPJ(nfe.SupplierID);
						if (supplierPK.IsValid)
						{
							invoiceHeader.JZ_OH_Supplier = supplierPK;
						}

						var invoiceAmount = ConvertCurrency(nfe.NfeInvoiceAmount, exchangeRate);
						var freight = ConvertCurrency(nfe.NfeTotalFreight, exchangeRate);
						var insurance = ConvertCurrency(nfe.NfeTotalInsurance, exchangeRate);
						var otherCharges = ConvertCurrency(nfe.NfeTotalOtherCharges, exchangeRate);
						var discount = ConvertCurrency(nfe.NfeTotalDiscount, exchangeRate);
						var grossWeight = ReturnZeroIfNegative(nfe.NfeGrossWeight);
						var netWeight = ReturnZeroIfNegative(nfe.NfeNetWeight);

						var entryInstructionPK = nfe.EntryInstructionPK;
						if (Declaration.IsPersistent && !entryInstructionPK.IsValid)
						{
							entryInstructionPK = (newEntryInstructionPK ?? (newEntryInstructionPK = CreateEntryInstruction())).Value;
						}
						CreateInvoiceLines(nfe, invoiceHeader, entryInstructionPK);

						var invoiceLines = invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();

						DistributeToChargeAmount(invoiceLines, Common.CustomsChargeTypeList.Codes.OverseasFreight, freight);
						DistributeToChargeAmount(invoiceLines, Common.CustomsChargeTypeList.Codes.OverseasInsurance, insurance);
						DistributeToChargeAmount(invoiceLines, Common.CustomsChargeTypeList.Codes.OtherCharges, otherCharges);
						DistributeToChargeAmount(invoiceLines, Common.CustomsChargeTypeList.Codes.Discount, discount);
						DistributeToLinePrice(invoiceLines, invoiceAmount);

						if (nfe.InvoiceHeaderPK.IsEmpty)
						{
							invoiceHeader.JZ_InvoiceAmount = (invoiceAmount + freight + insurance + otherCharges) - discount;
							invoiceHeader.JZ_Weight = grossWeight;
							invoiceHeader.JZ_NetWeight = netWeight;
						}
					}

					InvoiceImported?.Invoke(++completedCount, totalCount, message);
				}
			}
			finally
			{
				Declaration.IsImportingData = false;
			}
		}

		void CreateInvoiceLines(NFEImportObject nfe, JobComInvoiceHeader invoice, ZGuid entryInstructionPK)
		{
			foreach (NFEImportObjectItem item in nfe.Items)
			{
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = item.ProductCode.Left(invoiceLine.JI_PartNoInfo.MaxLength);
				invoiceLine.FullGoodsDescription = item.GoodsDescription.Left(invoiceLine.FullGoodsDescriptionInfo.MaxLength);
				invoiceLine.JI_NFeNumber = nfe.NfeKey.Left(invoiceLine.JI_NFeNumberInfo.MaxLength);
				invoiceLine.JI_NFeItemNumber = item.NfeItemNumber.Left(invoiceLine.JI_NFeItemNumberInfo.MaxLength);
				invoiceLine.JI_Tariff = item.TariffCode.Left(invoiceLine.JI_TariffInfo.MaxLength);
				invoiceLine.JI_InvoiceUQ = item.InvoiceQuantityUQ.Left(invoiceLine.JI_InvoiceUQInfo.MaxLength);
				invoiceLine.JI_InvoiceQuantity = item.InvoiceQuantity;
				invoiceLine.JI_CustomsUnitQty = item.CustomsQuantityUQ.Left(invoiceLine.JI_CustomsUnitQtyInfo.MaxLength);
				invoiceLine.JI_CustomsQuantity = item.CustomsQuantity;
				invoiceLine.JI_LinePrice = ConvertCurrency(item.TotalValue, nfe.ExchangeRate);
				invoiceLine.JI_NFeLinePrice = item.TotalNfeValue;

				if (entryInstructionPK.IsValid)
				{
					invoiceLine.JI_CEI = entryInstructionPK;
				}
				invoiceLine.ComplementaryDescription = item.ComplementartDescription.Left(invoiceLine.ComplementaryDescriptionInfo.MaxLength);
				CreateCharges(item, invoiceLine, invoice.JZ_RX_NKInvoice_Currency, nfe);
			}
		}

		void DistributeToChargeAmount(JobComInvoiceLine[] invoiceLines, ZString chargeCode, ZDecimal totalValue)
		{
			var charges = invoiceLines.SelectMany(l => l.Charges.GetCharge(chargeCode)).ToArray();
			DistributeValueTo(charges, totalValue, x => x.J7_AmountInfo as ZPropertyInfoDecimal);
		}

		void DistributeToLinePrice(JobComInvoiceLine[] invoiceLines, ZDecimal totalValue)
		{
			DistributeValueTo(invoiceLines, totalValue, x => x.JI_LinePriceInfo as ZPropertyInfoDecimal);
		}

		void DistributeValueTo<T>(T[] distrubutees, ZDecimal totalValue, Func<T, ZPropertyInfoDecimal> getValueInfo)
		{
			DistributeTo(distrubutees, totalValue, getValueInfo);
		}

		void DistributeTo<T>(T[] distrubutees, ZDecimal totalValueToDistribute, Func<T, ZPropertyInfoDecimal> getValueInfo, short decimalPlaces = 2)
		{
			if (distrubutees.Length == 1)
			{
				getValueInfo(distrubutees[0]).Value = totalValueToDistribute;
			}
			else if (distrubutees.Length > 0)
			{
				ZDecimal minDiff = Math.Pow(0.1, decimalPlaces);
				int factorToInt = (int)Math.Pow(10, decimalPlaces);

				var totalValueOnLines = distrubutees.Cast<T>().Sum(c => getValueInfo(c).Value);
				var diff = new ZDecimal(totalValueToDistribute - totalValueOnLines).Truncate(decimalPlaces);
				if (diff != 0 && totalValueOnLines > 0 && totalValueToDistribute > 0)
				{
					int diffToInt = (int)(diff * factorToInt);
					decimal apportionedDiff = diffToInt / distrubutees.Length;
					if (apportionedDiff != 0)
					{
						distrubutees.ForEach(c => getValueInfo(c).Value += apportionedDiff / factorToInt);
					}

					int restOfDiff = Math.Abs(diffToInt) % distrubutees.Length;
					if (restOfDiff > 0)
					{
						distrubutees.Take(restOfDiff).ForEach(c => getValueInfo(c).Value += minDiff * Math.Sign(diff));
					}
				}
			}
		}

		void CreateCharges(NFEImportObjectItem item, JobComInvoiceLine invoiceLine, ZString currency, NFEImportObject nfe)
		{
			CreateChargeByTypeAndValue(invoiceLine, Common.CustomsChargeTypeList.Codes.OverseasFreight, currency, nfe.ExchangeRate, item.FreteValue, nfe.NfeTotalFreight);
			CreateChargeByTypeAndValue(invoiceLine, Common.CustomsChargeTypeList.Codes.OverseasInsurance, currency, nfe.ExchangeRate, item.SegValue, nfe.NfeTotalInsurance);
			CreateChargeByTypeAndValue(invoiceLine, Common.CustomsChargeTypeList.Codes.OtherCharges, currency, nfe.ExchangeRate, item.OutroValue, nfe.NfeTotalOtherCharges);
			CreateChargeByTypeAndValue(invoiceLine, Common.CustomsChargeTypeList.Codes.Discount, currency, nfe.ExchangeRate, item.DescValue, nfe.NfeTotalDiscount);
		}

		void CreateChargeByTypeAndValue(JobComInvoiceLine invoiceLine, ZString chargeType, ZString currency, ZDecimal exchangeRate, ZDecimal value, ZDecimal totalValue)
		{
			if (!value.IsEmpty && !totalValue.IsEmpty)
			{
				var lineCharge = invoiceLine.Charges.AddNew();
				lineCharge.J7_ChargeType = chargeType;
				lineCharge.J7_Amount = ConvertCurrency(value, exchangeRate);
				lineCharge.J7_RX_NKCurrency = currency;
			}
		}

		ZDecimal ConvertCurrency(ZDecimal value, ZDecimal exchangeRate) => decimal.Round(value / (exchangeRate.IsEmpty ? 1 : exchangeRate), 2);

		ZDecimal ReturnZeroIfNegative(ZDecimal value) => value >= 0 ? value : 0;

		ZGuid GetSupplierFromCNPJ(ZString regNo)
		{
			OrgHeader orgHeader = null;

			if (!regNo.IsEmpty)
			{
				var orgCusCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Brazil);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);
				orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regNo);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				query.AddToFilter(OrgHeaderSchema.OH_IsConsignor, true);
				query.AddSubQuery(orgCusCodeQuery, JoinCondition.And);

				orgHeader = Factory.LoadTop1<OrgHeader>(query);
			}

			return orgHeader?.PK ?? ZGuid.Empty;
		}

		ZGuid CreateEntryInstruction()
		{
			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			entryInstruction.CEI_Description = (NoResString)"Created from Import NF-e";
			return entryInstruction.PK;
		}

		#endregion
	}
}
