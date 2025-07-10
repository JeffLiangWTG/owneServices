using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDEPARTRMessage : CMRCUSRESMessage
	{
		public CMRDEPARTRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.DEPART;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return ExportCustomsManifestHeader.Load(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
