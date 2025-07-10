using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRESMRMessage : CMRCUSRESMessage
	{
		public CMRESMRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.ESM;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return ForwardingConsol.LoadFromRef(Factory, reference)
				?? ExportCustomsManifestHeader.Load(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
