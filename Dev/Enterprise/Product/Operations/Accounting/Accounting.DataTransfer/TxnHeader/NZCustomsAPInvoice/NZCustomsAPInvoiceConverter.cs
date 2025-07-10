using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	internal class NZCustomsAPInvoiceConverter : FlatFileConverter<Xsd.TxnHeaderCollection>
	{
		public NZCustomsAPInvoiceConverter(INotifications notifications, BusinessObjectFactory factory)
			: base(notifications, factory)
		{
		}

		#region Overrides

		protected override void ImportFlatFileCore(Xsd.TxnHeaderCollection valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader)
		{
			string fileHeader = flatFileReader.ReadLine() ?? string.Empty;

			if (IsFileHeaderValid(new OCsvLine(fileHeader)))
			{
				flatFileReader.ReadLine();
				flatFileReader.ReadLine();

				base.ImportFlatFileCore(valueObject, fileFormat, flatFileReader);
			}
			else
			{
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat,
					Res.GetString("0e52cd8c-abe5-4d43-bea4-c73e16d78a18", "Column names do not match the list of column names expected.\r\nActual: {0}\r\nExpected: {1}",
					fileHeader, ExpectedHeaders)));
			}
		}

		protected override void MapImport(Xsd.TxnHeaderCollection valueObject, FlatFileDataRowCollection rawFileRows)
		{
			Dictionary<ZString, NZCustomsAPInvoiceDataRow> groupedByJobNumbers = GetUniqueJobPerLine(rawFileRows);

			Xsd.TxnHeader invoice = valueObject.AddNew();
			invoice.Branch = GlbBranch.CurrentBranch.GB_Code;
			invoice.Department = GlbDepartment.CurrentDepartment.GE_Code;
			invoice.OsInvoiceAmtInclTax.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			ZDecimal invoiceTotal = 0;

			foreach (KeyValuePair<ZString, NZCustomsAPInvoiceDataRow> job in groupedByJobNumbers)
			{
				invoiceTotal += job.Value.TotalInclGST;
				invoice.OsInvoiceAmtInclTax.Value += job.Value.TotalInclGST;

				if (job.Value.TotalInclGST.Equals(job.Value.NetChargesExclGST + job.Value.GST))
				{
					ZQuery jobLookupQuery = new ZQuery(JobHeaderSchema.JH_JobNum, job.Key);
					jobLookupQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					JobHeader[] jobHeaders = Factory.Load<JobHeader>(jobLookupQuery);

					switch (jobHeaders.Length)
					{
						case 0:
							Notification.Notify(new ErrorNotification(ErrorType.Error, GetJobNumberDoesNotExist(job.Value.JobNumber)));
							break;

						case 1:
							Mapper.MapJobChargeLines(jobHeaders[0], job.Value, invoice);
							break;

						default:
							Notification.Notify(new ErrorNotification(ErrorType.Error, GetJobNumberIsNotUnique(job.Value.JobNumber)));
							break;
					}
				}
				else
				{
					Notification.Notify(new ErrorNotification(ErrorType.Error, GetIncorrectTotal(job.Value.JobNumber)));
				}
			}

			fExpectedInvoiceTotal = invoiceTotal;
		}

		Dictionary<ZString, NZCustomsAPInvoiceDataRow> GetUniqueJobPerLine(FlatFileDataRowCollection rawFileRows)
		{
			Dictionary<ZString, NZCustomsAPInvoiceDataRow> dict = new Dictionary<ZString, NZCustomsAPInvoiceDataRow>();

			foreach (NZCustomsAPInvoiceDataRow row in rawFileRows)
			{
				if (dict.ContainsKey(row.JobNumber.Trim()))
				{
					dict[row.JobNumber.Trim()].NetChargesExclGST += row.NetChargesExclGST;
					dict[row.JobNumber.Trim()].GST += row.GST;
					dict[row.JobNumber.Trim()].TotalInclGST += row.TotalInclGST;
				}
				else
				{
					dict.Add(row.JobNumber.Trim(), row);
				}
			}
			return dict;
		}

		#endregion

		#region Properties

		ZDecimal fExpectedInvoiceTotal;

		public ZDecimal ExpectedInvoiceTotal
		{
			get { return fExpectedInvoiceTotal; }
		}

		#endregion

		#region Implementation

		bool IsFileHeaderValid(OCsvLine header)
		{
			var expected = ExpectedHeaders.Split(',').Select(v => v.Trim().ToLower());
			var actual = header.FieldValues.Select(v => v.Trim().ToLower());
			return expected.Count() == actual.Count() && Enumerable.SequenceEqual(expected, actual);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "long string")]
		const string ExpectedHeaders = "Business Unit, Customer, Broker Code, Broker Or Client Name, Accounting Date, Cusmod Entry Num, Entry Type, Entry Reason, Job Number, Fees & Levies, Duty, Gst On Fees & Levies, Gst On Imports, Total Incl Gst, Stmt Num, Stmt Date, Bal Fwd Due Dt";

		protected NZCustomsAPInvoiceMapper Mapper
		{
			get { return new NZCustomsAPInvoiceMapper(Factory, Notification); }
		}

		static string GetJobNumberDoesNotExist(ZString jobNumber)
		{
			return Res.GetString("43346023-da5f-4ac4-8bc0-87455ba2a099", @"Job Number '{0}' does not exist.", jobNumber);
		}

		static string GetJobNumberIsNotUnique(ZString jobNumber)
		{
			return Res.GetString("bdf2a37a-1363-44be-9935-19926fd01f2a", @"Job Number '{0}' is not unique.", jobNumber);
		}

		static string GetIncorrectTotal(ZString jobNumber)
		{
			return Res.GetString("69153a93-f758-4b7c-9724-ec0b42000098", @"Job Number '{0}'. Sum of amounts does not equal to total.", jobNumber);
		}

		#endregion
	}
}
