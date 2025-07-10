using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class PreValidateTraderInfo
	{
		public IEnumerable<TraderData> Traders => EMCSPreValidateTraderInfoHelper.GetTraders(declaration);
		public IEnumerable<string> ProductCodes => EMCSPreValidateTraderInfoHelper.GetProductCodes(declaration);
		public bool CanSend => Traders.Any() && ProductCodes.Any();

		public PreValidateTraderInfo(EU.EMCS.Business.EMCSJobDeclaration emcsDeclaration)
		{
			declaration = emcsDeclaration;
		}

		readonly EU.EMCS.Business.EMCSJobDeclaration declaration;

		public class TraderData : IEquatable<TraderData>
		{
			public string Key => $"{TraderID}{TraderType}";
			public string TraderID { get; set; }
			public string TraderType { get; set; }
			public bool IsValid => !string.IsNullOrEmpty(TraderID);

			public TraderData() { }
			public TraderData(string traderId, string traderType)
			{
				TraderID = traderId;
				TraderType = traderType;
			}

			public override bool Equals(object obj) => Equals(obj as TraderData);

			public bool Equals(TraderData other) => other != null && TraderID == other.TraderID && TraderType == other.TraderType;

			public override int GetHashCode() => TraderID.GetHashCode() ^ TraderType.GetHashCode() ^ IsValid.GetHashCode();
		}
	}
}
