using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public enum PeriodicInvoiceModule
	{
		CFS,
		Consol,
		Customs,
		Forwarding,
		Consignment,
		Transport,
		Warehouse,
		Agency,
		Other
	}

	public class PeriodicInvoiceModuleDecider
	{
		public IEnumerable<ZString> GetJobTypesByPeriodicInvoiceModuleFromList(IEnumerable<ZString> jobTypeList, PeriodicInvoiceModule module)
		{
			IEnumerable<ZString> result = null;

			if (jobTypeList != null)
			{
				var jobTypeToCategoryMapping = GetJobTypeToCategoryMapping();

				if (module != PeriodicInvoiceModule.Other)
				{
					result = jobTypeList.Where(x => jobTypeToCategoryMapping[module].Contains(x));
				}
				else
				{
					var jobTypesFiltered = new List<ZString>();

					foreach (PeriodicInvoiceModule enumItem in Enum.GetValues(typeof(PeriodicInvoiceModule)))
					{
						if (enumItem != PeriodicInvoiceModule.Other)
						{
							jobTypesFiltered.AddRange(jobTypeToCategoryMapping[enumItem]);
						}
					}

					result = jobTypeList.Where(x => !jobTypesFiltered.Contains(x));
				}
			}

			return result;
		}

		public static List<ZString> GetJobTypesByInvoiceModule(ZString invoiceTypeModule)
		{
			var jobTypes = new List<ZString>();
			var jobTypeToCategoryMap = GetJobTypeToCategoryMapping();
			switch (invoiceTypeModule)
			{
				case InvoiceTypeModuleList.Codes.FWD:
					jobTypes = jobTypeToCategoryMap[PeriodicInvoiceModule.Forwarding].ToList();
					break;
				case InvoiceTypeModuleList.Codes.CUS:
					jobTypes = jobTypeToCategoryMap[PeriodicInvoiceModule.Customs].ToList();
					break;
				case InvoiceTypeModuleList.Codes.CFS:
					jobTypes = jobTypeToCategoryMap[PeriodicInvoiceModule.CFS].ToList();
					break;
				case InvoiceTypeModuleList.Codes.ISF:
					jobTypes = new List<ZString>() { JobInvoicingConsumerTypes.ImporterSecurityFiling.Code };
					break;
				case InvoiceTypeModuleList.Codes.MSC:
					jobTypes = new List<ZString>() { "MSC" };
					break;
				case InvoiceTypeModuleList.Codes.TCN:
					jobTypes = jobTypeToCategoryMap[PeriodicInvoiceModule.Consignment].ToList();
					break;
				case InvoiceTypeModuleList.Codes.TPT:
					jobTypes = jobTypeToCategoryMap[PeriodicInvoiceModule.Transport].ToList();
					break;
				default:
					break;
			}
			return jobTypes;
		}

		public static Dictionary<PeriodicInvoiceModule, ZString[]> GetJobTypeToCategoryMapping()
		{
			var jobTypeToCategoryMapping = new Dictionary<PeriodicInvoiceModule, ZString[]>();

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.CFS, new ZString[] {
																						JobInvoicingConsumerTypes.CFSShipment.Code,
																				});

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Consol, new ZString[] {
																						JobInvoicingConsumerTypes.CFSLoadList.Code
																				});

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Customs, new ZString[] {
																						JobInvoicingConsumerTypes.Brokerage.Code
																				});

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Forwarding, new ZString[] {
																						JobInvoicingConsumerTypes.Shipment.Code,
																						JobInvoicingConsumerTypes.QuotedBooking.Code,
																						JobInvoicingConsumerTypes.OneOffQuotation.Code
																				});

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Consignment, new ZString[] {
																						JobInvoicingConsumerTypes.TransportBooking.Code,
																						JobInvoicingConsumerTypes.TransportBookingConsignment.Code,
																						JobInvoicingConsumerTypes.TransportConsignment.Code
																				});

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Transport, new ZString[] { JobInvoicingConsumerTypes.LocalCartage.Code });

			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Warehouse, new ZString[] {
																						JobInvoicingConsumerTypes.WarehouseInwards.Code,
																						JobInvoicingConsumerTypes.WarehouseOutwards.Code
																				});
			jobTypeToCategoryMapping.Add(PeriodicInvoiceModule.Agency, new ZString[] {
																						JobInvoicingConsumerTypes.AgencyBillOfLading.Code,
																						JobInvoicingConsumerTypes.AgencyBooking.Code,
																				});
			return jobTypeToCategoryMapping;
		}
	}
}
