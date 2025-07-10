using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class BaseCLREGResponseMessage : CMRCUSRESMessage
	{
		public BaseCLREGResponseMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = null;
			ZString reference = GetReferenceFromSendersReference();
			if (!reference.IsEmpty)
			{
				var genAddOn = Factory.LoadTop1<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Data, reference));
				if (genAddOn != null)
				{
					result = Factory.Load<OrgHeader>(genAddOn.XA_ParentID);
				}
			}
			if (result == null)
			{
				result = base.GetWrappedObject();
			}
			return result;
		}
	}
}
