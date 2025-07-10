using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	#region Key

	/// <summary>
	/// A key that represents the properties of a "Charge" that make it unique based on posting.
	/// </summary>
	public class PostingChargeKey : IComparable
	{
		public PostingChargeKey(ZGuid org, ZString invoiceType, ZString jobNumber, ZGuid orgAddress, ZGuid orgContact, ZShort taxRatePostingGroupId, ZGuid branch = default, ZString placeOfSupply = default, ZGuid taxBranch = default)
		{
			this.Org = org;
			this.OrgAddress = orgAddress;
			this.OrgContact = orgContact;
			this.InvoiceType = invoiceType;
			this.JobNumber = jobNumber;
			this.TaxRatePostingGroupId = taxRatePostingGroupId;

			if (!branch.IsDefault)
			{
				var registryItem = AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting;
				if (registryItem.Value.EnableBranchLevelPosting)
				{
					Branch = BranchLevelPostingHelper.GetParentBranchPK(registryItem, branch);
				}
			}
			if (!placeOfSupply.IsDefault && AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.Value)
			{
				PlaceOfSupply = placeOfSupply;
			}

			if (!taxBranch.IsDefault && AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				TaxBranch = taxBranch;
			}
		}

		public PostingChargeKey(ZGuid org, ZString invoiceType, ZGuid orgAddress, ZGuid orgContact, ZShort taxRatePostingGroupId, ZGuid branch = default, ZString placeOfSupply = default, ZGuid taxBranch = default)
			: this(org, invoiceType, "", orgAddress, orgContact, taxRatePostingGroupId, branch, placeOfSupply, taxBranch)
		{
		}

		public PostingChargeKey(PostingChargeKey existingKey)
			: this(existingKey.Org, existingKey.InvoiceType, existingKey.JobNumber, existingKey.OrgAddress, existingKey.OrgContact, existingKey.TaxRatePostingGroupId, existingKey.Branch, existingKey.PlaceOfSupply, existingKey.TaxBranch)
		{
			TaxRate = existingKey.TaxRate;
			SellCurrency = existingKey.SellCurrency;
			SplitInvoiceCount = existingKey.SplitInvoiceCount;
			SellReference = existingKey.SellReference;
			IsCommentChargeKey = existingKey.IsCommentChargeKey;
			TaxSystemSplitKey = existingKey.TaxSystemSplitKey;
		}

		public readonly ZGuid Org;
		public readonly ZGuid OrgAddress;
		public readonly ZGuid OrgContact;
		public readonly ZShort TaxRatePostingGroupId;

		public ZString InvoiceType;
		public ZGuid TaxRate;
		public ZString SellCurrency;
		public int SplitInvoiceCount;

		public ZString JobNumber;

		public ZString SellReference;

		public bool IsCommentChargeKey;

		public ZInt TaxSystemSplitKey;

		public ZGuid TaxBranch;

		public readonly ZGuid Branch;
		public readonly ZString PlaceOfSupply;

		bool IsPostingGroupsEnabled => !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value
			&& AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return CompareTo(obj, true);
		}

		public int CompareTo(object obj, bool shouldComparePostingGroupId)
		{
			var postingKey = (PostingChargeKey)obj;

			if (postingKey.SellCurrency == SellCurrency &&
				postingKey.InvoiceType == InvoiceType &&
				postingKey.Org == Org &&
				postingKey.OrgAddress == OrgAddress &&
				postingKey.OrgContact == OrgContact &&
				postingKey.TaxRate == TaxRate &&
				postingKey.SplitInvoiceCount == SplitInvoiceCount &&
				postingKey.JobNumber == JobNumber &&
				postingKey.SellReference == SellReference &&
				postingKey.Branch == Branch &&
				postingKey.PlaceOfSupply == PlaceOfSupply &&
				postingKey.TaxSystemSplitKey == TaxSystemSplitKey &&
				postingKey.TaxBranch == TaxBranch &&
				(!shouldComparePostingGroupId || !IsPostingGroupsEnabled || postingKey.TaxRatePostingGroupId == TaxRatePostingGroupId))
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}

		#endregion
	}

	#endregion

	public class PostingChargeCollection : IEnumerable
	{
		public PostingChargeCollection()
		{
			Hash = new Hashtable();
		}

		/// <summary>
		/// Set the charges that relate to the specified key.
		/// </summary>
		/// <param name="chargeKey"></param>
		/// <param name="charges"></param>
		public void SetCharges(PostingChargeKey chargeKey, IReceivablesPostingChargeCollection charges)
		{
			charges.Key = chargeKey;
			foreach (PostingChargeKey key in Hash.Keys)
			{
				if (key.CompareTo(chargeKey) == 0)
				{
					Hash[key] = charges;
					return;
				}
			}

			Hash[chargeKey] = charges;
		}

		/// <summary>
		/// Get the charges collection that relate to the specified key.
		/// </summary>
		/// <param name="chargeKey"></param>
		/// <returns></returns>
		public IReceivablesPostingChargeCollection GetCharges(PostingChargeKey chargeKey)
		{
			foreach (PostingChargeKey key in Hash.Keys)
			{
				if (key.CompareTo(chargeKey) == 0)
				{
					return (IReceivablesPostingChargeCollection)Hash[key];
				}
			}

			return null;
		}

		/// <summary>
		/// Does the specified key have any charges?
		/// </summary>
		/// <param name="chargeKey"></param>
		/// <returns></returns>
		public bool ContainsKey(PostingChargeKey chargeKey)
		{
			foreach (PostingChargeKey key in Hash.Keys)
			{
				if (key.CompareTo(chargeKey) == 0)
				{
					return true;
				}
			}

			return false;
		}

		public IReceivablesPostingChargeCollection this[PostingChargeKey key]
		{
			get { return GetCharges(key); }
		}

		public IReceivablesPostingChargeCollection GetCharges(OrgHeader org, RefCurrency currency)
		{
			foreach (IReceivablesPostingChargeCollection charges in this)
			{
				if (charges.Count > 0 && charges[0].Debtor.PK == org.PK && charges[0].SellCurrency.RX_Code == currency.RX_Code)
				{
					return charges;
				}
			}

			return null;
		}

		public void MergeCommentChargeKeysWithoutComparingPostingGroups()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value && AccTaxRate.IsPostingGroupsEnabled(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				var mergeList = new List<Tuple<IReceivablesPostingChargeCollection, IReceivablesPostingChargeCollection>>();

				foreach (IReceivablesPostingChargeCollection childCollection in this)
				{
					if (childCollection.Key.IsCommentChargeKey)
					{
						foreach (IReceivablesPostingChargeCollection parentCollection in this)
						{
							if (childCollection != parentCollection && !parentCollection.Key.IsCommentChargeKey &&
								parentCollection.Key.CompareTo(childCollection.Key, false) == 0)
							{
								mergeList.Add(Tuple.Create(childCollection, parentCollection));
								break;
							}
						}
					}
				}

				foreach (var merge in mergeList)
				{
					var child = merge.Item1;
					var parent = merge.Item2;

					parent.AddRange(child);
					Hash.Remove(child.Key);
				}
			}
		}

		#region Implementation

		readonly Hashtable Hash;

		public int Count
		{
			get { return Hash.Count; }
		}

		public ICollection Keys
		{
			get { return Hash.Keys; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return Hash.Values.GetEnumerator();
		}

		#endregion
	}
}
