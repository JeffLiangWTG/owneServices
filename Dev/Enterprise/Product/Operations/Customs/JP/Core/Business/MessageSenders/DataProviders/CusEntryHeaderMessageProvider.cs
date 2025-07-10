using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;
using ILocation = CargoWise.Customs.JP.MessageDefinitions.ILocation;

namespace Enterprise.Customs.JP.Business
{
	sealed class CusEntryHeaderMessageProvider : IIDAEntry, IEDAEntry, IEntrySubmission, IECR, ICEW, IEAC
	{
		public CusEntryHeaderMessageProvider(MessageSendingObject sendingObject)
		{
			entryHeader = Argument.NotNull(sendingObject?.Header, nameof(entryHeader));
			declaration = Argument.NotNull(sendingObject?.Header?.Declaration, nameof(declaration));

			this.sendingObject = sendingObject;
			this.entryInstruction = entryHeader.EntryInstruction;
			this.isImport = entryHeader.IsImport;
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly MessageSendingObject sendingObject;
		readonly CusEntryInstruction entryInstruction;
		readonly bool isImport;

		public string DeclarationNumber => entryHeader.EntryNumber;

		public string ValueType => entryInstruction?.CEI_ValueType;

		public string DeclarationType => entryInstruction?.CEI_Style;

		public string DeclarationSubType => entryInstruction?.CEI_SubStyle;

		public string CargoType
		{
			get
			{
				if (sendingObject.ProcedureCode == JPProcedureCodeList.Codes.ECR)
				{
					return entryInstruction?.CEI_ECRCargoType;
				}
				else
				{
					return entryInstruction?.CEI_DeclarationCargoType;
				}
			}
		}

		public string MainPartyType
		{
			get
			{
				var documentaryAddress = declaration.IsImport
					? declaration.ImporterDocumentaryAddress
					: declaration.IsExport
						? declaration.SupplierDocumentaryAddress
						: null;
				if (documentaryAddress == null)
				{
					return "2";
				}
				else if (documentaryAddress.E2_AddressOverride)
				{
					return documentaryAddress.E2_GovRegNumType == OrgCusCode.JapanCodeTypes.LPC && !documentaryAddress.E2_GovRegNum.IsEmpty ? "1" : "2";
				}
				else
				{
					var orgCusCodes = documentaryAddress.Organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.LPC);
					return !string.IsNullOrWhiteSpace(orgCusCodes) ? "1" : "2";
				}
			}
		}
		public string CustomsOffice => declaration.JE_CustomsOffice;

		public string CustomsOfficeDepartment => declaration.JE_CustomsOfficeDepartment;

		public string CustomsOfficeForSepcialDeclaration => entryInstruction?.CEI_CustomsOfficeForSpecialDeclarations;

		public string CustomsOfficeDepartmentForSpecialDeclaration => entryInstruction?.CEI_CustomsOfficeDepartmentForSpecialDeclarations;

		public DateTime? DeclarationDate
		{
			get
			{
				if (entryInstruction != null)
				{
					var dateForDuty = entryInstruction.CEI_DateForDuty;
					return GetDateTime(dateForDuty);
				}
				return null;
			}
		}

		IJapaneseAddress IIDAEntry.Importer => TryGetJobDocAddressProvider(declaration.ImporterDocumentaryAddress, true);

		public IAttorneyForCustomsProcedures AttorneyForCustomsProcedures => TryGetAttorneyForCustomsProceduresProvider(declaration);

		public IJapaneseAddress CustomsDepot => TryGetJobDocAddressProvider(declaration.DepotDocAddress);

		public string ComprehensiveDeclarationType
		{
			get
			{
				var comprehensiveDeclarationType = ZString.Empty;
				if (declaration.IsSea && (declaration.Bills.Count > 1 || (declaration.Bills.Count == 1 && declaration.Bills[0].CU_BillType == BillTypeList.Codes.MasterBill)))
				{
					switch (declaration.JE_ContainerMode)
					{
						case Core.Constants.ContainerModes.Containerised:
							comprehensiveDeclarationType = "C";
							break;
						case Core.Constants.ContainerModes.BreakBulk:
							comprehensiveDeclarationType = "M";
							break;
						case Core.Constants.ContainerModes.Bulk:
							comprehensiveDeclarationType = "L";
							break;
						default:
							break;
					}
				}
				return comprehensiveDeclarationType;
			}
		}

		public string DeclarantCode => entryInstruction?.CEI_DeclarantCode;

		public IJapaneseAddress ImportTrader => TryGetJobDocAddressProvider(declaration.DeclarationConsigneeAddress);

