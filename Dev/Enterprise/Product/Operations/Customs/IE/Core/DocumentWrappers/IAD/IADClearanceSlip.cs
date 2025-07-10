using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using CusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	[CodeAlive("Used in documents DataContext")]
	sealed class IADClearanceSlip : DocBaseWrapper
	{
		public static IADClearanceSlip New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new IADClearanceSlip(entryHeader, factoryToWrap);

		IADClearanceSlip(CusEntryHeader entryHeader, BusinessObjectFactory factory) : base(entryHeader, factory)
		{
			CusEntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			jobDeclaration = entryHeader.Declaration;
		}
		public readonly CusEntryHeader CusEntryHeader;
		readonly JobDeclaration jobDeclaration;

		public ZString MovementReferenceNumber => CusEntryHeader.MovementReferenceNumber;

		public ZDateTime PrintDate => ZDateTime.Now;

		public ZDateTime IssuingDate => CusEntryHeader.MovementReferenceNumberIssueDate;

		#region Routing

		public ZString Routing => Factory.GetCached(ref routing, () => IADRoutingHelper.GetRouting(CusEntryHeader));
		CachedProperty<ZString> routing;

		#endregion

		public ZString JobNumber => CusEntryHeader.Declaration.JobNumber;

		public ZString LRN => CusEntryHeader.CH_BGMReference;

		public ZString GoodsLocation
		{
			get
			{
				if (CusEntryHeader.EntryInstruction?.GoodsLocation is CusGoodsLocation goodsLocation)
				{
					return $"{goodsLocation.CGL_Qualifier} {goodsLocation.CGL_Type} {goodsLocation.Unlocode}";
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString UCR => !CusEntryHeader.Declaration.JE_UCR.IsEmpty
						? CusEntryHeader.Declaration.JE_UCR
						: CusEntryHeader.RandomHeader.JZ_UCR;

		public ZString ImporterEORI => CusEntryHeader.ImporterEoriOfMainOffice;
		public ZString ImporterAddressLines => ToDisplayLines(jobDeclaration?.ImporterDocumentaryAddress);

		public ZString SupplierEORI => CusEntryHeader.SupplierEoriOfMainOffice;
		public ZString SupplierAddressLines => ToDisplayLines(jobDeclaration?.SupplierDocumentaryAddress);

		static ZString ToDisplayLines(JobDocAddress docAddress) => docAddress == null ?
				string.Empty
				: new ZStringBuilder()
					.AppendIfNotEmpty(docAddress.CompanyName)
					.AppendIfNotEmpty(docAddress.Address1)
					.AppendIfNotEmpty(docAddress.Address2)
					.AppendIfNotEmpty(docAddress.City)
					.AppendIfNotEmpty(docAddress.Postcode)
					.AppendIfNotEmpty(docAddress.Country?.Code)
					.ToStringWithNewLineBetweenAppends();

		public ZInt Packages => CusEntryHeader.PackagesCount;

		public ZString NetWeight => CusEntryHeader.TotalCustomsQuantity.Round(3).ToStringTrimZeros();

		public ZString GrossWeight => CusEntryHeader.TotalGrossWeightInKG.Round(3).ToStringTrimZeros();

		public ZString TransportTypeAndID => jobDeclaration is JobDeclaration declaration ? $"{declaration.JE_TransportMeans} {declaration.JE_TransportIDInland}" : string.Empty;

		public ZString Containers => Factory.GetValue(
			ref containers, () => string.Join(System.Environment.NewLine, CusEntryHeader.Containers.Select(cnt => cnt.CO_ContainerNumber).OrderBy(n => n).ToArray())
		);
		CachedProperty<string> containers;

		public ZString Seals => Factory.GetValue(
			ref seals, () => string.Join(System.Environment.NewLine, CusEntryHeader.Containers.Select(cnt => cnt.CO_Seal).OrderBy(n => n).ToArray())
		);
		CachedProperty<string> seals;

		public ZString TransportDocuments => Factory.GetCached(ref transportDocuments, () => PrintDocuments(
			CusEntryHeader,
			header => header.PreviousDocuments,
			line => line.PreviousDocuments,
			doc => doc.CSI_Code.StartsWith(transportDocumentsPrefix)
		));
		CachedProperty<ZString> transportDocuments;

		const string transportDocumentsPrefix = "7";

		public ZString BillReferences => Factory.GetCached(ref billReferences, () => PrintDocuments(
			CusEntryHeader,
			header => header.SupportingDocuments,
			line => line.SupportingDocuments,
			doc => BillReferenceFilter.Value.Contains(doc.CSI_Code)
		));
		CachedProperty<ZString> billReferences;

		[ThreadSafe]
		readonly static Lazy<HashSet<string>> BillReferenceFilter = new(() => new HashSet<string> {
						Constants.TransportDocumentCodes._N703,
						Constants.TransportDocumentCodes._N705,
						Constants.TransportDocumentCodes._N710,
						Constants.TransportDocumentCodes._N741
		});

		static string PrintDocuments<TDocument>(
			CusEntryHeader header,
			Func<CusEntryHeader, IEnumerable<TDocument>> getHeaderDocuments,
			Func<CusEntryLine, IEnumerable<TDocument>> getLineDocuments,
			Func<TDocument, bool> filter
		) where TDocument : Customs.Business.CusSupportingInfo
		{
			var allDocuments = getHeaderDocuments(header).Concat(header.MergedLines.SelectMany(getLineDocuments)).Where(filter);
			var allDocumentNumbers = allDocuments.Select(d => d.CSI_ReferenceNumber).OrderBy(n => n).ToArray();
			return string.Join(System.Environment.NewLine, allDocumentNumbers);
		}
	}
}
