using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CFSRecordCreator
	{
		public CFSRecordCreator(DepotCusOutturn outturn, BusinessObjectFactory factory)
		{
			if (outturn == null)
			{
				throw new ArgumentNullException(nameof(outturn));
			}

			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			this.Outturn = outturn;
			this.Factory = factory;
		}

		public readonly CusOutturn Outturn;
		public readonly BusinessObjectFactory Factory;

		#region Implementation

		protected CFSLoadListConsol FindExistingConsol(DepotCusOutturn outturn)
		{
			CFSLoadListConsol result = null;
			if (outturn.Header != null)
			{
				if (!outturn.C5_MasterBill.IsEmpty)
				{
					ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, outturn.C5_MasterBill);
					filter.AddToFilter(JobConsolSchema.JK_IsCFS, ZBool.True);
					CFSLoadListConsol[] existingConsolsWithMasterBill = (CFSLoadListConsol[])Factory.Load(typeof(CFSLoadListConsol), filter);
					foreach (CFSLoadListConsol consol in existingConsolsWithMasterBill)
					{
						if (ConsolVoyageMatchesOutturn(consol, outturn.Header))
						{
							result = consol;
							break;
						}
					}
				}
				if (result == null && !outturn.C5_ContainerNumber.IsEmpty)
				{
					CFSContainer container = FindExistingContainer(outturn);
					if (container != null && container.Consol != null)
					{
						result = container.Consol;
					}
				}
			}
			return result;
		}

		protected bool ConsolVoyageMatchesOutturn(CFSLoadListConsol consol, CusOutturnHeader header)
		{
			return consol != null && header != null && consol.JK_JX_JV_NKVessel.ToUpper() == VesselFromLloyds(header.C6_LloydsIMO).ToUpper()
						&& consol.JK_JX_JV_VoyageFlight.ToUpper() == header.C6_VoyageNum.ToUpper();
		}

		protected CFSContainer FindExistingContainer(DepotCusOutturn outturn, CFSLoadListConsol consol = null)
		{
			CFSContainer result = null;
			if (!outturn.C5_ContainerNumber.IsEmpty && outturn.Header != null)
			{
				ZQuery filter = new ZQuery(JobContainerSchema.JC_ContainerNum, outturn.C5_ContainerNumber);
				filter.AddToFilter(JobContainerSchema.JC_IsCFSRegistered, ZBool.True);
				if (consol != null)
				{
					filter.AddToFilter(JobContainerSchema.JC_JK, consol.PK);
				}

				CFSContainer[] existingContainers = (CFSContainer[])Factory.Load(typeof(CFSContainer), filter);
				foreach (CFSContainer container in existingContainers)
				{
					if (container.Consol != null && ConsolVoyageMatchesOutturn(container.Consol, outturn.Header))
					{
						result = container;
						break;
					}
				}
			}
			return result;
		}

		protected ZString ContainerModeFromOutturn(DepotCusOutturn outturn)
		{
			ZString result = Enterprise.Core.Constants.ContainerModes.LCL;
			if (outturn.IsBreakBulk)
			{
				result = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			}
			else if (outturn.IsBulk)
			{
				result = Enterprise.Core.Constants.ContainerModes.Bulk;
			}
			else if (outturn.IsFCX)
			{
				result = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			}
			else
			{
				if ((outturn.Underbond != null && outturn.Underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination)
					|| (!outturn.C5_HouseBill.IsEmpty && !outturn.C5_ContainerNumber.IsEmpty))
				{
					result = Enterprise.Core.Constants.ContainerModes.LCL;
				}
				else
				{
					result = Enterprise.Core.Constants.ContainerModes.FCL;
				}
			}
			return result;
		}

		protected ZString VesselFromLloyds(ZString lloydsNumber)
		{
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Code = lloydsNumber;
				vessel.RV_LloydsNumber = lloydsNumber;
			}
			return vessel.RV_Code;
		}

		#endregion
	}
}
