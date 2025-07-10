using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using static Enterprise.Accounting.Business.AccountingConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public class NettingTransactionDataContextManager : EventDataContextManager<NettingReceivableTransaction>, ITransactionDataContextManager, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.NettingTransaction; }
		}

		public override ZString DataContextKey
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", ParentBO.Period.NSP_Period, ParentBO.Issuer.Organisation.OH_Code, ParentBO.NRT_Reference); }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new NettingMatchEventParentFinder(factory, this, logger);
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		public ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager)
		{
			return new NettingTransactionDataObjectWriter(writeManager);
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			try
			{
				var matchStatus = HelperMethods.GetValueFromContextCollection(eventDataObject as UniversalEvent, InvoiceForNettingInfoList.Status, throwExceptionWhenEmpty: true);

				NettingHelper.HandleSettledOutOfNettingEvent(Db.Connection, businessObject as INettingTransaction, matchStatus);
			}
			catch (Exception ex) when (ex is IncorrectDataSetupException)
			{
				logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
				throw;
			}
		}

		public bool ManagesTransactions
		{
			get { return false; }
		}
	}
}
