using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	public abstract class ImportExportAwareSupportingInfoTest<T> : Customs.Business.Testing.CusSupportingInfoTest<T> where T : ImportExportAwareSupportingInfo
	{
		public void TestParentTypeFlags()
		{
			var supportingInfo = Factory.New<T>();
			supportingInfo.CSI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			Assert("Parent is InvoiceHeader", supportingInfo.ParentIsJobComInvoiceHeader);
			Assert("Parent is InvoiceHeader", supportingInfo.IsParentInvoiceOrLineOrCusEntry);
			supportingInfo.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			Assert("Parent is InvoiceLine", supportingInfo.ParentIsJobComInvoiceLine);
			Assert("Parent is InvoiceLine", supportingInfo.IsParentInvoiceOrLineOrCusEntry);
			supportingInfo.CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
			Assert("Parent is CusEntryInstruction", supportingInfo.IsParentInvoiceOrLineOrCusEntry);
			supportingInfo.CSI_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			Assert("Parent is OrgHeader", !supportingInfo.IsParentInvoiceOrLineOrCusEntry);
		}

		public void TestIsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var supportingInfo = Factory.New<T>();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(TransitionPeriodConfig.Code, TransitionPeriodConfig.DataGroupingCode, TransitionPeriodConfig.EffectiveDate, true))
			{
				AssertEquals("Precondition: IsTransitionPeriodAES30 is true", true, declaration.IsTransitionPeriodAES30);

				supportingInfo.CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = declaration.PK;
				AssertEquals("JE", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = invoice.PK;
				AssertEquals("JZ", true, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = invLine.PK;
				AssertEquals("JI", true, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = instruction.PK;
				AssertEquals("CEI", true, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentID = ZGuid.Empty;
				AssertEquals("Declaration is null", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(TransitionPeriodConfig.Code, TransitionPeriodConfig.DataGroupingCode, TransitionPeriodConfig.EffectiveDate, false))
			{
				AssertEquals("Precondition: IsTransitionPeriodAES30 is false", false, declaration.IsTransitionPeriodAES30);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = declaration.PK;
				AssertEquals("JE", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = invoice.PK;
				AssertEquals("JZ", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = invLine.PK;
				AssertEquals("JI", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentTableCode = CusEntryInstructionSchema.Constants.Prefix;
				supportingInfo.CSI_ParentID = instruction.PK;
				AssertEquals("CEI", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);

				supportingInfo = Factory.New<T>();
				supportingInfo.CSI_ParentID = ZGuid.Empty;
				AssertEquals("Declaration is null", false, supportingInfo.IsParentInvoiceOrLineOrCusEntryAndIsTransitionPeriod);
			}
		}

		protected virtual (ZString Code, ZString DataGroupingCode, ZDateTime EffectiveDate) TransitionPeriodConfig => (Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today);

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var additionalInfo = Factory.New<T>();
			AssertNull("Stand alone AdditionalInfo", additionalInfo.Declaration);

			additionalInfo.CSI_ParentTableCode = declaration.TablePrefix;
			additionalInfo.CSI_ParentID = declaration.PK;
			AssertSame("Declaration AdditionalInfo", declaration, additionalInfo.Declaration);

			additionalInfo.CSI_ParentTableCode = invoice.TablePrefix;
			additionalInfo.CSI_ParentID = invoice.PK;
			AssertSame("Invoice Header AdditionalInfo", declaration, additionalInfo.Declaration);

			additionalInfo.CSI_ParentTableCode = invoiceLine.TablePrefix;
			additionalInfo.CSI_ParentID = invoiceLine.PK;
			AssertSame("Invoice line AdditionalInfo", declaration, additionalInfo.Declaration);

			additionalInfo.CSI_ParentTableCode = instruction.TablePrefix;
			additionalInfo.CSI_ParentID = instruction.PK;
			AssertSame("Instruction AdditionalInfo", declaration, additionalInfo.Declaration);
		}
	}
}
