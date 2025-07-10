using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSEACRRMessage : CMRCUSRESMessage
	{
		public CMRSEACRRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference(SendersReference);

			if (!reference.IsEmpty)
			{
				result = CusSCAHouse.Load(Factory, reference);
			}
			if (result == null)
			{
				result = base.GetWrappedObject();
			}

			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.SEACR;
		}

		public override bool SupportsHTMLResponseEmails
		{
			get { return true; }
		}
	}
}
