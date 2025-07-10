
using System.Collections.Generic;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport.Level1FileFormat;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

using CusHAWB = Enterprise.Customs.AU.Declaration.Business.CusHAWB;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationFromAirCargoCreator : DeclarationFromAirCargoCreator
	{
		public UPEDeclarationFromAirCargoCreator(CusHAWB houseAirCargo)
			: base(houseAirCargo)
		{
		}

		public new UPECusHAWB HouseAirCargo
		{
			get { return (UPECusHAWB)base.HouseAirCargo; }
		}

		public void AddAdditionalInvoiceLines(string invoiceNumber, decimal invoiceWeight, string invoiceWeighUQ, BaseJobDeclaration declaration, _500000LineCollection additionalInvoiceLines)
		{
			if (declaration != null && additionalInvoiceLines != null)
			{
				CreateCommercialInvoices(invoiceNumber, invoiceWeight, invoiceWeighUQ, declaration, additionalInvoiceLines);
			}
		}

		protected override ZGuid GetConsigneePK(INotifications notify, BaseJobDeclaration declaration)
		{
			return HouseAirCargo.ImporterOrConsigneeMatchedOrgPK;
		}

		protected override void CreateCore(BaseJobDeclaration declaration, INotifications notify)
		{
			using (declaration.GetValidationSuspender())
			{
				base.CreateCore(declaration, notify);

				declaration.JE_OwnerRef = HouseAirCargo.CS_HAWB.Left(18);
				declaration.JE_AgentsReference = HouseAirCargo.WayBillShort;
				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				declaration.JE_RS_NKServiceLevel = HouseAirCargo.CS_RS_NK_ServiceLevel;

				if (declaration.JE_TotalWeightUnit == Core.Constants.Weight.Pounds)
				{
					declaration.JE_TotalWeight = Core.Constants.Weight.Convert(declaration.JE_TotalWeight, Core.Constants.Weight.Pounds, Core.Constants.Weight.Kilograms);
					declaration.JE_TotalWeight = ZArchitecture.Core.Utilities.Round(declaration.JE_TotalWeight, 1);
					declaration.JE_TotalWeightUnit = Core.Constants.Weight.Kilograms;
				}

				if (HouseAirCargo.Level1Record != null && HouseAirCargo.Level1Record._200000 != null)
				{
					ZDateTime shippedOnBoardDate = HouseAirCargo.Level1Record._200000.ShippedOnBoardDate;
					declaration.JE_DateAtOrigin = shippedOnBoardDate;
					declaration.JE_ExportDate = shippedOnBoardDate;
				}

				declaration.JE_DateOfFirstArrival = declaration.JE_DateOfArrival;

				Level1Record record = HouseAirCargo.Level1Record;
				if (record != null)
				{
					CreateCommercialInvoices(declaration.JE_HouseBill, declaration.JE_TotalWeight, declaration.JE_TotalWeightUnit, declaration, record._500000Lines);
				}
			}
		}

		void CreateCommercialInvoices(string invoiceNumber, decimal invoiceWeight, string invoiceWeightUQ, BaseJobDeclaration declaration, _500000LineCollection commercialInvoices)
		{
			JobComInvoiceHeader invHead = null;
			foreach (_500000Line commercialInvoice in commercialInvoices)
			{
				if (invHead == null)
				{
					invHead = GetInvoiceHeader(invoiceNumber, invoiceWeight, invoiceWeightUQ, declaration, commercialInvoice);
					invHead.JZ_Weight = invoiceWeight;
					invHead.JZ_WeightUQ = invoiceWeightUQ;
				}

				JobComInvoiceLine invLine = invHead.JobComInvoiceLines.AddNew();
				invLine.JI_InvoiceQuantity = commercialInvoice.Quantity;
				invLine.JI_InvoiceUQ = commercialInvoice.UnitOfQuantity;
				invLine.JI_LinePrice = commercialInvoice.Price;
				invHead.JZ_InvoiceAmount += commercialInvoice.Price;
				invLine.JI_Tariff = commercialInvoice.CommodityCode;
				invLine.JI_PartNo = commercialInvoice.PartNumber;
				invLine.JI_CountryOfOrigin = commercialInvoice.CountryOfOrigin;
				invLine.JI_Description = commercialInvoice.Description;
			}
		}

		JobComInvoiceHeader GetInvoiceHeader(string invoiceNumber, decimal invoiceWeight, string invoiceWeightUQ, BaseJobDeclaration declaration, _500000Line commercialInvoice)
		{
			JobComInvoiceHeader result;
			ZQuery invoiceNumberFilter = new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);
			IList<BaseJobComInvoiceHeader> invoiceHeaders = new List<BaseJobComInvoiceHeader>(declaration.Invoices.Find(invoiceNumberFilter));
			if (invoiceHeaders.Count > 0)
			{
				result = (JobComInvoiceHeader)invoiceHeaders[0];
			}
			else
			{
				result = (JobComInvoiceHeader)declaration.Invoices.AddNew();
				result.JZ_InvoiceNumber = invoiceNumber;
				result.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				result.JZ_Weight = invoiceWeight;
				result.JZ_WeightUQ = invoiceWeightUQ;
				RefCurrency invoiceCurrency = RefCurrency.LoadFromCurrencyCode(Factory, commercialInvoice.CurrencyCode);
				if (invoiceCurrency != null)
				{
					result.JZ_RX_NKInvoice_Currency = invoiceCurrency.RX_Code;
				}
			}

			return result;
		}
	}
}
