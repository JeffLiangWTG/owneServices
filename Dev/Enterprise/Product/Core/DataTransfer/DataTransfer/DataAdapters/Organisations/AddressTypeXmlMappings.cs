using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class AddressTypeXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		AddressTypeXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping("", nameof(Xsd.AddressCapabilityAddressType.MAIN));
			yield return new Mapping(OrgConstants.AddressType.Office, nameof(Xsd.AddressCapabilityAddressType.OFC));
			yield return new Mapping(OrgConstants.AddressType.Postal, nameof(Xsd.AddressCapabilityAddressType.PST));

			yield return new Mapping(OrgConstants.AddressType.Receivables, nameof(Xsd.AddressCapabilityAddressType.ARM));
			yield return new Mapping(OrgConstants.AddressType.Payables, nameof(Xsd.AddressCapabilityAddressType.APM));

			yield return new Mapping(OrgConstants.AddressType.Pickup, nameof(Xsd.AddressCapabilityAddressType.PIC));
			yield return new Mapping(OrgConstants.AddressType.Delivery, nameof(Xsd.AddressCapabilityAddressType.DLV));
			yield return new Mapping(OrgConstants.AddressType.PickupAndDelivery, nameof(Xsd.AddressCapabilityAddressType.PAD));

			yield return new Mapping(OrgConstants.AddressType.Sales, nameof(Xsd.AddressCapabilityAddressType.SQM));
			yield return new Mapping(OrgConstants.AddressType.Miscellaneous, nameof(Xsd.AddressCapabilityAddressType.MSC));
			yield return new Mapping(OrgConstants.AddressType.Residential, nameof(Xsd.AddressCapabilityAddressType.RSD));
			yield return new Mapping(OrgConstants.AddressType.CustomsAddressOfRecord, nameof(Xsd.AddressCapabilityAddressType.CST));
			yield return new Mapping(OrgConstants.AddressType.AWB, nameof(Xsd.AddressCapabilityAddressType.AWB));
			yield return new Mapping(OrgConstants.AddressType.EUCustomsAddress, nameof(Xsd.AddressCapabilityAddressType.ECA));
		}

		public static readonly AddressTypeXmlMappings Instance = new AddressTypeXmlMappings();

		public Xsd.AddressCapabilityAddressType GetExternalCode(OrgAddressCapabilityWrapper capability, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(capability.AddressCapabilityType, Xsd.AddressCapabilityAddressType.MAIN, errorContext, notifications);
		}

		protected override string GetExternalCodeCore(string enterpriseCode, string errorContext, INotifications notifications)
		{
			throw new NotSupportedException("Call the other overload");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Address Capability Type"; }
		}
	}
}
