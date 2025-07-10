using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREMMRMessage : CMRCUSRESMessage
	{
		public CMREMMRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.EMM;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return ForwardingConsol.LoadFromRef(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
