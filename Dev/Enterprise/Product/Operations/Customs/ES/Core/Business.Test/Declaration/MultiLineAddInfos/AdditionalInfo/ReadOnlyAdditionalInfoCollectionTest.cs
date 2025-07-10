using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ReadOnlyAdditionalInfoCollection))]
	class ReadOnlyAdditionalInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReadOnlyAdditionalInfoCollection>
	{
		public void TestReadOnlyAdditionalInfoCollectionWithNoAdditionalInfos()
		{
			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			readOnlyAdditionalInfoCollection.LoadNew();
			AssertEquals("When there are no previous documents, ReadOnlyAdditionalInfoCollection.Count", 0, readOnlyAdditionalInfoCollection.Count);
		}

		public void TestIsLoaded()
		{
			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("Not Loaded", false, readOnlyAdditionalInfoCollection.IsLoaded);
				readOnlyAdditionalInfoCollection.LoadNew();
				AssertEquals("Loaded", true, readOnlyAdditionalInfoCollection.IsLoaded);
			});
		}

		public void TestReadOnlyAdditionalInfoCollectionEntryLines()
		{
			var addInf1 = GetAdditionalInfo("REF111", "TRA");
			addInf1.CSI_ParentID = entryLine.PK;
			addInf1.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf1.CSI_Status = "ACC";
			var addInf2 = GetAdditionalInfo("REF222", "INF");
			addInf2.CSI_ParentID = entryLine.PK;
			addInf2.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf2.CSI_Status = "ACC";
			var addInf3 = GetAdditionalInfo("REF333", "INF");
			addInf3.CSI_ParentID = entryLine.PK;
			addInf3.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf3.CSI_Status = "ACC";

			Factory.Save();

			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			readOnlyAdditionalInfoCollection.LoadNew();
			AssertEquals("ReadOnlyAdditionalInfoCollection.Count", 3, readOnlyAdditionalInfoCollection.Count);

			var readOnlyAdditionalInfoList = readOnlyAdditionalInfoCollection.Cast<ReadOnlyAdditionalInfo>();
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), addInf1);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), addInf2);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), addInf3);
		}

		public void TestReadOnlyAdditionalInfoCollectionInAllLevelsIncludingEntryLines()
		{
			var addInf1 = GetAdditionalInfo("REF111", "TRA");
			declaration.AdditionalInfos.Add(addInf1);
			var addInf2 = GetAdditionalInfo("REF222", "TRA");
			entryInstruction.AdditionalInfos.Add(addInf2);
			var addInf3 = GetAdditionalInfo("REF333", "TRA");
			invoice.AdditionalInfos.Add(addInf3);
			var addInf4 = GetAdditionalInfo("REF444", "INF");
			invoiceLine.AdditionalInfos.Add(addInf4);
			var addInf5 = GetAdditionalInfo("REF555", "INF");
			addInf5.CSI_ParentID = entryLine.PK;
			addInf5.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf5.CSI_Status = "ACC";

			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			readOnlyAdditionalInfoCollection.LoadNew();
			AssertEquals("ReadOnlyAdditionalInfoCollection.Count", 5, readOnlyAdditionalInfoCollection.Count);

			var readOnlyAdditionalInfoList = readOnlyAdditionalInfoCollection.Cast<ReadOnlyAdditionalInfo>();
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), addInf1);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), addInf2);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), addInf3);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), addInf4);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF555"), addInf5);
		}

		public void TestReadOnlyAdditionalInfoCollectionWithDuplicates()
		{
			var addInf1 = GetAdditionalInfo("REF111", "TRA");
			declaration.AdditionalInfos.Add(addInf1);
			var addInf2 = GetAdditionalInfo("REF222", "INF");
			entryInstruction.AdditionalInfos.Add(addInf2);
			var addInf3 = GetAdditionalInfo("REF333", "INF");
			invoice.AdditionalInfos.Add(addInf3);
			var addInf4 = GetAdditionalInfo("REF444", "TRA");
			invoiceLine.AdditionalInfos.Add(addInf4);
			var addInf5 = GetAdditionalInfo("REF444", "TRA");
			addInf5.CSI_ParentID = entryLine.PK;
			addInf5.CSI_ParentTableCode = entryLine.TablePrefix;
			addInf5.CSI_Status = "ACC";

			var readOnlyAdditionalInfoCollection = new ReadOnlyAdditionalInfoCollection(entryLine);
			readOnlyAdditionalInfoCollection.LoadNew();
			AssertEquals("ReadOnlyAdditionalInfoCollection.Count", 4, readOnlyAdditionalInfoCollection.Count);

			var readOnlyAdditionalInfoList = readOnlyAdditionalInfoCollection.Cast<ReadOnlyAdditionalInfo>();
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), addInf1);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), addInf2);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), addInf3);
			CheckReadOnlyDocument(readOnlyAdditionalInfoList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), addInf5);
		}

		protected void CheckReadOnlyDocument(ReadOnlyAdditionalInfo readOnlyAdditionalInfo, AdditionalInfo additionalInfo)
		{
			CombineAssertions("Grouped Additional Info" + readOnlyAdditionalInfo.CSI_ReferenceNumber, () =>
			{
				AssertEquals("CSI_Code", additionalInfo.CSI_Code, readOnlyAdditionalInfo.CSI_Code);
				AssertEquals("CSI_Description", additionalInfo.CSI_Description, readOnlyAdditionalInfo.CSI_Description);
				AssertEquals("CSI_SubType", additionalInfo.CSI_SubType, readOnlyAdditionalInfo.CSI_SubType);
				AssertEquals("CSI_ReferenceNumber", additionalInfo.CSI_ReferenceNumber, readOnlyAdditionalInfo.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", additionalInfo.CSI_ReferenceNumber2, readOnlyAdditionalInfo.CSI_ReferenceNumber2);
				AssertEquals("CSI_RX_NKCurrency", additionalInfo.CSI_RX_NKCurrency, readOnlyAdditionalInfo.CSI_RX_NKCurrency);
				AssertEquals("CSI_Value", additionalInfo.CSI_Value, readOnlyAdditionalInfo.CSI_Value);
				AssertEquals("CSI_Status", additionalInfo.CSI_Status, readOnlyAdditionalInfo.CSI_Status);
			});
		}

		protected override ReadOnlyAdditionalInfoCollection GetCollectionToTest()
		{
			invoiceLine.AdditionalInfos.Add(GetAdditionalInfo("REF111", "TRA"));
			invoiceLine.AdditionalInfos.Add(GetAdditionalInfo("REF222", "TRA"));
			invoiceLine.AdditionalInfos.Add(GetAdditionalInfo("REF333", "INF"));
			var result = new ReadOnlyAdditionalInfoCollection(entryLine);
			result.LoadNew();

			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var addInf = GetAdditionalInfo("REF333", "TRA");
			return new ReadOnlyAdditionalInfo(addInf);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = "BLT";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_NetWeight = 200m;
			invoiceLine.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		AdditionalInfo GetAdditionalInfo(ZString refNumber, ZString kind)
		{
			var addInf = Factory.New<AdditionalInfo>();
			addInf.SuspendValidation();

			addInf.CSI_Code = "1234";
			addInf.CSI_Description = "description";
			addInf.CSI_SubType = kind;
			addInf.CSI_ReferenceNumber = refNumber;
			addInf.CSI_ReferenceNumber2 = refNumber + "Extra";
			addInf.CSI_RX_NKCurrency = "EUR";
			addInf.CSI_Value = 20;
			addInf.CSI_Status = "QWE";

			return addInf;
		}
	}
}
