using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using Gb = Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class FindCusEntryHeaderFromAwbAndAgentReferenceHelper
	{
		public FindCusEntryHeaderFromAwbAndAgentReferenceHelper(ICcsukCusAwb awb, string agentReference = "")
		{
			this.awb = awb;
			this.agentReference = agentReference;
		}

		public CusEntryHeader GetEntryFromAwbAsBestWeCan()
		{
			var result = GetEntryFromAwbByFK() ?? GetEntryFromAwbByNaturalKey();

			return result;
		}

		CusEntryHeader GetEntryFromAwbByFK()
		{
			CusEntryHeader entry = null;
			JobDeclaration dec = null;

			// First try with foreign keys....
			var mawbOrBasic = awb as CusMAWB;
			var hawb = awb as CusHAWB;
			var split = awb as SplitConsignment;
			if (mawbOrBasic != null && mawbOrBasic.IsBasic)
			{
				dec = awb.Factory.Load<JobDeclaration>(mawbOrBasic.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry);
				if (dec == null && mawbOrBasic.Consol != null && mawbOrBasic.Consol.IsDirect && mawbOrBasic.Consol.Shipments.Count == 1)
				{
					dec = mawbOrBasic.Consol.Shipments[0].Declarations.OfType<JobDeclaration>().FirstOrDefault();
				}
			}
			else if (hawb != null)
			{
				dec = hawb.Declaration;
				if (dec == null && hawb.Shipment != null && hawb.Shipment.Declarations.Length == 1)
				{
					dec = hawb.Shipment.Declarations.OfType<JobDeclaration>().FirstOrDefault();
				}
			}
			else if (split != null)
			{
				dec = split.OwnDeclaration;
				if (dec == null)
				{
					// Split doesn't have its own dec, but its hawb's shipment's declaration will have multiple entry headers, one for each split
					var splitHouse = split as SplitHouse;
					var splitBasic = split as SplitBasic;
					if (splitHouse != null && splitHouse.HAWB != null)
					{
						dec = splitHouse.HAWB.Declaration;
					}
					else if (splitBasic != null && splitBasic.Basic != null)
					{
						dec = splitBasic.Basic.MasterLevelHouseHelper.Declaration;
					}

					if (dec != null && dec.ActiveEntryHeaders.Count > 0)
					{
						entry = dec.ActiveEntryHeaders.Count > 1
								? (from CusEntryHeader e
									   in dec.ActiveEntryHeaders
								   where e.RandomHeader.ZG_HouseSplitReference == split.SplitReference
								   select e).FirstOrDefault()
								: dec.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
					}
				}
			}

			if (dec != null && entry == null)
			{
				entry = (from CusEntryHeader e in dec.ActiveEntryHeaders select e).FirstOrDefault();
			}

			return entry;
		}

		CusEntryHeader GetEntryFromAwbByNaturalKey()
		{
			CusEntryHeader entry = null;

			// Try an inventory reference match
			var acpPrefix = new MasterUCRCalculator().GetAirportPrefixFromSixCharShedCode(awb.CargoTerminalOperatorAirportAndShed, awb.Factory);
			var masterUcr = acpPrefix + awb.CargoTerminalOperator + awb.ChiefMasterUCRReferenceSuffix;
			var entryNumbers = CusEntryNumber.Load(awb.Factory, CusEntryNumberTypes.EU.MasterUCR, masterUcr, Core.Constants.CountryCodes.UnitedKingdom);
			if (entryNumbers != null)
			{
				var entryNumber = (from CusEntryNumber cen in entryNumbers where cen.CE_ParentTable == CusEntryHeader.Schema.TableName select cen).FirstOrDefault() ?? (from CusEntryNumber cen in entryNumbers where cen.CE_ParentTable == Gb.JobDeclaration.Schema.TableName select cen).FirstOrDefault();
				if (entryNumber != null)
				{
					if (entryNumber.CE_ParentTable == CusEntryHeader.Schema.TableName)
					{
						entry = entryNumber.Factory.Load<CusEntryHeader>(entryNumber.CE_ParentID);
					}
					else if (entryNumber.CE_ParentTable == Gb.JobDeclaration.Schema.TableName)
					{
						var dec = entryNumber.Factory.Load<JobDeclaration>(entryNumber.CE_ParentID);
						entry = dec != null && dec.ActiveEntryHeaders.Count == 1 ? dec.ActiveEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault() : null;
					}
				}
			}
			// Finally try the B0001000 number in the FSN (allow exactly one match only)			
			if (entry == null)
			{
				if (!agentReference.IsEmpty)
				{
					var query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, agentReference);
					query.AddToFilter(GbInterchangeSender.BritishBranches(awb.Factory, JobDeclarationSchema.JE_GB));
					var decs = awb.Factory.Load<JobDeclaration>(query);
					if (decs.Length != 1)
					{
						query = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, SQLComparisonOperator.EndsWith, agentReference); // CCSUK return only the leftmost 8 chars that go in the box 7 reference, so we can send the rightmost. i.e. for B00010001 we send 00010001, otherwise we'd get back only B0001000
						query.AddToFilter(GbInterchangeSender.BritishBranches(awb.Factory, JobDeclarationSchema.JE_GB));
						decs = awb.Factory.Load<JobDeclaration>(query);
						if (decs.Length == 1 && decs[0].ActiveEntryHeaders.Count == 1)
						{
							entry = (CusEntryHeader)decs[0].ActiveEntryHeaders[0];
						}
					}
					else
					{
						entry = decs[0].ActiveEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault();
					}
				}
			}

			return entry;
		}

		readonly ICcsukCusAwb awb;
		readonly ZString agentReference;
	}
}
