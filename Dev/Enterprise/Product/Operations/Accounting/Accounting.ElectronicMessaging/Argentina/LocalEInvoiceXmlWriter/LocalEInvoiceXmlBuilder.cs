using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	class LocalEInvoiceXmlBuilder : IEInvoiceXmlBuilder
	{
		public LocalEInvoiceXmlBuilder()
		{
			feDetRequestBuilder_constructorInitializedOnly = new FEDetRequestBuilder();
			feAuthRequestBuilder_contructorInitializedOnly = new FEAuthRequestBuilder();
			feCabRequestBuilder_contructorInitializedOnly = new FECabRequestBuilder();
			complianceSequenceRetriever_constructorInitalizedOnly = new ComplianceSequenceRetriever();
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
			EInvoicingDependecies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
		}

		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependecies;

		IFEDetRequestBuilder FEDetRequestBuilder => feDetRequestBuilder_constructorInitializedOnly;
		IFEDetRequestBuilder feDetRequestBuilder_constructorInitializedOnly;
		IFEAuthRequestBuilder FEAuthRequestBuilder => feAuthRequestBuilder_contructorInitializedOnly;
		readonly IFEAuthRequestBuilder feAuthRequestBuilder_contructorInitializedOnly;
		IFECabRequestBuilder FECabRequestBuilder => feCabRequestBuilder_contructorInitializedOnly;
		IFECabRequestBuilder feCabRequestBuilder_contructorInitializedOnly;
		IComplianceSequenceRetriever ComplianceSequenceRetriever => complianceSequenceRetriever_constructorInitalizedOnly;
		IComplianceSequenceRetriever complianceSequenceRetriever_constructorInitalizedOnly;
		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteFeDetRequestBuilder_ForTestOnly(IFEDetRequestBuilder replacement) => feDetRequestBuilder_constructorInitializedOnly = replacement;
		public IFEDetRequestBuilder FeDetRequestBuilder_ExposedForTestOnly => FEDetRequestBuilder;
		public IFEAuthRequestBuilder FeAuthRequestBuilder_ExposedForTestOnly => FEAuthRequestBuilder;
		public void SubstituteFeCabRequestBuilder_ForTestOnly(IFECabRequestBuilder replacement) => feCabRequestBuilder_contructorInitializedOnly = replacement;
		public IFECabRequestBuilder FeCabRequestBuilder_ExposedForTestOnly => FECabRequestBuilder;
		public void SubstituteComplianceSequenceRetriever_ForTestOnly(IComplianceSequenceRetriever replacement) => complianceSequenceRetriever_constructorInitalizedOnly = replacement;
		public IComplianceSequenceRetriever ComplianceSequenceRetriever_ExposedForTestOnly => ComplianceSequenceRetriever;
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif

		XStreamingElement IEInvoiceXmlBuilder.BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch)
		{
			#region SuppressResourceStringsCheckRegion

			XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";

			return new XStreamingElement(soap + "Envelope",
					new XAttribute(XNamespace.Xmlns + "soapenv", soap.NamespaceName),
					BuildXMLLocalEInvoice(transaction, soap));

			XStreamingElement BuildXMLLocalEInvoice(TransactionInfo transactionInfo, XNamespace soapNameSpace)
			{
				var registrationNumber = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, Core.Constants.CountryCodes.Argentina,
					ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT);

				XNamespace ar = "http://ar.gov.afip.dif.FEV1/";

				return new XStreamingElement(soapNameSpace + "Body",
							new XAttribute(XNamespace.Xmlns + "ar", ar.NamespaceName),
							new XElement(ar + "FECAESolicitar",
									FEAuthRequestBuilder.BuildFEAuthRequestInfo(transactionInfo, ar, registrationNumber),
									BuildXMLLocalFeCAEReq(transactionInfo, ar, registrationNumber, accBatch.Factory)
									));
			}

			XStreamingElement BuildXMLLocalFeCAEReq(TransactionInfo transactionInfo, XNamespace xNamespace, string registrationNumber, BusinessObjectFactory factory)
			{
				var complianceSubtype = transactionInfo.ComplianceSubType.GetValueOrDefault(ZString.Empty);
				var originalReferenceComplianceSubtype = EInvoicingDependecies.GetArgentinaEInvoiceHelper().GetOriginalTransactionComplianceSubType(transactionInfo.OriginalReference);

				(ZGuid branchPK, ZGuid companyPK, ZGuid departmentPK) = EInvoicingDependecies.GetArgentinaEInvoiceHelper().GetBranchCompanyAndDepartamentPKFromTransactionInfo(transactionInfo, factory);

				AccComplianceSequence transactionComplianceSequenceFromSubType = null;
				try
				{
					transactionComplianceSequenceFromSubType = ComplianceSequenceRetriever.GetComplianceSequenceFromSubType(complianceSubtype, companyPK, branchPK, departmentPK, ZDateTime.Today);
				}
				catch (MultipleComplianceSequenceFoundException)
				{
					transactionComplianceSequenceFromSubType = null;
				}

				return new XStreamingElement(xNamespace + "FeCAEReq",
									FECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, xNamespace, transactionComplianceSequenceFromSubType),
									FEDetRequestBuilder.BuildFEDetRequestInfo(transactionInfo, xNamespace, factory, originalReferenceComplianceSubtype, registrationNumber));
			}

			#endregion
		}
	}
}

