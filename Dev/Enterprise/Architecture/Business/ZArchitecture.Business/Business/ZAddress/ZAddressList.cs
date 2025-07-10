using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	#region Item

	public class AddressCapabilityItem
	{
		public ZString Capability { get; set; }
		public ZBool IsDefault { get; set; }
	}

	public class ZAddressItem : ICodeDescription
	{
		public ZAddressItem(ZGuid pK, ZString usageComment, ZString addressDescription, AddressCapabilityItem[] capabilities)
		{
			this.PK = pK;
			this.UsageComment = usageComment;
			this.AddressDescription = addressDescription;
			this.Capabilities = capabilities;
		}

		public readonly ZGuid PK;
		public readonly string UsageComment;
		public readonly string AddressDescription;
		public readonly AddressCapabilityItem[] Capabilities;

		public AddressCapabilityItem GetCapability(string requiredCapability)
		{
			if (Capabilities != null)
			{
				foreach (AddressCapabilityItem capability in Capabilities)
				{
					if (capability.Capability == requiredCapability)
					{
						return capability;
					}
				}
			}

			return null;
		}

		#region ICodeDescription Members

		string ICodeDescription.Description { get { return AddressDescription; } }
		string ICodeDescription.Code { get { return UsageComment; } }
		object ICodeDescription.PK { get { return PK; } }

		#endregion
	}

	#endregion

	public class ZAddressList : IList, ICompositeCollection
	{
		public ZAddressList()
		{
		}

		public ZAddressList(Action populateListAction)
		{
			this.populateListAction = populateListAction;
		}

		public void AddAddress(ZGuid pK, ZString usageComment, ZString addressDescription, params AddressCapabilityItem[] capabilities)
		{
			List.Add(new ZAddressItem(pK, usageComment, addressDescription, capabilities));
		}

		readonly Action populateListAction;

		public CodeDescriptionPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CodeDescriptionPairList();
					if (populateListAction != null)
					{
						populateListAction();
					}
				}
				return list;
			}
		}
		CodeDescriptionPairList list;

		#region Getting Addresses

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): CST, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid CSTAddressOrFallback => AddressOrFallback(OrgConstants.AddressType.CustomsAddressOfRecord);

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): CST, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid ECAAddressOrFallback => AddressOrFallback(OrgConstants.AddressType.EUCustomsAddress);

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): PIC, PAD, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid PICAddressOrFallback
		{
			get
			{
				ZGuid result = SelectAddressByAddressType(OrgConstants.AddressType.Pickup);
				if (result.IsEmpty)
				{
					result = AddressOrFallback(OrgConstants.AddressType.PickupAndDelivery);
				}

				return result;
			}
		}

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): DLV, PAD, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid DLVAddressOrFallback
		{
			get
			{
				ZGuid result = SelectAddressByAddressType(OrgConstants.AddressType.Delivery);
				if (result.IsEmpty)
				{
					result = AddressOrFallback(OrgConstants.AddressType.PickupAndDelivery);
				}

				return result;
			}
		}
		/// <summary>
		/// Returns an address PK from the list in the following order (first found): ARM, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid ARMAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.Receivables); }
		}

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): APM, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid APMAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.Payables); }
		}

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): SQM, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid SQMAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.Sales); }
		}

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid OFCAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.Office); }
		}

		public ZGuid PSTAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.Postal); }
		}

		/// <summary>
		/// Returns an address PK from the list in the following order (first found): PAD, OFC, List[0], ZGuid.Empty.
		/// </summary>
		public ZGuid PADAddressOrFallback
		{
			get { return AddressOrFallback(OrgConstants.AddressType.PickupAndDelivery); }
		}

		protected virtual ZGuid AddressOrFallback(string addressType)
		{
			ZGuid result = SelectAddressByAddressType(addressType);
			if (result.IsEmpty)
			{
				result = SelectAddressByAddressType(OrgConstants.AddressType.Office);
				if (result.IsEmpty)
				{
					result = List.Count > 0 ? (ZGuid)List[0].PK : ZGuid.Empty;
				}
			}

			return result;
		}

		ZGuid SelectAddressByAddressType(string addressType)
		{
			bool requiresPADDefault = (addressType == OrgConstants.AddressType.Delivery || addressType == OrgConstants.AddressType.Pickup);
			string addressType2 = requiresPADDefault ? OrgConstants.AddressType.PickupAndDelivery : addressType;

			ZGuid result = ZGuid.Empty;
			foreach (ZAddressItem item in List)
			{
				AddressCapabilityItem capability1 = item.GetCapability(addressType);
				AddressCapabilityItem capability2 = item.GetCapability(addressType2);

				if (capability1 != null || capability2 != null)
				{
					result = (result.IsEmpty && capability1 != null) ? item.PK : result;
					if ((capability1 != null && capability1.IsDefault) || (capability2 != null && capability2.IsDefault))
					{
						return item.PK;
					}
				}
			}

			return result;
		}

		#endregion

		#region List Implementation

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return List.IsReadOnly; }
		}

		object IList.this[int index]
		{
			get { return List[index]; }
			set { List[index] = (ICodeDescription)value; }
		}

		void IList.RemoveAt(int index)
		{
			List.RemoveAt(index);
		}

		void IList.Insert(int index, object value)
		{
			List.Insert(index, value);
		}

		void IList.Remove(object value)
		{
			List.Remove((ICodeDescription)value);
		}

		bool IList.Contains(object value)
		{
			return ((IList)List).Contains(value);
		}

		void IList.Clear()
		{
			List.Clear();
		}

		int IList.IndexOf(object value)
		{
			return List.IndexOf((ICodeDescription)value);
		}

		int IList.Add(object value)
		{
			return List.Add((ICodeDescription)value);
		}

		bool IList.IsFixedSize
		{
			get { return List.IsFixedSize; }
		}

		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized
		{
			get { return List.IsSynchronized; }
		}

		public int Count
		{
			get { return List.Count; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			List.CopyTo(array, index);
		}

		object ICollection.SyncRoot
		{
			get { return List.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		#endregion

		#endregion

		#region ICompositeCollection

		int ICompositeCollection.MaxLength
		{
			get { throw new NotImplementedException(); }
		}

		Type ICompositeCollection.TypeOfElementFromCode(ZString code)
		{
			return ObjectFactory.GetType<IOrgAddress>();
		}

		Type ICompositeCollection.TypeOfElementFromPK(ZGuid pk)
		{
			return ObjectFactory.GetType<IOrgAddress>();
		}

		#endregion
	}
}
