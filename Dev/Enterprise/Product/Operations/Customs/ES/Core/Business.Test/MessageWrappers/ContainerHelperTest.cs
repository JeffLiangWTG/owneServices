using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ContainerHelperTest : TestCaseWithFactory
	{
		public void TestGetFirstESContainerPackagingType()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ContainersPivot.RemoveAndDeleteAll();
				AssertEquals("Expected empty GetFirstESContainerPackagingType when there are no containers", ZString.Empty, ContainerHelper.GetFirstESContainerPackagingType(entryLine));

				var containerPivot = invoiceLine.ContainersPivot.AddNew();
				AssertEquals("Expected GetFirstESContainerPackagingType when there are containers", BusinessQuantityUnit.Container, ContainerHelper.GetFirstESContainerPackagingType(entryLine));

				var container = declaration.CusContainers.AddNew();
				containerPivot.C2_CO = container.PK;

				var containerType = Factory.New<RefContainer>();
				containerType.RC_Code = "TEST";
				container.CO_RC = containerType.PK;
				AssertEquals("Expected GetFirstESContainerPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, ContainerHelper.GetFirstESContainerPackagingType(entryLine));

				var codeMap = containerType.CodeMapCollection.AddNew();
				codeMap.RCM_RN_NKCountry = CountryCodes.Spain;
				codeMap.RCM_Code = "7";
				AssertEquals("Expected GetFirstESContainerPackagingType when container type has ES Code", "7", ContainerHelper.GetFirstESContainerPackagingType(entryLine));

				codeMap.RCM_RN_NKCountry = CountryCodes.Eritrea;
				AssertEquals("Expected GetFirstESContainerPackagingType when container type hasn't ES Code", BusinessQuantityUnit.Container, ContainerHelper.GetFirstESContainerPackagingType(entryLine));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);
			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
		}
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
		CusEntryLine entryLine;
	}
}
