using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementInShipmentController))]
	class NctsMovementInShipmentControllerTests : ZControllerBasherTest
	{
		[RequiresSTA]
		public void TestGetForm()
		{
			var headerWithShipment = (NctsHeader)GetBusinessObjectThatIsInTheDatabase();
			var controller = new NctsMovementInShipmentControllerForTest();
			using (var form = controller.GetForm(headerWithShipment))
			{
				AssertEquals("ShipmentForm", form.GetType().Name);
			}
		}

		public void TestModuleIdNeededForFavourites()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementInShipmentController);
			AssertEquals(ModuleIDs.Customs.EU.NctsMovementModule, controller.ModuleID);
		}

		[RequiresSTA]
		public override void TestViewForm()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(IsNcts5Country());

			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				base.TestViewForm();
			}
		}

		[RequiresSTA]
		public override void TestSaveFormWithCustomsPlugIns()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(IsNcts5Country());

			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				base.TestSaveFormWithCustomsPlugIns();
			}
		}

		public override Type ControllerToBashType => typeof(NctsMovementInShipmentController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementInShipmentController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = IsNcts5Country() ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.CusAuthorizationUsages.RemoveAndDeleteAll();
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();
			return header;
		}

		protected override IZForm GetEditFormToShow()
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(IsNcts5Country());

			using (ObjectFactory.Substitute(mockSettings.Object))
			{
				return base.GetEditFormToShow();
			}
		}

		bool IsNcts5Country()
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Core.Constants.CountryCodes.Germany:
				case Core.Constants.CountryCodes.Ireland:
					return true;
				default:
					return false;
			}
		}

		class NctsMovementInShipmentControllerForTest : NctsMovementInShipmentController
		{
			public new IZForm GetForm(IBusiness bizO)
			{
				return base.GetForm(bizO);
			}
		}
	}
}
