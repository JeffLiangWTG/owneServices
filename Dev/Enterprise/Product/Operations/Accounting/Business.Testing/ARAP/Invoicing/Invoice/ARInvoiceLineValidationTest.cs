using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoiceLine))]
	public class ARInvoiceLineValidationTest : InvoicingLineValidationTest
	{
		public override void TestCheckAL_JHErrorMessage()
		{
			Assert(true);
		}

		public override void TestCheckAL_JHWithUXml_JobErrorMessages()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.FRT.PK;

			line.Validation.ValidateAL_JH();
			Assert(!line.AL_JHInfo.HasErrors());

			line.AL_JH = ZGuid.Invalid;
			line.Validation.ValidateAL_JH();
			Assert(!line.AL_JHInfo.HasErrors());
		}

		public override void TestCheckAL_JHForSelectedGLTypeCharge()
		{
			ForwardingShipment testShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;
			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			ZQuery testFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, "P&L");
			testFilter.AddToFilter(ViewGenericChargeSchema.VC_DisallowDirectPosting, false);
			GenericCharge.GenericCharge testGenericCharge = Factory.LoadTop1(typeof(GenericCharge.GenericCharge), testFilter) as GenericCharge.GenericCharge;
			line.GenericCharge = testGenericCharge.PK;
			((ARInvoiceLineValidation)line.Validation).CheckAL_JH_ForTestOnly();

			Assert(!line.AL_JHInfo.HasErrors());

			line.AL_JH = testShipment.PK;
			((ARInvoiceLineValidation)line.Validation).CheckAL_JH_ForTestOnly();

			Assert(!line.AL_JHInfo.HasErrors());
		}

		public void TestValidateAL_OverseasTotal_WithSourceReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Assert("PreCondition", invoice.IsSourceReferenceEnabled);
				Assert(!invoice.IsSourceReferenceUsed);

				invoice.Lines.AddNew();
				var line = invoice.Lines[0];

				line.AL_OSExTaxAmount = 9.11M;
				line.AL_LocalTaxAmount = 1.73M;
				line.AL_OverseasTotal = 9.11M;

				((ARInvoiceLineValidation)line.Validation).ValidateAL_OverseasTotal();
				AssertHasError(line.AL_OverseasTotalInfo, "Overseas Total does not equal Overseas Amount + Overseas Tax.");

				invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				Assert(invoice.IsSourceReferenceUsed);

				((ARInvoiceLineValidation)line.Validation).ValidateAL_OverseasTotal();
				AssertNoError(line.AL_OverseasTotalInfo, "Overseas Total does not equal Overseas Amount + Overseas Tax.");
				AssertHasWarning(line.AL_OverseasTotalInfo, "Overseas Total does not equal Overseas Amount + Overseas Tax.");
			}
		}

		public override void TestCheckAL_JHForSelectedCharge()
		{
			ForwardingShipment testShipment = Factory.NewWithValidTestData(typeof(ForwardingShipment)) as ForwardingShipment;

			ZQuery testFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, "MRG");
			AccChargeCode chargeCode = Factory.LoadTop1(typeof(AccChargeCode), testFilter) as AccChargeCode;
			chargeCode.AC_DepartmentFilterList = "ALL";

			Factory.Save();

			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.GenericCharge = chargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).CheckAL_JH_ForTestOnly();

			Assert(!line.AL_JHInfo.HasError("You must select a job for this charge code."));

			line.AL_JH = testShipment.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateAL_JH();

			Assert(!line.AL_JHInfo.HasError("You must select a job for this charge code."));
		}

		public void TestCheckGenericCharge_ChargeCode()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode eNettRegisteredChargeCode = TestObjectCreator.CreateChargeCode("ENETT", "ENETT", Enterprise.Core.Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
			AccChargeCode notRegisteredChargeCode = TestObjectCreator.CreateChargeCode("NONREG", "NONREG", Enterprise.Core.Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
			OrgHeader eNettRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
			OrgHeader notRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			AccGLHeader glheader = Factory.LoadTop1<AccGLHeader>(new ZQuery());
			OrgPatternMatchOverride ov = TestObjectCreator.AALSHI.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = eNettRegisteredChargeCode.AC_Code;
			EnettRegistrationCode regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "AAA";
			regCode.AuthenticationCode = "";
			regCode.OrganisationPK = TestObjectCreator.AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																	Guid.Empty,
																																	Guid.Empty,
																																	regCode);
			string warning = eNettHelper.NoEnettMappingError;

			line.AL_AG = ZGuid.Empty;
			invoice.AH_OH = notRegisteredOrg.PK;
			line.AL_AC = eNettRegisteredOrg.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = notRegisteredChargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = ZGuid.Empty;
			line.AL_AG = glheader.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);

			line.AL_AG = ZGuid.Empty;
			invoice.AH_OH = eNettRegisteredOrg.PK;
			line.AL_AC = eNettRegisteredChargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = ZGuid.Empty;
			line.AL_AG = glheader.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = notRegisteredChargeCode.PK;
			line.AL_AG = ZGuid.Empty;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, warning);
			regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "";
			regCode.AuthenticationCode = "";
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																	Guid.Empty,
																																	Guid.Empty,
																																	regCode);
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
		}

		public void TestCheckGenericCharge_GLAccount()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoiceType);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			AccChargeCode eNettRegisteredChargeCode = TestObjectCreator.CreateChargeCode("ENETT", "ENETT", Enterprise.Core.Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
			AccChargeCode notRegisteredChargeCode = TestObjectCreator.CreateChargeCode("NONREG", "NONREG", Enterprise.Core.Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
			OrgHeader eNettRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");
			OrgHeader notRegisteredOrg = Factory.NewWithValidTestData<OrgHeader>();
			AccGLHeader glheader = Factory.LoadTop1<AccGLHeader>(new ZQuery());
			OrgPatternMatchOverride ov = TestObjectCreator.AALSHI.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = eNettRegisteredChargeCode.AC_Code;
			EnettRegistrationCode regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "AAA";
			regCode.AuthenticationCode = "";
			regCode.OrganisationPK = TestObjectCreator.AALSHI.PK;
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																					Guid.Empty,
																																					Guid.Empty,
																																					regCode);
			string warning = "This GL Account doesn't have an eNett mapping.";

			line.AL_AG = ZGuid.Empty;
			invoice.AH_OH = notRegisteredOrg.PK;
			line.AL_AC = eNettRegisteredOrg.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = notRegisteredChargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = ZGuid.Empty;
			line.AL_AG = glheader.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);

			line.AL_AG = ZGuid.Empty;
			invoice.AH_OH = eNettRegisteredOrg.PK;
			line.AL_AC = eNettRegisteredChargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = notRegisteredChargeCode.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
			line.AL_AC = ZGuid.Empty;
			line.AL_AG = glheader.PK;
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertHasWarning(line.GenericChargeInfo, warning);
			regCode = new EnettRegistrationCode();
			regCode.RegistrationCode = "";
			regCode.AuthenticationCode = "";
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
																																	Guid.Empty,
																																	Guid.Empty,
																																	regCode);
			((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
			AssertNoWarning(line.GenericChargeInfo, warning);
		}

		public void TestCheckCommentChargeLineValidation()
		{
			var invoice = (InvoicingBase)Factory.New(InvoiceType);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			var chargeCode = TestObjectCreator.CreateChargeCode("RVN", "Revenue", Constants.ChargeType.Revenue, 0, null, null);
			line.AL_AC = chargeCode.PK;
			var commentLine = (InvoicingLineBase)invoice.Lines.AddNew();
			var commentChargeCode = TestObjectCreator.CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null);
			commentLine.AL_AC = commentChargeCode.PK;

			using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.CommentChargeLineARInvoiceWarningOptions.NoAction))
			{
				((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
				AssertNoWarning(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertNoError(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				((ARInvoiceLineValidation)commentLine.Validation).ValidateGenericCharge();
				AssertNoWarning(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertNoError(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
			}

			using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.CommentChargeLineARInvoiceWarningOptions.WarningValidation))
			{
				((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
				AssertNoWarning(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertNoError(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				((ARInvoiceLineValidation)commentLine.Validation).ValidateGenericCharge();
				AssertHasWarning(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertNoError(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
			}

			using (AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.CommentChargeLineARInvoiceWarningOptions.ErrorValidation))
			{
				((ARInvoiceLineValidation)line.Validation).ValidateGenericCharge();
				AssertNoWarning(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertNoError(line.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				((ARInvoiceLineValidation)commentLine.Validation).ValidateGenericCharge();
				AssertNoWarning(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
				AssertHasError(commentLine.GenericChargeInfo, AccountingConstants.CommentChargeLineValidationMessage);
			}
		}

		protected override Type InvoiceLineType
		{
			get { return typeof(ARInvoiceLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}

		protected override InvoiceLineValidation GetValidation(InvoiceLine parent)
		{
			return new ARInvoiceLineValidation(parent);
		}

		public override void TestCheckAL_ATWhenRegisteredCompanyAndNonRegisteredDebtor()
		{
			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			base.TestCheckAL_ATWhenRegisteredCompanyAndNonRegisteredDebtor();
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(ARInvoice);
		}
	}
}
