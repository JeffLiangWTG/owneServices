using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class CertificateRequestDataWrapper : ICertificateRequestData
	{
		public CertificateRequestDataWrapper(QuarantineExDocHeader quarantineExDocHeader)
		{
			Argument.NotNull(quarantineExDocHeader, "quarantineExDocHeader");
			Argument.NotNull(quarantineExDocHeader.InvoiceHeader, "quarantineExDocHeader.InvoiceHeader");
			Argument.NotNull(quarantineExDocHeader.InvoiceHeader.JobDeclaration, "quarantineExDocHeader.InvoiceHeader.JobDeclaration");

			this.quarantineExDocHeader = quarantineExDocHeader;
			invoiceHeader = quarantineExDocHeader.InvoiceHeader;
			jobDeclaration = invoiceHeader.JobDeclaration;
		}

		#region ICertificateRequestData Members

		public BusinessObject Object
		{
			get { return quarantineExDocHeader; }
		}

		public ZString CommodityType
		{
			get { return EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(quarantineExDocHeader.QH_ProduceType); }
		}

		public ZString DischargePort
		{
			get { return jobDeclaration.PortOfArrival != null ? jobDeclaration.PortOfArrival.Code : ZString.Empty; }
		}

		public ZString DestinationCity
		{
			get { return jobDeclaration.FinalDestination != null ? jobDeclaration.FinalDestination.Description : ZString.Empty; }
		}

		public ZString CertificateRequiredLocation
		{
			get { return quarantineExDocHeader.QH_CertificateRequiredLocation; }
		}

		public ZString ExporterCertificateReference
		{
			get
			{
				return jobDeclaration.JE_UseOwnerRefAsQuarantineRef && !jobDeclaration.JE_OwnerRef.IsEmpty
						? jobDeclaration.JE_OwnerRef
						: (ZString)EDIMessage.SendersReferencePlaceHolder;
			}
		}

		public ZString NotifyPartyText
		{
			get { return jobDeclaration.EXDOCNotifyText; }
		}

		public ZString LetterOfCreditText
		{
			get { return jobDeclaration.EXDOCLetterOfCredit; }
		}

		public ZBool SeparateCertificateContainerInd
		{
			get { return quarantineExDocHeader.QH_SplitHealthCertByContainer; }
		}

		public ZBool SeparateCertificateMarksInd
		{
			get { return quarantineExDocHeader.QH_SplitHealthCertByMarks; }
		}

		public ZBool SeparateCertificatePackerInd
		{
			get { return quarantineExDocHeader.QH_SplitHealthCertByPacker; }
		}

		public IEnumerable<IImportPermit> ImportPermits
		{
			get
			{
				var includedPermits = new List<ZString>();
				foreach (JobComInvoiceLine line in invoiceHeader.JobComInvoiceLines)
				{
					if (!line.JI_TempImportNum.IsEmpty && !includedPermits.Contains(line.JI_TempImportNum))
					{
						includedPermits.Add(line.JI_TempImportNum);
						yield return new ImportPermitWrapper(line);
					}
				}
			}
		}

		public ZString OwnerExporterNumber
		{
			get { return invoiceHeader.EXDOCExporterNumber; }
		}

		public OrgHeader Consignee
		{
			get { return jobDeclaration.Consignee; }
		}

		public OrgHeader Forwarder
		{
			get { return jobDeclaration.Forwarder; }
		}

		public ZString TransportMode
		{
			get { return EXDOCTransportModeCodes.GetEXDOCCodeFromTransportModeCode(jobDeclaration.TransportMode); }
		}

		public ZString VoyageFlightNumber
		{
			get { return jobDeclaration.JE_VoyageFlightNo; }
		}

		public ZString CarrierName
		{
			get { return jobDeclaration.ShippingLine != null ? jobDeclaration.ShippingLine.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString VesselName
		{
			get { return jobDeclaration.JE_VesselName; }
		}

		public ZDateTime DepartureDate
		{
			get { return jobDeclaration.JE_ExportDate; }
		}

		public IEnumerable<ICertificateLine> CertificateLines
		{
			get
			{
				int lineNum = 0;
				invoiceHeader.JobComInvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name);
				foreach (JobComInvoiceLine line in invoiceHeader.JobComInvoiceLines)
				{
					yield return new CertificateLineWrapper(++lineNum, line);
				}
			}
		}

		#endregion

		readonly JobComInvoiceHeader invoiceHeader;
		readonly JobDeclaration jobDeclaration;
		readonly QuarantineExDocHeader quarantineExDocHeader;

		#region ImportPermitWrapper

		class ImportPermitWrapper : IImportPermit
		{
			public ImportPermitWrapper(JobComInvoiceLine invoiceLine)
			{
				Argument.NotNull(invoiceLine, "invoiceLine");
				PermitNumber = invoiceLine.JI_TempImportNum;
				PermitDate = invoiceLine.JI_TempImportDate;
			}

			#region IImportPermit Members

			public ZString PermitNumber { get; private set; }
			public ZDateTime PermitDate { get; private set; }

			#endregion
		}

		#endregion
	}
}
