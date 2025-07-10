using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using Enterprise.Customs.BE.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC025CMessageInterpreter))]
	sealed class CC025CMessageInterpreterTest : MessageInterpreterTestCase<CC025CMessageInterpreter, ICC025CDataProvider>
	{
		public override void TestInterpret()
		{
			var mockCC025C = new Mock<ICC025CDataProvider>();

			var consignmentItem3 = new ConsignmentItemType02
			{
				DeclarationGoodsItemNumber = "3",
				ReleaseType = "2",
				Packaging = new Collection<PackagingType02>(new List<PackagingType02>
				{ new PackagingType02 { NumberOfPackages = "5", TypeOfPackages = "PKG", ShippingMarks = "marksandnumbers" } }),
			};
			var consignmentItem4 = new ConsignmentItemType02
			{
				DeclarationGoodsItemNumber = "4",
				ReleaseType = "2",
				Packaging = new Collection<PackagingType02>(new List<PackagingType02>
				{ new PackagingType02() { NumberOfPackages = "5", TypeOfPackages = "PKG", ShippingMarks = "marksandnumbers2" },
				new PackagingType02() { NumberOfPackages = "10", TypeOfPackages = "BOX", ShippingMarks = "marksandnumbers3" } }),
			};

			var houseConsignmentProvider = HouseConsignmentXmlProvider.New(new HouseConsignmentType02
			{
				SequenceNumber = "1",
				ReleaseType = "2",
				ConsignmentItem = new Collection<ConsignmentItemType02>(new List<ConsignmentItemType02>
				{ consignmentItem3, consignmentItem4 }),
			});

			mockCC025C.Setup(x => x.HouseConsignments).Returns(new ReadOnlyCollection<HouseConsignmentXmlProvider>(new List<HouseConsignmentXmlProvider>() { houseConsignmentProvider }));

			CombineAssertions(() =>
			{
				mockCC025C.Setup(m => m.ReleaseIndicator).Returns(Constants.ReleaseIndicator.FullRelease);
				var result = Interpreter.Interpret(mockCC025C.Object, null);
				AssertEquals("release type 1", "All Goods are released for transit upon arrival. The movement is closed.</br>1) House Bill: full release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);

				mockCC025C.Setup(m => m.ReleaseIndicator).Returns(Constants.ReleaseIndicator.PartialRelease);
				houseConsignmentProvider.ConsignmentItems.First().ReleaseType = "2";
				houseConsignmentProvider.ConsignmentItems.Last().ReleaseType = "1";

				houseConsignmentProvider.ReleaseType = "1";
				result = Interpreter.Interpret(mockCC025C.Object, null);
				AssertEquals("release type 2", "Goods are partially released.</br>1) House Bill: partial release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: partial release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);

				mockCC025C.Setup(m => m.ReleaseIndicator).Returns(Constants.ReleaseIndicator.PartialReleaseClosed);
				result = Interpreter.Interpret(mockCC025C.Object, null);
				AssertEquals("release type 3", "Goods are partially released. The movement is closed.</br>1) House Bill: partial release</br>3) Item: full release</br>-- 5 PKG with the marks and numbers 'marksandnumbers' are released</br>4) Item: partial release</br>-- 5 PKG with the marks and numbers 'marksandnumbers2' are released</br>-- 10 BOX with the marks and numbers 'marksandnumbers3' are released", result);
				mockCC025C.Setup(m => m.ReleaseIndicator).Returns(Constants.ReleaseIndicator.NoRelease);
				result = Interpreter.Interpret(mockCC025C.Object, null);
				AssertEquals("release type 4", "No release of Goods.", result);
			});
		}
	}
}
