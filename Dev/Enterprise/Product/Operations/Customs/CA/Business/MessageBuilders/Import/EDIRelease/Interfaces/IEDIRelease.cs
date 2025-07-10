namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.MasterFiles.Integration;

	public interface IEDIReleaseMin : ICAEDIFACTMessageAttachee
	{
		// BGM
		ZString TransactionNumber { get; }

		// CST
		ZString ServiceOptionID { get; }
		ZString AssessmentOption { get; }
		ZString ImporterNumber { get; }
		ZString PriorityIndicator { get; }

		// LOC
		ZString PortOfClearance { get; }
		ZString GoodsLocationCode { get; }
		ZString GoodsLocationName { get; }

		// DTM
		ZDateTime DateOfArrival { get; }
		ZDateTime DateOfDeparture { get; }

		// MEA
		ZDecimal GrossWeight { get; }
		ZString GrossWeightUnits { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUnits { get; }

		// EQD
		ZString[] ContainerNumbers { get; }

		// RFF
		ZString[] CargoControlNumbers { get; }

		// PAC
		ZInt[] NumberOfPackages { get; }
		ZString[] TypeOfPackages { get; }

		// NAD
		IDocAddress Importer { get; }
		IDocAddress Carrier { get; }
		IDocAddress Broker { get; }
		IDocAddress DeliveryAddress { get; }

		// TOD
		ZString DeliveryInstructions { get; }

		// MOA
		ZDecimal TotalValueForDuty { get; }

		// G10
		IEnumerable<IEDIInvoiceOGD> Invoices { get; }
	}

	public interface IEDIReleaseOGD : IEDIReleaseMin
	{
		// OGD Data
		ZBool OGDCFIA { get; }
		ZBool OGDIC { get; }
		ZBool OGDNR { get; }
		ZBool OGDTC { get; }
		ZString DeliveryPhone { get; }
		ZString DeliveryFax { get; }
	}
}
