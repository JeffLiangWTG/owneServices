using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryInstruction = Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public static class CcsukFindBusinessObjectHelper
	{
		#region Find Hawb from CusEntryInstruction

		public static CusHAWB FindHawbFromEntryInstruction(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			CusHAWB hawb = null;

			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null)
				{
					hawb = FindHawbUsingLinkedDeclaration(factory, declaration);
					if (hawb == null)
					{
						hawb = FindHawbUsingLinkedShipment(factory, declaration);
					}
					if (hawb == null)
					{
						hawb = FindHawbsUsingLinkedNaturalKeys(factory, declaration).FirstOrDefault();
					}
				}
			}
			return hawb;
		}

		public static CusHAWB FindHawbUsingLinkedDeclaration(BusinessObjectFactory factory, JobDeclaration declaration)
		{
			CusHAWB hawb = null;
			var hawbs = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, declaration.PK));
			if (hawbs != null && hawbs.Length > 0)
			{
				hawb = hawbs.FirstOrDefault(h => h.MAWB != null && h.MAWB.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk);
			}
			return hawb;
		}

		static CusHAWB FindHawbUsingLinkedShipment(BusinessObjectFactory factory, JobDeclaration declaration)
		{
			CusHAWB hawb = null;
			var shipment = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, declaration.JE_JS)).FirstOrDefault();
			if (shipment != null)
			{
				hawb = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK)).FirstOrDefault();
			}
			return hawb;
		}

		static List<CusHAWB> FindHawbsUsingLinkedNaturalKeys(BusinessObjectFactory factory, JobDeclaration declaration, bool isMasterHouse = false)
		{
			var hawbsMatchingMasterBillQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			hawbsMatchingMasterBillQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, isMasterHouse);
			var mawbSubQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			mawbSubQuery.AddToFilter(CusMAWBSchema.CM_MAWB, declaration.JE_MasterBill);
			hawbsMatchingMasterBillQuery.AddSubQuery(mawbSubQuery, JoinCondition.And);
			var hawbsMatchingMasterBill = factory.Load<CusHAWB>(hawbsMatchingMasterBillQuery);

			var hawbsMatchingHouseBill = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, declaration.JE_HouseBill));
			var hawbsMatchingSubLocationOfGoods = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_WarehouseLocation, declaration.JE_SubLocationOfGoods));

			var hawbsMatchingMasterBillPKs = hawbsMatchingMasterBill.Select(h => h.PK).ToList();
			var hawbsMatchingHouseBillPKs = hawbsMatchingHouseBill.Select(h => h.PK).ToList();
			var hawbsMatchingSubLocationOfGoodsPKs = hawbsMatchingSubLocationOfGoods.Select(h => h.PK).ToList();

			var matchingPKs = hawbsMatchingMasterBillPKs
				.Intersect(hawbsMatchingHouseBillPKs)
				.Intersect(hawbsMatchingSubLocationOfGoodsPKs)
				.ToList();

			return hawbsMatchingHouseBill.Where(h => matchingPKs.Contains(h.PK)).ToList();
		}

		#endregion

		#region Find SplitHouse from CusEntryInstruction

		public static SplitHouse FindSplitHouseFromEntryInstruction(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitHouse splitHouse = null;
			splitHouse = FindSplitHouseUsingLinkedDeclaration(factory, cei);
			if (splitHouse == null)
			{
				splitHouse = FindSplitHouseUsingLinkedShipment(factory, cei);
			}
			if (splitHouse == null)
			{
				splitHouse = FindSplitHouseUsingLinkedNaturalKeys(factory, cei);
			}
			return splitHouse;
		}

		static SplitHouse FindSplitHouseUsingLinkedDeclaration(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitHouse splitHouse = null;
			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null)
				{
					var hawbs = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, declaration.PK));
					splitHouse = GetSplitHouseFromUkHawbs(factory, cei, hawbs);
				}
			}
			return splitHouse;
		}

		static SplitHouse GetSplitHouseFromUkHawbs(BusinessObjectFactory factory, CusEntryInstruction cei, CusHAWB[] hawbs)
		{
			SplitHouse splitHouse = null;

			if (hawbs != null && hawbs.Length > 0)
			{
				var ukHawb = (from CusHAWB h in hawbs where h.MAWB != null && h.MAWB.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk && !h.CS_IsMasterHouse select h).FirstOrDefault();
				if (ukHawb != null)
				{
					var splits = Array.Empty<CusPartShip>();
					splits = factory.Load<SplitHouse>(new ZQuery(CusPartShipSchema.CG_CS, ukHawb.PK));
					splitHouse = (from SplitHouse s in splits where s.CG_MessageReference == cei.CEI_SplitReference select s).FirstOrDefault();
				}
			}
			return splitHouse;
		}

		static SplitHouse FindSplitHouseUsingLinkedShipment(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitHouse splitHouse = null;
			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null)
				{
					var shipment = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, declaration.JE_JS)).FirstOrDefault();
					if (shipment != null)
					{
						var hawbs = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK));
						splitHouse = GetSplitHouseFromUkHawbs(factory, cei, hawbs);
					}
				}
			}
			return splitHouse;
		}

		static SplitHouse FindSplitHouseUsingLinkedNaturalKeys(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitHouse splitHouse = null;
			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null)
				{
					var hawbs = FindHawbsUsingLinkedNaturalKeys(factory, declaration).ToArray();
					splitHouse = GetSplitHouseFromUkHawbs(factory, cei, hawbs);
				}
			}
			return splitHouse;
		}

		#endregion

		#region Find SplitBasic from CusEntryInstruction

		public static SplitBasic FindSplitBasicFromEntryInstruction(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitBasic splitBasic = null;
			splitBasic = FindSplitBasicUsingLinkedDeclaration(factory, cei);
			if (splitBasic == null)
			{
				splitBasic = FindSplitBasicUsingLinkedShipment(factory, cei);
			}
			if (splitBasic == null)
			{
				splitBasic = FindSplitBasicUsingLinkedNaturalKeys(factory, cei);
			}
			return splitBasic;
		}

		static SplitBasic FindSplitBasicUsingLinkedDeclaration(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitBasic splitBasic = null;

			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null && declaration.JE_HouseBill == ZString.Empty)
				{
					var hawbs = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, declaration.PK));
					splitBasic = GetSplitBasicFromUkHawbs(factory, cei, hawbs);
				}
			}

			return splitBasic;
		}

		static SplitBasic GetSplitBasicFromUkHawbs(BusinessObjectFactory factory, CusEntryInstruction cei, CusHAWB[] hawbs)
		{
			SplitBasic splitBasic = null;

			if (hawbs != null && hawbs.Length > 0)
			{
				var ukHawb = (from CusHAWB h in hawbs where h.MAWB != null && h.MAWB.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk && h.CS_HAWB == "" && h.CS_IsMasterHouse select h).FirstOrDefault();
				if (ukHawb != null)
				{
					splitBasic = factory.Load<SplitBasic>(new ZQuery(CusPartShipSchema.CG_CM_LinkToPartMaster, ukHawb.MAWB.PK)).FirstOrDefault();
				}
			}
			return splitBasic;
		}

		static SplitBasic FindSplitBasicUsingLinkedShipment(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitBasic splitBasic = null;

			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null && declaration.JE_HouseBill == ZString.Empty)
				{
					var shipment = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, declaration.JE_JS)).FirstOrDefault();
					if (shipment != null)
					{
						var hawbs = factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK));
						splitBasic = GetSplitBasicFromUkHawbs(factory, cei, hawbs);
					}
				}
			}
			return splitBasic;
		}

		static SplitBasic FindSplitBasicUsingLinkedNaturalKeys(BusinessObjectFactory factory, CusEntryInstruction cei)
		{
			SplitBasic splitBasic = null;
			if (cei != null)
			{
				var declaration = cei.JobDeclaration;
				if (declaration != null && declaration.JE_HouseBill == ZString.Empty)
				{
					var hawbs = FindHawbsUsingLinkedNaturalKeys(factory, declaration, true).ToArray();
					splitBasic = GetSplitBasicFromUkHawbs(factory, cei, hawbs);
				}
			}
			return splitBasic;
		}

		#endregion

		#region Find CusEntryInstruction from Hawb

		public static CusEntryInstruction FindCusEntryInstructionFromHawb(BusinessObjectFactory factory, CusHAWB hawb)
		{
			CusEntryInstruction cei = null;

			cei = FindCusEntryInstructionUsingLinkedDeclaration(factory, hawb);
			if (cei == null)
			{
				cei = FindCusEntryInstructionUsingLinkedShipment(factory, hawb);
			}
			if (cei == null)
			{
				cei = FindCusEntryInstructionUsingLinkedNaturalKeys(factory, hawb);
			}

			return cei;
		}

		static CusEntryInstruction FindCusEntryInstructionUsingLinkedDeclaration(BusinessObjectFactory factory, CusHAWB hawb)
		{
			return factory.Load<CusEntryInstruction>(new ZQuery(CusEntryInstructionSchema.CEI_JE, hawb.CS_JE_CustomsFormalEntry)).FirstOrDefault();
		}

		static CusEntryInstruction FindCusEntryInstructionUsingLinkedShipment(BusinessObjectFactory factory, CusHAWB hawb)
		{
			CusEntryInstruction cei = null;
			var shipment = factory.Load<ForwardingShipment>(hawb.CS_JS);
			if (shipment != null)
			{
				var declaration = (from JobDeclaration d in shipment.Declarations.OfType<JobDeclaration>() where d.CountryCode == Core.Constants.CountryCodes.UnitedKingdom select d).FirstOrDefault();
				if (declaration != null)
				{
					cei = declaration.CusEntryInstruction;
				}
			}
			return cei;
		}

		static CusEntryInstruction FindCusEntryInstructionUsingLinkedNaturalKeys(BusinessObjectFactory factory, CusHAWB hawb)
		{
			CusEntryInstruction cei = null;

			if (hawb != null)
			{
				var mawb = hawb.MAWB;

				if (mawb != null)
				{
					var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_MasterBill, mawb.CM_MAWB);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_HouseBill, hawb.CS_HAWB);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_SubLocationOfGoods, hawb.CargoTerminalOperatorAirportAndShed);

					var declaration = factory.LoadTop1<JobDeclaration>(declarationQuery);

					if (declaration != null)
					{
						cei = declaration.CusEntryInstruction;
					}
				}
			}
			return cei;
		}

		#endregion

		#region Find CusEntryInstruction from SplitHouse

		public static CusEntryInstruction FindCusEntryInstructionFromSplitHouse(BusinessObjectFactory factory, SplitHouse splitHouse)
		{
			CusEntryInstruction cei = null;

			var hawb = factory.Load<CusHAWB>(splitHouse.CG_CS);
			if (hawb != null)
			{
				cei = FindCusEntryInstructionFromHawb(factory, hawb);
				if (cei != null && splitHouse.CG_MessageReference != cei.CEI_SplitReference)
				{
					cei = null;
				}
			}
			return cei;
		}

		#endregion

		#region Find CusEntryInstruction from SplitBasic

		public static CusEntryInstruction FindCusEntryInstructionFromSplitBasic(BusinessObjectFactory factory, CusPartShip splitBasic)
		{
			CusEntryInstruction cei = null;

			var basic = factory.Load<CusMAWB>(splitBasic.CG_CM_LinkToPartMaster);
			if (basic != null)
			{
				var hawb = basic.MasterLevelHouseHelper;
				cei = FindCusEntryInstructionFromHawb(factory, hawb);
				if (cei != null && (cei.JobDeclaration.JE_HouseBill != "" || hawb.CS_HAWB != "" || !hawb.CS_IsMasterHouse))
				{
					cei = null;
				}
			}
			return cei;
		}

		#endregion
	}
}
