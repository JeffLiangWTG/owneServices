using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AccComplianceReportData),
	Enterprise.Core.Constants.DocManagerCodes.ComplianceReport)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;
	using WTG.ProductionRules.Core;

	public class AccComplianceReportData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccComplianceReport); } }

		protected override Type CollectionType
		{
			get { return typeof(AccComplianceReportCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccComplianceReportCollection(factory);
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccComplianceReport; } }

		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("fcde8bff-0ff6-4d1c-8784-5b1c1090836b", "Compliance Report"); } }

		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new ComplianceReportEDocsViaUniversalXmlSupport();

		class ComplianceReportEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
		{
			public ZString ExampleCodeFormat => new ZString("SAT|2023-02-02");

			public ZString ExpectedCodeFormat => new ZString("ReportTypeCode|ReportDateFrom");

			public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
			{
				Argument.NotNull(factory, nameof(factory));
				Argument.NotNullOrWhitespace(code, nameof(code));

				var codeParts = code.Split('|');
				if (codeParts.Length == 2)
				{
					var filter = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, Env.CurrentCompany.PK);
					filter.AddToFilter(AccComplianceReportSchema.ACR_ReportType, codeParts[0]);
					var date = new ZDate(codeParts[1]);
					filter.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, date);
					return factory.LoadTop1<AccComplianceReport>(filter);
				}
				return null;
			}
		}
	}
}
