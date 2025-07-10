using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumberRegistryDataType))]
	class PaymentReceiptTypeReferenceNumberRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PaymentReceiptTypeReferenceNumberRegistryDataType>
	{
		#region Implementation

		protected override PaymentReceiptTypeReferenceNumberRegistryDataType GetNewDataType()
		{
			return new PaymentReceiptTypeReferenceNumberRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PaymentReceiptTypeReferenceNumberRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PaymentReceiptTypeReferenceNumberCollection items = new PaymentReceiptTypeReferenceNumberCollection();
			PaymentReceiptTypeReferenceNumber item = items.AddNew();
			item.Type = "TST";
			item.ReferenceNumber = (NoResString)"Test Ref Number";

			string stringValue = @"﻿<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfPaymentReceiptTypeReferenceNumber xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<PaymentReceiptTypeReferenceNumber>
<Type>TST</Type>
<ReferenceNumber>Test Ref Number</ReferenceNumber>
</PaymentReceiptTypeReferenceNumber>
</ArrayOfPaymentReceiptTypeReferenceNumber>";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(items, Encoding.Unicode.GetBytes(stringValue)) };
		}
		#endregion
	}
}
