using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CFSLoadListConsolWrapper : SeaCargoDepotLoadList
		, IOutturnableLine
		, IObsoleteValidation
		, IAUCusUnderbondUnionCollectionParent
		, Customs.Business.IMessageManageableBizObj
	{
		protected CFSLoadListConsolWrapper(CFSLoadListConsol loadListConsol)
			: base(loadListConsol)
		{
		}

		public static new CFSLoadListConsolWrapper Load(CFSLoadListConsol parent)
		{
			return (CFSLoadListConsolWrapper)Load(typeof(CFSLoadListConsolWrapper), parent);
		}

		#region IOutturnableLine Members

		bool IOutturnableLine.IsDeleted
		{
			get { return LoadList.IsDeleted; }
		}

		ZString IOutturnableLine.UnderbondHumanReadableName
		{
			get { return "Consol: " + LoadList.JK_MasterBillNum; }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
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
			foreach (CFSShipment shipment in LoadList.Shipments)
			{
				result.Add(CFSShipmentWrapper.Load(shipment));
			}
			foreach (CFSContainer container in LoadList.Containers)
			{
				result.Add(CFSContainerWrapper.Load(container));
			}
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
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
