using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents.DataTransfer.Testing
{
	public class ClientSharedOrderValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdateFromValueObjectCoreOverride()
		{
			Xsd.Order orderValue = GetXsdOrder();
			Xsd.OrderOrderLine orderlineValue = orderValue.OrderLines.AddNew();
			orderlineValue.OrderLineNo = 100;
			orderlineValue.OrderLineDetail.QtyOrdered.Value = 1;

			Order newOrder = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			Assert("new order", !newOrder.IsInDatabase);
			Assert("base ShouldUpdateExistingObject was called", !DataAdapter.BaseShouldUpdateExistingObjectWasCalled);
			Assert("base ImportFromValueObjectCore called", DataAdapter.BaseImportFromValueObjectCoreWasCalled);

			Factory.Save();
			DataAdapter.ResetToOriginal();
			Order existingOrder = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			Assert("existing order", existingOrder.IsInDatabase);
			Assert("base ShouldUpdateExistingObject was called", DataAdapter.BaseShouldUpdateExistingObjectWasCalled);
			Assert("base ImportFromValueObjectCore called", !DataAdapter.BaseImportFromValueObjectCoreWasCalled);

			Factory.Save();
			DataAdapter.ResetToOriginal();
			DataAdapter.AllowToProceedWithUpdate = true;
			orderlineValue.OrderLineDetail.QtyOrdered.Value = 111;
			existingOrder = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			Assert("base ShouldUpdateExistingObject was called", DataAdapter.BaseShouldUpdateExistingObjectWasCalled);
			Assert("base ImportFromValueObjectCore called", DataAdapter.BaseImportFromValueObjectCoreWasCalled);
			Assert("number of orderlines", existingOrder.OrderLines.Count == 1);
			OrderLine orderline = existingOrder.OrderLines[0];
			AssertEquals("update orderline", 111m, orderline.JO_Quantity);

			orderlineValue.OrderLineNo = 222;
			existingOrder = DataAdapter.CreateOrUpdateFromValueObject(orderValue, Context);
			Assert("number of orderlines", existingOrder.OrderLines.Count == 2);
			AssertEquals("old orderline is not in the import so it should be cancelled", Core.Constants.OrderStatus.Cancelled, orderline.JO_LineStatus);
		}

		#region Implementation

		Xsd.Order GetXsdOrder()
		{
			Xsd.Order result = new Xsd.Order();
			result.OrderDetail = new Xsd.OrderOrderDetail();
			result.OrderDetail.Buyer = new Xsd.Organisation();
			result.OrderDetail.Buyer.OwnerCode = "XYZ";
			result.OrderIdentifier.OrderNumber = "ORDERNUMBER";
			SetOrgMatch(result.OrderDetail.Buyer.OwnerCode);
			return result;
		}

		void SetOrgMatch(string code)
		{
			OrgPatternMatchOverride orgMatch = Factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_ForeignCode = code;
			orgMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			OrgHeader localOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.OrgProxy.PK));
			orgMatch.OO_LocalGuid = localOrg.PK;
			orgMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
		}

		readonly NotificationBuffer Notifications = new NotificationBuffer();

		ClientSharedOrderValueObjectDataAdapterTestClass DataAdapter
		{
			get
			{
				return dataAdapter ?? (dataAdapter = new ClientSharedOrderValueObjectDataAdapterTestClass());
			}
		}
		ClientSharedOrderValueObjectDataAdapterTestClass dataAdapter;

		ValueObjectImportContext Context
		{
			get
			{
				if (fContext == null)
				{
					fContext = new ValueObjectImportContext(Factory, Notifications);
				}
				return fContext;
			}
		}
		ValueObjectImportContext fContext;

		internal class ClientSharedOrderValueObjectDataAdapterTestClass : ClientSharedOrderValueObjectDataAdapter
		{
			public ClientSharedOrderValueObjectDataAdapterTestClass()
				: base()
			{
				BaseShouldUpdateExistingObjectWasCalled = false;
				BaseImportFromValueObjectCoreWasCalled = false;
				AllowToProceedWithUpdate = false;
			}

			public void ResetToOriginal()
			{
				BaseShouldUpdateExistingObjectWasCalled = false;
				BaseImportFromValueObjectCoreWasCalled = false;
				AllowToProceedWithUpdate = false;
			}

			public ZBool BaseShouldUpdateExistingObjectWasCalled
			{
				get;
				set;
			}

			public ZBool BaseImportFromValueObjectCoreWasCalled
			{
				get;
				set;
			}

			public ZBool AllowToProceedWithUpdate
			{
				get;
				set;
			}

			protected override void ImportFromValueObjectCore(Order order, Xsd.Order xsdOrder, IValueObjectImportContext context)
			{
				BaseImportFromValueObjectCoreWasCalled = true;
				base.ImportFromValueObjectCore(order, xsdOrder, context);
			}

			protected override bool ShouldUpdateExistingObject(Order orderBizObj, INotifications notifications)
			{
				BaseShouldUpdateExistingObjectWasCalled = true;
				return AllowToProceedWithUpdate;
			}
		}

		#endregion
	}
}
