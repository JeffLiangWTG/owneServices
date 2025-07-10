using System;
using System.Collections.Immutable;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class AddressTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			#region SuppressResourceStringsCheckRegion

			public const string Delivery = "Delivery";
			public const string Main = "Main";
			public const string Payables = "Payables";
			public const string Pickup = "Pickup";
			public const string Postal = "Postal";
			public const string Receivables = "Receivables";
			public const string Sales = "Sales";
			public const string Transport = "Transport";

			#endregion
		}

		public static class Descriptions
		{
			public static string Delivery { get { return Res.GetString("bfb675e2-c553-49a4-adc5-741ef7651394", "Delivery Address"); } }
			public static string Main { get { return Res.GetString("f4de2f68-a254-44b0-b305-1c4bd2a48028", "Office Address"); } }
			public static string Payables { get { return Res.GetString("2705c19e-c118-476f-9bbb-dca03e4ffc51", "Accounts Payable Mailing Address"); } }
			public static string Pickup { get { return Res.GetString("947672d1-8438-4786-a705-ee7d717d609f", "Consignment Pickup Address"); } }
			public static string Postal { get { return Res.GetString("fae1d5cb-d300-47c0-a4ef-fa4832cbd42a", "Consignment Postal Address"); } }
			public static string Receivables { get { return Res.GetString("4685b562-1a4e-45e0-9c21-8e60df4a324e", "Accounts Receivable Mailing Address"); } }
			public static string Sales { get { return Res.GetString("efd3313c-8ff7-4889-8595-d5193e9f051d", "Address for Sales Related Documents"); } }
			public static string Transport { get { return Res.GetString("28a7d4ef-af0f-4f77-8cd4-c4f0fd3a2611", "Consignment Pickup or Delivery Address"); } }
		}

		public AddressTypeList()
		{
			Add(new AddressType(Codes.Delivery, Descriptions.Delivery, OrgAddressType.Delivery, OrgAddressType.PickupAndDelivery, OrgAddressType.Pickup));
			Add(new AddressType(Codes.Main, Descriptions.Main));
			Add(new AddressType(Codes.Payables, Descriptions.Payables, OrgAddressType.Payables));
			Add(new AddressType(Codes.Pickup, Descriptions.Pickup, OrgAddressType.Pickup, OrgAddressType.PickupAndDelivery, OrgAddressType.Delivery));
			Add(new AddressType(Codes.Postal, Descriptions.Postal, OrgAddressType.Postal));
			Add(new AddressType(Codes.Receivables, Descriptions.Receivables, OrgAddressType.Receivables));
			Add(new AddressType(Codes.Sales, Descriptions.Sales, OrgAddressType.Sales));
			Add(new AddressType(Codes.Transport, Descriptions.Transport, OrgAddressType.PickupAndDelivery, OrgAddressType.Pickup, OrgAddressType.Delivery));
		}

		protected override int AddCore(ICodeDescription element)
		{
			if (element.GetType() != typeof(AddressType))
			{
				throw new Exception("This Collection can only contain objects of type: " + typeof(AddressType).FullName);
			}
			return base.AddCore(element);
		}

		public class AddressType : CodeDescriptionPair
		{
			public AddressType(string code, string description, params OrgAddressType[] fallbackChain)
				: base(code, description)
			{
				FallbackChain = fallbackChain.ToImmutableArray();
			}
			public readonly ImmutableArray<OrgAddressType> FallbackChain;
		}

		public OrgAddress GetAddressUsingFallbackIfTypeCodeRecognised(OrgHeader organisation, string addressTypeCode)
		{
			OrgAddress result = null;
			AddressType addressType = (AddressType)this[addressTypeCode];
			if (addressType != null && addressType.FallbackChain != null)
			{
				foreach (OrgAddressType orgAddressType in addressType.FallbackChain)
				{
					result = organisation.Addresses.DefaultAddressOfType(orgAddressType, false);
					if (result != null)
					{
						break;
					}
				}
				if (result == null)
				{
					result = organisation.MainAddress;
				}
			}
			return result;
		}
	}
}
