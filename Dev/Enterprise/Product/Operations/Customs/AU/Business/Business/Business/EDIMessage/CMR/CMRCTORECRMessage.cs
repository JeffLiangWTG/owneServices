using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCTORECRMessage : CMRCUSRESMessage
	{
		public CMRCTORECRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.CTOREC;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return JobDeclaration.LoadFirstMatchingInCurrentCompanyIncludingInActive(Factory, reference)
				?? base.GetWrappedObject();
		}
	}
}
