using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucalGenralResponseMaker_OrgHeader : LucalGenralResponseMaker_DUCR
	{
		public LucalGenralResponseMaker_OrgHeader(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects, EDIMessage inboundEdiMessage, LucasGenralEnquiryHandler.EnquiryObjectTypes objectRequestType, ZString commonAccessReference)
			: base(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference)
		{
			this.objectRequestType = objectRequestType;
			this.requestText = requestText;
		}

		internal override void MakeAndSendAllResponses()
		{
			var entriesForThisOrgs = GetEntriesForOrgs();
			if (entriesForThisOrgs.Length == 0)
			{
				var failure = new LucalGenralResponseMaker_Failure("REQUEST REJECTED - CLIENT FOUND BUT NO CONSIGNMENTS MATCH", inboundEdiMessage, requestText, commonAccessReference);
				failure.MakeAndSendAllResponses();
				return;
			}
			else if (entriesForThisOrgs.Length == 1)
			{
				mainBusinessObject = entriesForThisOrgs[0];
				childrenBusinessObjects = System.Array.Empty<IBusiness>();
				base.MakeAndSendAllResponses();
			}
			else
			{
				childrenBusinessObjects = entriesForThisOrgs;
				base.MakeAndSendAllResponses();
			}
		}

		CusEntryHeader[] GetEntriesForOrgs()
		{
			var column = objectRequestType == LucasGenralEnquiryHandler.EnquiryObjectTypes.CNSE ? JobDeclarationSchema.JE_OH_Importer :
						(objectRequestType == LucasGenralEnquiryHandler.EnquiryObjectTypes.SHPR ? JobDeclarationSchema.JE_OH_Supplier : null);
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var sub = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			sub.AddToFilter(column, (from BusinessObject bizO in childrenBusinessObjects select bizO.PK));
			sub.AddToFilter(JobDeclarationSchema.JE_RL_NKOrigin, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
			sub.AddToFilter(JobDeclarationSchema.JE_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddMonths(-12));
			query.AddSubQuery(sub, JoinCondition.And);
			query.OrderBy = CusEntryHeaderSchema.CH_BGMReference.Name;
			return inboundEdiMessage.Factory.Load<CusEntryHeader>(query);
		}

		readonly LucasGenralEnquiryHandler.EnquiryObjectTypes objectRequestType;
		readonly string requestText;
	}
}
