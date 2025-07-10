using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Edifact.D99B.Elements;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREXDRMessage : CMRCUSRESMessage
	{
		public CMREXDRMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString ExportDeclarationNumber
		{
			get
			{
				return GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.ExportDeclaration);
			}
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference();
			if (!reference.IsEmpty)
			{
				var declaration = Factory.LoadTop1<JobDeclaration>(JobDeclarationFilter.ForDeclarationReference(true, reference));
				result = declaration?.ActiveEntryHeaders.FirstOrDefault() ?? declaration;
			}
			if (result == null)
			{
				result = base.GetWrappedObject();
			}
			return result;
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessage.CMRMessageTypes.EXDR;
		}

		#endregion
	}
}
