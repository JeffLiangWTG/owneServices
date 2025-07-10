using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class AddInfoCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_SealsCount_PromptValueCannotBeNegativeMessageWhenConfigurationIsEnabled()
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, configurationValue: true))
				{
					entryInstruction.ZG_SealsCount = 1;
					AssertNoNotifications(entryInstruction.ZG_SealsCountInfo);

					entryInstruction.ZG_SealsCount = 0;
					AssertNoNotifications(entryInstruction.ZG_SealsCountInfo);

					entryInstruction.ZG_SealsCount = -1;
					AssertHasMessageError(entryInstruction.ZG_SealsCountInfo, "value cannot be negative.");
				}
			});
		}

		public void TestCheckZG_SealsCount_DoesNotPromptValueCannotBeNegativeMessageWhenConfigurationIsDisabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, configurationValue: false))
			{
				entryInstruction.ZG_SealsCount = -1;
				AssertNoNotifications(entryInstruction.ZG_SealsCountInfo);
			}
		}

		public void TestCheckZG_SealsCount_PromptSealsNumberMismatchMessageWhenConfigurationIsEnabled()
		{
			const string messageError = "Seals Quantity is less than the sum of distinct seals entered in 'Seals' and 'Containers' tabs.";

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, configurationValue: true))
				{
					entryInstruction.Seals.AddNew().CY_Data = "";
					entryInstruction.Seals.AddNew().CY_Data = "F";
					entryInstruction.Seals.AddNew().CY_Data = "F";
					entryInstruction.Seals.AddNew().CY_Data = "D";

					entryInstruction.ZG_SealsCount = 2;
					AssertNoMessageErrorContaining("When ZG_SealsCount is equal to the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);

					entryInstruction.ZG_SealsCount = 1;
					AssertHasMessageErrorContaining("When ZG_SealsCount is less than the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);

					entryInstruction.ZG_SealsCount = 3;
					AssertNoMessageErrorContaining("When ZG_SealsCount is greater than the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);
				}
			});
		}

		public void TestCheckZG_SealsCount_PromptSealsNumberMismatchMessageIncludingContainerSealsWhenConfigurationIsEnabled()
		{
			const string messageError = "Seals Quantity is less than the sum of distinct seals entered in 'Seals' and 'Containers' tabs.";

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, configurationValue: true))
				{
					var container1 = AddNewDeclarationContainerWithSeals("A", "B");
					var container2 = AddNewDeclarationContainerWithSeals("C", "B");
					var container3 = AddNewDeclarationContainerWithSeals("D", "E");
					entryInstruction.Seals.AddNew().CY_Data = "";
					entryInstruction.Seals.AddNew().CY_Data = "F";
					entryInstruction.Seals.AddNew().CY_Data = "D";

					var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
					invoiceLine.JI_CEI = entryInstruction.PK;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container1).IsForInvoiceLine = true;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine = true;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container3).IsForInvoiceLine = true;

					entryInstruction.ZG_SealsCount = 6;
					AssertNoMessageErrorContaining("When ZG_SealsCount is equal to the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);

					entryInstruction.ZG_SealsCount = 5;
					AssertHasMessageErrorContaining("When ZG_SealsCount is less than the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);

					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container2).IsForInvoiceLine = false;
					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container3).IsForInvoiceLine = false;
					entryInstruction.ZG_SealsCount = 5;
					AssertNoMessageErrorContaining("When ZG_SealsCount is greater than the distinct count of entry instruction seals", entryInstruction.ZG_SealsCountInfo, messageError);
				}
			});
		}

		public void TestCheckZG_SealsCount_DoesNotPromptSealsNumberMismatchMessageWhenConfigurationIsDisabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, configurationValue: false))
			{
				entryInstruction.Seals.AddNew().CY_Data = "";
				entryInstruction.Seals.AddNew().CY_Data = "F";
				entryInstruction.Seals.AddNew().CY_Data = "F";
				entryInstruction.Seals.AddNew().CY_Data = "D";

				entryInstruction.ZG_SealsCount = 1;
				AssertNoNotifications(entryInstruction.ZG_SealsCountInfo);
			}
		}

		public void TestCheckZG_SealsCount_DoesNotPromptMessageWhenDeclarationIsNull()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.ZG_SealsCount = -1;
			AssertNoNotifications(entryInstruction.ZG_SealsCountInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;

		CusContainer AddNewDeclarationContainerWithSeals(ZString seal, ZString secondSeal)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_Seal = seal;
			container.CO_SecondSeal = secondSeal;
			return container;
		}
	}
}
