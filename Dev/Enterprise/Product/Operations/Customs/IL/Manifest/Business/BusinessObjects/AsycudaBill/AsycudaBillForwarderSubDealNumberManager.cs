using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBillForwarderSubDealNumberManager
	{
		public AsycudaBillForwarderSubDealNumberManager(AsycudaBill asycudaBill)
		{
			this.asycudaBill = Argument.NotNull(asycudaBill, nameof(asycudaBill));

			this.header = asycudaBill.Header;
			this.transportDocuments = asycudaBill.TransportDocuments;
		}

		public void AssignSubDealNumberToTransportDocument()
		{
			var forwarderSubDealNumber = ZString.Empty;

			if (asycudaBill.ABL_BolType != AsycudaBillKindList.Codes.HWB)
			{
				return;
			}

			var transportDocumentIL1 = transportDocuments.FirstOrDefault(td => td.CSI_Code == TransportDocsTypeList.Codes.IL1 && !td.IsDeleted);

			var (shippingAgentCodeCustomsRegNo, forwarderCodeCustomsRegNo) = GetShippingAgentAndForwarderRegNo();

			if (!shippingAgentCodeCustomsRegNo.IsEmpty && !forwarderCodeCustomsRegNo.IsEmpty)
			{
				if (!CanUpdateTransportDocumentRecord(transportDocumentIL1))
				{
					return;
				}

				forwarderSubDealNumber = BuildSubDealNumber(transportDocumentIL1, shippingAgentCodeCustomsRegNo, forwarderCodeCustomsRegNo);
				UpdateTransportDocumentRecord(forwarderSubDealNumber, transportDocumentIL1);
			}
			else if (transportDocumentIL1 != null)
			{
				transportDocuments.Delete(transportDocumentIL1);
			}

			asycudaBill.TransportDocuments.ForEach(s => s.Validation.ValidateAll());
		}

		(ZString shippingAgentCodeCustomsRegNo, ZString forwarderCodeCustomsRegNo) GetShippingAgentAndForwarderRegNo()
		{
			if (!header.IsSea || header.AMA_ManifestNumber.IsEmpty)
			{
				return (ZString.Empty, ZString.Empty);
			}

			if (GetShippingAgentCodeCustomsRegNo() is ZString shippingAgentCodeCustomsRegNo
				&& shippingAgentCodeCustomsRegNo.Length != validCustomsRegNoLength)
			{
				return (ZString.Empty, ZString.Empty);
			}

			if (GetForwarderCodeCustomsRegNo() is ZString forwarderCodeCustomsRegNo
				&& forwarderCodeCustomsRegNo.Length != validCustomsRegNoLength)
			{
				return (ZString.Empty, ZString.Empty);
			}

			return (shippingAgentCodeCustomsRegNo, forwarderCodeCustomsRegNo);
		}

		ZString BuildSubDealNumber(AsycudaTransportDocumentInfo transportDocumentIL1, ZString shippingAgentCodeCustomsRegNo, ZString forwarderCodeCustomsRegNo)
		{
			var existingSuffixes = GetExistingSuffixes();

			GetSuffixLetterAndSequenceNumber(existingSuffixes, out ZString suffixLetter, out int seqNumber);

			ZString forwarderSubDealNumber = string.Concat("I", shippingAgentCodeCustomsRegNo, forwarderCodeCustomsRegNo, suffixLetter, seqNumber.ToString("D2"));
			return forwarderSubDealNumber;
		}

		Dictionary<ZString, List<int>> GetExistingSuffixes()
		{
			var transportDocumentsFromDB = FetchTransportDocumentsForManifest(asycudaBill.Factory, header.AMA_ManifestNumber);
			var transportDocumentsCurrent = header.Bills.Cast<AsycudaBill>()
				.SelectMany(d => d.TransportDocuments)
				.Where(td => td.CSI_Code == TransportDocsTypeList.Codes.IL1 && td.CSI_ReferenceNumber.Length == dealNumberSize);
			var idsToExclude = transportDocumentsCurrent.Where(td => td.IsInDatabase).Select(td => td.PK).ToList();

			var existingSuffixes = transportDocumentsCurrent
				.Union(transportDocumentsFromDB.Where(s => !idsToExclude.Contains(s.PK)))
				.Select(td => (td.CSI_ReferenceNumber.SubstringSafe(uniqueAvailablePrefixPosition, 1), td.CSI_ReferenceNumber.SubstringSafe(uniqueAvailablePrefixPosition + 1, 2)))
				.Where(td => !td.Item1.IsEmpty && !td.Item2.IsEmpty && td.Item1.IsEnglishOnlyOrEmpty && td.Item2.IsNumbersOnlyOrEmpty)
				.GroupBy(td => td.Item1)
				.ToDictionary(g => g.Key, g => g.Select(td => int.Parse(td.Item2)).OrderBy(rn => rn).ToList());
			return existingSuffixes;
		}

		static void GetSuffixLetterAndSequenceNumber(Dictionary<ZString, List<int>> existingSuffixes, out ZString uniquePrefix, out int seqNumber)
		{
			uniquePrefix = ZString.Empty;
			seqNumber = 1;
			foreach (var letter in existingSuffixes.Keys)
			{
				if (existingSuffixes[letter].Count == 99)
				{
					continue;
				}

				seqNumber = Enumerable.Range(minSequence, maxSequence).Except(existingSuffixes[letter]).First();
				uniquePrefix = letter;
				break;
			}

			if (uniquePrefix.IsEmpty)
			{
				uniquePrefix = existingSuffixes.Keys.Count != 0
				? ((char)(existingSuffixes.Keys.Max()[0] + 1)).ToString()
				: "A";
			}
		}

		void UpdateTransportDocumentRecord(ZString forwarderSubDealNumber, AsycudaTransportDocumentInfo transportDocumentIL1)
		{
			if (transportDocumentIL1 == null)
			{
				using (transportDocuments.SuspendListChanged())
				{
					transportDocumentIL1 = transportDocuments.AddNew();
					transportDocumentIL1.CSI_Code = TransportDocsTypeList.Codes.IL1;
					transportDocumentIL1.CSI_ReferenceNumber = forwarderSubDealNumber;
				}
			}
			else if (transportDocumentIL1.CSI_ReferenceNumber != forwarderSubDealNumber)
			{
				transportDocumentIL1.CSI_ReferenceNumber = forwarderSubDealNumber;
			}
		}

		bool CanUpdateTransportDocumentRecord(AsycudaTransportDocumentInfo transportDocumentIL1)
			=> transportDocumentIL1 == null || transportDocumentIL1.CSI_ReferenceNumber.IsEmpty;

		ZString GetForwarderCodeCustomsRegNo()
			=> header.Declarant?.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.ManifestProviderID, CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;

		ZString GetShippingAgentCodeCustomsRegNo()
			=> header.ShippingAgent?.Header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, CountryCodes.Israel)?.OK_CustomsRegNo ?? ZString.Empty;

		static IEnumerable<CusSupportingInfo> FetchTransportDocumentsForManifest(BusinessObjectFactory factory, string manifestNumber)
		{
			var headerSubQuery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaBillSchema.ABL_AMA);
			headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, CountryCodes.Israel);
			headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestNumber, manifestNumber);

			var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), CusSupportingInfoSchema.CSI_ParentID);
			billSubQuery.AddSubQuery(headerSubQuery, JoinCondition.And);

			var csiQuery = new ZDBOnlyQuery(typeof(CusSupportingInfo));
			csiQuery.AddToFilter(CusSupportingInfoSchema.CSI_ParentTableCode, AsycudaBillSchema.Constants.Prefix);
			csiQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
			csiQuery.AddToFilter(CusSupportingInfoSchema.CSI_SubType, AdditionalInfoSubTypeList.Codes.TransportDocument);
			csiQuery.AddToFilter(CusSupportingInfoSchema.CSI_Code, TransportDocsTypeList.Codes.IL1);
			csiQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			return factory.Load<CusSupportingInfo>(csiQuery);
		}

		const int dealNumberSize = 10;
		const int uniqueAvailablePrefixPosition = 7;
		const int minSequence = 1;
		const int maxSequence = 99;
		const int validCustomsRegNoLength = 3;

		readonly AsycudaBill asycudaBill;
		readonly AsycudaManifestHeader header;
		readonly IAsycudaTransportDocumentInfoCollection transportDocuments;
	}
}
