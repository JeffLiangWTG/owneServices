using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusUnderbondFactory
	{
		#region OutturnHeader Creation and Loading

		protected internal CusOutturnHeader GetOrCreateOutturnHeader(BusinessObjectFactory factory, ZString lloydsNumber, ZString voyageNumber, ZString premiseID, ZBool createIfNotOurPremiseID)
		{
			CusOutturnHeader result = null;

			ZQuery filter = new ZQuery(CusOutturnHeaderSchema.C6_LloydsIMO, lloydsNumber);
			filter.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, voyageNumber);

			if (!premiseID.IsEmpty)
			{
				filter.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, premiseID);
			}
			else
			{
				filter.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			CusOutturnHeader[] possibleResults = (CusOutturnHeader[])factory.Load(typeof(CusOutturnHeader), filter);
			foreach (CusOutturnHeader header in possibleResults)
			{
				if (result == null)
				{
					result = header;
				}
				else if (CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(factory), header.C6_OutturningPremiseID))
				{
					result = header;
				}
			}

			if (result == null && (createIfNotOurPremiseID || CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(factory), premiseID)))
			{
				result = factory.New<CusOutturnHeader>();
				SetDetailsOnOutturnHeader(result, lloydsNumber, voyageNumber, premiseID);
			}

			return result;
		}

		protected void SetDetailsOnOutturnHeader(CusOutturnHeader outturnHeader, ZString lloydsNumber, ZString voyageNumber, ZString premiseID)
		{
			outturnHeader.C6_LloydsIMO = lloydsNumber;
			outturnHeader.C6_VoyageNum = voyageNumber;
			outturnHeader.C6_OutturningPremiseID = premiseID;
			outturnHeader.C6_ResponsiblePartyID = ABNForCompany(GlbCompany.GetCurrentCompany(outturnHeader.Factory), premiseID);
		}

		#endregion

		#region PremiseID Matching

		protected ZString ABNForCompany(GlbCompany company, ZString premiseID)
		{
			return PremiseIDMatcher.ABNForCompany(company, premiseID);
		}

		protected bool CompanyHasDepotWithPremiseID(GlbCompany company, ZString premiseID)
		{
			return PremiseIDMatcher.CompanyHasDepotWithPremiseID(company, premiseID);
		}

		protected bool OrgIsDepotWithPremiseId(OrgHeader organisation, ZString premiseID)
		{
			return PremiseIDMatcher.OrgIsDepotWithPremiseID(organisation, premiseID);
		}

		protected bool CompanyHasCTOWithPremiseID(ZString premiseID)
		{
			return CompanyHasCTOWithPremiseID(premiseID, false);
		}

		public bool CompanyHasCTOWithPremiseID(ZString premiseID, bool companyProxy)
		{
			bool result = OrgIsCTOWithPremiseId(GlbCompany.CurrentCompany.OrgProxy, premiseID, companyProxy);
			if (!result)
			{
				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					if (OrgIsCTOWithPremiseId(branch.OrgProxy, premiseID, companyProxy))
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		protected bool OrgIsCTOWithPremiseId(OrgHeader organisation, ZString premiseID, bool isCompanyProxy)
		{
			bool result = false;
			if (organisation != null)
			{
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (address.LocalControlledPremisesID == premiseID)
					{
						result = isCompanyProxy || organisation.OH_IsAirCTO || organisation.OH_IsSeaCTO;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region TypeOfUnderbond

		protected virtual Type TypeOfUnderbond
		{
			get { return typeof(CusUnderbond); }
		}

		#endregion

		#region Underbond Creation and Loading

		protected CusUnderbond GetOrCreateUnderbond(BusinessObjectFactory factory, ZString originPremiseID, ZString destinationPremiseID, ZString containerNumber, ZString houseBill, ZString oceanBillNum, ZBool isExpectedArrival, CusOutturnHeader header)
		{
			CusUnderbond result = null;

			if (containerNumber.IsEmpty && houseBill.IsEmpty && oceanBillNum.IsEmpty)
			{
				ErrorReporter.ReportOnce("Missing consignment Key Details", "Consignment Key Details Blank!");
			}

			if (isExpectedArrival)
			{
				result = GetUnderbond(factory, originPremiseID, destinationPremiseID, containerNumber, houseBill, oceanBillNum, header);
			}
			else
			{
				result = GetUnderbond(factory, ZString.Empty, originPremiseID, containerNumber, houseBill, oceanBillNum, header);
			}

			if (result == null)
			{
				result = (CusUnderbond)factory.New(TypeOfUnderbond);
				SetDetailsOnUnderbond(result, header, originPremiseID, destinationPremiseID);
			}

			return result;
		}

		protected void SetDetailsOnUnderbond(CusUnderbond underbond, CusOutturnHeader header, ZString originPremiseID, ZString destinationPremiseID)
		{
			if (header != null)
			{
				underbond.C4_C6 = header.PK;
			}

			underbond.C4_ResponsiblePartyID = ABNForCompany(GlbCompany.GetCurrentCompany(underbond.Factory), destinationPremiseID);
			underbond.C4_DestinationPremiseID = destinationPremiseID;
			underbond.C4_OriginPremiseID = originPremiseID;
		}

		protected CusUnderbond GetUnderbond(BusinessObjectFactory factory, ZString originPremiseID, ZString destinationPremiseID, ZString containerNumber, ZString houseBill, ZString oceanBillNum, CusOutturnHeader header)
		{
			CusUnderbond result = null;

			ZQuery filter = new ZQuery(CusUnderbondSchema.C4_DestinationPremiseID, destinationPremiseID);

			if (!originPremiseID.IsEmpty)
			{
				filter.AddToFilter(CusUnderbondSchema.C4_OriginPremiseID, originPremiseID);
			}

			if (header == null)
			{
				filter.AddToFilter(CusUnderbondSchema.C4_C6, ZGuid.Empty);
			}
			else
			{
				filter.AddToFilter(CusUnderbondSchema.C4_C6, header.PK);
			}

			CusUnderbond[] possibleUnderbonds = (CusUnderbond[])factory.Load(TypeOfUnderbond, filter);

			foreach (CusUnderbond underbond in possibleUnderbonds)
			{
				if (underbond.LinkedObject != null
					&& ((!houseBill.IsEmpty && !containerNumber.IsEmpty && UnderbondForHouse(underbond, containerNumber, houseBill))
					|| (!containerNumber.IsEmpty && UnderbondForContainer(underbond, containerNumber))
					|| (!oceanBillNum.IsEmpty && UnderbondForBLK(underbond, oceanBillNum))))
				{
					result = underbond;
					break;
				}
			}

			return result;
		}

		protected bool UnderbondForHouse(CusUnderbond underbond, ZString containerNumber, ZString houseBillNumber)
		{
			bool result = false;
			CusSCADepotHouse depotHouse = underbond.LinkedObject as CusSCADepotHouse;
			if (depotHouse != null)
			{
				CusSCADepotContainer container = (CusSCADepotContainer)underbond.Factory.Load(typeof(CusSCADepotContainer), depotHouse.CX_CJ);
				result = container != null && container.CJ_ContainerNumber == containerNumber && depotHouse.CX_HouseBill == houseBillNumber;
			}
			CFSShipmentWrapper shipmentWrapper = underbond.LinkedObject as CFSShipmentWrapper;
			if (shipmentWrapper != null && shipmentWrapper.Container != null)
			{
				result = shipmentWrapper.Shipment.JS_HouseBill == houseBillNumber && shipmentWrapper.Container.JC_ContainerNum == containerNumber;
			}
			return result;
		}

		protected bool UnderbondForContainer(CusUnderbond underbond, ZString containerNumber)
		{
			bool result = false;
			CusSCADepotContainer depotContainer = underbond.LinkedObject as CusSCADepotContainer;
			if (depotContainer != null)
			{
				result = depotContainer.CJ_ContainerNumber == containerNumber;
			}

			CFSContainerWrapper containerWrapper = underbond.LinkedObject as CFSContainerWrapper;
			if (containerWrapper != null)
			{
				result = containerWrapper.Container.JC_ContainerNum == containerNumber;
			}
			return result;
		}

		protected bool UnderbondForBLK(CusUnderbond underbond, ZString oceanBillNumber)
		{
			bool result = false;
			CusSCADepotHouse depotHouse = underbond.LinkedObject as CusSCADepotHouse;
			if (depotHouse != null)
			{
				result = depotHouse.CX_HouseBill == oceanBillNumber;
			}
			CFSShipmentWrapper shipmentWrapper = underbond.LinkedObject as CFSShipmentWrapper;
			if (shipmentWrapper != null)
			{
				foreach (CFSLoadListConsol consol in shipmentWrapper.Shipment.Consols)
				{
					result = consol.JK_MasterBillNum == oceanBillNumber;
				}
			}
			return result;
		}

		#endregion

		#region OutturnLine Loading

		protected internal DepotCusOutturn GetOutturnLine(CusOutturnHeader header, ZString containerMode, ZString containerNumber, ZString houseBillNumber, ZString oceanBillNumber)
		{
			ZQuery filter = null;
			if (header == null)
			{
				filter = new ZQuery(CusOutturnSchema.C5_C6, ZGuid.Empty);
			}
			else
			{
				filter = new ZQuery(CusOutturnSchema.C5_C6, header.PK);
			}
			filter.AddToFilter(CusOutturnSchema.C5_CargoType, containerMode);

			ZQuery containerNumberFilter = new ZQuery(CusOutturnSchema.C5_ContainerNumber, containerNumber);
			ZQuery houseBillNumberFilter = new ZQuery(CusOutturnSchema.C5_HouseBill, houseBillNumber);
			ZQuery oceanBillNumberFilter = new ZQuery(CusOutturnSchema.C5_MasterBill, oceanBillNumber);

			switch (containerMode)
			{
				case CMRImportCargoTypes.Codes.BreakBulk:
				case CMRImportCargoTypes.Codes.Bulk:
					filter.AddToFilter(houseBillNumberFilter);
					filter.AddToFilter(oceanBillNumberFilter);
					break;

				case CMRImportCargoTypes.Codes.FullContainerLoad:
				case CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills:
					filter.AddToFilter(containerNumberFilter);
					break;

				case CMRImportCargoTypes.Codes.LessThanContainerLoad:
					filter.AddToFilter(containerNumberFilter);
					filter.AddToFilter(houseBillNumberFilter);
					filter.AddToFilter(oceanBillNumberFilter);
					break;
			}

			var result = header.Factory.LoadTop1<DepotCusOutturn>(filter);
			return result;
		}

		#endregion
	}
}