		public IWesternAddress Shipper => TryGetJobDocAddressProvider(declaration.SupplierDocumentaryAddress);

		public IJapaneseAddress InspectionWitness => TryGetInspectionWitnessProvider(declaration);

		public IEnumerable<string> BillNumbers
		{
			get
			{
				if (declaration.IsSea)
				{
					if (declaration.Bills.Count > 1)
					{
						yield return declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill).FirstOrDefault()?.CU_BillNum ?? ZString.Empty;
					}
					else
					{
						yield return declaration.Bills.Cast<Bill>().FirstOrDefault()?.CU_BillNum ?? ZString.Empty;
					}
				}
				else if (declaration.IsAir)
				{
					yield return entryInstruction?.CEI_BillNumber ?? ZString.Empty;
					yield return declaration.JE_MasterBill;
				}
			}
		}

		public IMeasurement Quantity => TryGetMeasurementProvider(entryInstruction, nameof(Quantity));

		public IMeasurement GrossWeight => TryGetMeasurementProvider(entryInstruction, nameof(GrossWeight));

		public string MarksAndNumbers => entryInstruction?.JP_MarksAndNumbers;

		public IVessel Vessel
		{
			get
			{
				List<string> vesselProviderBusinessCodes = new List<string> { JPProcedureCodeList.Codes.IDA, JPProcedureCodeList.Codes.ECR };
				if (vesselProviderBusinessCodes.Contains(sendingObject.ProcedureCode) || declaration.IsSea)
				{
					return TryGetVesselProvider(declaration, sendingObject);
				}
				return null;
			}
		}

		public DateTime? DateOfArrival => GetDateTime(declaration.JE_DateOfArrival);

		public ILocation PortOfUnloading => TryGetLocationProvider(declaration, nameof(PortOfUnloading));

		public ILocation PortOfLoading => TryGetLocationProvider(declaration, nameof(PortOfLoading));

		public ILocation PortOfOrigin => TryGetLocationProvider(declaration, nameof(PortOfOrigin));

		public string TradeType => entryInstruction?.CEI_TradeType;

		public int? ContainerCount => entryInstruction?.CEI_ContainerCount;

		public string TaxRebateType => (isImport && (entryInstruction != null) && entryInstruction.CEI_DutyDrawback == YesNoList.Codes.Yes) ? "X" : "";

		public string ImportTradeControlOrdinanceArticle3 => entryInstruction?.CEI_TradeControlOrder;

		public string ImportApprovalCertificateHasCommercialValue => entryInstruction?.CEI_CommercialValueType;

		public string ResultOfContentInspection => entryInstruction?.CEI_ContentInspectionResult;

		public string CustomsInspectionCode => entryInstruction?.CEI_CustomsInspectionCode;

		public IEnumerable<string> OtherLawCodes => entryInstruction?.OtherLaws?.Take(CusOtherLawReferenceCollection<CusOtherLawReference>.MaxRowCount).Cast<CusOtherLawReference>().Select(x => x.CFR_Reference.ToString());

		public string CommonControlNumber => entryInstruction?.CEI_CommonControlNumber;

		public string FoodHygineCertificateId => entryInstruction?.CEI_FoodHygieneCertificateType.ToString();

		public string PlantProtectionCertificateId => entryInstruction?.CEI_PlantProtectionCertificateType.ToString();

		public string AnimalQuarantineCertificateId => entryInstruction?.CEI_AnimalQuarantineCertificateType.ToString();

		public IEnumerable<IApprovalCertificate> ApprovalCertificates
		{
			get
			{
				var maxCount = isImport ? ApprovalCertificateInfoCollection.MaxCountForImport : ApprovalCertificateInfoCollection.MaxCountForExport;
				var certificates = entryInstruction?.ApprovalCertificateInfos.ToArray();
				foreach (ApprovalCertificateInfo certificate in certificates?.Take(maxCount) ?? Enumerable.Empty<ApprovalCertificateInfo>())
				{
					yield return TryGetApprovalCertificateProvider(certificate);
				}
			}
		}

		public IInvoice Invoice => TryGetInvoiceProvider(entryHeader);

		public string FreightTypeCode => entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_FreightType;

		public IMoney Freight => TryGetMoneyProvider(entryHeader, CustomsChargeTypeList.Codes.OverseasFreight);

		public string InsuranceTypeCode => entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_InsuranceType;

		public IMoney Insurance => TryGetMoneyProvider(entryHeader, CustomsChargeTypeList.Codes.OverseasInsurance);

