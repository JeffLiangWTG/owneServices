using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSTREQRMessage : CMRCUSRESMessage
	{
		public CMRSTREQRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.STREQ;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return ExportCustomsManifestLines.Load(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
