using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADDeclaration : ICADMessageDeclaration
{
	public CADDeclaration(JobDeclaration declaration, MessageSubTypes messageSubType, ZString versionID, CADCorrectionMessageSendingActionCollection amendmentActions)
	{
		this.declaration = declaration;
		this.messageSubType = messageSubType;
		this.amendmentActions = amendmentActions;
		this.versionID = versionID;
		this.isLVS = declaration.IsLVS;
		var decMessageSubType = declaration.JE_MessageSubType;
		this.isWarehouse = CADEntryTypeList.WarehouseEntryTypes.Contains(decMessageSubType) && decMessageSubType != CADEntryTypeList.Codes.ExWarehouse22;
	}

	public CADDeclaration(ZString versionID)
	{
		this.versionID = versionID;
	}

	readonly ZString versionID;
	readonly CADCorrectionMessageSendingActionCollection amendmentActions;
	readonly JobDeclaration declaration;
	readonly MessageSubTypes messageSubType;
	readonly bool isLVS;
	readonly bool isWarehouse;

	#region ICADMessageDeclaration Members

	string ICADMessageDeclaration.FunctionCode
	{
		get
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case MessageSubTypes.Create:
					result = CADMessageFunctionCodes.Codes.Original;
					break;
				case MessageSubTypes.Change:
				case MessageSubTypes.Amend:
					result = CADMessageFunctionCodes.Codes.Change;
					break;
				default:
					result = CADMessageFunctionCodes.Codes.Default;
					break;
			}
			return result;
		}
	}

	string ICADMessageDeclaration.FunctionalReferenceID
	{
		get
		{
			return declaration.JE_DeclarationReference;
		}
	}

	string ICADMessageDeclaration.ID
	{
		get
		{
			return declaration.ParentRelatedDeclaration != null ? declaration.CA_OriginalTransactionNo : declaration.B3EntryHeader.CH_BGMReference;
		}
	}

	string ICADMessageDeclaration.LanguageCode => "EN";

	string ICADMessageDeclaration.TypeCode
	{
		get
		{
			if (isLVS)
			{
				return FType;
			}
			return declaration.JE_MessageSubType.SubstringSafe(0, 2);
		}
	}

	internal const string FType = "F";

	string ICADMessageDeclaration.VersionID
	{
		get
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case MessageSubTypes.Change:
				case MessageSubTypes.Amend:
					result = versionID;
					break;
				default:
					result = ZString.Empty;
					break;
			}
			return result;
		}
	}

	decimal ICADMessageDeclaration.TotalGrossMassMeasure
	{
		get
		{
			var result = ZDecimal.Zero;
			if (isLVS)
			{
				result = 1m;
			}
			else
			{
				var messageSubType = declaration.JE_MessageSubType;
				if (messageSubType == CADEntryTypeList.Codes.ExWarehouse201
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse211
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse212
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse213
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse214
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse215
					|| messageSubType == CADEntryTypeList.Codes.ExWarehouse216
					|| messageSubType == CADEntryTypeList.Codes.TransferOfGoods301
					|| messageSubType == CADEntryTypeList.Codes.TransferOfGoods302)
				{
					result = ZDecimal.Zero;
				}
				else
				{
					result = Utilities.Round(declaration.GrossWeightInKilos(), 0);
					if (result.IsEmpty)
					{
						result = 1m;
					}
				}
			}
			return result;
		}
	}

	IEnumerable<ICADMessageDeclarationAdditionalDocument> ICADMessageDeclaration.AdditionalDocument
	{
		get
		{
			if (!(isLVS || declaration.IsWarehouseNotInType))
			{
				var cargoControlNumbers = (declaration.ParentRelatedDeclaration ?? declaration).CargoControlNumbers;
				foreach (var ccn in cargoControlNumbers)
				{
					yield return ccn;
				}
			}
		}
	}

	IEnumerable<ICADMessageDeclarationAdditionalInformation> ICADMessageDeclaration.AdditionalInformation
	{
		get
		{
			if (declaration.ParentRelatedDeclaration != null)
			{
				if (messageSubType == MessageSubTypes.Create)
				{
					yield return new CADDeclarationAdditionalInformation(statementCode: StatementCodes.Codes.AsDeclared, statementTypeCode: AdditionalInformationTypeCodes.Codes.PRE);
				}
				else
				{
					yield return new CADDeclarationAdditionalInformation(statementCode: StatementCodes.Codes.AsAdjusted, statementTypeCode: AdditionalInformationTypeCodes.Codes.PRE);
				}
			}
			else
			{
				if (isWarehouse)
				{
					yield return new CADDeclarationAdditionalInformation(statementCode: declaration.JE_MessageSubType, statementTypeCode: AdditionalInformationTypeCodes.Codes.STC);
				}
			}
		}
	}

	IEnumerable<ICADMessageDeclarationAmendment> ICADMessageDeclaration.Amendment => (messageSubType == MessageSubTypes.Change || messageSubType == MessageSubTypes.Amend)
		? CADDeclarationAmendment.GenerateAmendmentsBySendingActions(amendmentActions) : Enumerable.Empty<ICADMessageDeclarationAmendment>();

	string ICADMessageDeclaration.BorderTransportMeans
	{
		get
		{
			if (isLVS)
			{
				return "02";
			}
			return TransportTypeList.GetTransportModeNumber(declaration.JE_TransportMode).PadLeft(2, '0');
		}
	}

	string ICADMessageDeclaration.CarrierID
	{
		get
		{
			if (declaration.IsTypeF || declaration.IsWarehouseNotInType)
			{
				return ZString.Empty;
			}
			return declaration.JE_CarrierCode;
		}
	}

	decimal ICADMessageDeclaration.ConsignmentFreightRate
	{
		get
		{
			if (isLVS)
			{
				return 1;
			}

			var rate = ZDecimal.Zero;
			if (!declaration.IsWarehouseNotInType)
			{
				var hasUSEOC = false;
				var cad = RefCurrency.LoadFromCurrencyCode(declaration.Factory, Core.Constants.CurrencyCodes.Canada);
				foreach (JobComInvoiceHeader invoice in declaration.Invoices)
				{
					if (invoice.IsUSorTerritory && invoice.TotalValueForDuty >= VFDLimitForCAD)
					{
						hasUSEOC = true;
						var amount = new Money(invoice.OverseasFreight.Amount, invoice.OverseasFreight.Currency);
						rate += invoice.CurrencyConverter.ConvertExact(amount, cad).Amount;
					}
				}

				if (hasUSEOC && rate.IsEmpty)
				{
					rate = declaration.CalculatedFreightAmount;
				}
			}

			rate = rate.Round(0);
			return rate >= 0 && rate < 1 ? 1 : rate;
		}
	}

	const decimal VFDLimitForCAD = 3300m;

	string ICADMessageDeclaration.DeclarantID
	{
		get
		{
			var brokerBusinessNumber = declaration.BrokerBusinessNumber;
			var importerID = ((ICADMessageDeclaration)this).ImporterID;
			return brokerBusinessNumber == importerID ? ZString.Empty : brokerBusinessNumber;
		}
	}

	string ICADMessageDeclaration.ImporterID
	{
		get
		{
			var result = string.Empty;
			if (declaration.IsConsolidatedLVS)
			{
				result = IB3HeaderHelper.GetLVSBusinessNumber(declaration.Importer);
			}
			else
			{
				result = IB3HeaderHelper.GetBusinessNumber(declaration.IsExistingEffectiveCasualImport, declaration.ImporterOfRecord, declaration.Importer);
			}
			return result;
		}
	}

	ICADMessageDeclarationPreviousDocument ICADMessageDeclaration.PreviousDocument
	{
		get
		{
			if (declaration.IsWarehouseNotInType)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					foreach (var dutyAndTax in invoiceLine.DutiesAndTaxes)
					{
						var previousTransactionNumber = dutyAndTax.C1_PreviousTranNumber;
						if (!previousTransactionNumber.IsEmpty)
						{
							return new CADDeclarationPreviousDocument(id: previousTransactionNumber, typeCode: "632");
						}
					}
				}
			}
			return null;
		}
	}

	ICADMessageDeclarationReleaseLocation ICADMessageDeclaration.ReleaseLocation
	{
		get
		{
			return new CADDeclarationReleaseLocation(declaration, isWarehouse);
		}
	}

	string ICADMessageDeclaration.ReleaseDateTime
	{
		get
		{
			if (isLVS)
			{
				var lastDatePeriod = new ZDate(declaration.JE_PeriodYear, declaration.JE_PeriodMonth, 1).AddMonths(1).AddDays(-1);
				return ZDate.Today >= lastDatePeriod ? lastDatePeriod.ToString("yyyyMMdd") : declaration.JE_EntryAuthorisationDate.ToString("yyyyMMdd");
			}
			return declaration.JE_EntryAuthorisationDate.ToString("yyyyMMdd");
		}
	}

	string ICADMessageDeclaration.UnloadingLocationID
	{
		get
		{
			var result = ZString.Empty;
			if (!(declaration.IsTypeF || declaration.IsWarehouseNotInType))
			{
				result = declaration.CA_UnladingOffice;
			}
			return result;
		}
	}

	IEnumerable<ICADMessageDeclarationGoodsShipment> ICADMessageDeclaration.GoodsShipment
	{
		get
		{
			var list = new List<ICADMessageDeclarationGoodsShipment>();
			if (declaration.IsLVS)
			{
				var wrapper = new B3ImportMessageWrapper(declaration.B3EntryHeader, true);
				wrapper.GetB3SubHeaders().ForEach(x => list.Add(new CADLVSGoodsShipmentWrapper(x, messageSubType)));
			}
			else
			{
				declaration.Invoices.Cast<JobComInvoiceHeader>().ForEach(x => list.Add(new CADGoodsShipmentWrapper(x, messageSubType, isWarehouse)));
			}
			return list;
		}
	}

	#endregion
}
