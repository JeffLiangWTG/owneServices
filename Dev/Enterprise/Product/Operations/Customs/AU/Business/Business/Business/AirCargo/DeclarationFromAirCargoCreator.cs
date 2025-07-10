using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationFromAirCargoCreator
	{
		public DeclarationFromAirCargoCreator(CusHAWB houseAirCargo)
		{
			this.HouseAirCargo = houseAirCargo;
		}

		public readonly CusHAWB HouseAirCargo;

		public BaseJobDeclaration CreateIgnoreWarnings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			CreateIgnoreWarnings(declaration);
			return declaration;
		}

		public void CreateIgnoreWarnings(BaseJobDeclaration declaration)
		{
			NotificationBuffer notificationsWhichWillBeIgnored = new NotificationBuffer();
			Create(declaration, notificationsWhichWillBeIgnored);
		}

		public BaseJobDeclaration Create(INotifications notify)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Create(declaration, notify);
			return declaration;
		}

		public void Create(BaseJobDeclaration declaration, INotifications notify)
		{
			CreateCore(declaration, notify);
		}

		protected virtual void CreateCore(BaseJobDeclaration declaration, INotifications notify)
		{
			CusHAWB hAWBInDeclarationFactory = declaration.Factory.Load<CusHAWB>(HouseAirCargo.PK);
			hAWBInDeclarationFactory.CS_JE_CustomsFormalEntry = declaration.PK;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			SetDetailsFromMAWB(declaration, notify);
			SetDetailsFromHAWB(declaration, notify);
		}

		protected virtual ZGuid GetConsigneePK(INotifications notify, BaseJobDeclaration declaration)
		{
			return HouseAirCargo.Consignee?.PK ?? ZGuid.Empty;
		}

		protected virtual ZGuid GetConsignorPK(INotifications notify, BaseJobDeclaration declaration)
		{
			return HouseAirCargo.Consignor?.PK ?? ZGuid.Empty;
		}
		protected BusinessObjectFactory Factory
		{
			get { return HouseAirCargo.Factory; }
		}

		protected CusMAWB MasterAirCargo
		{
			get { return HouseAirCargo.MAWB; }
		}

		#region Implementation

		protected StringToBusinessObjectFieldConverter converter = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;

		void SetDetailsFromMAWB(BaseJobDeclaration declaration, INotifications notify)
		{
			if (MasterAirCargo != null)
			{
				declaration.JE_ApplicationCode = MasterAirCargo.CM_ApplicationCode;
				converter.SetPropertyInfoValue(declaration.JE_RL_NKPortOfLoadingInfo, MasterAirCargo.CM_RL_NKLoadPort, notify);
				declaration.JE_DateOfArrival = MasterAirCargo.CM_ArrivalDate;
				converter.SetPropertyInfoValue(declaration.JE_RL_NKPortOfArrivalInfo, MasterAirCargo.CM_RL_NKDischargePort, notify);
				if (!MasterAirCargo.CM_RL_NKFirstArrivalPort.IsEmpty)
				{
					converter.SetPropertyInfoValue(declaration.JE_RL_NKPortOfFirstArrivalInfo, MasterAirCargo.CM_RL_NKFirstArrivalPort, notify);
					declaration.JE_DateOfFirstArrival = MasterAirCargo.CM_DateOfFirstArrival;
				}
				declaration.JE_MasterBill = MasterAirCargo.CM_MAWB;
				declaration.JE_VoyageFlightNo = MasterAirCargo.CM_FlightNo;
				declaration.JE_Folio = MasterAirCargo.CM_Folio;
			}
		}

		void SetDetailsFromHAWB(BaseJobDeclaration declaration, INotifications notify)
		{
			SetConsignee(declaration, notify);
			SetConsignor(declaration, notify);

			converter.SetPropertyInfoValue(declaration.JE_HouseBillInfo, HouseAirCargo.CS_HAWB, notify);
			converter.SetPropertyInfoValue(declaration.JE_OwnerRefInfo, HouseAirCargo.CS_HAWB, notify);
			converter.SetPropertyInfoValue(declaration.JE_RL_NKOriginInfo, HouseAirCargo.CS_RL_NKOrigin, notify);
			converter.SetPropertyInfoValue(declaration.JE_RL_NKFinalDestinationInfo, HouseAirCargo.CS_RL_NKDestination, notify);
			declaration.JE_TotalWeight = HouseAirCargo.CS_Weight;

			converter.SetPropertyInfoValue(declaration.JE_TotalWeightUnitInfo, HouseAirCargo.CS_WeightUQ, notify);
			converter.SetPropertyInfoValue(declaration.JE_GoodsDescriptionInfo, HouseAirCargo.CS_GoodsDescription, notify);

			declaration.JE_TotalNoOfPieces = HouseAirCargo.CS_PiecesManifested;
			declaration.JE_TotalNoOfPacks = HouseAirCargo.CS_PiecesManifested;
			converter.SetPropertyInfoValue(declaration.JE_TotalNoOfPacksPackTypeInfo, Core.Constants.PkgUnit.Piece, notify);
		}

		void SetConsignee(BaseJobDeclaration declaration, INotifications notify)
		{
			declaration.JE_OH_Importer = GetConsigneePK(notify, declaration);
		}

		void SetConsignor(BaseJobDeclaration declaration, INotifications notify)
		{
			declaration.JE_OH_Supplier = GetConsignorPK(notify, declaration);
		}

		#endregion
	}
}
