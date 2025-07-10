using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSContainerWrapper : BusinessObjectWrapper
		, IOutturnableLine
		, ICusUnderbondDependentCollectionParent
		, IAUCusUnderbondUnionCollectionParent
		, IObsoleteValidation
		, Customs.Business.IMessageManageableBizObj
	{
		protected CFSContainerWrapper(CFSContainer container)
			: base(container)
		{
			this.Container = container;
		}

		public readonly CFSContainer Container;

		public static CFSContainerWrapper Load(CFSContainer parent)
		{
			return (CFSContainerWrapper)Load(typeof(CFSContainerWrapper), parent);
		}

		public ZString UnderbondHumanReadableName
		{
			get { return "Container: " + Container.JC_ContainerNum; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return Container.JC_Calc_TotalPackages; }
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

		#region IOutturnableLine Members

		bool IOutturnableLine.IsDeleted
		{
			get { return Container.IsDeleted; }
		}

		#endregion

		#region ICusUnderbondDependentCollectionParent Members

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get
			{
				return "Container: " + Container.JC_ContainerNum;
			}
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines => Factory.GetValue(ref outturnableLines, GetOutturnableLines);
		CachedProperty<IOutturnableLine[]> outturnableLines;

		IOutturnableLine[] GetOutturnableLines()
		{
			List<IOutturnableLine> result = new List<IOutturnableLine>();
			result.Add(this);
			foreach (CFSShipment shipment in Container.PackUnpackShipments)
			{
				result.Add(CFSShipmentWrapper.Load(shipment));
			}
			return result.ToArray();
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
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

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (fAllUnderbonds == null)
				{
					fAllUnderbonds = new CusUnderbondUnionCollection(this);
					fAllUnderbonds.Load();
					RegisterEditableChildObject(fAllUnderbonds);
				}
				return fAllUnderbonds;
			}
		}
		CusUnderbondUnionCollection fAllUnderbonds;

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			ArrayList result = new ArrayList();
			foreach (CFSPackLine packLine in Container.PackLines)
			{
				if (packLine.Shipment != null)
				{
					result.Add(CFSShipmentWrapper.Load(packLine.Shipment));
				}
			}
			result.Add(this);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion

		#region ISeaOutturnReportHeaderInformationProvider Members

		public ISeaOutturnReportHeaderInformation GetHeader(CusUnderbond underbond)
		{
			if (underbond != null)
			{
				var outturnHeader = Factory.Load<CusOutturnHeader>(underbond.C4_C6);
				if (outturnHeader != null)
				{
					return new DepotCusOutturnHeaderOutturnReportHeaderInformation(outturnHeader);
				}
			}
			return null;
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new SeaCargoDepotMultiMessageManager(this);
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion
	}
}
