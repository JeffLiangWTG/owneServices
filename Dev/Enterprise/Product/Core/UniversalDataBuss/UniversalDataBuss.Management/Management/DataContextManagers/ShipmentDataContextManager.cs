using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management
{
	public abstract class ShipmentDataContextManager<T> : EventDataContextManager<T>, IShipmentDataContextManager, IShipmentDataContextManagerInternal
		where T : BusinessObject
	{
		protected ShipmentDataContextManager() { }

		void IShipmentDataContextManager.DefaultDataTargetFromRecipientRole(IDataContextDataObject dataContext, IXmlSessionTracker importSessionLogger)
		{
			if (RecipientRoleTargettedToThisModule(dataContext.RecipientRoleCollection, dataContext.DataSourceCollection, importSessionLogger))
			{
				dataContext.AddDataTarget(DataContextType, null);
			}
		}

		protected abstract bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger);

		bool IShipmentDataContextManager.UseIncomingShipmentData(ITopLevelDataObject universalShipment, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			if (universalShipment.GetImportAction() is ImportAction importAction)
			{
				switch (importAction)
				{
					case ImportAction.LinkOnly:
						return UseIncomingShipmentData(LinkToExistingBusinessObject, (UniversalShipment)universalShipment, logger, (UniversalObjectFactory)factory);
				}
			}
			return UseIncomingShipmentData(UpdateOrCreateNewBusinessObject, (UniversalShipment)universalShipment, logger, (UniversalObjectFactory)factory);
		}

		IKeysResult IShipmentDataContextManager.GetKeysForBlockingParallelImport(ITopLevelDataObject topLevelDataObject, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return UseIncomingShipmentData(GetKeysCore, (UniversalShipment)topLevelDataObject, logger, (UniversalObjectFactory)factory) ?? KeysResult.NoMatch();
		}

		IKeysResult GetKeysCore(UniversalShipment universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, UniversalObjectFactory factory)
		{
			var boReader = GetShipmentDataObjectReader(universalShipment, logger, factory);
			var foundObject = boReader?.GetExistingBusinessObject();
			var keys = new[] { foundObject?.IsInDatabase ?? false ? (KeyValue: foundObject.PK.ToStringKey(), KeySource: $"{foundObject.GetType().Name}/PK") : (KeyValue: "", KeySource: "") }
				.ConcatIgnoreNull(boReader?.ReadKeysForParallelism())
				.Concat(GetShipmentKeys(universalShipment))
				.Where(s => !string.IsNullOrEmpty(s.KeyValue))
				.ToArray();
			return KeysResult.Match(keys);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Better to keep this together.")]
		IEnumerable<(string KeyValue, string KeySource)> GetShipmentKeys(UniversalShipment shipment)
		{
			return Extensions.AsStringsIgnoringNull(
				(shipment.AgentsReference, nameof(shipment.AgentsReference)),
				(shipment.BookingConfirmationReference, nameof(shipment.BookingConfirmationReference)),
				(shipment.CartageWaybillNumber, nameof(shipment.CartageWaybillNumber)),
				(shipment.CFSReference, nameof(shipment.CFSReference)),
				(shipment.InterimReceiptNumber, nameof(shipment.InterimReceiptNumber)),
				(shipment.OwnerRef, nameof(shipment.OwnerRef)),
				(shipment.QuoteNumber, nameof(shipment.QuoteNumber)),
				(shipment.WayBillNumber, nameof(shipment.WayBillNumber))
			)
			.ConcatIgnoreNull(shipment.LocalProcessing?.SelectKeysSafe(l =>
				Extensions.AsStringsIgnoringNull((l.ArrivalCartageRef, $"{nameof(shipment.LocalProcessing)}/{nameof(l.ArrivalCartageRef)}"))
				.ConcatIgnoreNull(l?.OrderNumberCollection?.SelectNonNull(o => (o.OrderReference, $"{nameof(shipment.LocalProcessing)}/{nameof(l.OrderNumberCollection)}/{nameof(o.OrderReference)}")))))
			.ConcatIgnoreNull(shipment.AdditionalBillCollection?.SelectNonNull(b => (b.BillNumber, $"{nameof(shipment.AdditionalBillCollection)}/{nameof(b.BillNumber)}")))
			.ConcatIgnoreNull(shipment.AdditionalReferenceCollection?.SelectNonNull(r =>
				(Extensions.JoinIgnoringNull(r.Type?.Code, r.ReferenceNumber), $"{nameof(shipment.AdditionalReferenceCollection)}/{nameof(r.Type)}_{nameof(r.ReferenceNumber)}")))
			.ConcatIgnoreNull(shipment.CommercialInfo?.CommercialInvoiceCollection?.SelectNonNull(s =>
				(s.InvoiceNumber, $"{nameof(shipment.CommercialInfo)}/{nameof(shipment.CommercialInfo.CommercialInvoiceCollection)}/{nameof(s.InvoiceNumber)}")))
			.ConcatIgnoreNull(shipment.TransportLegCollection?.SelectKeysSafe(l => new[]
			{
		(Extensions.JoinIgnoringNull(l.VesselName, l.VoyageFlightNo), $"{nameof(shipment.TransportLegCollection)}/{nameof(l.VesselName)}+{nameof(l.VoyageFlightNo)}"),
		(Extensions.JoinIgnoringNull(l.VoyageFlightNo, l.EstimatedDeparture?.ToString()), $"{nameof(shipment.TransportLegCollection)}/{nameof(l.VoyageFlightNo)}+{nameof(l.EstimatedDeparture)}")
			}))
			.ConcatIgnoreNull(shipment.ContainerCollection?.SelectNonNull(c => (c.ContainerNumber, $"{nameof(shipment.ContainerCollection)}/{nameof(c.ContainerNumber)}")))
			.ConcatIgnoreNull(shipment.EntryNumberCollection?.Select(e =>
				(Extensions.JoinIgnoringNull(e?.Type?.Code, e?.Number), $"{nameof(shipment.EntryNumberCollection)}/{nameof(e.Type)}_{nameof(e.Number)}")))
			.ConcatIgnoreNull(shipment.EntryHeaderCollection?.Select(e => e?.EntryNumberCollection)
				.WhereNotNull().SelectMany(e => e).Select(e =>
					(Extensions.JoinIgnoringNull(e?.Type?.Code, e?.Number), $"{nameof(shipment.EntryHeaderCollection)}/{nameof(e.Type)}_{nameof(e.Number)}")));
		}

		delegate TResult WithIncomingsShipmentData<TResult>(UniversalShipment universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, UniversalObjectFactory factory);

		TResult UseIncomingShipmentData<TResult>(WithIncomingsShipmentData<TResult> withIncommingShipmentData, UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			IDataTargetDataObject dataTarget;
			if (TryGetMatchingDataTarget(universalShipment, out dataTarget) && IsNotFromSameSystemAndModule(universalShipment.DataContext, logger))
			{
				return withIncommingShipmentData(universalShipment, logger, dataTarget, factory);
			}
			else
			{
				return default(TResult);
			}
		}

		BusinessObject IShipmentDataContextManager.FindExistingBusinessObjectForIncomingShipment(ITopLevelDataObject universalShipment, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return FindExistingBusinessObjectForIncomingShipment((UniversalShipment)universalShipment, logger, (UniversalObjectFactory)factory);
		}

		BusinessObject FindExistingBusinessObjectForIncomingShipment(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			BusinessObject result = null;
			var boReader = GetShipmentDataObjectReader(universalShipment, logger, factory);
			if (boReader != null)
			{
				result = boReader.GetExistingBusinessObject();
			}

			return result;
		}

		protected virtual bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = universalShipment.GetMatchingDataTarget(DataContextType);
			return dataTarget != null;
		}

		public abstract bool ManagesShipments { get; }

		ITopLevelDataObjectWriter IShipmentDataContextManager.GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return GetShipmentDataObjectWriter(writeManager);
		}

		protected abstract ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager);
		protected abstract ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory);

		ITopLevelDataObjectReader IShipmentDataContextManagerInternal.GetShipmentDataObjectReader(ITopLevelDataObject universalShipment, IXmlImportLogger logger, IUniversalObjectFactory factory)
		{
			return GetShipmentDataObjectReader((UniversalShipment)universalShipment, logger, (UniversalObjectFactory)factory);
		}

		protected virtual IEnumerable<ITopLevelDataObjectReader> GetShipmentDataObjectReaders(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			yield return GetShipmentDataObjectReader(universalShipment, logger, factory);
		}

		bool LinkToExistingBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, UniversalObjectFactory factory)
		{
			var boReader = GetShipmentDataObjectReader(universalShipment, logger, factory);
			if (boReader == null)
			{
				return false;
			}

			var bizObj = boReader.GetExistingBusinessObject();

			if (bizObj == null)
			{
				var errorMessage = Res.GetString("5aaecc98-5993-4eed-a818-eae300cc57d5", "[*Unable to link because existing business object could not be found.*]");
				throw new DataObjectReadFailureException(errorMessage);
			}

			if (BeforeLinkToExistingBusinessObject(universalShipment, logger, factory, (T)bizObj))
			{
				logger.LogBoth(LogType.Information, Res.GetString("cef4c924-91f3-4b18-8081-21b7fd297cad", "Universal Shipment data was linked to {0}.", bizObj.HumanReadableName));
				logger.FireDataImportedToBusinessObject(bizObj);

				return true;
			}

			return false;
		}

		protected virtual bool BeforeLinkToExistingBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, T bizObj) => true;

		bool UpdateOrCreateNewBusinessObject(UniversalShipment universalShipment, IXmlImportLogger logger, IDataTargetDataObject dataTarget, UniversalObjectFactory factory)
		{
			var boReader = GetShipmentDataObjectReader(universalShipment, logger, factory);
			if (boReader == null)
			{
				return false;
			}

			ReadIntoBusinessObject(boReader, logger, factory);
			return true;
		}

		void ReadIntoBusinessObject(ITopLevelDataObjectReader boReader, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var foundObject = boReader.ReadIntoTopLevelBusinessObject();
			if (foundObject != null)
			{
				LogTopLevelDataContextKey(logger, foundObject);
			}
		}

		protected virtual void LogTopLevelDataContextKey(IXmlImportLogger logger, BusinessObject businessObject)
		{
			logger.LogTopLevelDataContextKey(() => businessObject.GetUniversalDataContextManager().DataContextKey);
		}
	}
}
