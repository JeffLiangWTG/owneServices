using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREXRELMessage : CMRCUSRESMessage
	{
		public CMREXRELMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ForwardingShipment FindShipment()
		{
			ForwardingShipment result = null;
			var houseBill = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber);
			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			query.AddToFilter(JobShipmentSchema.JS_HouseBill, houseBill);
			var houseBills = Factory.Load<ForwardingShipment>(query);
			if (houseBills.Length == 1)
			{
				result = houseBills[0];
			}
			else if (houseBills.Length > 1)
			{
				var houseBillCAN = GetReference(CUSRES.Group3, "CDI");
				var houseBillExemption = GetReference(CUSRES.Group3, "EEC");
				var houseBillContingecy = GetReference(CUSRES.Group3, "CC");
				foreach (ForwardingShipment hb in houseBills)
				{
					if (!houseBillCAN.IsEmpty)
					{
						var cANQuery = new ZDBOnlyQuery(typeof(AUCusEntryNumber));
						cANQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, hb.PK);
						cANQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
						cANQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, houseBillCAN);
						cANQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CANType.CustomsAuthorityNumber.Code);
						cANQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
						var canEntryNum = Factory.LoadTop1<AUCusEntryNumber>(cANQuery);
						if (canEntryNum != null)
						{
							if (canEntryNum.CE_ParentID == hb.PK)
							{
								result = hb;
								break;
							}
						}
					}

					if (!houseBillExemption.IsEmpty)
					{
						var exemptionQuery = new ZDBOnlyQuery(typeof(AUCusEntryNumber));
						exemptionQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, hb.PK);
						exemptionQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
						exemptionQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, houseBillExemption.SubstringSafe(1, 3));
						exemptionQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
						var entryNum = Factory.LoadTop1<AUCusEntryNumber>(exemptionQuery);
						if (entryNum != null)
						{
							result = hb;
							break;
						}
					}

					if (!houseBillContingecy.IsEmpty)
					{
						var contingencyQuery = new ZDBOnlyQuery(typeof(AUCusEntryNumber));
						contingencyQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, hb.PK);
						contingencyQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
						contingencyQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, houseBillContingecy);
						contingencyQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CANType.ContingencyCustomsAuthorityNumber.Code);
						contingencyQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
						var entryNum = Factory.LoadTop1<AUCusEntryNumber>(contingencyQuery);
						if (entryNum != null)
						{
							result = hb;
							break;
						}
					}
				}
			}

			return result;
		}

		ZQuery GetConsolEXRELReference(ZString reference)
		{
			var query = new ZDBOnlyQuery(typeof(ForwardingConsol));

			var subQuery = new ZDBOnlySubQuery(typeof(AUCusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, reference);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetExportCustomsManifestHeaderEXRELReference(ZString reference)
		{
			return new ZQuery(ExportCustomsManifestHeaderSchema.ED_CAN, reference);
		}

		protected override internal BusinessObject GetWrappedObject()
		{
			BusinessObject result = FindShipment();
			if (result == null)
			{
				ZString reference = GetReference(CUSRES.Group3, ReferenceFunctionCodeQualifierList.ConveyanceReferenceNumber);
				result = Factory.LoadTop1<ForwardingConsol>(GetConsolEXRELReference(reference));
				if (result == null)
				{
					result = Factory.LoadTop1<ExportCustomsManifestHeader>(GetExportCustomsManifestHeaderEXRELReference(reference));
				}

				if (result == null)
				{
					result = base.GetWrappedObject();
				}
			}

			return result;
		}

		protected override ZString GetStatusCore()
		{
			return CMRMessageStatusDescription.CLEAR;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessage.CMRMessageTypes.EXREL;
		}

		public override bool SupportsHTMLResponseEmails
		{
			get { return true; }
		}
	}
}
