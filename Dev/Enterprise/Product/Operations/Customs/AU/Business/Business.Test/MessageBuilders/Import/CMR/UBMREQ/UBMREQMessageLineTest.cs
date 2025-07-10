using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Moq;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UBMREQMessageLineTest : TestCaseWithFactory
	{
		public void TestSegmentGroupType()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.SegmentGroupType);
		}

		public void TestGetNewSegmentGroup()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.GetNewSegmentGroup(new CUSCARMessage()).GetType());
		}

		public void TestCNI()
		{
			var group7 = (SegmentGroup7)MessageLine.GetNewSegmentGroup(new CUSCARMessage());
			MessageLine.Populate(group7, "I");
			Assert(group7.ToString(new UNOCCMRCharacterSet()).IndexOf("CNI++:::I'") != -1);
		}

		public void TestGID()
		{
			Assert(MessageLine.StringValue.IndexOf("GID+1'") != -1);
		}

		public void TestNumberOfPacks()
		{
			Assert("Number of packages", MessageLine.StringValue.IndexOf("PAC+123'") != -1);
		}

		public void TestCargoType()
		{
			Assert("CargoType", MessageLine.StringValue.IndexOf("PAC+++FCL:67:95'") != -1);
		}

		public void TestPackageType()
		{
			Assert("PackageType", MessageLine.StringValue.IndexOf("PAC+++BOX:185:95'") != -1);
		}

		public void TestPackageTypeFromHeader()
		{
			packageType = "KEG";
			requestLine = new TestUnderbondRequestLine();
			requestLine.HouseAirWaybillNumber = "HW234";
			Assert("PackageType", MessageLine.StringValue.IndexOf("PAC+++KEG:185:95'") != -1);
		}

		public void TestNumberOfPacksFromHeader()
		{
			numberOfPackages = 23;
			requestLine = new TestUnderbondRequestLine();
			requestLine.HouseAirWaybillNumber = "HW234";
			Assert("Number of packages", MessageLine.StringValue.IndexOf("PAC+23'") != -1);
		}

		public void TestPopulateHouseAWB()
		{
			Assert("Housebill", MessageLine.StringValue.IndexOf("RFF+HWB:HW123'") != -1);
		}

		public void TestPopulateMasterAWB()
		{
			Assert("Master bill number", MessageLine.StringValue.IndexOf("RFF+MWB:MB123'") != -1);
		}

		public void TestPopulateContainer()
		{
			Assert("Container number", MessageLine.StringValue.IndexOf("RFF+AAQ:CN123'") != -1);
		}

		public void TestPopulateHBL()
		{
			Assert("HouseBill", MessageLine.StringValue.IndexOf("RFF+BH:HB123'") != -1);
		}

		public void TestPopulateOceanBill()
		{
			Assert("Ocean Bill", MessageLine.StringValue.IndexOf("RFF+MB:OB123'") != -1);
		}

		public void TestPopulateUniqueConsigmentRef()
		{
			Assert("Unique Consignment Ref", MessageLine.StringValue.IndexOf("RFF+UCN:321'") != -1);
		}

		public void TestUniqueIdentifier()
		{
			AssertEquals("UniqueIdentifier", "CONTAINERNUMBER=CN123OCEANBILL=OB123HOUSEBILL=HB123MAWB=MB123HAWB=HW123", MessageLine.UniqueIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			requestLine = new TestUnderbondRequestLine();
			requestLine.HouseAirWaybillNumber = "HW234";
		}

		TestUnderbondRequestLine requestLine;

		Mock<IUnderbondMovementRequestLine> requestLineMock;
		Mock<IUnderbondMovementRequestLine> RequestLineMock
		{
			get
			{
				if (requestLineMock == null)
				{
					requestLineMock = new Mock<IUnderbondMovementRequestLine>();
					requestLineMock.Setup(m => m.ContainerNumber).Returns("CN123");
					requestLineMock.Setup(m => m.HouseBillOfLading).Returns("HB123");
					requestLineMock.Setup(m => m.HouseAirWaybillNumber).Returns("HW123");
					requestLineMock.Setup(m => m.OceanBillOfLading).Returns("OB123");
					requestLineMock.Setup(m => m.MasterAirWaybillNumber).Returns("MB123");
					requestLineMock.Setup(m => m.UniqueConsignmentReferenceNumber).Returns("321");
					requestLineMock.Setup(m => m.NumberOfPackages).Returns(123);
					requestLineMock.Setup(m => m.PackageType).Returns("BOX");
					requestLineMock.Setup(m => m.ImportCargoType).Returns("FCL");
				}
				return requestLineMock;
			}
		}

		int numberOfPackages;
		string packageType = string.Empty;

		UBMREQMessageLine messageLine;
		UBMREQMessageLine MessageLine => messageLine ?? (messageLine = new UBMREQMessageLine(RequestLineMock.Object, numberOfPackages, packageType));

		sealed class TestUnderbondRequestLine : IUnderbondMovementRequestLine
		{
			public ZString HouseAirWaybillNumber { get; set; }

			public ZString MasterAirWaybillNumber { get; set; }

			public ZInt NumberOfPackages { get; set; }

			public ZString ContainerNumber => string.Empty;

			public ZString HouseBillOfLading => string.Empty;

			public ZString OceanBillOfLading => string.Empty;

			public ZString UniqueConsignmentReferenceNumber => string.Empty;

			public ZString PackageType => string.Empty;

			public ZString ImportCargoType => string.Empty;
		}
	}
}
