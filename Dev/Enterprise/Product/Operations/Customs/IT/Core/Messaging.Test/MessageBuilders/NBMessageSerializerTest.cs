using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.MessageBuilders;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class NBMessageSerializerTest : BaseMessageSerializerTest
{
	public void TestHeader()
	{
		AssertContains("TNB           99999900IM	REF	R	120719	3	REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1		OFF-B1	1	3902.12346	A123456789	3343.12346	99.23	REA2	A2-REF	B	020719	A2	2	OFF-A2	MRNB	REB2	B2-REF	B	020719	B2	2	OFF-B2	2	2343.12	B123456789	33243.96	44.3	REA3	A3-REF	C	030719	A3	3	OFF-A3	MRNC	REB3	B3-REF	C	030719	B3	3	OFF-B3	3	3333.33	C123456789	33333.33	33.3	REA4	A4-REF	D	040719	A4	4	OFF-A4	MRND	REB4	B4-REF	D	040719	B4	4	OFF-B4	4	4444.44	D123456789	44444.44	44.4	1	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestFixedPart()
	{
		AssertContains("TNB           99999900", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestOperation1()
	{
		AssertContains("REA1	A1-REF	A	010719	A1	1	OFF-A1	MRNA	REB1	B1-REF	A	010719	B1		OFF-B1	1	3902.12346	A123456789	3343.12346	99.23	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestOperation2()
	{
		AssertContains("REA2	A2-REF	B	020719	A2	2	OFF-A2	MRNB	REB2	B2-REF	B	020719	B2	2	OFF-B2	2	2343.12	B123456789	33243.96	44.3	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestOperation3()
	{
		AssertContains("REA3	A3-REF	C	030719	A3	3	OFF-A3	MRNC	REB3	B3-REF	C	030719	B3	3	OFF-B3	3	3333.33	C123456789	33333.33	33.3	", flatFileMessageSerializer.Serialize(nBMessage));
	}
	public void TestOperation4()
	{
		AssertContains("REA4	A4-REF	D	040719	A4	4	OFF-A4	MRND	REB4	B4-REF	D	040719	B4	4	OFF-B4	4	4444.44	D123456789	44444.44	44.4	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestOperation5()
	{
		AssertContains("REA5	A5-REF	E	050719	A5	5	OFF-A5	MRNE	REB5	B5-REF	E	050719	B5	5	OFF-B5	5	5555.55	E123456789	55555.55	55.5	", flatFileMessageSerializer.Serialize(nBMessage));
	}
	public void TestOperation6()
	{
		AssertContains("REA6	A6-REF	F	060719	A6	6	OFF-A6	MRNF	REB6	B6-REF	F	060719	B6	6	OFF-B6	6	6666.66	F123456789	66666.66	66.6	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestContinuation()
	{
		AssertContains("?NB1          99999900REA5	A5-REF	E	050719	A5	5	OFF-A5	MRNE	REB5	B5-REF	E	050719	B5	5	OFF-B5	5	5555.55	E123456789	55555.55	55.5	REA6	A6-REF	F	060719	A6	6	OFF-A6	MRNF	REB6	B6-REF	F	060719	B6	6	OFF-B6	6	6666.66	F123456789	66666.66	66.6																																									0	", flatFileMessageSerializer.Serialize(nBMessage));
	}

	public void TestContinuationFixedPart()
	{
		AssertContains("?NB1          99999900", flatFileMessageSerializer.Serialize(nBMessage));
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCompleteMessage()
	{
		AssertASCIIFileSameAsString(BaseSourcePath + Messaging.Testing.TestConstants.ProjectRelativePath + @"TestFiles\TestNB.txt", flatFileMessageSerializer.Serialize(nBMessage));
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCompleteMessageWithFallbackProcedure()
	{
		AssertASCIIFileSameAsString(BaseSourcePath + Messaging.Testing.TestConstants.ProjectRelativePath + @"TestFiles\TestNB_WithFallbackProcedure.txt", flatFileMessageSerializer.Serialize(nBMessageWithFallbackProcedure));
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nBMessageMock = new Mock<INBMessageSendingObject>();
		nBMessageMock.Setup(m => m.Header).Returns(SetupHeader());
		nBMessageMock.Setup(m => m.DataBlocks).Returns(SetupDataBlocks());
		nBMessageMock.Setup(m => m.AnnualProgressiveNumber).Returns("999999");

		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>();
		sadMessageSendingObject.Setup(m => m.FallbackProcedure).Returns(false);
		sadMessageSendingObject.Setup(m => m.DeclarantTaxNumber).Returns("");

		nBMessage = new NBMessage(nBMessageMock.Object, sadMessageSendingObject.Object);

		var fallbackProcedureSadMessageSendingObject = new Mock<ISadMessageSendingObject>();
		fallbackProcedureSadMessageSendingObject.Setup(m => m.FallbackProcedure).Returns(true);
		fallbackProcedureSadMessageSendingObject.Setup(m => m.DeclarantTaxNumber).Returns("DEC TAX NO");
		nBMessageWithFallbackProcedure = new NBMessage(nBMessageMock.Object, fallbackProcedureSadMessageSendingObject.Object);

		flatFileMessageSerializer = new TabbedFlatFileMessageSerializer();
	}

	IEnumerable<IPreviousOperationInfo> SetupDataBlocks()
	{
		return new IPreviousOperationInfo[]
		{
			SetupOperation1(),
			SetupOperation2(),
			SetupOperation3(),
			SetupOperation4(),
			SetupOperation5(),
			SetupOperation6(),
		};
	}

	INBHeader SetupHeader()
	{
		var nBMessageHeaderMock = new Mock<INBHeader>();
		nBMessageHeaderMock.Setup(m => m.MessageCodeEntry).Returns("IM");
		nBMessageHeaderMock.Setup(m => m.ReferenceNumber).Returns("REF");
		nBMessageHeaderMock.Setup(m => m.DeclarationCIN).Returns("R");
		nBMessageHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate(2019, 07, 12));
		nBMessageHeaderMock.Setup(m => m.ItemNumber).Returns(3);

		return nBMessageHeaderMock.Object;
	}

	IPreviousOperationInfo SetupOperation1()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA1");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A1-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("A");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 01));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A1");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A1");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(1);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNA");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB1");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B1-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("A");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 01));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B1");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B1");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns((ZInt?)null);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(1);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(3902.123456);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("A123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(3343.123456);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(99.23);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation2()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA2");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A2-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("B");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 02));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A2");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A2");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(2);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNB");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB2");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B2-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("B");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 02));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B2");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B2");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(2);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(2);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(2343.12);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("B123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(33243.96);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(44.3);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation3()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA3");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A3-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("C");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 03));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A3");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A3");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(3);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNC");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB3");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B3-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("C");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 03));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B3");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B3");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(3);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(3);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(3333.33);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("C123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(33333.33);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(33.3);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation4()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA4");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A4-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("D");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 04));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A4");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A4");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(4);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRND");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB4");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B4-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("D");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 04));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B4");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B4");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(4);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(4);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(4444.44);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("D123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(44444.44);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(44.4);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation5()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA5");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A5-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("E");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 05));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A5");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A5");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(5);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNE");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB5");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B5-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("E");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 05));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B5");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B5");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(5);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(5);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(5555.55);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("E123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(55555.55);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(55.5);

		return nBPreviousOperation.Object;
	}
	IPreviousOperationInfo SetupOperation6()
	{
		var nBPreviousOperation = new Mock<IPreviousOperationInfo>();

		var nBPreviousAllibrament = new Mock<IPreviousAdministrativeReference>();
		nBPreviousAllibrament.Setup(m => m.Register).Returns("REA6");
		nBPreviousAllibrament.Setup(m => m.ReferenceNumber).Returns("A6-REF");
		nBPreviousAllibrament.Setup(m => m.ReferenceCIN).Returns("F");
		nBPreviousAllibrament.Setup(m => m.Date).Returns(new ZDate(2019, 07, 06));
		nBPreviousAllibrament.Setup(m => m.Series).Returns("A6");
		nBPreviousAllibrament.Setup(m => m.CustomsOffice).Returns("OFF-A6");
		nBPreviousAllibrament.Setup(m => m.ItemNumber).Returns(6);
		nBPreviousOperation.Setup(m => m.PreviousAllibrament).Returns(nBPreviousAllibrament.Object);

		nBPreviousOperation.Setup(m => m.MRN).Returns("MRNF");

		var nBPreviousProcedureReference = new Mock<IPreviousAdministrativeReference>();
		nBPreviousProcedureReference.Setup(m => m.Register).Returns("REB6");
		nBPreviousProcedureReference.Setup(m => m.ReferenceNumber).Returns("B6-REF");
		nBPreviousProcedureReference.Setup(m => m.ReferenceCIN).Returns("F");
		nBPreviousProcedureReference.Setup(m => m.Date).Returns(new ZDate(2019, 07, 06));
		nBPreviousProcedureReference.Setup(m => m.Series).Returns("B6");
		nBPreviousProcedureReference.Setup(m => m.CustomsOffice).Returns("OFF-B6");
		nBPreviousProcedureReference.Setup(m => m.ItemNumber).Returns(6);
		nBPreviousOperation.Setup(m => m.PreviousProcedure).Returns(nBPreviousProcedureReference.Object);

		nBPreviousOperation.Setup(m => m.NumberOfPackages).Returns(6);
		nBPreviousOperation.Setup(m => m.GrossMass).Returns(6666.66);
		nBPreviousOperation.Setup(m => m.CombinedNomenclature).Returns("F123456789");
		nBPreviousOperation.Setup(m => m.NetMass).Returns(66666.66);
		nBPreviousOperation.Setup(m => m.SupplementaryUnit).Returns(66.6);

		return nBPreviousOperation.Object;
	}

	TabbedFlatFileMessageSerializer flatFileMessageSerializer;
	NBMessage nBMessage;
	NBMessage nBMessageWithFallbackProcedure;

	protected override Type MessageType => typeof(NBMessage);
}
