using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementController))]
	class NctsMovementControllerTests : ZControllerBasherTest
	{
		public void TestModuleIdNeededForFavourites()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			AssertEquals(ModuleIDs.Customs.EU.NctsMovementModule, controller.ModuleID);
		}

		[RequiresSTA]
		public void TestNewFormHasCorrectHeaderType()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
				using (var form = (NctsMovementForm)controller.ShowNewForm())
				{
					var nctsHeader = (NctsHeader)form.BusinessEntity;
					AssertEquals(NctsMovementType.Codes.Departure, nctsHeader.BH_HeaderType);
				}
			}
		}

		[RequiresSTA]
		public void TestGetForm_Phase4()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			using (var form = ((ZControllerInternals)controller).GetForm(header))
			{
				AssertType<NctsMovementForm>(form);
			}
		}

		[RequiresSTA]
		public void TestGetForm_Phase5_Departure()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var form = ((ZControllerInternals)controller).GetForm(header))
			{
				AssertType<Phase5DepartureMovementForm>(form);
			}
		}

		[RequiresSTA]
		public void TestGetForm_Phase5_Arrival()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var form = ((ZControllerInternals)controller).GetForm(arrivalHeader))
			{
				AssertType<Phase5ArrivalMovementForm>(form);
			}
		}

		public override Type ControllerToBashType => typeof(NctsMovementController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Factory.Save();
			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
