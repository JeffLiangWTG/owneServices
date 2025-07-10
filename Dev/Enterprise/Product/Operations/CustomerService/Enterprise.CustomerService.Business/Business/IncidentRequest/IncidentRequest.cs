using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CustomerService.Business
{
	[CodeProperty(Schema.INC_IncidentNumber)]
	public class IncidentRequest : AutoIncidentRequest, IDocManagerSupport
	{
		public IncidentRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = CreateDocManagerInfo();
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		protected virtual DocManagerInfo CreateDocManagerInfo() => new DocManagerInfo(this, Core.Constants.DocManagerCodes.IncidentRequest);

		#region Saving

		public override void OnSaving()
		{
			PopulateIncidentNumber();
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					INC_IncidentNumber = ZString.Empty;
				}
			}

			base.OnSaved(saveSucceeded);
		}

		public void PopulateIncidentNumber()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(INC_IncidentNumberInfo, Env.NumberFountains.CustomerServiceIncidentNo);
			}
		}

		#endregion
	}
}
