using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
		}

		public static DocDeclaration New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			return jobDeclaration == null ? null : new DocDeclaration(jobDeclaration, factoryToWrap);
		}

		#region Overrides

		protected override ZString GoodsDescriptionCore
		{
			get
			{
				return JobDeclaration.GoodsDescriptionForDocumentsAndInvoice(base.GoodsDescriptionCore);
			}
		}

		protected override ZString OwnerRefCore
		{
			get
			{
				if (JobDeclaration.IsLVX)
				{
					return JobDeclaration.LVXDocumentsAndInvoiceOwnerRef;
				}
				else if (JobDeclaration.IsLVS)
				{
					return ZString.Empty;
				}
				else if (JobDeclaration.IsB2Adjustments)
				{
					return JobDeclaration.B2DocumentsAndInvoiceOwnerRef;
				}
				return base.OwnerRefCore;
			}
		}

		protected override RefUNLOCO OriginCore
		{
			get
			{
				return JobDeclaration.IsLVX ? null : base.OriginCore;
			}
		}

		protected override RefUNLOCO FinalDestinationCore
		{
			get
			{
				return IsLVXOrB2 ? null : base.FinalDestinationCore;
			}
		}

		bool IsLVSTotalConsolidationOrB2OrLVX
		{
			get { return JobDeclaration.IsLVSTotalConsolidation || JobDeclaration.IsB2Adjustments || JobDeclaration.IsLVX; }
		}

		bool IsLVXOrB2
		{
			get { return JobDeclaration.IsLVX || JobDeclaration.IsB2Adjustments; }
		}

		public override ZString SupplierInvoiceNumbers
		{
			get { return IsLVSTotalConsolidationOrB2OrLVX ? ZString.Empty : base.SupplierInvoiceNumbers; }
		}

		protected override DocOrganisation GetImporter()
		{
			return JobDeclaration.IsLVSTotalConsolidation ? null : base.GetImporter();
		}

		bool IsLVSOrB2
		{
			get { return JobDeclaration.IsLVS || JobDeclaration.IsB2Adjustments; }
		}

		bool IsConsolidatedLVSOrB2
		{
			get { return JobDeclaration.IsConsolidatedLVS || JobDeclaration.IsB2Adjustments; }
		}

		protected override DocOrganisation GetSupplier()
		{
			return IsConsolidatedLVSOrB2 ? null : base.GetSupplier();
		}

		protected override ZString OrderRefCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.OrderRefCore; }
		}

		protected override ZString WeightCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.WeightCore; }
		}

		protected override ZString WeightUnitCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.WeightUnitCore; }
		}

		protected override ZString VolumeCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.VolumeCore; }
		}

		protected override ZString VolumeUnitCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.VolumeUnitCore; }
		}

		protected override ZString PackagesCore
		{
			get { return IsLVSOrB2 ? ZString.Empty : base.PackagesCore; }
		}

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public ZString CommercialInvoiceOriginatorAddress
		{
			get { return new AddressFormatter(Factory, JobDeclaration.CommercialInvoiceOriginator, GlbCompany.CurrentCompany, false).PostalAddress(); }
		}

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		public ZString PreviousCargoControlNumber
		{
			get
			{
				var previousCCN = CusEntryNumber.Load(JobDeclaration, CanadaAdditionalReferenceNumberTypes.Codes.PCN, JobDeclaration.CountryCode);
				return previousCCN != null ? previousCCN.CE_EntryNum : ZString.Empty;
			}
		}

		public ZString CarrierName
		{
			get
			{
				var builder = new ZStringBuilder();
				var carrierCode = JobDeclaration.JE_CarrierCode;
				builder.AppendIfNotEmpty(carrierCode);
				builder.AppendIfNotEmpty(JobDeclaration.CA_CarrierName);
				return builder.ToStringWithDelimiterBetweenAppends(" - ");
			}
		}

		public ZString USPortOfExit
		{
			get
			{
				var invoice = (JobComInvoiceHeader)JobDeclaration.Invoices.FirstOrDefault();
				return invoice == null ? ZString.Empty : GetCodeDescriptionFormatted(invoice.CA_USPortOfExit, invoice.AddInfoLookups.USPortOfExitList);
			}
		}

		public ZString B3MergedBy
		{
			get { return JobDeclaration.CA_MergeBy; }
		}

		public ZString CargoControlNumber
		{
			get
			{
				var cargoControlNumber = JobDeclaration.CargoControlNumbers.FirstOrDefault();
				return cargoControlNumber == null ? ZString.Empty : cargoControlNumber.CY_CargoControlNumber;
			}
		}

		#endregion

		#region Collections

		public DocReleaseStatusCollection ReleaseStatuses
		{
			get { return releaseStatuses ?? (releaseStatuses = new DocReleaseStatusCollection(JobDeclaration.ReleaseStatusesToPrint, Factory)); }
		}
		public DocReleaseStatusCollection releaseStatuses;

		public DocCusContainerCollection Containers
		{
			get { return (DocCusContainerCollection)ContainersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByMergedLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByMergedLineNoInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByMergedNumericLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByMergedNumericLineNoInternal; }
		}

		#endregion

		#region Implementation

		JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)WrappedObject; }
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionToWrap)
		{
			throw new NotSupportedException();
		}

		#endregion

		internal static ZString GetCodeDescriptionFormatted(ZString code, ICodeDescriptionPairList pairList)
		{
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(code);
			builder.AppendIfNotEmpty(pairList.GetDescriptionFromCode(code));
			return builder.ToStringWithDelimiterBetweenAppends(" - ");
		}

		internal static ZString GetCodeDescriptionFormatted(ZString code, ZZRefCusCodeListCombinedCollection codeListCollection)
		{
			codeListCollection.Load();
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(code);
			builder.AppendIfNotEmpty(codeListCollection.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(x => x.ZZD_Code == code)?.ZZD_Description);
			return builder.ToStringWithDelimiterBetweenAppends(" - ");
		}
	}
}
