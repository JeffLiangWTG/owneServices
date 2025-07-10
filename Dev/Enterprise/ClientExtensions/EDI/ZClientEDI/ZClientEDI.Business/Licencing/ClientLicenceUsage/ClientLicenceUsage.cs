using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class ClientLicenceUsage : AutoClientLicenceUsage
	{
		public ClientLicenceUsage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Staff")]
		public override ZGuid LX_LS
		{
			get { return base.LX_LS; }
			set { base.LX_LS = value; }
		}

		public ClientStaff Staff
		{
			get { return Factory.Load<ClientStaff>(LX_LS); }
		}

		[RelatedBusinessObject("Company")]
		public override ZGuid LX_LCC
		{
			get { return base.LX_LCC; }
			set { base.LX_LCC = value; }
		}

		public ClientCompany Company
		{
			get { return Factory.Load<ClientCompany>(LX_LCC); }
		}

		#endregion
	}
}

