using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer), RootElement("UbiquitousShipment")]
	public class UberShipment : ITopLevelDataObject, IDataObject
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.UberDataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.UberDataContext))]
		public IDataContextDataObject DataContext { get; set; }

		[ReferenceProperty, MaxLength(25)]
		public ZString? ReferenceData { get; set; }

		public UberCodeDescriptionPair JobType { get; set; }
		[CandidateKey]
		public ZInt? CandidateKeyField { get; set; }
		[Mandatory]
		public ZDecimal? ZZZMandatoryField { get; set; }
		public UberUNLOCO Destination { get; set; }
		public UberUNLOCO Origin { get; set; }
		public ZInt? PackageCount { get; set; }
		public UberPackingUnit PackageUnit { get; set; }
		[MaxLength(2048), AllowLineControlWhiteSpace]
		public ZString? UberBigField { get; set; }
		public ZBool? UberBoolean { get; set; }
		public ZDateTime? UberDateTime { get; set; }
		public ZDate? UberDate { get; set; }
		public ZDecimal? UberDecimal { get; set; }
		public ZInt? UberInteger { get; set; }
		public ZLong? UberLongInt { get; set; }
		public UberCodeDescriptionPairAlwaysAttributes UberMediumField { get; set; }
		public ZShort? UberShortIntField { get; set; }
		public ZByte? UberByteField { get; set; }
		[MaxLength(10)]
		public ZString? UberSmallField { get; set; }
		public TimeSpan? UberDuration { get; set; }

		public ZDecimal? Volume { get; set; }
		public UberUnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? Weight { get; set; }
		public UberUnitOfWeight WeightUnit { get; set; }
		public UberTransportMode? TransportMode { get; set; }
		public ZInt? OuterPacks { get; set; }
		public UberPackageType OuterPacksPackageType { get; set; }
		public UberIncoTerm ShipmentIncoTerm { get; set; }
		public ZDecimal? FreightRate { get; set; }
		public UberCurrency FreightRateCurrency { get; set; }
		[VerticalPartition]
		public UberOrder Order { get; set; }
		public UberChargeCode ChargeCode { get; set; }
		public UberServiceLevel ServiceLevel { get; set; }
		public UberEventType EventType { get; set; }
		public UberPancake Pancake { get; set; }

		[VerticalPartition]
		public UberInnerRelatedObject InnerRelation { get; set; }

		[Mandatory]
		public List<UberOrganization> OrganizationCollection { get; set; }

		public UberList<UberDate> DateCollection { get; set; }

		public UberList<UberXmlDate> XmlDateCollection { get; set; }

		public List<UberChildHeader> HeaderCollection { get; set; }

		public List<UberShipment> ShipmentCollection { get; set; }

		public List<UberContainer> ContainerCollection { get; set; }

		public List<UberOrganizationAddress> OrganizationAddressCollection { get; set; }

		IEnumerable<IMessageNumber> ITopLevelDataObject.MessageNumberCollection => throw new NotImplementedException();

		#region IDisposable Support

		bool disposed; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					foreach (var shipment in ShipmentCollection)
					{
						shipment?.Dispose();
					}
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void SetWriterStrategy(IDataObjectWriterStrategy strategy)
		{
		}

		void ISettableWriterStrategy.SetWriterStrategy(IDataObjectWriterStrategy strategy)
		{
			throw new NotImplementedException();
		}

		void ITopLevelDataObject.SetMessageNumber(MessageNumberType type, ZString value)
		{
			throw new NotImplementedException();
		}

		void IDisposable.Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public enum UberTransportMode { Sea, Air, Road, Rail, Storage }
}

