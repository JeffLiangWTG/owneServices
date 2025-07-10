using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDEPRECRMessage : CMRCUSRESMessage
	{
		public CMRDEPRECRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.DEPREC;
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			ZString reference = GetReferenceFromSendersReference();
			return ForwardingConsol.LoadFromRef(Factory, reference)
				?? base.GetWrappedObject();
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (fEM_MessageInterpretation.IsEmpty && CUSRES != null)
				{
					fEM_MessageInterpretation = GetReport();
				}
				return fEM_MessageInterpretation;
			}
		}
		ZString fEM_MessageInterpretation;
	}
}
