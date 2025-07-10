using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitHeaderDocManagerInfo :
		DocManagerInfo,
		Freight.Business.ShipmentDocManagerInfo.IHaveEDocsChildren
	{
		public CusExitHeaderDocManagerInfo(CusExitHeader cusExitHeader)
			: base(cusExitHeader, Core.Constants.DocManagerCodes.CustomsExitHeader)
		{
		}

		public BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay()
		{
			var businessObjects = new List<BusinessObject>();
			businessObjects.AddRange(CusExitHeader.CusExitConsignments);
			businessObjects.AddRange(CusExitHeader.CusExitReports);
			return businessObjects.ToArray();
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();

			foreach (var baseMessage in CusExitHeader.CusExitConsignments)
			{
				list.Add(baseMessage.Factory.Load<CusExitConsignment>(baseMessage.PK));
			}
			list.AddRange(CusExitHeader.CusExitConsignments);

			foreach (var cusExitReport in CusExitHeader.CusExitReports)
			{
				list.Add(cusExitReport.Factory.Load<CusExitReport>(cusExitReport.PK));
			}
			list.AddRange(CusExitHeader.CusExitReports);

			return list.ToArray();
		}

		CusExitHeader CusExitHeader => (CusExitHeader)BusinessEntity;
	}
}
