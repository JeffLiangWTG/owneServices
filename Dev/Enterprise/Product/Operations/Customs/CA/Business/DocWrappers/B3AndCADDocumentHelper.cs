using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class B3AndCADDocumentHelper
	{
		public ZString ConvertDutyAndTaxType(ZString type, JobComInvoiceLine invoiceLine)
		{
			var result = CADDutyTaxFeeTypeCodes.ConvertDutyAndTaxType(type);
			if (result == DutyAndTaxTypes.Codes.CTA)
			{
				var commodityType = LookupsHelper.GetCasualImportCommodityType(invoiceLine.Factory, invoiceLine.CA_CasualImportCommodity);
				switch (commodityType)
				{
					case CasualImportConstants.CasualImpCommodityType.Alcohol:
						result = CADDutyTaxFeeTypeCodes.Codes.TAC;
						break;
					case CasualImportConstants.CasualImpCommodityType.Tobacco:
						result = invoiceLine.CA_CasualImportCommodity.EqualsIgnoringCase(Cannabis) ? CADDutyTaxFeeTypeCodes.Codes.PAT : CADDutyTaxFeeTypeCodes.Codes.AAD;
						break;
					default:
						break;
				}
			}
			return result;
		}
		const string Cannabis = "Cannabis";

		public string GetCusAgentNameAndPhone(BusinessObject jobDeclaration)
		{
			if (jobDeclaration is JobDeclaration declaration)
			{
				var broker = GetBroker(declaration);
				if (broker != null)
				{
					var builder = new ZStringBuilder(broker.GS_FullName);
					var phone = broker.GS_PublishWorkPhone && !broker.GS_WorkPhone.IsEmpty ? broker.GS_WorkPhone
												: broker.HomeBranch != null ? broker.HomeBranch.GB_Phone : ZString.Empty;
					builder.AppendIfNotEmpty(phone);
					return builder.ToStringWithDelimiterBetweenAppends(Delimiter);
				}
			}
			return null;
		}

		public GlbStaff GetBroker(JobDeclaration declaration)
		{
			return declaration.DeclarantOnEntryDocsBrokerOnB3 ?? declaration.CusAgent;
		}

		public string GetCurrentUserNameAndPhone()
		{
			var currentUser = GlbStaff.CurrentUser;
			var currentBranch = GlbBranch.CurrentBranch;
			var currentCompany = GlbCompany.CurrentCompany;

			var builder = new ZStringBuilder(currentUser != null ? currentUser.GS_FullName.ToString() : Core.Constants.ProductName);
			var phone = currentUser != null && currentUser.GS_PublishWorkPhone && !currentUser.GS_WorkPhone.IsEmpty ? currentUser.GS_WorkPhone
										: currentBranch != null && !currentBranch.GB_Phone.IsEmpty ? currentBranch.GB_Phone
												: currentCompany != null ? currentCompany.GC_Phone : ZString.Empty;
			builder.AppendIfNotEmpty(phone);
			return builder.ToStringWithDelimiterBetweenAppends(Delimiter);
		}

		public ZString GetPostalAddressAsASingleLine(BusinessObjectFactory factory, IDocAddress address)
		{
			var result = ZString.Empty;
			if (address is OrgAddress orgAddress)
			{
				result = new AddressFormatter(factory, orgAddress, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLine();
			}
			else if (address is JobDocAddress docAddress)
			{
				result = new AddressFormatter(factory, docAddress, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLine();
			}
			return result;
		}

		public ZString GetPostalAddressAsASingleLine(BusinessObjectFactory factory, ZString name, ZString address1, ZString city, ZString state, ZString postCode, ZString countryName)
		{
			return new AddressFormatter(factory, name, address1, ZString.Empty, city, state, postCode, (NoResString)countryName, false).PostalAddressAsASingleLine();
		}

		public ZString GetDescriptionFromIClassificationLine1(IClassificationLine1 line)
		{
			var builder = new ZStringBuilder();
			if (line != null)
			{
				if (!line.TRSNumber.IsEmpty)
				{
					builder.Append(Res.GetString("b66cf8bc-ee2c-4f8c-9b1b-ecefdf6e50da", "TRS #:{0}", line.TRSNumber));
				}

				foreach (var partNumberDescription in line.PartNumberDescriptions)
				{
					builder.AppendIfNotEmpty(partNumberDescription);
				}
			}
			return ((ZString)builder.ToStringWithDelimiterBetweenAppends(";")).Left(DescriptionBoxLength);
		}

		const string Delimiter = ", ";
		const int DescriptionBoxLength = 200;
	}
}
