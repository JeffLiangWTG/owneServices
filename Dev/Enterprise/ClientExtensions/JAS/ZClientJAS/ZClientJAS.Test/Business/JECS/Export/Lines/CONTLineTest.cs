using CargoWise.Types;
using Enterprise.Client.JAS.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class CONTLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			AssertEquals(ExpectedLineAsString, Line.LineAsString);
		}

		public void TestLineAsString_NullParams()
		{
			Line = new CONTLine(null, null);
			AssertEquals(ExpectedLineAsStringWhenNullParams, Line.LineAsString);
			Line = new CONTLine(Container, null);
			AssertEquals(ExpectedLineAsStringWhenNullPackLine, Line.LineAsString);
		}

		public void TestLineAsString_NullContainer()
		{
			Line = new CONTLine(null, PackLine);
			AssertEquals(ExpectedLineAsStringWhenNullContainer, Line.LineAsString);
		}

		public void TestLineAsStringWhenGoodsDescriptionExceedsMaxLength()
		{
			SetPackLineGoodsDescription("1234567890123456789012345678901234567890123456789012345678901234567890");
			AssertEquals(ExpectedLineAsStringWhenGoodsDescriptionExceedsMaxLength, Line.LineAsString);
		}

		public void TestGrossWeight()
		{
			SetPackLineWeight(1000, Constants.Weight.Grams);
			AssertEquals(ExpectedLineAsString.Replace(";87.566;", ";1;"), Line.LineAsString);
			SetPackLineWeight(1, Constants.Weight.ShortTons);
			AssertEquals(ExpectedLineAsString.Replace(";87.566;", ";907.185;"), Line.LineAsString);
		}

		public void TestVolume()
		{
			SetPackLineVolume(100, Constants.Volume.Litre);
			AssertEquals(ExpectedLineAsString.Replace(";24.5;", ";0.1;"), Line.LineAsString);
			SetPackLineVolume(100, Constants.Volume.CubicYards);
			AssertEquals(ExpectedLineAsString.Replace(";24.5;", ";76.455;"), Line.LineAsString);
		}

		public void TestContainerType_NoMappedContainerTypes()
		{
			SetContainerType("20RE");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";20RE;"), Line.LineAsString);
			SetContainerType("40RE");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40RE;"), Line.LineAsString);
			SetContainerType("20OT");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";20OT;"), Line.LineAsString);
			SetContainerType("40OT");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40OT;"), Line.LineAsString);
			SetContainerType("20FR");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";20FR;"), Line.LineAsString);
			SetContainerType("40FR");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40FR;"), Line.LineAsString);
			SetContainerType("20PL");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";20PL;"), Line.LineAsString);
			SetContainerType("40PL");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40PL;"), Line.LineAsString);
			SetContainerType("40HC");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40HC;"), Line.LineAsString);
			SetContainerType("40REHC");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40REHC;"), Line.LineAsString);
			SetContainerType("40NOR");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";40NOR;"), Line.LineAsString);
		}

		public void TestContainerType_FromMappedContainerTypes()
		{
			SetContainerMapping("TEST1", "20");
			SetContainerType("TEST1");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";20;"), Line.LineAsString);
			SetContainerMapping("ASDF", "27");
			SetContainerType("ASDF");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";27;"), Line.LineAsString);
			SetContainerMapping("40HC", "33");
			SetContainerType("40HC");
			AssertEquals("The mapped type should take precedence over our container type code", ExpectedLineAsString.Replace(";99;", ";33;"), Line.LineAsString);
			SetContainerType("LALALA");
			AssertEquals(ExpectedLineAsString.Replace(";99;", ";LALALA;"), Line.LineAsString);
		}

		public void TestTypeOfService()
		{
			SetContainerDeliveryMode(Constants.DeliveryModes.Codes.CY_CY);
			AssertEquals(ExpectedLineAsString.Replace(";CS;", ";CY;"), Line.LineAsString);
			SetContainerDeliveryMode("09823409");
			AssertEquals(ExpectedLineAsString.Replace(";CS;", ";;"), Line.LineAsString);
			Container.JC_JK = Consol.PK;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(ExpectedLineAsString.Replace(";CS;", ";BB;"), Line.LineAsString);
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals(ExpectedLineAsString.Replace(";CS;", ";RR;"), Line.LineAsString);
			Consol.JK_ConsolMode = "__(";
			AssertEquals(ExpectedLineAsString.Replace(";CS;", ";;"), Line.LineAsString);
		}

		#region Overrides
		protected override int ExpectedFieldCount
		{
			get
			{
				return 12;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "CONT";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new CONTLine(Container, PackLine);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JASDataRegistryTest.SetJASWWOrganisationItemForTest(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			JASDataRegistryTest.UnsetJASWWOrganisationItemForTest();
		}

		#endregion
		#region Data for Test
		#region PackLine
		Mock<JASForwardingPackLine> PackLineMock
		{
			get
			{
				if (fPackLineMock == null)
				{
					fPackLineMock = Factory.NewMoq<JASForwardingPackLine>();
					SetDefaultValuesForPackLine();
				}

				return (Mock<JASForwardingPackLine>)fPackLineMock;
			}
		}

		JASForwardingPackLine PackLine
		{
			get
			{
				return PackLineMock.Object;
			}
		}

		void SetDefaultValuesForPackLine()
		{
			SetPackLineVolume(24.5m, Constants.Volume.CubicMetres);
			SetPackLineWeight(87.566m, Constants.Weight.Kilograms);
			SetPackLineGoodsDescription("Goods Description");
			SetPackLinePackageCount(46);
			SetPackLineHarmonisedCode("1110928372");
			SetPackLinePrice(78.34m);
			PackLine.LinePriceCurrency = "HKD";
		}

		void SetPackLineVolume(ZDecimal volume, ZString volumeUnit)
		{
			PackLineMock.Setup(m => m.JL_ActualVolume).Returns(volume);
			PackLineMock.Setup(m => m.JL_ActualVolumeUQ).Returns(volumeUnit);
		}

		void SetPackLineWeight(ZDecimal weight, ZString weightUnit)
		{
			PackLineMock.Setup(m => m.JL_ActualWeight).Returns(weight);
			PackLineMock.Setup(m => m.JL_ActualWeightUQ).Returns(weightUnit);
		}

		void SetPackLineGoodsDescription(ZString description)
		{
			PackLineMock.Setup(m => m.JL_Description).Returns(description);
		}

		void SetPackLinePackageCount(ZInt count)
		{
			PackLineMock.Setup(m => m.JL_PackageCount).Returns(count);
		}

		void SetPackLineHarmonisedCode(ZString harmonisedCode)
		{
			PackLineMock.Setup(m => m.JL_HarmonisedCode).Returns(harmonisedCode);
		}

		void SetPackLinePrice(ZDecimal commodityValue)
		{
			PackLineMock.Setup(m => m.JL_LinePrice).Returns(commodityValue);
		}

		Mock fPackLineMock;
		#endregion
		#region Container
		Mock<ForwardingContainer> ContainerMock
		{
			get
			{
				if (fContainerMock == null)
				{
					fContainerMock = Factory.NewMoq<ForwardingContainer>();
					SetDefaultValuesForContainer();
				}

				return (Mock<ForwardingContainer>)fContainerMock;
			}
		}

		ForwardingContainer Container
		{
			get
			{
				return ContainerMock.Object;
			}
		}

		void SetDefaultValuesForContainer()
		{
			ContainerMock.Setup(m => m.JC_ContainerNum).Returns((ZString)"CONAA1100");
			ContainerMock.Setup(m => m.JC_SealNum).Returns((ZString)"SEAL1001");
			SetContainerDeliveryMode(Core.Constants.DeliveryModes.Codes.CFS_CFS);
		}

		void SetContainerType(ZString containerType)
		{
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = containerType;
			}

			ContainerMock.Setup(m => m.JC_RC).Returns(refContainer.PK);
		}

		void SetContainerDeliveryMode(ZString deliveryMode)
		{
			ContainerMock.Setup(m => m.JC_DeliveryMode).Returns(deliveryMode);
		}

		void SetContainerMapping(ZString localCode, ZString foreignCode)
		{
			RefContainer refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, localCode);
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = localCode;
			}

			OrgHeader jASWWOrganisation = Factory.Load<OrgHeader>(JASDataRegistry.Instance.JASWWOrganisationPK);
			jASWWOrganisation.PatternMatchOverrides_ForBinding.RemoveAll();
			OrgPatternMatchOverride @override = jASWWOrganisation.CreatePatternMatchOverrideForTest();
			@override.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			@override.OO_LocalGuid = refContainer.PK;
			@override.OO_ForeignCode = foreignCode;
		}

		Mock fContainerMock;
		#endregion
		#region Consol
		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		JASForwardingConsol fConsol;
		#endregion
		#endregion
		#region Expected LineAsString
		const string ExpectedLineAsString = "CONT3100;CONAA1100;SEAL1001;;99;87.566;24.5;46;Goods Description;CS;1110928372;78.34;HKD";
		const string ExpectedLineAsStringWhenGoodsDescriptionExceedsMaxLength = "CONT3100;CONAA1100;SEAL1001;;99;87.566;24.5;46;1234567890123456789012345678901234567890;CS;1110928372;78.34;HKD";
		const string ExpectedLineAsStringWhenNullParams = "CONT3100;;;;;;;;;;;;";
		const string ExpectedLineAsStringWhenNullContainer = "CONT3100;N/A;;;99;87.566;24.5;46;Goods Description;;1110928372;78.34;HKD";
		const string ExpectedLineAsStringWhenNullPackLine = "CONT3100;;;;;;;;;;;;";
		#endregion
	}
}
