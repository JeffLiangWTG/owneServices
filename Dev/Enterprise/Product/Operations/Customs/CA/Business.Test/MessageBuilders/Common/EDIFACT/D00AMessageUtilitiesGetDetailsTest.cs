using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSRES;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class D00AMessageUtilitiesGetDetailsTest : TestCaseWithFactory
	{
		public void TestGetDocumentReference()
		{
			var bgmSection = new SegmentMessageSection<BGMSegment>();
			bgmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "BGM+85+ABCD1234+9");
			AssertEquals("ABCD1234", D00AMessageUtilities.GetDocumentReference(bgmSection));
		}

		public void TestGetProcessingIndicator()
		{
			var gisSection = new SegmentMessageSection<GISSegment>();
			gisSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "GIS+14");
			AssertEquals("14", D00AMessageUtilities.GetProcessingIndicator(gisSection));
		}

		public void TestGetReference()
		{
			var group3Section = new SegmentGroupMessageSection<SegmentGroup3>();
			var rffSection = group3Section.InstantiateAChildAndAddItToChildrenCollection().RFF;
			rffSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "RFF+MB:123456789");
			AssertEquals("123456789", D00AMessageUtilities.GetReference(group3Section, ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber));
		}

		public void TestGetContainers()
		{
			var group6Section = new SegmentGroupMessageSection<SegmentGroup6>();
			var eqdSection = group6Section.InstantiateAChildAndAddItToChildrenCollection().EQD;
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 1");
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 2");
			eqdSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "EQD+CN+CONTAINER 3");

			var containers = D00AMessageUtilities.GetEquipment(group6Section, EquipmentTypeCodeQualifierList.Container);
			AssertEquals("Container count", 3, containers.Count());
			AssertEquals("CONTAINER 1", containers.ElementAt(0));
			AssertEquals("CONTAINER 2", containers.ElementAt(1));
			AssertEquals("CONTAINER 3", containers.ElementAt(2));
		}

		public void TestGetErrrorIdentifier()
		{
			var group4Section = new SegmentGroupMessageSection<SegmentGroup4>();
			var erpSection = group4Section.InstantiateAChildAndAddItToChildrenCollection().ERP;
			erpSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "ERP+2:55:28");
			AssertEquals("28", D00AMessageUtilities.GetErrrorIdentifier(group4Section));
		}

		public void TestGetDate()
		{
			var dtmSection = new SegmentMessageSection<DTMSegment>(1);
			dtmSection.InstantiateAChildAndAddItToChildrenCollection().Parse(characterSet, "DTM+9:200912221030:203");
			AssertEquals(new ZDateTime(2009, 12, 22, 10, 30, 0), D00AMessageUtilities.GetDate(dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingDateTime));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			characterSet = new CACharSet();
		}

		CACharSet characterSet;

		#endregion
	}
}
