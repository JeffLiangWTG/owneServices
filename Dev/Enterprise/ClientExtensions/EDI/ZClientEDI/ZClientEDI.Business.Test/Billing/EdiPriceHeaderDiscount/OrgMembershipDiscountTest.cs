using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OrgMembershipDiscount))]
	public class OrgMembershipDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestXmlSerialization()
		{
			var discount = new OrgMembershipDiscount();
			var line1 = discount.Lines.AddNew();
			line1.MembershipType = "FTA";
			var line2 = discount.Lines.AddNew();
			line2.MembershipType = "CBAFF";

			var serializer = ZXmlSerializer.New(typeof(OrgMembershipDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (OrgMembershipDiscount)serializer.Deserialize(reader);
			AssertEquals(2, discount2.Lines.Count);
			AssertNotNull(discount2.Lines.OfType<OrgMembershipDiscountLine>().Single(x => x.MembershipType == "FTA"));
			AssertNotNull(discount2.Lines.OfType<OrgMembershipDiscountLine>().Single(x => x.MembershipType == "CBAFF"));
		}

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(AutoOrgMembershipDiscount.Schema));
		}
	}

	[TestedType(typeof(OrgMembershipDiscountLine))]
	public class OrgMembershipDiscountLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var line = new OrgMembershipDiscountLine();
			line.RunPreSaveValidation();
			AssertHasErrors(line.MembershipTypeInfo);

			line.MembershipType = "--";
			AssertHasErrors(line.MembershipTypeInfo);

			line.MembershipType = "FTA";
			AssertNoErrors(line.MembershipTypeInfo);

			var discount = new OrgMembershipDiscount();
			var line1 = discount.Lines.AddNew();
			line1.MembershipType = "FTA";
			AssertNoErrors(line1.MembershipTypeInfo);
			var line2 = discount.Lines.AddNew();
			line2.MembershipType = "FTA";
			AssertHasError(line2.MembershipTypeInfo, "The Membership Type has been duplicated and must be unique.");
			line2.MembershipType = "CBAFF";
			AssertNoErrors(line2.MembershipTypeInfo);
		}

		public void TestXmlSerialization()
		{
			var line = new OrgMembershipDiscountLine();
			line.MembershipType = "FTA";

			var serializer = ZXmlSerializer.New(typeof(OrgMembershipDiscountLine));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, line);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var line2 = (OrgMembershipDiscountLine)serializer.Deserialize(reader);
			AssertEquals("FTA", line2.MembershipType);
		}
	}

	[TestedType(typeof(OrgMembershipDiscountLineCollection))]
	internal class OrgMembershipDiscountLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgMembershipDiscountLineCollection>
	{
		protected override OrgMembershipDiscountLineCollection GetCollectionToTest()
		{
			return new OrgMembershipDiscountLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgMembershipDiscountLine();
		}
	}
}
