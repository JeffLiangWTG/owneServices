using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ApprovalRequestChargeDetails))]
	public class ApprovalRequestChargeDetailsTest : ApprovalRequestChargeDetailsTest<ApprovalRequestChargeDetails>
	{
		protected override string GetInstanceSpecificExpectedXML() => string.Empty;

		protected override void SetInstanceSpecificBizOPropertiesForXMLTest(ApprovalRequestChargeDetails charge)
		{
			//Do nothing
		}

		protected override void AssertInstanceSpecificBizOPropertiesForXMLTest(ApprovalRequestChargeDetails charge)
		{
			//Do nothing
		}
	}

	public abstract class ApprovalRequestChargeDetailsTest<DetailsType> : NonPersistentBusinessObjectTestCase
			where DetailsType : ApprovalRequestChargeDetails
	{
		public virtual void TestOpertorEqual()
		{
			DetailsType a = null;
			DetailsType b = null;
			AssertEquals(true, a == b);

			a = (DetailsType)GetNewBusinessObject();
			b = null;
			AssertEquals(false, a == b);

			a = null;
			b = (DetailsType)GetNewBusinessObject();
			AssertEquals(false, a == b);

			a = (DetailsType)GetNewBusinessObject();
			b = (DetailsType)GetNewBusinessObject();
			AssertEquals(true, a == b);

			a.JobNumber = "1";
			AssertEquals(false, a == b);
			b.JobNumber = "1";
			AssertEquals(true, a == b);

			a.ChargeCode = "1";
			AssertEquals(false, a == b);
			b.ChargeCode = "1";
			AssertEquals(true, a == b);

			a.Branch = "1";
			AssertEquals(false, a == b);
			b.Branch = "1";
			AssertEquals(true, a == b);

			a.Department = "1";
			AssertEquals(false, a == b);
			b.Department = "1";
			AssertEquals(true, a == b);

			a.PlaceOfSupply = "SA";
			AssertEquals(false, a == b);
			b.PlaceOfSupply = "SA";
			AssertEquals(true, a == b);

			a.PlaceOfSupplyType = "STA";
			AssertEquals(false, a == b);
			b.PlaceOfSupplyType = "STA";
			AssertEquals(true, a == b);
		}

		public virtual void TestCopyFrom()
		{
			var charge = (DetailsType)GetNewBusinessObject();

			charge.JobNumber = "job";
			charge.ChargeCode = "charge";
			charge.Branch = "branch";
			charge.Department = "department";

			var newCharge = (DetailsType)GetNewBusinessObject();

			newCharge.CopyFrom(charge);
			AssertEquals("JobNumber", "job", newCharge.JobNumber);
			AssertEquals("ChargeCode", "charge", newCharge.ChargeCode);
			AssertEquals("Branch", "branch", newCharge.Branch);
			AssertEquals("Department", "department", newCharge.Department);
		}

		#region Serialization

		public void TestReadXML()
		{
			foreach (var setFPOS in new[] { true, false })
			{
				var xml = GetXML(forReader: true, setFPOS);

				using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					var charge = (DetailsType)GetNewBusinessObject();

					reader.Read();
					charge.ReadXml(reader);
					AssertFullyPopulatedBusinessObjectForXMLTest(charge);
				}
			}
		}

		public void TestWriteXML()
		{
			foreach (var setFPOS in new[] { true, false })
			{
				var charge = GetNewFullyPopulatedBusinessObjectForXMLTest(setFPOS);

				using (MemoryStream stream = new MemoryStream())
				using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode))
				{
					charge.WriteXml(writer);
					writer.Flush();
					var xml = Encoding.Unicode.GetString(stream.ToArray());
					var expectedXML = GetXML(forReader: false, setFPOS);
					this.AssertXMLEqualsByDiff("Resulting XML", "<wrapper>" + expectedXML + "</wrapper>", "<wrapper>" + xml + "</wrapper>");
				}
			}
		}

		string GetXML(bool forReader, bool setFPOS = false)
		{
			string template = forReader ? "<palceholder>{0}</palceholder>" : "\uFEFF{0}";

			return string.Format(template, GetExpectedXML(setFPOS));
		}
		string GetExpectedXML(bool setFPOS)
		{
			var result = (setFPOS ? "<JobNumber>job</JobNumber><ChargeCode>charge</ChargeCode><Branch>branch</Branch><Department>department</Department><PlaceOfSupply>AS</PlaceOfSupply><PlaceOfSupplyType>STA</PlaceOfSupplyType>"
					: "<JobNumber>job</JobNumber><ChargeCode>charge</ChargeCode><Branch>branch</Branch><Department>department</Department>");

			result += "<Description /><AccInvMsgPK>00000000-0000-0000-0000-000000000000</AccInvMsgPK><TaxDate />";
			result += GetInstanceSpecificExpectedXML();

			return result;
		}

		DetailsType GetNewFullyPopulatedBusinessObjectForXMLTest(bool setFPOS = false)
		{
			var charge = (DetailsType)GetNewBusinessObject();

			charge.JobNumber = "job";
			charge.ChargeCode = "charge";
			charge.Branch = "branch";
			charge.Department = "department";
			if (setFPOS)
			{
				charge.PlaceOfSupply = "AS";
				charge.PlaceOfSupplyType = "STA";
			}
			SetInstanceSpecificBizOPropertiesForXMLTest(charge);
			return charge;
		}

		protected void AssertFullyPopulatedBusinessObjectForXMLTest(DetailsType charge, bool setFPOS = false)
		{
			AssertEquals("JobNumber", "job", charge.JobNumber);
			AssertEquals("ChargeCode", "charge", charge.ChargeCode);
			AssertEquals("Branch", "branch", charge.Branch);
			AssertEquals("Department", "department", charge.Department);
			if (setFPOS)
			{
				AssertEquals("PlaceOfSupply", "AS", charge.PlaceOfSupply);
				AssertEquals("PlaceOfSupplyType", "STA", charge.PlaceOfSupplyType);
			}
			AssertInstanceSpecificBizOPropertiesForXMLTest(charge);
		}

		protected abstract string GetInstanceSpecificExpectedXML();

		protected abstract void SetInstanceSpecificBizOPropertiesForXMLTest(DetailsType charge);

		protected abstract void AssertInstanceSpecificBizOPropertiesForXMLTest(DetailsType charge);
		#endregion
	}
}
