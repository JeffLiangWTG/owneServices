using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(Schema.CJ_ContainerNumber), DescriptionProperty(Schema.CJ_ContainerNumber)]
	public class CusSCADepotContainer : AutoCusSCADepotContainer, ICusUnderbondDependentCollectionParent
	{
		#region Constructors

		public CusSCADepotContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Static Loaders

		public static CusSCADepotContainer Load(BusinessObjectFactory factory, ZString containerNumber, ZString voyage, ZString status, ZString lloydsNumber)
		{
			var containerFilter = new ZQuery(CusSCADepotContainerSchema.CJ_ContainerNumber, containerNumber);
			containerFilter.AddToFilter(CusSCADepotContainerSchema.CJ_Voyage, voyage);
			containerFilter.AddToFilter(CusSCADepotContainerSchema.CJ_Status, status);

			if (!lloydsNumber.IsEmpty)
			{
				containerFilter.AddToFilter(CusSCADepotContainerSchema.CJ_LloydsNumber, lloydsNumber);
			}

			var containers = (CusSCADepotContainer[])factory.Load(typeof(CusSCADepotContainer), containerFilter);

			if (containers.Length > 1)
			{
				ErrorReporter.ReportOnce("DepotContainer" + containerNumber + voyage + status + lloydsNumber, "Found more than one match for CusSCADepotContainer");
			}

			return containers.FirstOrDefault();
		}

		#endregion

		#region FromContainer

		public void FromContainer(CommonContainer container)
		{
			if (container != null)
			{
				CJ_ContainerNumber = container.JC_ContainerNum;
				CJ_IsSealOk = container.JC_IsSealOk;
				if (container.Sailing?.Vessel != null)
				{
					CJ_LloydsNumber = container.Sailing.Vessel.RV_LloydsNumber;
				}
				CJ_PackageCount = ((IPackLineCollection)container.PackLines).Totals.TotalPackages;
				CJ_SealNumber = container.JC_SealNum.Left(Schema.CJ_SealNumberMaxLength);
				if (container.Sailing?.Voyage != null)
				{
					CJ_Voyage = container.Sailing.Voyage.JV_VoyageFlight;
				}
			}
		}

		#endregion

		#region MessageDataEqual

		public bool MessageDataEqual(CusSCADepotContainer compareTo)
		{
			var result = compareTo.CJ_ContainerNumber == CJ_ContainerNumber
				&& compareTo.CJ_IsSealOk == CJ_IsSealOk
				&& compareTo.CJ_LloydsNumber == CJ_LloydsNumber
				&& compareTo.CJ_PackageCount == CJ_PackageCount
				&& compareTo.CJ_SealNumber == CJ_SealNumber
				&& compareTo.CJ_Voyage == CJ_Voyage;
			return result;
		}

		#endregion

		#region Related Business Objects

		public CusSCADepotHouseCollection HouseBills
		{
			get
			{
				if (fHouseBills == null)
				{
					fHouseBills = new CusSCADepotHouseCollection(this, Factory);
					fHouseBills.Load();
					fHouseBills.IsManagedForDataRefresh = true;
				}
				return fHouseBills;
			}
		}

		#endregion
		#region Business Object Overrides

		public override void Delete()
		{
			HouseBills.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Implementation

		CusSCADepotHouseCollection fHouseBills;

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		public ZString Details
		{
			get
			{
				return CJ_ContainerNumber;
			}
		}

		public IOutturnableLine[] OutturnableLines
		{
			get
			{
				return Underbonds.Cast<CusUnderbond>().SelectMany(x => x.Outturns.Cast<CusOutturn>()).Select(x => x.Parent).Where(x => x != null).Distinct().ToArray();
			}
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return CJ_PackageCount; }
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds => Underbonds;
		[ChildEditable(true)]
		public CusUnderbondCollection Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		public ZString UnderbondHumanReadableName
		{
			get
			{
				return "CONTAINER: " + CJ_ContainerNumber;
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
