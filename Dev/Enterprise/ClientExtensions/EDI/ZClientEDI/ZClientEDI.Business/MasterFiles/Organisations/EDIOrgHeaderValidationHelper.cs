using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common.Deduplication.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public static class EDIOrgHeaderValidationHelper
	{
		#region ValidateENTCode
		public static ZString ValidateENTCodeForOrganisationNode(ZNode<OrgHeader> parentNode, IEnumerable<OrgHeader> children, BusinessObjectFactory factory)
		{
			var warningMsg = new StringBuilder();
			warningMsg.AppendFormat(@"One or more organisations in your selection has a different enterprise license code than the parent organisation(s). 
As this is a rare scenario, please only proceed after verifying that it is correct. Following are the mismatches:

Parent Organisation:
{0}  Ent.Code: {1}
Child Orgnisations:
", parentNode.BizObj.NameAndCode, ((IEDIOrgHeader)parentNode.BizObj).LicenceEnterpriseCode);
			var validFlag = false;
			foreach (var child in children)
			{
				var reloadedChild = factory.Load<OrgHeader>(child.PK);
				var parentOrg = (IEDIOrgHeader)parentNode.BizObj;
				var childOrg = (IEDIOrgHeader)reloadedChild;

				if (CheckENTCodeMismatch(parentOrg.LicenceEnterpriseCode, childOrg.LicenceEnterpriseCode))
				{
					warningMsg.AppendLine(string.Format(@"{0}  Ent.Code: {1}", ((OrgHeader)childOrg).NameAndCode, childOrg.LicenceEnterpriseCode));
					validFlag = true;
				}
			}
			if (validFlag)
			{
				return warningMsg.ToString();
			}
			return string.Empty;
		}

		public static ZString ValidateENTCodeForOrganisation(EDIOrgHeader orgHeader)
		{
			var query = new ZDBOnlyQuery(typeof(OrgRelatedParty));
			query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, orgHeader.PK);
			var savedRelatedParties = orgHeader.Factory.Load<OrgRelatedParty>(query);
			var allRelatedParties = orgHeader.AllRelatedParties;

			foreach (var relatedParty in allRelatedParties.Cast<EDIOrgRelatedParty>())
			{
				if (!savedRelatedParties.Contains(relatedParty))
				{
					var warningMessage = ValidateENTCodeForRelatedParty(relatedParty);
					if (!string.IsNullOrEmpty(warningMessage))
					{
						return warningMessage;
					}
				}
			}
			return string.Empty;
		}

		public static ZString ValidateENTCodeForRelatedParty(EDIOrgRelatedParty relatedParty)
		{
			if (relatedParty.PR_PartyType == RelatedPartyTypeList.Codes.ManagementGrouping &&
				CheckENTCodeMismatch(relatedParty.EDIParentOrg?.LicenceEnterpriseCode, relatedParty.EDIRelatedParty?.LicenceEnterpriseCode))
			{
				return string.Format("An organization you are attempting to add has a different enterprise license code than {0}. As this is a rare scenario, please only proceed after verifying that it is correct.", relatedParty.EDIParentOrg.NameAndCode);
			}
			return string.Empty;
		}

		static bool CheckENTCodeMismatch(string parentENTCode, string childENTCode)
		{
			return !string.IsNullOrEmpty(parentENTCode) &&
				   !string.IsNullOrEmpty(childENTCode) &&
				   parentENTCode != childENTCode;
		}
		#endregion
	}
}
