using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class JobDeclarationAndShipmentFromHawbLookerUpper
	{
		public JobDeclarationAndShipmentFromHawbLookerUpper(CusHAWB hawb, BusinessObjectFactory factory, EDIMessage ediMessage)
		{
			this.hawb = hawb;
			this.factory = factory;
			this.ediMessage = ediMessage;
		}

		public JobDeclaration FindDeclaration()
		{
			JobDeclaration dec = null;
			if (!hawb.CS_JE_CustomsFormalEntry.IsEmpty)
			{
				dec = factory.Load<JobDeclaration>(hawb.CS_JE_CustomsFormalEntry);
				hawb.CS_JS = !dec.JE_JS.IsEmpty && hawb.CS_JS.IsEmpty ? dec.JE_JS : hawb.CS_JS;
			}
			if (dec == null)
			{
				var query1 = new ZQuery();
				query1.AddToFilter(JobDeclarationSchema.JE_MasterBill, hawb.MAWB.CM_MAWB);
				query1.AddToFilter(JobDeclarationSchema.JE_HouseBill, hawb.CS_HAWB);
				var dateQuery = new ZQuery(JobDeclarationSchema.JE_DateOfFirstArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddYears(-1));
				dateQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_SystemCreateTimeUtc, ZDateTime.Now.AddYears(-1)), JoinCondition.Or);
				query1.AddToFilter(dateQuery);
				dec = factory.LoadTop1<JobDeclaration>(query1);
				if (dec == null)
				{
					var shipment = factory.Load<ForwardingShipment>(hawb.CS_JS);
					if (shipment == null)
					{
						shipment = ParserHelper.FindBritishShipmentWithThisHawbNumberAndOnThisMasterAndMaybeOriginToo(hawb.CS_HAWB, hawb.MAWB.CM_MAWB, hawb.MAWB.AirportOfOrigin, ediMessage);
						if (shipment == null)
						{
							dec = CreateNewImportDeclaration();
							hawb.CS_JE_CustomsFormalEntry = dec.PK;
						}
						else
						{
							dec = FindGBDecFromShipmentOrMakeNewOne(hawb, shipment, dec);
						}
					}
					else
					{
						dec = FindGBDecFromShipmentOrMakeNewOne(hawb, shipment, dec);
					}
				}
				else
				{
					hawb.CS_JS = hawb.CS_JS.IsEmpty ? dec.JE_JS : hawb.CS_JS;
					hawb.CS_JE_CustomsFormalEntry = hawb.CS_JE_CustomsFormalEntry.IsEmpty ? dec.PK : hawb.CS_JE_CustomsFormalEntry;
				}
			}
			return dec;
		}

		JobDeclaration FindGBDecFromShipmentOrMakeNewOne(CusHAWB hawb, ForwardingShipment shipment, JobDeclaration dec)
		{
			var result = (from JobDeclaration d in shipment.Declarations.OfType<JobDeclaration>() where d.CountryCode == Core.Constants.CountryCodes.UnitedKingdom select d);
			dec = result.FirstOrDefault();
			if (dec == null)
			{
				dec = CreateNewImportDeclaration();
				hawb.CS_JE_CustomsFormalEntry = dec.PK;
				dec.JE_JS = shipment.PK;
				hawb.CS_JS = shipment.PK;
			}
			return dec;
		}

		JobDeclaration CreateNewImportDeclaration()
		{
			var dec = factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_DeclarationType = "";
			dec.JE_EntryStyle = "IM";
			dec.JE_EntrySubStyle = "";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		readonly CusHAWB hawb;
		readonly EDIMessage ediMessage;
		readonly BusinessObjectFactory factory;
	}
}
