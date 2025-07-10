using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public static class MessageProviderHelper
	{
		#region Party Provider

		public static ZString GetPartyRegNo(OrgAddress orgAddress)
		{
			var result = ZString.Empty;
			var header = orgAddress?.Header;
			if (header != null)
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_OH, header.PK);
				_ = query.AddToFilter(OrgCusCodeSchema.OK_CodeType, new[] { OrgCusCode.IrelandCodeTypes.PYE, OrgCusCode.IrelandCodeTypes.ITX, OrgCusCode.IrelandCodeTypes.CGT });
				_ = query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Ireland);
				query.FetchOnlyFromLocalCache = !header.IsInDatabase;
				OrgCusCode lastOrgCusCode = null;
				foreach (var orgCusCode in header.Factory.Load<OrgCusCode>(query))
				{
					var codeType = orgCusCode.OK_CodeType.ToUpperInvariant();
					if (codeType == OrgCusCode.IrelandCodeTypes.PYE)
					{
						result = (codeType + orgCusCode.OK_CustomsRegNo).ToUpper();
						break;
					}
					else if (lastOrgCusCode == null
						|| (!lastOrgCusCode.OK_CodeType.EqualsIgnoringCase(OrgCusCode.IrelandCodeTypes.ITX) && codeType == OrgCusCode.IrelandCodeTypes.ITX))
					{
						lastOrgCusCode = orgCusCode;
					}
				}
				if (result.IsEmpty && lastOrgCusCode != null)
				{
					result = (lastOrgCusCode.OK_CodeType + lastOrgCusCode.OK_CustomsRegNo).ToUpper();
				}
			}
			return result;
		}

		#endregion

		public static ZString GetDefaultTerritoryForPortOfArrival(this JobDeclaration declaration) => declaration.GetDefaultTerritory(declaration.JE_RL_NKPortOfArrival.Left(2));

		public static IReadOnlyCollection<string> GetCountryOfRoutingConsignment(this JobDeclaration declaration)
		{
			var portOfArrival = declaration.GetDefaultTerritoryForPortOfArrival();
			return portOfArrival.IsEmpty ? Array.Empty<string>() : new[] { (string)portOfArrival };
		}

		internal static IPackaging[] GetPackingDetail(JobComInvoiceLine invoiceLine)
		{
			var result = new List<IPackaging>();
			foreach ((var pivot, var package) in invoiceLine.PackagesPivot.Cast<Customs.Business.InvoiceLinePackagePivot>().Select(x => (pivot: x, package: x.Package as Package)).Where(x => x.package != null).OrderBy(x => x.pivot.CHC_SystemCreateTimeUtc).ThenBy(x => x.package.CW_SystemCreateTimeUtc))
			{
				var packageType = package.PackageType;
				int quantity = 0;
				if (packageType != PackageType.Bulk)
				{
					quantity = pivot.CHC_NumberOfPacks;
				}
				result.Add(new PackagingProvider(package.CW_PackType, quantity, package.CW_MarksAndNos));
			}
			return result.ToArray();
		}

		public static string GetDocAddressLine(IDocAddress address)
		{
			if (address == null)
			{
				return string.Empty;
			}

			var result = new ZStringBuilder();
			result.AppendIfNotEmpty(address.E2_Address1);
			result.AppendIfNotEmpty(address.E2_Address2);
			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		public static IReadOnlyCollection<IPreviousDocumentLine> GetE1301PreviousDocumentLines(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine, bool shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod)
		{
			return GetPreviousDocuments(declaration, entryHeader, entryLine, shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod).Select(x => new PreviousDocumentLineProvider(x)).ToArray<IPreviousDocumentLine>();
		}

		public static IReadOnlyCollection<IDocument> GetE1301PreviousDocuments(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine, bool shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod)
		{
			return GetPreviousDocuments(declaration, entryHeader, entryLine, shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod).Select(x => new PreviousDocumentProvider(x)).ToArray<IDocument>();
		}

		static IEnumerable<PreviousDocument> GetPreviousDocuments(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine, bool shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod)
		{
			IEnumerable<PreviousDocument> previousDocuments = null;
			if (shouldIncludeHeaderPreviousDocumentDuringTransitionPeriod && declaration.IsTransitionPeriodAES30 && entryHeader.PreviousDocuments.Any())
			{
				previousDocuments = EU.Business.Extensions.GetAggregatedData(declaration.PreviousDocumentKeys, entryLine.PreviousDocuments.Union(entryHeader.PreviousDocuments));
			}
			else
			{
				previousDocuments = entryLine.PreviousDocuments;
			}

			return previousDocuments;
		}

		public static IReadOnlyCollection<ISupportingDocumentLine> GetE1301SupportingDocumentLines(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			return GetSupportingDocuments(declaration, entryHeader, entryLine).Select(x => new SupportingDocumentLineProvider(x)).ToArray<ISupportingDocumentLine>();
		}

		public static IReadOnlyCollection<IDocument> GetE1301SupportingDocuments(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			return GetSupportingDocuments(declaration, entryHeader, entryLine).Select(x => new SupportingDocumentProvider(x)).ToArray<IDocument>();
		}

		static IEnumerable<SupportingDocument> GetSupportingDocuments(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			IEnumerable<SupportingDocument> supportingDocuments = null;
			if (declaration.IsTransitionPeriodAES30 && entryHeader.SupportingDocuments.Any())
			{
				supportingDocuments = EU.Business.Extensions.GetAggregatedData(declaration.CreateEntryCreationStrategy().GetSupportingDocumentKeys(), entryLine.SupportingDocuments.Union(entryHeader.SupportingDocuments));
			}
			else
			{
				supportingDocuments = entryLine.SupportingDocuments.Cast<SupportingDocument>();
			}

			return supportingDocuments;
		}

		public static IReadOnlyCollection<IAdditionalInformation> GetE1301AdditionalInformations(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			IEnumerable<AdditionalInfo> additionalInformations = null;
			if (declaration.IsTransitionPeriodAES30 && entryHeader.AdditionalInfos.Any())
			{
				additionalInformations = EU.Business.Extensions.GetAggregatedData(declaration.AdditionalInfoKeys, FindAdditionalInfos(entryLine, AdditionalInfoSubTypeList.Codes.AdditionalInformation).Union(FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation)));
			}
			else
			{
				additionalInformations = FindAdditionalInfos(entryLine, AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			}

			return additionalInformations.Select(x => AdditionalInformationProvider.New(x)).ToArray<IAdditionalInformation>();
		}

		public static IReadOnlyCollection<IDocument> GetE1301AdditionalReferences(JobDeclaration declaration, CusEntryHeader entryHeader, CusEntryLine entryLine)
		{
			IEnumerable<AdditionalInfo> additionalReferences = null;
			if (declaration.IsTransitionPeriodAES30 && entryHeader.AdditionalInfos.Any())
			{
				var shouldIncludeID23FromHeader = entryLine.CL_LineNumber == 1;
				additionalReferences = EU.Business.Extensions.GetAggregatedData(declaration.AdditionalInfoKeys,
					FindAdditionalInfos(entryHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference).Where(x => shouldIncludeID23FromHeader || !x.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture))
					.Union(FindAdditionalInfos(entryLine, AdditionalInfoSubTypeList.Codes.AdditionalReference))
				);
			}
			else
			{
				additionalReferences = FindAdditionalInfos(entryLine, AdditionalInfoSubTypeList.Codes.AdditionalReference);
			}

			return additionalReferences.Select(x => new AdditionalReferenceProvider(x)).ToArray<IDocument>();
		}

		public static IEnumerable<AdditionalInfo> FindAdditionalInfos(CusEntryLine entryLine, string subType) => entryLine.AdditionalInfos.Where(inf => inf.CSI_SubType == subType).Cast<AdditionalInfo>();

		public static IEnumerable<AdditionalInfo> FindAdditionalInfos(CusEntryHeader entryHeader, string subType) => entryHeader.AdditionalInfos.Where(inf => inf.CSI_SubType == subType).Cast<AdditionalInfo>();

		public static IEnumerable<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> FindAdditionalInfos(TemporaryStoragePackedItem item, string subType) => item.AdditionalInfos.Where(inf => inf.CSI_SubType == subType).Cast<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();

		public static IEnumerable<AdditionalInfo> FindAdditionalInfosExcludeCode(CusEntryHeader entryHeader, string subType, string exludedCode) => entryHeader.AdditionalInfos.Where(inf => inf.CSI_SubType == subType && inf.CSI_Code != exludedCode).Cast<AdditionalInfo>();

		public static IReadOnlyCollection<TProvider> GetTransportDocuments<TProvider>(this IAdditionalInfoCollectionProvider collectionProvider) where TProvider : CusSupportingInfoProvider
			=> collectionProvider.GetAdditionalInfos<TProvider>(AdditionalInfoSubTypeList.Codes.TransportDocument);

		public static IReadOnlyCollection<TProvider> GetAdditionalReferences<TProvider>(this IAdditionalInfoCollectionProvider collectionProvider) where TProvider : CusSupportingInfoProvider
			=> collectionProvider.GetAdditionalInfos<TProvider>(AdditionalInfoSubTypeList.Codes.AdditionalReference);

		public static IReadOnlyCollection<TProvider> GetAdditionalInformations<TProvider>(this IAdditionalInfoCollectionProvider collectionProvider) where TProvider : CusSupportingInfoProvider
			=> collectionProvider.GetAdditionalInfos<TProvider>(AdditionalInfoSubTypeList.Codes.AdditionalInformation);

		static IReadOnlyCollection<TProvider> GetAdditionalInfos<TProvider>(this IAdditionalInfoCollectionProvider collectionProvider, ZString subType)
		=> collectionProvider.AdditionalInfos
			.Where(info => info.CSI_SubType == subType)
			.Select(x => (TProvider)Activator.CreateInstance(typeof(TProvider), x))
			.ToArray();

		public static string GetVatIdentificationNumber(IFactory factory, ZGuid orgHeaderPK)
		{
			if (orgHeaderPK.IsEmpty)
			{
				return string.Empty;
			}
			var orgHeader = factory.Load<OrgHeader>(orgHeaderPK);
			return orgHeader?.GetEoriNumber(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ?? string.Empty;
		}

		public static ZString GetRepresentativeStatus(TemporaryStorageHeader header) => GetRepresentativeStatus(header.AMA_OA_Representative == header.AMA_OA_Declarant);

		public static ZString GetRepresentativeStatus(bool representativeSameAsDeclarant) => representativeSameAsDeclarant ? Constants.RepresentativeStatus.SameAsDeclarant : Constants.RepresentativeStatus.DifferentFromDeclarant;

		public static string[] GetTransportDocumentKeys() => new[]
		{
			AdditionalInfo.Schema.CSI_Code,
			AdditionalInfo.Schema.CSI_ReferenceNumber
		};
	}
}
