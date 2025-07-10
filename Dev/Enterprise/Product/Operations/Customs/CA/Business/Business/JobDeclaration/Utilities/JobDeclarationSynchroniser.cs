using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		public new JobDeclaration Destination
		{
			get { return (JobDeclaration)base.Destination; }
		}

		#region HookCollectionSynchronisers

		void HookCollectionSynchronisers()
		{
			HookCusEntryNumbersSynchronisers();
		}

		#region HookCusEntryNumbersSynchronisers

		void HookCusEntryNumbersSynchronisers()
		{
			var cusEntryNumbers = from CusEntryNumber number in Source.Numbers
								  where number.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN
										|| number.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.PCN
								  select number;

			foreach (var source in cusEntryNumbers)
			{
				var numbers = Destination.AdditionalReferenceNumbers.Find(new ZQuery(CusEntryNumSchema.CE_EntryType, source.CE_EntryType));
				var destination = !numbers.Any() ? Destination.AdditionalReferenceNumbers.AddNew() : (CusEntryNumber)numbers[0];
				Synchronisers.Add(new FieldSynchroniser(destination.CE_EntryNumInfo, source.CE_EntryNumInfo, true));
				Synchronisers.Add(new FieldSynchroniser(destination.CE_EntryTypeInfo, source.CE_EntryTypeInfo, true));
			}
		}

		#endregion

		#endregion

		#region HookFieldSynchronisers

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!SyncChangesDetected)
			{
				DefaultForwarderToCurrentBranchOrganisationIfNotEmpty();
				if (SyncChangesDetected)
				{
					return;
				}
				HookCollectionSynchronisers();
			}
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.CA_PortOfExitInfo, GetPortOfExit, GetPortOfExitRelatedInfos, true));
			ConsolFieldSynchronisers.Add(new FieldSynchroniser(Destination.CA_PlaceOfReportInfo, GetPlaceOfReport, GetPlaceOfReportRelatedInfos, true));
		}

		#region CA_TransportDocumentNumber

		ZString GetTransportDocumentNumber()
		{
			var transportDocumentNumber = ZString.Empty;
			if (Destination.IsExport)
			{
				if (Destination.IsSea && hookedConsol != null)
				{
					transportDocumentNumber = hookedConsol.JK_BookingReference;
				}
				else if (Destination.IsAir)
				{
					transportDocumentNumber = (hookedConsol != null && !hookedConsol.JK_MasterBillNum.IsEmpty) ? hookedConsol.JK_MasterBillNum : Source.JS_HouseBill;
				}
			}
			return transportDocumentNumber;
		}

		#endregion

		#region JE_OH_Forwarder

		void DefaultForwarderToCurrentBranchOrganisationIfNotEmpty()
		{
			if (Destination.JE_OH_Forwarder.IsEmpty)
			{
				var forwarder = GetCurrentBranchOrgPKIfItsFlagAsForwarder();
				if (DetectEnabled)
				{
					if (Destination.JE_OH_Forwarder != forwarder)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					Destination.JE_OH_Forwarder = forwarder;
				}
			}
		}

		ZGuid GetCurrentBranchOrgPKIfItsFlagAsForwarder()
		{
			var org = GlbBranch.CurrentBranch.OrgProxy;
			if (org != null && org.OH_IsForwarder)
			{
				return org.PK;
			}
			return ZGuid.Empty;
		}

		#endregion

		#region JK_RL_NKLoadPort

		protected override IZType GetPortOfLoading()
		{
			var result = ZString.Empty;
			if (hookedConsol != null)
			{
				result = Destination.IsImport ? hookedConsol.JK_RL_NKLoadForImportTransport : hookedConsol.JK_RL_NKLoadPort;
			}
			return result;
		}

		protected override ZPropertyInfo[] GetPortOfLoadingRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadPortInfo);
				infos.Add(hookedConsol.JK_RL_NKLoadForImportTransportInfo);
			}
			return infos.ToArray();
		}

		#endregion

		#region CA_PortOfExit

		IEnumerable<ZPropertyInfo> GetPortOfExitRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			infos.Add(Destination.JE_RL_NKPortOfLoadingInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadForExportTransportInfo);
			}
			return infos;
		}

		IZType GetPortOfExit()
		{
			var result = Destination.CA_PortOfExit;
			if (Destination.IsExport && result.IsEmpty)
			{
				var portCode = (hookedConsol != null && !hookedConsol.JK_RL_NKLoadForExportTransport.IsEmpty) ? hookedConsol.JK_RL_NKLoadForExportTransport : Destination.JE_RL_NKPortOfLoading;
				var unloco = Destination.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode);
				if (unloco != null)
				{
					var portOfExit = Destination.GetPortOfficeFromUnLoco(unloco);
					if (!portOfExit.IsEmpty)
					{
						result = portOfExit;
					}
				}
			}
			return result;
		}

		#endregion

		#region CA_PlaceOfReport

		IEnumerable<ZPropertyInfo> GetPlaceOfReportRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			infos.Add(Destination.JE_RL_NKPortOfLoadingInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKLoadForExportTransportInfo);
			}
			return infos;
		}

		IZType GetPlaceOfReport()
		{
			var result = Destination.CA_PlaceOfReport;
			if (Destination.IsExport && result.IsEmpty)
			{
				result = CACustomsDataRegistry.Instance.DefaultPlaceOfReport.Value;
				if (result.IsEmpty)
				{
					var portCode = (hookedConsol != null && !hookedConsol.JK_RL_NKLoadForExportTransport.IsEmpty) ? hookedConsol.JK_RL_NKLoadForExportTransport : Destination.JE_RL_NKPortOfLoading;
					var unloco = Destination.Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode);
					if (unloco != null)
					{
						var placeOfReport = Destination.GetPortOfficeFromUnLoco(unloco);
						if (!placeOfReport.IsEmpty)
						{
							result = placeOfReport;
						}
					}
				}
			}
			return result;
		}

		#endregion

		#region DateOfExport

		//TODO: Check and remove this unused methods

		//IEnumerable<ZPropertyInfo> GetDateOfExportRelatedInfos()
		//{
		//    var infos = new List<ZPropertyInfo>();
		//    infos.Add(Destination.JE_ExportDateInfo);
		//    if (hookedConsol != null)
		//    {
		//        infos.Add(hookedConsol.JK_JX_JA_E_DEPInfo);
		//        infos.Add(hookedConsol.JK_JX_JA_A_DEPInfo);
		//    }
		//    return infos;
		//}

		//IZType GetDateOfExport()
		//{
		//    if (hookedConsol != null)
		//    {
		//        Transport transport = hookedConsol.Transports.ExportTransport;
		//        if (transport != null)
		//        {
		//            if (!transport.JW_ATD.IsEmpty)
		//            {
		//                return transport.JW_ATD;
		//            }
		//            if (!transport.JW_ETD.IsEmpty)
		//            {
		//                return transport.JW_ETD;
		//            }
		//        }
		//    }
		//    return Destination.JE_ExportDate;
		//}

		#endregion

		#region DateOfArrival

		protected override ZPropertyInfo[] GetDateOfArrivalRelatedInfos()
		{
			var infos = new List<ZPropertyInfo>(base.GetDateOfArrivalRelatedInfos());
			infos.Add(Destination.JE_MessageTypeInfo);
			if (!infos.Contains(Source.JS_E_ARVInfo))
			{
				infos.Add(Source.JS_E_ARVInfo);
			}
			return infos.ToArray();
		}

		protected override IZType GetDateOfArrival()
		{
			IZType result = ZDateTime.Empty;
			if (Destination.IsExport && hookedConsol != null)
			{
				var transport = hookedConsol.Transports.ImportTransport;
				if (transport != null)
				{
					if (!transport.JW_ATA.IsEmpty)
					{
						result = transport.JW_ATA;
					}
					else if (!transport.JW_ETA.IsEmpty)
					{
						result = transport.JW_ETA;
					}
				}
			}
			if (result.IsEmpty)
			{
				result = base.GetDateOfArrival();
			}
			return result;
		}

		#endregion

		protected override ZString GetConvertedPackType(ZString packType)
		{
			var result = ZString.Empty;
			if (Destination.IsIID)
			{
				var mappings = CACustomsDataRegistry.Instance.CAPackageTypesMapping.GetValueWithoutFallback(Destination.RegistryCompanyPK, Guid.Empty, Guid.Empty);
				result = mappings.GetMappedPackageType(packType);
			}
			if (result.IsEmpty)
			{
				result = base.GetConvertedPackType(packType);
			}
			return result;
		}

		#endregion

		protected override void OnSynchronised()
		{
			base.OnSynchronised();
			if (Destination.CA_TransportDocumentNumber.IsEmpty)
			{
				var bill = GetTransportDocumentNumber();
				if (DetectEnabled)
				{
					if (!Destination.CA_TransportDocumentNumber.EqualsIgnoringCase(Destination.FormatAirwayBillIfApplicable(bill)))
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					Destination.SetBillTransportDocumentNumber(bill);
				}
			}
		}

		protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser()
		{
			return new PackingSynchroniser(this, Destination);
		}

		protected override Customs.Business.BillsSynchroniser GetBillsSynchroniser()
		{
			return new BillsSynchroniser(Destination, GetHouseBillOfSpecificShipment);
		}

		protected override PackLineSynchronisersArg GetPackLineSynchronisersArgs()
		{
			return new CAPackLineSynchronisersArg();
		}

		protected override void InitializePackLineSynchronisersArgs(PackLineSynchronisersArg args)
		{
			base.InitializePackLineSynchronisersArgs(args);

			if (args is CAPackLineSynchronisersArg caSyncArgs)
			{
				caSyncArgs.GetCargoControlNumberSynchroniser = () => new CargoControlNumberCollectionSynchroniser(Source, Destination);
				caSyncArgs.SyncChangesDetectedForExtraSynchronisers = (detectEnabled) =>
				{
					var cargoControlNumberSynchroniser = caSyncArgs.GetCargoControlNumberSynchroniser();
					if (cargoControlNumberSynchroniser != null)
					{
						cargoControlNumberSynchroniser.DetectEnabled = detectEnabled;
						cargoControlNumberSynchroniser.Synchronise();
						return cargoControlNumberSynchroniser.SyncChangesDetected;
					}

					return false;
				};
			}
		}
	}
}
