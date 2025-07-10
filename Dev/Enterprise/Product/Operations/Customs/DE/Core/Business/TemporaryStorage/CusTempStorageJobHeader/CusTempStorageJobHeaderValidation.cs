using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class CusTempStorageJobHeaderValidation : EU.Business.CusTempStorage.CusTempStorageJobHeaderValidation
	{
		protected CusTempStorageJobHeaderValidation(CusTempStorageJobHeader header) : base(header)
		{
		}

		public new CusTempStorageJobHeader Parent => (CusTempStorageJobHeader)base.Parent;

		protected override void CheckSJH_ReferenceNumber()
		{
			base.CheckSJH_ReferenceNumber();

			if (Parent.SJH_ReferenceNumber.IsEmpty)
			{
				var warningMessage = Parent.SJH_JobReference.IsEmpty
					? Res.GetString("9c96bbb8-677a-4499-bcb6-67e578c1387a|WhenJobNumberNotKnown", "If Customer Reference is empty, Job Number will be determined as Local Reference Number and sent to Customs.")
					: Res.GetString("9c96bbb8-677a-4499-bcb6-67e578c1387a|WhenJobNumberKnown", "If Customer Reference is empty, Job Number ({0}) will be determined as Local Reference Number and sent to Customs.", Parent.SJH_JobReference);
				Parent.SJH_ReferenceNumberInfo.AddWarning(warningMessage);
			}
		}

		protected override void CheckSJH_TransportMode()
		{
			base.CheckSJH_TransportMode();
			var previousReferenceType = Parent.SJH_PreviousReferenceType;
			if (previousReferenceType == PreviousReferenceType.Codes._ESUMA || previousReferenceType == PreviousReferenceType.Codes._ENST2L)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_TransportModeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_TransportModeInfo);
		}

		protected override void CheckSJH_TransportMeansCode()
		{
			base.CheckSJH_TransportMeansCode();

			CheckSJH_TransportMeansCode_Mandatory();
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_TransportMeansCodeInfo);

			if (Parent.SJH_TransportMode != ZString.Empty && Parent.SJH_TransportMeansCode != ZString.Empty)
			{
				var allowedTransportMeans = new List<ZString>();
				switch (Parent.SJH_TransportMode)
				{
					case TransportTypeList.Codes.Sea:
					case TransportTypeList.Codes.InlandWaterwayTransport:
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Vessel);
						break;
					case TransportTypeList.Codes.Rail:
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Wagon);
						break;
					case TransportTypeList.Codes.Road:
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Truck);
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Car);
						break;
					case TransportTypeList.Codes.Air:
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Aircraft);
						break;
					case TransportTypeList.Codes.Mail:
					case TransportTypeList.Codes.FixedTransportInstallations:
					case TransportTypeList.Codes.OwnPropulsion:
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Without);
						allowedTransportMeans.Add(TemporaryStorageTransportMeansList.Codes.Other);
						break;
				}
				if (!allowedTransportMeans.Contains(Parent.SJH_TransportMeansCode))
				{
					Parent.SJH_TransportMeansCodeInfo.AddMessageError(Res.GetString("330EE5E4-830F-41D2-99F9-2F4A65F65C08", "When transport type is '{0}', transport means only allow values in '{1}'", Parent.SJH_TransportMode, string.Join("', '", allowedTransportMeans)));
				}
			}
		}

		protected virtual void CheckSJH_TransportMeansCode_Mandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_TransportMeansCodeInfo);
		}

		protected override void CheckSJH_TransportRegNo()
		{
			base.CheckSJH_TransportRegNo();
			var mandatoryMsg = ZString.Empty;
			switch (Parent.SJH_TransportMeansCode)
			{
				case TemporaryStorageTransportMeansList.Codes.Aircraft:
					mandatoryMsg = Res.GetString("25b9965f-6612-4547-8f71-3ea37dbc4e9b", "Flight Number");
					var transportRegNo = Parent.SJH_TransportRegNo;
					if (!transportRegNo.IsEmpty && !FlightCodeValidator.IsValid(transportRegNo))
					{
						Parent.SJH_TransportRegNoInfo.AddWarning(FlightCodeWarningMessage);
					}
					break;
				case TemporaryStorageTransportMeansList.Codes.Wagon:
				case TemporaryStorageTransportMeansList.Codes.Truck:
				case TemporaryStorageTransportMeansList.Codes.Car:
					mandatoryMsg = Res.GetString("314dc38e-aa5d-43ae-b8e2-04e35af3e6d2", "Transport Registration Number");
					break;
				case TemporaryStorageTransportMeansList.Codes.Vessel:
					mandatoryMsg = Res.GetString("c23cdef7-32a2-49af-9565-c5671d745cc6", "Vessel");
					ListValidation.MessageErrorIfInvalidCode(Parent.SJH_TransportRegNoInfo, Parent.Lookups.RefVesselList);
					break;
			}
			if (!mandatoryMsg.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_TransportRegNoInfo, mandatoryMsg);
			}
		}

		protected override void CheckSJH_RL_NKLoading()
		{
			base.CheckSJH_RL_NKLoading();
			if (Parent.SJH_TransportMeansCode == TemporaryStorageTransportMeansList.Codes.Aircraft)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_RL_NKLoadingInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.SJH_RL_NKLoadingInfo);
		}

		protected override void CheckSJH_CustomsOffice()
		{
			base.CheckSJH_CustomsOffice();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SJH_CustomsOfficeInfo);
		}

		protected override void CheckSJH_OA_Presenter()
		{
			base.CheckSJH_OA_Presenter();
			var presenter = Parent.Presenter;
			if (presenter != null)
			{
				var eoriCodeMissing = Parent.PresenterEoriNumber.IsEmpty;
				var eoriBranchMissing = Parent.PresenterEoriBranch.IsEmpty;

				if (Parent.SJH_PreviousReferenceType == PreviousReferenceType.Codes._N355 && eoriCodeMissing)
				{
					Parent.SJH_OA_PresenterInfo.AddMessageError(Res.GetString("E810503D-BB9A-4DA8-B484-51D129AB97B3", "Presenter must have a Registration Number / Code of Type 'EOR'."));
				}
				if ((eoriCodeMissing || eoriBranchMissing)
					&& (presenter.OA_Address1.IsEmpty || presenter.OA_RN_NKCountryCode.IsEmpty || presenter.OA_PostCode.IsEmpty || presenter.OA_City.IsEmpty))
				{
					Parent.SJH_OA_PresenterInfo.AddMessageError(Res.GetString("F2B6AF59-B754-44C2-B7FC-5C60A922C3F3", "Address, Country/Region, City and Post Code must be entered as Presenter is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeMissing, eoriBranchMissing)));
				}
				if (Parent.Representative == null)
				{
					if (presenter.OA_OH != GlbBranch.CurrentBranch.GB_OH_OrgProxy)
					{
						Parent.SJH_OA_PresenterInfo.AddMessageError(Res.GetString("0B23ECE5-5A2A-48DC-B087-6EC6158881CA", "Presenter must equal your own Organization when Representative is empty."));
					}
					if (AtlasNumberMissing(presenter.Header))
					{
						Parent.SJH_OA_PresenterInfo.AddMessageError(Res.GetString("7BD7FA9D-3539-4DDC-AB50-752055341279", "Presenter must have an ATLAS Participant Identification Number when Representative is empty."));
					}
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SJH_OA_PresenterInfo);
			}
		}

		protected override void CheckSJH_OA_Representative()
		{
			base.CheckSJH_OA_Representative();
			var representative = Parent.Representative;
			var presenter = Parent.Presenter;
			if (representative != null)
			{
				if (representative.OA_OH != GlbBranch.CurrentBranch.GB_OH_OrgProxy)
				{
					Parent.SJH_OA_RepresentativeInfo.AddMessageError(Res.GetString("254BC739-22A0-41F5-90B3-728421C40BF9", "Representative must equal your own Organization."));
				}
				var eoriCodeMissing = Parent.RepresentativeEoriNumber.IsEmpty;
				var eoriBranchMissing = Parent.RepresentativeEoriBranch.IsEmpty;
				if (eoriCodeMissing || eoriBranchMissing)
				{
					Parent.SJH_OA_RepresentativeInfo.AddMessageError(Res.GetString("1EF2FE46-FDED-40C9-9990-D55085E62C53", "Representative is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeMissing, eoriBranchMissing)));
				}
				if (presenter != null && EoriNumbersMatch())
				{
					Parent.SJH_OA_RepresentativeInfo.AddMessageError(Res.GetString("2E882930-EC07-46A3-AF01-00337ED835E2", "Presenter and Representative may not have the same EORI number."));
				}
				if (AtlasNumberMissing(representative.Header))
				{
					Parent.SJH_OA_RepresentativeInfo.AddMessageError(Res.GetString("8A0C0E04-0B30-4FEE-91AB-438C5010C9A2", "Representative must have an ATLAS Participant Identification Number."));
				}
			}
			else if (presenter != null && (Parent.PresenterEoriNumber.IsEmpty || Parent.PresenterEoriBranch.IsEmpty))
			{
				Parent.SJH_OA_RepresentativeInfo.AddMessageError(Res.GetString("BAE6BFB3-5769-4E7B-98F0-1C8E05C27FCB", "Representative is required to be entered as the Presenter is missing EORI number or branch."));
			}

			bool EoriNumbersMatch()
			{
				var presenterEoriNum = presenter?.Header?.GetEUEoriDetails() ?? ZString.Empty;
				var representativeEoriNum = representative?.Header?.GetEUEoriDetails() ?? ZString.Empty;
				return !presenterEoriNum.IsEmpty && !representativeEoriNum.IsEmpty && presenterEoriNum == representativeEoriNum;
			}
		}

		bool AtlasNumberMissing(OrgHeader orgHeader)
		{
			var atlasCode = ZString.Empty;
			if (orgHeader != null)
			{
				atlasCode = orgHeader.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, Core.Constants.CountryCodes.Germany);
			}
			return atlasCode.IsEmpty;
		}

		string FlightCodeWarningMessage => Res.GetString("40451D94-320E-4958-9615-34435843D555",
@"Flight numbers have a specific set of rules which are followed by all airlines.
The first 2 characters of the Flight No. must start with:
- A letter followed by a number
- A number followed by a letter
- Two letters
And must then be followed by between 1 and 4 numbers.
And an optional letter.");
	}
}
