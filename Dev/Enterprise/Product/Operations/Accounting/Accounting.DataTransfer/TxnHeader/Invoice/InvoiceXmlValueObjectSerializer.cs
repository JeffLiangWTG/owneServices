using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class InvoiceXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public InvoiceXmlValueObjectSerializer(ZString ledger, bool importInSingleFactory)
			: base(typeof(Xsd.TxnHeader), importInSingleFactory)
		{
			this.Ledger = ledger;
		}

		protected override void ReadCollectionFromXml(System.Xml.XmlReader reader, IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObjectImportContext context)
		{
			bool proceed = true;
			if (InvoiceXmlValueObjectSerializer.IsCrossLedger(context))
			{
				if (Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
				{
					QueryUserYesNoEventArgs e = new QueryUserYesNoEventArgs(Res.GetString("719c5927-27ba-427f-bb8e-c433108a5a93", @"You are trying to import AR Transaction. However, {0} has detected that the XML file is configured to AR to AP Cross Ledger Import.
Since you are importing this to AR Ledger, it will still be imported as AR Transaction. 

We recommend that you should check your XML file's structure.

Do you want to proceed?", Core.Constants.ProductName, ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode, GlbCompany.CurrentCompany.GC_Code, ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation.EDICode), false);
					context.QueryUser(e);
					proceed = e.Response;
				}
				else
				{
					QueryUserYesNoEventArgs e = new QueryUserYesNoEventArgs(Res.GetString("34c66445-325b-4f63-b05c-696822ab799d", @"The XML file you are trying to import was created by Company {0}. You are currently logged in to Company {1}.
All Accounts Receivable Transactions contained within this XML file will be imported into company {1} as Accounts Payable Transactions.

Also, Converted AP Transactions Creditor will be set to {2}.

Do you wish to proceed?", ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode, GlbCompany.CurrentCompany.GC_Code, ((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.EDIOrganisation.EDICode), false);
					context.QueryUser(e);
					proceed = e.Response;
				}
			}

			if (proceed)
			{
				if (Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
				{
					((FinancialInvoiceDataAdapter)dataAdapter).DisableCrossLedgerImport = true;
				}
				base.ReadCollectionFromXml(reader, dataAdapter, collection, context);
			}
		}

		public static bool IsCrossLedger(IValueObjectImportContext context)
		{
#if DEBUG
			if (IsCrossLedgerOverrideforTest.HasValue)
			{
				return IsCrossLedgerOverrideforTest.Value;
			}
#endif
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;

			return !(interchange == Xsd.XmlInterchange.Empty ||
				interchange.InterchangeInfo.Source.EnterpriseCode.IsEmpty ||
				interchange.InterchangeInfo.Source.CompanyCode.IsEmpty ||
				(interchange.InterchangeInfo.Source.EnterpriseCode == ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode &&
				interchange.InterchangeInfo.Source.CompanyCode == GlbCompany.CurrentCompany.GC_Code));
		}

#if DEBUG
		[ThreadStatic]
		public static bool? IsCrossLedgerOverrideforTest;
#endif
		//    try
		//    {
		//        Xsd.TxnHeader XmlInvoice = (Xsd.TxnHeader)FinancialInvoiceSerializer.Deserialize(Reader);
		//        if (FinancialInvoiceSerializer.CanDeserialize(Reader))
		//        {
		//            Context.Notify(new ErrorNotification(ErrorType.Error, "There is more than one Invoice transaction in the XML file."));
		//        }
		//        ImportFromValueObject(XmlInvoice, Collection, Context);
		//    }
		//    catch (XmlException ex)
		//    {
		//        Context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
		//    }
		//}

		//void ImportFromValueObject(Xsd.TxnHeader XmlInvoice, BusinessObjectCollection Collection, ValueObjectImportContext Context)
		//{
		//    if (XmlInvoice != null)
		//    {
		//        Type TypeToLoad = TxnHeaderMapper.GetBizObjTypeFromIValueObject(XmlInvoice);

		//        if (TypeToLoad != null)
		//        {
		//            InvoicingBase Invoice = (InvoicingBase)Context.Factory.New(TxnHeaderMapper.GetBizObjTypeFromIValueObject(XmlInvoice));
		//            FinancialInvoiceDataAdapter DataAdapter = new FinancialInvoiceDataAdapter();
		//            DataAdapter.RunExtraValidation = false;
		//            ((IValueObjectDataAdapter)DataAdapter).ImportFromValueObject(Invoice, XmlInvoice, Context);
		//            Collection.Add(Invoice);
		//        }
		//        else
		//        {
		//            Context.Notify(new ErrorNotification(ErrorType.Error, "This transaction type cannot be imported"));
		//        }
		//    }
		//}

		//XmlValueObjectSerializer FinancialInvoiceSerializer
		//{
		//    get
		//    {
		//        if (fFinancialInvoiceSerializer == null)
		//        {
		//            fFinancialInvoiceSerializer = new XmlValueObjectSerializer(typeof(Xsd.TxnHeader), "FinancialInvoice");
		//        }
		//        return fFinancialInvoiceSerializer;
		//    }
		//}
		//XmlValueObjectSerializer fFinancialInvoiceSerializer;

		readonly ZString Ledger;
	}
}