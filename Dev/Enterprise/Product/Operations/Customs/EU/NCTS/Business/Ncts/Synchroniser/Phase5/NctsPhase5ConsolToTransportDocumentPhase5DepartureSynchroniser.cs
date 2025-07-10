using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Freight.Forwarding.Business;
using SupportingDocumentTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.SupportingDocumentTypes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5ConsolToTransportDocumentPhase5DepartureSynchroniser : BusinessObjectSynchroniser
	{
		public NctsPhase5ConsolToTransportDocumentPhase5DepartureSynchroniser(CusSupportingInfo transportDocument, ForwardingConsol source)
			: base(transportDocument, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_SubTypeInfo, GetSubTypeConverted, GetMasterBillNumInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_CodeInfo, GetCodeConverted, GetMasterBillNumInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_ReferenceNumberInfo, Source.JK_MasterBillNumInfo));
		}

		internal new CusSupportingInfo Destination => (CusSupportingInfo)base.Destination;

		internal new ForwardingConsol Source => (ForwardingConsol)base.Source;

		IZType GetSubTypeConverted() => new ZString(AdditionalInfoSubTypeList.Codes.TransportDocument);

		IZType GetCodeConverted() => new ZString(Source.JK_TransportMode == TransportTypeList.Codes.Air ? SupportingDocumentTypes.N741 : SupportingDocumentTypes.N705);

		IEnumerable<ZPropertyInfo> GetMasterBillNumInfos()
		{
			yield return Source.JK_MasterBillNumInfo;
		}
	}
}
