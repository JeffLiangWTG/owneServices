using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUDeclarationValueObjectDataAdapter : DeclarationValueObjectDataAdapter, Integration.Customs.AU.IAUDeclarationValueObjectDataAdapter
	{
		public AUDeclarationValueObjectDataAdapter()
		{
		}

		public AUDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		protected override void ImportFromValueObjectCore(BaseJobDeclaration bizObj, Xsd.ConsolAndShipment value, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(bizObj, value, context);
			var aUJobDec = bizObj as JobDeclaration;
			if (aUJobDec != null)
			{
				aUJobDec.ApportionmentDirty = true;
				aUJobDec.ResumeApportionment();
			}
		}

		protected override string AddInfoPrefix
		{
			get { return "ZA_"; }
		}

		protected override InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(BaseJobDeclaration jobDec)
		{
			return new AUInvoiceValueObjectDataAdapter(jobDec);
		}

		protected override void ImportDeclarationDetails(BaseJobDeclaration jobDec, Xsd.Declaration declarationXsd, ZString consolMasterBill, IValueObjectImportContext context)
		{
			if (declarationXsd != null)
			{
				var toJobDec = jobDec.Factory.Load<JobDeclaration>(jobDec.PK);
				context.SetPropertyInfoValue(toJobDec.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo, declarationXsd.ManifestID, ForeignKeyType.None, "ManifestID");

				base.ImportDeclarationDetails(jobDec, declarationXsd, consolMasterBill, context);
			}
		}

		protected override void SetShipmentTypeDetailsCore(BaseJobDeclaration jobDec)
		{
			jobDec.JE_MessageType = jobDec.JE_RL_NKPortOfArrival.StartsWith(Core.Constants.CountryCodes.Australia) ? Customs.Business.JobMessageTypeList.Codes.Import : Customs.Business.JobMessageTypeList.Codes.Export;
			if (jobDec.JE_ContainerMode.IsEmpty)
			{
				jobDec.JE_ContainerMode = ((JobDeclaration)jobDec).DefaultContainerMode;
			}
		}

		protected override void ExportEntryHeader(Xsd.CustomsEntry entryHeaderXsd, Customs.Business.CusEntryHeader entryHeader, RefCurrency localCurrency)
		{
			base.ExportEntryHeader(entryHeaderXsd, entryHeader, localCurrency);
			var auEntryHeader = (CusEntryHeader)entryHeader;
			var localTAndIValue = entryHeader.CurrencyConverter.ConvertExact(auEntryHeader.TransportAndInsurance, localCurrency).Amount;
			entryHeaderXsd.TransportAndInsurance = Xsd.FinancialValue.FromAmountAndCurrency(localTAndIValue, localCurrency);
			entryHeaderXsd.CustomsFactor = auEntryHeader.CustomsFactor;
		}

		protected override void ExportOrders(Xsd.Shipment shipment, BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			base.ExportOrders(shipment, jobDec, context);

			var adapter = new OrderValueObjectDataAdapter<Order, Xsd.Order>();
			foreach (var order in jobDec.AttachedOrders)
			{
				var xsdOrder = adapter.ExportToValueObject(order, context);
				shipment.Orders.Add(xsdOrder);
			}
		}

		protected override InvoicesGeneratorFromXSD GetNewInvoicesGenerator(BaseJobDeclaration jobDec)
		{
			return new AUInvoicesGeneratorFromXSD(jobDec);
		}
	}
}
