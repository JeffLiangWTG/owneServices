using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.ZMGermany
{
	public class ZMGermanyReport : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZMGermanyReport(AccComplianceReport complianceReport) : base(complianceReport?.Factory)
		{
			ComplianceReport = Argument.NotNull(complianceReport, nameof(complianceReport));
			selectedVersionString = AllVersions;
		}

		readonly BusinessObjectFactory localFactory = new ();
		internal const string AllVersions = "ALL";

		public ZMTaxReturnLinesCollectionView GridData
		{
			get
			{
				if (gridData == null)
				{
					(gridData, taxReturnHeaderList) = GetZMReportLines(ComplianceReport);
				}
				return gridData;
			}
		}
		ZMTaxReturnLinesCollectionView gridData;

		public List<AccTaxReturn> TaxReturnHeaderList
		{
			get
			{
				if (gridData == null)
				{
					(gridData, taxReturnHeaderList) = GetZMReportLines(ComplianceReport);
				}
				return taxReturnHeaderList;
			}
		}
		List<AccTaxReturn> taxReturnHeaderList;

		public AccTaxReturn CurrentTaxReturn => TaxReturnHeaderList.OrderByDescending(v => v.ATR_Version).FirstOrDefault();

		public AccComplianceReport ComplianceReport { get; }

		[ResourceStringData("ZMGermanyReport|RegistrationID", Caption = "Registration ID", FullDescription = "ZMI Registration ID in the config tab of the reporting organization")]
		public ZString RegistrationID => CurrentTaxReturn?.ATR_VATRegNo ?? ZString.Empty;

		[ResourceStringData("ZMGermanyReport|SenderID", Caption = "Sender ID", FullDescription = "STE Registration ID in the config tab of the reporting organization")]
		public ZString SenderID => CurrentTaxReturn?.ATR_GovtReturnIdentifier ?? ZString.Empty;

		[List("Version_List")]
		[ResourceStringData("ZMGermanyReport|SelectedVersion", Caption = "Version", FullDescription = "Version of the ELMA5 file. New: Transactions not in a version. Generated: A file has been generated. Submitted: A generated file has been submitted to the tax authorities.")]
		public ZString SelectedVersion
		{
			get { return selectedVersionString; }
			set
			{
				selectedVersionString = value;
				GridData.Rebuild();
			}
		}
		ZString selectedVersionString;

		public int SelectedVersionNumber => int.TryParse(SelectedVersion, out var versionAsInt) ? versionAsInt : 0;

		public CodeDescriptionPairList Version_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (TaxReturnHeaderList.Any())
				{
					var statusOrder = new ZString[] { AccTaxReturn.Status.Saved, AccTaxReturn.Status.Generated, AccTaxReturn.Status.Submitted };
					var lowestStatusIndex = TaxReturnHeaderList.Min(x => Array.IndexOf(statusOrder, x.ATR_Status));

					var statusLookupList = AccTaxReturnLookups.GetTaxReturnStatusList();
					statusLookupList.AddOverwriteIfExists(new CodeDescriptionPair(AccTaxReturn.Status.Saved, Res.GetString("47E70706-E0CF-49FE-9238-79D75C6BB126", "New")));

					result.AddPair(AllVersions, statusLookupList.GetDescriptionFromCode(statusOrder[lowestStatusIndex]));
					foreach (var taxReturn in TaxReturnHeaderList.OrderBy(v => v.ATR_Version))
					{
						result.AddPair(taxReturn.ATR_Version.ToString(), statusLookupList.GetDescriptionFromCode(taxReturn.ATR_Status));
					}
				}

				return result;
			}
		}

		public static string GetLineKey(AccTaxReturnLine taxReturnLine) => $"{taxReturnLine.ARL_RN_NKCountryCode}{taxReturnLine.ARL_OrgRegNo}";
		public static string GetLineKeyWithVersion(AccTaxReturnLine taxReturnLine) => $"{taxReturnLine.TaxReturn.ATR_Version}:{GetLineKey(taxReturnLine)}";

		(ZMTaxReturnLinesCollectionView, List<AccTaxReturn>) GetZMReportLines(AccComplianceReport complianceReport)
		{
			var taxReturnRecords = Factory.Load<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, complianceReport.PK)).OrderByDescending(v => v.ATR_Version);
			taxReturnHeaderList = taxReturnRecords.ToList();
			var highestVersionDictionary = new Dictionary<string, AccTaxReturnLine>();
			var allTaxReturnLines = new ZMTaxReturnLinesCollection(localFactory);
			var highestVersionNumber = (taxReturnRecords.FirstOrDefault()?.ATR_Version ?? ZInt.Zero) + 1;
			AccTaxReturn taxReturnHeader = null;

			foreach (var taxReturn in taxReturnRecords)
			{
				foreach (AccTaxReturnLine taxReturnLine in taxReturn.Lines)
				{
					var key = GetLineKey(taxReturnLine);
					if (!highestVersionDictionary.ContainsKey(key))
					{
						highestVersionDictionary.Add(key, taxReturnLine);
					}
					allTaxReturnLines.Add(taxReturnLine);
				}
			}

			var groupedReportLines = complianceReport.ReportLines.OfType<AccComplianceReportLine>().GroupBy(v => v.OrgCountryCode + v.OK_CustomsRegNo)
				.Select(v => new
				{
					Key = v.Key,
					CountryCode = v.First().OrgCountryCode,
					RegNo = v.First().OK_CustomsRegNo,
					OrgCode = v.First().OH_Code,
					Amount = v.Sum(a => a.TotalExTaxAmount)
				}
				);
			var organisations = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code,
				complianceReport.ReportLines.OfType<AccComplianceReportLine>().Select(v => (string)v.OH_Code).Distinct().ToArray())
				).ToDictionary(v => v.OH_Code, v => v);

			foreach (var line in groupedReportLines)
			{
				if (!highestVersionDictionary.TryGetValue(line.Key, out var otherLine) || line.Amount != otherLine.ARL_TotalAmountIncludingTax)
				{
					if (taxReturnHeader == null)
					{
						taxReturnHeader = CreateTaxReturnHeader(complianceReport, highestVersionNumber);
					}
					allTaxReturnLines.Add(CreateTaxReturnLine(taxReturnHeader, organisations[line.OrgCode], line.CountryCode, line.RegNo, line.Amount));
				}
			}

			if (taxReturnHeader != null && taxReturnHeader.Lines.Count > 0)
			{
				taxReturnHeaderList.Add(taxReturnHeader);
			}

			HasChanges = false;

			return (new ZMTaxReturnLinesCollectionView(allTaxReturnLines, (reportLine) => SelectedVersionNumber == 0 || reportLine.TaxReturn.ATR_Version == SelectedVersionNumber),
				taxReturnHeaderList);
		}

		internal AccTaxReturn CreateTaxReturnHeader(AccComplianceReport complianceReport, int highestVersionNumber)
		{
			var taxReturnHeader = localFactory.New<AccTaxReturn>();
			taxReturnHeader.ATR_ACR_ComplianceReport = complianceReport.PK;
			taxReturnHeader.ATR_CompanyName = complianceReport.Company.CompanyName;
			taxReturnHeader.ATR_Address1 = complianceReport.Company.Address1;
			taxReturnHeader.ATR_Address2 = complianceReport.Company.Address2;
			taxReturnHeader.ATR_City = complianceReport.Company.City;
			taxReturnHeader.ATR_PostCode = complianceReport.Company.Postcode;
			taxReturnHeader.ATR_RN_NKCountryCode = complianceReport.Company.GC_RN_NKCountryCode;
			taxReturnHeader.ATR_State = complianceReport.Company.State;
			taxReturnHeader.ATR_Version = highestVersionNumber;
			taxReturnHeader.ATR_Status = AccTaxReturn.Status.Saved;
			taxReturnHeader.ATR_ReturnType = "ZMD";

			var customsCodes = complianceReport.Company.OrgProxy.CustomsCodes;
			taxReturnHeader.ATR_VATRegNo = customsCodes.OfType<OrgCusCode>().FirstOrDefault(v => v.OK_CodeType == GermanyOrgCusCodeInfo.OrgCusCodes.ZMI)?.OK_CustomsRegNo ?? ZString.Empty;
			taxReturnHeader.ATR_GovtReturnIdentifier = customsCodes.OfType<OrgCusCode>().FirstOrDefault(v => v.OK_CodeType == GermanyOrgCusCodeInfo.OrgCusCodes.SteuerlicheIdentifikationsnummer)?.OK_CustomsRegNo ?? ZString.Empty;
			return taxReturnHeader;
		}

		internal AccTaxReturnLine CreateTaxReturnLine(AccTaxReturn header, OrgHeader organisation, ZString countryCode, ZString registrationNumber, ZDecimal amount)
		{
			var newLine = header.Lines.AddNew();
			newLine.ARL_RN_NKCountryCode = countryCode;
			newLine.ARL_OrgRegNo = registrationNumber;
			newLine.ARL_TotalAmountIncludingTax = amount;
			newLine.ARL_OH_Organisation = organisation.PK;
			newLine.ARL_OrgName = organisation.OH_FullName;
			newLine.ARL_City = organisation.CityName;
			newLine.ARL_Comment = organisation.OH_Code;
			return newLine;
		}

		public ZString ValidateCurrentTaxReturn()
		{
			var result = new ZStringBuilder();
			if (RegistrationID.IsEmpty)
			{
				result.Append(Res.GetString("766B4A06-E79F-4137-A937-3BAEB89D7D2F", "Please enter your BUNDESZENTRALAMT FUER STEUERN REGISTRATION-ID as registration code in your company."));
			}
			if (SenderID.IsEmpty)
			{
				result.Append(Res.GetString("CDCA5E51-FC79-46C9-9E69-7CD9EFC75FD5", "Please enter your BUNDESZENTRALAMT FUER STEUERN SENDERKENNUNG as registration code in your company."));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public bool CanGenerate()
		{
			return !taxReturnHeaderList.Any(v => v.ATR_Status == AccTaxReturn.Status.Generated);
		}

		public void MarkSelectedVersionAsGenerated()
		{
			var selectedVersion = TaxReturnHeaderList.Find(v => v.ATR_Version == SelectedVersionNumber);
			if (selectedVersion != null)
			{
				selectedVersion.ATR_Status = AccTaxReturn.Status.Generated;
				SetupLocalFactoryToSaveWithMainFactory();
			}
		}

		public void MarkGeneratedVersionRowAsSubmitted()
		{
			var generatedVersion = TaxReturnHeaderList.Find(v => v.ATR_Status == AccTaxReturn.Status.Generated);
			if (generatedVersion != null)
			{
				generatedVersion.ATR_Status = AccTaxReturn.Status.Submitted;
				SetupLocalFactoryToSaveWithMainFactory();
			}
		}

		void SetupLocalFactoryToSaveWithMainFactory()
		{
			Factory.Saved += UnlinkLocalFactoryFromMainFactory;
			Factory.ChildFactories.Add(localFactory);
		}

		void UnlinkLocalFactoryFromMainFactory(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				Factory.ChildFactories.Remove(localFactory);
				Factory.Saved -= UnlinkLocalFactoryFromMainFactory;
			}
		}
	}
}
