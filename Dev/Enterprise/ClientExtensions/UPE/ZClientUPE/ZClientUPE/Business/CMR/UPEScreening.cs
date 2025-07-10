using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.CMR
{
	public class UPEScreening
	{
		public UPEScreening(UPECusHAWB cusHAWB)
		{
			CusHAWB = cusHAWB;
		}

		#region UPS Stop Phrases

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsignorName => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsignorName, UPEDataRegistry.Instance.StopPhrasesForConsignorName);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsignorAccountNum => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.Level1RecordConsignorAccountNum, UPEDataRegistry.Instance.StopPhrasesForConsignorAccountNum);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsignorStreet => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsignorStreet, UPEDataRegistry.Instance.StopPhrasesForConsignorAddress);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsignorStreet2 => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsignorStreet2, UPEDataRegistry.Instance.StopPhrasesForConsignorAddress);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsigneeName => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsigneeName, UPEDataRegistry.Instance.StopPhrasesForConsigneeName);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsigneeAccountNum => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.Level1RecordConsigneeAccountNum, UPEDataRegistry.Instance.StopPhrasesForConsigneeAccountNum);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsigneeStreet => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsigneeStreet, UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress);

		public IEnumerable<ZString> UPSStopPhrasesFoundInConsigneeStreet2 => StopPhraseMatcher.FindMatchingStopPhrasesExact(CusHAWB.CS_ConsigneeStreet2, UPEDataRegistry.Instance.StopPhrasesForConsigneeAddress);

		public IEnumerable<ZString> UPSStopPhrasesFoundInGoodsDescription => StopPhraseMatcher.FindMatchingStopPhrasesPluralized(CusHAWB.CS_GoodsDescription, UPEDataRegistry.Instance.StopPhrasesForGoodsDescription);

		public IEnumerable<ZString> QuarantineStopPhrasesFoundInGoodsDescription => StopPhraseMatcher.FindMatchingStopPhrasesPluralized(CusHAWB.CS_GoodsDescription, UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription);

		public ZString GoodsDescriptionStopPhraseWarning
		{
			get
			{
				var result = new StringWriter();
				if (!SACDecider.IsValueOverTheScreenFreeValue)
				{
					var customsStopPhrases = SACDecider.StopPhrasesFoundInGoodsDescription;
					if (customsStopPhrases.Any())
					{
						result.WriteLine("Customs Stop Words found: {0}", string.Join(", ", customsStopPhrases));
					}

					var uPSStopPhrases = UPSStopPhrasesFoundInGoodsDescription;
					if (uPSStopPhrases.Any())
					{
						result.WriteLine("UPS Stop Words found: {0}", string.Join(", ", uPSStopPhrases));
					}

					var quarantineStopPhrases = QuarantineStopPhrasesFoundInGoodsDescription;
					if (quarantineStopPhrases.Any())
					{
						result.WriteLine("Quarantine Stop Words found: {0}", string.Join(", ", quarantineStopPhrases));
					}
				}
				return result.GetStringBuilder().ToString().Trim();
			}
		}

		SACDecider SACDecider
		{
			get { return new SACDecider(CusHAWB.Factory, CusHAWB.GoodsValueInAUD, CusHAWB.CS_GoodsDescription); }
		}

		#endregion

		#region Identifying Shipments

		public bool IsIdentifiedForScreening
		{
			get { return AllScreeningStopPhrases.Any() || IsGoodsValueIdentified; }
		}

		public bool IsIdentifiedForQuarantine
		{
			get { return QuarantineStopPhrasesFoundInGoodsDescription.Any(); }
		}

		public bool IsGoodsValueIdentified
		{
			get
			{
				if (CusHAWB.CS_ShipmentTypeForBinding == ShipmentTypeCodeDescriptionPairList.Codes.NonDocuments)
				{
					foreach (KeyValuePair<decimal, decimal> range in UPEDataRegistry.Instance.NonDocumentScreeningValueRanges)
					{
						if (CusHAWB.GoodsValueInAUD >= range.Key && CusHAWB.GoodsValueInAUD <= range.Value)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public ZString IdentifiedReason
		{
			get
			{
				var result = ZString.Empty;
				var stopPhrases = AllStopPhrases;
				int i = stopPhrases.Count;
				do
				{
					result = AbbrieviatedProfilingScreeningOrQuarantine + GetIdentifiedReason(i--, stopPhrases);
				}
				while (result.Length > ProcessQueueSchema.P4_CustomsReason.MaxLength);
				return result;
			}
		}

		#endregion

		#region Implementation

		readonly UPECusHAWB CusHAWB;

		List<ZString> AllScreeningStopPhrases
		{
			get
			{
				var result = new List<ZString>();
				result.AddRange(UPSStopPhrasesFoundInGoodsDescription);
				result.AddRange(UPSStopPhrasesFoundInConsigneeAccountNum);
				result.AddRange(UPSStopPhrasesFoundInConsignorAccountNum);
				result.AddRange(UPSStopPhrasesFoundInConsigneeName);
				result.AddRange(UPSStopPhrasesFoundInConsignorName);
				result.AddRange(UPSStopPhrasesFoundInConsigneeStreet);
				result.AddRange(UPSStopPhrasesFoundInConsigneeStreet2);
				result.AddRange(UPSStopPhrasesFoundInConsignorStreet);
				result.AddRange(UPSStopPhrasesFoundInConsignorStreet2);
				return result;
			}
		}

		List<ZString> AllStopPhrases
		{
			get
			{
				var result = new List<ZString>();
				result.AddRange(SACDecider.StopPhrasesFoundInGoodsDescription);
				result.AddRange(AllScreeningStopPhrases);
				result.AddRange(QuarantineStopPhrasesFoundInGoodsDescription);
				return result;
			}
		}

		ZString GetIdentifiedReason(int maxWords, List<ZString> allStopPhrases)
		{
			var allStopPhrasesCount = allStopPhrases.Count;
			var phrases = allStopPhrases.GetRange(0, Math.Min(maxWords, allStopPhrasesCount));
			string result = string.Join(",", phrases.ToArray());

			if (maxWords < allStopPhrasesCount)
			{
				result += ",etc";
			}
			return result;
		}

		ZString AbbrieviatedProfilingScreeningOrQuarantine
		{
			get
			{
				ZString result = "";
				switch (CusHAWB.IdentifiedForAction)
				{
					case "Profiling":
						result = "PRF:";
						break;

					case "Screening":
						result = "SCR:";
						break;

					case "Quarantine":
						result = "QUA:";
						break;

					case "Any":
						result = "ANY:";
						break;
				}
				return result;
			}
		}

		#endregion
	}
}
