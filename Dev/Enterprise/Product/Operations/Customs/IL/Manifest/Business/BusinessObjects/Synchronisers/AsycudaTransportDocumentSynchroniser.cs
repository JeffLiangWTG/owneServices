using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaTransportDocumentSynchroniser : ASYCUDA.Business.AsycudaTransportDocumentSynchroniser
	{
		public AsycudaTransportDocumentSynchroniser(AsycudaTransportDocumentInfo destination, CusEntryNumber source) : base(destination, source)
		{
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_ReferenceNumberInfo, Source.CE_EntryNumInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.CSI_CodeInfo, GetCode, GetSourcevaluesAffectingCode));
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingCode()
		{
			yield return Source.CE_EntryTypeInfo;
		}

		IZType GetCode()
		{
			return (ZString)Constants.IsraeliCustoms.ManifestTransportContractDocumentId;
		}

		internal new AsycudaTransportDocumentInfo Destination => (AsycudaTransportDocumentInfo)base.Destination;

		internal new CusEntryNumber Source => (CusEntryNumber)base.Source;
	}
}
