using System;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiCancellationBuilder
	{
		XStreamingElement BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch);
	}

	class CFDiCancellationBuilder : ICFDiCancellationBuilder
	{
		readonly IMexicoEInvoicingDependencyFactory MexicoEInvocingDependencies;
		readonly ITransactionInfoHelper Helper;

		public CFDiCancellationBuilder()
		{
			MexicoEInvocingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetMexicoEInvoicingDependencyFactory();
			Helper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetTransactionInfoHelper();
		}

		XStreamingElement ICFDiCancellationBuilder.BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch)
		{
			#region SuppressResourceStringsCheckRegion

			var cancellationCredentials = GetCredentialData(transaction);

			var doc = new XStreamingElement("Cancelacion",
				new XElement("RFCEmisor", GetRFCEmisor(transaction).GetValueOrDefault()),
				new XElement("RFCReceptor", GetRFCReceptor(transaction).GetValueOrDefault()),
				new XElement("Total", GetTotalAmountFormatted(transaction, accBatch.Factory)),
				new XElement("UUID", GetUUID(transaction)),
				new XElement("Certificado", cancellationCredentials.Certificado),
				new XElement("ClavePrivada", cancellationCredentials.ClavePrivada),
				new XElement("Motivo", MexicoEInvocingDependencies.GetEInvoiceHelper().GetReasonOfCancellation(accBatch.PK))
			);

			#endregion

			return doc;
		}

		ZString? GetRFCEmisor(TransactionInfo transaction)
			=> Helper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC);

		ZString? GetRFCReceptor(TransactionInfo transaction)
			=> Helper.GetRegistrationCode(transaction.OrganizationAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFC)
			?? Helper.GetRegistrationCode(transaction.OrganizationAddress, CountryCodes.Mexico, MexicoOrgCusCodeInfo.OrgCusCodes.RFG);

		ZString GetTotalAmountFormatted(TransactionInfo transaction, BusinessObjectFactory factory)
			=> GetTotalAmount(transaction.OSTotal).ToString(Helper.GetOSCurrencyDecimals(factory, transaction));

		ZDecimal GetTotalAmount(ZDecimal? value) => value.HasValue ? Math.Abs(value.Value) : (decimal)ZDecimal.Zero;

		(string Certificado, string ClavePrivada) GetCredentialData(TransactionInfo transactionInfo)
		{
			var certificate = MexicoEInvocingDependencies.GetCompanyCredential().GetCompanyCredential(transactionInfo);
			return MexicoEInvocingDependencies.GetCertificateHelper().GetCredentialDataForCancellation(certificate);
		}

		string GetUUID(TransactionInfo transaction) => Helper.GetGovernmentNumber(transaction.OriginalReference?.AuthorizationDetailCollection);
	}
}
