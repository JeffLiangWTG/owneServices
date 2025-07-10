using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	public static class ComplianceReportAdditionalDataCollectorFactory
	{
		public static ComplianceReportAdditionalDataCollector GetComplianceReportAdditionalDataCollector(AccComplianceReport report, ComplianceReportDataCollectionMode mode = ComplianceReportDataCollectionMode.None,
			ZGuid? selectedSupplierPK = null, string selectedSupplierCode = null)
		{
			Argument.NotNull(report, nameof(report));
			switch (mode)
			{
				case ComplianceReportDataCollectionMode.SAFT:
				case ComplianceReportDataCollectionMode.SAFTSelfBilling:
				case ComplianceReportDataCollectionMode.SAFT1_10:
				case ComplianceReportDataCollectionMode.SAFT1_30:
					return new SAFTAdditionalDataCollector(report, mode, selectedSupplierPK, selectedSupplierCode);
				case ComplianceReportDataCollectionMode.JPKV7M:
					return new JPKAdditionalDataCollector(report);
				case ComplianceReportDataCollectionMode.OpenFormatSimplified:
					return new OpenFormatAdditionalDataCollector(report);
				default:
					return new SAFTAdditionalDataCollector(report, mode, selectedSupplierPK, selectedSupplierCode);
			}
		}

#if DEBUG

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		[TypeFactoryAnnotationMethod]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			yield return typeof(SAFTAdditionalDataCollector).FullName + "," + typeof(SAFTAdditionalDataCollector).AssemblyQualifiedName;
			yield return typeof(JPKAdditionalDataCollector).FullName + "," + typeof(SAFTAdditionalDataCollector).AssemblyQualifiedName;
		}

#endif
	}
}
