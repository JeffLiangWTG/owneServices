using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgRelatedParty : OrgRelatedParty
	{
		public EDIOrgRelatedParty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.AllPartyTypeList")]
		public override ZString PR_PartyType
		{
			get { return base.PR_PartyType; }
			set
			{
				base.PR_PartyType = value;
				if (value == EDIOrgRelatedPartyLookups.ContractingPartyCode)
				{
					PR_FreightDirection = ZString.Empty;
				}
			}
		}

		public EDIOrgHeader EDIParentOrg
		{
			get { return ((EDIOrgHeader)Parent); }
		}

		public EDIOrgHeader EDIRelatedParty => Factory.Load<EDIOrgHeader>(PR_OH_RelatedParty);

		protected override OrgRelatedPartyValidation GetNewValidation()
		{
			return new EDIOrgRelatedPartyValidation(this);
		}

		protected override OrgRelatedPartyLookups GetNewLookups()
		{
			return new EDIOrgRelatedPartyLookups(this);
		}
	}
}

