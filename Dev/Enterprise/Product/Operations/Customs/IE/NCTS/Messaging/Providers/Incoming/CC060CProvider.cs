using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC060C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC060CProvider
	{
		Cc060CType XmlObject { get; }

		public CC060CProvider(Cc060CType xmlObject)
		{
			XmlObject = xmlObject;
			this.transitOperation = Argument.NotNull(xmlObject.TransitOperation, nameof(xmlObject.TransitOperation));
			this.customsOfficeOfDeparture = Argument.NotNull(xmlObject.CustomsOfficeOfDeparture, nameof(xmlObject.CustomsOfficeOfDeparture));
			this.holderOfTheTransitProcedure = Argument.NotNull(xmlObject.HolderOfTheTransitProcedure, nameof(xmlObject.HolderOfTheTransitProcedure));
			this.representative = xmlObject.Representative;
		}
		readonly TransitOperationType22 transitOperation;
		readonly CustomsOfficeOfDepartureType03 customsOfficeOfDeparture;
		readonly HolderOfTheTransitProcedureType13 holderOfTheTransitProcedure;
		readonly RepresentativeType04 representative;

		public ZString MRN => transitOperation.Mrn;

		public ZString LRN => transitOperation.Lrn;

		public ZDateTime ControlNotificationDateAndTime => transitOperation.ControlNotificationDateAndTime.ConvertToZDateTime();

		public ZString NotificationType => transitOperation.NotificationType;

		public ZString CustomsOfficeOfDeparture => customsOfficeOfDeparture.ReferenceNumber;

		public CC060CHolderOfTheTransitProcedureProvider HolderOfTheTransitProcedure => holderOfTheTransitProcedureCached ?? (holderOfTheTransitProcedureCached = new CC060CHolderOfTheTransitProcedureProvider(holderOfTheTransitProcedure));
		CC060CHolderOfTheTransitProcedureProvider holderOfTheTransitProcedureCached;

		public CC060CRepresentativeProvider Representative => representative == null ? null : representativeCached ?? (representativeCached = new CC060CRepresentativeProvider(representative));
		CC060CRepresentativeProvider representativeCached;

		public IReadOnlyCollection<CC060CTypeOfControlProvider> TypeOfControls => typeOfControlCached ?? (typeOfControlCached = XmlObject.TypeOfControls?.Select(x => new CC060CTypeOfControlProvider(x)).ToArray() ?? Array.Empty<CC060CTypeOfControlProvider>());
		IReadOnlyCollection<CC060CTypeOfControlProvider> typeOfControlCached;

		public IReadOnlyCollection<CC060RequestedDocumentProvider> RequestedDocuments => requestedDocumentCached ?? (requestedDocumentCached = XmlObject.RequestedDocument?.Select(x => new CC060RequestedDocumentProvider(x)).ToArray() ?? Array.Empty<CC060RequestedDocumentProvider>());
		IReadOnlyCollection<CC060RequestedDocumentProvider> requestedDocumentCached;
	}
}
