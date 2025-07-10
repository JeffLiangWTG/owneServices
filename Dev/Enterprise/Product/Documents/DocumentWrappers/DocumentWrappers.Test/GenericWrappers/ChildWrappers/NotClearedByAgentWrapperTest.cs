using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class NotClearedByAgentWrapperTest : TestCaseWithFactory
	{
		public void TestWrapperMappingsEmpty()
		{
			NotClearedByAgentWrapper wrapperEmpty = new NotClearedByAgentWrapper(null);
			AssertEquals("wrapperEmpty.Statement", "", wrapperEmpty.Statement);
			AssertEquals("wrapperEmpty.Number", "", wrapperEmpty.Number);
			AssertEquals("wrapperEmpty.IssueDate", ZDateTime.Empty, wrapperEmpty.IssueDate);
			AssertEquals("wrapperEmpty.ExpiryDate", ZDateTime.Empty, wrapperEmpty.ExpiryDate);
		}

		public void TestWrapperMappingFull()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			FreightDataRegistry.Instance.NotClearedByAgentStatement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~~IIIIII");

			NotClearedByAgentWrapper wrapperFull = new NotClearedByAgentWrapper(shipment);
			AssertEquals("wrapperFull.Statement", "", wrapperFull.Statement);
			AssertEquals("wrapperFull.Number", "", wrapperFull.Number);
			AssertEquals("wrapperFull.IssueDate", ZDateTime.Empty, wrapperFull.IssueDate);
			AssertEquals("wrapperFull.ExpiryDate", ZDateTime.Empty, wrapperFull.ExpiryDate);

			CusEntryNumber num = shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			wrapperFull = new NotClearedByAgentWrapper(shipment);
			AssertEquals("wrapperFull.Statement", "", wrapperFull.Statement);
			AssertEquals("wrapperFull.Number", "", wrapperFull.Number);
			AssertEquals("wrapperFull.IssueDate", ZDateTime.Empty, wrapperFull.IssueDate);
			AssertEquals("wrapperFull.ExpiryDate", ZDateTime.Empty, wrapperFull.ExpiryDate);

			shipment.CusEntryNumbers.RemoveAndDeleteAll();
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);

			wrapperFull = new NotClearedByAgentWrapper(shipment);
			AssertEquals("wrapperFull.Statement", "", wrapperFull.Statement);
			AssertEquals("wrapperFull.Number", "", wrapperFull.Number);
			AssertEquals("wrapperFull.IssueDate", ZDateTime.Empty, wrapperFull.IssueDate);
			AssertEquals("wrapperFull.ExpiryDate", ZDateTime.Empty, wrapperFull.ExpiryDate);

			num = shipment.CusEntryNumbers.AddNew();
			num.CE_EntryType = CusEntryNumberTypes.EU.T1;
			num.CE_EntryNum = "asdasd";
			num.CE_IssueDate = new ZDateTime(2008, 10, 9);
			num.CE_ExpiryDate = new ZDateTime(2008, 10, 15);

			wrapperFull = new NotClearedByAgentWrapper(shipment);
			AssertEquals("wrapperFull.Statement", "~~~IIIIII", wrapperFull.Statement);
			AssertEquals("wrapperFull.Number", "asdasd", wrapperFull.Number);
			AssertEquals("wrapperFull.IssueDate", new ZDateTime(2008, 10, 9), wrapperFull.IssueDate);
			AssertEquals("wrapperFull.ExpiryDate", new ZDateTime(2008, 10, 15), wrapperFull.ExpiryDate);
		}
	}
}
