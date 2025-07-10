using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class TransactionBatchToGEIConverterForVietnam : TransactionBatchToGEIConverter
	{
		string MessageType;

		AccEInvoicingBatch EInvoicingBatch;

		AccEInvoicingTransactionPivot Pivot => (AccEInvoicingTransactionPivot)EInvoicingBatch?.TransactionPivots.FirstOrDefault();

		AdditionalTransactionInfoForVietnamEInvoice AdditionalTransactionInfo { get; set; }

		ZGuid TransactionHeaderPK => Pivot?.AIP_ParentID ?? ZGuid.Empty;

		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			EInvoicingBatch = batch;
			MessageType = VietnamEInvoiceAPICommandList.GetMessageType(Pivot?.AIP_ActionType ?? ZString.Empty);

			if (MessageType == VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice || MessageType == VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice)
			{
				base.PerformBeforeConvert(batch);
			}
		}

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			switch (MessageType)
			{
				case VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice:
					PopulateSRNAdditionalTransactionInfo();
					return new RINGlobalElectronicInvoiceBuilderForVietnam(batchNumber, UniversalBatch, AdditionalTransactionInfo);
				case VietnamEInvoiceAPICommandList.Codes.CancelReceivablesInvoice:
					PopulateRCNAdditionalTransactionInfo();
					return new RCNGlobalElectronicInvoiceBuilderForVietnam(batchNumber, AdditionalTransactionInfo);
				case VietnamEInvoiceAPICommandList.Codes.RequestDocumentForInvoice:
					PopulateRDNAdditionalTransactionInfo();
					return new RDNGlobalElectronicInvoiceBuilderForVietnam(batchNumber, AdditionalTransactionInfo);
				case VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice:
					PopulateRAJAdditionalTransactionInfo();
					return new RAJGlobalElectronicInvoiceBuilderForVietnam(batchNumber, UniversalBatch, AdditionalTransactionInfo);
				case VietnamEInvoiceAPICommandList.Codes.ApproveReceivablesInvoice:
					PopulateRAPAdditionalTransactionInfo();
					return new RAPGlobalElectronicInvoiceBuilderForVietnam(batchNumber, AdditionalTransactionInfo);
				default:
					throw new InvalidOperationException($"Invalid API command: {MessageType}");
			}
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new TransactionBatchExporter(dataAccess, new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true));

		void PopulateSRNAdditionalTransactionInfo()
		{
			var arInvoice = ReadonlyFactory.Load<TransactionHeader>(TransactionHeaderPK);
			var isPeriodicInvoice = InvoiceTypeCalculationProvider.IsDeferredInvoiceType(arInvoice.AH_TransactionCategory);
			var jobHeader = ReadonlyFactory.Load<JobHeader>(arInvoice.AH_JH);

			PopulateAdditionalTransactionInfo(jobHeader, arInvoice, isPeriodicInvoice);
		}

		void PopulateAdditionalTransactionInfo(JobHeader jobHeader, TransactionHeader arInvoice, bool isPeriodicInvoice, TransactionHeader arCreditNote = null)
		{
			var consol = (ForwardingConsol)null;
			var shipment = (ForwardingShipment)null;
			var declaration = (BaseJobDeclaration)null;

			if (jobHeader != null)
			{
				consol = ReadonlyFactory.Load<ForwardingConsol>(jobHeader.JH_ParentID);
				shipment = ReadonlyFactory.Load<ForwardingShipment>(jobHeader.JH_ParentID);
				declaration = ReadonlyFactory.Load<BaseJobDeclaration>(jobHeader.JH_ParentID);
			}
			else if (!isPeriodicInvoice)
			{
				var indexOfSuffix = arInvoice.AH_ConsolidatedInvoiceRef.IndexOf("/");
				var consolidatedInvoiceRef = indexOfSuffix == -1 ? arInvoice.AH_ConsolidatedInvoiceRef : arInvoice.AH_ConsolidatedInvoiceRef.Substring(0, indexOfSuffix);
				consol = ReadonlyFactory.Load<ForwardingConsol>(new ZQuery().AddToFilter(JobConsolSchema.JK_UniqueConsignRef, consolidatedInvoiceRef)).FirstOrDefault();
				shipment = ReadonlyFactory.Load<ForwardingShipment>(new ZQuery().AddToFilter(JobShipmentSchema.JS_UniqueConsignRef, consolidatedInvoiceRef)).FirstOrDefault();
			}

			if (arInvoice != null)
			{
				var orgCusCode = arInvoice.Header.CustomsCodes?.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Constants.CountryCodes.VietNam).FirstOrDefault();
				var matchedBankAccount = arInvoice.Header.CompanyData.ARAccountDetailsCollection?.Cast<AccARAccountDetails>().
					Where(x => x.A1_RN_NKCountryCode == Constants.CountryCodes.VietNam && x.A1_IsDefaultAccount && x.A1_PaymentMethod == AccARAccountDetails.ARBankAccPayment).
					OrderBy(x => x.A1_RX_NKAccountCurrency == arInvoice.AH_RX_NKTransactionCurrency ? 0 : 1).
					ThenBy(x => x.A1_RX_NKAccountCurrency != Constants.CurrencyCodes.VietNam).FirstOrDefault();

				AdditionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice
				{
					OriginalTransactionPK = arInvoice.PK,
					OriginalTransactionReference = arInvoice.AH_TransactionReference,
					TransactionReference = arInvoice.AH_TransactionReference,
					VATRegistrationNum = orgCusCode?.OK_CustomsRegNo ?? ZString.Empty,
					BankName = matchedBankAccount?.A1_BankName ?? ZString.Empty,
					AccountNumber = matchedBankAccount?.A1_BankAccount ?? ZString.Empty,
					CompanyPK = arInvoice.Company.PK,
					BranchPK = arInvoice.Branch.PK,
					ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
					TransactionNumber = arInvoice.AH_TransactionNum,
					IsConsolInvoice = consol != null,
					FormattedAddress = GetFormattedAddress(arInvoice.DisplayInvoiceAddressOverride, arInvoice.Company),
					InvoiceCreatingStaff = arInvoice.CheckRequesterUserFullName,
					TransactionLineHouseBillDictionary = isPeriodicInvoice && (arInvoice is TransactionHeaderWithLines headerWithLines) && headerWithLines.Lines.Select(x => x.Job).Distinct().Count() > 1
						? headerWithLines.Lines.Select(x => x.Job).Distinct().ToDictionary(job => job.JH_JobNum, job => ((Job)job).JH_HouseBillNo)
						: null,
				};

				SetUpAdditionalComplianceSequenceInfo(arInvoice);

				if (arCreditNote != null)
				{
					AdditionalTransactionInfo.TransactionPK = arCreditNote.PK;
					AdditionalTransactionInfo.TransactionReference = arCreditNote.AH_TransactionReference;
					AdditionalTransactionInfo.ComplianceDocumentDate = arCreditNote.AH_ComplianceDocumentDate;
					AdditionalTransactionInfo.RectifyDate = arCreditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm");
					AdditionalTransactionInfo.RectifyReason = arCreditNote.AH_Desc.ToString();
					AdditionalTransactionInfo.RectifySupportingDocumentNumber = arCreditNote.SupportingDocumentNumber;

					SetUpAdditionalComplianceSequenceInfo(arCreditNote);
					SetUpOriginalAdditionalComplianceSequenceInfo(arInvoice);
				}

				SetUpAdditionalContactInfo(arCreditNote ?? arInvoice);
				SetUpAdditionalJobInfo(jobHeader);

				if (consol != null)
				{
					SetUpConsolInfoInAdditionalTransactionInfo(consol);
				}
				else if (shipment != null)
				{
					SetUpShipmentInfoInAdditionalTransactionInfo(shipment);
				}
				else if (declaration != null)
				{
					SetUpDeclarationInfoInAdditionalTransactionInfo(declaration);
				}
			}
		}

		void PopulateRCNAdditionalTransactionInfo()
		{
			if (EInvoicingBatch != null)
			{
				var arCreditNote = ReadonlyFactory.Load<TransactionHeader>(TransactionHeaderPK);
				var arInvoice = ReadonlyFactory.LoadTop1<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, arCreditNote?.AH_TransactionBelongsToGroup));

				if (arCreditNote != null && arInvoice != null)
				{
					AdditionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice
					{
						OriginalTransactionReference = arInvoice.AH_TransactionReference,
						CompanyPK = arInvoice.Company.PK,
						BranchPK = arInvoice.Branch.PK,
						ComplianceDocumentDate = arInvoice.AH_ComplianceDocumentDate,
						RectifyDate = arCreditNote.AH_PostDate.Date.ToString("yyyy-MM-dd HH:mm"),
						RectifyReason = arCreditNote.AH_Desc.ToString(),
						RectifySupportingDocumentNumber = arCreditNote.SupportingDocumentNumber,
					};

					SetUpAdditionalComplianceSequenceInfo(arCreditNote);
					SetUpOriginalAdditionalComplianceSequenceInfo(arInvoice);
				}
			}
		}

		void PopulateRDNAdditionalTransactionInfo()
		{
			var transactionHeader = ReadonlyFactory.Load<TransactionHeader>(TransactionHeaderPK);

			if (transactionHeader != null)
			{
				if (transactionHeader.IsReversalTransaction)
				{
					transactionHeader = ReadonlyFactory.Load<TransactionHeader>(transactionHeader.AH_TransactionBelongsToGroup);
				}

				AdditionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice
				{
					OriginalTransactionPK = transactionHeader.PK,
					CompanyPK = transactionHeader.Company.PK,
					BranchPK = transactionHeader.Branch.PK,
				};
			}
		}

		void PopulateRAJAdditionalTransactionInfo()
		{
			var arTransaction = ReadonlyFactory.Load<TransactionHeader>(TransactionHeaderPK);
			var arInvoice = ReadonlyFactory.LoadTop1<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, arTransaction?.AH_TransactionBelongsToGroup));
			var jobHeader = ReadonlyFactory.Load<JobHeader>(arInvoice.AH_JH);
			var isPeriodicInvoice = InvoiceTypeCalculationProvider.IsDeferredInvoiceType(arInvoice.AH_TransactionCategory);

			PopulateAdditionalTransactionInfo(jobHeader, arInvoice, isPeriodicInvoice, arTransaction);
		}

		void PopulateRAPAdditionalTransactionInfo()
		{
			var arInvoice = ReadonlyFactory.Load<TransactionHeader>(TransactionHeaderPK);

			if (arInvoice != null)
			{
				AdditionalTransactionInfo = new AdditionalTransactionInfoForVietnamEInvoice
				{
					TransactionPK = arInvoice.PK,
					CompanyPK = arInvoice.Company.PK,
					BranchPK = arInvoice.Branch.PK,
					TransactionReference = arInvoice.AH_TransactionReference,
				};

				SetUpAdditionalComplianceSequenceInfo(arInvoice);
			}
		}

		void SetUpConsolInfoInAdditionalTransactionInfo(ForwardingConsol consol)
		{
			AdditionalTransactionInfo.MasterBillNumber = consol.JK_MasterBillNum;
			AdditionalTransactionInfo.TransportMode = consol.TransportMode;
			AdditionalTransactionInfo.TransportInfo = GetTransportInfo(consol);
			AdditionalTransactionInfo.ETA = consol.JK_JX_JB_E_ARV.ToShortDateString();
			AdditionalTransactionInfo.ETD = consol.JK_JX_JA_E_DEP.ToShortDateString();
			AdditionalTransactionInfo.SendingAgent = consol.SendingForwarder?.OH_FullName ?? ZString.Empty;
			AdditionalTransactionInfo.ReceivingAgent = consol.ReceivingForwarder?.OH_FullName ?? ZString.Empty;
			AdditionalTransactionInfo.Carrier = ((OrgHeader)consol.JK_OA_ShippingLineAddress_ZAddress?.OrgHeader)?.OH_FullName ?? ZString.Empty;
			AdditionalTransactionInfo.Package = consol.JK_TotalShipmentQuantity.ToString() + " PACKS";

			var weight = consol.GetTotalShipmentWeightForDoc(WeightAndVolumeDisplayTypes.Codes.Actual);
			var chargeableWeight = consol.GetTotalShipmentChargeableForDoc(WeightAndVolumeDisplayTypes.Codes.Actual);
			var volume = consol.GetTotalShipmentVolumeForDoc(WeightAndVolumeDisplayTypes.Codes.Actual);

			AdditionalTransactionInfo.Weight = Utilities.FormatNumber(weight, 3) + " " + consol.JK_TotalShipmentWeightUnit;
			AdditionalTransactionInfo.ChargeableWeight = Utilities.FormatNumber(chargeableWeight, 3) + " " + consol.JK_ConsolChargeableUnit;
			AdditionalTransactionInfo.Volume = Utilities.FormatNumber(volume, 3) + " " + consol.JK_TotalShipmentVolumeUnit;
			AdditionalTransactionInfo.OriginPort = CalculatePortCodeAndName(consol.LoadPort);
			AdditionalTransactionInfo.DestinationPort = CalculatePortCodeAndName(consol.DischargePort);
		}

		void SetUpShipmentInfoInAdditionalTransactionInfo(ForwardingShipment shipment)
		{
			AdditionalTransactionInfo.ETA = shipment.JS_E_ARV.ToShortDateString();
			AdditionalTransactionInfo.ETD = shipment.JS_E_DEP.ToShortDateString();
			AdditionalTransactionInfo.OrderReference = shipment.DocsAndCartage?.JP_OrderItemsAsString ?? ZString.Empty;
			AdditionalTransactionInfo.Package = shipment.JS_OuterPacks.ToString() + " " + shipment.JS_F3_NKPackType;
			AdditionalTransactionInfo.Weight = Utilities.FormatNumber(shipment.JS_ActualWeight, 3) + " " + shipment.JS_UnitOfWeight;
			AdditionalTransactionInfo.ChargeableWeight = Utilities.FormatNumber(shipment.JS_ActualChargeable, 3) + " " + shipment.JS_ChargeableUnit;
			AdditionalTransactionInfo.Volume = Utilities.FormatNumber(shipment.JS_ActualVolume, 3) + " " + shipment.JS_UnitOfVolume;
			AdditionalTransactionInfo.OriginPort = CalculatePortCodeAndName(shipment.Origin);
			AdditionalTransactionInfo.DestinationPort = CalculatePortCodeAndName(shipment.Destination);
			AdditionalTransactionInfo.TransactionParentID = shipment.JS_UniqueConsignRef;

			if (shipment.Consols.Any())
			{
				shipment.Consols.Sort(CommonConsol.Schema.JK_UniqueConsignRef, ListSortDirection.Ascending);
				var firstConsol = shipment.Consols[0];

				AdditionalTransactionInfo.MasterBillNumber = firstConsol.JK_MasterBillNum;
				AdditionalTransactionInfo.ConsolID = firstConsol.JK_UniqueConsignRef;
				AdditionalTransactionInfo.TransportMode = firstConsol.TransportMode;
				AdditionalTransactionInfo.TransportInfo = GetTransportInfo(firstConsol);
			}
		}

		void SetUpAdditionalContactInfo(TransactionHeader transaction)
		{
			var contact = new AdditionalContactInfo();
			var emails = GetOrgARContactsEmail(transaction);

			AdditionalTransactionInfo.ContactInfo = new AdditionalContactInfo()
			{
				Name = orgContact?.OC_ContactName ?? ZString.Empty,
				Mails = emails,
				Phone = orgContact?.OC_Phone ?? ZString.Empty
			};
		}

		public static List<ZString> GetOrgARContactsEmail(TransactionHeader transaction)
		{
			var contact = new AdditionalContactInfo();
			var orgARContacts = transaction.Header.Contacts.Cast<OrgContact>().Where(x =>
				x.OC_IsActive && x.Documents.Cast<OrgDocument>().Any(y =>
					y.OD_DocumentGroup == ContactType.Receivables.Code || y.OD_DocumentGroup == ContactType.All.Code));

			orgContact = transaction.InvoiceContactOverride ?? orgARContacts.FirstOrDefault();

			var emails = new List<ZString>();
			var includeLocalDebtorOnly = AccountingMasterFilesRegistry.Instance.IncludeContactEmailsOfLocalDebtorOrganizationOnly.GetValueWithoutFallback(transaction.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (!includeLocalDebtorOnly || IsDebtorLocal(transaction))
			{
				if (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.GetValueWithoutFallback(transaction.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) == ExportMultipleDebtorOrganizationContactEmailCodes.MAR)
				{
					var overrideContactEmail = transaction.InvoiceContactOverride?.OC_Email ?? ZString.Empty;
					if (!overrideContactEmail.IsEmpty)
					{
						emails.Add(overrideContactEmail);
					}

					emails.AddRange(orgARContacts
						.Where(x => !x.OC_Email.IsEmpty && x.OC_Email != overrideContactEmail && x.Documents
							.OfType<OrgDocument>().Any(y => y.OD_DeliverBy == ContactNotifyModes.Email))
						.Select(x => x.OC_Email)
						.Distinct());
				}
				else
				{
					emails.Add(orgContact?.OC_Email ?? ZString.Empty);
				}
			}

			return emails;
		}

		static bool IsDebtorLocal(TransactionHeader transaction)
		{
			var transactionDebtorCountryCode = transaction.InvoiceAddressOverride?.Country?.Code ?? ZString.Empty;
			return transactionDebtorCountryCode == CountryCodes.VietNam;
		}

		void SetUpAdditionalJobInfo(JobHeader job)
		{
			if (job != null)
			{
				AdditionalTransactionInfo.JobInfo = new AdditionalJobInfo()
				{
					JobDepartmentDesc = job.Department?.GE_Desc ?? ZString.Empty,
					JobOperationStaff = job.RepOps?.GS_FullName ?? ZString.Empty,
					JobSalesStaff = job.RepSales?.GS_FullName ?? ZString.Empty,
				};
			}
		}

		void SetUpAdditionalComplianceSequenceInfo(TransactionHeader transactionHeader)
		{
			if (transactionHeader != null)
			{
				AdditionalTransactionInfo.ComplianceSequenceInfo = new AdditionalComplianceSequenceInfo()
				{
					SeriesPrefix = transactionHeader.ComplianceSequence?.XD_Prefix ?? ZString.Empty,
					IsComplianceNumberFormatDefault = (transactionHeader.ComplianceSequence?.XD_NumberFormat ?? ZString.Empty) == AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code,
					ComplianceSequenceMaximumNumberDigits = transactionHeader.ComplianceSequence?.XD_MaximumNumberDigits ?? ZByte.Zero,
				};
			}
		}

		void SetUpOriginalAdditionalComplianceSequenceInfo(TransactionHeader transactionHeader)
		{
			if (transactionHeader != null)
			{
				AdditionalTransactionInfo.ComplianceSequenceInfo.OriginalSeriesPrefix = transactionHeader.ComplianceSequence?.XD_Prefix ?? ZString.Empty;
				AdditionalTransactionInfo.ComplianceSequenceInfo.OriginalIsComplianceNumberFormatDefault = (transactionHeader.ComplianceSequence?.XD_NumberFormat ?? ZString.Empty) == AccComplianceSequenceLookups.ComplianceNumberFormatDefault.Code;
				AdditionalTransactionInfo.ComplianceSequenceInfo.OriginalComplianceSequenceMaximumNumberDigits = transactionHeader.ComplianceSequence?.XD_MaximumNumberDigits ?? ZByte.Zero;
			}
		}

		void SetUpDeclarationInfoInAdditionalTransactionInfo(BaseJobDeclaration declaration)
		{
			AdditionalTransactionInfo.IsDeclarationInvoice = true;
			AdditionalTransactionInfo.MasterBillNumber = declaration.JE_MasterBillForGenericWrapper;
			AdditionalTransactionInfo.TransportMode = declaration.JE_TransportMode;
			AdditionalTransactionInfo.TransportInfo = ObjectFactory.Get<ITransportWrapperProvider>().GetTransportReference(declaration, declaration.Factory);
			AdditionalTransactionInfo.ETA = declaration.JE_DateAtFinalDestination.ToShortDateString();
			AdditionalTransactionInfo.ETD = declaration.JE_DateAtOrigin.ToShortDateString();
			AdditionalTransactionInfo.Carrier = declaration.ShippingLine?.OH_FullName ?? ZString.Empty;
			AdditionalTransactionInfo.Package = declaration.JE_TotalNoOfPacks.ToString() + " " + declaration.JE_TotalNoOfPacksPackType;
			AdditionalTransactionInfo.Weight = Utilities.FormatNumber(declaration.JE_TotalWeight, 3) + " " + declaration.JE_TotalWeightUnit;
			AdditionalTransactionInfo.Volume = Utilities.FormatNumber(declaration.JE_TotalVolume, 3) + " " + declaration.JE_TotalVolumeUnit;
			AdditionalTransactionInfo.OriginPort = CalculatePortCodeAndName(declaration.Origin);
			AdditionalTransactionInfo.DestinationPort = CalculatePortCodeAndName(declaration.FinalDestination);
			AdditionalTransactionInfo.OrderReference = declaration.DocsAndCartage?.JP_OrderItemsAsString ?? ZString.Empty;
			AdditionalTransactionInfo.TransactionParentID = declaration.JE_DeclarationReference;
		}

		string CalculatePortCodeAndName(RefUNLOCO port)
		{
			return port == null ? string.Empty : string.Format($"{port.Code} - {port.RL_PortName}.{port.Country.RN_DescMultilingual}");
		}

		#region Consol TransportInfo
		ZString GetTransportInfo(CommonConsol consol)
		{
			var result = ZString.Empty;

			switch (consol.TransportMode)
			{
				case Constants.TransportModes.Air:
				case Constants.TransportModes.AirSea:
					result = GetAirTransportDetails(consol);
					break;

				case Constants.TransportModes.Rail:
				case Constants.TransportModes.Road:
					result = consol.JK_JX_JV_NKVessel + " / " + consol.JK_JX_JV_VoyageFlight;
					break;

				case Constants.TransportModes.Sea:
				case Constants.TransportModes.SeaAir:
					var lloydsNumber = consol.Vessel == null ? ZString.Empty : consol.Vessel.RV_LloydsNumber;
					result = GetCombinedValue(GetCombinedValue(consol.JK_JX_JV_NKVessel, consol.JK_JX_JV_VoyageFlight), lloydsNumber);
					break;
			}
			return result;
		}

		ZString GetAirTransportDetails(CommonConsol consol)
		{
			var result = ZString.Empty;
			var transportCollection = consol.Transports;
			transportCollection.Sort("JW_LegOrder", ListSortDirection.Ascending);// May be an property name of sort

			if (transportCollection.Count == 1)
			{
				result = GetAirTransportAndLegDetails(consol, transportCollection[0]);
			}
			else
			{
				result = GetFirstAndSecondLegAirTransportDetails(consol, transportCollection);
			}

			return result;
		}

		ZString GetAirTransportAndLegDetails(CommonConsol consol, Transport transport)
		{
			var transportLegLoadPort = transport.JW_RL_NKLoadPort;
			var transportLegDischargePort = transport.JW_RL_NKDiscPort;
			var atd = consol.JK_JX_JA_A_DEP.IsEmpty ? consol.JK_JX_JA_E_DEP : consol.JK_JX_JA_A_DEP;
			var departureDate = atd.IsEmpty ? consol.JK_JX_JA_E_DEP : atd;

			ZString result;
			if (transportLegDischargePort == consol.JK_JX_JB_RL_NKPortOfDischarge && transportLegLoadPort == consol.JK_JX_JA_RL_NKPortOfLoading)
			{
				result = FormatFlightDetails(consol, consol.JK_JX_JV_VoyageFlight, departureDate);
			}
			else
			{
				result = FormatFlightDetails(consol, consol.JK_JX_JV_VoyageFlight, departureDate);
				result += " -> ";
				result += FormatFlightDetails(consol, transport.JW_VoyageFlight, transport.JW_ETD);
			}

			return result;
		}

		ZString GetFirstAndSecondLegAirTransportDetails(CommonConsol consol, TransportCollection transportCollection)
		{
			var result = ZString.Empty;
			Transport firstLeg = null;
			Transport secondLeg = null;

			var transportPlanningNotOtherCollection = new TransportCollection(consol);
			transportPlanningNotOtherCollection.AddRange(transportCollection.Where(x => x.JW_TransportType != Constants.TransportPlanningType.Other));
			transportPlanningNotOtherCollection.Sort("JW_TransportType", ListSortDirection.Ascending);// May be an property name of sort

			if (transportPlanningNotOtherCollection.Count > 0)
			{
				firstLeg = transportPlanningNotOtherCollection[0];
			}
			if (transportPlanningNotOtherCollection.Count > 1)
			{
				secondLeg = transportPlanningNotOtherCollection[1];
			}

			bool bothTransportLegsFound = (firstLeg != null && secondLeg != null);

			var transportPlanningOtherSortedByETD = new TransportCollection(consol);
			transportPlanningOtherSortedByETD.AddRange(transportCollection.Where(x => x.JW_TransportType == Constants.TransportPlanningType.Other));
			transportPlanningOtherSortedByETD.Sort("JW_ETD", ListSortDirection.Ascending);// May be an property name of sort

			if (!bothTransportLegsFound && transportPlanningOtherSortedByETD.Count > 0)
			{
				if (firstLeg == null)
				{
					firstLeg = transportPlanningOtherSortedByETD[0];
				}
				else
				{
					secondLeg = transportPlanningOtherSortedByETD[0];
				}
				if (secondLeg == null && transportPlanningOtherSortedByETD.Count > 1)
				{
					secondLeg = transportPlanningOtherSortedByETD[1];
				}
			}

			if (firstLeg != null)
			{
				ZString discPort = firstLeg.JW_RL_NKDiscPort;
				result = FormatFlightDetails(consol, firstLeg.JW_VoyageFlight, firstLeg.JW_ETD);
			}

			if (secondLeg != null)
			{
				result += " -> ";
				ZString discPort = secondLeg.JW_RL_NKDiscPort;
				result += FormatFlightDetails(consol, secondLeg.JW_VoyageFlight, secondLeg.JW_ETD);
			}

			return result;
		}

		ZString FormatFlightDetails(CommonConsol consol, ZString voyageNo, ZDateTime etd)
		{
			ZString departureDate = Suppression.GetValue(etd.ToShortDateString(), consol, SuppressFields.ETD, ZString.Empty, ContactType.Receivables);
			return GetCombinedValue(voyageNo, departureDate);
		}

		ZString GetCombinedValue(ZString string1, ZString string2)
		{
			var separator = " / ";

			if (string1 == string2)
			{
				return string1;
			}
			return string1 + (string1.IsEmpty || string2.IsEmpty ? "" : separator) + string2;
		}
		#endregion

		ReadOnlyBusinessObjectFactory ReadonlyFactory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;
		public static OrgContact orgContact { get; private set; }

		#region Address

		string GetFormattedAddress(ZGuid orgAddressPK, GlbCompany invoiceCompany)
		{
			var result = "";
			var orgAddress = ReadonlyFactory.Load<OrgAddress>(orgAddressPK);
			if (orgAddress != null)
			{
				var formatter = new ContactAddressFormatter(ReadonlyFactory, orgAddress, invoiceCompany);
				result = formatter.PostalAddressWithoutCompanyName().Replace("\n", ", ");
			}

			return result;
		}

		class ContactAddressFormatter : AddressFormatter
		{
			public ContactAddressFormatter(BusinessObjectFactory factory, OrgAddress orgAddress, GlbCompany senderCompany)
			: base(factory, (OrgHeader)null, senderCompany, true)
			{
				Argument.NotNull(orgAddress, nameof(orgAddress));
				OrgAddress = orgAddress;
			}

			readonly OrgAddress OrgAddress;

			protected override void RetrieveOrganisationDetails()
			{
				Name = OrgAddress.OA_CompanyNameOverride.IsEmpty ? OrgAddress.Header.OH_FullName : OrgAddress.OA_CompanyNameOverride;
				Address1 = OrgAddress.OA_Address1;
				Address2 = OrgAddress.OA_Address2;
				City = OrgAddress.OA_City;
				State = OrgAddress.OA_State;
				PostCode = OrgAddress.OA_PostCode;
				Language = OrgAddress.OA_Language;
				AdditionalAddressInformation = OrgAddress.OA_AdditionalAddressInformation;

				var uNLOCO = OrgAddress.RelatedPortCode ?? OrgAddress.Header.UNLOCO;
				if (uNLOCO != null && uNLOCO.Country != null)
				{
					CountryName = uNLOCO.Country.RN_DescMultilingual;
					CountryCode = uNLOCO.RL_RN_NKCountryCode;
				}
			}
		}

		#endregion
	}

	public class AdditionalTransactionInfoForVietnamEInvoice
	{
		public ZGuid TransactionPK { get; set; }

		public ZGuid OriginalTransactionPK { get; set; }

		public ZString OriginalTransactionReference { get; set; }

		public ZString VATRegistrationNum { get; set; }

		public ZString BankName { get; set; }

		public ZString AccountNumber { get; set; }

		public ZGuid CompanyPK { get; set; }

		public ZGuid BranchPK { get; set; }

		public ZString RectifyReason { get; set; }

		public ZString RectifyDate { get; set; }

		public ZString RectifySupportingDocumentNumber { get; set; }

		public ZDate ComplianceDocumentDate { get; set; }

		public ZString ETA { get; set; }

		public ZString ETD { get; set; }

		public ZString OrderReference { get; set; }

		public ZString SendingAgent { get; set; }

		public ZString ReceivingAgent { get; set; }

		public ZString Carrier { get; set; }

		public ZString TransactionNumber { get; set; }

		public bool IsConsolInvoice { get; set; }

		public bool IsDeclarationInvoice { get; set; }

		public ZString MasterBillNumber { get; set; }

		public ZString ConsolID { get; set; }

		public ZString TransportInfo { get; set; }

		public ZString Package { get; set; }

		public ZString Weight { get; set; }

		public ZString ChargeableWeight { get; set; }

		public ZString Volume { get; set; }

		public ZString TransportMode { get; set; }

		public ZString OriginPort { get; set; }

		public ZString DestinationPort { get; set; }

		public ZString FormattedAddress { get; set; }

		public ZString InvoiceCreatingStaff { get; set; }

		public ZString TransactionReference { get; set; }

		public ZString TransactionParentID { get; set; }

		public Dictionary<ZString, ZString> TransactionLineHouseBillDictionary { get; set; }

		public AdditionalContactInfo ContactInfo { get; set; }

		public AdditionalJobInfo JobInfo { get; set; }

		public AdditionalComplianceSequenceInfo ComplianceSequenceInfo { get; set; }

		public ZString SequenceNumber
		{
			get
			{
				return GetSequenceNumber(false);
			}
		}

		public ZString OriginalSequenceNumber
		{
			get
			{
				return GetSequenceNumber(true);
			}
		}

		ZString GetSequenceNumber(bool isOriginal)
		{
			var result = string.Empty;
			var transactionReference = isOriginal ? OriginalTransactionReference : TransactionReference;

			if (!string.IsNullOrEmpty(transactionReference))
			{
				result = transactionReference;

				if (ComplianceSequenceInfo != null)
				{
					var seriesPrefix = isOriginal ? ComplianceSequenceInfo.OriginalSeriesPrefix : ComplianceSequenceInfo.SeriesPrefix;
					var isComplianceNumberFormatDefault = isOriginal ? ComplianceSequenceInfo.OriginalIsComplianceNumberFormatDefault : ComplianceSequenceInfo.IsComplianceNumberFormatDefault;

					if (!string.IsNullOrEmpty(seriesPrefix) && isComplianceNumberFormatDefault && transactionReference.IndexOf(seriesPrefix) == 0)
					{
						result = transactionReference.Remove(0, seriesPrefix.Length);
					}
				}
			}

			return result;
		}
	}

	public class AdditionalContactInfo
	{
		public ZString Name { get; set; }

		public List<ZString> Mails { get; set; }

		public ZString Phone { get; set; }
	}

	public class AdditionalJobInfo
	{
		public ZString JobDepartmentDesc { get; set; }

		public ZString JobOperationStaff { get; set; }

		public ZString JobSalesStaff { get; set; }
	}

	public class AdditionalComplianceSequenceInfo
	{
		public ZString OriginalSeriesPrefix { get; set; }

		public ZString SeriesPrefix { get; set; }

		public bool OriginalIsComplianceNumberFormatDefault { get; set; }

		public bool IsComplianceNumberFormatDefault { get; set; }

		public ZByte OriginalComplianceSequenceMaximumNumberDigits { get; set; }

		public ZByte ComplianceSequenceMaximumNumberDigits { get; set; }
	}
}
