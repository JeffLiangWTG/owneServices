using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CARLSTLineBuilderTest : TestCaseWithFactory
	{
		public void TestCNISegment()
		{
			AssertMessageContains("CNI++:::I'");
		}

		public void TestRFFCargoCodeSegment()
		{
			AssertMessageContains("RFF+ACC:CARGOCODE'");
		}

		public void TestRFFCargoIdentifierSegment()
		{
			AssertMessageContains("RFF+ACH:CARGOID'");
		}

		public void TestLOCDestinationSegment()
		{
			AssertMessageContains("LOC+8+AUSYD::6'");
		}

		public void TestLOCLoadingSegment()
		{
			AssertMessageContains("LOC+76+NZAKL::6'");
		}

		public void TestGIDSegment()
		{
			AssertMessageContains("GID+1'");
		}

		public void TestPACImportCargoTypeSegment()
		{
			AssertMessageContains("PAC+100'");
		}

		public void TestPACPackagesSegment()
		{
			AssertMessageContains("PAC+++FCL:67:95'");
		}

		public void TestPACPackageTypeSegment()
		{
			AssertMessageContains("PAC+++BOX:185:95'");
		}

		public void TestGetNewSegmentGroup()
		{
			var cUSCAR = new CUSCARMessage();
			var result = Builder.GetNewSegmentGroup(cUSCAR);
			AssertEquals("SegmentGroup", cUSCAR.Group7[0], result);
		}

		public void TestSegmentGroupType()
		{
			AssertEquals("SegmentGroupType", typeof(SegmentGroup7), Builder.SegmentGroupType);
		}

		public void TestUniqueIdentifier()
		{
			AssertEquals("UniqueIdentifier", "CARGOTYPE=FCLCARGOCODE=CARGOCODECARGOID=CARGOID", Builder.UniqueIdentifier);
		}

		void AssertMessageContains(ZString text)
		{
			var group7 = new SegmentGroup7();
			Builder.Populate(group7, "I");
			var messageText = group7.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
		}

		Mock<ICargoListReportLine> lineMock;
		Mock<ICargoListReportLine> LineMock
		{
			get
			{
				if (lineMock == null)
				{
					lineMock = new Mock<ICargoListReportLine>();
					lineMock.Setup(m => m.CargoCode).Returns(new ZString("CARGOCODE"));
					lineMock.Setup(m => m.CargoIdentifier).Returns(new ZString("CARGOID"));
					lineMock.Setup(m => m.PortOfDestination).Returns(new ZString("AUSYD"));
					lineMock.Setup(m => m.PortOfLoading).Returns(new ZString("NZAKL"));
					lineMock.Setup(m => m.ImportCargoType).Returns(new ZString(CMRCargoTypes.Codes.FullContainerLoad));
					lineMock.Setup(m => m.NumberOfPackages).Returns(new ZInt(100));
					lineMock.Setup(m => m.PackageType).Returns(new ZString("BOX"));
				}
				return lineMock;
			}
		}

		CARLSTLineBuilder Builder => new CARLSTLineBuilder(LineMock.Object);
	}
}
