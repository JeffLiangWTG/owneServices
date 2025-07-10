using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashDocumentText : AutoDashDocumentText, IDashDocumentText
	{
		public DashDocumentText(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
