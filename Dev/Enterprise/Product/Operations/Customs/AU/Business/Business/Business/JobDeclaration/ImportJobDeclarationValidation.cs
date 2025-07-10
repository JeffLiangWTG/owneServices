using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportJobDeclarationValidation : JobDeclarationValidation
	{
		public ImportJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();

			if (IsPortOfLoadingMandatory)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKPortOfLoadingInfo);
				if (JobDeclaration.PortOfLoading != null && JobDeclaration.PortOfLoading.RL_RN_NKCountryCode == "AU")
				{
					JobDeclaration.JE_RL_NKPortOfLoadingInfo.AddMessageError("Port of loading must not be an Australian port.");
				}
			}
		}

		protected virtual bool IsPortOfLoadingMandatory
		{
			get { return !JobDeclaration.IsNature30; }
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (!JobDeclaration.IsNature30)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_ExportDateInfo);
			}
			if (JobDeclaration.JE_ExportDate.IsValid && JobDeclaration.JE_ExportDate.Date > (JobDeclaration.JE_EDITransmitDate.IsEmpty ? ZDateTime.Today : JobDeclaration.JE_EDITransmitDate.Date))
			{
				foreach (JobComInvoiceHeader header in JobDeclaration.Invoices)
				{
					if (!header.JZ_ValuationDateOverride.IsValid)
					{
						JobDeclaration.JE_ExportDateInfo.AddMessageError("The shipment export date is used as the date of valuation for the goods and cannot be in the future.");
						break;
					}
				}
			}
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (JobDeclaration.JE_DateOfArrival.IsValid)
			{
				if (JobDeclaration.JE_DateOfArrival.Date > ZDateTime.Today.AddMonths(3))
				{
					JobDeclaration.JE_DateOfArrivalInfo.AddMessageError("DTAR must not be more than three months in the future.");
				}
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			var info = JobDeclaration.JE_OH_ImporterInfo;
			MessageValidation.CheckEntered(info);

			if (JobDeclaration.Importer != null)
			{
				if (JobDeclaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.JZ_IncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid) && JobDeclaration.JE_OH_Importer != JobDeclaration.JE_OH_Supplier)
				{
					info.AddWarning(Res.GetString("F008C8ED-2AD8-4524-AAE6-8D54B70C2919", "An invoice on this job has been entered under a DDP transaction but the Importer of Record does not match the Supplier listed on the Declaration."));
				}

				if (ShouldCheckCMRImporter)
				{
					var customsClientID = JobDeclaration.Importer.GetCustomsClientID();
					if (customsClientID.IsEmpty && JobDeclaration.Importer.LocalBusinessRegNo.IsEmpty)
					{
						info.AddMessageError(Res.GetString("54E3F696-5871-4A6F-8CC8-A39263B6E146", "ABN or the CID must be entered for an Importer."));
					}
					else if (JobDeclaration.Importer.LocalBusinessRegNo.IsEmpty && !customsClientID.IsEmpty && customsClientID.Length != 11)
					{
						info.AddMessageError(Res.GetString("17B958D9-879E-461F-8D49-442E2BE4C582", "Importer CID must be 11 characters long."));
					}
					else if (!JobDeclaration.Importer.LocalBusinessRegNo.IsEmpty)
					{
						var splitter = new ABNCACSplitter(JobDeclaration.Importer.LocalBusinessRegNo);

						if (splitter.ABN.Length != 11 && splitter.ABN.Length + splitter.CAC.Length != 14)
						{
							info.AddMessageError(Res.GetString("921B4EBE-3589-44B6-85C4-C2279F8382C8", "Importer ABN must be 11 or 14 characters long. If 14 characters, the last 3 digits must be the CAC."));
						}
					}
				}
			}
		}

		protected virtual bool ShouldCheckCMRImporter
		{
			get { return false; }
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (!JobDeclaration.IsNature30 && (JobDeclaration.IsSea || JobDeclaration.IsAir))
			{
				if (IsVoyageFlightNoMandatory)
				{
					MessageValidation.CheckEntered(JobDeclaration.JE_VoyageFlightNoInfo);
				}
				if (!JobDeclaration.JE_VoyageFlightNoInfo.HasNotifications())
				{
					if (JobDeclaration.IsAir)
					{
						ZString warningMessage = FlightNoValidation.ValidateFlightNo(JobDeclaration.JE_VoyageFlightNoInfo);

						if (!warningMessage.IsEmpty)
						{
							JobDeclaration.JE_VoyageFlightNoInfo.AddWarning(warningMessage);
						}
					}
					else if (JobDeclaration.IsSea)
					{
						if (IsVoyageFlightNoUsedInMessages)
						{
							if (JobDeclaration.JE_VoyageFlightNo.Length > 6)
							{
								JobDeclaration.JE_VoyageFlightNoInfo.AddMessageError("The voyage number must consist of 6 or less characters.");
							}
							else if (JobDeclaration.CleanVoyageNumber.Length == 0)
							{
								JobDeclaration.JE_VoyageFlightNoInfo.AddMessageError("The voyage number must consist of at least 1 numerical character.");
							}
						}
					}
				}
			}
		}

		protected virtual bool IsVoyageFlightNoMandatory
		{
			get { return true; }
		}

		protected virtual bool IsVoyageFlightNoUsedInMessages
		{
			get { return true; }
		}

		protected override void CheckJE_OwnerRef()
		{
			base.CheckJE_OwnerRef();

			if (JobDeclaration.JE_OwnerRef.IsEmpty && !JobDeclaration.AutoAssignImporterRef)
			{
				JobDeclaration.JE_OwnerRefInfo.AddMessageError("Owner reference required for sending import messages.");
			}
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKFinalDestinationInfo);
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			base.CheckJE_RL_NKPortOfFirstArrival();
			if (!JobDeclaration.IsNature30)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_RL_NKPortOfFirstArrivalInfo);
			}
		}
		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (JobDeclaration.IsSea && !JobDeclaration.IsNature30 && JobDeclaration.VesselNumber.IsEmpty)
			{
				JobDeclaration.JE_VesselNameInfo.AddMessageError("A vessel with a Lloyds number, or a Customs allocated ship number is required for declaration to customs");
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			base.CheckJE_DateOfFirstArrival();
			if (!JobDeclaration.IsNature30)
			{
				MessageValidation.CheckEntered(JobDeclaration.JE_DateOfFirstArrivalInfo);
				if (JobDeclaration.JE_DateOfFirstArrival.IsValid)
				{
					if (!JobDeclaration.IsWarehousedByExternalAgent && JobDeclaration.JE_DateOfFirstArrival < ZDateTime.Today.AddMonths(-6))
					{
						JobDeclaration.JE_DateOfFirstArrivalInfo.AddMessageError("The date of first arrival should not be more than 6 months ago.");
					}
					else if (JobDeclaration.JE_DateOfFirstArrival > ZDateTime.Today.AddMonths(3))
					{
						JobDeclaration.JE_DateOfFirstArrivalInfo.AddMessageError("The date of first arrival should not be more than 3 months in the future.");
					}
				}
			}
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();

			if (JobDeclaration != null && JobDeclaration.IsSAC && JobDeclaration.IsTransportModeOther)
			{
				JobDeclaration.JE_TransportModeInfo.AddMessageError("'Other' is not a valid transport mode for a SAC Declaration");
			}
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();

			if (JobDeclaration != null && JobDeclaration.IsSAC && JobDeclaration.IsUPEDeclaration)
			{
				JobDeclaration.JE_MessageSubTypeInfo.AddMessageError("A Self-Assessed Clearance cannot be used for goods that are Unaccompanied Personal Effects");
			}
		}

		public override bool IsMasterBillMandatory
		{
			get { return JobDeclaration.IsSea || JobDeclaration.IsAir; }
		}

		protected override void CheckJE_TotalNoOfPieces()
		{
			base.CheckJE_TotalNoOfPieces();
			if (JobDeclaration != null && JobDeclaration.IsSea && JobDeclaration.IsPackingInformationRelevant)
			{
				if (JobDeclaration.JE_TotalNoOfPieces != JobDeclaration.PackagesOuterPackageCount)
				{
					JobDeclaration.JE_TotalNoOfPiecesInfo.AddWarning(UnitsPackingUnitsWarning);
				}
			}
		}
		public const string UnitsPackingUnitsWarning = "The Units entered on the Declaration does not equal the total Outer Packing Unit count entered on the packing tab.";
	}
}
