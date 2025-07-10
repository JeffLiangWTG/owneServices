using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class CONTRecordTest : JXCRecordTestCase
	{
		[ExpectNoExceptions]
		public void TestUpdateContainerAndPackLine_NullParam()
		{
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;OOLE947946;;12;702.770;4.620;2;.;;;0.00;HKD");
			record.UpdateContainerAndPackLine(null, null);
		}

		public void TestUpdateContainerAndPackLine_NullShipment()
		{
			AssertEquals("Pre-condition", 0, Consol.Containers.Count);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;SEAL123;;40HC;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			record.UpdateContainerAndPackLine(Consol, null);
			AssertEquals(1, Consol.Containers.Count);
			ForwardingContainer container = Consol.Containers[0];
			AssertEquals("OOLU5948074", container.JC_ContainerNum);
			AssertEquals("SEAL123", container.JC_SealNum);
			AssertEquals("40HC", container.RefContainer.RC_Code);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, container.Consol.JK_ConsolMode);
		}

		public void TestUpdateContainerAndPackLine_NullConsol()
		{
			AssertEquals("Pre-condition", 0, Shipment.OuterPackLines.Count);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;SEAL123;;12;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			record.UpdateContainerAndPackLine(null, Shipment);
			AssertEquals(1, Shipment.OuterPackLines.Count);
			JASForwardingPackLine packLine = (JASForwardingPackLine)Shipment.OuterPackLines[0];
			AssertEquals(702.77m, packLine.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, packLine.JL_ActualWeightUQ);
			AssertEquals(4.62m, packLine.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicMetres, packLine.JL_ActualVolumeUQ);
			AssertEquals(2, packLine.JL_PackageCount);
			AssertEquals("Description", packLine.JL_Description);
			AssertEquals("1234", packLine.JL_HarmonisedCode);
			AssertEquals(20.52m, packLine.JL_LinePrice);
			AssertEquals("HKD", packLine.LinePriceCurrency);
		}

		public void TestUpdateContainerAndPackLine_NewContainer()
		{
			AssertEquals("Pre-condition", 0, Shipment.OuterPackLines.Count);
			AssertEquals("Pre-condition", 0, Consol.Containers.Count);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;SEAL123;;40HC;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			record.UpdateContainerAndPackLine(Consol, Shipment);
			AssertEquals(1, Consol.Containers.Count);
			ForwardingContainer container = Consol.Containers[0];
			AssertEquals("OOLU5948074", container.JC_ContainerNum);
			AssertEquals("SEAL123", container.JC_SealNum);
			AssertEquals("40HC", container.RefContainer.RC_Code);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, container.Consol.JK_ConsolMode);
			AssertEquals(1, Shipment.OuterPackLines.Count);
			JASForwardingPackLine packLine = (JASForwardingPackLine)Shipment.OuterPackLines[0];
			AssertEquals(container.PK, packLine.JL_JC);
			AssertEquals(702.77m, packLine.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, packLine.JL_ActualWeightUQ);
			AssertEquals(4.62m, packLine.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicMetres, packLine.JL_ActualVolumeUQ);
			AssertEquals(2, packLine.JL_PackageCount);
			AssertEquals("Description", packLine.JL_Description);
			AssertEquals("1234", packLine.JL_HarmonisedCode);
			AssertEquals(20.52m, packLine.JL_LinePrice);
			AssertEquals("HKD", packLine.LinePriceCurrency);
		}

		public void TestUpdateContainerAndPackLine_ContainerAlreadyExist()
		{
			ForwardingContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "OOLU5948074";
			container.JC_SealNum = "ORGSEAL";
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;SEAL123;;40HC;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			record.UpdateContainerAndPackLine(Consol, Shipment);
			AssertEquals(1, Consol.Containers.Count);
			AssertEquals("OOLU5948074", container.JC_ContainerNum);
			AssertEquals("SEAL123", container.JC_SealNum);
			AssertEquals("40HC", container.RefContainer.RC_Code);
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, container.Consol.JK_ConsolMode);
			AssertEquals(1, Shipment.OuterPackLines.Count);
			JASForwardingPackLine packLine = (JASForwardingPackLine)Shipment.OuterPackLines[0];
			AssertEquals(702.77m, packLine.JL_ActualWeight);
			AssertEquals(Core.Constants.Weight.Kilograms, packLine.JL_ActualWeightUQ);
			AssertEquals(4.62m, packLine.JL_ActualVolume);
			AssertEquals(Core.Constants.Volume.CubicMetres, packLine.JL_ActualVolumeUQ);
			AssertEquals(2, packLine.JL_PackageCount);
			AssertEquals("Description", packLine.JL_Description);
			AssertEquals("1234", packLine.JL_HarmonisedCode);
			AssertEquals(20.52m, packLine.JL_LinePrice);
			AssertEquals("HKD", packLine.LinePriceCurrency);
		}

		public void TestUpdateContainerAndPackLine_ContainerNumShouldOnlyContainerLettersAndNumbers()
		{
			AssertEquals("Pre-condition", 0, Consol.Containers.Count);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;AXIL/ 90293*/*/+99;SEAL123;;12;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			record.UpdateContainerAndPackLine(Consol, Shipment);
			AssertEquals("Non letters and numbers should be excluded", "AXIL9029399", Consol.Containers[0].JC_ContainerNum);
		}

		public void TestUpdateContainerAndPackLine_NumericFieldsShouldNotExceedMaxValue()
		{
			AssertEquals("Pre-condition", 0, Shipment.OuterPackLines.Count);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOLU5948074;SEAL123;;12;1500000;1000000;2;Description;BB;1234;99999999999999999999999999999999;HKD");
			record.UpdateContainerAndPackLine(Consol, Shipment);
			AssertEquals(1, Shipment.OuterPackLines.Count);
			JASForwardingPackLine packLine = (JASForwardingPackLine)Shipment.OuterPackLines[0];
			AssertEquals("Should not be populated if exceeding max value allowed", ZDecimal.Zero, packLine.JL_ActualVolume);
			AssertEquals("Should not be populated if exceeding max value allowed", ZDecimal.Zero, packLine.JL_ActualWeight);
			AssertEquals("Should not be populated if exceeding max value allowed", ZDecimal.Zero, packLine.JL_LinePrice);
		}

		[ExpectNoExceptions]
		public void TestUpdateContainerAndPackLine_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('X', 1000);
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord(string.Format("CONT3100;{0};{0};{0};{0};{0};{0};{0};{0};{0};{0};{0};{0}", excessivelyLongString));
			record.UpdateContainerAndPackLine(Consol, Shipment);
		}

		public void TestUpdateContainerAndPackLine_ContainerAlreadyExistWithSlightlyDifferentContainerNum()
		{
			ForwardingContainer container = Consol.Containers.AddNew();
			container.JC_ContainerNum = "OOLU5948074";
			container.JC_SealNum = "ORGSEAL";
			CONTRecord record = (CONTRecord)RecordFactory.NewRecord("CONT3100;OOL/U**/5-948074;SEAL123;;12;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			AssertEquals("Pre-condition", 1, Consol.Containers.Count);
			record.UpdateContainerAndPackLine(Consol, Shipment);
			AssertEquals("Should not create a new container, if you strip out the special characters from 'OOL/U**/5-948074' then it should be the same as 'OOLU5948074'", 1, Consol.Containers.Count);
			container.JC_ContainerNum = "0012%$12";
			record = (CONTRecord)RecordFactory.NewRecord("CONT3100;0012%$12;SEAL123;;12;702.770;4.620;2;Description;BB;1234;20.52;HKD");
			AssertEquals("Should not create a new container, container number should be matched", 1, Consol.Containers.Count);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new CONTRecord(lineType, lineContent);
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
					fConsol.Shipments.AddNew();
				}

				return fConsol;
			}
		}

		JASForwardingShipment Shipment
		{
			get
			{
				return (JASForwardingShipment)Consol.Shipments[0];
			}
		}

		JASForwardingConsol fConsol;
	}
}
