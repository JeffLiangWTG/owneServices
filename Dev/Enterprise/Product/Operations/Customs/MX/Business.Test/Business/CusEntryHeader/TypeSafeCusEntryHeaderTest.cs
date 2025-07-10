using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);
	}
}
