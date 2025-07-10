using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public class EdiLicenceUsage : AutoEdiLicenceUsage
	{
		public EdiLicenceUsage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Staff")]
		public override ZGuid LX2_LS
		{
			get { return base.LX2_LS; }
			set { base.LX2_LS = value; }
		}

		public ClientStaff Staff
		{
			get { return Factory.Load<ClientStaff>(LX2_LS); }
		}

		[RelatedBusinessObject("Company")]
		public override ZGuid LX2_LCC
		{
			get { return base.LX2_LCC; }
			set { base.LX2_LCC = value; }
		}

		public ClientCompany Company
		{
			get { return Factory.Load<ClientCompany>(LX2_LCC); }
		}

		#endregion
	}
}

