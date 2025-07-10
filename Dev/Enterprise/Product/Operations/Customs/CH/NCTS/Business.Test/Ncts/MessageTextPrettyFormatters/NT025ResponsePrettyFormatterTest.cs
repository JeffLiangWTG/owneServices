using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NT025ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var responseDetailMock = new Mock<INT025ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT025ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT025ResponsePrettyFormatter(Factory, null));
	});

	public void TestGetFormattedText_FullRelease() => CombineAssertions(() =>
	{
		var arrivalIndicationMock = new Mock<INT025ResponseDetail>();
		arrivalIndicationMock.Setup(e => e.MRN).Returns("22CHVL2525YYN7IZJ7");
		arrivalIndicationMock.Setup(e => e.MRNVersion).Returns("1");
		arrivalIndicationMock.Setup(e => e.ReleaseDate).Returns(new DateTime(2021, 11, 19));
		arrivalIndicationMock.Setup(e => e.IsFullRelease).Returns(true);
		arrivalIndicationMock.Setup(e => e.CustomsOfficeReferenceNumber).Returns("CH001251");

		var formatter = new NT025ResponsePrettyFormatter(Factory, arrivalIndicationMock.Object);
		var html = formatter.GetFormattedText();

		AssertMultilineASCIIEquals("formatted description", GetExpectedFormattedText("22CHVL2525YYN7IZJ7", "1", "Full Release"), html);
	});

	public void TestGetFormattedText_PartialRelease() => CombineAssertions(() =>
	{
		var consignmentItem11 = new Mock<IConsignmentItem>();
		consignmentItem11.Setup(i => i.GoodsItemNumber).Returns(1);
		consignmentItem11.Setup(i => i.IsReleased).Returns(true);

		var houseConsignmentMock1 = new Mock<IHouseConsignment>();
		houseConsignmentMock1.Setup(c => c.SequenceNumber).Returns(1);
		houseConsignmentMock1.Setup(c => c.IsFullRelease).Returns(true);
		houseConsignmentMock1.Setup(c => c.ConsignmentItems).Returns(new[] { consignmentItem11.Object });

		var consignmentItem21 = new Mock<IConsignmentItem>();
		consignmentItem21.Setup(i => i.GoodsItemNumber).Returns(1);
		consignmentItem21.Setup(i => i.IsReleased).Returns(true);

		var consignmentItem22 = new Mock<IConsignmentItem>();
		consignmentItem22.Setup(i => i.GoodsItemNumber).Returns(2);
		consignmentItem22.Setup(i => i.IsBlocked).Returns(true);

		var houseConsignmentMock2 = new Mock<IHouseConsignment>();
		houseConsignmentMock2.Setup(c => c.SequenceNumber).Returns(1);
		houseConsignmentMock2.Setup(c => c.IsPartialRelease).Returns(true);
		houseConsignmentMock2.Setup(c => c.ConsignmentItems).Returns(new[] { consignmentItem21.Object, consignmentItem22.Object });

		var arrivalIndicationMock = new Mock<INT025ResponseDetail>();
		arrivalIndicationMock.Setup(e => e.MRN).Returns("22CHVL2525YYN7IZJ7");
		arrivalIndicationMock.Setup(e => e.MRNVersion).Returns("1");
		arrivalIndicationMock.Setup(e => e.ReleaseDate).Returns(new DateTime(2021, 11, 19));
		arrivalIndicationMock.Setup(e => e.IsPartialRelease).Returns(true);
		arrivalIndicationMock.Setup(e => e.CustomsOfficeReferenceNumber).Returns("CH001251");
		arrivalIndicationMock.Setup(e => e.HouseConsignments).Returns(new[] { houseConsignmentMock1.Object,  houseConsignmentMock2.Object });

		var formatter = new NT025ResponsePrettyFormatter(Factory, arrivalIndicationMock.Object);
		var html = formatter.GetFormattedText();

		AssertMultilineASCIIEquals("formatted description", GetExpectedFormattedText("22CHVL2525YYN7IZJ7", "1", "Partial Release"), html);
	});

	string GetExpectedFormattedText(string mrn, string mrnVersion, string indicator)
	{
		var formattedText = $"<h2>Release for further processing</h2>" +
			$"<table>" +
				$"<tr><td>MRN:</td><td>{mrn}.{mrnVersion}</td></tr>" +
				$"<tr><td>Release Date:</td><td>19.11.2021</td></tr>" +
				$"<tr><td>Release Indicator:</td><td>{indicator}</td></tr>" +
				$"<tr><td>Customs Office:</td><td>CH001251</td></tr>";

		if (indicator == "Partial Release")
		{
			formattedText += $"<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>House Consignment:</td><td>1 - Full Release</td></tr>" +
				"<tr><td style=\"text-indent: 50px;\">Item:</td><td>1 - Released</td></tr>" +
				"<tr><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>House Consignment:</td><td>1 - Partial Release</td></tr>" +
				"<tr><td style=\"text-indent: 50px;\">Item:</td><td>1 - Released</td></tr>" +
				"<tr><td style=\"text-indent: 50px;\">Item:</td><td>2 - Blocked</td></tr>";
		}

		formattedText += "</table>";

		return formattedText;
	}
}
