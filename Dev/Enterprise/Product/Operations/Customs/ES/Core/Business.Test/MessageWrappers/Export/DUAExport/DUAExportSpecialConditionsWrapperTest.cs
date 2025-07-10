using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DUAExportSpecialConditionsWrapperTest : WrapperHelperTest<DUAExportSpecialConditionsWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null Entry Line", () => new DUAExportSpecialConditionsWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No InvoiceLines", () => new DUAExportSpecialConditionsWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestCode1()
		{
			CombineAssertions(() =>
			{
				SetupAdditionalInfos(1);
				wrapper = new DUAExportSpecialConditionsWrapper(entryLine);
				var code1 = wrapper.Code1;

				AssertEquals("Expected filled Code1", AddInfos[0].Code, code1);
				AssertEquals("Cached Code1", wrapper.Code1, code1);
			});
		}

		public void TestCode2()
		{
			CombineAssertions(() =>
			{
				SetupAdditionalInfos(2);
				wrapper = new DUAExportSpecialConditionsWrapper(entryLine);
				var code2 = wrapper.Code2;

				AssertEquals("Expected filled Code2", AddInfos[1].Code, code2);
				AssertEquals("Cached Code2", wrapper.Code2, code2);
			});
		}

		public void TestCode3()
		{
			CombineAssertions(() =>
			{
				SetupAdditionalInfos(3);
				wrapper = new DUAExportSpecialConditionsWrapper(entryLine);
				var code3 = wrapper.Code3;

				AssertEquals("Expected filled Code3", AddInfos[2].Code, code3);
				AssertEquals("Cached Code3", wrapper.Code3, code3);
			});
		}

		public void TestCode4()
		{
			CombineAssertions(() =>
			{
				SetupAdditionalInfos(4);
				wrapper = new DUAExportSpecialConditionsWrapper(entryLine);
				var code4 = wrapper.Code4;

				AssertEquals("Expected filled Code4", AddInfos[3].Code, code4);
				AssertEquals("Cached Code4", wrapper.Code4, code4);
			});
		}

		public void TestText()
		{
			CombineAssertions(() =>
			{
				SetupAdditionalInfos(4);
				wrapper = new DUAExportSpecialConditionsWrapper(entryLine);

				var textString = AddInfos[0].Desc + " " + AddInfos[1].Desc + " " + AddInfos[2].Desc + " " + AddInfos[3].Desc;
				var text = wrapper.Text;

				AssertEquals("Expected filled Text", textString, text);
				AssertEquals("Cached Text", wrapper.Text, text);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = "EXP";

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = new DUAExportSpecialConditionsWrapper(entryLine);
		}
		JobDeclaration declaration;
		CusEntryLine entryLine;
		DUAExportSpecialConditionsWrapper wrapper;

		void SetupAdditionalInfos(int maxAddInfo)
		{
			for (var i = 0; i < maxAddInfo; i++)
			{
				var addInfo = declaration.AdditionalInfos.AddNew();
				addInfo.CSI_Code = AddInfos[i].Code;
				addInfo.CSI_Description = AddInfos[i].Desc;
			}
		}

		protected override DUAExportSpecialConditionsWrapper GetProvider() => wrapper;
	}
}
