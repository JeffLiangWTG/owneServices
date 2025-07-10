using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.DataAdapters;

namespace Enterprise.Accounting.DataTransfer
{
	public class CASSHOTFileDataAdapter : ValueObjectDataAdapter<CASSBilling, CASSCostHeader>
	{
		public CASSHOTFileDataAdapter(INotifications notification)
			: base()
		{
			Argument.NotNull(notification, "Notification");
			this.Notification = notification;
		}

		public void Fill(CASSBilling cassBilling)
		{
			var context = new ValueObjectImportContext(cassBilling.Factory, Notification);
			ImportFromValueObject(cassBilling, cassBilling.CostHeader, context);
		}

		#region Implementation
		readonly INotifications Notification;
		#endregion

		public override System.Xml.Schema.XmlSchema CollectionSchema
		{
			get { throw GetNotSupportedException("CollectionSchema"); }
		}

		protected override void ExportToValueObjectCore(CASSBilling bizObj, CASSCostHeader constructedValueObject, Enterprise.DataTransfer.Integration.IValueObjectExportContext context)
		{
			throw new NotImplementedException();
		}

		protected override void ImportFromValueObjectCore(CASSBilling cassBilling, CASSCostHeader costHeader, Enterprise.DataTransfer.Integration.IValueObjectImportContext context)
		{
			NotificationManager notifier = new NotificationManager(context);

			if (costHeader == null)
			{
				return;
			}

			if (!costHeader.Equals(cassBilling.CostHeader))
			{
				throw new ArgumentException("Provided ValueObject 'costHeader' is different from the CASSBilling's CostHeader. Please use CASSBilling.Initialize to set CASSBilling's CostHeader");
			}

			costHeader.RunPreSaveValidation();
			if (!costHeader.HasErrors)
			{
				cassBilling.RemoveAllLines();
				Dictionary<string, CASSBillingLine> exportLinesByMAWB = new Dictionary<string, CASSBillingLine>();
				Dictionary<string, CASSBillingLine> importLinesByMAWB = new Dictionary<string, CASSBillingLine>();
				Func<bool, Dictionary<string, CASSBillingLine>> getLinesByMAWBDictionary = isImportLine => isImportLine ? importLinesByMAWB : exportLinesByMAWB;

				ZString billingCurrency = "";
				if (cassBilling != null)
				{
					using (cassBilling.GetValidationSuspender())
					using (cassBilling.Lines.SuspendListChanged())
					{
						billingCurrency = costHeader.BillingCurrency;

						if (costHeader.Lines != null)
						{
							foreach (CASSCostLine costLine in costHeader.Lines)
							{
								string lineCurrency = cassBilling.IsImportBilling ? billingCurrency : costLine.CurrencyCode;
								string currentMAWB = costLine.AirlinePrefix + costLine.AWBSerialNumber;

								CASSBillingLine cassBillingLine;
								if (getLinesByMAWBDictionary(cassBilling.IsImportBilling).TryGetValue(currentMAWB, out cassBillingLine))
								{
									if (cassBillingLine.CASSCostCurrencyCode == lineCurrency)
									{
										if (cassBillingLine.LoadPortIATA != costLine.Origin)
										{
											notifier.AddWarningToNotifications(Res.GetString("a0b815f5-0fe1-46eb-9cf9-bb3f6cc2b553", "AWB {0} records have different Load Port IATA Codes.", cassBillingLine.MAWBNumber));
										}

										if (cassBillingLine.DischargePortIATA != costLine.Destination)
										{
											notifier.AddWarningToNotifications(Res.GetString("aea420d2-a434-4cb8-a199-d45f6aca4c39", "AWB {0} records have different Discharge Port IATA Codes.", cassBillingLine.MAWBNumber));
										}
									}
									else
									{
										notifier.AddErrorToNotifications(Res.GetString("38809770-9963-4b42-8b28-a0d3eb58276e", "AWB {0} records have different currencies.",
											cassBillingLine.MAWBNumber));
										continue;
									}
								}

								if (cassBillingLine == null)
								{
									cassBillingLine = cassBilling.Lines.AddNew();
									getLinesByMAWBDictionary(cassBilling.IsImportBilling).Add(currentMAWB, cassBillingLine);
								}

								using (cassBillingLine.GetValidationSuspender())
								{
									cassBillingLine.AddCostLine(costLine, lineCurrency);
								}
							}
						}

						cassBilling.ForceRecalculateData();
					}
				}
				else
				{
					ErrorReporter.ReportOnce("CASSBilling object should not be null.");
				}
			}
			else
			{
				notifier.AddDataErrorPreventSaveToNotifications(Res.GetString("c722afbc-a2e0-4af0-a276-d3eeb512a06b", "Cannot refresh costs, as one or more file line(s) have error."));
			}
		}

		public override string RootCollectionElementName
		{
			get { throw GetNotSupportedException("RootCollectionElementName"); }
		}

		public override string RootElementName
		{
			get { throw GetNotSupportedException("RootElementName"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Name")]
		public override System.Xml.Schema.XmlSchema Schema
		{
			get { throw GetNotSupportedException("Schema"); }
		}

		public NotSupportedException GetNotSupportedException(string name)
		{
			throw new NotSupportedException(Res.GetString("b2b33675-5ed4-45e9-854e-cfd74025e8b8", "{0} is not supported by this Adapter.", name));
		}
	}
}
