
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance.Argentina;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IFECabRequestBuilder
	{
		XStreamingElement BuildFECabRequestInfo(TransactionInfo transaction, XNamespace xNameSpace, AccComplianceSequence sequence);
	}

	class FECabRequestBuilder : IFECabRequestBuilder
	{
		public FECabRequestBuilder()
		{
			EInvoicingExtension = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetArgentinaEInvoicingExtension();
			EInvoicingDependecies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
			ArgentinaEInvoiceHelper = EInvoicingDependecies.GetArgentinaEInvoiceHelper();
		}

		readonly IArgentinaEInvoicingExtension EInvoicingExtension;
		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependecies;
		readonly IArgentinaEInvoiceHelper ArgentinaEInvoiceHelper;

		#region SuppressResourceStringsCheckRegion

		XStreamingElement IFECabRequestBuilder.BuildFECabRequestInfo(TransactionInfo transaction, XNamespace xNameSpace, AccComplianceSequence sequence)
		{
			Argument.NotNull(transaction, nameof(transaction));

			return new XStreamingElement(xNameSpace + "FeCabReq",
						new XElement(xNameSpace + "CantReg", 1),
						new XElement(xNameSpace + "PtoVta", ArgentinaEInvoiceHelper.GetComplianceSequencePrefixInfo(sequence)),
						new XElement(xNameSpace + "CbteTipo", EInvoicingExtension.GetDocumentType(transaction.ComplianceSubType.GetValueOrDefault(ZString.Empty))));
		}

		#endregion
	}
}
