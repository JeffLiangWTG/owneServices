using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.YAS
{
	public static class YASExtentions
	{
		public static bool IsUnmatched(this JobDocAddress address)
		{
			return address != null && address.E2_CompanyName == "UNMATCHED ORGANISATION";
		}

		public static bool IsOwnerCodeAlreadyAutoMapped(ZString ownerCode, BusinessObjectFactory factory, ZGuid orgPK)
		{
			return OrgHeader.LoadFromForeignCode(factory, ownerCode, GlbCompany.CurrentCompany.OrgProxy) != null;
		}

		public static void MapOwnerCodeToOrganisation(BusinessObjectFactory factory, ZString foreignCode, ZGuid organisationPK)
		{
			OrgPatternMatchOverride orgMatch = factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_LocalGuid = organisationPK;
			orgMatch.OO_ForeignCode = foreignCode;
			orgMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
		}

		public static ZString GetOrgDetailsFromNote(Notes notes, UnmatchOrgRecords orgRecords, OrganisationTypes orgType, ZString orgSubType, bool isCompanyName, ZString noteKeyWord, int offsetOfText)
		{
			ZString result = ZString.Empty;

			var unmatchOrgRecordCriteria = new UnmatchOrgRecordCriteria { OrganisationType = orgType };
			unmatchOrgRecordCriteria.OrganisationSubType = (orgSubType.IsEmpty) ? orgType.ToString() : orgSubType.ToString();
			UnmatchOrgRecord unMatchOrgRecord = orgRecords.FindUnmatchOrg(unmatchOrgRecordCriteria);

			if (unMatchOrgRecord != null)
			{
				result = (isCompanyName) ? unMatchOrgRecord.OrganisationName : unMatchOrgRecord.OwnerCode;
			}
			else
			{
				StmNote[] unmatchOrgNotes = notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description);
				if (unmatchOrgNotes != null && unmatchOrgNotes.Length != 0)
				{
					ZString noteText = unmatchOrgNotes[0].ST_NoteText;
					int startPos = noteText.IndexOf(noteKeyWord);

					if (startPos != -1)
					{
						if (isCompanyName)
						{
							startPos += offsetOfText;
							result = noteText.Remove(0, startPos);
							int endPos = result.IndexOf("\r\n");
							result = result.Left(endPos);
						}
						else
						{
							result = noteText.Remove(0, startPos);
							int ownerCodePos = result.IndexOf("OWNERCODE:") + 11;
							result = result.Remove(0, ownerCodePos);
							int endPos = result.IndexOf("\r\n");
							result = result.Left(endPos);
						}
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static void CheckIfAutoCodeMappingShouldBeDone(OrgHeader organisation, BusinessObjectFactory factory, Notes notes, UnmatchOrgRecords unmatchOrgRecords, OrganisationTypes orgType, ZString orgSubType, bool isComapnyName, ZString searchKeyWord, ZString orgTypeInText, int offsetOfText)
		{
			ZString searchResult = GetOrgDetailsFromNote(notes, unmatchOrgRecords, orgType, orgSubType, isComapnyName, searchKeyWord, offsetOfText);
			ZString typeOfInfo = (isComapnyName) ? "name " : "Owner Code ";

			if (!searchResult.IsEmpty && !IsOwnerCodeAlreadyAutoMapped(searchResult, factory, organisation.PK))
			{
				if (searchResult.Length > 50)
				{
					Globals.Message.ShowInformation(ZString.Format("The organization {0}({1}) has more than 50 characters. It cannot be matched to this CargoWise One Code ({2}).",
							typeOfInfo, searchResult, organisation.OH_Code), " Organisation Mapping");
				}
				else if (Globals.Message.Show("Would you like to match the organization " + typeOfInfo + "(" + searchResult + ") to this CargoWise One Code (" + organisation.OH_Code + ")?",
					orgTypeInText + " Organisation Mapping", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					YASExtentions.MapOwnerCodeToOrganisation(factory, searchResult, organisation.PK);
				}
			}
		}

		public static ZGuid UnmatchedOrganisationPK
		{
			get { return Registry.Business.OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation; }
		}

		public static bool HasArrived(this TransportCollection collection)
		{
			if (collection != null && collection.Count > 0)
			{
				collection.Sort(JobConsolTransportSchema.Constants.JW_LegOrder);
				return !collection[collection.Count - 1].JW_ATA.IsEmpty;
			}
			return false;
		}

		#region Has invoice or cost been posted against shipment

		public static bool HasInvoiceOrCostBeenPosted(this ForwardingShipment shipment)
		{
			bool result = false;
			if (shipment.IsInDatabase && !shipment.JS_UniqueConsignRef.IsEmpty)
			{
				InvoicingBase[] invoices = shipment.Factory.Load<InvoicingBase>(CreateFilterForARAPInvoices(shipment.JS_UniqueConsignRef));
				result = (invoices != null && ((IList)invoices).Count > 0);
			}
			return result;
		}

		static ZQuery CreateFilterForARAPInvoices(ZString jobUniqueRef)
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(InvoicingBase));
			dbOnlyQuery.DefaultJoinCondition = JoinCondition.And;
			dbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			dbOnlyQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			dbOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			dbOnlyQuery.AddToFilter(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice), JoinCondition.Or, new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote)), JoinCondition.And);

			ZDBOnlySubQuery jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, jobUniqueRef);

			dbOnlyQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			query.AddToFilter(dbOnlyQuery);

			return query;
		}

		#endregion

		public static bool Match(ZString searchPattern, ZString eventReference)
		{
			//only '*' is treated as special character in this case. All others are treated as literal character
			var sb = new StringBuilder(searchPattern);
			sb.Replace(MultiCharWildcard, MultipleWildCardSubstitue);

			var escapedSearchPattern = Regex.Escape(sb.ToString());
			escapedSearchPattern = string.Concat(escapedSearchPattern.Replace(MultipleWildCardSubstitue, ".*"));
			return new Regex(string.Concat('^', escapedSearchPattern, '$'), RegexOptions.Singleline | RegexOptions.IgnoreCase).IsMatch(eventReference);
		}

		const string MultiCharWildcard = "*";
		const string MultipleWildCardSubstitue = "<<substitute1/>>";
	}
}
