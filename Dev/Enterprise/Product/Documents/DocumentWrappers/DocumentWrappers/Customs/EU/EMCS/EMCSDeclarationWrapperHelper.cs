using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS
{
	public static class EMCSDeclarationWrapperHelper
	{
		public static ZString GetConsignorData(JobDocAddress jobDocAddress, bool exciseNumber = false)
		{
			if (jobDocAddress != null && jobDocAddress.IsValidAddress)
			{
				if (exciseNumber)
				{
					return GetTraderExciseNumber(jobDocAddress);
				}
				else
				{
					return GetNameAndAddress(jobDocAddress);
				}
			}
			return ZString.Empty;
		}

		public static ZString GetConsigneeData(JobDocAddress jobDocAddress, bool exciseNumber = false)
		{
			if (jobDocAddress != null && jobDocAddress.IsValidAddress)
			{
				if (exciseNumber)
				{
					return GetTraderExciseNumber(jobDocAddress);
				}
				else
				{
					return new ZStringBuilder(GetNameAndAddress(jobDocAddress))
						.AppendIfNotEmpty(GetTraderExciseNumber(jobDocAddress))
						.ToStringWithNewLineBetweenAppends();
				}
			}
			return ZString.Empty;
		}

		public static ZString GetJobDocAddressData(JobDocAddress jobDocAddress, bool eori = false)
		{
			if (jobDocAddress != null && jobDocAddress.IsValidAddress)
			{
				if (eori)
				{
					return jobDocAddress.GetEuIdentificationNumber();
				}
				else
				{
					return GetNameAndAddress(jobDocAddress);
				}
			}
			return ZString.Empty;
		}

		public static ZString GetCustomsOfficeData(EMCSJobDeclaration declaration, string code)
		{
			var office = declaration.CustomsOffices.Cast<OfficeCode>().FirstOrDefault(x => x.CY_Code == code);
			if (office != null)
			{
				return new ZStringBuilder(office.CY_Data).AppendIfNotEmpty(office.CY_OfficeAddress).ToStringWithNewLineBetweenAppends();
			}
			return ZString.Empty;
		}

		public static ZString GetOtherTransportDetails(EMCSJobDeclaration declaration)
		{
			var details = new ZStringBuilder();
			foreach (var container in declaration.CusContainers)
			{
				var detail = new ZStringBuilder()
					.AppendIfNotEmpty(container.CO_ContainerNumber)
					.AppendIfNotEmpty(container.ZG_UnitCode)
					.AppendIfNotEmpty(container.CO_Seal)
					.AppendIfNotEmpty(container.SealDetails)
					.ToStringWithDelimiterBetweenAppends(Delimiter);
				_ = details.AppendIfNotEmpty(detail);
			}
			return details.ToStringWithNewLineBetweenAppends();
		}

		public static ZString GetPackagesData(EMCSJobDeclaration declaration, int lineNumber)
		{
			if (GetInvoiceLine(declaration, lineNumber, out var line))
			{
				return GetPackagesData(line);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static ZString GetPackagesData(EMCSJobComInvoiceLine line)
		{
			var result = new ZStringBuilder();
			foreach (var pivot in line.EMCSPackagePivots.Cast<NonPersistentPackagePivot>().Where(p => p.IsForInvoiceLine))
			{
				var package = pivot.Package;
				var numberOfPackages = ((!line.ZG_IsMainPack) ?
					new long?(0L)
					:
					(package.IsCountable() ? new long?(package.B5_UnitCount) : null)
					).ToString();

				var data = new ZStringBuilder(line.JI_NDescription)
					.AppendIfNotEmpty(numberOfPackages)
					.AppendIfNotEmpty(package.B5_UnitType)
					.AppendIfNotEmpty(package.IsCountable() ? package.B5_MarksAndNumbers : ((ZString)null));

				if (line.ZG_AlcoholicStrength > 0)
				{
					_ = data.Append(line.ZG_AlcoholicStrength.ToString());
				}

				if (line.ZG_Density > 0)
				{
					_ = data.Append(line.ZG_Density.ToString());
				}
				_ = result.Append(data.ToStringWithDelimiterBetweenAppends(Delimiter));
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public static ZString GetItemCommodityCode(EMCSJobDeclaration declaration, int lineNumber)
		{
			return GetInvoiceLine(declaration, lineNumber, out var line) ? line.JI_Tariff : ZString.Empty;
		}

		public static ZString GetItemCustomsQuantityAndUnity(EMCSJobDeclaration declaration, int lineNumber)
		{
			if (GetInvoiceLine(declaration, lineNumber, out var line))
			{
				return GetItemCustomsQuantityAndUnity(line);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static ZString GetItemCustomsQuantityAndUnity(EMCSJobComInvoiceLine line)
		{
			var result = new ZStringBuilder();
			result.Append(line.JI_CustomsQuantity.ToString()).AppendIfNotEmpty(line.CustomsUnitQtyDescription);

			return result.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		public static ZString GetItemMass(EMCSJobDeclaration declaration, int lineNumber, bool netMass = false)
		{
			if (GetInvoiceLine(declaration, lineNumber, out var line))
			{
				return GetItemMass(line, netMass);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static ZString GetItemMass(EMCSJobComInvoiceLine line, bool netMass)
		{
			var result = new ZStringBuilder();
			_ = result.Append(netMass ? line.JI_NetWeight.ToString() : line.JI_Weight.ToString())
				.Append(netMass ? line.JI_NetWeightUQ : line.JI_WeightUQ);
			return result.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		public static ZString GetItemProducedInUK(EMCSJobDeclaration declaration, int lineNumber)
		{
			if (GetInvoiceLine(declaration, lineNumber, out var line))
			{
				return new ZBool(line.ZG_Origin.Trim().ToUpper() == Core.Constants.CountryCodes.UnitedKingdom).ToYesNoString();
			}

			return ZString.Empty;
		}

		public static ZString GetAdditionalInformation(EMCSJobDeclaration declaration)
		{
			var result = ZString.Empty;
			var lines = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Take(3);
			if (lines.Any(l => l.ZG_SizeOfProducer > 0))
			{
				var size = lines.First(l => l.ZG_SizeOfProducer > 0).ZG_SizeOfProducer;
				result = string.Format(AdditionalInformationMessage, size);
			}
			return result;
		}

		public static ZString GetSignatoryCompanyAndPhone()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			return new ZStringBuilder().AppendIfNotEmpty(orgProxy?.OH_FullName)
				.AppendIfNotEmpty(orgProxy?.MainAddress?.PhoneNumber?.ToString())
				.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		public static ZString GetSignatoryName(EMCSJobDeclaration declaration)
		{
			var code = declaration.JE_GS_NKCusAgent;
			if (string.IsNullOrEmpty(code))
			{
				return GlbStaff.CurrentUser.GS_FullName;
			}
			else
			{
				var records = declaration.Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, code));
				return records.Length > 0 ? records[0].GS_FullName : ZString.Empty;
			}
		}

		public static ZString GetSignatoryPlaceAndDate()
		{
			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			return new ZStringBuilder().AppendIfNotEmpty(orgProxy?.MainAddress.City)
				.Append(ZDateTime.Now.ToString(DateFormat))
				.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		public static ZString GetImportSADNumbers(EMCSJobDeclaration declaration)
		{
			var result = new ZStringBuilder();
			declaration.ImportSADNumbers.Cast<ImportSADNumber>().Select(sn => sn.CSI_Description).ForEach(sn => result.AppendIfNotEmpty(sn));
			return result.ToStringWithDelimiterBetweenAppends(",");
		}

		public static ZString GetGuarantor(EMCSJobDeclaration declaration)
		{
			return (string)declaration.ZG_GuarantorType switch
			{
				EMCSGuarantorTypeList.Codes.Consignor => GetConsignorData(declaration.SupplierDocumentaryAddress),
				EMCSGuarantorTypeList.Codes.Transporter => GetJobDocAddressData(declaration.TransporterDocumentaryAddress),
				EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts => GetJobDocAddressData(declaration.OwnerDocumentaryAddress),
				EMCSGuarantorTypeList.Codes.Consignee => GetConsigneeData(declaration.ImporterDocumentaryAddress),
				_ => ZString.Empty,
			};
		}

		public static ZString GetRepresentative(EMCSJobDeclaration declaration)
		{
			var orgProxy = declaration.Branch.OrgProxy;
			return new ZStringBuilder().AppendIfNotEmpty(orgProxy?.OH_FullName)
				.AppendIfNotEmpty(orgProxy?.MainAddress?.Address1AndAddress2)
				.AppendIfNotEmpty(orgProxy?.MainAddress?.Postcode)
				.AppendIfNotEmpty(orgProxy?.MainAddress?.City)
				.AppendIfNotEmpty(orgProxy?.MainAddress?.Country?.Description)
				.ToStringWithNewLineBetweenAppends();
		}

		static ZString GetTraderExciseNumber(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return jobDocAddress.E2_GovRegNumType == OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber ? jobDocAddress.E2_GovRegNum.ToString() : string.Empty;
				}
				else
				{
					return jobDocAddress.Organisation.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
				}
			}
			return ZString.Empty;
		}

		static ZString GetNameAndAddress(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					return new ZStringBuilder(jobDocAddress.E2_CompanyName)
						.AppendIfNotEmpty(jobDocAddress.E2_Address1 + jobDocAddress.E2_Address2)
						.AppendIfNotEmpty(jobDocAddress.E2_Postcode)
						.AppendIfNotEmpty(jobDocAddress.E2_City)
						.ToStringWithNewLineBetweenAppends();
				}
				else
				{
					var address = jobDocAddress.Address;
					return new ZStringBuilder(jobDocAddress.Organisation.OH_FullName)
						.AppendIfNotEmpty(address.OA_Address1 + address.OA_Address2)
						.AppendIfNotEmpty(address.OA_PostCode)
						.AppendIfNotEmpty(address.OA_City)
						.ToStringWithNewLineBetweenAppends();
				}
			}
			return ZString.Empty;
		}

		static bool GetInvoiceLine(EMCSJobDeclaration declaration, int lineNumber, out EMCSJobComInvoiceLine line)
		{
			line = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().FirstOrDefault(il => il.JI_LineNo == lineNumber);
			return line != null;
		}

		public const string Delimiter = " | ";
		public const string DateFormat = "yyyy-MM-dd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		public const string AdditionalInformationMessage = "It is hereby certified that the beer described has been produced by an independent small brewery with a production in the previous year of {0} hectolitres.";
	}
}
