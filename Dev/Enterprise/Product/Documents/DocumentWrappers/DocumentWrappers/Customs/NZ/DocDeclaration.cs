using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.NZ.Business.Declaration.CusEntryLine;
using CusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.NZ.Business.Declaration.CusEntryLine>;
using InvoiceHeaderActiveCollection = Enterprise.Customs.NZ.Business.Declaration.InvoiceHeaderActiveCollection;
using InvoiceLineCompleteCollection = Enterprise.Customs.NZ.Business.Declaration.InvoiceLineCompleteCollection;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		public new static class SDFields
		{
			public static class InsuranceCertificate
			{
				public const string MarineCertificateNumber = "Marine Certificate Number";
				public const string ShipperReference = "Shipper Reference";
				public const string MarinePolicyNumber = "Marine Policy Number";
				public const string ClaimsPayableInCurrency = "Claims payable in (Currency)";
				public const string InsuredWith = "Insured for Invoice Value with";
				public const string SubjectTo = "Subject To";
				public const string SubjectOnlyToConditions = "Subject only to Conditions";
				public const string PlaceandDateOfIssue = "Place and Date of Issue";
			}

			public static class DocumentOfOrigin
			{
				public const string NoOfDeliverance = "No Of Deliverance";
				public const string CNCode = "CN Code";
				public const string DutyRate = "Duty Rate";
				public const string CarcaseMassInKG = "Carcase Mass (kg)";
				public const string Place = "Document of Origin Place";
				public const string Date = "Document of Origin Date";
				public const string ExpiryDate = "Expiry Date";
			}

			public static class SanitaryCertificate
			{
				public const string ColdStore = "Cold Store";
				public const string ProcessedAt = "Processed at";
				public const string Species = "Species";
				public const string SlaughteredAt = "Slaughtered At";
				public const string DoneAt = "Sanitary Certificate Done At";
				public const string DoneOn = "Sanitary Certificate Done On";
				public const string OfficialVeterinarian = "Official Veterinarian";
			}

			public static class AUPrefCertOfOrigin
			{
				public const string PackingCostsIncluded = "Packing Costs Incl?";
				public const string PackingCostsAmount = "Packing Costs Amount";
				public const string PrepaidOverseasFreightIncluded = "Prepaid Overseas Freight Incl?";
				public const string PrepaidOverseasFreightAmount = "Prepaid Overseas Freight Amount";
				public const string PrepaidDomesticFreightIncluded = "Prepaid Domestic Freight Incl?";
				public const string PrepaidDomesticFreightAmount = "Prepaid Domestic Freight Amount";
				public const string PrepaidInsuranceIncluded = "Prepaid Insurance Incl?";
				public const string PrepaidInsuranceAmount = "Prepaid Insurance Amount";
				public const string OtherPrepaidCostsDescription = "Other Prepaid Costs Description";
				public const string OtherPrepaidCostsIncluded = "Other Prepaid Costs Incl?";
				public const string OtherPrepaidCostsAmount = "Other Prepaid Costs Amount";
			}
		}

		protected DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
			SetupFlattenedListProperties();
		}

		public static DocDeclaration New(JobDeclaration declaration, BusinessObjectFactory factoryToWrap)
		{
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				return overridden(declaration, factoryToWrap);
			}
			else
				if (declaration == null)
			{
				return null;
			}
			else
			{
				return new DocDeclaration(declaration, factoryToWrap);
			}
		}

		#region ZString Fields
		public ZDateTime BarrierDate
		{
			get { return Declaration.BarrierDate; }
		}

		public ZString EntryStyle
		{
			get { return Declaration.EntryStyle; }
		}

		public ZString CustomsDeliveryInstructions
		{
			get { return Declaration.CustomsDeliveryInstructions; }
		}

		public ZString CustomsDeliveryInstructionsWithITR => Declaration.ConsolidatedDec != null ? Declaration.CustomsDeliveryInstructionsForConsolidatedDO : Declaration.CustomsDeliveryInstructionsWithITR;

		public ZString Agent
		{
			get { return (Declaration.Branch != null && Declaration.Branch.OrgProxy != null) ? Declaration.Branch.OrgProxy.OH_FullName : ZString.Empty; }
		}

		public ZString AgentCustomsCode
		{
			get { return NZCustomsDataRegistry.Instance.NZBrokerageID.Value.ToUpperInvariant(); }
		}

		public ZString VoyageNo
		{
			get { return (TransportMode == Core.Constants.TransportModes.Sea) ? VoyageFlightNo : ZString.Empty; }
		}

		public ZString CraftFlight
		{
			get
			{
				ZString result = ZString.Empty;

				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Sea:
						if (Vessel != null)
						{
							result = Vessel.ToString();
						}

						break;
					case Core.Constants.TransportModes.Air:
						result = VoyageFlightNo;
						break;
				}

				return result;
			}
		}

		public ZString CraftName
		{
			get
			{
				ZString result = ZString.Empty;
				if (TransportMode == Core.Constants.TransportModes.Sea && Vessel != null)
				{
					result = Vessel.ToString();
				}

				return result;
			}
		}

		public ZString VoyageFlight
		{
			get
			{
				return VoyageFlightNo;
			}
		}

		public ZString IntendedShipmentMonth
		{
			get { return Declaration.JE_ExportDate.ToString("MMMM"); }
		}
		#endregion

		#region MiscellaneousDescriptions Collection

		public MiscellaneousDescriptionsCollection MiscellaneousDescriptions
		{
			get
			{
				if (fMiscellaneousDescriptions == null)
				{
					fMiscellaneousDescriptions = new MiscellaneousDescriptionsCollection(this);
					fMiscellaneousDescriptions.Load();
				}
				return fMiscellaneousDescriptions;
			}
		}
		MiscellaneousDescriptionsCollection fMiscellaneousDescriptions;

		#endregion

		#region DocumentNote properties
		#region General Fields
		public ZString FlightOrVoyage
		{
			get
			{
				ZString result = VoyageFlightDetails;
				if (TransportMode == Core.Constants.TransportModes.Air)
				{
					result += "/" + DateOfArrival.ToShortDateString();
				}
				return result;
			}
		}

		public ZString CasperCode
		{
			get { return (Supplier != null) ? Supplier.CCD : ZString.Empty; }
		}

		public ZString GSTRegNo
		{
			get { return (Supplier != null) ? Supplier.GST : ZString.Empty; }
		}
		#endregion

		#region Insurance Certificate
		public ZString MarineCertificate
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.MarineCertificateNumber); }
		}

		public ZString ShipperReference
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.ShipperReference); }
		}

		public ZString MarinePolicyNumber
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.MarinePolicyNumber); }
		}

		public ZString PaymentCurrency
		{
			get
			{
				ZString currencyCode = Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.ClaimsPayableInCurrency);
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				return (currency != null) ? new ZString(currency.RX_Code + "-" + currency.RX_DescMultilingual) : currencyCode;
			}
		}

		public ZString InsuredWith
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.InsuredWith); }
		}

		public ZString SubjectTo
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.SubjectTo); }
		}

		public ZString SubjectToConditions
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.SubjectOnlyToConditions); }
		}

		public ZString PlaceOfIssue
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuranceCertificate.PlaceandDateOfIssue); }
		}
		#endregion

		#region Document of Origin
		public ZString NoOfDeliverance
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.NoOfDeliverance); }
		}

		public ZString CNCode
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.CNCode); }
		}

		public ZString DutyRate
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.DutyRate); }
		}

		public ZString CarcaseMass
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.CarcaseMassInKG); }
		}

		public ZString DocOriginPlace
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.Place); }
		}

		public ZString DocOriginDate
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.Date); }
		}

		public ZString DocOriginExpiryDate
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentOfOrigin.ExpiryDate); }
		}
		#endregion

		#region Sanitary Certificate
		public ZString ColdStore
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.ColdStore); }
		}

		public ZString ProcessedAt
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.ProcessedAt); }
		}

		public ZString Species
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.Species); }
		}

		public ZString SlaughteredAt
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.SlaughteredAt); }
		}

		public ZString DoneAt
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.DoneAt); }
		}

		public ZString DoneOn
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.DoneOn); }
		}

		public ZString OfficialVeterinarian
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.SanitaryCertificate.OfficialVeterinarian); }
		}
		#endregion

		#region AUPrefCertOfOrigin Fields
		public ZString PackingCostsIncluded
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PackingCostsIncluded); }
		}

		public ZString PackingCostsAmount
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PackingCostsAmount); }
		}

		public ZString PrepaidOverseasFreightIncluded
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidOverseasFreightIncluded); }
		}

		public ZString PrepaidOverseasFreightAmount
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidOverseasFreightAmount); }
		}

		public ZString PrepaidDomesticFreightIncluded
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidDomesticFreightIncluded); }
		}

		public ZString PrepaidDomesticFreightAmount
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidDomesticFreightAmount); }
		}

		public ZString PrepaidInsuranceIncluded
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidInsuranceIncluded); }
		}

		public ZString PrepaidInsuranceAmount
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.PrepaidInsuranceAmount); }
		}

		public ZString OtherPrepaidCostsDescription
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsDescription); }
		}

		public ZString OtherPrepaidCostsIncluded
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsIncluded); }
		}

		public ZString OtherPrepaidCostsAmount
		{
			get { return Declaration.DocNote.GetSystemDefinedFieldValue(SDFields.AUPrefCertOfOrigin.OtherPrepaidCostsAmount); }
		}
		#endregion
		#endregion

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)WrappedObject; }
		}

		protected delegate DocDeclaration NewDelegate(JobDeclaration declaration, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Child DocumentWrapperCollections
		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection((CusContainerCollection)collectionToWrap, Factory);
		}

		public DocCusContainerCollection Containers
		{
			get { return (DocCusContainerCollection)ContainersInternal; }
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		DocContainerAndPackageInfoCollection fDocContainerAndPackageInfos;
		public DocContainerAndPackageInfoCollection DocContainerAndPackageInfos
		{
			get
			{
				if (fDocContainerAndPackageInfos == null)
				{
					fDocContainerAndPackageInfos = GetNewDocContainerAndPackageInfoCollection();
					fDocContainerAndPackageInfos.Load();
				}
				return fDocContainerAndPackageInfos;
			}
		}

		protected DocContainerAndPackageInfoCollection GetNewDocContainerAndPackageInfoCollection()
		{
			return new DocContainerAndPackageInfoCollection(Declaration);
		}

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		public DocBaseJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get { return InvoiceHeadersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection FSAInvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)FSAInvoiceLinesLinesInternal; }
		}

		public DocJobComInvoiceLineCollection FSAInvoiceLinesDocumentLineOne
		{
			get { return (DocJobComInvoiceLineCollection)FSAInvoiceLinesLineOne; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateFSAJobComInvoiceLineLineOneCollection(Enterprise.Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			var result = new DocJobComInvoiceLineCollection(Factory);
			var allInvoiceLines = new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
			foreach (DocJobComInvoiceLine invLine in allInvoiceLines)
			{
				if (invLine.RequiresPermitCodes)
				{
					result.Add(invLine);
					if (result.Count > 0)
					{
						break;
					}
				}
			}

			return result;
		}

		protected override DocBaseJobComInvoiceLineCollection CreateFSAJobComInvoiceLineCollection(Enterprise.Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			var result = new DocJobComInvoiceLineCollection(Factory);
			var allInvoiceLines = new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
			bool firstPermitLine = true;
			foreach (DocJobComInvoiceLine invLine in allInvoiceLines)
			{
				if (invLine.RequiresPermitCodes)
				{
					if (firstPermitLine)
					{
						firstPermitLine = false;
					}
					else
					{
						result.Add(invLine);
					}
				}
			}

			return result;
		}
		#endregion

		#region Formal Entry Stuff
		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Enterprise.Customs.Business.CusEntryHeader> collectionToWrap)
		{
			if (Declaration.IsECIWriteoff)
			{
				throw new NotSupportedException();
			}
			else
			{
				return new FormalEntry.DocCusEntryHeaderCollection(collectionToWrap, Factory);
			}
		}

		#region EntryHeader
		public FormalEntry.DocCusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					CusEntryHeader entryHeader = Declaration.CusEntryHeader as CusEntryHeader;
					if (entryHeader != null)
					{
						fEntryHeader = FormalEntry.DocCusEntryHeader.New((CusEntryHeader)Declaration.CusEntryHeader, Factory);
					}
				}
				return fEntryHeader;
			}
		}
		FormalEntry.DocCusEntryHeader fEntryHeader;
		#endregion

		#region EntryLines
		public FormalEntry.DocCusEntryLineCollection EntryLines
		{
			get
			{
				if (fEntryLines == null)
				{
					CusEntryHeader entryHeader = Declaration.CusEntryHeader as CusEntryHeader;
					if (entryHeader != null)
					{
						CusEntryLineCollection entryLines = entryHeader.MergedLines;
						entryLines.Sort(CusEntryLine.Schema.CL_LineNumber, System.ComponentModel.ListSortDirection.Ascending);
						fEntryLines = new FormalEntry.DocCusEntryLineCollection(entryLines, Factory);
					}
					else
					{
						fEntryLines = new FormalEntry.DocCusEntryLineCollection(Factory);
					}
				}
				return fEntryLines;
			}
		}
		FormalEntry.DocCusEntryLineCollection fEntryLines;
		#endregion

		#region EntryLinesForEntryPrint
		public FormalEntry.DocCusEntryLineCollectionPaddedForEntryPrint EntryLinesForEntryPrint
		{
			get
			{
				if (fEntryLinesForEntryPrint == null)
				{
					CusEntryHeader entryHeader = Declaration.CusEntryHeader as CusEntryHeader;
					if (entryHeader != null)
					{
						CusEntryLineCollection entryLines = entryHeader.MergedLines;
						entryLines.Sort(CusEntryLine.Schema.CL_LineNumber, System.ComponentModel.ListSortDirection.Ascending);
						fEntryLinesForEntryPrint = new FormalEntry.DocCusEntryLineCollectionPaddedForEntryPrint(entryLines, Factory);
					}
					else
					{
						fEntryLinesForEntryPrint = new FormalEntry.DocCusEntryLineCollectionPaddedForEntryPrint(Factory);
					}
				}
				return fEntryLinesForEntryPrint;
			}
		}
		FormalEntry.DocCusEntryLineCollectionPaddedForEntryPrint fEntryLinesForEntryPrint;
		#endregion

		public ZInt TotalFSALines
		{
			get
			{
				ZInt result = 0;
				CusEntryHeader entryHeader = Declaration.CusEntryHeader as CusEntryHeader;
				if (entryHeader != null)
				{
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						if (RequiresPermitCodes(entryLine))
						{
							result++;
						}
					}
				}

				return result;
			}
		}

		protected ZBool RequiresPermitCodes(CusEntryLine entryLine)
		{
			bool requiresPermits = NZCTariffsPermitsApplyTo.Load(Factory, entryLine.CL_AdValoremTariff, true, false) != null;
			return requiresPermits && entryLine.PermitCodes.Count == 0;
		}

		public ZBool ExtraFSALineSectionsRequired
		{
			get
			{
				ZInt fsaLinesCount = 0;
				CusEntryHeader entryHeader = Declaration.CusEntryHeader as CusEntryHeader;
				if (entryHeader != null)
				{
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						if (RequiresPermitCodes(entryLine))
						{
							fsaLinesCount++;
						}

						if (fsaLinesCount > 1)
						{
							break;
						}
					}
				}

				return fsaLinesCount > 1;
			}
		}

		#endregion

		#region ECI WriteOff Stuff
		#region TotalInvoiceAmount
		public override ZDecimal TotalInvoiceAmount
		{
			get { return Declaration.IsECIWriteoff ? Declaration.JE_ECI_InvoiceAmount : base.TotalInvoiceAmount; }
		}
		#endregion

		#region TotalInvoiceCurrency
		public override DocCurrency TotalInvoiceCurrency
		{
			get
			{
				if (Declaration.IsECIWriteoff)
				{
					if (fTotalInvoiceCurrency == null)
					{
						fTotalInvoiceCurrency = DocCurrency.New(Declaration.ECI_InvoiceCurrency, Factory);
					}
					return fTotalInvoiceCurrency;
				}
				else
				{
					return base.TotalInvoiceCurrency;
				}
			}
		}
		#endregion
		#endregion

		#region Flattened Collection Properties used on Entry Print as Documents don't handle indexers
		void SetupFlattenedListProperties()
		{
			SetupFlattenedInvoiceProperties();
			SetupFlattenedPermitProperties();
			SetupFlattenedOtherInfoProperties();
		}

		#region OtherInfo
		void SetupFlattenedOtherInfoProperties()
		{
			if (Declaration.OtherInfos.Count > 0)
			{
				fOtherInfoCode1 = Declaration.OtherInfos[0].ZO_Code;
				fOtherInfoData1 = Declaration.OtherInfos[0].ZO_Data;
			}
			if (Declaration.OtherInfos.Count > 1)
			{
				fOtherInfoCode2 = Declaration.OtherInfos[1].ZO_Code;
				fOtherInfoData2 = Declaration.OtherInfos[1].ZO_Data;
			}
			if (Declaration.OtherInfos.Count > 2)
			{
				fOtherInfoCode3 = Declaration.OtherInfos[2].ZO_Code;
				fOtherInfoData3 = Declaration.OtherInfos[2].ZO_Data;
			}
		}

		public ZString OtherInfoCode1
		{
			get { return fOtherInfoCode1; }
		}
		ZString fOtherInfoCode1;

		public ZString OtherInfoData1
		{
			get { return fOtherInfoData1; }
		}
		ZString fOtherInfoData1;

		public ZString OtherInfoCode2
		{
			get { return fOtherInfoCode2; }
		}
		ZString fOtherInfoCode2;

		public ZString OtherInfoData2
		{
			get { return fOtherInfoData2; }
		}
		ZString fOtherInfoData2;

		public ZString OtherInfoCode3
		{
			get { return fOtherInfoCode3; }
		}
		ZString fOtherInfoCode3;

		public ZString OtherInfoData3
		{
			get { return fOtherInfoData3; }
		}
		ZString fOtherInfoData3;
		#endregion

		#region Permit Information
		void SetupFlattenedPermitProperties()
		{
			if (Declaration.PermitCodes.Count > 0)
			{
				fPermitCode1 = Declaration.PermitCodes[0].ZO_Code;
				fPermitNumber1 = Declaration.PermitCodes[0].ZO_Data;
			}
			if (Declaration.PermitCodes.Count > 1)
			{
				fPermitCode2 = Declaration.PermitCodes[1].ZO_Code;
				fPermitNumber2 = Declaration.PermitCodes[1].ZO_Data;
			}
			if (Declaration.PermitCodes.Count > 2)
			{
				fPermitCode3 = Declaration.PermitCodes[2].ZO_Code;
				fPermitNumber3 = Declaration.PermitCodes[2].ZO_Data;
			}
		}

		public ZString PermitCode1
		{
			get { return fPermitCode1; }
		}
		ZString fPermitCode1;

		public ZString PermitCode2
		{
			get { return fPermitCode2; }
		}
		ZString fPermitCode2;

		public ZString PermitCode3
		{
			get { return fPermitCode3; }
		}
		ZString fPermitCode3;

		public ZString PermitNumber1
		{
			get { return fPermitNumber1; }
		}
		ZString fPermitNumber1;

		public ZString PermitNumber2
		{
			get { return fPermitNumber2; }
		}
		ZString fPermitNumber2;

		public ZString PermitNumber3
		{
			get { return fPermitNumber3; }
		}
		ZString fPermitNumber3;
		#endregion

		#region Invoice Properties
		void SetupFlattenedInvoiceProperties()
		{
			if (Declaration.Invoices.Count > 0)
			{
				fInvoiceNumber1 = Declaration.Invoices[0].JZ_InvoiceNumber;
				fInvoiceIncoTerm1 = Declaration.Invoices[0].JZ_IncoTerm;
			}
			if (Declaration.Invoices.Count > 1)
			{
				fInvoiceNumber2 = Declaration.Invoices[1].JZ_InvoiceNumber;
				fInvoiceIncoTerm2 = Declaration.Invoices[1].JZ_IncoTerm;
			}
			if (Declaration.Invoices.Count > 2)
			{
				fInvoiceNumber3 = Declaration.Invoices[2].JZ_InvoiceNumber;
				fInvoiceIncoTerm3 = Declaration.Invoices[2].JZ_IncoTerm;
			}
			if (Declaration.Invoices.Count > 3)
			{
				fInvoiceNumber4 = Declaration.Invoices[3].JZ_InvoiceNumber;
				fInvoiceIncoTerm4 = Declaration.Invoices[3].JZ_IncoTerm;
			}
		}

		public ZString InvoiceNumber1
		{
			get { return fInvoiceNumber1; }
		}
		ZString fInvoiceNumber1;

		public ZString InvoiceNumber2
		{
			get { return fInvoiceNumber2; }
		}
		ZString fInvoiceNumber2;

		public ZString InvoiceNumber3
		{
			get { return fInvoiceNumber3; }
		}
		ZString fInvoiceNumber3;

		public ZString InvoiceNumber4
		{
			get { return fInvoiceNumber4; }
		}
		ZString fInvoiceNumber4;

		public ZString InvoiceIncoTerm1
		{
			get { return fInvoiceIncoTerm1; }
		}
		ZString fInvoiceIncoTerm1;

		public ZString InvoiceIncoTerm2
		{
			get { return fInvoiceIncoTerm2; }
		}
		ZString fInvoiceIncoTerm2;

		public ZString InvoiceIncoTerm3
		{
			get { return fInvoiceIncoTerm3; }
		}
		ZString fInvoiceIncoTerm3;

		public ZString InvoiceIncoTerm4
		{
			get { return fInvoiceIncoTerm4; }
		}
		ZString fInvoiceIncoTerm4;
		#endregion

		#region DocContainerAndPackageInfos Single Element Accessors
		public DocContainerAndPackageInfo DocContainerAndPackageInfo1
		{
			get { return (DocContainerAndPackageInfos.Count > 0) ? DocContainerAndPackageInfos[0] : null; }
		}

		public DocContainerAndPackageInfo DocContainerAndPackageInfo2
		{
			get { return (DocContainerAndPackageInfos.Count > 1) ? DocContainerAndPackageInfos[1] : null; }
		}

		public DocContainerAndPackageInfo DocContainerAndPackageInfo3
		{
			get { return (DocContainerAndPackageInfos.Count > 2) ? DocContainerAndPackageInfos[2] : null; }
		}

		public DocContainerAndPackageInfo DocContainerAndPackageInfo4
		{
			get { return (DocContainerAndPackageInfos.Count > 3) ? DocContainerAndPackageInfos[3] : null; }
		}

		#endregion

		public DocContainerAndPackageInfoCollection DocContainerAndPackageOverflow
		{
			get
			{
				if (fDocContainerAndPackageOverflow == null)
				{
					fDocContainerAndPackageOverflow = new DocContainerAndPackageInfoCollection(Declaration);
					if (DocContainerAndPackageInfos.Count > 4)
					{
						for (int i = 4; i < DocContainerAndPackageInfos.Count; i++)
						{
							fDocContainerAndPackageOverflow.Add(DocContainerAndPackageInfos[i]);
						}
					}
				}
				return fDocContainerAndPackageOverflow;
			}
		}
		DocContainerAndPackageInfoCollection fDocContainerAndPackageOverflow;

		public ZBool HasOverflowContainersAndPackaging
		{
			get { return DocContainerAndPackageOverflow.Count > 0; }
		}

		#endregion

		#region New Properties for Entry Print
		public ZString DepositRefund
		{
			get { return ""; }
		}

		public ZString OriginalEntryNumber
		{
			get { return Declaration.JE_OriginalEntryNumber; }
		}

		public override ZString PaymentMethod
		{
			get { return Declaration.PaymentMethod; }
		}

		public ZString CountryOfDestination
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration.IsTSWDeclaration)
				{
					if (Declaration.DeliveryDestinationPartyDocAddress != null && Declaration.DeliveryDestinationPartyDocAddress.Country != null)
					{
						result = Declaration.DeliveryDestinationPartyDocAddress.Country.Code + " - " + Declaration.DeliveryDestinationPartyDocAddress.Country.Description;
					}
					else if (Declaration.Importer != null && Declaration.Importer.Country != null)
					{
						result = Declaration.Importer.Country.Code + " - " + Declaration.Importer.Country.Description;
					}
				}

				if (result.IsEmpty && FinalDestination != null)
				{
					result = FinalDestination.CountryCodeAndName;
				}

				return result;
			}
		}

		public ZString ProcessIndicatorSoldConsigned
		{
			get
			{
				return Declaration.IsTSWDeclaration ? Declaration.JE_TransactionNature.ToString() : Declaration.Lookups.JE_SoldOrConsigned_List.GetDescriptionFromCode(Declaration.JE_SoldOrConsigned);
			}
		}

		public ZString TransactionTypeLabel
		{
			get { return Declaration.IsTSWDeclaration ? "Nature of Transaction" : "Process Indicator  Sold/Consignment"; }
		}

		public ZString NatureOfTransaction
		{
			get { return Declaration.IsTSWDeclaration ? Declaration.JE_TransactionNature : ZString.Empty; }
		}

		public new ZDecimal TotalWeightInKG
		{
			get { return NZWeightHelper.ApplyWeightRounding(Declaration.JE_DeclaredWeight); }   // This value is always in KGM in NZ dec
		}

		protected override GlbStaff DeclarantDelegate
		{
			get { return Declaration.LastBrokerToSubmitOrBrokerSelectedOrCurrentUser; }
		}

		public ZString DeclarantCustomsCode
		{
			get { return NZCustomsDataRegistry.Instance.HideDeclarantCodeOnCustomsDocumentation.Value ? ZString.Empty : Declarant.GetNZWrapper().NZBPassword.GP_UserID; }
		}

		public ZString DeclarantName
		{
			get { return Declarant.GS_FullName; }
		}

		public ZString DeclarantNameAndCode
		{
			get { return DeclarantName + (DeclarantCustomsCode.IsEmpty ? "" : " (" + DeclarantCustomsCode + ")"); }
		}

		bool IsDeclarant
		{
			get { return !CurrentUser.BrokerID.IsEmpty; }
		}

		public ZString DeclarantNameIfBroker
		{
			get
			{
				var result = ZString.Empty;
				if (IsDeclarant && !NZCustomsDataRegistry.Instance.HideDeclarantCodeOnCustomsDocumentation.Value)
				{
					result = CurrentUser.FullName;
				}

				return result;
			}
		}

		public ZString DeclarantCodeIfBroker
		{
			get
			{
				var result = ZString.Empty;
				if (IsDeclarant && !NZCustomsDataRegistry.Instance.HideDeclarantCodeOnCustomsDocumentation.Value)
				{
					result = CurrentUser.BrokerID;
				}

				return result;
			}
		}

		public ZString CustomsEntryRemarks
		{
			get { return Declaration.CustomsMessageRemarks; }
		}

		public DocOrganisation DeliveryAuthority
		{
			get
			{
				if (fDeliveryAuthority == null)
				{
					fDeliveryAuthority = DocOrganisation.New(Declaration.NotifyParty, Factory);
				}
				return fDeliveryAuthority;
			}
		}
		DocOrganisation fDeliveryAuthority;

		public DocDocAddress CustomsControlledArea
		{
			get { return base.WarehouseAddress; }
		}
		#endregion

		#region New Properties for Customs Certificate
		public DocOrganisation Client
		{
			get { return (Declaration.IsExport) ? Supplier : Importer; }
		}

		public ZString EntryTypeAndStyle
		{
			get { return Declaration.EntryTypeAndStyle; }
		}

		public ZString EntryNumberAndDate
		{
			get
			{
				ZDateTime entryDate = this.EntryDate;
				return DeclarationNumber + " / " + (entryDate.IsEmpty ? "(none)" : entryDate.ToShortDateString());
			}
		}

		public ZDateTime EntryDate
		{
			get
			{
				var lastMessage = Declaration.CusEntryHeader.Messages.LastMessage;
				return lastMessage != null && lastMessage.EM_SystemCreateTimeUtc.IsValid
						? Env.Time.GetLocalTimeFromUtc(lastMessage.EM_SystemCreateTimeUtc.ToDateTime()) : ZDateTime.Empty;
			}
		}

		public ZString ClientReference
		{
			get { return Declaration.JE_DeclarationReference + (Declaration.JE_OwnerRef.IsEmpty ? "" : " / " + Declaration.JE_OwnerRef); }
		}

		public ZString WeightIncludingUnit
		{
			get
			{
				var totalWeight = NZWeightHelper.ApplyWeightRounding(TotalWeight);
				return totalWeight.IsEmpty ? "" : totalWeight + " " + WeightUQ;
			}
		}

		public ZString VolumeIncludingUnit
		{
			get { return TotalVolume.IsEmpty ? "" : TotalVolume.ToString(3) + " " + VolumeUQ; }
		}

		public ZString DrawbackIfDrawbackApplicable
		{
			get { return Declaration.IsExport ? "Drawback" : ""; }
		}
		#endregion

		#region MAF Cover Sheet DocWrapper

		public DocMAFCoverSheet MAFCS
		{
			get { return mafcs ?? (mafcs = new DocMAFCoverSheet(Factory, Declaration.Factory.GetValue<NZDocsMAFCoverSheet>() ?? new NZDocsMAFCoverSheet(Declaration))); }
		}
		DocMAFCoverSheet mafcs;

		#endregion

		public ZString ImporterClientCode
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration != null && Importer != null)
				{
					result = Declaration.IsExport ? Importer.CSC : Importer.CCD;
				}

				return result;
			}
		}

		protected override DocOrganisation GetSupplier()
		{
			OrgHeader supplier = Declaration.Supplier;
			if (supplier != null && supplier.IsMiscellaneous && Declaration.AllowsMiscSupplierAndImporter)
			{
				supplier = MiscSupplier;
			}
			return DocOrganisation.New(supplier, Factory);
		}

		OrgHeader fMiscSupplier;
		OrgHeader MiscSupplier
		{
			get
			{
				if (fMiscSupplier == null)
				{
					fMiscSupplier = new BusinessObjectFactory().New<OrgHeader>();
					fMiscSupplier.OH_FullName = Declaration.MiscSupplierName.Left(OrgHeader.Schema.OH_FullNameMaxLength);
					fMiscSupplier.OH_Code = "MISC";
				}
				return fMiscSupplier;
			}
		}

		protected override DocOrganisation GetImporter()
		{
			OrgHeader importer = Declaration.Importer;
			if (importer != null && importer.IsMiscellaneous && Declaration.AllowsMiscSupplierAndImporter)
			{
				importer = MiscImporter;
			}
			return DocOrganisation.New(importer, Factory);
		}

		OrgHeader fMiscImporter;
		OrgHeader MiscImporter
		{
			get
			{
				if (fMiscImporter == null)
				{
					fMiscImporter = new BusinessObjectFactory().New<OrgHeader>();
					fMiscImporter.OH_FullName = Declaration.MiscImporterName.Left(OrgHeader.Schema.OH_FullNameMaxLength);
					fMiscImporter.OH_Code = "MISC";
				}
				return fMiscImporter;
			}
		}

		#region HealthClearanceDocument
		public DocAddress DefaultHoldingPremises
		{
			get
			{
				if (defaultHoldingPremises == null)
				{
					defaultHoldingPremises = DocAddress.New(ImporterATFAddress ?? Declaration.DepotDocAddress.Address, Factory);
				}
				return defaultHoldingPremises;
			}
		}
		DocAddress defaultHoldingPremises;

		OrgAddress ImporterATFAddress
		{
			get
			{
				OrgHeader importer = Declaration.Importer;
				if (importer != null)
				{
					foreach (OrgCusCode customsCode in importer.CustomsCodes)
					{
						if (customsCode.OK_CodeType == OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility)
						{
							if (customsCode.OK_CustomsRegNo == Declaration.JE_ATFOtherInfoValue)
							{
								return customsCode.PremisesAddress;
							}
						}
					}
				}

				return null;
			}
		}

		public ZString LocalCustomsClientCode
		{
			get { return GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode; }
		}

		public ZString LocalCustomsDeclarantCode
		{
			get { return LocalCustomsClientCode; }
		}

		#endregion

	}
}
