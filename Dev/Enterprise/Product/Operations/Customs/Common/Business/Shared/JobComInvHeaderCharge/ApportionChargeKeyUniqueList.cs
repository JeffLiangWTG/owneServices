using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Customs.Common
{
	public class ApportionChargeKeyUniqueList
	{
		public void AddUniquely(ApportionChargeKey[] chargeKeys)
		{
			foreach (ApportionChargeKey chargeKey in chargeKeys)
			{
				AddUniquely(chargeKey);
			}
		}

		public void AddUniquely(ApportionChargeKey chargeKey)
		{
			if (!InternalUniqueList.ContainsKey(chargeKey.ToString()))
			{
				InternalUniqueList.Add(chargeKey.ToString(), chargeKey);
			}
		}

		public ApportionChargeKey[] GetUniqueItems()
		{
			return (ApportionChargeKey[])new ArrayList(InternalUniqueList.Values).ToArray(typeof(ApportionChargeKey));
		}

		#region Implementation

		public Dictionary<string, ApportionChargeKey> InternalUniqueList
		{
			get
			{
				if (fInternalUniqueList == null)
				{
					fInternalUniqueList = new Dictionary<string, ApportionChargeKey>();
				}
				return fInternalUniqueList;
			}
		}
		Dictionary<string, ApportionChargeKey> fInternalUniqueList;

		#endregion
	}
}

