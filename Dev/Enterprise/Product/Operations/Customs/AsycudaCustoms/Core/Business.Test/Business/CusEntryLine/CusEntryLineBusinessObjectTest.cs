using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		protected override ZString ExpectedFallbackEntrylineDescription => "LINE";

		public void TestCL_CustomsValue_ResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryLine), nameof(CusEntryLine.CL_CustomsValue), false, attribute => attribute.Caption == "Customs Value");
		}

		public void TestCL_ValueForVAT_ResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusEntryLine), nameof(CusEntryLine.CL_ValueForVAT), false, attribute => attribute.Caption == "VAT/GST Value");
		}
		public void TestCL_AdValoremTariffMaxlength()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();

			AssertEquals("Max Length of CusEntryLine CL_AdValoremTariff should be equals to max length of JobComInvoiceLine JI_Tariff",
				jobComInvoiceLine.JI_TariffInfo.MaxLength, cusEntryLine.CL_AdValoremTariffInfo.MaxLength);
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}
