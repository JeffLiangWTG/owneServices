using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public static class eNettHelper
	{
		public static bool IsOrganisationeNettRegistered(OrgHeader organisation)
		{
			return !organisation.ENettRegistrationNumber.IsEmpty;
		}

		public static bool DoesOrgHaveeNettDDRAccount(OrgHeader organisation)
		{
			return GetEnettBankAccountDetails(organisation) != null;
		}

		public static bool IsBankAccountEnettRegistered(AccBankAccount account)
		{
			bool result = false;
			if (account != null)
			{
				result = AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value.ContainsBankAccount(account.PK);
			}
			return result;
		}

		public static ZString NoEnettBankInformationOnOrganisationError
		{
			get { return Res.GetString("619ba8ef-cfc5-4a0e-bb56-f6e83769a595", "This organization is not registered for ComPay and has no direct debit bank information."); }
		}

		public static ZString BankAccountNotEnettRegistered
		{
			get { return Res.GetString("a330bfc4-aa75-48f3-9efe-8717f28b7c28", "Bank account is not registered for ComPay."); }
		}

		public static ZString PayAnyoneWarning
		{
			get
			{
				return Res.GetString("fa9bb008-5532-4698-a5ae-ed4b7e7d57f4", "You have selected the ComPay Direct Debit Payment option.\r\nThe Organization you are paying does not have an eNett registration number, so this payment will be made using the ‘Pay Anyone’ feature.\r\nPlease be advised that the cleared funds will not hit the Payee's bank for up to 3 days.");
			}
		}

		public static ZString NoEnettMappingError
		{
			get
			{
				return Res.GetString("22b9c489-8b59-43c5-8dc0-699311817e4e", @"This charge code is not mapped to a ComPay charge code.
To  map your charge codes to ComPay charge codes, you will need to:
- Open the ComPay organization screen
- Navigate to the Details > Config > EDI Code Mapping tab
- Enter charge code mappings using the 'CHC' (Charge Code) relationship for each charge code used with ComPay");
			}
		}

		public static AccAPAccountDetails GetEnettBankAccountDetails(OrgHeader organisation)
		{
			AccAPAccountDetails result = null;
			foreach (AccAPAccountDetails accountDetail in organisation.CompanyData.AccountDetailsCollection)
			{
				if (accountDetail.A1_PaymentMethod == ReceiptTypes.eNettDirectDebit)
				{
					result = accountDetail;
					break;
				}
			}
			if (result == null) // Fall back to DDR account
			{
				foreach (AccAPAccountDetails accountDetail in organisation.CompanyData.AccountDetailsCollection)
				{
					if (accountDetail.A1_PaymentMethod == ReceiptTypes.DirectDebit)
					{
						result = accountDetail;
						break;
					}
				}
			}
			return result;
		}

		#region GetBrokersENettRegistrationNumber

		public static ZString GetBrokersENettRegistrationNumber(TransactionHeader transactionHeader)
		{
			ZString result = ZString.Empty;
			if (transactionHeader != null &&
					transactionHeader.Job != null &&
					transactionHeader.DocumentSupporter != null &&
					transactionHeader.Header != null)
			{
				IJobInvoicingPlugIn plugin = (IJobInvoicingPlugIn)transactionHeader.Factory.Load<Enterprise.Integration.Freight.ICommonShipment>(transactionHeader.Job.JH_ParentID);
				if (plugin != null && plugin.InvoicingSupporter.Consignee != null && IsInvoiceForConsigneeBillTo(plugin, transactionHeader))
				{
					string transportMode = transactionHeader.DocumentSupporter.TransportMode;
					string containerMode = transactionHeader.DocumentSupporter.ContainerMode;

					switch (SendDocsTo(transactionHeader, transportMode, containerMode, plugin))
					{
						case OrgConstants.SendDocsTo.Broker:
						case OrgConstants.SendDocsTo.Both:
							OrgHeader broker = BrokerOrganisation(plugin.InvoicingSupporter.Consignee, transportMode, containerMode);
							result = broker != null && broker.CompanyData.OB_IsDebtor ? broker.ENettRegistrationNumber : ZString.Empty;
							break;
					}
				}
			}
			return result;
		}

		static bool IsInvoiceForConsigneeBillTo(IJobInvoicingPlugIn plugin, TransactionHeader transactionHeader)
		{
			bool result = false;
			if (plugin != null && plugin.InvoicingSupporter.IsImport && plugin.InvoicingSupporter.Consignee != null && transactionHeader != null)
			{
				ZGuid billTo = plugin.InvoicingSupporter.Consignee.PK;

				if ((plugin.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.Shipment || plugin.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.QuotedBooking)
					&& plugin.InvoicingSupporter.Consignee.GetFreightBillTo(true, plugin.InvoicingSupporter.TransportMode, plugin.InvoicingSupporter.ContainerMode) != null)
				{
					billTo = plugin.InvoicingSupporter.Consignee.GetFreightBillTo(true, plugin.InvoicingSupporter.TransportMode, plugin.InvoicingSupporter.ContainerMode).PK;
				}
				else if (plugin.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.Brokerage && plugin.InvoicingSupporter.Consignee.GetCustomsBillTo(true, plugin.InvoicingSupporter.TransportMode, plugin.InvoicingSupporter.ContainerMode) != null)
				{
					billTo = plugin.InvoicingSupporter.Consignee.GetCustomsBillTo(true, plugin.InvoicingSupporter.TransportMode, plugin.InvoicingSupporter.ContainerMode).PK;
				}
				result = billTo == transactionHeader.AH_OH;
			}
			return result;
		}

		static OrgHeader BrokerOrganisation(OrgHeader consignee, string transportMode, string containerMode)
		{
			OrgHeader fBrokerOrganisation;
			OrgSupplierBuyerLink supplierBuyerLink = GetSupplierBuyerLink(consignee, transportMode, containerMode);
			if (supplierBuyerLink != null && supplierBuyerLink.OL_OH_ImportBroker.IsValid)
			{
				fBrokerOrganisation = supplierBuyerLink.ImportBroker;
			}
			else
			{
				fBrokerOrganisation = (transportMode == Constants.TransportModes.Air) ?
												consignee.DeliveryAirCustomsBroker : consignee.DeliverySeaCustomsBroker;
			}
			return fBrokerOrganisation;
		}

		static ZString SendDocsTo(TransactionHeader transactionHeader, string transportMode, string containerMode, IJobInvoicingPlugIn plugin)
		{
			ZString result = ZString.Empty;
			OrgSupplierBuyerLink supplierBuyerLink = GetSupplierBuyerLink(plugin.InvoicingSupporter.Consignee, transportMode, containerMode);
			if (supplierBuyerLink != null && !supplierBuyerLink.OL_SendImportDocsTo.IsEmpty)
			{
				result = supplierBuyerLink.OL_SendImportDocsTo;
			}
			else
			{
				if (transportMode == Constants.TransportModes.Air)
				{
					result = transactionHeader.Header.MiscServ.OM_IMSendImportDocsTo;
				}
				else
				{
					result = transactionHeader.Header.MiscServ.OM_IMSendSeaImportDocsTo;
				}
			}
			return result;
		}

		static OrgSupplierBuyerLink GetSupplierBuyerLink(OrgHeader consignee, string transportMode, string containerMode)
		{
			OrgSupplierBuyerLink result = null;
			ZQuery filter = GetRelatedPartyFilter(transportMode, containerMode);
			OrgRelatedParty[] relatedPartys = (OrgRelatedParty[])consignee.ConsigneeRelatedParties.Find(filter);

			if (relatedPartys.Length > 0)
			{
				var list = relatedPartys.ToList();
				list.Sort(new RelatedPartyFreightModeComparer());
				var relatedParty = list[0];

				foreach (OrgSupplierBuyerLink link in consignee.SupplierLinks)
				{
					if (link.OL_OH_Supplier == relatedParty.PR_OH_RelatedParty && link.OrgSupBuyLinkTrnModes.Find(transportMode, containerMode) != null)
					{
						result = link;
					}
				}
			}
			return result;
		}

		static ZQuery GetRelatedPartyFilter(string transportMode, string containerMode)
		{
			var result = new ZQuery(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.CustomsAgentBroker);

			var transportModeFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightTransportMode, transportMode);
			var containerModeFilter = new ZQuery(OrgRelatedPartySchema.PR_FreightContainerMode, ZString.Empty);
			if (!string.IsNullOrEmpty(containerMode))
			{
				containerModeFilter.AddToFilter(JoinCondition.Or, OrgRelatedPartySchema.PR_FreightContainerMode, containerMode);
			}
			transportModeFilter.AddToFilter(containerModeFilter, JoinCondition.And);

			ZQuery transportModeAllFilter = new ZQuery(transportModeFilter);
			if (transportMode != Constants.TransportModes.All)
			{
				var transportModeAllfilter = new ZQuery(OrgRelatedPartySchema.PR_FreightTransportMode, Constants.TransportModes.All);
				transportModeAllFilter.AddToFilter(transportModeAllfilter, JoinCondition.Or);
			}

			result.AddToFilter(transportModeAllFilter);
			return result;
		}

		class RelatedPartyFreightModeComparer : IComparer<OrgRelatedParty>, IComparer
		{
			public int Compare(object x, object y)
			{
				return Compare((OrgRelatedParty)x, (OrgRelatedParty)y);
			}

			public int Compare(OrgRelatedParty x, OrgRelatedParty y)
			{
				return Score(y) - Score(x);
			}

			[WTG.StaticAnalysis.Annotation.CodeAlive("Developer friendly enum")]
			[Flags]
			enum Scores
			{
				TransportModeAll = 1,
				TransportMode = 2,
				ContainerMode = 4,
			}

			int Score(OrgRelatedParty org)
			{
				int result = 0;

				if (!org.PR_FreightTransportMode.IsEmpty)
				{
					if (!org.PR_FreightContainerMode.IsEmpty)
					{
						result |= (int)Scores.ContainerMode;
					}
					else if (org.PR_FreightTransportMode != Core.Constants.TransportModes.All)
					{
						result |= (int)Scores.TransportMode;
					}
					else
					{
						result |= (int)Scores.TransportModeAll;
					}
				}

				return result;
			}
		}

		#endregion

		#region Check Transaction's eNett messages

		public static bool ENettMessageHasBeenSucessfullySent(TransactionHeader transactionHeader)
		{
			return ENettMessageHasBeenSucessfullySent(transactionHeader.Factory, transactionHeader.PK);
		}

		public static bool ENettMessageHasBeenSucessfullySent(BusinessObjectFactory factory, ZGuid transactionHeaderPK)
		{
			ZQuery query = GetENettMessageFilter(transactionHeaderPK);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
			const string indexName = "NR_RX__EM_LinkUniqueID";
			query.TableIndexHints.Add(new TableIndexHint(indexName));
			return factory.LoadTop1<EDIMessage>(query) != null;
		}

		public static bool ENettMessageAlreadyExists(TransactionHeader transactionHeader)
		{
			return ENettMessageAlreadyExists(transactionHeader.Factory, transactionHeader.PK);
		}

		public static bool ENettMessageAlreadyExists(BusinessObjectFactory factory, ZGuid transactionHeaderPK)
		{
			return factory.LoadTop1<EDIMessage>(GetENettMessageFilter(transactionHeaderPK)) != null;
		}

		static ZQuery GetENettMessageFilter(ZGuid transactionHeaderPK)
		{
			ZQuery query = new ZQuery();

			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.eNett);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "ENE");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, AccTransactionHeader.Schema.TableName);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, transactionHeaderPK);

			return query;
		}

		#endregion

	}
}