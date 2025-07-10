using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalController))]
	class CreditControlledDocumentsApprovalControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(CreditControlledDocumentsApproval);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CreditControlledDocumentsApproval;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path";
			menutItem.SU_MenuName = "name";

			var bizo = Factory.New<CreditControlledDocumentsApproval>();
			using (bizo.SuspendSettingHasChanges())
			{
				var parentBusinessObject = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				parentBusinessObject.FillWithValidTestData();
				bizo.Initialize(parentBusinessObject, menutItem.PK);
				bizo.XP_ReasonDescription = "Desc";
			}
			Factory.Save();
			return bizo;
		}
	}
}
