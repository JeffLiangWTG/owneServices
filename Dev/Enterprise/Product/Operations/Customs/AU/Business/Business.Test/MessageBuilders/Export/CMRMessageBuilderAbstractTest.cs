using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRMessageBuilderAbstractTest : TestCaseWithFactory
	{
		public void TestCheckCertificatesIfNeededWithNoDetails()
		{
			SetCertificatesToBlank();
			var builder = GetMessageBuilderToTest();
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			var sender = new SendsMessagesToCustomsShutterUpperer();
			builder.MessageInitiator = sender;
			builder.Messages = new EDIMessageCollection(Factory.New<DummyBusinessObject>());
			builder.CheckCertificatesIfNeeded();
			AssertEquals("Warning",
				@"There are one or more problems with the cryptographic registry items:

There is no Customs Encryption Certificate.
There is no Trust Point Certificate.
There is no Company Private Key File.
", sender.Warning);

			sender = new SendsMessagesToCustomsShutterUpperer();
			builder.MessageInitiator = sender;
			builder.CheckCertificatesIfNeeded();
			AssertNotNull("UserWarnedAgain", sender.Warning);
		}

		public void TestIsWithdrawal()
		{
			var builder = GetMessageBuilderToTest();
			AssertEquals("IsWithdrawal", false, builder.IsWithdrawal);

			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			AssertEquals("IsWithdrawal", true, builder.IsWithdrawal);
		}

		protected abstract CMRMessageBuilder GetMessageBuilderToTest();

		protected void SetCertificatesToBlank()
		{
			Env.Registry.AUCCompanyCertificateData = System.Array.Empty<byte>();

			var certificatesHelper = ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory);
			certificatesHelper.RemoveCertificates();
		}

		protected void SetCTOCode(JobDeclaration declaration, ZString cTOCode)
		{
			var cTOAddress = declaration.Factory.New<OrgAddress>();
			declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			var cTOHeader = declaration.Factory.New<OrgHeader>();
			cTOAddress.OA_OH = cTOHeader.PK;
			cTOHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, cTOCode);
		}

		protected void SetDepotCode(JobDeclaration declaration, ZString code)
		{
			var address = declaration.Factory.New<OrgAddress>();
			var header = declaration.Factory.New<OrgHeader>();
			address.OA_OH = header.PK;
			declaration.DepotDocAddress.E2_OA_Address = address.PK;
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, code);
		}

		protected void SetWarehouseCode(JobDeclaration declaration, ZString code)
		{
			var address = declaration.Factory.New<OrgAddress>();
			var header = declaration.Factory.New<OrgHeader>();
			address.OA_OH = header.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = address.PK;
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, code);
		}

		protected void SetOwnerCodeAndName(JobDeclaration declaration, ZString code, ZString name)
		{
			var header = declaration.Factory.New<OrgHeader>();
			declaration.JE_OH_Supplier = header.PK;
			header.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, code);
			header.OH_FullName = name;
		}

		protected void AddInvoiceLine(JobDeclaration declaration, ZString tariff, ZDecimal quantity, ZString quantityUnit, ZString description)
		{
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader header;
			if (groupHeader.JobComInvoiceHeaders.Count > 0)
			{
				header = groupHeader.JobComInvoiceHeaders[0];
			}
			else
			{
				header = groupHeader.JobComInvoiceHeaders.AddNew();
			}

			var newLine = header.JobComInvoiceLines.AddNew();
			newLine.JI_Tariff = tariff;
			newLine.JI_CustomsQuantity = quantity;
			newLine.JI_CustomsUnitQty = quantityUnit;
			newLine.JI_Description = description;
		}
	}
}
