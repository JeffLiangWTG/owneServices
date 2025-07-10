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
	[TestedType(typeof(NctsMovementInConsolController))]
	class NctsMovementInConsolControllerTests : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			var headerWithConsol = (NctsHeader)GetBusinessObjectThatIsInTheDatabase();
			var controller = new NctsMovementInConsolControllerForTest();
			using (var form = controller.GetForm(headerWithConsol))
			{
				AssertEquals("ConsolForm", form.GetType().Name);
			}
		}

		public void TestModuleIdNeededForFavourites()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementInConsolController);
			AssertEquals(ModuleIDs.Customs.EU.NctsMovementModule, controller.ModuleID);
		}

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

		public override Type ControllerToBashType => typeof(NctsMovementInConsolController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.NctsMovementInConsolController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = IsNcts5Country() ? CusInBondApplicationCodeList.Codes.NCTS5 : CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.CusAuthorizationUsages.RemoveAndDeleteAll();
			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
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

		class NctsMovementInConsolControllerForTest : NctsMovementInConsolController
		{
			public new IZForm GetForm(IBusiness bizO)
			{
				return base.GetForm(bizO);
			}
		}
	}
}
