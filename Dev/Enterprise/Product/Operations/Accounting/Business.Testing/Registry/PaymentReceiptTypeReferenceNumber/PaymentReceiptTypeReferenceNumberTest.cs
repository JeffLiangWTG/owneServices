using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PaymentReceiptTypeReferenceNumber))]
	public class PaymentReceiptTypeReferenceNumberTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new PaymentReceiptTypeReferenceNumber(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Guid.Empty), Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			PaymentReceiptTypeReferenceNumber bizObj = new PaymentReceiptTypeReferenceNumber();
			bizObj.Type = "TST";
			bizObj.ReferenceNumber = (NoResString)"Test Ref Number";

			return bizObj;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentReceiptTypeReferenceNumber();
		}

		protected new PaymentReceiptTypeReferenceNumber BizObj
		{
			get { return (PaymentReceiptTypeReferenceNumber)base.BizObj; }
		}

		#endregion
	}
}
