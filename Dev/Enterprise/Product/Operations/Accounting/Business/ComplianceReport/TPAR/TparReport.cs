using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.TPAR
{
	public class TparReport : AccTaxReturn, IDocManagerSupport
	{
		public TparReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SubmitReport()
		{
			if (IsGenerated)
			{
				Reload();

				var linesToDelete = Lines.Cast<TparReportCreditorLine>().Where(x => !x.IsInDatabase).ToArray();
				foreach (var line in linesToDelete)
				{
					Lines.RemoveAndDelete(line);
				}
				Lines.Reload(true);

				ATR_Status = Status.Submitted;
			}
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

		public ZString LastGeneratedFileName => DocManagerInfo.AllEDocs.Cast<IeDoc>()
			.Where(x => x.DocType == FileType && x.Description == FileDescription && x.FileName.StartsWith(FileNameID, StringComparison.OrdinalIgnoreCase))
			.OrderByDescending(y => y.DateAdded).FirstOrDefault()?.FileName ?? string.Empty;

		void WriteFileData(StreamWriter writer)
		{
			writer.WriteLine(GetSenderDataRecord1());
			writer.WriteLine(GetSenderDataRecord2());
			writer.WriteLine(GetSenderDataRecord3());
			writer.WriteLine(GetPayerIdentityDataRecord());
			writer.WriteLine(GetSoftwareDataRecord());

			var linesToGenerate = Lines.Cast<TparReportCreditorLine>().Where(x => x.ARL_OverriddenTotalAmountIncludingTax > 0);
			linesToGenerate.ForEach(y => writer.WriteLine(y.GetPayeeDataRecord()));

			writer.WriteLine(GetTotalDataRecord(6 + linesToGenerate.Count())); //number of lines in the file
		}

		#region eDocs

		string FileNameID => "TPAR-" + FileReference;
		string FileDescription => (NoResString)"Compliance Tax Report";
		string FileType => Core.Constants.RefDocTypes.MiscellaneousDocument;

		string CreateAndAttachFileToEdoc()
		{
			DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(true);
			var filename = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}-{1}.txt", FileNameID, ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));

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

		#region DataRecords

		string GetSenderDataRecord1()
		{
			var record = TparReportHelper.RecordHeader + "IDENTREGISTER1" +
						 TparReportHelper.FormatAustralianBusinessNumber(ATR_VATRegNo) + RunType +
						 ComplianceReport.ACR_DateTo.ToString("ddMMyyyy", CultureInfo.InvariantCulture) +
						 'P' + 'C' + 'M' + TparReportHelper.FixWidth(AtoReportSpecificationVersion, 10);
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		string GetSenderDataRecord2()
		{
			var record = TparReportHelper.RecordHeader + "IDENTREGISTER2" +
						 TparReportHelper.FixWidth(ATR_CompanyName, 200) +
						 TparReportHelper.FixWidth(SenderContactName, 38) +
						 TparReportHelper.FixWidth(SenderContactPhone, 15) +
						 TparReportHelper.FixWidth(SenderFaxNumber, 15) +
						 TparReportHelper.FixWidth(FileReference, 16);
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		string GetSenderDataRecord3()
		{
			var record = TparReportHelper.RecordHeader + "IDENTREGISTER3" +
						 TparReportHelper.FixWidth(ATR_Address1, 38) +
						 TparReportHelper.FixWidth(ATR_Address2, 38) +
						 TparReportHelper.FixWidth(ATR_City, 27) +
						 TparReportHelper.FixWidth(ATR_State, 3) +
						 TparReportHelper.FixWidth(ATR_PostCode, 4, padWithZeros: true) +
						 TparReportHelper.FixWidth(SenderCountryName, 20) +
						 TparReportHelper.FixWidth(string.Empty, 38) + // Ignoring Sender Postal Address Line 1
						 TparReportHelper.FixWidth(string.Empty, 38) + // Ignoring Sender Postal Address Line 2
						 TparReportHelper.FixWidth(string.Empty, 27) + // Ignoring Sender Postal City
						 TparReportHelper.FixWidth(string.Empty, 3) +  // Ignoring Sender Postal State
						 TparReportHelper.FixWidth(string.Empty, 4, padWithZeros: true) + // Ignoring Sender Postal PostCode
						 TparReportHelper.FixWidth(string.Empty, 20) + // Ignoring Sender Postal Country
						 TparReportHelper.FixWidth(SenderEmail, 76);
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		string GetPayerIdentityDataRecord()
		{
			var record = TparReportHelper.RecordHeader + "IDENTITY" +
						 TparReportHelper.FormatAustralianBusinessNumber(ATR_VATRegNo) +
						 TparReportHelper.FixWidth(PayerBranchNumber, 3) +
						 TparReportHelper.FixWidth(FinancialYear, 4, padWithZeros: true) +
						 TparReportHelper.FixWidth(ATR_CompanyName, 200) +
						 TparReportHelper.FixWidth(string.Empty, 200) + // Ignoring Payer Trading Name
						 TparReportHelper.FixWidth(ATR_Address1, 38) +
						 TparReportHelper.FixWidth(ATR_Address2, 38) +
						 TparReportHelper.FixWidth(ATR_City, 27) +
						 TparReportHelper.FixWidth(ATR_State, 3) +
						 TparReportHelper.FixWidth(ATR_PostCode, 4, padWithZeros: true) +
						 TparReportHelper.FixWidth(SenderCountryName, 20) +
						 TparReportHelper.FixWidth(SenderContactName, 38) +
						 TparReportHelper.FixWidth(PayerContactPhone, 15) +
						 TparReportHelper.FixWidth(PayerFaxNumber, 15) +
						 TparReportHelper.FixWidth(PayerEmail, 76);
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		string GetSoftwareDataRecord()
		{
			var record = TparReportHelper.RecordHeader + (NoResString)"SOFTWARECOMMERCIAL" +
						 (NoResString)"Wisetech Global " + DbConnectionConstants.ApplicationNames.CargoWiseOne +
						 (NoResString)" v" + new EnterpriseInformationRetriever().VersionNumber;
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		string GetTotalDataRecord(int numberOfRecords)
		{
			var record = TparReportHelper.RecordHeader + "FILE-TOTAL" +
						 numberOfRecords.ToString(CultureInfo.InvariantCulture).PadLeft(8, '0');
			return TparReportHelper.FixWidth(record, TparReportHelper.RecordLength);
		}

		#endregion

		#region Report details

		string FileReference => string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}-{1}-v{2}",
			ComplianceReport.Company.GC_Code,
			FinancialYear,
			ATR_Version);

		string FinancialYear => TparReportHelper.GetFinancialYear(ComplianceReport);

		string RunType => EnvProxy.Instance.IsProductionSystem ? "P" : "T";

		string AtoReportSpecificationVersion => "FPAIVV02.0";

		#endregion

		#region Sender details

		string SenderCountryName => TparReportHelper.GetCountryName(Country);

		string SenderContactName => EnvProxy.Instance.CurrentUser.FullName;

		public string SenderContactPhone => !string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.WorkPhone) ?
			EnvProxy.Instance.CurrentUser.WorkPhone : (string)ComplianceReport.Company.OrgProxy.MainAddress.OA_Phone;

		string SenderFaxNumber => !string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.Fax) ?
			EnvProxy.Instance.CurrentUser.Fax : (string)ComplianceReport.Company.OrgProxy.MainAddress.OA_Fax;

		string SenderEmail => !string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.EmailAddress) ?
			EnvProxy.Instance.CurrentUser.EmailAddress : (string)ComplianceReport.Company.OrgProxy.MainAddress.OA_Email;

		#endregion

		#region Payer details

		string PayerBranchNumber => TparReportHelper.GetNumbersOnlyAustralianBusinessNumber(ATR_VATRegNo.PadLeft(11, '0') + "001").Substring(11, 3);

		string PayerContactPhone => ComplianceReport.Company.OrgProxy.MainAddress.OA_Phone;

		string PayerFaxNumber => ComplianceReport.Company.OrgProxy.MainAddress.OA_Fax;

		string PayerEmail => ComplianceReport.Company.OrgProxy.MainAddress.OA_Email;

		#endregion

		#endregion

		#region Load Details

		public override void OnLoaded()
		{
			base.OnLoaded();
			SetHeaderDetails();
		}

		public override ZGuid ATR_ACR_ComplianceReport
		{
			get => base.ATR_ACR_ComplianceReport;
			set
			{
				base.ATR_ACR_ComplianceReport = value;
				SetHeaderDetails();
			}
		}

		void SetHeaderDetails()
		{
			if (ComplianceReport != null && ComplianceReport.Company != null && !IsSubmitted)
			{
				var company = ComplianceReport.Company;
				var orgProxy = company.OrgProxy;

				ATR_VATRegNo = TparReportHelper.GetAustralianBusinessNumber(orgProxy);
				ATR_CompanyName = orgProxy?.OH_FullName ?? string.Empty;
				ATR_Address1 = company.GC_Address1;
				ATR_Address2 = company.GC_Address2;
				ATR_City = TparReportHelper.GetCity(company.GC_City, company.GC_PostCode, company.GC_State, company.GC_RN_NKCountryCode);
				ATR_State = TparReportHelper.GetState(company.GC_State, company.GC_RN_NKCountryCode);
				ATR_PostCode = TparReportHelper.GetPostCode(company.GC_PostCode, company.GC_RN_NKCountryCode);
				ATR_RN_NKCountryCode = company.GC_RN_NKCountryCode;

				Lines.Cast<TparReportCreditorLine>().ForEach(x => x.SetLineDetails());
			}
		}

		#endregion

		#region Lines

		public new TparReportCreditorLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new TparReportCreditorLineCollection(this);
					RegisterEditableChildObject(lines);
					lines.Load();
					SynchronizeCreditorLinesWithComplianceReportLines();
				}
				return lines;
			}
		}
		TparReportCreditorLineCollection lines;

		void SynchronizeCreditorLinesWithComplianceReportLines()
		{
			if (ComplianceReport != null && !IsSubmitted)
			{
				var groupedComplianceReportLines = ComplianceReport.ReportLines.Cast<AccComplianceReportLine>().GroupBy(x => x.OH_Code).Select(y => new
				{
					OrgCode = y.First().OH_Code,
					TotalGST = Math.Floor(y.Sum(z => z.TotalTaxAmount)),
					TotalAmountIncludingTax = Math.Floor(y.Sum(z => z.TotalExTaxAmount + z.TotalTaxAmount))
				});

				if (groupedComplianceReportLines.Any())
				{
					var orgCodes = groupedComplianceReportLines.Select(x => x.OrgCode).ToArray();
					var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgCodes));

					//synchronize compliance report lines with creditor lines
					foreach (var groupedComplianceReportLine in groupedComplianceReportLines)
					{
						var org = orgHeaders.First(x => x.OH_Code == groupedComplianceReportLine.OrgCode);
						var creditorLine = lines.Cast<TparReportCreditorLine>().FirstOrDefault(x => x.ARL_OH_Organisation == org.PK);

						if (org.CompanyData.IsIncludedInTparReport)
						{
							if (creditorLine == null)
							{
								creditorLine = lines.AddNew();
							}
							SetCreditorLineOrgDetails(creditorLine, org);
							SetCreditorLineAmounts(creditorLine, groupedComplianceReportLine.TotalAmountIncludingTax, groupedComplianceReportLine.TotalGST);
						}
						else if (creditorLine != null)
						{
							lines.RemoveAndDelete(creditorLine);
						}
					}

					//synchronize creditor lines with compliance report lines
					var orgPks = orgHeaders.Select(x => x.PK).ToHashSet();
					lines.Cast<TparReportCreditorLine>().Where(x => !orgPks.Contains(x.ARL_OH_Organisation)).ToList().ForEach(y => lines.RemoveAndDelete(y));
				}
			}
		}

		void SetCreditorLineOrgDetails(TparReportCreditorLine creditorLine, OrgHeader org)
		{
			if (!IsSubmitted)
			{
				creditorLine.ARL_OH_Organisation = org.PK;
				creditorLine.SetLineDetails();
			}
		}

		void SetCreditorLineAmounts(TparReportCreditorLine creditorLine, decimal totalAmount, decimal gstAmount)
		{
			if (!creditorLine.IsTotalAmountOverridden)
			{
				creditorLine.ARL_OverriddenTotalAmountIncludingTax = totalAmount;
			}
			if (!creditorLine.IsGSTAmountOverridden)
			{
				creditorLine.ARL_OverriddenGSTAmount = gstAmount;
			}
			creditorLine.ARL_TotalAmountIncludingTax = totalAmount;
			creditorLine.ARL_GSTAmount = gstAmount;
		}

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

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ATR_ReturnType = ReturnType.TPAR;
			ATR_Status = string.Empty;
			ATR_Version = 1;
		}

		protected override AccTaxReturnValidation GetNewValidation() => new TparReportValidation(this);

		#region Property overrides

		[ResourceStringData("TparReport|ATR_Comment", Caption = "Comment")]
		public override ZString ATR_Comment { get => base.ATR_Comment; set => base.ATR_Comment = value; }

		[ResourceStringData("TparReport|ATR_VATRegNo", Caption = "ABN")]
		public override ZString ATR_VATRegNo { get => base.ATR_VATRegNo; set => base.ATR_VATRegNo = value; }

		[ResourceStringData("TparReport|ATR_Version", Caption = "Version")]
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

#if DEBUG
		public void ClearReportLines_ForTestOnly()
		{
			lines = null;
		}
#endif
	}
}
