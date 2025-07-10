using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsBillAdditionalDocument : EU.NCTS.Business.NctsBillAdditionalDocument
	{
		public NctsBillAdditionalDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new NctsBillAdditionalDocumentLookups Lookups => (NctsBillAdditionalDocumentLookups)base.Lookups;

		[MaxLength(nameof(CSI_ReferenceNumber_MaxLength))]

		[ReadOnlyMember(nameof(ReadOnlyProvider) + "." + nameof(IAdditionalDocumentReadOnlyProvider.ReferenceNumberReadOnly))]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		int CSI_ReferenceNumber_MaxLength =>
			Header is EU.NCTS.Business.NctsHeader header && header.IsPhase5Departure && (CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument || CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference)
			? header.IsInPhase5TransitionPeriod ? 35 : 70
			: AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength;

		NctsBill Bill => Parent as NctsBill;

		bool IsPhase5Arrival => Bill?.Header.IsPhase5Arrival ?? false;

		protected override CusSupportingInfoValidation GetNewValidation() =>
			IsPhase5Arrival
				? new CusSupportingInfoDisabledValidation(this)
				: new NctsBillAdditionalDocumentValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new NctsBillAdditionalDocumentLookups(this);

		#region ReadOnlyProvider

		protected override IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
		{
			var baseReadOnlyProvider = base.GetNewReadOnlyProvider();
			return new NctsBillAdditionalDocumentReadOnlyProvider(this, baseReadOnlyProvider);
		}

		#endregion
	}
}
