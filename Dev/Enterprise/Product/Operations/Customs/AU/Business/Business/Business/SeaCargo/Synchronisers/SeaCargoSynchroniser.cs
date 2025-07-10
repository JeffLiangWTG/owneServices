using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class SeaCargoSynchroniser
	{
		public SeaCargoSynchroniser(CommonConsol consol, CusSCAOceanBill oceanBill, bool synchroniseConsol = true)
		{
			Consol = consol;
			OceanBill = oceanBill;
			this.synchroniseConsol = synchroniseConsol;
		}

		#region Constants
		public const string BillCharactersToKeep = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		#endregion

		public readonly CommonConsol Consol;
		readonly bool synchroniseConsol;

		public CusSCAOceanBill OceanBill
		{
			get { return GetOrCreateOceanBill(true); }
			protected set
			{
				if (fOceanBill != null)
				{
					fOceanBill.OverrideFreightDefaultsChanged -= OnOverrideFreightDefaultsChanged;
				}

				fOceanBill = value;

				if (fOceanBill != null)
				{
					fOceanBill.OverrideFreightDefaultsChanged += OnOverrideFreightDefaultsChanged;
				}
			}
		}

		void OnOverrideFreightDefaultsChanged(object sender, ZBool overrideFreightDefaultsNewValue)
		{
			if (overrideFreightDefaultsNewValue)
			{
				StopAllSynchronisers();
			}
			else
			{
				// rebuild the sea cargo synchroniser and force a refresh through all child synchronisers
				SynchroniseOceanBill();
			}
		}

		protected bool IsEnabled => fOceanBill != null && !fOceanBill.OverrideFreightDefaults && fOceanBill.Messages.Count == 0;

		public CusSCAOceanBill ExistingOceanBill => GetOrCreateOceanBill(false);

		CusSCAOceanBill GetOrCreateOceanBill(bool shouldCreate)
		{
			if (fOceanBill == null)
			{
				OceanBill = GetExistingOceanBill();
				if (fOceanBill == null && shouldCreate)
				{
					CreateNewOceanBill();
				}

				InitialiseContainers(fOceanBill);

				if (IsEnabled && synchroniseConsol)
				{
					LoadHouseBills();
					GetOceanBillSynchroniser(fOceanBill, Consol);
				}
			}
			return fOceanBill;
		}

		CusSCAOceanBill fOceanBill;

		void InitialiseContainers(CusSCAOceanBill oceanBill)
		{
			_ = oceanBill?.Containers.Select(x => x.Pivots).Count();
		}

		public CusSCAHouse GetHouseBill(CommonShipment shipment)
		{
			var house = LoadHouseBillForShipment(shipment)
				?? CreateNewHouseBill(shipment);

			if (house != null && ShouldStartHouseSynchronisers(house))
			{
				if (!SynchroniserExists(house))
				{
					HookHouseBillSynchronisers(shipment, house);
					house.Messages.CountChanged += StopSynchronisationOnMessages_CountChanged;
					house.ManualStatusAcceptedAsAcknowledged += StopSynchronisationOnManualStatusOverrideAccepted;
				}

				EnableAllSynchronisers();
			}

			return house;
		}

		CusSCAHouse LoadHouseBillForShipment(CommonShipment shipment)
		{
			var house = OceanBill.HouseBills.Cast<BaseCusSCAHouse>().FirstOrDefault(x => x.CA_JS == shipment.PK);
			if (house == null)
			{
				var loader = new BaseCusSCAHouse.Loader(Factory);
				house = loader.LoadFromShipmentAndApplicationCode(shipment.PK, CusSCAOceanBill.ApplicationCodes, reloadQuery: true);
			}
			return (CusSCAHouse)house;
		}

		public void SynchroniseOceanBill()
		{
			DefaultOceanBillDetailsAndChildren();
		}

		public void SynchroniseHouse(CusSCAHouse houseBill, CommonShipment shipment)
		{
			DefaultSeaCargoHouseFromShipment(OceanBill, houseBill, shipment);
			DefaultOceanBillDetailsAndChildren();
		}

		public void Synchronise(SynchroniseAction action)
		{
			foreach (var synch in BusinessObjectSynchronisers)
			{
				synch.Synchronise(new SynchroniseEventArgs(action));
			}
		}

		public void LoadHouseBills()
		{
			foreach (CommonShipment shipment in Consol.Shipments)
			{
				GetHouseBill(shipment);
			}
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return Consol.Factory; }
		}

		protected internal List<BusinessObjectSynchroniser> BusinessObjectSynchronisers => fBusinessObjectSynchronisers ?? (fBusinessObjectSynchronisers = new List<BusinessObjectSynchroniser>());
		List<BusinessObjectSynchroniser> fBusinessObjectSynchronisers;

		protected abstract Type OceanBillType { get; }

		public CusSCAOceanBill GetExistingOceanBill()
		{
			return (CusSCAOceanBill)new BaseCusSCAOceanBill.Loader(Factory).LoadFromConsolAndApplicationCode(Consol, CusSCAOceanBill.ApplicationCodes);
		}

		protected void CreateNewOceanBill()
		{
			ReportErrorIfIsNotLocked();

			OceanBill = (CusSCAOceanBill)Factory.New(OceanBillType);
			OceanBill.CB_ParentId = Consol.PK;
			OceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			DefaultOceanBillDetailsAndChildren();
		}

		/// <summary>
		/// Creates a new House Bill in another factory and saves it, then loads again in current factory.
		/// Except if an existing house is found in the other factory that cannot be loaded in the current factory, which indicates
		/// it has been deleted locally but not yet saved, so a local house is created that can replace it.
		/// </summary>
		/// <param name="shipment"></param>
		/// <returns></returns>
		CusSCAHouse CreateNewHouseBill(CommonShipment shipment)
		{
			CusSCAHouse house = null;

			if (shipment.IsInDatabase)
			{
				try
				{
					var tmpFactory = new BusinessObjectFactory();
					var oceanBill = tmpFactory.Load<CusSCAOceanBill>(OceanBill.PK);
					if (oceanBill != null)
					{
						house = oceanBill.HouseBills.Cast<CusSCAHouse>().FirstOrDefault(hb => hb.CA_JS == shipment.PK);
						if (house == null)
						{
							house = CreateNewHouseBill(oceanBill, shipment);
							tmpFactory.Save();
						}
						house = Factory.Load<CusSCAHouse>(house.PK);
					}
				}
				catch (ZSaveException)
				{
					house = null;
				}
			}

			if (house != null)
			{
				OceanBill.HouseBills.Reload(false);
			}
			else
			{
				house = CreateNewHouseBill(OceanBill, shipment);
			}

			return house;
		}

		CusSCAHouse CreateNewHouseBill(CusSCAOceanBill oceanBill, CommonShipment shipment)
		{
			var house = oceanBill.HouseBills.AddNew();
			DefaultSeaCargoHouseFromShipment(oceanBill, house, shipment);
			return house;
		}

		void ReportErrorIfIsNotLocked()
		{
			if (NeedCheckLocker)
			{
				using (var mutex = (ZGlobalMutex)CusSCAOceanBill.CreateMutexForConsol(Consol.PK, Core.Constants.CountryCodes.Australia))
				{
					if (mutex.Lock())
					{
						var factoryName = string.IsNullOrWhiteSpace(Factory.NameForDebugging) ? "Unknown" : Factory.NameForDebugging;
						ErrorReporter.ReportOnce($"CreateNewOceanBillOnConsolWithoutLock", $@"A new Ocean Bill is being created without any locks from {Consol.HumanReadableName} in factory[{factoryName}] by {Env.CurrentUser.FullName}.");
					}
				}
			}
		}

		protected virtual bool NeedCheckLocker => !Globals.IsTest;

		public void DefaultOceanBillDetailsAndContainers()
		{
			var oceanBill = OceanBill;
			oceanBill.CB_RL_NKPortOfLoading = Consol.JK_RL_NKLoadForFirstImportTransport;
			oceanBill.CB_RL_NKPortOfDischarge = Consol.JK_RL_NKDiscForLastImportTransport;

			oceanBill.CB_OceanBill = Consol.JK_MasterBillNum;
			if (Consol.IsCoLoad)
			{
				oceanBill.CB_MasterHouseBill = Consol.JK_CoLoadMasterBill;
			}

			oceanBill.CB_VesselName = Consol.JK_VesselOfLastImportTransport;
			oceanBill.CB_Voyage = Consol.JK_VoyageOfLastImportTransport;
			if (Consol.ShippingLine != null)
			{
				oceanBill.CB_PrincipalID = Consol.ShippingLine.LocalPrincipalID.SubstringSafe(0, 11);
				oceanBill.CB_OH_ShippingLine = Consol.ShippingLinePK;
			}
			DefaultOceanBillContainers();
		}

		protected virtual void DefaultOceanBillDetailsAndChildren()
		{
			DefaultOceanBillDetailsAndContainers();
		}

		void DefaultOceanBillContainer(CommonContainer container)
		{
			CusSCAContainer aCusSCAContainer = OceanBill.Containers.Find(container.JC_ContainerNum)
				?? OceanBill.Containers.AddNew();
			aCusSCAContainer.CN_ContainerNumber = container.JC_ContainerNum;
			aCusSCAContainer.CN_SealNumber = (container.JC_SealNum.Length > aCusSCAContainer.CN_SealNumberInfo.MaxLength) ? container.JC_SealNum.Substring(0, aCusSCAContainer.CN_SealNumberInfo.MaxLength) : container.JC_SealNum;
			if (container.Container != null)
			{
				aCusSCAContainer.CN_RC_NKContainerType = container.Container.RC_Code;
			}
			aCusSCAContainer.CN_ContainerMode = ConvertContainerMode(container.JC_ContainerMode);
			aCusSCAContainer.CN_CB = OceanBill.PK;
			aCusSCAContainer.CN_MoveUnderbondFrom = GetArrivalCTOBondID();
			aCusSCAContainer.CN_MoveUnderbondTo = GetUnpackingDepot();
		}

		void DefaultOceanBillContainers()
		{
			foreach (CommonContainer aContainer in Consol.Containers)
			{
				DefaultOceanBillContainer(aContainer);
			}
		}

		ZString GetArrivalCTOBondID()
		{
			ZString result = "";
			if (Consol.ArrivalCTOAddress != null)
			{
				OrgCusCode bondIDCusCode = LoadBondIDCusCode(Consol.ArrivalCTOAddress);
				if (bondIDCusCode != null)
				{
					result = bondIDCusCode.OK_CustomsRegNo;
				}
			}
			return result.SubstringSafe(0, 5);
		}

		ZString GetUnpackingDepot()
		{
			ZString result = "";
			if (Consol.UnpackDepotAddress != null)
			{
				OrgCusCode unpackDepotBondID = LoadBondIDCusCode(Consol.UnpackDepotAddress);
				if (unpackDepotBondID != null)
				{
					result = unpackDepotBondID.OK_CustomsRegNo;
				}
			}
			return result.SubstringSafe(0, 5);
		}

		protected OrgCusCode LoadBondIDCusCode(OrgAddress bondIDOrgAddress)
		{
			OrgCusCode result = null;
			ZQuery bondIDFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
			bondIDFilter.AddToFilter(OrgCusCodeSchema.OK_OH, bondIDOrgAddress.Header.PK);
			bondIDFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			ZQuery addressCTOFilter = new ZQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, bondIDOrgAddress.PK);
			addressCTOFilter.AddToFilter(bondIDFilter);

			result = Factory.LoadTop1<OrgCusCode>(addressCTOFilter);
			if (result == null)
			{
				result = Factory.LoadTop1<OrgCusCode>(bondIDFilter);
			}
			return result;
		}

		protected virtual ZString ConvertContainerMode(ZString containerMode)
		{
			ZString result = containerMode;
			switch (containerMode)
			{
				case Core.Constants.ContainerModes.FCL:
					if (Consol.JK_ConsolMode == Core.Constants.ContainerModes.Groupage)
					{
						result = Core.Constants.ContainerModes.FreightAllKind;
					}
					break;
				case Core.Constants.ContainerModes.Groupage:
					result = Core.Constants.ContainerModes.FreightAllKind;
					break;
				case Core.Constants.ContainerModes.BuyersConsol:
					result = Core.Constants.ContainerModes.FCLMixedShipper;
					break;
			}
			return result;
		}

		public void DefaultSeaCargoHouseFromShipment(CusSCAHouse houseBill, CommonShipment shipment)
		{
			DefaultSeaCargoHouseFromShipment(OceanBill, houseBill, shipment);
		}

		void DefaultSeaCargoHouseFromShipment(CusSCAOceanBill oceanBill, CusSCAHouse houseBill, CommonShipment shipment)
		{
			UpdateOceanBillContainerList(oceanBill);
			DefaultHouseDetails(houseBill, shipment);
			DefaultHouseContainers(houseBill, shipment);
		}

		void UpdateOceanBillContainerList(CusSCAOceanBill oceanBill)
		{
			if (Consol != null && Consol.Containers.Count > 0)
			{
				foreach (CommonContainer aContainer in Consol.Containers)
				{
					if (aContainer.Container != null && oceanBill.Containers.Find(aContainer.JC_ContainerNum) == null)
					{
						var newContainer = oceanBill.Containers.AddNew();
						newContainer.CN_ContainerNumber = aContainer.JC_ContainerNum;
						newContainer.CN_SealNumber = (aContainer.JC_SealNum.Length > newContainer.CN_SealNumberInfo.MaxLength) ? aContainer.JC_SealNum.Substring(0, newContainer.CN_SealNumberInfo.MaxLength) : aContainer.JC_SealNum;
						newContainer.CN_RC_NKContainerType = aContainer.Container.RC_Code;
						newContainer.CN_ContainerMode = ConvertContainerMode(aContainer.JC_ContainerMode);
					}
				}
			}

			if (Consol.JK_ConsolMode == Enterprise.Core.Constants.ContainerModes.Bulk
				|| Consol.JK_ConsolMode == Enterprise.Core.Constants.ContainerModes.Liquid)
			{
				EnsureBulkContainerLineExists(oceanBill);
			}
			else if (Consol.JK_ConsolMode == Enterprise.Core.Constants.ContainerModes.BreakBulk
				|| Consol.JK_ConsolMode == Enterprise.Core.Constants.ContainerModes.RollOnRollOff)
			{
				EnsureBreakBulkContainerLineExists(oceanBill);
			}
		}

		void EnsureBulkContainerLineExists(CusSCAOceanBill oceanBill)
		{
			if (oceanBill.Containers.Find(CusSCAPivot.Bulk) == null)
			{
				var bulkContainerLine = oceanBill.Containers.AddNew();
				bulkContainerLine.CN_ContainerNumber = CusSCAPivot.Bulk;
				bulkContainerLine.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			}
		}

		void EnsureBreakBulkContainerLineExists(CusSCAOceanBill oceanBill)
		{
			if (oceanBill.Containers.Find(CusSCAPivot.BreakBulk) == null)
			{
				var bulkContainerLine = oceanBill.Containers.AddNew();
				bulkContainerLine.CN_ContainerNumber = CusSCAPivot.BreakBulk;
				bulkContainerLine.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			}
		}

		public static ZString ManifestID(CommonShipment shipment)
		{
			ZString result = "";
			if (shipment != null)
			{
				if (shipment.CoLoadMasterShipment == null)
				{
					if (shipment.ArrivalConsol != null && shipment.ArrivalConsol.ReceivingForwarder != null)
					{
						result = shipment.ArrivalConsol.ReceivingForwarder.LocalManifestID;
						if (result.IsEmpty &&
							(GlbBranch.CurrentBranch.GB_OH_OrgProxy == shipment.ArrivalConsol.ReceivingForwarder.PK ||
							GlbCompany.CurrentCompany.GC_OH_OrgProxy == shipment.ArrivalConsol.ReceivingForwarder.PK))
						{
							result = Env.Registry.ManifestClientID;
						}
					}
				}
				else
				{
					if (shipment.CoLoadMasterShipment.Consignee != null)
					{
						result = shipment.CoLoadMasterShipment.Consignee.LocalManifestID;
					}
				}
			}
			return result;
		}

		protected virtual void DefaultHouseDetails(CusSCAHouse houseBill, CommonShipment shipment)
		{
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_CB = OceanBill.PK;
			houseBill.CA_HouseBill = shipment.JS_HouseBill;
			houseBill.CA_MasterHouseBill = HouseBillSynchroniser.GetMasterHouseBillFromShipment(shipment);
			houseBill.CA_RL_NK_PortOfOrigin = shipment.JS_RL_NKOrigin;
			houseBill.CA_RL_NK_PortOfDestination = shipment.JS_RL_NKDestination;
			houseBill.CA_RN_NKGoodsOrigin = shipment.JS_RL_NKOrigin.SubstringSafe(0, 2);
			houseBill.CA_PrepaidCollectOther = shipment.JS_PaymentTerm;
			DefaultConsigneeDetails(houseBill, shipment);
			DefaultConsignorDetails(houseBill, shipment);
			DefaultNotifyPartyDetails(houseBill, shipment);
		}

		void DefaultConsigneeDetails(CusSCAHouse houseBill, CommonShipment shipment)
		{
			houseBill.UpdateConsigneeFromShipment(shipment);
		}

		void DefaultConsignorDetails(CusSCAHouse houseBill, CommonShipment shipment)
		{
			houseBill.UpdateConsignorFromShipment(shipment);
		}

		void DefaultNotifyPartyDetails(CusSCAHouse houseBill, CommonShipment shipment)
		{
			houseBill.UpdateNotifyFromShipment(shipment);
		}

		void DefaultHouseContainer(CusSCAHouse houseBill, PackLine packLine)
		{
			CommonContainer jobContainer = packLine.GetContainer(Consol);
			if (jobContainer != null)
			{
				CusSCAPivot containerPivot = houseBill.Pivot.FromContainerNumber(jobContainer.JC_ContainerNum)
					?? houseBill.Pivot.AddNew();
				CusSCAContainer oceanBillContainer = houseBill.OceanBill.Containers.Find(jobContainer.JC_ContainerNum);
				if (oceanBillContainer != null)
				{
					CommonShipment shipment = packLine.Shipment;
					containerPivot.CV_CN = oceanBillContainer.PK;
				}
			}
		}

		protected void DefaultHouseContainers(CusSCAHouse houseBill, CommonShipment shipment)
		{
			if (shipment.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || shipment.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
			{
				DefaultBulkBreakBulk(houseBill, shipment, CusSCAPivot.BreakBulk, CMRImportCargoTypes.Codes.BreakBulk);
			}
			else if (shipment.JS_PackingMode == Core.Constants.ContainerModes.Bulk || shipment.JS_PackingMode == Core.Constants.ContainerModes.Liquid)
			{
				DefaultBulkBreakBulk(houseBill, shipment, CusSCAPivot.Bulk, CMRImportCargoTypes.Codes.Bulk);
			}
			else
			{
				foreach (PackLine aPackLine in shipment.OuterPackLines)
				{
					DefaultHouseContainer(houseBill, aPackLine);
				}
			}
		}

		protected void DefaultBulkBreakBulk(CusSCAHouse houseBill, CommonShipment shipment, string containerMode, ZString sCAContainerMode)
		{
			var containerPivot = houseBill.Pivot.FromContainerNumber(containerMode);
			if (containerPivot == null)
			{
				containerPivot = houseBill.Pivot.AddNew();
				containerPivot.CV_AssociatedContainer = containerMode;
			}
			containerPivot.CN_ContainerMode = sCAContainerMode;
			containerPivot.CV_GoodsDescription = shipment.JS_GoodsDescription;
			if (containerMode == CusSCAPivot.BreakBulk)
			{
				containerPivot.CV_PackageCount = (ZShort)((int)shipment.JS_OuterPacks & 0xFFFF);
			}
			containerPivot.CV_PackageType = SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(shipment.JS_F3_NKPackType);
			ZString marksAndNumbersNote = MarksAndNumbersNoteTextFromShipment(shipment);
			containerPivot.CV_MarksAndNumbers = marksAndNumbersNote.Replace("\r", "").Replace("\n", " ").Replace("\t", " ").SubstringSafe(0, 350);
			containerPivot.CV_Weight = shipment.JS_ActualWeight;
			containerPivot.CV_WeightUQ = shipment.JS_UnitOfWeight;
			if (shipment.JS_UnitOfVolume == Enterprise.Core.Constants.Volume.CubicMetres)
			{
				containerPivot.CV_Volume = shipment.JS_ActualVolume;
			}
			else
			{
				if (shipment.JS_ActualVolume != 0m)
				{
					containerPivot.CV_Volume = Enterprise.Core.Constants.Volume.Convert(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume,
						Enterprise.Core.Constants.Volume.CubicMetres);
				}
			}
		}

		protected ZString MarksAndNumbersNoteTextFromShipment(CommonShipment shipment)
		{
			ZString result = "";
			if (shipment != null)
			{
				StmNote[] notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				foreach (StmNote note in notes)
				{
					result += note.ST_NoteText;
				}
			}
			return result;
		}

		protected void HookHouseBillSynchronisers(CommonShipment shipment, CusSCAHouse house)
		{
			if (!SynchroniserExists(house))
			{
				var houseBillSynch = GetNewHouseBillSynchroniser(shipment, house);
				houseBillSynch.DestinationDeleted += HouseBillSynch_DestinationDeleted;
				BusinessObjectSynchronisers.Add(houseBillSynch);
				if (!house.IsInDatabase)
				{
					houseBillSynch.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}
				HookOceanBillSynchronisers();
			}
		}

		protected abstract HouseBillSynchroniser GetNewHouseBillSynchroniser(CommonShipment shipment, CusSCAHouse house);

		OceanBillSynchroniser GetOceanBillSynchroniser(CusSCAOceanBill oceanBill, CommonConsol consol)
		{
			if (fOceanBillSynchroniser == null)
			{
				fOceanBillSynchroniser = GetNewOceanBillSynchroniser(oceanBill, consol);
				if (fOceanBill.IsInDatabase)
				{
					fOceanBillSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
				}
				else
				{
					fOceanBillSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				}
				BusinessObjectSynchronisers.Add(fOceanBillSynchroniser);
			}
			return fOceanBillSynchroniser;
		}
		OceanBillSynchroniser fOceanBillSynchroniser;

		protected virtual OceanBillSynchroniser GetNewOceanBillSynchroniser(CusSCAOceanBill oceanBill, CommonConsol consol)
		{
			return new OceanBillSynchroniser(oceanBill, consol);
		}

		protected virtual bool ShouldStartHouseSynchronisers(CusSCAHouse house)
		{
			return IsEnabled && house.CA_ShipmentStatus.IsEmpty;
		}

		protected void HookOceanBillSynchronisers()
		{
			if (!oceanBillSynchronisersHooked && synchroniseConsol)
			{
				oceanBillSynchronisersHooked = true;
				GetOceanBillSynchroniser(OceanBill, Consol);
			}
		}
		bool oceanBillSynchronisersHooked;

		protected bool SynchroniserExists(BusinessObject bizO)
		{
			var result = false;

			foreach (var synchroniser in BusinessObjectSynchronisers)
			{
				if (synchroniser is HouseBillSynchroniser houseSynch && houseSynch.Destination == bizO
				 || synchroniser is OceanBillSynchroniser oceanSynch && oceanSynch.Destination == bizO)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		void StopAllSynchronisers()
		{
			foreach (BusinessObjectSynchroniser synchroniser in BusinessObjectSynchronisers)
			{
				synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
			}
		}

		void EnableAllSynchronisers()
		{
			foreach (BusinessObjectSynchroniser synchroniser in BusinessObjectSynchronisers)
			{
				if (!synchroniser.IsEnabled)
				{
					synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
				}
			}
		}

		#region Event Handlers

		void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			StopAllSynchronisers();
		}

		void StopSynchronisationOnManualStatusOverrideAccepted(object sender, EventArgs e)
		{
			StopAllSynchronisers();
		}

		void EnableSynchronisationOnManualStatusOverrideReset(object sender, EventArgs e)
		{
			EnableAllSynchronisers();
		}

		void HouseBillSynch_DestinationDeleted(object sender, EventArgs e)
		{
			if (sender is HouseBillSynchroniser houseBillSynch)
			{
				houseBillSynch.DestinationDeleted -= HouseBillSynch_DestinationDeleted;
				BusinessObjectSynchronisers.Remove(houseBillSynch);
				houseBillSynch.Dispose();
			}
		}

		#endregion

		#endregion
	}
}
