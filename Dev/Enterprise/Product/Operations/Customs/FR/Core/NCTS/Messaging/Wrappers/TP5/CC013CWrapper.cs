using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC013CWrapper : ICC013C
	{
		public CC013CWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.nctsHeader = Argument.NotNull((NctsHeader)sendingObject.NctsHeader, nameof(sendingObject));
		}

		readonly NctsHeader nctsHeader;
		readonly TP5MessageSendingObject sendingObject;

		public static CC013CWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new CC013CWrapper(sendingObject);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = TransitOperationWrapper.New(sendingObject, true));
		ITransitOperation transitOperation;

		public ICollection<IAuthorisation> Authorisation => authorisation ?? (authorisation = GetAuthorisations());
		ICollection<IAuthorisation> authorisation;

		ICollection<IAuthorisation> GetAuthorisations()
		{
			var result = new Collection<IAuthorisation>();
			nctsHeader.MovementHeader.CusAuthorizationUsages.ForEach(authorizationUsage => result.Add(AuthorisationUsageWrapper.New(authorizationUsage)));
			return result;
		}

		public ICustomsOffice CustomsOfficeOfDeparture => customsOfficeOfDeparture ?? (customsOfficeOfDeparture = CustomsOfficeWrapper.New(nctsHeader.MovementHeader.DepartureCustomsOfficeCode));
		ICustomsOffice customsOfficeOfDeparture;

		public ICustomsOffice CustomsOfficeOfDestinationDeclared => customsOfficeOfDestinationDeclared ?? (customsOfficeOfDestinationDeclared = CustomsOfficeWrapper.New(nctsHeader.MovementHeader.DestinationCustomsOfficeCode));
		ICustomsOffice customsOfficeOfDestinationDeclared;

		public ICollection<ICustomsOfficeOfTransit> CustomsOfficeOfTransitDeclared => customsOfficeOfTransitDeclared ?? (customsOfficeOfTransitDeclared = GetCustomsOfficeOfTransitDeclared());
		ICollection<ICustomsOfficeOfTransit> customsOfficeOfTransitDeclared;

		ICollection<ICustomsOfficeOfTransit> GetCustomsOfficeOfTransitDeclared()
		{
			var result = new Collection<ICustomsOfficeOfTransit>();
			nctsHeader.MovementHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().Where(x => x.CY_Code == EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument).ForEach(customsOffice => result.Add(CustomsOfficeOfTransitWrapper.New(customsOffice)));
			return result;
		}

		public ICollection<ICustomsOffice> CustomsOfficeOfExitForTransitDeclared => customsOfficeOfExitForTransitDeclared ?? (customsOfficeOfExitForTransitDeclared = GetCustomsOfficeOfExitForTransitDeclared());
		ICollection<ICustomsOffice> customsOfficeOfExitForTransitDeclared;
		ICollection<ICustomsOffice> GetCustomsOfficeOfExitForTransitDeclared()
		{
			var result = new Collection<ICustomsOffice>();
			nctsHeader.MovementHeader.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().Where(x => x.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit).ForEach(customsOffice => result.Add(CustomsOfficeWrapper.New(customsOffice.CY_Data)));
			return result;
		}

		public IHolderOfTheTransitProcedure HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = HolderOfTheTransitProcedureWrapper.New(nctsHeader.MovementHeader.ProcedureHolder));
		IHolderOfTheTransitProcedure holderOfTheTransitProcedure;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(nctsHeader.MovementHeader.Representative.Organisation));
		IRepresentative representative;

		public ICollection<IGuarantee> Guarantee => guarantee ?? (guarantee = GetGuarantees());
		ICollection<IGuarantee> guarantee;
		ICollection<IGuarantee> GetGuarantees()
		{
			var result = new Collection<IGuarantee>();
			nctsHeader.GetEffectiveGuarantees().ForEach(guarantee => result.Add(GuaranteeWrapper.New(guarantee)));
			return result;
		}

		public IConsignment Consignment => consignment ?? (consignment = ConsignmentWrapper.New(nctsHeader.MovementHeader));
		IConsignment consignment;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE013"));
		IMessageEnveloppe messageEnveloppe;
	}
}
