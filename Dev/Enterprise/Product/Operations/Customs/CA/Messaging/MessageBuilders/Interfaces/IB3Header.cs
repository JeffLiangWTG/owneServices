using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
namespace Enterprise.Customs.CA.Messaging
{
	public interface IB3HeaderWithScheduledMessageSupport : IB3Header
	{
		void CancelAndDeactivateScheduledB3Message();
		void PopulateEntrySubmittedDateIfRequired(ZDateTime? scheduledTime);
	}

	public interface IB3Header : ICAEDIFACTMessageAttachee
	{
		ZString MessageType { get; }
		ZString BatchNumber { get; }
		ZString B3TypeCode { get; }
		ZString PaymentCode { get; }
		ZString CBSAOffice { get; }
		ZString PortOfUnlading { get; }
		ZString WarehouseNumber { get; }
		ZString TransactionNumber { get; }
		ZString BusinessNumber { get; }
		ZString GSTNumber { get; }
		ZString TransportMode { get; }
		ZString CarrierCodeAtImportation { get; }
		IEnumerable<IB3BRelease> B3BInputReleases { get; }
		ZDecimal TotalValueForDuty { get; }
		IEnumerable<IB3SubHeader> PositiveB3SubHeaders { get; }
		IEnumerable<IClassificationLine1> PositiveClassificationLines { get; }
		IEnumerable<IB3SubHeader> NegativeB3SubHeaders { get; }
		IEnumerable<IClassificationLine1> NegativeClassificationLines { get; }
		ITotalAmounts PositiveTotalAmounts { get; }
		ITotalAmounts NegativeTotalAmounts { get; }
		bool IsCalculationsDone { get; }
		bool SumPosAndNeg { get; }

		//B3 Document
		ZString B3Comments { get; }
		ZDateTime ReleaseDate { get; }
		IDocAddress Importer { get; }
		ZString AccountSecurityCode { get; }

		void RefreshCachedValues();
	}
}
