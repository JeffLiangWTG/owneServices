using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUAExportExternalPackagesInfoWrapperTest : ExternalPackagesInfoCommonWrapperTest
	{
		public override void TestPackageType()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ContainersPivot.RemoveAndDeleteAll();
				AssertEquals("Expected empty ExternalPackagingType when there are no containers", ZString.Empty, wrapper.PackageType);

				var containerPivot = invoiceLine.ContainersPivot.AddNew();
				AssertEquals("Expected filled ExternalPackagingType when there are containers", BusinessQuantityUnit.Container, wrapper.PackageType);

				var container = declaration.CusContainers.AddNew();
				containerPivot.C2_CO = container.PK;

				var containerType = Factory.New<RefContainer>();
				containerType.RC_Code = "TEST";
				container.CO_RC = containerType.PK;
				AssertEquals("Expected filled ExternalPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, wrapper.PackageType);

				var codeMap = containerType.CodeMapCollection.AddNew();
				codeMap.RCM_RN_NKCountry = CountryCodes.Spain;
				codeMap.RCM_Code = "7";
				AssertEquals("Expected filled ExternalPackagingType when container type has ES Code", "7", wrapper.PackageType);

				codeMap.RCM_RN_NKCountry = CountryCodes.Eritrea;
				AssertEquals("Expected filled ExternalPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, wrapper.PackageType);
			});
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			wrapper = new DUAExportExternalPackagesInfoWrapper(entryLine);
		}
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		CusEntryLine entryLine;
		ExternalPackagesInfoCommonWrapper wrapper;
	}
}
