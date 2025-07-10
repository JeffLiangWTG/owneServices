using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class PresentationCusExitReportValidation : CusExitReportValidation
	{
		public PresentationCusExitReportValidation(CusExitReport parent)
			: base(parent)
		{
		}

		protected override void CheckCER_Type()
		{
			var parent = Parent;
			JobDeclaration declaration = null;
			if (parent.Header?.Declaration is JobDeclaration headerDeclaration)
			{
				declaration = headerDeclaration;
			}
			else if (parent.Header?.Shipment is ForwardingShipment shipment)
			{
				declaration = (JobDeclaration)shipment.DeclarationForDocuments;
			}

			if (DeclarationIsNotReleasedForIE529(declaration))
			{
				parent.CER_TypeInfo.AddMessageError(Res.GetString("336C8615-5783-4E16-B94A-D58DD77334CE", "Message type Exit Control Presentation (IE507) cannot be submitted, when the Export entry status is not Released for Export (IE529)"));
			}
		}

		bool DeclarationIsNotReleasedForIE529(JobDeclaration declaration)
		{
			if (declaration is not null && declaration.IsExport)
			{
				return declaration.CustomsEntryHeaders.IsNullOrEmpty() || declaration.CustomsEntryHeaders.Any(entry => !TryGetIE529Messages(entry, out var ie529Messages) || ie529Messages.All(message => message.EM_Status != EDIMessage.Status.ProcessedOK));
			}
			else
			{
				return false;
			}

			bool TryGetIE529Messages(CusEntryHeader cusEntryHeader, out IEnumerable<Enterprise.Messaging.Business.EDIMessage> ie529Messages)
			{
				ie529Messages = cusEntryHeader.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>().Where(message => message.EM_ApplicationCode == EDIMessage.ApplicationCodes.IECustomsExport && message.EM_MessageType == AESIncomingMessageTypeList.Codes.IE529 && message.EM_ReceiveTransmit == EDIMessage.Direction.Receive);

				return ie529Messages.Any();
			}
		}

		protected override void CheckCER_TransportMode()
		{
			base.CheckCER_TransportMode();
			CheckFieldsAllRequiredWhenOneIsPresent(Parent.CER_TransportModeInfo);
		}

		protected override void CheckCER_TransportType()
		{
			base.CheckCER_TransportType();
			CheckFieldsAllRequiredWhenOneIsPresent(Parent.CER_TransportTypeInfo);
		}

		protected override void CheckCER_TransportID()
		{
			base.CheckCER_TransportID();
			CheckFieldsAllRequiredWhenOneIsPresent(Parent.CER_TransportIDInfo);
		}

		protected override void CheckCER_RN_NKTransportNationality()
		{
			base.CheckCER_RN_NKTransportNationality();
			CheckFieldsAllRequiredWhenOneIsPresent(Parent.CER_RN_NKTransportNationalityInfo);
		}

		void CheckFieldsAllRequiredWhenOneIsPresent(ZPropertyInfo targetInfo)
		{
			if (targetInfo.Value.IsEmpty
				&& !(Parent.CER_TransportMode.IsEmpty && Parent.CER_TransportType.IsEmpty && Parent.CER_TransportID.IsEmpty && Parent.CER_RN_NKTransportNationality.IsEmpty)
			)
			{
				var message = Res.GetString(
					"B497AEC0-6B6C-44FA-B47C-995A0D64AB15",
					"All of {0},{1},{2},{3} are required when one of these is present.",
					Parent.CER_TransportModeInfo.HumanReadableName,
					Parent.CER_TransportTypeInfo.HumanReadableName,
					Parent.CER_TransportIDInfo.HumanReadableName,
					Parent.CER_RN_NKTransportNationalityInfo.HumanReadableName
				);
				targetInfo.AddMessageError(message);
			}
		}
	}
}
