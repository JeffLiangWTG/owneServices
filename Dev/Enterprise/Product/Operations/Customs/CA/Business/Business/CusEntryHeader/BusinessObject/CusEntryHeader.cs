using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.Integration;
using EDIMessageStatusList = Enterprise.Messaging.Integration.EDIMessageStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[UniversalDataContext(DataContextType.CAIntegratedImportDeclaration)]
	public partial class CusEntryHeader : Customs.Business.CusEntryHeader, IServiceLocator, IAddInfoManager, Integration.Customs.CA.ICACusEntryHeader
	{
		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusEntryHeader.Schema
		{
			public const string CA_PortOfClearanceOverride = "CA_PortOfClearanceOverride";
			public const string EffectivePortOfClearance = "EffectivePortOfClearance";
		}

		protected override ZString HumanReadableNameCore => Res.GetString("02AEC68A-511E-44D9-BAA7-833DB9E59D60", "Customs Entry ({0}) {1}", CH_MessageType, CH_BGMReference);

		#region New Properties

		#region Port Of Clearance Override

		public ZString EffectivePortOfClearance
		{
			get
			{
				if (CA_PortOfClearanceOverride.IsEmpty && CH_EntrySubmittedDate.IsValid)
				{
					return Declaration.ReleaseOffice;
				}

				return CA_PortOfClearanceOverride;
			}
		}

		public ZString CA_PortOfClearanceOverride
		{
			get { return AddInfo.CA_PortOfClearanceOverride; }

			set
			{
				if (Declaration.ReleaseOffice != value)
				{
					AddInfo.CA_PortOfClearanceOverride = value;
				}
				else
				{
					AddInfo.CA_PortOfClearanceOverride = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo CA_PortOfClearanceOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CA_PortOfClearanceOverride, x => AddInfo.CA_PortOfClearanceOverrideInfo); }
		}

		#endregion

		public bool HasDiscrepancyInTotalDutyAndTaxes
		{
			get
			{
				if (hasDiscrepancyInTotalDutyAndTaxes == null)
				{
					hasDiscrepancyInTotalDutyAndTaxes = new CachedProperty<ZBool>(Factory, () =>
					{
						var result = true;
						if (IsCAD)
						{
							result = DutyFeeChangedSinceLastResponse || MergedLines.Any(x => x.CL_CommoditySequence.IsEmpty);
						}
						else if (IsB3C)
						{
							var originalB3 = (from EDIMessage message in Messages
																where message != null
																	&& message.EM_MessageType == MessageTypeList.Codes.B3CUSDEC
																	&& message.IsTransmitMessage
																	&& message.EM_Status == EDIMessage.Status.Sent
																orderby message.EM_SystemCreateTimeUtc descending
																select message).FirstOrDefault();

							if (originalB3 != null)
							{
								var b3Message = originalB3 as B3Message;
								IB3Header wrapperResult = new B3AsLodgedDocumentWrapper(b3Message);
								var totalValueOnSentMessage = wrapperResult.PositiveTotalAmounts.TotalAllDutyAndTaxes;

								if (totalValueOnSentMessage == TotalDutyAndTax)
								{
									result = false;
								}
							}
						}

						return result;
					});
				}
				return hasDiscrepancyInTotalDutyAndTaxes.Value;
			}
		}
		CachedProperty<ZBool> hasDiscrepancyInTotalDutyAndTaxes;

		#region Calculated totals

		public ZDecimal TotalSimaDuty
		{
			get
			{
				setTotalAmounts();
				return totalSimaDutyAmount;
			}
		}

		public ZDecimal TotalExciseTax
		{
			get
			{
				setTotalAmounts();
				return totalExciseTaxAmount;
			}
		}

		public ZDecimal TotalDutyAndTax
		{
			get
			{
				setTotalAmounts();
				return totalDutyAndTaxAmount;
			}
		}

		void setTotalAmounts()
		{
			if (!setTotalAmountsDone)
			{
				totalSimaDutyAmount = 0m;
				totalExciseTaxAmount = 0m;
				foreach (CusEntryLine entryLine in MergedLines)
				{
					totalSimaDutyAmount += entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalSIMAAmount)
	+ entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount);
					totalExciseTaxAmount += entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalExciseTaxAmount);
				}
				totalDutyAndTaxAmount = TotalDutyAmount + totalSimaDutyAmount + totalExciseTaxAmount + GSTAmount;
				setTotalAmountsDone = true;
			}
		}
		bool setTotalAmountsDone;
		ZDecimal totalDutyAndTaxAmount;
		ZDecimal totalSimaDutyAmount;
		ZDecimal totalExciseTaxAmount;

		#endregion

		#region Positive And Negative Amounts

		public ITotalAmounts PositiveTotalAmounts
		{
			get { return IsDutyPayable ? ClassificationLines.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Positive, Declaration.CA_AnySightDepositAmount) : ClassificationLines.GetEmptyTotalAmounts(); }
		}

		public ITotalAmounts NegativeTotalAmounts
		{
			get { return IsDutyPayable ? ClassificationLines.GetTotalAmounts(MessageConstants.B3RecordIdentifiers.Negative, Declaration.CA_AnySightDepositAmount) : ClassificationLines.GetEmptyTotalAmounts(); }
		}

		bool IsDutyPayable
		{
			get { return !Declaration.IsWarehouseEntry || Declaration.IsExWarehouseAndDutyPayable; }
		}

		public IEnumerable<IClassificationLine1> ClassificationLines
		{
			get { return AllEntryLines.Cast<IClassificationLine1>(); }
		}

		#endregion

		#region Booleans

		public ZBool DutyFeeChangedSinceLastResponse
		{
			get { return MergedLines.Any(x => x.DutyFeeChangedSinceLastResponse); }
		}

		public bool IsDataLoadingModule
		{
			get { return CH_MessageType == MessageTypeList.Codes.DataLoadingModule; }
		}

		public bool IsG7ExportDeclaration
		{
			get { return CH_MessageType == MessageTypeList.Codes.G7Export; }
		}

		public bool IsImportEDIRelease
		{
			get { return CH_MessageType == MessageTypeList.Codes.EDIRelease; }
		}

		public bool IsB3CorCAD
		{
			get { return CH_MessageType == MessageTypeList.Codes.B3CUSDEC || CH_MessageType == MessageTypeList.Codes.CommercialAccountingDeclaration; }
		}

		public bool IsB3C
		{
			get { return CH_MessageType == MessageTypeList.Codes.B3CUSDEC; }
		}

		public bool IsCAD
		{
			get { return CH_MessageType == MessageTypeList.Codes.CommercialAccountingDeclaration; }
		}

		public bool IsClearedB3CorCAD
		{
			get { return IsB3CorCAD && (CH_EntryStatus == B3EntryStatusList.Codes.Accepted || CH_EntryStatus == B3EntryStatusList.Codes.Confirmed || CH_EntryStatus == CADEntryStatusList.Codes.Approved); }
		}

		protected override bool IsExportCore() => Declaration != null && Declaration.IsExport;

		public ZBool NeedCancelDeferredB3CADMessage { get; set; }

		public ZBool NeedResendDeferredB3CADMessage { get; set; }

		public ZBool NeedResendB3CADMessageSilent { get; set; }

		public ZDateTime OriginalDeferredB3CADMessageTime { get; set; }

		#endregion

		#region Export Declaration Document

		public ZString BGMReference
		{
			get { return CH_BGMReference; }
		}

		public ZString LicenceAndPermits
		{
			get
			{
				List<ZString> list = new List<ZString>();
				if (Declaration != null)
				{
					foreach (DeclarationExportPermit permit in Declaration.Permits)
					{
						if (!list.Contains(permit.CY_Data))
						{
							list.Add(permit.CY_Data);
						}
					}
					foreach (JobComInvoiceLine invoiceLine in InvoiceLines)
					{
						foreach (InvoiceLineExportPermit permit in invoiceLine.Permits)
						{
							if (!list.Contains(permit.CY_Data))
							{
								list.Add(permit.CY_Data);
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(ExportLicenceNumber))
				{
					list.Add(ExportLicenceNumber);
				}

				if (!CAEDAuthorizationID.IsEmpty)
				{
					list.Add("Auth.ID:" + CAEDAuthorizationID);
				}

				return list.ConcatenateWithPageDelimiter("/", 100, 300);
			}
		}

		public ZString CAEDAuthorizationID
		{
			get
			{
				var supplier = Declaration.Supplier;
				return supplier == null ? ZString.Empty
								: supplier.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.AuthorizationID);
			}
		}

		public ZString ConveyanceIdentificationNos
		{
			get { return this.MergedLines.Select(x => x.ConveyanceIdentificationNumber).ConcatenateWithPageDelimiter(",", 110, 360); }
		}

		#endregion

		#region ExportLicenceNumber

		public ZString ExportLicenceNumber
		{
			get
			{
				if (exportLicenceNumber.IsEmpty && Declaration?.ExportLicenceProxy is OrgHeader orgProxy)
				{
					var orgCustomCode = orgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.ExportLicenceNumber, Constants.CountryCodes.Canada);
					if (orgCustomCode != null)
					{
						exportLicenceNumber = orgCustomCode.OK_CustomsRegNo;
					}
				}
				return exportLicenceNumber;
			}
		}
		ZString exportLicenceNumber;

		#endregion

		#region CH_CERSProofOfReportNumber

		public ZString CH_CERSProofOfReportNumber
		{
			get { return Declaration != null ? Declaration.JE_CERSProofOfReportNumber : ZString.Empty; }
		}

		#endregion

		public bool IsB3GrossWeightSet { get; set; }

		public ActiveBusinessObjectCollection<EDIMessage> MessagesForDisplay
		{
			get
			{
				if (messagesForDisplay == null)
				{
					var query = new ZQuery(Messages.CompleteFilter);
					if (IsImportEDIRelease)
					{
						var declaration = Declaration;
						var declarationPK = declaration?.PK ?? ZGuid.Invalid;
						var shipmentPK = declaration?.Shipment?.PK ?? ZGuid.Invalid;
						query.AddToFilter(EDIMessageQueryHelper.GetEDIMessageGenPivotQuery(new[] { PK, declarationPK, shipmentPK }, new ZString[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent, UniversalEventMessageTypes.Codes.IIDResponses, UniversalEventMessageTypes.Codes.D4Notices }), JoinCondition.Or);
					}
					messagesForDisplay = new ActiveBusinessObjectCollection<EDIMessage>(Factory, query);
				}
				return messagesForDisplay;
			}
		}
		ActiveBusinessObjectCollection<EDIMessage> messagesForDisplay;

		#endregion

#if DEBUG

		public void ResetCachedValuesForTest()
		{
			exportLicenceNumber = ZString.Empty;
			setTotalAmountsDone = false;
		}

#endif

		#region Overrides

		public override ZDateTime EffectiveValuationDate
		{
			get { return IsCAD && Declaration != null ? Declaration.DateOfValuation : base.EffectiveValuationDate; }
		}

		public override void PopulateEntrySubmittedDateIfRequired(ZDateTime? submittedDate = null)
		{
			if (Declaration != null)
			{
				var manualSubmissionBo = new ManualSubmissionBO(Declaration, CH_MessageType, Declaration.Factory);
				var manualSubmisionDate = manualSubmissionBo.CurrentEntrySubmissionBO.ManualSubmissionDate;
				if (!manualSubmisionDate.IsEmpty && CH_EntrySubmittedDate == manualSubmisionDate)
				{
					CH_EntrySubmittedDate = ZDateTime.Empty;
					CA_PortOfClearanceOverride = ZString.Empty;
				}
			}

			base.PopulateEntrySubmittedDateIfRequired(submittedDate);
		}

		[ReadOnly(true)]
		public override ZDateTime CH_EntrySubmittedDate
		{
			get { return base.CH_EntrySubmittedDate; }
			set { base.CH_EntrySubmittedDate = value; }
		}

		public override ZString ReferenceNumber
		{
			get
			{
				return Declaration.IsLVX && Declaration.Invoices.Count > 0 ? Res.GetString("EE0D2F02-5C99-4C95-85E1-6CFF68C17669", " LVS ID : {0}", Declaration.Invoices[0].JZ_InvoiceNumber) : base.ReferenceNumber.ToString();
			}
		}

		#region Overrides for Accounting Integration

		bool IsB3NonLVS
		{
			get { return IsB3CorCAD && Declaration != null && (!Declaration.IsConsolidatedLVS || Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.ConsolidationByImporter); }
		}

		public override bool IsFormalEntry
		{
			get { return IsB3NonLVS; }
		}

		protected override ZString GetUniqueNumberForAccountingIntegrationCore()
		{
			return CACustomsDataRegistry.Instance.SuspendAssignmentOfEntryNumberToDisbursementCharges.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty) ? ZString.Empty : base.GetUniqueNumberForAccountingIntegrationCore();
		}

		protected override bool HasBeenLodgedAtCustomsForAccIntegration
		{
			get { return IsB3NonLVS && HasBeenLodgedAtCustoms; }
		}

		protected override bool IsChangingToClearStatusForAccIntegration
		{
			get { return IsB3NonLVS && base.IsChangingToClearStatusForAccIntegration; }
		}

		#endregion

		public override ZDecimal TotalAmountPayable
		{
			get
			{
				ZDecimal result = 0m;
				if (!Declaration.IsImporterDirectPaymentAutoRated)
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						result += entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalDutyAmount)
							+ entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalSIMAAmount)
							+ entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalExciseTaxAmount)
							+ entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalGSTAmount)
							+ entryLine.Fees.GetAmount(Enterprise.Customs.CA.Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount);
					}
				}
				return result;
			}
		}

		protected override bool IsCustomsClearedEventSupported
		{
			get { return base.IsCustomsClearedEventSupported && !IsB3CorCAD; }
		}

		protected override Customs.Business.WeightUQCalculator GetWeightCalculator()
		{
			return new WeightUQCalculator(this);
		}

		public override bool HasBeenLodgedAtCustoms
		{
			get { return !IsDataLoadingModule && StatusCalculator != null ? StatusCalculator.IsLodged(CH_EntryStatus) : StatusLogManager.HasAClearLog(Logs, CH_MessageType, (IStatusList)Lookups.MessageStatusList); }
		}

		public override bool NeedToMaintainLinesDuringMerge
		{
			get { return false; }
		}

		public override bool ShouldLogCustomsClearedToDeclarationOrShipment
		{
			get { return !IsDataLoadingModule; }
		}

		public override bool ShouldPopulateJE_EntrySubmittedDate
		{
			get { return Declaration.JE_EntrySubmittedDate.IsEmpty || Declaration.JE_EntrySubmittedDate > CH_EntrySubmittedDate; }
		}

		public override bool HasBeenWithdrawn
		{
			get { return !IsDataLoadingModule && StatusCalculator != null && StatusCalculator.IsWithdrawn(CH_EntryStatus); }
		}

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return !HasBeenWithdrawn;
		}

		protected override ZDecimal TransactionValueCore
		{
			get { return InvoiceHeaders.Sum(header => header.InvoiceAmountInCAD); }
		}

		public override ZString EntryNumber
		{
			get { return Declaration != null && Declaration.IsImport ? Declaration.DeclarationNumber : base.EntryNumber; }
		}

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			return CusEntryNumber.Load(this, EntryNumberType, Constants.CountryCodes.Canada);
		}

		protected override ZString EntryNumberType
		{
			get
			{
				return IsG7ExportDeclaration ? Declaration?.JE_MessageType ?? ZString.Empty : base.EntryNumberType;
			}
		}

		protected override bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus)
		{
			return !IsStatusClear((ZString)CH_EntryStatusInfo.OriginalValue) && IsStatusClear(CH_EntryStatus);
		}

		protected override bool IsStatusClear(string status)
		{
			return IsImportEDIRelease ?
				status == EDIReleaseImportEntryStatusList.Codes.GoodsReleased || status == EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired :
				this.IsB3CorCAD ?
				status == B3EntryStatusList.Codes.Accepted || status == B3EntryStatusList.Codes.Confirmed || status == CADEntryStatusList.Codes.Approved :
				status == EntryStatusList.Codes.Clear;
		}

		protected override ZDateTime GetCustomsClearedDateWithoutUsingLoggedEvent()
		{
			return IsImportEDIRelease ?
				Declaration.JE_EntryAuthorisationDate :
				base.GetCustomsClearedDateWithoutUsingLoggedEvent();
		}

		public override ZString ClearanceEventReference
		{
			get { return CH_EntryStatus; }
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Factory.GetCachedValue<Registry.EntryChargeTypeList>();
		}

		internal ZDecimal GetTotalPSTForCasualImport()
		{
			var result = ZDecimal.Zero;
			foreach (JobComInvoiceLine line in InvoiceLines)
			{
				result += line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CPT);
				result += line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CTA);
			}
			return result;
		}

		internal Dictionary<OrganizationReferenceKey, ZDecimal> GetLVSTotalChargeValueByDebtorFor(EntryChargeType chargeTypeElement)
		{
			var amountsByBuyer = new Dictionary<OrganizationReferenceKey, ZDecimal>();
			var lvsIDsByBuyer = new Dictionary<OrganizationReferenceKey, List<ZString>>();
			var entryHeader = Declaration.B3EntryHeader;
			if (entryHeader != null)
			{
				foreach (JobComInvoiceLine line in entryHeader.InvoiceLines)
				{
					var amount = CaculateInvoiceAmountPerChargeType(chargeTypeElement, line);
					var invoiceHeader = line.InvoiceHeader;
					if (!amount.IsEmpty && invoiceHeader != null)
					{
						var importer = Declaration.IsLVSTotalConsolidation ? invoiceHeader.Importer_Effective : Declaration.Importer;
						var buyer = importer;
						var reference = ZString.Empty;
						if (!LVSDeclarationRatingAdapter.IsCollect(invoiceHeader.IncoTerm, ChargeCodeGroupList.Codes.CustomsDuty))
						{
							if (importer != null)
							{
								reference = importer.OH_Code;
							}

							buyer = (Declaration.Job != null && Declaration.Job.AgentCollect != null ? Declaration.Job.AgentCollect : null);
						}
						if (buyer != null)
						{
							buyer = buyer.DeliveryCustomsBillTo;
						}

						OrganizationReferenceKey key;
						if (importer != null && OrgImpAddInfo.Get(importer).ZO_EffectiveLVSInvoiceDetailCode == LVSInvoiceDetailCodes.Codes.Detail)
						{
							key = new OrganizationReferenceKey(buyer, reference, invoiceHeader.JZ_InvoiceNumber);// when details are required add LVSID as reference 2 to iitemise by individual shipment
						}
						else
						{
							key = new OrganizationReferenceKey(buyer, reference);
						}
						if (!amountsByBuyer.ContainsKey(key))
						{
							amountsByBuyer.Add(key, amount);
							lvsIDsByBuyer.Add(key, new List<ZString>());
						}
						else
						{
							amountsByBuyer[key] += amount;
						}
						lvsIDsByBuyer[key].Add(invoiceHeader.JZ_InvoiceNumber);
					}
				}
			}

			var result = new Dictionary<OrganizationReferenceKey, ZDecimal>();
			foreach (OrganizationReferenceKey key in amountsByBuyer.Keys)
			{
				ZStringBuilder refBuilder;
				if (key.Reference2.IsEmpty)
				{
					refBuilder = new ZStringBuilder(Declaration.DeclarationNumber);
					refBuilder.Append(Res.GetString("f2f02e32-8cee-49f0-bad1-38a293f0bcda", "  LVS IDs: {0}", ZString.Join(", ", lvsIDsByBuyer[key].Distinct().OrderBy(x => x).ToArray())));
				}
				else
				{
					refBuilder = new ZStringBuilder(string.Format("{0} - {1}", Declaration.DeclarationNumber, key.Reference2));
				}
				if (!key.Reference.IsEmpty)
				{
					refBuilder.Append(Res.GetString("3b6fd98a-464b-4bc6-a130-6f5d971c2b8e", "  Importer: {0}", key.Reference));
				}

				result.Add(new OrganizationReferenceKey(key.Organization, refBuilder.ToStringWithNewLineBetweenAppends()), amountsByBuyer[key]);
			}

			return result;
		}

		ZDecimal CaculateInvoiceAmountPerChargeType(EntryChargeType chargeType, JobComInvoiceLine line)
		{
			var amount = ZDecimal.Zero;
			switch (chargeType.Code)
			{
				case Registry.EntryChargeTypeList.Codes.TotalDutyAmount:
					amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.CustomsDuty);
					break;
				case Registry.EntryChargeTypeList.Codes.TotalSIMAAmount:
					if (line.DutyAndTaxManager.SIMADuties.Any() && IDutyAndTaxDataExtensions.IsSimaAmountPayable(line.DutyAndTaxManager.SIMADuties.First().C1_ExemptCode))
					{
						amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);
					}
					break;
				case Registry.EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount:
					if (line.DutyAndTaxManager.SIMADuties.Any() && !IDutyAndTaxDataExtensions.IsSimaAmountPayable(line.DutyAndTaxManager.SIMADuties.First().C1_ExemptCode))
					{
						amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.SIMADuty);
					}
					break;
				case Registry.EntryChargeTypeList.Codes.TotalExciseTaxAmount:
					amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.ExciseTax);
					break;
				case Registry.EntryChargeTypeList.Codes.TotalGSTAmount:
					if (!line.IsGSTDirectPayment)
					{
						amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
					}
					break;
				case Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount:
					if (line.IsGSTDirectPayment)
					{
						amount = line.DutyAndTaxManager.GetTotalAmount(DutyAndTaxTypes.Codes.GST);
					}
					break;
			}
			return amount;
		}

		internal Dictionary<OrganizationReferenceKey, ZDecimal> GetIM2TotalChargeValueByDebtorFor(EntryChargeType chargeType)
		{
			var result = new Dictionary<OrganizationReferenceKey, ZDecimal>();
			var entryHeader = Declaration.B3EntryHeader;
			var previousEntryHeader = Declaration.PreviousJob.B3EntryHeader;

			var effectivevalue = entryHeader.GetTotalChargeValueFor(chargeType, "") - previousEntryHeader.GetTotalChargeValueFor(chargeType, "");
			if (effectivevalue < 0 && chargeType.Code == Registry.EntryChargeTypeList.Codes.TotalGSTAmount)
			{
				effectivevalue = ZDecimal.Zero;
			}
			result.Add(new OrganizationReferenceKey(null, ZString.Empty), effectivevalue);
			return result;
		}

		internal Dictionary<OrganizationReferenceKey, ZDecimal> GetB2TotalChargeValueByDebtorFor(EntryChargeType chargeType)
		{
			var result = new Dictionary<OrganizationReferenceKey, ZDecimal>();
			var invoiceHeaders = Declaration.Invoices.Cast<JobComInvoiceHeader>();

			var claimAccount = ZDecimal.Zero;
			var accountedAccount = ZDecimal.Zero;

			foreach (JobComInvoiceLine accountLine in invoiceHeaders.SelectMany(header => header.AsAccountForFilteredInvoiceLines.Cast<JobComInvoiceLine>()))
			{
				foreach (JobComInvoiceLine claimLine in accountLine.ReadOnlyAsClaimForFilteredInvoiceLines)
				{
					claimAccount += CaculateInvoiceAmountPerChargeType(chargeType, claimLine);
				}
				accountedAccount += CaculateInvoiceAmountPerChargeType(chargeType, accountLine);
			}
			var effectivevalue = claimAccount - accountedAccount;

			if (effectivevalue < 0 && chargeType.Code == Registry.EntryChargeTypeList.Codes.TotalGSTAmount)
			{
				effectivevalue = ZDecimal.Zero;
			}
			result.Add(new OrganizationReferenceKey(null, ZString.Empty), effectivevalue);
			return result;
		}

		public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPayment, ILogger logger)
		{
			bool result = true;
			if (Declaration != null)
			{
				var isTransferOfGoods = Declaration.IsCADEnabled
					? Declaration.JE_MessageSubType != CADEntryTypeList.Codes.TransferOfGoods301 && Declaration.JE_MessageSubType != CADEntryTypeList.Codes.TransferOfGoods302
					: Declaration.JE_MessageSubType != B3EntryTypeList.Codes.TransferOfGoods30;
				result = IsFeePaidByBroker(feeCode, Declaration.EffectiveImporterAddInfo, () => Declaration.IsImporterDirectPaymentAutoRated, () => Declaration.IsGSTDirectPayment, () => Declaration.IsGSTDirectAutoRated,
					() => !Declaration.IsInwardWarehouseEntry && isTransferOfGoods && !Declaration.IsImporterAccountSecurityCodeUsed);
			}
			return result;
		}

		public bool IsFeePaidByBroker(string feeCode, ZGuid importerPK)
		{
			bool result = true;
			if (importerPK.IsValid)
			{
				if (Declaration != null)
				{
					var isTransferOfGoods = Declaration.IsCADEnabled
					? Declaration.JE_MessageSubType != CADEntryTypeList.Codes.TransferOfGoods301 && Declaration.JE_MessageSubType != CADEntryTypeList.Codes.TransferOfGoods302
					: Declaration.JE_MessageSubType != B3EntryTypeList.Codes.TransferOfGoods30;
					result = !Declaration.IsInwardWarehouseEntry && isTransferOfGoods;
					if (result)
					{
						var importer = Factory.Load<OrgHeader>(importerPK);
						if (importer != null)
						{
							var importerAddInfo = OrgImpAddInfo.Get(importer);
							result = IsFeePaidByBroker(feeCode, importerAddInfo, () => Declaration.IsImporterDirectPaymentAutoRatedCore(importerAddInfo),
								() => Declaration.IsGSTDirectPaymentCore(importerAddInfo), () => JobDeclaration.IsGSTDirectAutoRatedCore(importerAddInfo), () => true);
						}
					}
				}
			}
			else
			{
				result = IsFeePaidByBroker(feeCode, "", null);
			}
			return result;
		}

		bool IsFeePaidByBroker(string feeCode, OrgImpAddInfo importerAddInfo, Func<bool> isImporterDirectPayment, Func<bool> isGSTDirectPayment, Func<bool> isGSTDirectAutoRated, Func<bool> declarationPreCheck)
		{
			var result = true;
			if (IsCAD)
			{
				result = importerAddInfo?.ZO_CADIsBrokerToPay ?? false;
			}
			else
			{
				result = declarationPreCheck();
				if (result)
				{
					if (feeCode == Registry.EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount || isImporterDirectPayment())
					{
						result = false;
					}
					else if (feeCode == Registry.EntryChargeTypeList.Codes.TotalGSTAmount)
					{
						result = !isGSTDirectPayment();
					}
					else if (feeCode == Registry.EntryChargeTypeList.Codes.TotalGSTDirectAmount)
					{
						result = isGSTDirectPayment() && isGSTDirectAutoRated();
					}
				}
			}
			return result;
		}

		#region Overrides for Bonded Warehouse

		protected override string GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage()
		{
			return (Declaration?.IsExWarehouseEntry ?? false) ? JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresPTNAndPTLN : base.GetInvoiceLineMarkedForBondedWarehousingRequiresEntryDetailsMessage();
		}

		protected override bool IsInwardBondedWarehousingEnabledCore
		{
			get { return Declaration?.IsInwardWarehouseEntry ?? false; }
		}

		protected override bool IsOutwardBondedWarehousingEnabledCore
		{
			get { return Declaration?.IsExWarehouseEntry ?? false; }
		}

		#endregion

		#region OnSaving & OnFactorySaving

		public override void OnSaving()
		{
			base.OnSaving();
			FillInCH_BGMReference();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				EntryNumber = ZString.Empty;
			}
			base.OnSaved(saveSucceeded);
		}

		public void PopulateEntryNumberIfNeeded()
		{
			if (IsExport && !ExportLicenceNumber.IsEmpty && (!IsInDatabase || EntryNumber.Length != 13))
			{
				EntryNumber = GenerateEntryNumber();
			}
		}

		ZString GenerateEntryNumber()
		{
			ZString fountainKey = ZDateTime.Now.Year.ToString();
			fountainKey = fountainKey.SubstringSafe(fountainKey.Length - 1, 1);
			fountainKey = ExportLicenceNumber + fountainKey;

			return fountainKey + Env.NumberFountains.GetCAEntryNumberGeneratorFountain(fountainKey).GetNextFormatted(Factory).Substring(2, 6);
		}

		internal void FillInCH_BGMReference()
		{
			if (CH_BGMReference.IsEmpty && IsExport)
			{
				Declaration.PopulateJE_DeclarationReferenceIfNeeded();
				CH_BGMReference = Declaration.JE_DeclarationReference;
			}
			if (Declaration.IsImportIncludingB2 && (CH_BGMReference.IsEmpty || CH_BGMReference.EndsWith("000000000", StringComparison.OrdinalIgnoreCase)))
			{
				CH_BGMReference = Declaration.TransactionNumber;
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			ReCalculateDeclarationStatus();
			AddStatusCustomsLogIfRequired();
			CancelDeferredB3MessageIfRequired();
		}

		#region ReCalculateDeclarationStatus

		void ReCalculateDeclarationStatus()
		{
			if (Declaration != null && (CH_EntryStatusInfo.HasChanges || CH_StatusInfo.HasChanges || Messages.HasChanges))
			{
				Declaration.DeriveDeclarationStatus();
				Declaration.JE_MessageStatusDescriptionInfo.RefreshBinding();
			}
		}

		#endregion

		#region AddStatusCustomsLogIfRequired

		void AddStatusCustomsLogIfRequired()
		{
			if (IsDataLoadingModule)
			{
				if (CH_StatusInfo.HasChanges)
				{
					StatusLogManager.AddALogIfNecessary(Logs, CH_Status);
				}
			}
			else
			{
				if (CH_EntryStatusInfo.HasChanges)
				{
					StatusLogManager.AddAStatusLog(Logs, CH_EntryStatus);

					if (IsClearedB3CorCAD && Declaration != null && Declaration.IsConsolidatedLVS)
					{
						foreach (var invoice in Declaration.Invoices)
						{
							var declaration = (JobDeclaration)invoice.JobDeclaration;
							if (declaration.IsLVX)
							{
								foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
								{
									StatusLogManager.AddAStatusLog(entryHeader.Logs, CH_EntryStatus);
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region CancelDeferredB3MessageIfRequired

		void CancelDeferredB3MessageIfRequired()
		{
			if (NeedCancelDeferredB3CADMessage && IsB3CorCAD)
			{
				CancelAndDeactivateDeferredB3Message();
				NeedCancelDeferredB3CADMessage = false;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		#region StatusCalculator

		internal EDIFACTMessageStatusCalculator StatusCalculator
		{
			get
			{
				if (statusCalculator == null)
				{
					switch (CH_MessageType)
					{
						case MessageTypeList.Codes.G7Export:
							statusCalculator = new G7ExportStatusCalculator();
							break;
						case MessageTypeList.Codes.EDIRelease:
							statusCalculator = new EDIReleaseImportStatusCalculator();
							break;
						case MessageTypeList.Codes.B3CUSDEC:
							statusCalculator = new B3ImportStatusCalculator();
							break;
						case MessageTypeList.Codes.CommercialAccountingDeclaration:
							statusCalculator = new CADStatusCalculator();
							break;
					}
				}
				return statusCalculator;
			}
		}
		EDIFACTMessageStatusCalculator statusCalculator;

		#endregion

		#endregion

		#region IServiceLocator Members

		object IServiceLocator.GetService(Type serviceType)
		{
			if (serviceType == typeof(ICustomsCharges))
			{
				return new CACusEntryHeaderCustomsCharges(this);
			}
			return null;
		}

		#endregion

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region B3 Schedule Part

		public void CancelAndDeactivateDeferredB3Message()
		{
			var deferredMessages = DeferredB3Messages;
			if (deferredMessages != null)
			{
				foreach (var message in deferredMessages)
				{
					message.EM_IsActive = false;
					message.EM_Status = EDIMessageStatusList.Codes.Cancelled;
				}
			}
		}

		public ZDateTime DeferredB3MessageTime
		{
			get
			{
				if (deferredB3MessageTime == null)
				{
					deferredB3MessageTime = new CachedProperty<ZDateTime>(Factory, () =>
					{
						var result = ZDateTime.Empty;
						var deferredB3Messages = DeferredB3Messages;
						if (deferredB3Messages != null)
						{
							result = deferredB3Messages.Select(msg => msg.EM_HeldUntilDate).OrderBy(dt => dt).FirstOrDefault();
						}
						return result;
					});
				}
				return deferredB3MessageTime.Value;
			}
		}
		CachedProperty<ZDateTime> deferredB3MessageTime;

		public IEnumerable<EDIMessage> DeferredB3Messages
		{
			get
			{
				if (IsB3CorCAD)
				{
					if (deferredMessages == null)
					{
						deferredMessages = new CachedValue<IEnumerable<EDIMessage>>(() =>
						{
							IEnumerable<EDIMessage> result = null;
							if (Messages != null)
							{
								result = (from EDIMessage message in this.Messages
										  where message.EM_MessageType == CH_MessageType
										  && message.EM_Status == EDIMessageStatusList.Codes.Queued
										  && message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit
										  && !message.EM_HeldUntilDate.IsEmpty
										  && message.EM_IsActive
										  select message);
							}
							return result;
						});
					}
					return deferredMessages.Value;
				}
				else
				{
					return null;
				}
			}
		}
		CachedValue<IEnumerable<EDIMessage>> deferredMessages;

		#endregion

		public override ZString DefaultStatusDescription
		{
			get
			{
				return ZString.Empty;
			}
		}

		public override ZString CH_EntryStatus
		{
			get { return base.CH_EntryStatus; }
			set
			{
				var hasChanged = base.CH_EntryStatus != value;
				base.CH_EntryStatus = value;
				if (hasChanged && Declaration != null)
				{
					Declaration.SetDeclarationException();
					if (IsCAD)
					{
						Declaration.UpdateCA_AccountingAge();
					}
				}
			}
		}

		public override ZString CH_MessageType
		{
			get { return base.CH_MessageType; }
			set
			{
				var oldValue = CH_MessageType;
				base.CH_MessageType = value;
				if (!IsCopying && oldValue != CH_MessageType && Declaration != null)
				{
					Declaration.InvoiceLines?.MarkAsNeedingValidation();
					if (IsCAD)
					{
						Declaration.UpdateCA_AccountingAge();
					}
				}
			}
		}

		#region B13A Documents

		const int MergedLinesPagingFirst = 8;

		const int MegedLinesPagingSinceSecond = 32;

		CusEntryLineWrapperForB13ACollection mergedlinesForB13A;

		public CusEntryLineWrapperForB13ACollection MergedLinesForB13A
		{
			get
			{
				if (mergedlinesForB13A == null)
				{
					mergedlinesForB13A = new CusEntryLineWrapperForB13ACollection();
					MergedLines.Cast<CusEntryLine>().ForEach(line => mergedlinesForB13A.Add(new CusEntryLineWrapperForB13A(
						countryOfOrigin: line.CountryOfOrigin?.RN_Desc ?? ZString.Empty,
						provinceOfOrigin: line.ProvinceOfOrigin,
						effectiveDescription: line.EffectiveDescription,
						formattedTariff: line.FormattedTariff,
						customsQuantity: line.CustomsQuantity,
						customsUnitQtyDescription: line.CustomsUnitQtyDescription,
						valueFOBPointOfExit: line.ValueFOBPointOfExit
					)));
				}
				AlignLineWrappersAndContent(mergedlinesForB13A);
				return mergedlinesForB13A;
			}
		}

		void AlignLineWrappersAndContent(CusEntryLineWrapperForB13ACollection wrapperForB13ACollection)
		{
			var containerNumbers = (Declaration.CusContainers.Count - 1) / 25;
			var invoiceNumbers = Declaration.InvoiceNumbers;
			var conveyanceIdentificationNos = ConveyanceIdentificationNos;
			var licenceAndPermits = LicenceAndPermits;

			var requiredLines = MergedLines.Count;
			var requiredPageCount = 0;
			var pagerRegex = @"\*\d+\*";
			if (Regex.IsMatch(invoiceNumbers, pagerRegex)
				|| Regex.IsMatch(licenceAndPermits, pagerRegex))
			{
				requiredPageCount =
				(
					from match in Regex.Matches(invoiceNumbers, pagerRegex).Cast<Match>()
					select int.Parse(match.Value.Replace("*", string.Empty), CultureInfo.InvariantCulture)
				).Union(
					from match in Regex.Matches(conveyanceIdentificationNos, pagerRegex).Cast<Match>()
					select int.Parse(match.Value.Replace("*", string.Empty), CultureInfo.InvariantCulture)
				).Union(
					from match in Regex.Matches(licenceAndPermits, pagerRegex).Cast<Match>()
					select int.Parse(match.Value.Replace("*", string.Empty), CultureInfo.InvariantCulture)
				).Max();
			}

			var maxNum = new List<int> { requiredPageCount, containerNumbers };
			requiredPageCount = maxNum.Max() + 1;

			var requiredLinesByOtherContents = MergedLinesPagingFirst + MegedLinesPagingSinceSecond * (requiredPageCount - 2) + 1;
			if (requiredLinesByOtherContents > requiredLines)
			{
				requiredLines = requiredLinesByOtherContents;
			}

			while (wrapperForB13ACollection.Count < requiredLines)
			{
				wrapperForB13ACollection.AddNew();
			}
			while (wrapperForB13ACollection.Count > requiredLines)
			{
				wrapperForB13ACollection.Remove(wrapperForB13ACollection[wrapperForB13ACollection.Count - 1]);
			}
		}

		#endregion

	}
}
