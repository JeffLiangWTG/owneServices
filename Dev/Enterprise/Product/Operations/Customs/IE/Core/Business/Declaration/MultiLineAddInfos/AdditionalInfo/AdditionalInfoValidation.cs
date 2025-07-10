using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_SubType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			CheckCSI_ReferenceNumber_1D23();
			CheckCSI_ReferenceNumber_BR2049();
			CheckCSI_ReferenceNumber_BR20330();
		}

		void CheckCSI_ReferenceNumber_1D23()
		{
			if (Parent.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture) && Parent.IsAnAdditionalReference)
			{
				var referenceNumber = Parent.CSI_ReferenceNumber;
				if (!referenceNumber.IsEmpty && ZDateTime.TryParseExact(referenceNumber, out var referenceDateTime, Constants.DateTimeFormat.AdditionalReferenceDateTime))
				{
					var isMRNAllocated = false;
					var parentParent = Parent.Parent;
					if (parentParent is JobComInvoiceHeader invoiceHeader)
					{
						isMRNAllocated = invoiceHeader.Entries.Any(entryHeader => !entryHeader.MovementReferenceNumber.IsEmpty);
					}
					else if (parentParent is CusEntryInstruction entryInstruction && entryInstruction.EntryHeader is CusEntryHeader entry)
					{
						isMRNAllocated = !entry.MovementReferenceNumber.IsEmpty;
					}
					if (!isMRNAllocated && referenceDateTime.IsInThePast())
					{
						Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("38EF8D5B-7DE2-404B-8C95-973BBAC2F44C", "Scheduled time of departure cannot be in the past at time of registration."));
					}
				}
				else
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("51AB207B-3CCE-4D05-93F5-5AA643FE265E", "1D23 Reference is mandatory and must be in the correct format: '{0}'", Constants.DateTimeFormat.AdditionalReferenceDateTime));
				}
			}
			else if (Parent.CSI_ReferenceNumber.IsEmpty && !Parent.CSI_ReferenceNumber_ReadOnly)
			{
				var info = Parent.CSI_ReferenceNumberInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(info)));
			}
		}

		void CheckCSI_ReferenceNumber_BR2049()
		{
			if (Parent.IsATransportDocument
				&& Parent.CSI_Code == Constants.AdditionalReferenceCodes.AuthorisationForSpecialProcedureOtherThanTransit
				&& Parent.CSI_ReferenceNumber != Constants.AdditionalReferenceNumberCodes.NAI)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("2F59FF47-9CC1-4E19-8C18-AA08162BF46C", "[BR2049] Additional Document Reference must be 'NAI' when Kind is 'TRA' and Full Type is '00100'."));
			}
		}

		void CheckCSI_ReferenceNumber_BR20330()
		{
			if (Parent.IsAnAdditionalReference
				&& Parent.CSI_Code.StartsWith(Constants.AdditionalReferenceCodes.Prefix_6)
				&& Parent.CSI_ReferenceNumber != Constants.AdditionalReferenceNumberCodes.NAI)
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("91CF915F-374A-4753-947D-FF9C6AAC567D", "[BR20330] For Additional References, Reference must be 'NAI' when Full Type starts with '6'."));
			}
		}

		protected override void CheckCSI_Description()
		{
			var message = Res.GetString("ECDA776D-AB42-49E3-A7AB-1A9EBDFA9AA9", "You have not entered an Additional Document Description.");
			if (Parent.Declaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
			{
				if (Parent.CSI_Description.IsEmpty && !Parent.CSI_Description_ReadOnly && Parent.CSI_Code.IsEmpty)
				{
					var info = Parent.CSI_DescriptionInfo;
					info.AddMessageError(message);
				}
			}
			else
			{
				if (Parent.CSI_Description.IsEmpty && !Parent.CSI_Description_ReadOnly)
				{
					var info = Parent.CSI_DescriptionInfo;
					info.AddMessageError(message);
				}
			}
		}

		protected override void CheckCSI_Code()
		{
			if (Parent.Declaration is JobDeclaration declaration && declaration.IsUCC5AndIsImport)
			{
				if (ShouldCodeBeInTheList)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
				}

				if (Parent.CSI_Code.IsEmpty && !Parent.CSI_CodeInfo.ReadOnly && Parent.CSI_Description.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
				}
			}
			else
			{
				base.CheckCSI_Code();
			}

			CheckCSI_CodeBR2317();

			var parent = Parent;
			if (parent.IsATransportDocument
					&& parent.Declaration is JobDeclaration jobDeclaration
					&& jobDeclaration.IsAir)
			{
				if (BR1113Valid(parent, jobDeclaration))
				{
					CheckCSI_CodeBR1113(parent);
				}
			}
		}

		void CheckCSI_CodeBR2317()
		{
			var parent = Parent;
			if (parent.IsAnAdditionalReference
				&& parent.IsRoRoShipID
				&& parent.Parent is JobComInvoiceLine invoiceLine
				&& invoiceLine.JI_Calc_RequestedProcedure == UniversalReferenceConstants.ProcedureCodes.ProcedureCode._71
				&& parent.Declaration is JobDeclaration declaration
				&& declaration.IsImport
				&& declaration.IsSea)
			{
				parent.CSI_CodeInfo.AddMessageError(Res.GetString("6C09F472-7349-4625-AF43-4BDE331694FE", "[BR2317] If Requested Procedure is '71', and Transport Mode is 'SEA', declaring a '1D94' Additional Reference will result in rejection."));
			}
		}

		bool BR1113Valid(AdditionalInfo info, JobDeclaration declaration)
		{
			return declaration.IsImport
				&& info.Parent is JobComInvoiceLine invoiceLine
				&& !invoiceLine.JI_Calc_RequestedProcedure.IsEmpty
				&& !invoiceLine.IsCustomsWarehousingProcedure76Or77;
		}

		void CheckCSI_CodeBR1113(AdditionalInfo info)
		{
			if (info.CSI_Code == TransportDocumentCodes._N704 || info.CSI_Code == TransportDocumentCodes._N705 || info.CSI_Code == TransportDocumentCodes._N714)
			{
				info.CSI_CodeInfo.AddMessageError(Res.GetString("F62EE49E-6E71-4103-A038-BFEA1F78057B", "[BR1113] If Requested Procedure is not '76' nor '77', and Transport Mode is 'AIR', then 'N704', 'N705', or 'N714' Transport Document is not allowed."));
			}
		}

		protected override bool IsOtherFieldsEnabled => false;

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;
	}
}
