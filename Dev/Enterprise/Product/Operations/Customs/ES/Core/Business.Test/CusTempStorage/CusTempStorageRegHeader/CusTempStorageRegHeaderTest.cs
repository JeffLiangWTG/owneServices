using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegHeader))]
	class CusTempStorageRegHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusTempStorageRegHeaderLookups>(header.Lookups);
		}

		public void TestCusTempStorageRegLines()
		{
			CombineAssertions(() =>
			{
				AssertType<CusTempStorageRegLineCollection>("Type", header.CusTempStorageRegLines);
				AssertEquals("Lines are read-only", true, header.CusTempStorageRegLines.ReadOnly);

				var line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LineNumber = 1;
				AssertEquals("Transactions are not editable when line Status not OPN and no transaction type OBL", true, line.CusTempStorageRegLineTransactions.ReadOnly);

				header.CusTempStorageRegLines.DeleteAll();
				line = header.CusTempStorageRegLines.AddNew();
				line.SRL_LineNumber = 1;
				line.SRL_CustomsStatus = TempStorageDeclarationStatusList.Codes.Open;
				AssertEquals("Transactions are not editable when no transaction type OBL", true, line.CusTempStorageRegLineTransactions.ReadOnly);

				header.CusTempStorageRegLines.DeleteAll();
				line = Factory.New<CusTempStorageRegLine>();
				line.SRL_LineNumber = 1;
				line.SRL_CustomsStatus = TempStorageDeclarationStatusList.Codes.Open;
				var transaction = line.CusTempStorageRegLineTransactions.AddNew();
				transaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
				header.CusTempStorageRegLines.Add(line);
				AssertEquals("Transactions are editable when line Status is OPN and has transaction type OBL", false, line.CusTempStorageRegLineTransactions.ReadOnly);
			});
		}

		public void TestStorageRegLineType()
		{
			AssertEquals(typeof(CusTempStorageRegLine), header.GetStorageRegLineType());
		}

		public void TestSRH_Reference()
		{
			AssertEquals("ReadOnly", true, header.SRH_ReferenceInfo.ReadOnly);
		}

		public void TestSRH_InternalReference()
		{
			AssertEquals("ReadOnly", true, header.SRH_InternalReferenceInfo.ReadOnly);
		}

		public void TestSRH_ArrivalDate()
		{
			AssertEquals("ReadOnly", true, header.SRH_ArrivalDateInfo.ReadOnly);
		}

		public void TestSRH_PresentationDate()
		{
			AssertEquals("ReadOnly", true, header.SRH_PresentationDateInfo.ReadOnly);
		}

		public void TestSRH_PreviousReferenceType()
		{
			AssertEquals("ReadOnly", true, header.SRH_PreviousReferenceTypeInfo.ReadOnly);
		}

		public void TestSRH_PreviousReference()
		{
			AssertEquals("ReadOnly", true, header.SRH_PreviousReferenceInfo.ReadOnly);
		}

		public void TestSRH_Status()
		{
			AssertEquals("ReadOnly", true, header.SRH_StatusInfo.ReadOnly);
		}

		public void TestTSDStatusUrl() => CombineAssertions(() =>
		{
			using (ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://example.org?fRecinto=%recinto%&fAnio=%anio%&fNumero=%numero%&fMrn=%mrn%"))
			{
				AssertURL("", "http://example.org?fRecinto=&fAnio=&fNumero=&fMrn=");
				AssertURL("1234", "http://example.org?fRecinto=1234&fAnio=&fNumero=&fMrn=");
				AssertURL("12345", "http://example.org?fRecinto=1234&fAnio=5&fNumero=&fMrn=");
				AssertURL("123456", "http://example.org?fRecinto=1234&fAnio=5&fNumero=6&fMrn=");
				AssertURL("12345678901234567", "http://example.org?fRecinto=1234&fAnio=5&fNumero=678901234567&fMrn=");
				AssertURL("123456789012345678", "http://example.org?fRecinto=&fAnio=&fNumero=&fMrn=123456789012345678");
				AssertURL("1234567890123456789", "http://example.org?fRecinto=&fAnio=&fNumero=&fMrn=1234567890123456789");
			}

			void AssertURL(ZString srhReference, ZString expectedURL)
			{
				header.SRH_Reference = srhReference;
				AssertEquals($@"SRH_Reference=""{srhReference}""", expectedURL, header.TSDStatusURL);
			}
		});

		public void TestDelete()
		{
			var header = Factory.New<CusTempStorageRegHeader>();
			var regLine = header.CusTempStorageRegLines.AddNew();

			header.Delete();
			CombineAssertions(() =>
			{
				Assert("RegHeader is delete", header.IsDeleted);
				Assert("RegLine is delete", regLine.IsDeleted);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
			header.SRH_Reference = "TEST";
		}
		CusTempStorageRegHeader header;
	}
}
