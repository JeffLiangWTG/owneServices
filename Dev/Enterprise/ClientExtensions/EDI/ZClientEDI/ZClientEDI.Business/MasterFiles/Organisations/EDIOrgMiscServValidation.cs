using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgMiscServValidation : OrgMiscServValidation
	{
		public EDIOrgMiscServValidation(EDIOrgMiscServ parent)
			: base(parent)
		{
		}

		public new EDIOrgMiscServ Parent
		{
			get { return (EDIOrgMiscServ)base.Parent; }
		}
	}
}