		public string ComprehensiveInsuranceNumber => entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_ComprehensiveInsuranceNumber;

		public string ValuationType => entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_ValuationCode;

		public IEnumerable<string> ComprehensiveValuationDeclarationNumbers => entryHeader.InvoiceHeaders().FirstOrDefault()?.ComprehensiveValuations.Take(ComprehensiveValuationCollection.MaxRowCount).Cast<ComprehensiveValuation>().Select(x => x.CFR_Reference.ToString());

		public string ValuationCorrectionTypeCode => "DP";

		public IMoney ValuationCorrectionBaseAmount => null;

		public string ValuationCorrectionFormula => ZString.Empty;

		public IEnumerable<string> AdvanceRulingOnValuation
		{
			get
			{
				yield return entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_AdvanceRulingOnValuation1;
				yield return entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_AdvanceRulingOnValuation2;
			}
		}

		public decimal? TotalCustomsValueApportionmentCoefficient => null;

		public DateTime? IntoBondedAreaFirstApprovalDate
		{
			get
			{
				var invoiceLines = entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => !x.JI_BondedDate.IsEmpty);
				return invoiceLines.Any() ? invoiceLines.Min(x => GetDateTime(x.JI_BondedDate)) : null;
			}
		}

		public string BondedAreaCode => entryInstruction?.CEI_BondedLocationCode;

		public string PaymentDeadlineExtensionCode => declaration.JE_PaymentDeadlineExtension;

		public string BeforePermitApplicationReasonCode => entryInstruction?.CEI_BeforePermitApplicationReason;

		public string PaymentMethod => declaration.JE_PaymentMethod;

		public string BankAccountNumber => declaration.JE_DefermentAccountNumber;

		public IEnumerable<string> GuaranteeNumbers => entryInstruction?.Guarantees.Cast<CusGuaranteeReference>().Take(CusGuaranteeReferenceCollection.MaxRowCount).Select(x => x.CFR_Reference.ToString());

		public string CustomsRemarks => entryInstruction?.JP_CustomsNotes;

		public string CustomsBrokerRemarks => entryInstruction?.JP_BrokersNotes;

		public string OwnerRemarks => entryInstruction?.JP_OwnersNotes;

		public string OwnerSectionCode => declaration.JE_OwnerSectionCode;

		public string OwnerReferenceNumber => declaration.JE_OwnerRef;

		public string InternalReferenceNumber => entryInstruction?.CEI_DeclarationReference;

		public IJapaneseAddress Exporter => TryGetJobDocAddressProvider(declaration.SupplierDocumentaryAddress, true);

		public IWesternAddress Consignee => TryGetJobDocAddressProvider(declaration.DeclarationConsigneeAddress);

		public string ExportControlNumber => entryInstruction?.ExportControlNumber;

		public string BillNumber => declaration.IsAir ? entryInstruction?.CEI_BillNumber : string.Empty;

		public ILocation FinalDestination => TryGetLocationProvider(declaration, nameof(FinalDestination));

		public DateTime? DateOfDeparture => GetDateTime(declaration.JE_DateAtOrigin);

		public string ExportApprovalCertificateClassification => entryInstruction?.CEI_ApprovalCertificateCategory;

		public string PreInspectedCargoType => entryInstruction?.CEI_PreInspectedCargoType;

		public IMoney FOB => TryGetMoneyProvider(entryHeader, nameof(FOB));

		public decimal? TotalBasicPrice => null;

		public string RequiresShippingOrLoadingConfirmation => (entryInstruction?.CEI_LoadingConfirmationIsRequired ?? false) ? YesNoList.Codes.Yes : string.Empty;

		public IEnumerable<string> VanningLocationCodes => entryInstruction?.VanningLocations.Select(x => TryGetJobDocAddressProvider(x).Code);

		public IJapaneseAddress VanningLocation => TryGetJobDocAddressProvider(entryInstruction?.VanningLocations.FirstOrDefault() as JobDocAddress);

		IEnumerable<IImportItem> IIDAEntry.Items
		{
			get
			{
				var entryLines = entryHeader.MergedLines?.Take(99);
				foreach (var line in entryLines)
				{
					yield return TryGetImportItemProvider(line);
				}
			}
		}

		IEnumerable<IExportItem> IEDAEntry.Items
		{
			get
			{
				var entryLines = entryHeader.MergedLines?.Take(99);
				foreach (var line in entryLines)
				{
					yield return TryGetExportItemProvider(line);
				}
			}
		}

		string IEntrySubmission.DeclarationCondition => entryInstruction?.CEI_DeclarationCondition;

		string IEAC.DeclarationCondition => sendingObject.DeclarationCorrectionCopyRequest ? "P" : string.Empty;

		string IEAC.Reserved => ZString.Empty;

		public string ActionTypeCode => sendingObject.Action;

		public string NSINumber => entryInstruction?.NSI;

		public string GoodsDescription => entryInstruction?.CEI_GoodsDescription;

		public IMeasurement Weight => GrossWeight;

		public IMeasurement Volume => TryGetMeasurementProvider(entryInstruction, nameof(Volume));

		public string CarrierCode => declaration.JE_CarrierCode;

		public string VoyageNumber => declaration.JE_VoyageFlightNo;

		public DateTime? ArrivalDate
		{
			get
			{
				if (sendingObject.ProcedureCode == JPProcedureCodeList.Codes.ECR)
				{
					return GetDateTime(declaration.JE_ArrivalAtLoadingDate);
				}
				else
				{
					return GetDateTime(declaration.JE_DateOfArrival);
				}
			}
		}

		public string PortofLoadingCode => declaration.JE_RL_NKPortOfLoading;

		public DateTime? DateOfDepature => GetDateTime(declaration.JE_ExportDate);

		public string PortOfDischarge => declaration.JE_RL_NKPortOfArrival;

		public string ReceiptMode => declaration.JE_ReceiptMode;

		public string DeliveryMode => declaration.JE_DeliveryMode;

		public string Notes => entryInstruction?.JP_ECRNotes;

		public string FinalDestinationCode => declaration.JE_RL_NKFinalDestination;

		public string BookingNumber => declaration.JE_BookingNumber;

		public string DangerousGoodsCode => entryInstruction?.CEI_SpecialCargoCode;

		public IEnumerable<IMoveInDestination> MoveInDestinations => entryInstruction?.MoveInDestinationInfos.Select(TryGetMoveInDestinationProvider);

		DateTime? GetDateTime(ZDateTime time) => time.IsValid ? time.ToDateTime() : null;

		AttorneyForCustomsProceduresProvider TryGetAttorneyForCustomsProceduresProvider(JobDeclaration jobDeclaration) => jobDeclaration != null && jobDeclaration.IsInDatabase ? new AttorneyForCustomsProceduresProvider(jobDeclaration) : null;

		InspectionWitnessProvider TryGetInspectionWitnessProvider(JobDeclaration jobDeclaration) => jobDeclaration != null && jobDeclaration.IsInDatabase ? new InspectionWitnessProvider(jobDeclaration) : null;

		JobDocAddressProvider TryGetJobDocAddressProvider(JobDocAddress jobDocAddress, bool trimPhoneNumberIfNeeded = false) => jobDocAddress != null && jobDocAddress.IsInDatabase ? new JobDocAddressProvider(jobDocAddress, trimPhoneNumberIfNeeded) : null;

		MeasurementProvider TryGetMeasurementProvider(CusEntryInstruction entryInstruction, string type) => entryInstruction != null && entryInstruction.IsInDatabase ? new MeasurementProvider(entryInstruction, type) : null;

		LocationProvider TryGetLocationProvider(JobDeclaration jobDeclaration, string type) => jobDeclaration != null && jobDeclaration.IsInDatabase ? new LocationProvider(jobDeclaration, type) : null;

		ApprovalCertificateProvider TryGetApprovalCertificateProvider(ApprovalCertificateInfo certificate) => certificate != null && certificate.IsInDatabase ? new ApprovalCertificateProvider(certificate) : null;

		InvoiceProvider TryGetInvoiceProvider(CusEntryHeader entryHeader) => entryHeader != null && entryHeader.IsInDatabase ? new InvoiceProvider(entryHeader) : null;

		VesselProvider TryGetVesselProvider(JobDeclaration jobDeclaration, MessageSendingObject sendingObject) => jobDeclaration != null && jobDeclaration.IsInDatabase ? new VesselProvider(jobDeclaration, sendingObject) : null;

		MoneyProvider TryGetMoneyProvider(CusEntryHeader entryHeader, ZString code) => entryHeader != null && entryHeader.IsInDatabase ? new MoneyProvider(entryHeader, code) : null;

		ImportItemProvider TryGetImportItemProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new ImportItemProvider(entryLine) : null;

		ExportItemProvider TryGetExportItemProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new ExportItemProvider(entryLine) : null;

		MoveInDestinationProvider TryGetMoveInDestinationProvider(MoveInDestination moveInDestination) => moveInDestination != null && moveInDestination.IsInDatabase ? new MoveInDestinationProvider(moveInDestination) : null;
	}
}
