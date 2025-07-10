using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSContainerCreator : CFSRecordCreator
	{
		public CFSContainerCreator(DepotCusOutturn outturn, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
			CreateConsol(outturn);
			CreateContainer(outturn);
		}

		public CFSContainerCreator(DepotCusOutturn outturn, CFSLoadListConsol consol, BusinessObjectFactory factory)
			: base(outturn, factory)
		{
			fConsol = consol;
			CreateContainer(outturn);
		}

		public CFSLoadListConsol Consol
		{
			get { return fConsol; }
		}
		CFSLoadListConsol fConsol;

		public CFSContainer Container
		{
			get { return fContainer; }
		}
		CFSContainer fContainer;

		#region Implementation

		void CreateConsol(DepotCusOutturn outturn)
		{
			if (!outturn.C5_MasterBill.IsEmpty)
			{
				fConsol = FindExistingConsol(outturn);
			}
			if (Consol == null)
			{
				CFSContainer matchingContainer = FindExistingContainer(outturn);
				if (matchingContainer != null && Consol == null)
				{
					fConsol = matchingContainer.Consol;
				}
				if (Consol == null)
				{
					fConsol = new CFSLoadListConsolCreator(outturn, Factory).Consol;
					Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
					if (!outturn.C5_MasterBill.IsEmpty)
					{
						Consol.JK_MasterBillNum = outturn.C5_MasterBill;
					}
					Consol.JK_ConsolMode = ContainerModeFromOutturn(outturn);
				}
			}
		}

		void CreateContainer(DepotCusOutturn outturn)
		{
			fContainer = FindExistingContainer(outturn, Consol);
			if (fContainer == null)
			{
				fContainer = Consol.Containers.AddNew();
				Container.JC_ContainerNum = outturn.C5_ContainerNumber;
				Container.JC_ContainerMode = ContainerModeFromOutturn(outturn);
			}
			if (Container != null)
			{
				if (outturn.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoad ||
					outturn.C5_CargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills)
				{
					outturn.C5_ParentID = Container.PK;
					outturn.C5_ParentTableCode = JobContainerSchema.Constants.Prefix;
				}
				if (outturn.Underbond != null)
				{
					outturn.Underbond.C4_ParentID = Container.PK;
					outturn.Underbond.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
				}
			}
		}

		#endregion
	}
}
