using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class ClientLicenceFeeFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport_InvalidInput()
		{
			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);
			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = "FOO";
			var rec4 = flattenedCollection.AddNew();
			rec4.OrgCode = Org1.OH_Code;
			rec4.ServerCode = "XX4";

			AssertImport(flattenedCollection,
@"",

@"Record [Org. Code: FOO, Server Code: ] excluded: Org. Code not found
Record [Org. Code: OG1SYD, Server Code: XX4] excluded: Database not found
");
		}

		public void TestImport_InvalidLicCompany()
		{
			var feeTypes = new CodeDescriptionBoolCollection {
							{ "AAA", (NoResString)"Fee A", false },
							{ "BBB", (NoResString)"Fee B Third Party", true } };
			EDIDataRegistry.Instance.LicenceFeeTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeTypes);

			AccChargeCode good1 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD1");
			good1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			good1.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TEST_ORG_1";
			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);
			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = org.OH_Code;
			rec1.SystemCode = "ODM";
			rec1.FeeType = "AAA";
			rec1.ServerCode = "";
			rec1.StartDate = new ZDateTime(2018, 1, 1);
			rec1.AmountAsText = "100";
			rec1.Currency = "AUD";
			rec1.ChargeCode = "GOOD1";
			rec1.Comment = "Comment 1";
			rec1.RenewalMonths = 1;
			rec1.Description = "Fee 1";
			rec1.IsDiscountable = true;

			AssertImport(flattenedCollection,
@"",

@"Record [Org. Code: TEST_ORG_1, Server Code: ] excluded: Organization does not have a license.
");
		}

		public void TestImport_InvalidTaxDate()
		{
			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);

			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = Org1.OH_Code;
			rec1.ServerCode = Db1.LD_ServerCode;
			rec1.StartDate = new ZDateTime(2017, 1, 1);
			rec1.EndDate = new ZDateTime(2017, 1, 31);
			rec1.ChargeCode = "VOLUME";
			rec1.AmountAsText = "100";
			rec1.Currency = "AUD";
			rec1.IsDiscountable = true;
			rec1.TaxDate = "FOO";

			AssertImport(flattenedCollection,
@"",

@"Record [Org. Code: OG1SYD, Server Code: OG1] excluded: Tax Date code is invalid: FOO
");
		}

		public void TestImport_NewRecords()
		{
			var feeTypes = new CodeDescriptionBoolCollection {
							{ "AAA", (NoResString)"Fee A", false },
							{ "BBB", (NoResString)"Fee B Third Party", true } };
			EDIDataRegistry.Instance.LicenceFeeTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeTypes);

			AccChargeCode good1 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD1");
			good1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			good1.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;
			Factory.Save();

			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);

			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = Org1.OH_Code;
			rec1.SystemCode = "ODM";
			rec1.FeeType = "AAA";
			rec1.ServerCode = "";
			rec1.StartDate = new ZDateTime(2018, 1, 1);
			rec1.AmountAsText = "100";
			rec1.Currency = "AUD";
			rec1.ChargeCode = "GOOD1";
			rec1.Comment = "Comment 1";
			rec1.RenewalMonths = 1;
			rec1.Description = "Fee 1";
			rec1.IsDiscountable = true;
			rec1.Order = 0;

			var rec2 = flattenedCollection.AddNew();
			rec2.OrgCode = Org1.OH_Code;
			rec2.SystemCode = "ODM";
			rec2.FeeType = "AAA";
			rec2.ServerCode = Db1.LD_ServerCode;
			rec2.StartDate = new ZDateTime(2018, 2, 1);
			rec2.AmountAsText = "300";
			rec2.Currency = "NZD";
			rec2.ChargeCode = "GOOD1";
			rec2.Comment = "Comment 2";
			rec2.RenewalMonths = 12;
			rec2.Description = "Fee 2";
			rec2.IsDiscountable = false;
			rec2.TaxDate = BillingConstants.Fee.TaxDateCode.FeeTaxAtStartDate;
			rec2.Order = short.MaxValue;

			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = Org1.OH_Code;
			rec3.SystemCode = "ODM";
			rec3.FeeType = "BBB";
			rec3.ServerCode = Db1.LD_ServerCode;
			rec3.StartDate = new ZDateTime(2018, 3, 1);
			rec3.EndDate = new ZDateTime(2018, 5, 31);
			rec3.AmountAsText = "500";
			rec3.Currency = "USD";
			rec3.ChargeCode = "GOOD1";
			rec3.RenewalMonths = 1;
			rec3.Description = "Fee 3";
			rec3.IsDiscountable = true;
			rec3.TaxDate = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			rec3.Order = short.MinValue;

			var rec4 = flattenedCollection.AddNew();
			rec4.OrgCode = Org1.OH_Code;
			rec4.SystemCode = "ODM";
			rec4.FeeType = "BBB";
			rec4.ServerCode = Db1.LD_ServerCode;
			rec4.StartDate = new ZDateTime(2018, 4, 1);
			rec4.EndDate = new ZDateTime(2018, 6, 30);
			rec4.AmountAsText = "700";
			rec4.Currency = "NZD";
			rec4.ChargeCode = "GOOD1";
			rec4.RenewalMonths = 3;
			rec4.Description = "Fee 4";
			rec4.IsDiscountable = false;
			rec4.TaxDate = BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate;
			rec4.Order = 999;

			AssertImport(flattenedCollection,
@"01-Apr-18 00:00:00 - 30-Jun-18 00:00:00 - ODM - BBB - 700.0000 - NZD - GOOD1 - Fee 4 -  - 999 - N - CUR - OG1
01-Feb-18 00:00:00 -  - ODM - AAA - 300.0000 - NZD - GOOD1 - Fee 2 - Comment 2 - 32767 - N - STA - OG1
01-Jan-18 00:00:00 -  - ODM - AAA - 100.0000 - AUD - GOOD1 - Fee 1 - Comment 1 - 0 - Y - CUR - 
01-Mar-18 00:00:00 - 31-May-18 00:00:00 - ODM - BBB - 500.0000 - USD - GOOD1 - Fee 3 -  - -32768 - Y - END - OG1", null);
		}

		public void TestImport_Validation()
		{
			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);

			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = Org1.OH_Code;
			rec1.ServerCode = Db1.LD_ServerCode;
			rec1.StartDate = new ZDateTime(2017, 1, 1);
			rec1.EndDate = new ZDateTime(2017, 1, 31);
			rec1.ChargeCode = "VOLUME";
			rec1.AmountAsText = "A25";
			rec1.Currency = "AUD";

			AssertImport(flattenedCollection,
@"",

@"Record [Org. Code: OG1SYD, Server Code: OG1] excluded: Error - Amount: A25 is not a valid number
");
		}

		public void TestImport_Validation_NullRef()
		{
			var flattenedCollection = new ClientLicenceFeeFlattenedCollection(Factory);

			var rec1 = flattenedCollection.AddNew();
			rec1.OrgCode = Org1.OH_Code;
			rec1.ServerCode = "";
			rec1.StartDate = new ZDateTime(2017, 1, 1);
			rec1.EndDate = new ZDateTime(2017, 1, 31);
			rec1.ChargeCode = "VOLUME";
			rec1.AmountAsText = "25";
			rec1.Currency = "AUD";

			AssertImport(flattenedCollection,
@"",

@"Record [Org. Code: OG1SYD, Server Code: ] excluded: Error - L8_ChargeCode: ChargeCode must be active, of Charge Type NON or REV, and with Charge Group NGC or NJR in at least one company.
Error - L8_Description: Please enter a value.
Error - L8_RenewalMonths: Please enter a value.
Error - L8_RenewalMonths: value cannot be zero.
Error - L8_SystemCode: Please enter a value.
Error - L8_Type: Please enter a value.
");
		}

		protected override void SetUp()
		{
			base.SetUp();

			Org1 = BillingTestHelper.CreateOrganisation(Factory, "OG1");
			Db1 = Org1.LicEnterprise.Databases.OfType<LicenceDatabase>().First();

			Factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		void AssertImport(ClientLicenceFeeFlattenedCollection flattenedCollection, string rowsAsTextExpected, string logExpected)
		{
			var collectionInfo = new ClientLicenceFeeImportInfo(flattenedCollection);
			var collection = new ClientLicenceFeeCollectionNonDependent(Factory);
			var processor = new ClientLicenceFeeFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			Factory.Save();

			var rowsAsText = string.Join("\r\n", new BusinessObjectFactory().Load<ClientLicenceFee>(new ZQuery()).Select(x =>
				$"{x.L8_StartDate} - {x.L8_EndDate} - {x.L8_SystemCode} - {x.L8_Type} - {x.L8_Amount} - {x.L8_RX_NKCurrency} - {x.L8_ChargeCode} - {x.L8_Description} - {x.L8_Comment} - {x.L8_Order} - {x.L8_IsDiscountable} - {x.L8_TaxDateCode} - {x.Database?.LD_ServerCode ?? ""}")
				.OrderBy(x => x));

			CombineAssertions(() =>
			{
				AssertEquals("rowsAsText", rowsAsTextExpected, rowsAsText);
				AssertEquals("log", logExpected, processor.Log);
			});
		}

		EDIOrgHeader Org1;
		LicenceDatabase Db1;
	}
}
