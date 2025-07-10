using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public class PtrsReport : PtrsReportBase, IDocManagerSupport
	{
		public PtrsReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Generate report

		public string GenerateReport()
		{
			var result = string.Empty;

			if (IsSaved || IsGenerated)
			{
				result = CreateAndAttachFileToEdoc();
				ATR_Status = Status.Generated;
			}

			return result;
		}

		public ZString LastGeneratedFileName
			=> DocManagerInfo.AllEDocs.Cast<IeDoc>().Where(x => x.DocType == FileType
				&& x.Description == FileDescription
				&& x.FileName.StartsWith(FileNameID, StringComparison.OrdinalIgnoreCase))
			.OrderByDescending(y => y.DateAdded).FirstOrDefault()?.FileName ?? string.Empty;

		void WriteFileData(StreamWriter writer)
		{
			var builder = new ZStringBuilder();

			var fileHeader = string.Empty;
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsHead.csv"))
			using (var reader = new StreamReader(stream))
			{
				fileHeader = reader.ReadToEnd();
			}

			#region Adding Columns in order

			builder.Append(GetCsvSaveText(ATR_CompanyName)); // A
			builder.Append(GetCsvSaveText(ATR_VATRegNo)); // B
			appendRegNumber(C);
			appendText(D);
			appendRegNumber(E);
			appendRegNumber(F);
			appendText(G);
			appendRegNumber(H);
			appendRegNumber(I);
			appendRegNumber(J);
			appendDateValue(DateFrom); // K
			appendDateValue(DateTo); // L
			appendNumber(M);
			appendNumberOrEmpty(N);
			appendText(O);
			appendNumber(P);
			appendNumberOrEmpty(Q);
			appendText(R);
			appendNumber(S);
			appendNumberOrEmpty(T);
			appendText(U);
			appendPercent(V);
			appendPercent(W);
			appendPercent(X);
			appendPercent(Y);
			appendPercent(Z);
			appendPercent(AA);
			appendPercent(AB);
			appendPercent(AC);
			appendPercent(AD);
			appendPercent(AE);
			appendPercent(AF);
			appendPercent(AG);
			appendText(AH);
			appendText(AI);
			appendText(AJ);
			appendPercent(AK);
			appendText(AL);
			appendPercent(AM);
			appendPercent(AN);
			appendText(AO);
			appendText(AP);
			appendDate(AQ);
			appendText(AR);
			appendText(AS);
			appendText(AT);
			appendText(AU);
			appendText(AV);
			appendText(AW);
			appendText(AX);
			appendText(AY);
			appendText(AZ);
			appendText(BA);
			appendText(BB);
			appendText(BC);
			appendText(BD);
			appendDate(BE);
			appendText(BF);
			appendText(BG);
			appendDate(BH);

			#endregion

			writer.Write(fileHeader);
			writer.Write(builder.ToStringWithDelimiterBetweenAppends(","));

			void appendRegNumber(PtrsReportRegNumberColumn column) => builder.Append(GetCsvSaveText(column.Value));
			void appendText(PtrsReportCommentColumn column) => builder.Append(GetCsvSaveText(column.Value));
			void appendDateValue(ZDate date) => builder.Append(GetCsvSaveText(date.ToString("dd/MM/yyyy")));
			void appendDate(PtrsReportDateColumn column) => builder.Append(GetCsvSaveText(column.Value.ToString("dd/MM/yyyy")));
			void appendNumber(PtrsReportNumberColumn column) => builder.Append(column.Value.ToString());
			void appendNumberOrEmpty(PtrsReportNumberColumn column) => builder.Append(column.Value.IsEmpty ? string.Empty : column.Value.ToString());
			void appendPercent(PtrsReportPercentColumn column) => builder.Append(column.Value.ToString(1));
		}

		string GetCsvSaveText(string str)
		{
			bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
			if (mustQuote)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("\"");
				foreach (char nextChar in str)
				{
					sb.Append(nextChar);
					if (nextChar == '"')
					{
						sb.Append("\"");
					}
				}
				sb.Append("\"");
				return sb.ToString();
			}
			return str;
		}

		#region eDocs

		string FileNameID => "PTRS-" + FileReference;
		string FileDescription => (NoResString)"Small Business Payment Times Report";
		string FileType => Core.Constants.RefDocTypes.MiscellaneousDocument;

		string CreateAndAttachFileToEdoc()
		{
			DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			var filename = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}-{1}.csv", FileNameID, ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));

			using (var outputStream = new MemoryStream())
			using (var writer = new StreamWriter(outputStream))
			{
				WriteFileData(writer);
				writer.Flush();
				outputStream.Position = 0;

				var newFile = DocManagerInfo.AddFileOrDocument(outputStream.ToArray(), filename, FileType, description: FileDescription);
				newFile.IsPublished = true;
				DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			}

			return filename;
		}

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = ComplianceReport.DocManagerInfo());
		DocManagerInfo docManagerInfo;

		#endregion

		#region Report details

		string FileReference => string.Format(CultureInfo.InvariantCulture, "{0} {1}-{2}",
			ComplianceReport.Company.GC_Code,
			ComplianceReport.ACR_DateFrom.ToShortDateString(),
			ComplianceReport.ACR_DateTo.ToShortDateString());

		#endregion

		#region Sender details

		public string SenderContactPhone => !string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.WorkPhone) ?
			EnvProxy.Instance.CurrentUser.WorkPhone : (string)ComplianceReport.Company.OrgProxy.MainAddress.OA_Phone;

		#endregion

		#endregion

		#region Mark as Submitted

		public override void SubmitReport()
		{
			if (IsGenerated)
			{
				Reload();

				Columns.Reload(true);

				ATR_Status = Status.Submitted;
			}
		}

		#endregion

		#region Load Details

		public override void OnLoaded()
		{
			base.OnLoaded();
			UpdateReportablePercentColumns(null, EventArgs.Empty);
		}

		public override ZGuid ATR_ACR_ComplianceReport
		{
			get => base.ATR_ACR_ComplianceReport;
			set
			{
				allPaymentsComplianceReport = null;
				base.ATR_ACR_ComplianceReport = value;
				UpdateReportablePercentColumns(null, EventArgs.Empty);
			}
		}

		#endregion

		#region Related All Payments Report

		public AccComplianceReport AllPaymentsComplianceReport
		{
			get
			{
				if (allPaymentsComplianceReport == null && ComplianceReport != null)
				{
					var query = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, ComplianceReport.ACR_GC_Company);
					query.AddToFilter(AccComplianceReportSchema.ACR_ReportType, ComplianceReportTypes.PaymentTimesAllPaymentsReportType);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, ComplianceReport.ACR_DateFrom);
					query.AddToFilter(AccComplianceReportSchema.ACR_DateTo, ComplianceReport.ACR_DateTo);
					allPaymentsComplianceReport = Factory.LoadTop1<AccComplianceReport>(query);
				}
				return allPaymentsComplianceReport;
			}
		}
		AccComplianceReport allPaymentsComplianceReport;

		public PtrsAllPaymentsReport PtrsAllPaymentsReport
		{
			get
			{
				if (ptrsAllPaymentsReport == null && AllPaymentsComplianceReport != null)
				{
					ptrsAllPaymentsReport = Factory.LoadTop1<PtrsAllPaymentsReport>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, AllPaymentsComplianceReport.PK));
				}
				return ptrsAllPaymentsReport;
			}
		}
		PtrsAllPaymentsReport ptrsAllPaymentsReport;

		#endregion

		#region Report as only element of Collection

		/// <summary>
		/// To bind report properties to a single line grid on the form we need to wrap it into a collection
		/// </summary>
		public PtrsReportCollection ReportCollection => reportCollection ?? (reportCollection = new PtrsReportCollection(Factory, this.PK));
		PtrsReportCollection reportCollection;

		#endregion

		#region Columns

		#region Report based summaries

		public ZInt NumberInvoicesPaidWithin20Days { get; private set; }
		public ZInt NumberInvoicesPaidBetween21And30Days { get; private set; }
		public ZInt NumberInvoicesPaidBetween31And60Days { get; private set; }
		public ZInt NumberInvoicesPaidBetween61And90Days { get; private set; }
		public ZInt NumberInvoicesPaidBetween91And120Days { get; private set; }
		public ZInt NumberInvoicesPaidInMoreThan120Days { get; private set; }

		public ZInt TotalNumberInvoicesPaid =>
			NumberInvoicesPaidWithin20Days +
			NumberInvoicesPaidBetween21And30Days +
			NumberInvoicesPaidBetween31And60Days +
			NumberInvoicesPaidBetween61And90Days +
			NumberInvoicesPaidBetween91And120Days +
			NumberInvoicesPaidInMoreThan120Days;

		[ResourceStringData("d4843bb7-c1c1-4652-a582-5a97a3d5ca18", Caption = "0 - 20")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidWithin20Days { get; private set; }

		[ResourceStringData("a0bf1480-835d-4a55-9acb-95994a064d00", Caption = "21 - 30")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween21And30Days { get; private set; }

		[ResourceStringData("7b1e32de-6e62-4a74-8a61-f145ee5b215f", Caption = "31 - 60")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween31And60Days { get; private set; }

		[ResourceStringData("62617709-fbe6-41ff-8c51-0813a4d6dc19", Caption = "61 - 90")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween61And90Days { get; private set; }

		[ResourceStringData("33812bfe-5d33-437e-84f8-88e609a3ed32", Caption = "91 - 120")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween91And120Days { get; private set; }

		[ResourceStringData("63c02de9-75bd-41fb-8e70-a61a0c0cb150", Caption = "121 +")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidInMoreThan120Days { get; private set; }

		[ResourceStringData("e3ef1087-c1ed-4bee-91cd-1db2d9edb3cb", Caption = "Total")]
		[DecimalPlaces(2)]
		public ZDecimal TotalValueInvoicesPaid =>
			ValueInvoicesPaidWithin20Days +
			ValueInvoicesPaidBetween21And30Days +
			ValueInvoicesPaidBetween31And60Days +
			ValueInvoicesPaidBetween61And90Days +
			ValueInvoicesPaidBetween91And120Days +
			ValueInvoicesPaidInMoreThan120Days;

		[ResourceStringData("d51be962-ec9f-4049-8227-e982c25dd571", Caption = "All Payments from PTA Report")]
		[DecimalPlaces(2)]
		public ZDecimal AllPaymentsValue { get; private set; }

		protected override void GetDataFromComplianceReportLines()
		{
			if (ComplianceReport != null
				&& (ComplianceReport.ACR_Status == AccComplianceReport.Status.ReportGenerated || ComplianceReport.ACR_IsFinalised))
			{
				NumberInvoicesPaidWithin20Days =
				NumberInvoicesPaidBetween21And30Days =
				NumberInvoicesPaidBetween31And60Days =
				NumberInvoicesPaidBetween61And90Days =
				NumberInvoicesPaidBetween91And120Days =
				NumberInvoicesPaidInMoreThan120Days = ZInt.Zero;

				ValueInvoicesPaidWithin20Days =
				ValueInvoicesPaidBetween21And30Days =
				ValueInvoicesPaidBetween31And60Days =
				ValueInvoicesPaidBetween61And90Days =
				ValueInvoicesPaidBetween91And120Days =
				ValueInvoicesPaidInMoreThan120Days = ZDecimal.Zero;

				foreach (var line in ComplianceReport.ReportLines.OfType<AccComplianceReportLine>().Where(x => !x.PreCalculatedAmount.IsEmpty))
				{
					if (line.PreCalculatedCount <= 20)
					{
						NumberInvoicesPaidWithin20Days++;
						ValueInvoicesPaidWithin20Days += line.PreCalculatedAmount;
					}
					else if (line.PreCalculatedCount <= 30)
					{
						NumberInvoicesPaidBetween21And30Days++;
						ValueInvoicesPaidBetween21And30Days += line.PreCalculatedAmount;
					}
					else if (line.PreCalculatedCount <= 60)
					{
						NumberInvoicesPaidBetween31And60Days++;
						ValueInvoicesPaidBetween31And60Days += line.PreCalculatedAmount;
					}
					else if (line.PreCalculatedCount <= 90)
					{
						NumberInvoicesPaidBetween61And90Days++;
						ValueInvoicesPaidBetween61And90Days += line.PreCalculatedAmount;
					}
					else if (line.PreCalculatedCount <= 120)
					{
						NumberInvoicesPaidBetween91And120Days++;
						ValueInvoicesPaidBetween91And120Days += line.PreCalculatedAmount;
					}
					else
					{
						NumberInvoicesPaidInMoreThan120Days++;
						ValueInvoicesPaidInMoreThan120Days += line.PreCalculatedAmount;
					}
				}

				AllPaymentsValue = PtrsAllPaymentsReport?.AllInvoicesPaidWithOverride ?? ZDecimal.Zero;
			}
		}

		#endregion

		#region Report based overridden Numbers

		public ZInt NumberInvoicesPaidWithin20DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidWithin20DaysColumnName, NumberInvoicesPaidWithin20Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidWithin20DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days);
		}

		public ZInt NumberInvoicesPaidBetween21And30DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidBetween21And30DaysColumnName, NumberInvoicesPaidBetween21And30Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidBetween21And30DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days);
		}

		public ZInt NumberInvoicesPaidBetween31And60DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidBetween31And60DaysColumnName, NumberInvoicesPaidBetween31And60Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidBetween31And60DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days);
		}

		public ZInt NumberInvoicesPaidBetween61And90DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidBetween61And90DaysColumnName, NumberInvoicesPaidBetween61And90Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidBetween61And90DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days);
		}

		public ZInt NumberInvoicesPaidBetween91And120DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidBetween91And120DaysColumnName, NumberInvoicesPaidBetween91And120Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidBetween91And120DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days);
		}

		public ZInt NumberInvoicesPaidInMoreThan120DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidInMoreThan120DaysColumnName, NumberInvoicesPaidInMoreThan120Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidInMoreThan120DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
		}

		public ZInt TotalNumberInvoicesPaidWithOverride =>
			NumberInvoicesPaidWithin20DaysWithOverride +
			NumberInvoicesPaidBetween21And30DaysWithOverride +
			NumberInvoicesPaidBetween31And60DaysWithOverride +
			NumberInvoicesPaidBetween61And90DaysWithOverride +
			NumberInvoicesPaidBetween91And120DaysWithOverride +
			NumberInvoicesPaidInMoreThan120DaysWithOverride;

		public ZPropertyInfo TotalNumberInvoicesPaidWithOverrideInfo => GetZPropertyInfo(nameof(TotalNumberInvoicesPaidWithOverride));

		public ZInt NumberInvoicesPaidSupplyChainFinanceArrangements
		{
			get => this.GetOverriddenOrCalculatedNumber(NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName, ZInt.Zero, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
			set => this.SetOverriddenNumber(NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
		}

		const string NumberInvoicesPaidWithin20DaysColumnName = nameof(NumberInvoicesPaidWithin20Days);
		const string NumberInvoicesPaidBetween21And30DaysColumnName = nameof(NumberInvoicesPaidBetween21And30Days);
		const string NumberInvoicesPaidBetween31And60DaysColumnName = nameof(NumberInvoicesPaidBetween31And60Days);
		const string NumberInvoicesPaidBetween61And90DaysColumnName = nameof(NumberInvoicesPaidBetween61And90Days);
		const string NumberInvoicesPaidBetween91And120DaysColumnName = nameof(NumberInvoicesPaidBetween91And120Days);
		const string NumberInvoicesPaidInMoreThan120DaysColumnName = nameof(NumberInvoicesPaidInMoreThan120Days);
		const string NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName = nameof(NumberInvoicesPaidSupplyChainFinanceArrangements);

		#endregion

		#region Report based overridden Values

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidWithin20DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidWithin20DaysColumnName, ValueInvoicesPaidWithin20Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidWithin20DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days);
		}

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween21And30DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidBetween21And30DaysColumnName, ValueInvoicesPaidBetween21And30Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidBetween21And30DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days);
		}

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween31And60DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidBetween31And60DaysColumnName, ValueInvoicesPaidBetween31And60Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidBetween31And60DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days);
		}

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween61And90DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidBetween61And90DaysColumnName, ValueInvoicesPaidBetween61And90Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidBetween61And90DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days);
		}

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidBetween91And120DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidBetween91And120DaysColumnName, ValueInvoicesPaidBetween91And120Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidBetween91And120DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days);
		}

		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidInMoreThan120DaysWithOverride
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidInMoreThan120DaysColumnName, ValueInvoicesPaidInMoreThan120Days, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidInMoreThan120DaysColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
		}

		[ResourceStringData("311384d2-5981-41fc-95e8-e024ef0e2182", Caption = "Total Reportable Amount")]
		[DecimalPlaces(2)]
		public ZDecimal TotalValueInvoicePaidWithOverride =>
			ValueInvoicesPaidWithin20DaysWithOverride +
			ValueInvoicesPaidBetween21And30DaysWithOverride +
			ValueInvoicesPaidBetween31And60DaysWithOverride +
			ValueInvoicesPaidBetween61And90DaysWithOverride +
			ValueInvoicesPaidBetween91And120DaysWithOverride +
			ValueInvoicesPaidInMoreThan120DaysWithOverride;

		public ZPropertyInfo TotalValueInvoicesPaidWithOverrideInfo => GetZPropertyInfo(nameof(TotalValueInvoicePaidWithOverride));

		[ResourceStringData("b211deca-6e95-4dc4-b7cd-3a25e0171993", Caption = "Supply Chain Finance Arrangements")]
		[DecimalPlaces(2)]
		public ZDecimal ValueInvoicesPaidSupplyChainFinanceArrangements
		{
			get => this.GetOverriddenOrCalculatedAmount(ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName, ZDecimal.Zero, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
			set => this.SetOverriddenAmount(ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName, value, valueChanged: UpdateReportablePercentColumns, commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
		}

		const string ValueInvoicesPaidWithin20DaysColumnName = nameof(ValueInvoicesPaidWithin20Days);
		const string ValueInvoicesPaidBetween21And30DaysColumnName = nameof(ValueInvoicesPaidBetween21And30Days);
		const string ValueInvoicesPaidBetween31And60DaysColumnName = nameof(ValueInvoicesPaidBetween31And60Days);
		const string ValueInvoicesPaidBetween61And90DaysColumnName = nameof(ValueInvoicesPaidBetween61And90Days);
		const string ValueInvoicesPaidBetween91And120DaysColumnName = nameof(ValueInvoicesPaidBetween91And120Days);
		const string ValueInvoicesPaidInMoreThan120DaysColumnName = nameof(ValueInvoicesPaidInMoreThan120Days);
		const string ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName = nameof(ValueInvoicesPaidSupplyChainFinanceArrangements);

		#endregion

		#region Override Reasons

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidWithin20Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidWithin20DaysColumnName, ValueInvoicesPaidWithin20DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days);
			set => this.SetOverrideReason(NumberInvoicesPaidWithin20DaysColumnName, ValueInvoicesPaidWithin20DaysColumnName, value,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidWithin20Days,
				NumberInvoicesPaidWithin20Days, ValueInvoicesPaidWithin20Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidWithin20DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidWithin20Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidWithin20DaysColumnName, ValueInvoicesPaidWithin20DaysColumnName,
					NeedReasonToOverrideInvoicesPaidWithin20Days, ValueInvoicesPaidWithin20Days));

		bool NeedReasonToOverrideInvoicesPaidWithin20Days()
			=> NumberInvoicesPaidWithin20Days != NumberInvoicesPaidWithin20DaysWithOverride || ValueInvoicesPaidWithin20Days != ValueInvoicesPaidWithin20DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidBetween21And30Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidBetween21And30DaysColumnName, ValueInvoicesPaidBetween21And30DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days);
			set => this.SetOverrideReason(NumberInvoicesPaidBetween21And30DaysColumnName, ValueInvoicesPaidBetween21And30DaysColumnName, value,
					commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween21And30Days,
					NumberInvoicesPaidBetween21And30Days, ValueInvoicesPaidBetween21And30Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidBetween21And30DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidBetween21And30Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidBetween21And30DaysColumnName, ValueInvoicesPaidBetween21And30DaysColumnName,
					NeedReasonToOverrideInvoicesPaidBetween21And30Days, ValueInvoicesPaidBetween21And30Days));

		bool NeedReasonToOverrideInvoicesPaidBetween21And30Days()
			=> NumberInvoicesPaidBetween21And30Days != NumberInvoicesPaidBetween21And30DaysWithOverride || ValueInvoicesPaidBetween21And30Days != ValueInvoicesPaidBetween21And30DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidBetween31And60Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidBetween31And60DaysColumnName, ValueInvoicesPaidBetween31And60DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days);
			set => this.SetOverrideReason(NumberInvoicesPaidBetween31And60DaysColumnName, ValueInvoicesPaidBetween31And60DaysColumnName, value,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween31And60Days,
				NumberInvoicesPaidBetween31And60Days, ValueInvoicesPaidBetween31And60Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidBetween31And60DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidBetween31And60Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidBetween31And60DaysColumnName, ValueInvoicesPaidBetween31And60DaysColumnName,
					NeedReasonToOverrideInvoicesPaidBetween31And60Days, ValueInvoicesPaidBetween31And60Days));

		bool NeedReasonToOverrideInvoicesPaidBetween31And60Days()
			=> NumberInvoicesPaidBetween31And60Days != NumberInvoicesPaidBetween31And60DaysWithOverride || ValueInvoicesPaidBetween31And60Days != ValueInvoicesPaidBetween31And60DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidBetween61And90Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidBetween61And90DaysColumnName, ValueInvoicesPaidBetween61And90DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days);
			set => this.SetOverrideReason(NumberInvoicesPaidBetween61And90DaysColumnName, ValueInvoicesPaidBetween61And90DaysColumnName, value,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween61And90Days,
				NumberInvoicesPaidBetween61And90Days, ValueInvoicesPaidBetween61And90Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidBetween61And90DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidBetween61And90Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidBetween61And90DaysColumnName, ValueInvoicesPaidBetween61And90DaysColumnName,
					NeedReasonToOverrideInvoicesPaidBetween61And90Days, ValueInvoicesPaidBetween61And90Days));

		bool NeedReasonToOverrideInvoicesPaidBetween61And90Days()
			=> NumberInvoicesPaidBetween61And90Days != NumberInvoicesPaidBetween61And90DaysWithOverride || ValueInvoicesPaidBetween61And90Days != ValueInvoicesPaidBetween61And90DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidBetween91And120Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidBetween91And120DaysColumnName, ValueInvoicesPaidBetween91And120DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days);
			set => this.SetOverrideReason(NumberInvoicesPaidBetween91And120DaysColumnName, ValueInvoicesPaidBetween91And120DaysColumnName, value,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidBetween91And120Days,
				NumberInvoicesPaidBetween91And120Days, ValueInvoicesPaidBetween91And120Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidBetween91And120DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidBetween91And120Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidBetween91And120DaysColumnName, ValueInvoicesPaidBetween91And120DaysColumnName,
					NeedReasonToOverrideInvoicesPaidBetween91And120Days, ValueInvoicesPaidBetween91And120Days));

		bool NeedReasonToOverrideInvoicesPaidBetween91And120Days()
			=> NumberInvoicesPaidBetween91And120Days != NumberInvoicesPaidBetween91And120DaysWithOverride || ValueInvoicesPaidBetween91And120Days != ValueInvoicesPaidBetween91And120DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonToOverrideInvoicesPaidInMoreThan120Days
		{
			get => this.GetOverrideReason(NumberInvoicesPaidInMoreThan120DaysColumnName, ValueInvoicesPaidInMoreThan120DaysColumnName,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days);
			set => this.SetOverrideReason(NumberInvoicesPaidInMoreThan120DaysColumnName, ValueInvoicesPaidInMoreThan120DaysColumnName, value,
				commentRequiredCheck: NeedReasonToOverrideInvoicesPaidInMoreThan120Days,
				NumberInvoicesPaidInMoreThan120Days, ValueInvoicesPaidInMoreThan120Days);
		}

		public ZPropertyInfo ReasonToOverrideInvoicesPaidInMoreThan120DaysInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonToOverrideInvoicesPaidInMoreThan120Days),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidInMoreThan120DaysColumnName, ValueInvoicesPaidInMoreThan120DaysColumnName,
					NeedReasonToOverrideInvoicesPaidInMoreThan120Days, ValueInvoicesPaidInMoreThan120Days));

		bool NeedReasonToOverrideInvoicesPaidInMoreThan120Days()
			=> NumberInvoicesPaidInMoreThan120Days != NumberInvoicesPaidInMoreThan120DaysWithOverride || ValueInvoicesPaidInMoreThan120Days != ValueInvoicesPaidInMoreThan120DaysWithOverride;

		[MaxLength(AccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		[ReadOnly(false)]
		public ZString ReasonForInvoicesPaidSupplyChainFinanceArrangements
		{
			get => this.GetOverrideReason(NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName, ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName,
				commentRequiredCheck: () => NeedReasonForInvoicesPaidSupplyChainFinanceArrangements());
			set => this.SetOverrideReason(NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName, ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName, value,
				commentRequiredCheck: () => NeedReasonForInvoicesPaidSupplyChainFinanceArrangements(),
				ZInt.Zero, ZDecimal.Zero);
		}

		public ZPropertyInfo ReasonForInvoicesPaidSupplyChainFinanceArrangementsInfo
			=> GetWrappedZPropertyInfo(nameof(ReasonForInvoicesPaidSupplyChainFinanceArrangements),
				_ => this.GetOverrideReasonInfo(NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName, ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName,
					() => NeedReasonForInvoicesPaidSupplyChainFinanceArrangements(), ZDecimal.Zero));

		bool NeedReasonForInvoicesPaidSupplyChainFinanceArrangements()
			=> !NumberInvoicesPaidSupplyChainFinanceArrangements.IsEmpty || !ValueInvoicesPaidSupplyChainFinanceArrangements.IsEmpty;
		#endregion

		void UpdateReportablePercentColumns(object sender, EventArgs e)
		{
			if (!isInUpdateReportablePercentColumns && ComplianceReport != null && ComplianceReport.ACR_Status == AccComplianceReport.Status.ReportGenerated)
			{
				using (new DisposableAction(() => isInUpdateReportablePercentColumns = true, () => isInUpdateReportablePercentColumns = false))
				{
					var totalNumber = TotalNumberInvoicesPaidWithOverride;
					TotalNumberInvoicesPaidWithOverrideInfo.RefreshBinding();

					V.Value = calculateNumberPercent(NumberInvoicesPaidWithin20DaysWithOverride, totalNumber);
					W.Value = calculateNumberPercent(NumberInvoicesPaidBetween21And30DaysWithOverride, totalNumber);
					X.Value = calculateNumberPercent(NumberInvoicesPaidBetween31And60DaysWithOverride, totalNumber);
					Y.Value = calculateNumberPercent(NumberInvoicesPaidBetween61And90DaysWithOverride, totalNumber);
					Z.Value = calculateNumberPercent(NumberInvoicesPaidBetween91And120DaysWithOverride, totalNumber);
					AA.Value = calculateNumberPercent(NumberInvoicesPaidInMoreThan120DaysWithOverride, totalNumber);
					AM.Value = calculateNumberPercent(NumberInvoicesPaidSupplyChainFinanceArrangements, totalNumber);

					var totalValue = TotalValueInvoicePaidWithOverride;
					TotalValueInvoicesPaidWithOverrideInfo.RefreshBinding();

					AB.Value = calculateValuePercent(ValueInvoicesPaidWithin20DaysWithOverride, totalValue);
					AC.Value = calculateValuePercent(ValueInvoicesPaidBetween21And30DaysWithOverride, totalValue);
					AD.Value = calculateValuePercent(ValueInvoicesPaidBetween31And60DaysWithOverride, totalValue);
					AE.Value = calculateValuePercent(ValueInvoicesPaidBetween61And90DaysWithOverride, totalValue);
					AF.Value = calculateValuePercent(ValueInvoicesPaidBetween91And120DaysWithOverride, totalValue);
					AG.Value = calculateValuePercent(ValueInvoicesPaidInMoreThan120DaysWithOverride, totalValue);
					AN.Value = calculateValuePercent(ValueInvoicesPaidSupplyChainFinanceArrangements, totalValue);

					AK.Value = calculateValuePercent(totalValue, AllPaymentsValue);

					this.ValidateCommentsOfNumberOverrideColumns(
						NumberInvoicesPaidWithin20DaysColumnName,
						NumberInvoicesPaidBetween21And30DaysColumnName,
						NumberInvoicesPaidBetween31And60DaysColumnName,
						NumberInvoicesPaidBetween61And90DaysColumnName,
						NumberInvoicesPaidBetween91And120DaysColumnName,
						NumberInvoicesPaidInMoreThan120DaysColumnName,
						NumberInvoicesPaidSupplyChainFinanceArrangementsColumnName
					);

					this.ValidateCommentsOfValueOverrideColumns(
						ValueInvoicesPaidWithin20DaysColumnName,
						ValueInvoicesPaidBetween21And30DaysColumnName,
						ValueInvoicesPaidBetween31And60DaysColumnName,
						ValueInvoicesPaidBetween61And90DaysColumnName,
						ValueInvoicesPaidBetween91And120DaysColumnName,
						ValueInvoicesPaidInMoreThan120DaysColumnName,
						ValueInvoicesPaidSupplyChainFinanceArrangementsColumnName
					);

					this.RefreshBindingIncludingChildren();
					ReportCollection.RefreshBinding();
				}
			}

			ZDecimal calculateNumberPercent(ZInt part, ZInt total) => total.IsEmpty ? ZDecimal.Zero : new ZDecimal(Utilities.Round(part * 100m / total, 1));
			ZDecimal calculateValuePercent(ZDecimal part, ZDecimal total) => total.IsEmpty ? ZDecimal.Zero : new ZDecimal(Utilities.Round(part * 100m / total, 1));
		}

		bool isInUpdateReportablePercentColumns;

		#region Reportable Columns

		// Column A - BusinessName - ATR_CompanyName

		// Column B - ABN - ATR_VATRegNo

		/// <summary>
		/// ACN
		/// </summary>
		public PtrsReportRegNumberColumn C { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(C), PtrsReportRegNumberColumn.ACNLength); }

		[ReadOnlyMember(nameof(C_Value_ReadOnly))]
		public ZString C_Value { get => C.Value; set => C.Value = value; }
		public ZPropertyInfo C_ValueInfo => GetWrappedZPropertyInfo(nameof(C_Value), _ => C.ValueInfo);

		bool C_Value_ReadOnly => !ATR_VATRegNo.IsEmpty;

		/// <summary>
		/// ControllingCorporationName
		/// </summary>
		public PtrsReportCommentColumn D { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(D), 400); }
		public ZString D_Value
		{
			get => D.Value;
			set
			{
				D.Value = value;
				if (value.IsEmpty)
				{
					E.Value = F.Value = ZString.Empty;
				}
				E_ValueInfo.RefreshBinding();
				F_ValueInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo D_ValueInfo => GetWrappedZPropertyInfo(nameof(D_Value), _ => D.ValueInfo);

		/// <summary>
		/// ControllingCorporationABN
		/// </summary>
		public PtrsReportRegNumberColumn E { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(E), PtrsReportRegNumberColumn.ABNLength); }

		[ReadOnlyMember(nameof(E_Value_ReadOnly))]
		public ZString E_Value { get => E.Value; set => E.Value = value; }
		public ZPropertyInfo E_ValueInfo => GetWrappedZPropertyInfo(nameof(E_Value), _ => E.ValueInfo);

		bool E_Value_ReadOnly => D_Value.IsEmpty;

		/// <summary>
		/// ControllingCorporationACN
		/// </summary>
		public PtrsReportRegNumberColumn F { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(F), PtrsReportRegNumberColumn.ACNLength); }

		[ReadOnlyMember(nameof(F_Value_ReadOnly))]
		public ZString F_Value { get => F.Value; set => F.Value = value; }
		public ZPropertyInfo F_ValueInfo => GetWrappedZPropertyInfo(nameof(F_Value), _ => F.ValueInfo);

		bool F_Value_ReadOnly => D_Value.IsEmpty;

		/// <summary>
		/// HeadEntityName
		/// </summary>
		public PtrsReportCommentColumn G { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(G), 400); }
		public ZString G_Value
		{
			get => G.Value;
			set
			{
				G.Value = value;
				if (value.IsEmpty)
				{
					H.Value = I.Value = ZString.Empty;
				}
				H_ValueInfo.RefreshBinding();
				I_ValueInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo G_ValueInfo => GetWrappedZPropertyInfo(nameof(G_Value), _ => G.ValueInfo);

		/// <summary>
		/// HeadEntityABN
		/// </summary>
		public PtrsReportRegNumberColumn H { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(H), PtrsReportRegNumberColumn.ABNLength); }

		[ReadOnlyMember(nameof(H_Value_ReadOnly))]
		public ZString H_Value { get => H.Value; set => H.Value = value; }
		public ZPropertyInfo H_ValueInfo => GetWrappedZPropertyInfo(nameof(H_Value), _ => H.ValueInfo);

		bool H_Value_ReadOnly => G_Value.IsEmpty;

		/// <summary>
		/// HeadEntityACN
		/// </summary>
		public PtrsReportRegNumberColumn I { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(I), PtrsReportRegNumberColumn.ACNLength); }

		[ReadOnlyMember(nameof(I_Value_ReadOnly))]
		public ZString I_Value { get => I.Value; set => I.Value = value; }
		public ZPropertyInfo I_ValueInfo => GetWrappedZPropertyInfo(nameof(I_Value), _ => I.ValueInfo);

		bool I_Value_ReadOnly => G_Value.IsEmpty;

		/// <summary>
		/// BusinessIndustryCode
		/// </summary>
		public PtrsReportRegNumberColumn J { get => this.GetOrCreateColumn<PtrsReportRegNumberColumn>(nameof(J), PtrsReportRegNumberColumn.BICLength); }
		public ZString J_Value { get => J.Value; set => J.Value = value; }
		public ZPropertyInfo J_ValueInfo => GetWrappedZPropertyInfo(nameof(J_Value), _ => J.ValueInfo);

		/// <summary>
		/// Column K - ReportingPeriodStartDate - ComplianceReport.ACR_DateFrom
		/// </summary>
		public ZDate DateFrom { get => ComplianceReport.ACR_DateFrom; }

		/// <summary>
		/// Column L - ReportingPeriodEndDate - ComplianceReport.ACR_DateTo
		/// </summary>
		public ZDate DateTo { get => ComplianceReport.ACR_DateTo; }

		/// <summary>
		/// StandardPaymentPeriodInCalendarDays
		/// </summary>
		public PtrsReportNumberColumn M { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(M)); }
		public ZInt M_Value { get => M.Value; set => M.Value = value; }
		public ZPropertyInfo M_ValueInfo => GetWrappedZPropertyInfo(nameof(M_Value), _ => M.ValueInfo);

		/// <summary>
		/// ChangesToStandardPaymentPeriod
		/// </summary>
		public PtrsReportNumberColumn N { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(N)); }
		public ZInt N_Value { get => N.Value; set => N.Value = value; }
		public ZPropertyInfo N_ValueInfo => GetWrappedZPropertyInfo(nameof(N_Value), _ => N.ValueInfo);

		/// <summary>
		/// DetailsOfChangesToStandardPaymentPeriod
		/// </summary>
		public PtrsReportCommentColumn O { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(O)); }
		public ZString O_Value { get => O.Value; set => O.Value = value; }
		public ZPropertyInfo O_ValueInfo => GetWrappedZPropertyInfo(nameof(O_Value), _ => O.ValueInfo);

		/// <summary>
		/// ShortestActualStandardPaymentPeriod
		/// </summary>
		public PtrsReportNumberColumn P { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(P)); }
		public ZInt P_Value { get => P.Value; set => P.Value = value; }
		public ZPropertyInfo P_ValueInfo => GetWrappedZPropertyInfo(nameof(P_Value), _ => P.ValueInfo);

		/// <summary>
		/// ChangeShortestActualPaymentPeriod
		/// </summary>
		public PtrsReportNumberColumn Q { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(Q)); }
		public ZInt Q_Value { get => Q.Value; set => Q.Value = value; }
		public ZPropertyInfo Q_ValueInfo => GetWrappedZPropertyInfo(nameof(Q_Value), _ => Q.ValueInfo);

		/// <summary>
		/// DetailChangeShortestActualPaymentPeriod
		/// </summary>
		public PtrsReportCommentColumn R { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(R)); }
		public ZString R_Value { get => R.Value; set => R.Value = value; }
		public ZPropertyInfo R_ValueInfo => GetWrappedZPropertyInfo(nameof(R_Value), _ => R.ValueInfo);

		/// <summary>
		/// LongestActualStandardPaymentPeriod
		/// </summary>
		public PtrsReportNumberColumn S { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(S)); }
		public ZInt S_Value { get => S.Value; set => S.Value = value; }
		public ZPropertyInfo S_ValueInfo => GetWrappedZPropertyInfo(nameof(S_Value), _ => S.ValueInfo);

		/// <summary>
		/// ChangeLongestActualPaymentPeriod
		/// </summary>
		public PtrsReportNumberColumn T { get => this.GetOrCreateColumn<PtrsReportNumberColumn>(nameof(T)); }
		public ZInt T_Value { get => T.Value; set => T.Value = value; }
		public ZPropertyInfo T_ValueInfo => GetWrappedZPropertyInfo(nameof(T_Value), _ => T.ValueInfo);

		/// <summary>
		/// DetailChangeLongestActualPaymentPeriod
		/// </summary>
		public PtrsReportCommentColumn U { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(U)); }
		public ZString U_Value { get => U.Value; set => U.Value = value; }
		public ZPropertyInfo U_ValueInfo => GetWrappedZPropertyInfo(nameof(U_Value), _ => U.ValueInfo);

		/// <summary>
		/// NumberInvoicesPaidWithin20DaysOfReceipt
		/// </summary>
		public PtrsReportPercentColumn V { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(V)); }
		[DecimalPlaces(1)]
		public ZDecimal V_Value { get => V.Value; }

		/// <summary>
		/// NumberInvoicesPaidBetween21And30Days
		/// </summary>
		public PtrsReportPercentColumn W { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(W)); }
		[DecimalPlaces(1)]
		public ZDecimal W_Value { get => W.Value; }

		/// <summary>
		/// NumberInvoicesPaidBetween31And60Days
		/// </summary>
		public PtrsReportPercentColumn X { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(X)); }
		[DecimalPlaces(1)]
		public ZDecimal X_Value { get => X.Value; }

		/// <summary>
		/// NumberInvoicePaidBetween61And90Days
		/// </summary>
		public PtrsReportPercentColumn Y { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(Y)); }
		[DecimalPlaces(1)]
		public ZDecimal Y_Value { get => Y.Value; }

		/// <summary>
		/// NumberInvoicesPaidBetween91And120Days
		/// </summary>
		public PtrsReportPercentColumn Z { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(Z)); }
		[DecimalPlaces(1)]
		public ZDecimal Z_Value { get => Z.Value; }

		/// <summary>
		/// NumberInvoicesPaidInMoreThan120Days
		/// </summary>
		public PtrsReportPercentColumn AA { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AA)); }
		[DecimalPlaces(1)]
		public ZDecimal AA_Value { get => AA.Value; }

		/// <summary>
		/// ValueInvoicePaidByNumberWithin20Days
		/// </summary>
		public PtrsReportPercentColumn AB { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AB)); }
		[DecimalPlaces(1)]
		public ZDecimal AB_Value { get => AB.Value; }

		/// <summary>
		/// ValueInvoicesPaidBetween21And30Days
		/// </summary>
		public PtrsReportPercentColumn AC { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AC)); }
		[DecimalPlaces(1)]
		public ZDecimal AC_Value { get => AC.Value; }

		/// <summary>
		/// ValueInvoicesPaidBetween31And60Days
		/// </summary>
		public PtrsReportPercentColumn AD { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AD)); }
		[DecimalPlaces(1)]
		public ZDecimal AD_Value { get => AD.Value; }

		/// <summary>
		/// ValueInvoicesPaidBetween61And90Days
		/// </summary>
		public PtrsReportPercentColumn AE { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AE)); }
		[DecimalPlaces(1)]
		public ZDecimal AE_Value { get => AE.Value; }

		/// <summary>
		/// ValueInvoicesPaidBetween91And120Days
		/// </summary>
		public PtrsReportPercentColumn AF { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AF)); }
		[DecimalPlaces(1)]
		public ZDecimal AF_Value { get => AF.Value; }

		/// <summary>
		/// ValueInvoicesPaidInMoreThan120Days
		/// </summary>
		public PtrsReportPercentColumn AG { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AG)); }
		[DecimalPlaces(1)]
		public ZDecimal AG_Value { get => AG.Value; }

		/// <summary>
		/// InvoicePracticesAndArrangements
		/// </summary>
		public PtrsReportCommentColumn AH { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AH)); }
		public ZString AH_Value { get => AH.Value; set => AH.Value = value; }
		public ZPropertyInfo AH_ValueInfo => GetWrappedZPropertyInfo(nameof(AH_Value), _ => AH.ValueInfo);

		/// <summary>
		/// PracticesAndArrangementsForLodgingTender
		/// </summary>
		public PtrsReportCommentColumn AI { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AI)); }
		public ZString AI_Value { get => AI.Value; set => AI.Value = value; }
		public ZPropertyInfo AI_ValueInfo => GetWrappedZPropertyInfo(nameof(AI_Value), _ => AI.ValueInfo);

		/// <summary>
		/// PracticesAndArrangementsToAcceptInvoice
		/// </summary>
		public PtrsReportCommentColumn AJ { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AJ)); }
		public ZString AJ_Value { get => AJ.Value; set => AJ.Value = value; }
		public ZPropertyInfo AJ_ValueInfo => GetWrappedZPropertyInfo(nameof(AJ_Value), _ => AJ.ValueInfo);

		/// <summary>
		/// TotalValueOfSmallBusinessProcurement
		/// </summary>
		public PtrsReportPercentColumn AK { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AK)); }
		[DecimalPlaces(1)]
		public ZDecimal AK_Value { get => AK.Value; }

		/// <summary>
		/// SupplyChainFinanceArrangements
		/// </summary>
		public PtrsReportCommentColumn AL { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AL)); }
		public ZString AL_Value { get => AL.Value; set => AL.Value = value; }
		public ZPropertyInfo AL_ValueInfo => GetWrappedZPropertyInfo(nameof(AL_Value), _ => AL.ValueInfo);

		/// <summary>
		/// TotalNumberSupplyChainFinanceArrangement
		/// </summary>
		public PtrsReportPercentColumn AM { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AM)); }
		[DecimalPlaces(1)]
		public ZDecimal AM_Value { get => AM.Value; }

		/// <summary>
		/// TotalValueSupplyChainFinanceArrangements
		/// </summary>
		public PtrsReportPercentColumn AN { get => this.GetOrCreateColumn<PtrsReportPercentColumn>(nameof(AN)); }
		[DecimalPlaces(1)]
		public ZDecimal AN_Value { get => AN.Value; }

		/// <summary>
		/// BenefitsOfSupplyChainFinanceArrangements
		/// </summary>
		public PtrsReportCommentColumn AO { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AO)); }
		public ZString AO_Value { get => AO.Value; set => AO.Value = value; }
		public ZPropertyInfo AO_ValueInfo => GetWrappedZPropertyInfo(nameof(AO_Value), _ => AO.ValueInfo);

		/// <summary>
		/// RequirementToUseSupplyChainFinance
		/// </summary>
		public PtrsReportCommentColumn AP { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AP)); }
		public ZString AP_Value { get => AP.Value; set => AP.Value = value; }
		public ZPropertyInfo AP_ValueInfo => GetWrappedZPropertyInfo(nameof(AP_Value), _ => AP.ValueInfo);

		/// <summary>
		/// RequirementToUseSupplyChainFinance
		/// </summary>
		public PtrsReportDateColumn AQ { get => this.GetOrCreateColumn<PtrsReportDateColumn>(nameof(AQ)); }
		public ZDateTime AQ_Value { get => AQ.Value; set => AQ.Value = value; }
		public ZPropertyInfo AQ_ValueInfo => GetWrappedZPropertyInfo(nameof(AQ_Value), _ => AQ.ValueInfo);

		/// <summary>
		/// DetailOfChangeInBusinessName
		/// </summary>
		public PtrsReportCommentColumn AR { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AR)); }
		public ZString AR_Value { get => AR.Value; set => AR.Value = value; }
		public ZPropertyInfo AR_ValueInfo => GetWrappedZPropertyInfo(nameof(AR_Value), _ => AR.ValueInfo);

		/// <summary>
		/// DetailEntitesBelowReportingThreshold
		/// </summary>
		public PtrsReportCommentColumn AS { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AS)); }
		public ZString AS_Value { get => AS.Value; set => AS.Value = value; }
		public ZPropertyInfo AS_ValueInfo => GetWrappedZPropertyInfo(nameof(AS_Value), _ => AS.ValueInfo);

		/// <summary>
		/// ReportComments
		/// </summary>
		public PtrsReportCommentColumn AT { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AT)); }
		public ZString AT_Value { get => AT.Value; set => AT.Value = value; }
		public ZPropertyInfo AT_ValueInfo => GetWrappedZPropertyInfo(nameof(AT_Value), _ => AT.ValueInfo);

		/// <summary>
		/// SubmitterFirstName
		/// </summary>
		public PtrsReportCommentColumn AU { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AU), 400); }
		public ZString AU_Value { get => AU.Value; set => AU.Value = value; }
		public ZPropertyInfo AU_ValueInfo => GetWrappedZPropertyInfo(nameof(AU_Value), _ => AU.ValueInfo);

		/// <summary>
		/// SubmitterLastName
		/// </summary>
		public PtrsReportCommentColumn AV { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AV), 400); }
		public ZString AV_Value { get => AV.Value; set => AV.Value = value; }
		public ZPropertyInfo AV_ValueInfo => GetWrappedZPropertyInfo(nameof(AV_Value), _ => AV.ValueInfo);

		/// <summary>
		/// SubmitterPosition
		/// </summary>
		public PtrsReportCommentColumn AW { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AW), 400); }
		public ZString AW_Value { get => AW.Value; set => AW.Value = value; }
		public ZPropertyInfo AW_ValueInfo => GetWrappedZPropertyInfo(nameof(AW_Value), _ => AW.ValueInfo);

		/// <summary>
		/// SubmitterPhoneNumber
		/// </summary>
		public PtrsReportCommentColumn AX { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AX), 50); }
		public ZString AX_Value { get => AX.Value; set => AX.Value = value; }
		public ZPropertyInfo AX_ValueInfo => GetWrappedZPropertyInfo(nameof(AX_Value), _ => AX.ValueInfo);

		/// <summary>
		/// SubmitterEmail
		/// </summary>
		public PtrsReportEmailColumn AY { get => this.GetOrCreateColumn<PtrsReportEmailColumn>(nameof(AY), 50); }
		public ZString AY_Value { get => AY.Value; set => AY.Value = value; }
		public ZPropertyInfo AY_ValueInfo => GetWrappedZPropertyInfo(nameof(AY_Value), _ => AY.ValueInfo);

		/// <summary>
		/// ApproverFirstName
		/// </summary>
		public PtrsReportCommentColumn AZ { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(AZ), 400); }
		public ZString AZ_Value { get => AZ.Value; set => AZ.Value = value; }
		public ZPropertyInfo AZ_ValueInfo => GetWrappedZPropertyInfo(nameof(AZ_Value), _ => AZ.ValueInfo);

		/// <summary>
		/// SubmitterLastName
		/// </summary>
		public PtrsReportCommentColumn BA { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(BA), 400); }
		public ZString BA_Value { get => BA.Value; set => BA.Value = value; }
		public ZPropertyInfo BA_ValueInfo => GetWrappedZPropertyInfo(nameof(BA_Value), _ => BA.ValueInfo);

		/// <summary>
		/// ApproverPosition
		/// </summary>
		public PtrsReportCommentColumn BB { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(BB), 400); }
		public ZString BB_Value { get => BB.Value; set => BB.Value = value; }
		public ZPropertyInfo BB_ValueInfo => GetWrappedZPropertyInfo(nameof(BB_Value), _ => BB.ValueInfo);

		/// <summary>
		/// ApproverPhoneNumber
		/// </summary>
		public PtrsReportCommentColumn BC { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(BC), 50); }
		public ZString BC_Value { get => BC.Value; set => BC.Value = value; }
		public ZPropertyInfo BC_ValueInfo => GetWrappedZPropertyInfo(nameof(BC_Value), _ => BC.ValueInfo);

		/// <summary>
		/// ApproverEmail
		/// </summary>
		public PtrsReportEmailColumn BD { get => this.GetOrCreateColumn<PtrsReportEmailColumn>(nameof(BD), 50); }
		public ZString BD_Value { get => BD.Value; set => BD.Value = value; }
		public ZPropertyInfo BD_ValueInfo => GetWrappedZPropertyInfo(nameof(BD_Value), _ => BD.ValueInfo);

		/// <summary>
		/// ApprovalDate
		/// </summary>
		public PtrsReportDateColumn BE { get => this.GetOrCreateColumn<PtrsReportDateColumn>(nameof(BE), valueCheck: CheckNotBeforeReportDateTo); }
		[ResourceStringData("PtrsReport|BE_Value", Caption = "Approval Date")]
		public ZDateTime BE_Value { get => BE.Value; set => BE.Value = value; }
		public ZPropertyInfo BE_ValueInfo => GetWrappedZPropertyInfo(nameof(BE_Value), _ => BE.ValueInfo);

		/// <summary>
		/// PrincipalGoverningBodyName
		/// </summary>
		public PtrsReportCommentColumn BF { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(BF), 400); }
		public ZString BF_Value { get => BF.Value; set => BF.Value = value; }
		public ZPropertyInfo BF_ValueInfo => GetWrappedZPropertyInfo(nameof(BF_Value), _ => BF.ValueInfo);

		/// <summary>
		/// PrincipalGoverningBodyDescription
		/// </summary>
		public PtrsReportCommentColumn BG { get => this.GetOrCreateColumn<PtrsReportCommentColumn>(nameof(BG)); }
		public ZString BG_Value { get => BG.Value; set => BG.Value = value; }
		public ZPropertyInfo BG_ValueInfo => GetWrappedZPropertyInfo(nameof(BG_Value), _ => BG.ValueInfo);

		/// <summary>
		/// ResponsibleMemberDeclaration
		/// </summary>
		public PtrsReportDateColumn BH { get => this.GetOrCreateColumn<PtrsReportDateColumn>(nameof(BH), valueCheck: CheckNotBeforeReportDateToAndApprovalDate); }
		[ResourceStringData("PtrsReport|BH_Value", Caption = "Declaration Date")]
		public ZDateTime BH_Value { get => BH.Value; set => BH.Value = value; }
		public ZPropertyInfo BH_ValueInfo => GetWrappedZPropertyInfo(nameof(BH_Value), _ => BH.ValueInfo);

		void CheckNotBeforeReportDateTo(ZPropertyInfo info) => CheckNotBeforeDate(info, DateTo.ToZDateTime());

		void CheckNotBeforeReportDateToAndApprovalDate(ZPropertyInfo info)
		{
			if (!info.Value.IsEmpty && info.Value is ZDateTime dateTime)
			{
				var dateToCompare = !BE.Value.IsEmpty && dateTime < BE.Value
					? BE.Value
					: DateTo.ToZDateTime();
				CheckNotBeforeDate(info, dateToCompare);
			}
		}

		void CheckNotBeforeDate(ZPropertyInfo info, ZDateTime dateToCompare)
		{
			if (!info.Value.IsEmpty && !dateToCompare.IsEmpty
				&& info.Value is ZDateTime dateTime
				&& dateTime < dateToCompare)
			{
				info.AddError(Res.GetString("14a0cde0-9f6e-47c6-84eb-470c2cbd81df", "The Date must be not earlier than '{0}'.", dateToCompare.ToString("dd/MM/yyyy")));
			}
		}

		#endregion

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (HasChanges)
			{
				if (ATR_Status.IsEmpty || (ATR_StatusInfo.OriginalValue.Equals(Status.Generated) && IsGenerated))
				{
					ATR_Status = Status.Saved;
				}
			}
		}

		#region Property overrides

		[ResourceStringData("PtrsReport|ATR_Comment", Caption = "Comment")]
		public override ZString ATR_Comment { get => base.ATR_Comment; set => base.ATR_Comment = value; }

		[ResourceStringData("PtrsReport|ATR_VATRegNo", Caption = "ABN")]
		public override ZString ATR_VATRegNo { get => base.ATR_VATRegNo; set => base.ATR_VATRegNo = value; }

		[ResourceStringData("PtrsReport|ATR_Version", Caption = "Version")]
		public override ZInt ATR_Version { get => base.ATR_Version; set => base.ATR_Version = value; }

		#endregion

		#region ReadOnly properties

		protected bool ATR_Comment_ReadOnly => IsSubmitted;
		protected bool ATR_ACR_ComplianceReport_ReadOnly => true;
		protected bool ATR_Address1_ReadOnly => true;
		protected bool ATR_Address2_ReadOnly => true;
		protected bool ATR_City_ReadOnly => true;
		protected bool ATR_CompanyName_ReadOnly => true;
		protected bool ATR_GovtReceiptInformation_ReadOnly => true;
		protected bool ATR_PostCode_ReadOnly => true;
		protected bool ATR_ReturnType_ReadOnly => true;
		protected bool ATR_RN_NKCountryCode_ReadOnly => true;
		protected bool ATR_State_ReadOnly => true;
		protected bool ATR_Status_ReadOnly => true;
		protected bool ATR_VATRegNo_ReadOnly => true;
		protected bool ATR_Version_ReadOnly => true;

		#endregion
	}
}
