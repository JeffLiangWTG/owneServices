using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineJobDeclarationValidation : ExportJobDeclarationValidation
	{
		public QuarantineJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		#region CheckJE_MessageType

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();

			if (JobDeclaration.Invoices.Count > 1)
			{
				JobDeclaration.JE_MessageTypeInfo.AddError(MultiInvoicesNotAllowedForExDoc);
			}
		}

		internal const string MultiInvoicesNotAllowedForExDoc = "You have entered more than one invoice. Only one invoice is allowed for an EXDOC job.";

		#endregion

		#region CheckJE_MarksAndNumbersShort

		protected override void CheckJE_MarksAndNumbersShort()
		{
			base.CheckJE_MarksAndNumbersShort();
			if (JobDeclaration.JE_MarksAndNumbers.IsEmpty &&
				JobDeclaration.QuarantineInvoice != null &&
				JobDeclaration.QuarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator == EXDOCCertificatePrintCodes.Codes.NotRequired)
			{
				JobDeclaration.JE_MarksAndNumbersShortInfo.AddMessageError("Marks and numbers are mandatory when certificate print is not required.");
			}
		}

		#endregion

		#region CheckJE_OH_Forwarder

		protected override void CheckJE_OH_Forwarder()
		{
			base.CheckJE_OH_Forwarder();
			if (JobDeclaration.QuarantineInvoice != null &&
				JobDeclaration.QuarantineInvoice.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat &&
				JobDeclaration.FinalDestination != null &&
				(JobDeclaration.FinalDestination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Japan || JobDeclaration.FinalDestination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.KoreaSouth) &&
				JobDeclaration.Forwarder == null
				&& JobDeclaration.Importer != null)
			{
				JobDeclaration.JE_OH_ForwarderInfo.AddMessageError("Forwarder must be entered for produce type meat to Japan or South Korea.");
			}

			if (JobDeclaration.Forwarder != null && JobDeclaration.Importer == null)
			{
				JobDeclaration.JE_OH_ForwarderInfo.AddMessageError("Forwarder cannot be entered when Importer is blank.");
			}
		}

		#endregion

		#region CheckJE_ExportDate

		protected override void CheckJE_ExportDate()
		{
			var quarantineInvoice = JobDeclaration.QuarantineInvoice;

			if (quarantineInvoice != null &&
				quarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator == EXDOCCertificatePrintCodes.Codes.NotRequired &&
				JobDeclaration.JE_ExportDate.IsEmpty)
			{
				JobDeclaration.JE_ExportDateInfo.AddMessageError("Departure date must be entered when certificate is not required.");
			}

			if (quarantineInvoice != null && JobDeclaration.JE_ExportDate.Date < ZDateTime.Now.Date)
			{
				var requestForPermitNumberPrintDescription = quarantineInvoice.QuarantineExDocHeader.QH_RequestForPermitNumberPrintDescription;

				if (requestForPermitNumberPrintDescription.IsEmpty || requestForPermitNumberPrintDescription == EXDOCComplianceStatusCodes.Codes.Order)
				{
					JobDeclaration.JE_ExportDateInfo.AddMessageError("Departure date cannot be earlier than todays date.");
				}
			}
		}

		#endregion

		#region CheckJE_OH_Importer

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			if (JobDeclaration.IsAQISCertificateRequest && (JobDeclaration.JE_OH_Importer.IsEmpty || JobDeclaration.Importer == null || JobDeclaration.JE_ToOrder))
			{
				JobDeclaration.JE_OH_ImporterInfo.AddMessageError("An Importer should be specified for a Certificate Request AQS declaration, and it cannot be 'To Order'.");
			}

			var importer = JobDeclaration.Importer;
			if (importer != null)
			{
				var importWarningMessage = new ZStringBuilder();
				void CheckAndAppendWarning(string fieldName, string value, int limit)
				{
					var exceededCharacters = value.Length - limit;
					if (exceededCharacters > 0)
					{
						importWarningMessage.AppendLine($"    {fieldName} exceeds the {limit} character limit by {exceededCharacters}.");
					}
				}

				var mainAddress = importer.MainAddress;
				CheckAndAppendWarning("Importer FullName", importer.OH_FullName, 50);
				CheckAndAppendWarning("Importer Address 1", mainAddress.Address1, 35);
				CheckAndAppendWarning("Importer Address 2", mainAddress.Address2, 35);
				CheckAndAppendWarning("Importer City", mainAddress.City, 35);
				CheckAndAppendWarning("Importer State", mainAddress.State, 20);

				if (!importWarningMessage.IsEmpty)
				{
					importWarningMessage.Prepend("One or more segments of the Consignee address exceed the allowed limit and will be truncated in the message to EXDOC unless changed:\r\n");
					JobDeclaration.JE_OH_ImporterInfo.AddWarning(importWarningMessage.ToString());
				}
			}
		}

		#endregion

		#region CheckJE_OH_ShippingLine

		protected override void CheckJE_OH_ShippingLine()
		{
			base.CheckJE_OH_ShippingLine();
			if (!Parent.JE_OH_ShippingLineInfo.HasMessageErrors() && Parent.ShippingLine != null && Parent.ShippingLine.OH_FullName.Length > 35)
			{
				Parent.JE_OH_ShippingLineInfo.AddMessageError("The Shipping Line name must not be more than 35 characters long.");
			}
		}

		#endregion

		#region CheckJE_VoyageFlightNo

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (JobDeclaration.QuarantineInvoice != null &&
				(JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Air ||
				(JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Sea &&
				JobDeclaration.QuarantineInvoice.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.GrainsAndPlants)) &&
				JobDeclaration.QuarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator == EXDOCCertificatePrintCodes.Codes.NotRequired)
			{
				if (JobDeclaration.JE_VoyageFlightNo.IsEmpty)
				{
					JobDeclaration.JE_VoyageFlightNoInfo.AddMessageError("You have not entered a " + JobDeclaration.JE_VoyageFlightNoInfo.Description);
				}
			}
		}

		#endregion

		#region CheckJE_VesselName

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();
			if (JobDeclaration.QuarantineInvoice != null &&
				JobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Sea &&
				JobDeclaration.QuarantineInvoice.QuarantineExDocHeader.QH_CertificatePrintIndicator == EXDOCCertificatePrintCodes.Codes.NotRequired)
			{
				if (JobDeclaration.JE_VesselName.IsEmpty)
				{
					JobDeclaration.JE_VesselNameInfo.AddMessageError("You have not entered a " + JobDeclaration.JE_VesselNameInfo.Description);
				}
			}
		}

		#endregion
	}
}
