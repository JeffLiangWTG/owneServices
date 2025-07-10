using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class StatusRequestDocManagerInfo : DocManagerInfo
	{
		public StatusRequestDocManagerInfo(BusinessObject parent)
			: base(parent, Core.Constants.DocManagerCodes.EDIMessage)
		{
		}

		public new StatusRequest BusinessEntity => (StatusRequest)base.BusinessEntity;

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = base.GetRelatedObjects().ToList();
			result.AddRange(BusinessEntity.ResponseMessages);
			return result.ToArray();
		}
	}
}
