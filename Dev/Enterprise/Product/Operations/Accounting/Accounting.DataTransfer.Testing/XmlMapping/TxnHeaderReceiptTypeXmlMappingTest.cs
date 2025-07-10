using System;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.XmlMapping.Testing
{
	[TestedType(typeof(TxnHeaderReceiptPaymentTypeXmlMapping))]
	public class TxnHeaderReceiptTypeXmlMappingTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(ZArchitecture.Core.ReceiptTypes) }; }
		}

		public void TestName()
		{
			var mappings = new TxnHeaderReceiptPaymentTypeXmlMappingTestClass();
			AssertEquals("Name", "Receipt Payment Type", mappings.Name);
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[] { ZArchitecture.Core.ReceiptTypes.ForeignCurrencyBalance };
		}

		class TxnHeaderReceiptPaymentTypeXmlMappingTestClass : TxnHeaderReceiptPaymentTypeXmlMapping
		{
			public new string Name
			{
				get { return base.Name; }
			}
		}
	}
}
