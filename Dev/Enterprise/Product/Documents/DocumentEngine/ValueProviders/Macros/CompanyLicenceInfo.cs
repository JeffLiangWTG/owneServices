using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CompanyLicenceInfo : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			return new ValueProviderDocumenter("<CompanyLicenceInfo([{PK}]).{Property})>",
				ResString.GetMultilingualString("c8dcc71f-acfa-4be4-b265-37bc5fefcbb7", @"This macro is used to access licensing information for the given company. If you don't specify a PK or the PK is not recognized, will return values from the Current Company you are logged into. The 3 properties you can use are {0}, {1} and {2}.",
				"LicenceServerID", "LicenceEnterpriseCode", "LicenceCompanyCode"),
				new List<(string example, object expectedResult)> {
					("<CompanyLicenceInfo().LicenceServerID>", registrationKey.ServerCode),
					("<CompanyLicenceInfo().LicenceEnterpriseCode>", registrationKey.EnterpriseCode),
					("<CompanyLicenceInfo(<CompanyPK>).LicenceCompanyCode>", GlbCompany.CurrentCompany.GC_Code.ToString()) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = fRegex.Match(macro);
			string propertyName = match.Groups["Property"].Value;
			string guid = match.Groups["Guid"].Value;
			ZGuid companyPK = !string.IsNullOrEmpty(guid) && ZGuid.TryParse(guid, out companyPK) ? companyPK : GlbCompany.CurrentCompany.PK;
			GlbCompany company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, companyPK));
			switch (propertyName)
			{
				case "LicenceServerID":
					return ObjectFactory.Get<IProductRegistration>().Key.ServerCode;
				case "LicenceEnterpriseCode":
					return ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
				case "LicenceCompanyCode":
					return company.GC_Code;
				default:
					return "";
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^\s*<\s*CompanyLicenceInfo\s*\((?<Guid>.*)\)\s*\.\s*(?<Property>LicenceServerID|LicenceEnterpriseCode|LicenceCompanyCode)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
