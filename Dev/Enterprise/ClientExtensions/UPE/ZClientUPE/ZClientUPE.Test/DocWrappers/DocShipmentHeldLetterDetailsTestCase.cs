using System;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(DocShipmentHeldLetterDetails))]
	public class DocShipmentHeldLetterDetailsTestCase : DocumentWrapperTestCase
	{
		public void TestUPSContactName()
		{
			BizObj.UPSContactName = "UPSContactName";
			AssertEquals("UPSContactName", Doc.UPSContactName);
		}

		public void TestUPSContactPhone()
		{
			BizObj.UPSContactPhone = "UPSContactPhone";
			AssertEquals("UPSContactPhone", Doc.UPSContactPhone);
		}

		public void TestUPSContactFax()
		{
			UPEDataRegistry.Instance.UPSContactFax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Fax");
			AssertEquals("Fax", Doc.UPSContactFax);
		}

		public void TestUPSContactEmail()
		{
			UPEDataRegistry.Instance.UPSContactEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Email");
			AssertEquals("Email", Doc.UPSContactEmail);
		}

		public void TestReasonText()
		{
			BizObj.ReasonText = "Reason";
			AssertEquals("Reason", Doc.ReasonText);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestToString()
		{
			Doc.ToString();
		}

		#region Implementation
		ShipmentHeldLetterBusinessObject BizObj;
		DocShipmentHeldLetterDetails Doc;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			UPECusHAWB cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			BizObj = cusHAWB.ShipmentHeldLetterDetails;
			Doc = DocShipmentHeldLetterDetails.New(BizObj, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocShipmentHeldLetterDetails.New(BizObj, Factory) };
		}
		#endregion
	}
}
