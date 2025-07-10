using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using Dat.Integration.VersionControl;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportSheetNameAspectTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckinWithReportSheetNameChange()
		{
			using (var codeChangeFile = TempFile.NewWithExtension("cs"))
			{
				File.WriteAllText(codeChangeFile.Filename, CodeFileContent);
				var aspectHandler = new ReportSheetNameAspect();

				var pendingXlsChange = new Mock<IPendingChange>(MockBehavior.Strict);
				pendingXlsChange.Setup(m => m.ServerItem).Returns(@"Dev\Enterprise\Product\Documents\ExcelTemplates\Reports\TestTemplate.xls");
				pendingXlsChange.Setup(m => m.LocalItem).Returns(GetSourcePath("ShelvedReportTemplateForAspect.xlsx"));
				pendingXlsChange.Setup(m => m.ChangeType).Returns(TfsChangeType.Edit);
				pendingXlsChange.Setup(p => p.DownloadBaseFile(It.IsAny<string>()))
					.Callback<string>(mi => File.Copy(GetSourcePath("BaseReportTemplateForAspect.xlsx"), mi, true));

				var pendingCodeChange = new Mock<IPendingChange>(MockBehavior.Strict);
				pendingCodeChange.Setup(m => m.ServerItem).Returns(@"Dev\Enterprise\Architecture\Core\Core\ServiceManager\ServiceManager.Tasks\OnlineDataTransformation\Reports\TestDataTransformation.cs");
				pendingCodeChange.Setup(m => m.LocalItem).Returns(codeChangeFile.Filename);
				pendingCodeChange.Setup(m => m.ChangeType).Returns(TfsChangeType.Add);

				AssertEquals("No data transformation included in changes should receive aspectBit", 1, aspectHandler.GetAspectsForShelf("", "", new IPendingChange[] { pendingXlsChange.Object }).Count());
				AssertEquals("Should receive no aspectBit when data transformation is included in changes", 0, aspectHandler.GetAspectsForShelf("", "", new IPendingChange[] { pendingXlsChange.Object, pendingCodeChange.Object }).Count());
			}
		}

		public string GetSourcePath(string fileName)
		{
			return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles", fileName);
		}

		const string CodeFileContent = @"using System;
namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Reports
{
	public class TestDataTransformation: UpdateReportSheetNameConfigurationBase
	{}
}";
	}
}
